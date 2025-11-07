using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Sprite_Animation
{
    public class Character
    {
        private readonly int _shader;  // Program containing uOffset/uSize
        private float _timer;          // Accumulated time for frame stepping
        private int _frame;            // Current frame column (0..FrameCount-1)
        private Direction _currentDir; // Last non-none direction

        // Timing
        private const float FrameTime = 0.15f; // seconds per frame
        private const int FrameCount = 4;      // frames per row

        // Sprite sheet layout (pixel units) — edit to match your atlas
        private const float FrameW = 64f;
        private const float FrameH = 128f;
        private const float Gap = 60f;      // horizontal spacing between frames
        private const float TotalW = FrameW + Gap;
        private const float SheetW = 4 * TotalW - Gap; // 4 columns
        private const float SheetH = 256f;     // 2 rows of 128 => 256

        public Character(int shader)
        {
            _shader = shader;
            _currentDir = Direction.None;
            SetIdle();                          // Pick a visible starting frame
        }

        public void Update(float delta, Direction dir)
        {
            // Requirement: when input stops, keep showing the last used frame.
            if (dir == Direction.None)
            {
                _currentDir = Direction.None;
                return;
            }

            // If started moving or changed side: restart cycle
            if (_currentDir != dir)
            {
                _timer = 0f;
                _frame = 0;
                _currentDir = dir;
            }

            _timer += delta;
            if (_timer >= FrameTime)
            {
                _timer -= FrameTime;
                _frame = (_frame + 1) % FrameCount;
            }

            int row = dir == Direction.Right ? 0 : 1; // Row per direction
            SetFrame(_frame, row);                    // Update UV uniforms
        }

        public void Render()
        {
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4); // Draw quad
        }

        private void SetIdle()
        {
            SetFrame(0, 0); // Any default you prefer
        }

        // Converts (col,row) in pixels to normalized UVs and uploads to shader
        private void SetFrame(int col, int row)
        {
            float x = (col * TotalW) / SheetW; // normalized start U
            float y = (row * FrameH) / SheetH; // normalized start V
            float w = FrameW / SheetW;         // normalized width
            float h = FrameH / SheetH;         // normalized height

            GL.UseProgram(_shader);
            int off = GL.GetUniformLocation(_shader, "uOffset");
            int sz = GL.GetUniformLocation(_shader, "uSize");
            GL.Uniform2(off, x, y);
            GL.Uniform2(sz, w, h);
        }
    }
}