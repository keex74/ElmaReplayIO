/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    using System.Text;

    /// <summary>
    /// Defines the header of a ride in a replay.
    /// </summary>
    /// <param name="isMultiReplay">Indicates whether this is a multi-ride replay.</param>
    /// <param name="isFlagTag">Indicates whether this is a flag-tag replay.</param>
    /// <param name="link">The link value (?).</param>
    /// <param name="levelName">The level name that this replay is associated with.</param>
    /// <param name="frameCount">The total number of frames in the ride.</param>
    public class ReplayHeader(bool isMultiReplay, bool isFlagTag, uint link, string levelName, int frameCount)
    {
        /// <summary>
        /// Gets a value indicating whether this is a multi-ride replay.
        /// </summary>
        public bool IsMultiReplay { get; } = isMultiReplay;

        /// <summary>
        /// Gets a value indicating whether this is a flag-tag replay.
        /// </summary>
        public bool IsFlagTag { get; } = isFlagTag;

        /// <summary>
        /// Gets the link value (?).
        /// </summary>
        public uint Link { get; } = link;

        /// <summary>
        /// Gets the level name that this replay is associated with.
        /// </summary>
        public string LevelName { get; } = levelName;

        /// <summary>
        /// Gets the total number of frames in this ride.
        /// </summary>
        public int FrameCount { get; } = frameCount;

        /// <summary>
        /// Parse the ride header from the input data.
        /// </summary>
        /// <param name="br">A binary reader around the input data.</param>
        /// <returns>The ride header.</returns>
        /// <exception cref="RecParsingException">If parsing the ride header fails.</exception>
        /// <exception cref="IOException">If reading from the input data fails.</exception>
        internal static ReplayHeader ParseFrom(BinaryReader br)
        {
            try
            {
                var frameCount = br.ReadInt32();
                var version = br.ReadUInt32();
                if (version != 0x83)
                {
                    throw new RecParsingException($"Invalid replay identifier: {version}");
                }

                var isMulti = br.ReadInt32() > 0;
                var isFlagTag = br.ReadInt32() > 0;
                var link = br.ReadUInt32();
                var levelName = new StringBuilder();
                while (true)
                {
                    var c = br.ReadByte();
                    if (c == 0)
                    {
                        break;
                    }
                    else
                    {
                        levelName.Append((char)c);
                    }

                    if (levelName.Length > 32)
                    {
                        throw new RecParsingException($"Unexpected length of level name, read so far: {levelName}");
                    }
                }

                _ = br.ReadInt16();
                _ = br.ReadByte();

                return new ReplayHeader(isMulti, isFlagTag, link, levelName.ToString(), frameCount);
            }
            catch (EndOfStreamException)
            {
                throw new RecParsingException("Reached end of stream unexpectedly while parsing replay header.");
            }
        }
    }
}
