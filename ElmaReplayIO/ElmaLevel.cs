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
    public class ElmaLevel(string name, uint link, IReadOnlyCollection<ObjectDescription> objects, IReadOnlyCollection<ElmaPolygon> polygons, IReadOnlyCollection<ElmaPolygon> grassPolygons, Position<double> bottomLeft, Position<double> topRight)
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
        public IReadOnlyCollection<ElmaPolygon> Polygons { get; } = polygons;

        /// <summary>
        /// Gets the level's grass polygons.
        /// </summary>
        public IReadOnlyCollection<ElmaPolygon> GrassPolygons { get; } = grassPolygons;

        /// <summary>
        /// Gets the bottom-left most vertex coordinate (min-x, min-y).
        /// </summary>
        public Position<double> BottomLeft { get; } = bottomLeft;

        /// <summary>
        /// Gets the top-right most vertex coordinate (max-x, max-y).
        /// </summary>
        public Position<double> TopRight { get; } = topRight;

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
                var polygons = new List<ElmaPolygon>();
                var grassPolygons = new List<ElmaPolygon>();
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

                    if (vertices.Count > 2)
                    {
                        var polygon = new ElmaPolygon(i, vertices, grass != 0);
                        if (polygon.IsGrass)
                        {
                            grassPolygons.Add(polygon);
                        }
                        else
                        {
                            polygons.Add(polygon);
                        }
                    }
                }

                UpdateZOrder(polygons);

                var minX = polygons.SelectMany(p => p.Vertices).Select(p => p.X).Min();
                var maxX = polygons.SelectMany(p => p.Vertices).Select(p => p.X).Max();
                var minY = polygons.SelectMany(p => p.Vertices).Select(p => p.Y).Min();
                var maxY = polygons.SelectMany(p => p.Vertices).Select(p => p.Y).Max();

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

                var res = new ElmaLevel(levelName, link, objects, polygons, grassPolygons, new Position<double>(minX, minY), new Position<double>(maxX, maxY));
                return res;
            }
            catch (EndOfStreamException)
            {
                throw new FormatException("The end of the input data has been reached unexpectedly");
            }
        }

        private static void UpdateZOrder(List<ElmaPolygon> polygons)
        {
            var minX = polygons.SelectMany(p => p.Vertices).Select(p => p.X).Min() - 20; // infinity...
            var minY = polygons.SelectMany(p => p.Vertices).Select(p => p.Y).Min() - 20; // infinity...
            var pTest2 = new Position<double>(minX, minY); // Poiny at infinity.
            for (int m = 0; m < polygons.Count; m++)
            {
                // Determine how many other polygons a ray from this polygon to infinity passes out of.
                var pChild = polygons[m];
                var pTest1 = pChild.Vertices[0];
                int crosses = 0;
                for (int n = 0; n < polygons.Count; n++)
                {
                    var pParent = polygons[n];
                    if (m == n)
                    {
                        continue;
                    }

                    var theseCrosses = 0;
                    for (int i = 0; i < pParent.Vertices.Count - 1; i++)
                    {
                        var pTest3 = pParent.Vertices[i];
                        var pTest4 = pParent.Vertices[i + 1];
                        if (CheckIntersect(pTest1, pTest2, pTest3, pTest4))
                        {
                            theseCrosses++;
                        }
                    }

                    if (CheckIntersect(pTest1, pTest2, pParent.Vertices[0], pParent.Vertices[^1]))
                    {
                        theseCrosses++;
                    }

                    // If the ray crossed in and out of the polygon, then the polygon doesn't count, since it's on the same Z-order.
                    if (theseCrosses % 2 != 0)
                    {
                        crosses += theseCrosses;
                    }
                }

                pChild.ZOrder = crosses;
            }
        }

        private static bool CheckIntersect(Position<double> p1, Position<double> q1, Position<double> p2, Position<double> q2)
        {
            return CheckIntersect(p1.X, p1.Y, q1.X, q1.Y, p2.X, p2.Y, q2.X, q2.Y);
        }

        /// <summary>
        /// Converted from Python code posted by 'Bram Cohen' on Stackoverflow: https://stackoverflow.com/a/77384870
        /// Checks if two line segments intersect.
        /// </summary>
        /// <param name="x1">Segment 1, Point 1.X.</param>
        /// <param name="y1">Segment 1, Point 1.Y.</param>
        /// <param name="x2">Segment 1, Point 2.X.</param>
        /// <param name="y2">Segment 1, Point 2.X.</param>
        /// <param name="x3">Segment 2, Point 1.X.</param>
        /// <param name="y3">Segment 2, Point 1.Y.</param>
        /// <param name="x4">Segment 2, Point 2.X.</param>
        /// <param name="y4">Segment 2, Point 2.Y.</param>
        /// <returns>True if the segments intersect, false if not.</returns>
        private static bool CheckIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            const double epsilon = 1e-7;
            var c_area = Area(x1, y1, x2, y2, x3, y3);
            var d_area = Area(x1, y1, x2, y2, x4, y4);
            if (Math.Abs(c_area) < epsilon)
            {
                if (Math.Abs(x3 - x1) < epsilon)
                {
                    if (Math.Min(y1, y2) - epsilon < y3 && y3 < Math.Max(y1, y2) + epsilon)
                    {
                        return true;
                    }
                }
                else
                {
                    if (Math.Min(x1, x2) - epsilon < x3 && x3 < Math.Max(x1, x2) + epsilon)
                    {
                        return true;
                    }
                }

                if (Math.Abs(d_area) > epsilon)
                {
                    return false;
                }
            }

            if (Math.Abs(d_area) < epsilon)
            {
                if (Math.Abs(x4 - x1) < epsilon)
                {
                    if (Math.Min(y1, y2) - epsilon < y4 && y4 < Math.Max(y1, y2) + epsilon)
                    {
                        return true;
                    }
                }
                else
                {
                    if (Math.Min(x1, x2) - epsilon < x4 && x4 < Math.Max(x1, x2) + epsilon)
                    {
                        return true;
                    }
                }

                if (Math.Abs(c_area) > epsilon)
                {
                    return false;
                }

                if (Math.Abs(x3 - x1) < epsilon)
                {
                    return (y1 < y3) != (y1 < y4);
                }
                else
                {
                    return (x1 < x3) != (x1 < x4);
                }
            }

            if (c_area > 0 == d_area > 0)
            {
                return false;
            }

            var a_area = Area(x3, y3, x4, y4, x1, y1);
            var b_area = Area(x3, y3, x4, y4, x2, y2);
            return (a_area > 0) != (b_area > 0);
        }

        /// <summary>
        /// Converted from Python code posted by 'Bram Cohen' on Stackoverflow: https://stackoverflow.com/a/77384870
        /// </summary>
        private static double Area(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
        }
    }
}