namespace ElmaReplayAutoMerger
{
    using ElmaReplayIO;

    /// <summary>
    /// An example program for the ElmaReplayIO library.
    /// Implements a program that reads a base replay and automatically creates a merge if a new replay in this level is drives.
    /// </summary>
    static class Program
    {
        static readonly System.Timers.Timer changeTimer = new(TimeSpan.FromMilliseconds(500));
        static string? recPath;
        static byte[]? baseRecData;
        static string? baseRecLevelName;

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Syntax: ");
                Console.WriteLine("ElmaRecReaderAndMaker <recPath>");
                Console.WriteLine("    <recPath>: The path to the base replay file to use as comparison.");
                return 1;
            }

            var fi = new FileInfo(args[0]);
            if (!fi.Exists)
            {
                Console.WriteLine("Replay file does not exist");
                return 2;
            }

            // Parse the base replay
            Replay rec;
            try
            {
                using var fs = File.OpenRead(fi.FullName);
                rec = Replay.ParseFrom(fs);
            }
            catch (RecParsingException ex)
            {
                Console.WriteLine("Failed to read input replay:");
                Console.WriteLine(ex.Message);
                return 3;
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to read the input file:");
                Console.WriteLine(ex.Message);
                return 4;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception while parsing the input file:");
                Console.WriteLine(ex.ToString());
                return 5;
            }

            if (rec.Count > 1)
            {
                Console.WriteLine("Using a multi-ride replay as base replay is currently not supported.");
                return 6;
            }

            var baseRide = rec.MainRide;
            // Output some stats
            Console.WriteLine("Input replay stats:");
            Console.WriteLine($"{baseRide.Header.FrameCount} frames");
            Console.WriteLine($"{baseRide.Header.FrameCount * 33.3333 / 1000.0} s duration (by frame count).");
            if (baseRide.Events.Any(e => e.Type == EventType.ObjectTouch))
            {
                var lastObjectTouch = baseRide.Events.Where(e => e.Type == EventType.ObjectTouch).Last();
                Console.WriteLine($"{lastObjectTouch.Time.TotalSeconds:0.000} s last object touch.");
            }

            var appleTouches = baseRide.Events.Where(e => e.Type == EventType.AppleTake);
            var i = 0;
            Console.WriteLine("Apple takes:");
            foreach (var f in appleTouches)
            {
                Console.WriteLine($"Apple #{i:000} taken at {f.Time.TotalSeconds:0.000} s");
            }

            // Setup a file system watcher that checks the !last.rec in the same folder as the base replay
            var path = Path.GetDirectoryName(fi.FullName)!;
            Console.WriteLine("Starting !last.rec checks and auto-merges...");

            baseRecData = File.ReadAllBytes(fi.FullName);
            baseRecLevelName = baseRide.Header.LevelName;
            recPath = path;

            var fsw = new FileSystemWatcher(path)
            {
                Filter = "!last.rec",
                NotifyFilter = NotifyFilters.LastWrite,
            };

            fsw.Changed += HandleLastChanged;
            changeTimer.Elapsed += HandleChangeTimerElapsed;
            fsw.EnableRaisingEvents = true;
            Console.WriteLine("Press any key to stop and exit the program.");
            Console.ReadKey();
            return 0;
        }

        static void HandleLastChanged(object? sender, FileSystemEventArgs e)
        {
            // Add additional timer step here, because the change event is typically called twice in quick succession.
            changeTimer.Stop();
            changeTimer.Start();
        }

        static void HandleChangeTimerElapsed(object? sender, EventArgs e)
        {
            changeTimer.Stop();

            var path = Path.Combine(recPath!, "!last.rec");
            var newData = File.ReadAllBytes(path);
            using var ms = new MemoryStream(newData);
            var newRec = Replay.ParseFrom(ms);
            if (newRec[0].Header.LevelName != baseRecLevelName)
            {
                Console.WriteLine($"New replay is for a different level than the base replay!");
                return;
            }

            var outpath = Path.Combine(recPath!, "!automrg.rec");
            var allData = new List<byte>();
            allData.AddRange(baseRecData!);
            newData[8] = 1; // Is multi ride
            allData.AddRange(newData);
            allData[8] = 1; // Is multi ride
            File.WriteAllBytes(outpath, allData.ToArray());
            Console.WriteLine($"Created auto-merge file!");
        }
    }
}