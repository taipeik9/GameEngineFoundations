using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Cube
{
    public class Game(NativeWindowSettings nativeSettings) : GameWindow(GameWindowSettings.Default, nativeSettings)
    {
        private readonly float[] vertices =
        [
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f
        ];

        private readonly uint[] indices =
        [
            // Front face
            0, 1, 2,
            2, 3, 0,

            // Back face
            4, 5, 6,
            6, 7, 4,

            // Left face
            0, 3, 7,
            7, 4, 0,

            // Right face
            1, 2, 6,
            6, 5, 1,

            // Top face
            3, 2, 6,
            6, 7, 3,

            // Bottom face
            0, 1, 5,
            5, 4, 0
        ];

        private int vaoHandle;
        private int vboHandle;
        private int eboHandle;
        private int shaderProgramHandle;

        private float yRotation = 0.0f;
        private float xRotation = 0.0f;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            vaoHandle = GL.GenVertexArray();
            vboHandle = GL.GenBuffer();
            eboHandle = GL.GenBuffer();

            GL.BindVertexArray(vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            var vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                }
            ";

            var fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(0.4, 0.8, 1.0, 1.0);
                }
            ";

            var vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShader);
            GL.AttachShader(shaderProgramHandle, fragmentShader);
            GL.LinkProgram(shaderProgramHandle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                var info = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compilation error ({type}): {info}");
            }
            return shader;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 YRotationModel = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yRotation));
            Matrix4 XRotationModel = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(xRotation));
            Matrix4 model = YRotationModel * XRotationModel;
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

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left))
                yRotation -= 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right))
                yRotation += 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up))
                xRotation -= 100f * (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down))
                xRotation += 100f * (float)args.Time;
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