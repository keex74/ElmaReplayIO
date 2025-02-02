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
    public class Ride
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ride"/>  class.
        /// </summary>
        /// <param name="header">The ride header.</param>
        /// <param name="frames">The frame collection.</param>
        /// <param name="events">The event collection.</param>
        public Ride(ReplayHeader header, FrameCollection frames, EventCollection events)
        {
            this.Header = header;
            this.Frames = frames;
            this.Events = events;
            this.ApplesTaken = events.Where(e => e.Type == EventType.AppleTake).Count();
        }

        private const int MAGICNUMBER = 0x00492f75;

        /// <summary>
        /// Gets the ride header.
        /// </summary>
        public ReplayHeader Header { get; }

        /// <summary>
        /// Gets the ride frames.
        /// </summary>
        public FrameCollection Frames { get; }

        /// <summary>
        /// Gets the ride events.
        /// </summary>
        public EventCollection Events { get; }

        /// <summary>
        /// Gets the number of apples taken in this ride.
        /// </summary>
        public int ApplesTaken { get; }

        /// <summary>
        /// Parse a ride from the input data.
        /// </summary>
        /// <param name="br">A binary reader around the input data.</param>
        /// <returns>The ride.</returns>
        /// <exception cref="RecParsingException">If parsing the ride fails.</exception>
        /// <exception cref="IOException">If reading from the input data fails.</exception>
        internal static Ride ParseFrom(BinaryReader br, string? sourcePath)
        {
            var header = ReplayHeader.ParseFrom(br);
            ElmaLevel? level = null;
            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    level = LevTools.FindLevel(header, sourcePath);
                }
                catch (System.Exception)
                {
                    // Error while finding a level, tough luck.
                }
            }

            var frames = FrameCollection.ParseFrom(br, header);
            var events = EventCollection.ParseFrom(br, header, level);
            try
            {
                var magicNumber = br.ReadInt32();
                if (magicNumber != MAGICNUMBER)
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

        internal void WriteTo(BinaryWriter writer, bool isMultiReplay)
        {
            var header = new ReplayHeader(isMultiReplay, this.Header);
            header.WriteTo(writer);
            this.Frames.WriteTo(writer);
            this.Events.WriteTo(writer);
            writer.Write(MAGICNUMBER);
        }
    }
}
