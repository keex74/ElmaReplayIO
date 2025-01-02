/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines an elma level, with only barebones properties for now.
    /// </summary>
    /// <param name="name">The level name.</param>
    /// <param name="link">The level link parameter.</param>
    /// <param name="objects">The list of objects in the level.</param>
    public class ElmaLevel(string name, uint link, IReadOnlyCollection<ObjectDescription> objects, IReadOnlyCollection<IReadOnlyCollection<Position<double>>> polygons)
    {
        /// <summary>
        /// Gets the name of the level.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the link value of the level.
        /// </summary>
        public uint Link { get; set; } = link;

        /// <summary>
        /// Gets the objects in the level.
        /// </summary>
        public IReadOnlyCollection<ObjectDescription> Objects { get; } = objects;

        /// <summary>
        /// Gets the level polygons.
        /// </summary>
        public IReadOnlyCollection<IReadOnlyCollection<Position<double>>> Polygons { get; } = polygons;

        /// <summary>
        /// Parse a level from the given data stream.
        /// </summary>
        /// <param name="stream">The input data stream.</param>
        /// <returns>The elma level.</returns>
        /// <exception cref="FormatException">In case the input data has an invalid format.</exception>
        /// <exception cref="IOException">When reading the input data stream fails</exception>
        public static ElmaLevel ParseFrom(Stream stream)
        {
            try
            {
                using var br = new BinaryReader(stream, System.Text.Encoding.ASCII);
                var identifier = new string(br.ReadChars(5));
                if (identifier != "POT14")
                {
                    throw new FormatException("Level format is not supported");
                }

                br.ReadInt16(); // Unused
                var link = br.ReadUInt32();

                for (int i = 0; i < 4; i++)
                {
                    br.ReadDouble(); // Level integrity status
                }

                var levelName = new string(br.ReadChars(51).TakeWhile(c => c != 0).ToArray()).Trim();
                var lgr = new string(br.ReadChars(16).TakeWhile(c => c != 0).ToArray()).Trim();
                var groundTexture = new string(br.ReadChars(10).TakeWhile(c => c != 0).ToArray()).Trim();
                var skyTexture = new string(br.ReadChars(10).TakeWhile(c => c != 0).ToArray()).Trim();

                // Polygons
                var polygonCount = (int)Math.Floor(br.ReadDouble()); // -0.4643643, treated by floor function
                var polygons = new List<IReadOnlyCollection<Position<double>>>();
                for (int i = 0; i < polygonCount; i++)
                {
                    var grass = br.ReadInt32();
                    var vertexCount = br.ReadInt32();
                    var vertices = new List<Position<double>>();
                    for (int a = 0; a < vertexCount; a++)
                    {
                        var x = br.ReadDouble();
                        var y = br.ReadDouble();
                        vertices.Add(new Position<double>(x, y));
                    }

                    polygons.Add(vertices);
                }

                // Objects
                var objectCount = (int)Math.Floor(br.ReadDouble()); // -0.4643643, treated by floor function
                var objects = new List<ObjectDescription>();
                for (int i = 0; i < objectCount; i++)
                {
                    var x = br.ReadDouble();
                    var y = br.ReadDouble();
                    var typeValue = br.ReadInt32();
                    var gravityValue = br.ReadInt32();
                    var animation = br.ReadInt32();

                    ObjectType type = typeValue switch
                    {
                        1 => ObjectType.Flower,
                        2 => ObjectType.Apple,
                        3 => ObjectType.Killer,
                        4 => ObjectType.PlayerStart,
                        _ => throw new FormatException("Invalid object type"),
                    };

                    AppleGravityDirection gravity = gravityValue switch
                    {
                        0 => AppleGravityDirection.None,
                        1 => AppleGravityDirection.Up,
                        2 => AppleGravityDirection.Down,
                        3 => AppleGravityDirection.Left,
                        4 => AppleGravityDirection.Right,
                        _ => throw new FormatException("Invalid gravity value"),
                    };

                    var pos = new Position<double>(x, y);
                    objects.Add(new ObjectDescription(pos, type, gravity));
                }

                var res = new ElmaLevel(levelName, link, objects, polygons);
                return res;
            }
            catch (EndOfStreamException)
            {
                throw new FormatException("The end of the input data has been reached unexpectedly");
            }

        }
    }
}