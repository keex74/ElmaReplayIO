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
    /// Defines a read-only collection of <see cref="Frame"/> objects.
    /// </summary>
    public class FrameCollection
        : ReadOnlyCollection<Frame>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameCollection"/> class.
        /// </summary>
        /// <param name="list">The list of items to wrap.</param>
        public FrameCollection(IList<Frame> list)
              : base(list)
        {
        }

        /// <summary>
        /// Parse the frame list from the input data.
        /// </summary>
        /// <param name="br">A binary reader around the input data.</param>
        /// <param name="header">The replay header.</param>
        /// <returns>The frame collection.</returns>
        /// <exception cref="RecParsingException">If parsing the event collection fails.</exception>
        /// <exception cref="IOException">If reading from the input data fails.</exception>
        internal static FrameCollection ParseFrom(BinaryReader br, ReplayHeader header)
        {
            var floatLists = new List<List<float>>();
            var shortLists = new List<List<short>>();
            var byteLists = new List<List<byte>>();
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    var list = new List<float>();
                    ReadList(br, header.FrameCount, list);
                    floatLists.Add(list);
                }

                for (int i = 0; i < 7; i++)
                {
                    var list = new List<short>();
                    ReadList(br, header.FrameCount, list);
                    shortLists.Add(list);
                }

                for (int i = 0; i < 5; i++)
                {
                    var list = new List<byte>();
                    ReadList(br, header.FrameCount, list);
                    byteLists.Add(list);
                }
            }
            catch (EndOfStreamException)
            {
                throw new RecParsingException("Reached end of stream unexpectedly while parsing frames.");
            }

            var res = new List<Frame>();
            for (int i = 0; i < header.FrameCount; i++)
            {
                // If list reading was successful then the lists will have the correct lengths.
                var a = 0;
                var bikeX = floatLists[a++][i];
                var bikeY = -floatLists[a++][i];
                a = 0;
                var leftWheelX = shortLists[a++][i];
                var leftWheelY = shortLists[a++][i];
                var rightWheelX = shortLists[a++][i];
                var rightWheelY = shortLists[a++][i];
                var headX = shortLists[a++][i];
                var headY = shortLists[a++][i];
                var rotation = shortLists[a++][i];
                a = 0;
                var leftWheelRotation = byteLists[a++][i];
                var rightWheelRotation = byteLists[a++][i];
                var throttleData = byteLists[a++][i];
                var backWheelSpeed = byteLists[a++][i];
                var collisionStrength = byteLists[a++][i];

                var dir = (throttleData & (1 << 1)) > 0 ? Direction.Right : Direction.Left;
                var throttleOn = (throttleData & 1) > 0;

                var frame = new Frame(
                    new Position<float>(bikeX, bikeY),
                    new Position<short>(leftWheelX, leftWheelY),
                    new Position<short>(rightWheelX, rightWheelY),
                    new Position<short>(headX, headY),
                    rotation,
                    leftWheelRotation,
                    rightWheelRotation,
                    dir,
                    throttleOn,
                    backWheelSpeed,
                    collisionStrength
                    );

                res.Add(frame);
            }

            return new FrameCollection(res);
        }

        public void WriteTo(BinaryWriter writer)
        {
            WriteList(writer, (f) => f.BikePosition.X);
            WriteList(writer, (f) => f.BikePosition.Y);

            WriteList(writer, (f) => f.LeftWheelPosition.X);
            WriteList(writer, (f) => f.LeftWheelPosition.Y);
            WriteList(writer, (f) => f.RightWheelPosition.X);
            WriteList(writer, (f) => f.RightWheelPosition.Y);
            WriteList(writer, (f) => f.HeadPosition.X);
            WriteList(writer, (f) => f.HeadPosition.Y);
            WriteList(writer, (f) => f.BikeRotation);

            WriteList(writer, (f) => f.LeftWheelRotation);
            WriteList(writer, (f) => f.RightWheelRotation);
            WriteList(writer, (f) => f.ThrottleDataByte);
            WriteList(writer, (f) => f.BackWheelSpeed);
            WriteList(writer, (f) => f.CollisionStrength);
        }

        private void WriteList(BinaryWriter writer, Func<Frame, float> selector)
        {
            foreach (var f in this)
            {
                var value = selector(f);
                writer.Write(value);
            }
        }

        private void WriteList(BinaryWriter writer, Func<Frame, short> selector)
        {
            foreach (var f in this)
            {
                var value = selector(f);
                writer.Write(value);
            }
        }

        private void WriteList(BinaryWriter writer, Func<Frame, byte> selector)
        {
            foreach (var f in this)
            {
                var value = selector(f);
                writer.Write(value);
            }
        }

        private static void ReadList(BinaryReader br, int frameCount, List<float> list)
        {
            for (var i = 0; i < frameCount; i++)
            {
                var value = br.ReadSingle();
                list.Add(value);
            }
        }

        private static void ReadList(BinaryReader br, int frameCount, List<short> list)
        {
            for (var i = 0; i < frameCount; i++)
            {
                var value = br.ReadInt16();
                list.Add(value);
            }
        }

        private static void ReadList(BinaryReader br, int frameCount, List<byte> list)
        {
            for (var i = 0; i < frameCount; i++)
            {
                var value = br.ReadByte();
                list.Add(value);
            }
        }
    }
}
