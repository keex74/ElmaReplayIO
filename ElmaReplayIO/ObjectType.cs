/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines the type of an <see cref="EventType.ObjectTouch"/>.
    /// </summary>
    public enum ObjectType
    {

        /// <summary>
        /// The type of the object could not be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// A flower / level exit.
        /// </summary>
        Flower,

        /// <summary>
        /// An apple / food with no gravity change.
        /// </summary>
        Apple,

        /// <summary>
        /// A killer.
        /// </summary>
        Killer,

        /// <summary>
        /// The player starting location.
        /// </summary>
        PlayerStart,
    }
}
