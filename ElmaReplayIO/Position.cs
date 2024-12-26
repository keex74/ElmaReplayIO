/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines a position.
    /// </summary>
    /// <typeparam name="T">The type of the coordinate values.</typeparam>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public readonly struct Position<T>(T x, T y) where T : struct
    {
        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly T X = x;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly T Y = y;
    }
}
