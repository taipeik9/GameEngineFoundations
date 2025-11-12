using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTK_Sprite_Animation
{
    public class Background
    {
        private readonly int _shader;
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _texture;

        public Background(string texturePath)
        {
            _shader = CreateShaderProgram();
            _texture = Utils.LoadTexture(texturePath, true);

            // covering the entire bg of the window
            float[] vertices =
            {
                0f, 0f, 0f, 0f,
                Constants.WindowWidth, 0f, 1f, 0f,
                Constants.WindowWidth, Constants.WindowHeight, 1f, 1f,
                0f, Constants.WindowHeight, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.UseProgram(_shader);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            int texLoc = GL.GetUniformLocation(_shader, "uTexture");
            GL.Uniform1(texLoc, 0);

            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, Constants.WindowWidth, 0, Constants.WindowHeight, -1, 1);
            int projLoc = GL.GetUniformLocation(_shader, "projection");
            GL.UniformMatrix4(projLoc, false, ref proj);

            Matrix4 model = Matrix4.Identity;
            int modelLoc = GL.GetUniformLocation(_shader, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        private static int CreateShaderProgram()
        {
            string vs = @"
                #version 330 core
                layout(location = 0) in vec2 aPosition;
                layout(location = 1) in vec2 aTexCoord;
                out vec2 vTexCoord;
                uniform mat4 projection;
                uniform mat4 model;
                void main()
                {
                    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                    vTexCoord = aTexCoord;
                }
            ";

            string fs = @"
                #version 330 core
                in vec2 vTexCoord;
                out vec4 color;
                uniform sampler2D uTexture;
                void main()
                {
                    color = texture(uTexture, vTexCoord);
                }
            ";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            Utils.CheckShaderCompile(v, "VERTEX");

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            Utils.CheckShaderCompile(f, "FRAGMENT");

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            Utils.CheckProgramLink(p);

            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }
    }
}
