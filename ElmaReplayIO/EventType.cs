/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines the type of an <see cref="Event"/>.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// An object was touched.
        /// </summary>
        ObjectTouch,

        /// <summary>
        /// An apple was taken.
        /// </summary>
        AppleTake,

        /// <summary>
        /// The bike was turned by the user.
        /// </summary>
        Turn,

        /// <summary>
        /// A right volt was performed.
        /// </summary>
        VoltRight,

        /// <summary>
        /// A left volt was performed.
        /// </summary>
        VoltLeft,

        /// <summary>
        /// The ground was touched.
        /// </summary>
        GroundTouch,
    }
}
