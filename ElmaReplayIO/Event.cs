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
    /// <param name="time">The time on which the event occurs in a weird value.</param>
    /// <param name="type">The type of the event.</param>
    /// <param name="objectID">The ID of the object associated with the event in the level.</param>
    /// <param name="groundTouchStrength">Ground touch strength, whatever that means.</param>
    public readonly struct Event(double time, EventType type, int objectID, float groundTouchStrength)
    {
        /// <summary>
        /// The time of the event in the replay.
        /// </summary>
        public readonly TimeSpan Time = TimeSpan.FromMilliseconds(time * 2_289.377_289_38);

        /// <summary>
        /// The type of the event.
        /// </summary>
        public readonly EventType Type = type;

        /// <summary>
        /// The ID of the object in the level associated with this event.
        /// </summary>
        public readonly int ObjectID = objectID;

        /// <summary>
        /// The strength of the ground touch (?).
        /// </summary>
        public readonly float GroundTouchStrength = groundTouchStrength;
    }
}
