using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Character _character;
        private int _shaderProgram;
        private int _vao, _vbo;
        private int _texture;
        private Background _background;

        public SpriteAnimationGame()
            : base(
                new GameWindowSettings(),
                new NativeWindowSettings { ClientSize = (Constants.WindowWidth, Constants.WindowHeight), Title = "CrouchJumpSprint Game" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 0f);            // Transparent background (A=0)
            GL.Enable(EnableCap.Blend);               // Enable alpha blending
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderProgram = CreateShaderProgram();   // Compile + link
            _texture = Utils.LoadTexture("assets/Sprite_Character.png"); // Upload sprite sheet

            // Quad vertices: [pos.x, pos.y, uv.x, uv.y], centered model space
            float w = 32f, h = 64f;                   // Half-size: results in 64x128 sprite
            float[] vertices =
            {
                -w, -h, 0f, 0f,
                 w, -h, 1f, 0f,
                 w,  h, 1f, 1f,
                -w,  h, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Attribute 0: vec2 position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // Attribute 1: vec2 texcoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.UseProgram(_shaderProgram);

            // Bind sampler to texture unit 0 (WHY: avoid undefined default binding)
            int texLoc = GL.GetUniformLocation(_shaderProgram, "uTexture");
            GL.Uniform1(texLoc, 0);

            // Orthographic projection (pixel coordinates 0..800, 0..600)
            // IMPORTANT: positional args to avoid API-name mismatch across OpenTK versions.
            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(0, Constants.WindowWidth, 0, Constants.WindowHeight, -1, 1);
            GL.UniformMatrix4(projLoc, false, ref ortho);

            _character = new Character(_shaderProgram); // Initializes idle frame uniforms

            _background = new Background("assets/Background.png");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Read keyboard state -> map to Direction
            var keyboard = KeyboardState;

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            Direction dir = Direction.None;
            if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) dir = Direction.Right;
            else if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A)) dir = Direction.Left;
            bool crouch = keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S);
            bool jump = keyboard.IsKeyDown(Keys.Space);
            bool sprint = keyboard.IsKeyDown(Keys.LeftShift);

            // Animation update; when dir == None we *keep last frame visible*
            _character.Update((float)e.Time, dir, jump, sprint, crouch);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _background.Render();

            GL.UseProgram(_shaderProgram);

            // Model transform: update character position
            int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
            Matrix4 model = Matrix4.CreateTranslation(_character.getPosition());
            GL.UniformMatrix4(modelLoc, false, ref model);

            // Bind texture and VAO, then draw
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.BindVertexArray(_vao);

            _character.Render();

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            // Free GPU resources
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            base.OnUnload();
        }

        // --- Shader creation utilities ---------------------------------------------------------

        private int CreateShaderProgram()
        {
            // Vertex Shader: transforms positions, flips V in UVs (image origin vs GL origin)
            string vs = @"
                #version 330 core
                layout(location = 0) in vec2 aPosition;
                layout(location = 1) in vec2 aTexCoord;
                out vec2 vTexCoord;
                uniform mat4 projection;
                uniform mat4 model;
                void main() {
                    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                    vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y); // flip V so PNGs read intuitively
                }
            ";

            // Fragment Shader: samples sub-rect of the sheet using uOffset/uSize
            string fs = @"
                #version 330 core
                in vec2 vTexCoord;
                out vec4 color;
                uniform sampler2D uTexture; // bound to texture unit 0
                uniform vec2 uOffset;       // normalized UV start (0..1)
                uniform vec2 uSize;         // normalized UV size  (0..1)
                void main() {
                    vec2 uv = uOffset + vTexCoord * uSize;
                    color = texture(uTexture, uv);
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

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }
    }
}