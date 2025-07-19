
namespace ElmaReplayPainter;
using ElmaReplayIO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq.Expressions;

/// <summary>
/// An example program for the ElmaReplayIO library.
/// Implements a program that reads a base replay and automatically creates a merge if a new replay in this level is drives.
/// </summary>
static class Program
{
    static int PrintSyntax(int returnCode)
    {
        Console.WriteLine("Syntax: ElmaReplayPainter <drawall|drawdead|dump|rescuelev> <dumpsourcefile> <size> <levelname> <replayfolder> <output file name>");
        return returnCode;
    }

    static int DumpRec(string[] args)
    {
        if (args.Length != 2)
        {
            return PrintSyntax(7);
        }

        var src = args[1];
        if (!File.Exists(src))
        {
            return PrintSyntax(8);
        }

        using var fs = File.OpenRead(src);
        var rec = Replay.ParseFrom(fs, src);
        Console.WriteLine($"Level name: {rec.MainRide.Header.LevelName} ({rec.MainRide.Header.Link})");
        Console.WriteLine($"Is flag tag: {rec.MainRide.Header.IsFlagTag}");
        Console.WriteLine($"Number of rides: {rec.Count}");
        int rideNum = 0;
        foreach (var ride in rec)
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"Ride #{rideNum++}");
            Console.WriteLine($"Header frame count: {ride.Header.FrameCount}");
            Console.WriteLine($"Frame count: {ride.Frames.Count}");
            Console.WriteLine($"Apples taken: {ride.ApplesTaken}");
            Console.WriteLine($"Event count: {ride.Events.Count}");
        }

        return 0;
    }

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            return PrintSyntax(1);
        }

        var toDo = args[0];
        if (!(new[] { "drawall", "drawdead", "dump", "rescuelev" }).Contains(toDo))
        {
            return PrintSyntax(2);
        }

        if (toDo == "dump")
        {
            return DumpRec(args);
        }

        if (toDo == "rescuelev")
        {
            return RescueLev(args);
        }

        if (args.Length != 5)
        {
            return PrintSyntax(1);
        }

        if (!int.TryParse(args[1], out int imageWidth))
        {
            Console.WriteLine("Invalid image width.");
            return PrintSyntax(3);
        }

        var levelFile = args[2];
        if (!File.Exists(levelFile))
        {
            Console.WriteLine("Level does not exist.");
            return PrintSyntax(4);
        }

        var recFolder = args[3];
        if (!Directory.Exists(recFolder))
        {
            Console.WriteLine("Replay folder does not exist.");
            return PrintSyntax(5);
        }

        var outputFile = args[4];
        if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputFile)))
        {
            Console.WriteLine("Output folder does not exist.");
            return PrintSyntax(6);
        }

        Console.WriteLine("Parsing level.");
        ElmaLevel level;
        using (var fs = File.OpenRead(levelFile))
        {
            level = ElmaLevel.ParseFrom(fs);
        }

        var allRecs = Directory.GetFiles(recFolder, "*.rec").ToList();

        Console.WriteLine($"Parsing {allRecs.Count} replays...");
        allRecs.Sort();
        var recs = new List<Replay>();
        foreach (var r in allRecs)
        {
            using var fs = File.OpenRead(r);
            recs.Add(Replay.ParseFrom(fs, r));
        }

        recs = recs.OrderBy(r => r.MainRide.ApplesTaken).ToList();

        // Determine image size and scaling
        var minX = level.BottomLeft.X;
        var maxX = level.TopRight.X;
        var minY = level.BottomLeft.Y;
        var maxY = level.TopRight.Y;

        var scale = imageWidth / (maxX - minX);
        int imageHeight = (int)((maxY - minY) * scale);

        var maxFrames = recs.Max(r => r[0].Header.FrameCount);
        var maxApples = recs.Max(r => r.MainRide.ApplesTaken);
        var third = (int)Math.Ceiling((double)maxApples / 3);
        Console.WriteLine("Drawing image...");

        using Image<Rgba32> imageBackGround = new(imageWidth, imageHeight);
        PaintLevel(imageBackGround, (float)scale, level);

        using Image<Rgba32> frameImage = new(imageWidth, imageHeight);
        frameImage.Mutate(i => i.DrawImage(imageBackGround, 1));
        foreach (var r in recs)
        {
            var n = r.MainRide.ApplesTaken;

            Color col;
            int start;
            if (toDo == "drawall")
            {
                start = 0;
                if (n <= third)
                {
                    var x = (byte)Math.Min(255, (double)n / third * (255 - 64) + 64);
                    col = Color.FromRgb(0, 0, x);
                }
                else if (n <= 2 * third)
                {
                    var x = (byte)Math.Min(255, (double)(n - third) / third * (255 - 64) + 64);
                    col = Color.FromRgb(0, x, 0);
                }
                else
                {
                    var x = (byte)Math.Min(255, (double)(n - 2 * third) / third * (255 - 64) + 64);
                    col = Color.FromRgb(x, 0, 0);
                }
            }
            else
            {
                start = r.MainRide.Header.FrameCount - 1;
                col = Color.Red;
            }

            var brushC = new SolidBrush(col);
            for (int i = start; i < r.MainRide.Header.FrameCount; i++)
            {
                var ride = r[0];
                if (ride.Header.FrameCount > i)
                {
                    var frame = ride.Frames[i];
                    var pos = frame.BikePosition;
                    var x = (float)((frame.BikePosition.X - minX) * scale);
                    var y = (float)((frame.BikePosition.Y - minY) * scale);
                    frameImage.Mutate(i =>
                    {
                        var circle = new EllipsePolygon(new PointF(x, y), 3);
                        i.Fill(brushC, circle);
                    });
                }
            }

        }

        Console.WriteLine("Image done.");

        frameImage.SaveAsPng(outputFile);

        Console.WriteLine("Saved.");

        return 0;
    }

    static void PaintLevel(Image<Rgba32> image, float scale, ElmaLevel level)
    {
        var brushFore = new SolidBrush(Color.Black);
        var brushBack = new SolidBrush(Color.White);
        var minX = level.BottomLeft.X;
        var minY = level.BottomLeft.Y;
        image.Mutate(i => i.Clear(brushFore));
        var polys = level.Polygons.OrderBy(p => p.ZOrder);

        int pi = 0;
        foreach (var p in polys)
        {
            var points = new PointF[p.Vertices.Count];
            int i = 0;
            foreach (var point in p.Vertices)
            {
                points[i++] = new PointF((float)(point.X - minX) * scale, (float)(point.Y - minY) * scale);
            }

            var polygon = new Polygon(points);
            var b = p.ZOrder % 2 != 0 ? brushFore : brushBack;
            pi++;
            image.Mutate(i => i.Fill(b, polygon));
        }
    }

    static int RescueLev(string[] args)
    {
        if (args.Length != 6)
        {
            Console.WriteLine("Synxtax: rescuelev <input> <output> <incl. vertices> <incl. objects> <incl. pics>");
            Console.WriteLine("Specify incl. as 1 or 0, or true or false");
            return -1;
        }

        var okInclVerts = TryParseBool(args[3], out bool inclVerts);
        var okInclObjs = TryParseBool(args[4], out bool inclObjs);
        var okInclPics = TryParseBool(args[5], out bool inclPics);

        if (!okInclVerts || !okInclVerts || !okInclObjs)
        {
            Console.WriteLine("Synxtax: rescuelev <input> <output> <incl. vertices> <incl. objects> <incl. pics>");
            Console.WriteLine("Specify incl. as 1 or 0, or true or false");
            return -1;
        }

        var levName = args[1];
        if (!File.Exists(levName))
        {
            Console.WriteLine("Input file not found");
            return -1;
        }

        ElmaLevel lev;
        try
        {
            var bytes = File.ReadAllBytes(levName);
            using var ms = new MemoryStream(bytes);
            lev = ElmaLevel.ParseFrom(ms);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading the input file: {ex.Message}");
            return -2;
        }

        try
        {
            using var output = File.Create(args[2]);
            lev.SaveTo(output, inclVerts, inclObjs, inclPics);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving the output file: {ex.Message}");
            return -2;
        }
    }

    static bool TryParseBool(string arg, out bool value)
    {
        var okInclVerts = bool.TryParse(arg, out value);
        if (okInclVerts)
        {
            return true;
        }
        else 
        {
            if (arg == "1" || arg == "0")
            {
                value = arg == "1";
                return true;
            }
        }

        return false;
    }
}
