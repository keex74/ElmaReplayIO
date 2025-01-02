/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines a direction.
    /// </summary>
    public enum AppleGravityDirection
    {
        /// <summary>
        /// No direction change.
        /// </summary>
        None = 0,

        /// <summary>
        /// Up.
        /// </summary>
        Up = 1,

        /// <summary>
        /// Down.
        /// </summary>
        Down = 2,

        /// <summary>
        /// Left.
        /// </summary>
        Left = 3,
        /// <summary>
        /// Right.
        /// </summary>
        Right = 4,
    }
}
