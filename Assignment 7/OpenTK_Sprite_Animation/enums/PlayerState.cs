using System;

namespace OpenTK_Sprite_Animation
{
    // using bitwise flags here to allow for multiple states to be active at once
    // i.e. walking and jumping OR sprinting and jumping
    [Flags]
    public enum PlayerState
    {
        Idle = 1,      // 000001
        Walking = 2,   // 000010
        Sprinting = 4, // 000100
        PreJump = 8,   // 001000
        Grounded = 16, // 010000
        Crouching = 64 // 100000
    }
}