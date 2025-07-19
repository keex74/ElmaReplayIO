/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines an elma level, with only barebones properties for now.
    /// </summary>
    /// <param name="intact">Indicates if the level was read intact.</param>
    /// <param name="name">The level name.</param>
    /// <param name="lgrFile">The name of the LGR file.</param>
    /// <param name="groundTexture">The name of the ground texture.</param>
    /// <param name="skyTexture">The name of the sky texture.</param>
    /// <param name="link">The level link parameter.</param>
    /// <param name="integrity">The integrity values.</param>
    /// <param name="objects">The list of objects in the level.</param>
    /// <param name="polygons">The list of non-grass polygons.</param>
    /// <param name="grassPolygons">The list of grass polygons.</param>
    /// <param name="pictures">The list of pictures in the level.</param>
    /// <param name="bottomLeft">The bottom-left bounds.</param>
    /// <param name="topRight">The top-right bounds.</param>
    /// <param name="top10Data">The raw top 10 data.</param>
    public class ElmaLevel(bool intact, string name, string lgrFile, string groundTexture, string skyTexture, uint link, IReadOnlyList<double> integrity, IReadOnlyList<ObjectDescription> objects, IReadOnlyList<ElmaPolygon> polygons, IReadOnlyList<ElmaPolygon> grassPolygons, IReadOnlyList<ElmaPicture> pictures, Position<double> bottomLeft, Position<double> topRight, IReadOnlyList<byte> top10Data)
    {
        private const string EmptyTop10 = "FQVqt4ntWcRI/49zdrxwwN9XtC8Nnge8YwhvigkorTjgc/mggAAAijdvaWAXCXdBrIwmNKwCp5jJpf6A38CXtFDXHcpEQftH99hgOITLTIIxJJqRdskOa8x1/Mz4pXSYjgK7aETLTL2ZxDT0i3pMaFhTmnDNTyvM0RrggC6PczTnDtRGHb0Cmr8zJ4QGWBGkEOQddSqweC/mBobvpNX5/JEuVHRd+JGx9qJV+SP7R44PRfTnXVQ5jNEujzi/j7tooDjTyIlJkTu9AgOI5Sx/nLWTX1egoeRLEVz9a4Q0xvyRsRcJkTuWpXSYP71efaaJhCfgUmSnpaKQQE9zNJgEkB/NuKWBQ4jE2MnMGS0el2VG/DVYMtcqLf1KFpiB2oSdGVuMsIUVVb44pf6Ulcvj2lYiaQSxjQdGk2ymNF1U0PiEkB/hw7Zn7TiYYPZgyFsCXxzCp12jfs8kpxt/e95Cp9P2r6vMo2QDLCMVvq5AT0X0UAV+6ffL/Yz/j1L7R1MdM3a8q79AuOAX7wANQj5mE/ZT9o4ctbRQyroeuBsCEE3miYTs4fED3eueuL+pMv7PgCHDLH8mTsBIaK3jFey6EUhO1HRxJJoorbW0mAQaVtPivpTQEvuwV2W8q1Z3BuI7W/U+RQEV5a9pBG8h5DHVNKwwDyTVBqc8kVy1KmjOa4RBNmDIE7S5/Ktjn7M7GcTYEaQdowi3iczrngAA88sexed3fCiggEiJSRvUiMRVBs6mqv+Puy04kbG7o4tZt8TsMErooNx647mgOO3C1b44KBaYbc9zVRNLTBnyqQSd60KNIddLKweb2yNQboLV2AQ0Fb447bUQtlN59VgyiCDp1pjq3pcqlmqjXQz4L8Wsa1bGeVHSKSVn7Vn/nDjTp4TLBEhvxSKXwTavFMOV2GDpTA==";

        /// <summary>
        /// Gets a value indicating whether the level was intact while reading.
        /// </summary>
        public bool Intact { get; } = intact;

        /// <summary>
        /// Gets the name of the level.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the name of the level.
        /// </summary>
        public string LGR { get; } = lgrFile;

        /// <summary>
        /// Gets the name of the ground texture.
        /// </summary>
        public string GroundTexture { get; } = groundTexture;

        /// <summary>
        /// Gets the name of the sky texture.
        /// </summary>
        public string SkyTexture { get; } = skyTexture;

        /// <summary>
        /// Gets the link value of the level.
        /// </summary>
        public uint Link { get; set; } = link;

        /// <summary>
        /// Gets the objects in the level.
        /// </summary>
        public IReadOnlyList<ObjectDescription> Objects { get; } = objects;

        /// <summary>
        /// Gets the level polygons.
        /// </summary>
        public IReadOnlyList<ElmaPolygon> Polygons { get; } = polygons;

        /// <summary>
        /// Gets the level's grass polygons.
        /// </summary>
        public IReadOnlyList<ElmaPolygon> GrassPolygons { get; } = grassPolygons;

        /// <summary>
        /// Gets the level's pictures.
        /// </summary>
        public IReadOnlyList<ElmaPicture> Pictures { get; } = pictures;

        /// <summary>
        /// Gets the bottom-left most vertex coordinate (min-x, min-y).
        /// </summary>
        public Position<double> BottomLeft { get; } = bottomLeft;

        /// <summary>
        /// Gets the top-right most vertex coordinate (max-x, max-y).
        /// </summary>
        public Position<double> TopRight { get; } = topRight;

        /// <summary>
        /// Gets the raw top10 data, not decoded.
        /// </summary>
        public IReadOnlyList<byte> Top10Data { get; } = top10Data;

        /// <summary>
        /// Gets the level integrity values.
        /// </summary>
        public IReadOnlyList<double> Integrity { get; } = integrity;

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
                var integrity = new List<double>();
                for (int i = 0; i < 4; i++)
                {
                    // Level integrity status
                    integrity.Add(br.ReadDouble());
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
                var doubleCount = br.ReadDouble();
                var objectCount = (int)Math.Floor(doubleCount); // -0.4643643, treated by floor function
                var objects = new List<ObjectDescription>();
                for (int i = 0; i < objectCount; i++)
                {
                    var x = br.ReadDouble();
                    var y = br.ReadDouble();
                    var pos = new Position<double>(x, y);
                    var typeValue = br.ReadInt32();
                    var gravityValue = br.ReadInt32();
                    var animation = br.ReadInt32();
                    try
                    {
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

                        objects.Add(new ObjectDescription(pos, type, gravity, animation));
                    }
                    catch (FormatException)
                    {
                        objects.Add(new ObjectDescription(pos, (ObjectType)typeValue, (AppleGravityDirection)gravityValue, animation));
                    }
                }

                doubleCount = br.ReadDouble();
                var pictureCount = (int)Math.Floor(doubleCount);
                var pictures = new List<ElmaPicture>();
                for (var i = 0; i < pictureCount; i++)
                {
                    var pictureName = new string(br.ReadChars(10).TakeWhile(c => c != 0).ToArray()).Trim();
                    var textureName = new string(br.ReadChars(10).TakeWhile(c => c != 0).ToArray()).Trim();
                    var maskName = new string(br.ReadChars(10).TakeWhile(c => c != 0).ToArray()).Trim();
                    var x = br.ReadDouble();
                    var y = br.ReadDouble();
                    var distance = br.ReadInt32();
                    var clipping = br.ReadInt32();
                    var picture = new ElmaPicture(pictureName, textureName, maskName, new Position<double>(x, y), distance, clipping);
                    pictures.Add(picture);
                }

                var endOfData = br.ReadUInt32();
                uint EndOfDataMarker = 0x0067103A;
                var levelOk = endOfData == EndOfDataMarker;
                
                // Top10 data is skipped here.
                var top10 = br.ReadBytes(688);
                
                var endOfFile = br.ReadUInt32();
                uint EndOfFileMarker = 0x00845D52;
                levelOk &= endOfFile == EndOfFileMarker;

                var res = new ElmaLevel(levelOk, levelName, lgr, groundTexture, skyTexture, link, integrity, objects, polygons, grassPolygons, pictures, new Position<double>(minX, minY), new Position<double>(maxX, maxY), top10);
                return res;
            }
            catch (EndOfStreamException)
            {
                throw new FormatException("The end of the input data has been reached unexpectedly");
            }
        }
        
        /// <summary>
        /// Saves the level data to the output stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="saveVerts">True to include vertex data.</param>
        /// <param name="saveObjects">True to include object data.</param>
        /// <param name="savePictures">True to include picture data.</param>
        public void SaveTo(Stream stream, bool saveVerts, bool saveObjects, bool savePictures)
        {
            var enc = System.Text.Encoding.ASCII;
            using var bw = new BinaryWriter(stream, System.Text.Encoding.ASCII);
            bw.Write(enc.GetBytes("POT14"));
            var rnd = new Random();

            // Fresh identifier
            var ident = rnd.Next();
            var identBytes = BitConverter.GetBytes(ident);
            bw.Write(identBytes, 0, 2);
            bw.Write(identBytes);

            var sum = 0.0;
            if (saveVerts)
            {
                foreach (var p in this.Polygons)
                {
                    foreach (var v in p.Vertices)
                    {
                        sum += (v.X + v.Y);
                    }
                }
            }

            if (saveObjects)
            {
                sum += this.Objects.Sum(p => p.Position.X + p.Position.Y + (int)p.Type);
            }

            if (savePictures)
            {
                sum += this.Pictures.Sum(p => p.Position.X + p.Position.Y);
            }

            sum *= 3247.764325643;
            var integrity = new double[4];
            integrity[0] = sum;
            integrity[1] = rnd.Next(5871) + 11877 - sum;
            integrity[2] = rnd.Next(5871) + 11877 - sum; // Assume no errors
            integrity[3] = rnd.Next(6102) + 12112 - sum;

            foreach (var b in integrity)
            {
                bw.Write(b);
            }

            bw.Write(GetPaddedStringArray(this.Name, 51));
            bw.Write(GetPaddedStringArray(this.LGR, 16));
            bw.Write(GetPaddedStringArray(this.GroundTexture, 10));
            bw.Write(GetPaddedStringArray(this.SkyTexture, 10));

            if (saveVerts)
            {
                var allPolys = new List<ElmaPolygon>();
                allPolys.AddRange(this.Polygons);
                allPolys.AddRange(this.GrassPolygons);
                allPolys = allPolys.OrderBy(p => p.Index).ToList();
                bw.Write(allPolys.Count + 0.4643643);
                foreach (var p in allPolys)
                {
                    bw.Write(p.IsGrass ? 1 : 0);
                    bw.Write(p.Vertices.Count);
                    foreach (var v in p.Vertices)
                    {
                        bw.Write(v.X);
                        bw.Write(v.Y);
                    }
                }
            }
            else
            {
                bw.Write(0.4643643); // no polygons
            }

            if (saveObjects)
            {
                bw.Write(this.Objects.Count + 0.4643643);
                foreach (var obj in this.Objects)
                {
                    bw.Write(obj.Position.X);
                    bw.Write(obj.Position.Y);
                    bw.Write((int)obj.Type);
                    bw.Write((int)obj.AppleGravity);
                    bw.Write(obj.AnimationNumber);
                }
            }
            else
            {
                bw.Write(0.4643643); // no objects
            }

            if (savePictures)
            {
                foreach (var p in this.Pictures)
                {
                    bw.Write(GetPaddedStringArray(p.PictureName, 10));
                    bw.Write(GetPaddedStringArray(p.TextureName, 10));
                    bw.Write(GetPaddedStringArray(p.MaskName, 10));
                    bw.Write(p.Position.X);
                    bw.Write(p.Position.Y);
                    bw.Write(p.Distance);
                    bw.Write(p.Clipping);
                }
            }
            else
            {
                bw.Write(0.4643643); // no objects
            }

            uint EndOfDataMarker = 0x0067103A;
            uint EndOfFileMarker = 0x00845D52;

            bw.Write(EndOfDataMarker);
            bw.Write(Convert.FromBase64String(EmptyTop10));
            bw.Write(EndOfFileMarker);
        }

        private static byte[] GetPaddedStringArray(string str, int length)
        {
            var bytes = new List<byte>(length);
            var strBytes = Encoding.ASCII.GetBytes(str);
            bytes.AddRange(strBytes);
            bytes.AddRange(Enumerable.Repeat<byte>(0, length - strBytes.Length));
            return bytes.ToArray();
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