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
    /// Defines replay data consisting of one or more rides.
    /// </summary>
    public class Replay
        : ReadOnlyCollection<Ride>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Replay"/> class.
        /// </summary>
        /// <param name="list">The list of items to wrap.</param>
        public Replay(IList<Ride> list)
                 : base(list)
        {
            if (list.Count == 0)
            {
                throw new ArgumentException($"{nameof(list)} must not be empty.");
            }
        }

        /// <summary>
        /// Gets the first ride in the replay.
        /// </summary>
        public Ride MainRide => this[0];

        /// <summary>
        /// Parse replay data from the given input data stream.
        /// </summary>
        /// <param name="stream">The input data stream.</param>
        /// <returns>The replay data.</returns>
        /// <exception cref="ArgumentNullException">If  <paramref name="stream"/> is null.</exception>
        /// <exception cref="RecParsingException">If parsing the replay fails, usually due to invalid structure.</exception>
        /// <exception cref="System.IO.IOException">If an IO exception occurs when reading the from input stream.</exception>
        public static Replay ParseFrom(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            using var br = new BinaryReader(stream);
            var res = new List<Ride>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                var ride = Ride.ParseFrom(br);
                res.Add(ride);
            }

            if (res.Count == 0)
            {
                throw new RecParsingException("Input contains no rides.");
            }

            return new Replay(res);
        }

        /// <summary>
        /// Write the replay data into the given stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <exception cref="ArgumentNullException">If  <paramref name="stream"/> is null.</exception>
        /// <exception cref="RecWritingException">If writing the replay fails.</exception>
        /// <exception cref="System.IO.IOException">If an IO exception occurs when writing to the input stream.</exception>
        public void WriteReplay(Stream stream)
        {
            if (this.Count == 0)
            {
                throw new RecWritingException("The replay contains no rides.");
            }

            using var writer = new BinaryWriter(stream);
            var isMulti = this.Count > 1;
            foreach (var ride in this)
            {
                ride.WriteTo(writer, isMulti);
            }
        }
    }
}
