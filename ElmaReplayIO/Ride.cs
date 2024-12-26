/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines a single ride in an Elma replay.
    /// </summary>
    /// <param name="header">The ride header.</param>
    /// <param name="frames">The frame collection.</param>
    /// <param name="events">The event collection.</param>
    public class Ride(ReplayHeader header, FrameCollection frames, EventCollection events)
    {
        public ReplayHeader Header { get; } = header;
        public FrameCollection Frames { get; } = frames;
        public EventCollection Events { get; } = events;

        /// <summary>
        /// Parse a ride from the input data.
        /// </summary>
        /// <param name="br">A binary reader around the input data.</param>
        /// <returns>The ride.</returns>
        /// <exception cref="RecParsingException">If parsing the ride fails.</exception>
        /// <exception cref="IOException">If reading from the input data fails.</exception>
        internal static Ride ParseFrom(BinaryReader br)
        {
            var header = ReplayHeader.ParseFrom(br);
            var frames = FrameCollection.ParseFrom(br, header);
            var events = EventCollection.ParseFrom(br, header);
            try
            {
                var magicNumber = br.ReadInt32();
                if (magicNumber != 0x00492f75)
                {
                    throw new RecParsingException("Ride does not end with expected value");
                }

                return new Ride(header, frames, events);
            }
            catch (EndOfStreamException)
            {
                throw new RecParsingException("Reached end of stream unexpectedly while parsing ride magic number.");
            }

        }
    }
}
