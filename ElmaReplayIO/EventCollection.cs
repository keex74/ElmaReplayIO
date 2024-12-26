/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines a read-only collection of <see cref="Event"/> objects.
    /// </summary>
    public class EventCollection
        : ReadOnlyCollection<Event>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventCollection"/> class.
        /// </summary>
        /// <param name="list">The list of items to wrap.</param>
        public EventCollection(IList<Event> list)
            : base(list)
        {
        }

        /// <summary>
        /// Parse the event list from the input data.
        /// </summary>
        /// <param name="br">A binary reader around the input data.</param>
        /// <param name="header">The replay header.</param>
        /// <returns>The event collection.</returns>
        /// <exception cref="RecParsingException">If parsing the event collection fails.</exception>
        /// <exception cref="IOException">If reading from the input data fails.</exception>
        internal static EventCollection ParseFrom(BinaryReader br, ReplayHeader header)
        {
            var res = new List<Event>();
            try
            {
                var nEvents = br.ReadInt32();
                for (int i = 0; i < nEvents; i++)
                {
                    var time = br.ReadDouble();
                    var objectId = br.ReadInt16();
                    var type = br.ReadByte();
                    var v2 = br.ReadByte();
                    var groundTouchStrength = br.ReadSingle();

                    var t = type switch
                    {
                        0 => EventType.ObjectTouch,
                        1 => EventType.GroundTouch,
                        4 => EventType.AppleTake,
                        5 => EventType.Turn,
                        6 => EventType.VoltRight,
                        7 => EventType.VoltLeft,
                        _ => throw new RecParsingException($"Invalid event type: {type}")
                    };

                    var ev = new Event(time, t, objectId, v2, groundTouchStrength);
                    res.Add(ev);
                }
            }
            catch (EndOfStreamException)
            {
                throw new RecParsingException("Reached end of stream unexpectedly while parsing events.");
            }

            return new EventCollection(res);
        }


        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(this.Count);
            foreach (var e in this)
            {
                var time = e.Time.TotalMilliseconds / 2_289.377_289_38;
                writer.Write(time);
                writer.Write(e.ObjectID);
                writer.Write((byte)e.Type);
                writer.Write(e.V2);
                writer.Write(e.GroundTouchStrength);
            }
        }
    }
}
