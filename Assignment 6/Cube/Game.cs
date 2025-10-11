using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace Cube
{
    public class Game(NativeWindowSettings nativeSettings) : GameWindow(GameWindowSettings.Default, nativeSettings)
    {
        private readonly float[] vertices = {
            -0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            0.5f, -0.5f,  0.5f,
            0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f
        };

        private readonly uint[] indices = {
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
        };

        private int vboHandle;
        private int vaoHandle;
        private int eboHandle;
        private int shaderProgramHandle;
        private Vector3 cameraPos = new(0.0f, 0.0f, 3.0f); // camera starts 3 from origin
        private Vector3 cameraFront = new(0.0f, 0.0f, -1.0f); // camera looks toward neg z
        private readonly Vector3 cameraUp = new(0.0f, 1.0f, 0.0f); // the "world"'s up is pos y
        private float yaw = -90f; // we start facing neg z
        private float pitch = 0.0f; // we start no up or down tilt
        private readonly float sensitivity = 0.1f;
        private float fov = 45.0f;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            CursorState = CursorState.Grabbed;

            vboHandle = GL.GenBuffer();
            vaoHandle = GL.GenVertexArray();
            eboHandle = GL.GenBuffer();

            GL.BindVertexArray(vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            int vertexShader = CompileShader(ShaderType.VertexShader, "phong.vert");
            int fragmentShader = CompileShader(ShaderType.FragmentShader, "phong.frag");

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShader);
            GL.AttachShader(shaderProgramHandle, fragmentShader);
            GL.LinkProgram(shaderProgramHandle);

            GL.DetachShader(shaderProgramHandle, fragmentShader);
            GL.DetachShader(shaderProgramHandle, vertexShader);

            GL.UseProgram(shaderProgramHandle);

            Vector3 lightPos = new(2.0f, 2.0f, 2.0f);
            Vector3 lightColour = new(1.0f, 1.0f, 1.0f);
            Vector3 objectColour = new(0.4f, 0.8f, 1.0f);

            int lightPosLoc = GL.GetUniformLocation(shaderProgramHandle, "lightPos");
            int lightColourLoc = GL.GetUniformLocation(shaderProgramHandle, "lightColor");
            int objectColourLoc = GL.GetUniformLocation(shaderProgramHandle, "objectColor");
            GL.Uniform3(lightPosLoc, lightPos);
            GL.Uniform3(lightColourLoc, lightColour);
            GL.Uniform3(objectColourLoc, objectColour);
        }

        private static int CompileShader(ShaderType type, string path)
        {
            int shader = GL.CreateShader(type);
            string source;
            using (StreamReader reader = new(path))
            {
                source = reader.ReadToEnd();
            }
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

            // model
            Matrix4 model = Matrix4.Identity;

            // view
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);

            // projection
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(fov),
                Size.X / (float)Size.Y,
                0.1f, 100f
            );

            GL.UseProgram(shaderProgramHandle);

            int modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            int viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            int projLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");
            int cameraPosLoc = GL.GetUniformLocation(shaderProgramHandle, "viewPos");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);
            GL.Uniform3(cameraPosLoc, ref cameraPos);

            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float deltaTime = (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                Close();

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                cameraPos += deltaTime * cameraFront;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                cameraPos -= deltaTime * cameraFront;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                cameraPos -= deltaTime * Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp));

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                cameraPos += deltaTime * Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp));
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            base.OnMouseMove(args);

            yaw += args.DeltaX * sensitivity;
            pitch -= args.DeltaY * sensitivity;

            if (pitch > 89.0f) pitch = 89.0f;
            if (pitch < -89.0f) pitch = -89.0f;

            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraFront = Vector3.Normalize(front);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(vboHandle);
            GL.DeleteBuffer(eboHandle);
            GL.DeleteVertexArray(vaoHandle);
            GL.DeleteProgram(shaderProgramHandle);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs args)
        {
            base.OnMouseWheel(args);

            fov -= args.OffsetY;
            if (fov < 1.0f) fov = 1.0f;
            if (fov > 90.0f) fov = 90.0f;
        }
    }
}