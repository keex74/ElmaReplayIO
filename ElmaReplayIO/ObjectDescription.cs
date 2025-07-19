/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines an event during a replay.
    /// </summary>
    /// <param name="position">The object position.</param>
    /// <param name="type">The object type.</param>
    /// <param name="appleGravity">The apple gravity.</param>
    /// <param name="animationNumber">The animation number.</param>
    public readonly struct ObjectDescription(Position<double> position, ObjectType type, AppleGravityDirection appleGravity, int animationNumber)
    {
        /// <summary>
        /// The object position.
        /// </summary>
        public readonly Position<double> Position = position;

        /// <summary>
        /// The object type.
        /// </summary>
        public readonly ObjectType Type = type;

        /// <summary>
        /// The apple direction.
        /// </summary>
        /// <remarks>Will be None for non-apple objects.</remarks>
        public readonly AppleGravityDirection AppleGravity = appleGravity;

        /// <summary>
        /// The animation number.
        /// </summary>
        public readonly int AnimationNumber = animationNumber;
    }
}
