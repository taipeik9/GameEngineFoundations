using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace TexturedCube
{
    public class Game(NativeWindowSettings nativeSettings) : GameWindow(GameWindowSettings.Default, nativeSettings)
    {
        private readonly float[] vertices = {
            // --- FRONT ---
            -0.5f, -0.5f,  0.5f,  0.25f, 0.3333f,
            0.5f, -0.5f,  0.5f,  0.50f, 0.3333f,
            0.5f,  0.5f,  0.5f,  0.50f, 0.6667f,
            -0.5f,  0.5f,  0.5f,  0.25f, 0.6667f,

            // --- BACK ---
            0.5f, -0.5f, -0.5f,  0.75f, 0.3333f,
            -0.5f, -0.5f, -0.5f,  1.00f, 0.3333f,
            -0.5f,  0.5f, -0.5f,  1.00f, 0.6667f,
            0.5f,  0.5f, -0.5f,  0.75f, 0.6667f,

            // --- LEFT ---
            -0.5f, -0.5f, -0.5f,  0.00f, 0.3333f,
            -0.5f, -0.5f,  0.5f,  0.25f, 0.3333f,
            -0.5f,  0.5f,  0.5f,  0.25f, 0.6667f,
            -0.5f,  0.5f, -0.5f,  0.00f, 0.6667f,

            // --- RIGHT ---
            0.5f, -0.5f,  0.5f,  0.50f, 0.3333f,
            0.5f, -0.5f, -0.5f,  0.75f, 0.3333f,
            0.5f,  0.5f, -0.5f,  0.75f, 0.6667f,
            0.5f,  0.5f,  0.5f,  0.50f, 0.6667f,

            // --- TOP ---
            -0.5f,  0.5f,  0.5f,  0.25f, 0.6667f,
            0.5f,  0.5f,  0.5f,  0.50f, 0.6667f,
            0.5f,  0.5f, -0.5f,  0.50f, 1.0000f,
            -0.5f,  0.5f, -0.5f,  0.25f, 1.0000f,

            // --- BOTTOM ---
            -0.5f, -0.5f, -0.5f,  0.25f, 0.0000f,
            0.5f, -0.5f, -0.5f,  0.50f, 0.0000f,
            0.5f, -0.5f,  0.5f,  0.50f, 0.3333f,
            -0.5f, -0.5f,  0.5f,  0.25f, 0.3333f,
        };



        private readonly uint[] indices = {
            0,1,2, 0,2,3, // front
            4,5,6, 4,6,7, // back
            8,9,10, 8,10,11, // left
            12,13,14, 12,14,15, // right
            16,17,18, 16,18,19, // top
            20,21,22, 20,22,23 // bottom
        };

        private int vboHandle;
        private int vaoHandle;
        private int eboHandle;
        private int shaderProgramHandle;
        private int textureHandle;
        private float yRotation = 45f;
        private float xRotation = 0f;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            vboHandle = GL.GenBuffer();
            vaoHandle = GL.GenVertexArray();
            eboHandle = GL.GenBuffer();

            GL.BindVertexArray(vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);


            const string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec2 aTexCoord;

                out vec2 TexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main() {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                    TexCoord = aTexCoord;
                }
            ";

            const string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                in vec2 TexCoord;
                uniform sampler2D myTexture;

                void main() {
                    FragColor = texture(myTexture, TexCoord);
                }
            ";

            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShader);
            GL.AttachShader(shaderProgramHandle, fragmentShader);
            GL.LinkProgram(shaderProgramHandle);

            GL.DetachShader(shaderProgramHandle, fragmentShader);
            GL.DetachShader(shaderProgramHandle, vertexShader);

            textureHandle = LoadTexture("texture.png");
            GL.UseProgram(shaderProgramHandle);
            GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "myTexture"), 0);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                throw new Exception($"Shader Compilation Error {type}: {GL.GetShaderInfoLog(shader)}");
            }
            return shader;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            Matrix4 yRotationModel = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yRotation));
            Matrix4 xRotationModel = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(xRotation));
            Matrix4 model = yRotationModel * xRotationModel;
            Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

            GL.UseProgram(shaderProgramHandle);

            int modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            int viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            int projLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left))
                yRotation -= 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right))
                yRotation += 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up))
                xRotation -= 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down))
                xRotation += 100f * (float)args.Time;
        }

        private int LoadTexture(string path)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (Image<Rgba32> image = Image.Load<Rgba32>(path))
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                var pixels = new byte[image.Width * image.Height * 4];
                image.CopyPixelDataTo(pixels);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return textureId;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(vboHandle);
            GL.DeleteBuffer(eboHandle);
            GL.DeleteVertexArray(vaoHandle);
            GL.DeleteProgram(shaderProgramHandle);
        }
    }
}