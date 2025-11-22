using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;

namespace _2DCollision
{
    class BreakoutGame : GameWindow
    {
        private int _vao;
        private int _vbo;
        private int _shader;
        public BreakoutGame(Vector2i Size, string WindowTitle)
        : base(
            new GameWindowSettings(),
            new NativeWindowSettings { ClientSize = Size, Title = WindowTitle })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader = GL.CreateShaderProgram();
        }
    }
}