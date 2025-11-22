using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Collision3D
{
    class Collision3DGame : GameWindow
    {
        private int _shader;
        private int modelLoc;
        private int viewLoc;
        private int projLoc;

        private readonly List<Entity> entities = new List<Entity>();

        public Collision3DGame(Vector2i windowSize, string title) :
            base(
                GameWindowSettings.Default,
                new NativeWindowSettings { ClientSize = windowSize, Title = title }
            )
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 1f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader = CreateShaderProgram();

            entities.Add(new Entity(MeshFactory.CreateCube()));
            foreach (Entity e in entities)
            {
                e.Mesh.UploadToGPU();
            }

            modelLoc = GL.GetUniformLocation(_shader, "model");
            viewLoc = GL.GetUniformLocation(_shader, "view");
            projLoc = GL.GetUniformLocation(_shader, "projection");
        }

        private static int CreateShaderProgram()
        {
            string vertexString = File.ReadAllText("shaders/base.vert");
            string fragmentString = File.ReadAllText("shaders/base.frag");

            int v = CompileShader(vertexString, ShaderType.VertexShader);
            int f = CompileShader(fragmentString, ShaderType.FragmentShader);

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string log = GL.GetProgramInfoLog(p);
                throw new Exception("PROGRAM LINK ERROR:\n" + log);
            }

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shader);

            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 3), Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100f);

            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            foreach (Entity e in entities)
            {
                Matrix4 model = e.ModelMatrix;
                GL.UniformMatrix4(modelLoc, false, ref model);
                e.Mesh.Draw();
            }

            SwapBuffers();
        }

        private static int CompileShader(string shaderSrc, ShaderType type)
        {
            int s = GL.CreateShader(type);
            GL.ShaderSource(s, shaderSrc);
            GL.CompileShader(s);
            GL.GetShader(s, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0)
                throw new Exception($"SHADER COMPILE ERROR:\n{GL.GetShaderInfoLog(s)}");

            return s;
        }
    }
}