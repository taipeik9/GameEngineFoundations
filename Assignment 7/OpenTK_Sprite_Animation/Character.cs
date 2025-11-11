using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTK_Sprite_Animation
{
    public class Character
    {
        private readonly int _shader;  // Program containing uOffset/uSize
        private float _spriteTimer;          // Accumulated time for frame stepping
        private int _frame;            // Current frame column (0..FrameCount-1)
        private Direction _currentDir;
        private Direction _lastDir;
        private Vector3 _position = new Vector3(400, 300, 0);

        // movement variables
        private float _velocityX = 0.0f;
        private const float _maxVelocityWalking = 200.0f; // pixels per second
        private const float _maxVelocitySprinting = 350.0f; // pixels per second

        // how fast he accelerates and decelerates on the ground
        private const float _accelWalking = 1000.0f;
        private const float _accelSprinting = 1750.0f;
        private bool _wasSprinting = false;

        private const float _airAccelFactor = 0.35f; // multiplier for acceleration in air
        private const float _airDrag = 40.0f; // constant passive deceleration in the air
        private bool _isGrounded = false;
        private float _velocityY = 0.0f;
        private const float _shortHopJumpSpeed = 350f;
        private const float _fullHopJumpSpeed = 425f;
        private const float _groundY = 160.0f;
        private const float _gravity = -980f;
        private float jumpTimer = 0.0f;
        private bool startJump = false;
        private const float shortHopLimit = 0.1f;

        // Timing
        private const float FrameTime = 0.15f; // seconds per frame
        private const int FrameCount = 4;      // frames per row

        // Sprite sheet layout (pixel units) — edit to match your atlas
        private const float FrameW = 64f;
        private const float FrameH = 128f;
        private const float Gap = 60f;      // horizontal spacing between frames
        private const float TotalW = FrameW + Gap;
        private const float SheetW = 5 * TotalW - Gap; // 4 columns
        private const float SheetH = 256f;     // 2 rows of 128 => 256

        public Character(int shader)
        {
            _shader = shader;
            _currentDir = Direction.None;
            SetIdle();
        }

        public Vector3 getPosition()
        {
            return _position;
        }

        private void RestartSpriteCycle(Direction dir)
        {
            _spriteTimer = 0.14f; // starts at the end, so animation occurs on button press
            _frame = 0;
            _currentDir = dir;
        }

        public void Update(float delta, Direction dir, bool jump, bool isSprinting)
        {
            int row;
            if (_currentDir != dir)
            {
                RestartSpriteCycle(dir);
            }

            // sprite logic
            if (dir == Direction.None)
            {
                _currentDir = Direction.None;
                row = _lastDir == Direction.Right ? 0 : 1;
            }
            else
            {
                _spriteTimer += delta;
                if (_spriteTimer >= FrameTime)
                {
                    _spriteTimer -= FrameTime;
                    _frame = (_frame + 1) % FrameCount;
                }

                row = dir == Direction.Right ? 0 : 1; // Row per direction

                _lastDir = _currentDir;
            }
            SetFrame(_frame, row); // Update UV uniforms

            // horizontal movement logic
            bool sprint = !_isGrounded ? _wasSprinting : isSprinting;

            float accel = sprint ? _accelSprinting : _accelWalking;
            float normalizedAccel = (_isGrounded ? accel : accel * _airAccelFactor) * delta;
            float maxVelocity = sprint ? _maxVelocitySprinting : _maxVelocityWalking;

            int dirSign = dir == Direction.Right ? 1 : dir == Direction.Left ? -1 : 0;

            if (_isGrounded)
            {
                // normal movement logic for grounded player
                if (dirSign != 0)
                {
                    _velocityX += normalizedAccel * dirSign;
                }
                else
                {
                    if (_velocityX < 0)
                    {
                        _velocityX += normalizedAccel;
                        if (_velocityX > 0) _velocityX = 0;
                    }
                    else if (_velocityX > 0)
                    {
                        _velocityX -= normalizedAccel;
                        if (_velocityX < 0) _velocityX = 0;
                    }
                }
            }
            else
            {
                // modified movement logic including air properties for non-grounded player
                if (dirSign != 0)
                {
                    if (Math.Sign(_velocityX) == dirSign || _velocityX == 0f)
                    {
                        _velocityX += normalizedAccel * dirSign;
                    }
                    else
                    {
                        float desired = _velocityX + dirSign * normalizedAccel;
                        float deltaV = MathHelper.Clamp(desired - _velocityX, -normalizedAccel, normalizedAccel);
                        _velocityX += deltaV;
                    }
                }
                else
                {
                    if (_velocityX > 0)
                    {
                        _velocityX -= _airDrag * delta;
                        if (_velocityX < 0) _velocityX = 0;
                    }
                    else if (_velocityX < 0)
                    {
                        _velocityX += _airDrag * delta;
                        if (_velocityX > 0) _velocityX = 0;
                    }
                }

            }

            // clamp to max speed
            _velocityX = MathHelper.Clamp(_velocityX, -maxVelocity, maxVelocity);

            _position.X += _velocityX * delta;

            _wasSprinting = sprint;

            // jumping logic
            if (jump && _isGrounded)
            {
                startJump = true;
            }
            if (startJump)
            {
                jumpTimer += delta;
            }

            if (jumpTimer >= shortHopLimit)
            {
                _velocityY = jump ? _fullHopJumpSpeed : _shortHopJumpSpeed;
                _isGrounded = false;
                jumpTimer = 0.0f;
                startJump = false;
            }

            _velocityY += _gravity * delta;
            _position.Y += _velocityY * delta;

            if (_position.Y - (FrameH / 2) <= _groundY)
            {
                // ground collision occurred
                _position.Y = _groundY + (FrameH / 2);
                _velocityY = 0.0f; // stop vertical movement

                _isGrounded = true;
            }
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