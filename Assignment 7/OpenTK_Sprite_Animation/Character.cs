using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTK_Sprite_Animation
{
    public class Character
    {
        private readonly int _shader;  // Program containing uOffset/uSize
        private float _spriteTimer;          // Accumulated time for frame stepping
        private int _frame;            // Current frame column (0..FrameCount-1)
        private Direction _lastDir = Direction.Right;
        private Vector3 _position = new Vector3(400, 300, 0);
        private PlayerState _state;

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
        private float _velocityY = 0.0f;
        private const float _shortHopJumpSpeed = 325f;
        private const float _fullHopJumpSpeed = 450f;
        private const float _groundY = 160.0f;
        private const float _gravity = -980f;
        private float jumpTimer = 0.0f;
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
            SetIdle();
        }

        public Vector3 getPosition()
        {
            return _position;
        }

        private void HandleSpriteChange(float delta, Direction dir)
        {
            int row;

            // sprite logic
            if (_state.HasFlag(PlayerState.Idle))
            {
                _spriteTimer = 0.14f;
                _frame = 0;
                row = _lastDir == Direction.Right ? 0 : 1;
            }
            else if ((_state & (PlayerState.Walking | PlayerState.Sprinting)) != 0)
            {
                _spriteTimer += delta;
                if (_spriteTimer >= FrameTime)
                {
                    _spriteTimer -= FrameTime;
                    _frame = (_frame + 1) % FrameCount;
                }

                row = dir == Direction.Right ? 0 : 1; // Row per direction

                _lastDir = dir;
            }
            else
            {
                _frame = 4;
                row = _lastDir == Direction.Right ? 0 : 1;
            }
            SetFrame(_frame, row); // Update UV uniforms
        }

        private void HandleHorizontalMovement(float delta, Direction dir)
        {
            bool controlledSprint = !_state.HasFlag(PlayerState.Grounded) ? _wasSprinting : _state.HasFlag(PlayerState.Sprinting);

            float accel = controlledSprint ? _accelSprinting : _accelWalking;
            float normalizedAccel = (_state.HasFlag(PlayerState.Grounded) ? accel : accel * _airAccelFactor) * delta;
            float maxVelocity = controlledSprint ? _maxVelocitySprinting : _maxVelocityWalking;

            int dirSign = dir == Direction.Right ? 1 : dir == Direction.Left ? -1 : 0;

            if (_state.HasFlag(PlayerState.Grounded))
            {
                // normal movement logic for grounded player
                // this is a bitwise or checking if either walking or sprinting are in state
                if ((_state & (PlayerState.Walking | PlayerState.Sprinting)) != 0)
                {
                    _velocityX += normalizedAccel * dirSign;
                }
                else
                {
                    // deceleration to zero
                    if (_velocityX > 0)
                    {
                        _velocityX -= normalizedAccel;
                        if (_velocityX < 0) _velocityX = 0;
                    }
                    else if (_velocityX < 0)
                    {
                        _velocityX += normalizedAccel;
                        if (_velocityX > 0) _velocityX = 0;
                    }
                }
            }
            else
            {
                // modified movement logic including air properties for non-grounded player
                if ((_state & (PlayerState.Walking | PlayerState.Sprinting)) != 0)
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

            _wasSprinting = controlledSprint;
        }

        private void HandleJumping(float delta, bool jump)
        {
            if (_state.HasFlag(PlayerState.PreJump))
            {
                jumpTimer += delta;
            }

            if (jumpTimer >= shortHopLimit)
            {
                _velocityY = jump ? _fullHopJumpSpeed : _shortHopJumpSpeed;
                _state &= ~PlayerState.Grounded;
                jumpTimer = 0.0f;
                _state &= ~PlayerState.PreJump;
            }

            _velocityY += _gravity * delta;
            _position.Y += _velocityY * delta;

            if (_position.Y - (FrameH / 2) <= _groundY)
            {
                // ground collision occurred
                _position.Y = _groundY + (FrameH / 2);
                _velocityY = 0.0f; // stop vertical movement

                _state |= PlayerState.Grounded;
            }
        }

        private void CheckCollision()
        {
            const float CharacterHalfWidth = FrameW / 2;
            if (_position.X - CharacterHalfWidth < 0)
            {
                _position.X = CharacterHalfWidth;
                _velocityX = 0;
            }
            else if (_position.X + CharacterHalfWidth > Constants.WindowWidth)
            {
                _position.X = Constants.WindowWidth - CharacterHalfWidth;
                _velocityX = 0;
            }
        }
        public void Update(float delta, Direction dir, bool jump, bool sprint, bool crouch)
        {
            // set state
            if (_state.HasFlag(PlayerState.Grounded) && jump)
            {
                _state |= PlayerState.PreJump;
            }
            if (dir == Direction.None)
            {
                _state |= PlayerState.Idle;
                _state &= ~PlayerState.Walking;
                _state &= ~PlayerState.Sprinting;
            }
            else if (dir == Direction.Right || dir == Direction.Left)
            {
                _state &= ~PlayerState.Idle;
                if (sprint)
                {
                    _state &= ~PlayerState.Walking;
                    _state |= PlayerState.Sprinting;
                }
                else
                {
                    _state &= ~PlayerState.Sprinting;
                    _state |= PlayerState.Walking;
                }
            }
            if (crouch && _state.HasFlag(PlayerState.Grounded))
            {
                _state &= ~PlayerState.Idle;
                _state &= ~PlayerState.Sprinting;
                _state &= ~PlayerState.Walking;
                _state |= PlayerState.Crouching;
            }
            else
            {
                _state &= ~PlayerState.Crouching;
            }

            HandleSpriteChange(delta, dir);

            // horizontal movement logic
            HandleHorizontalMovement(delta, dir);

            // jumping logic
            HandleJumping(delta, jump);

            // window edge collision
            CheckCollision();
#if DEBUG
            PrintDebug();
#endif
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

#if DEBUG
        private void PrintDebug()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("****** DEBUG Information ******");
            Console.WriteLine($"_spriteTimer: {_spriteTimer}");
            Console.WriteLine($"_frame: {_frame}");
            Console.WriteLine($"_lastDir: {_lastDir}");
            Console.WriteLine($"_position: {_position}");
            Console.WriteLine($"_state: {_state}");
            Console.WriteLine($"_velocityX: {_velocityX}");
            Console.WriteLine($"_wasSprinting: {_wasSprinting}");
            Console.WriteLine($"_velocityY: {_velocityY}");
            Console.WriteLine($"jumpTimer: {jumpTimer}");
        }
#endif
    }
}