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
        static Replay? baseReplay;
        static byte[]? baseRecData;
        static string? baseRecLevelName;
        static int Main(string[] args)
        {
            var filename = string.Empty;
            var outputBaseData = true;
            var outputAppleTimes = true;
            var parseEvents = false;
            var parseFrames = false;
            var runAutoComparison = false;

            Range eventsRange = default;
            Range framesRange = default;

            if (args.Length == 0)
            {
                Console.WriteLine("Syntax: ");
                Console.WriteLine("ElmaRecReaderAndMaker <options> <recPath>");
                Console.WriteLine("    <options>:");
                Console.WriteLine("        -nobase - Don't output base replay stats");
                Console.WriteLine("        -noapples - Don't output apple times");
                Console.WriteLine("        -f=<RangeExpression> - extract frame data");
                Console.WriteLine("        -e=<RangeExpression> - extract event data");
                Console.WriteLine("        -check - Start monitoring replay folder for new times to compare");
                Console.WriteLine("    <recPath>: The path to the base replay file to use as comparison.");
                return 1;
            }

            try
            {
                for (int a = 0; a < args.Length; a++)
                {
                    if (args[a].StartsWith("-e="))
                    {
                        parseEvents = true;
                        eventsRange = ParseRange(args[a][3..]);
                    }
                    else if (args[a].StartsWith("-f="))
                    {
                        parseFrames = true;
                        framesRange = ParseRange(args[a][3..]);
                    }
                    else if (args[a] == "-check")
                    {
                        runAutoComparison = true;
                    }
                    else if (args[a] == "-nobase")
                    {
                        outputBaseData = false;
                    }
                    else if (args[a] == "-noapples")
                    {
                        outputAppleTimes = false;
                    }
                    else
                    {
                        filename = args[a];
                    }
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Invalid range arguments: {ex.Message}");
                return 1;
            }

            var fi = new FileInfo(filename);
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

            baseReplay = rec;
            var baseRide = rec.MainRide;

            if (outputBaseData) DumpBaseStats(baseRide);
            if (outputAppleTimes) DumpAppleTimes(baseRide);
            if (parseFrames) DumpFrames(baseRide, framesRange);
            if (parseEvents) DumpEvents(baseRide, eventsRange);

            if (!runAutoComparison)
            {
                return 0;
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
            Console.WriteLine("Press Enter to stop and exit the program.");
            Console.ReadLine();
            return 0;
        }

        static void DumpBaseStats(Ride baseRide)
        {
            // Output some stats
            Event? lastObjectTouch = default;
            if (baseRide.Events.Any(e => e.Type == EventType.ObjectTouch))
            {
                lastObjectTouch = baseRide.Events.Where(e => e.Type == EventType.ObjectTouch).Last();
            }

            Console.WriteLine($@"Input replay stats:
    Level: {baseRide.Header.LevelName} ({baseRide.Header.Link})
    Frames: {baseRide.Header.FrameCount}
    Events: {baseRide.Events.Count}
    Dur. (frames): {baseRide.Header.FrameCount * 33.3333 / 1000.0} s
    Dur. (object): {(lastObjectTouch.HasValue ? lastObjectTouch.Value.Time.TotalSeconds.ToString("0.000") : "---")} s");

        }

        static void DumpAppleTimes(Ride baseRide)
        {
            Console.WriteLine("Apple take times:");
            var appleTouches = baseRide.Events.Where(e => e.Type == EventType.AppleTake);
            var i = 0;
            foreach (var f in appleTouches)
            {
                Console.WriteLine($"    Apple #{i++:000} : {f.Time.TotalSeconds:0.000} s");
            }
        }

        static void DumpFrames(Ride ride, Range range)
        {
            try
            {
                var offsets = range.GetOffsetAndLength(ride.Header.FrameCount);
                var frames = ride.Frames.Skip(offsets.Offset).Take(offsets.Length);
                int idx = offsets.Offset;
                foreach (var f in frames)
                {
                    DumpFrame(idx++, f);
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.WriteLine("Invalid frame range: Arguments of out range");
            }
        }

        static void DumpFrame(int idx, Frame frame)
        {
            Console.WriteLine($@"Frame #{idx}
    Bike       : {frame.BikePosition} Rotation {frame.BikeRotation}
    Head       : {frame.HeadPosition}
    Left Wheel : {frame.LeftWheelPosition} Rotation: {frame.LeftWheelRotation}
    Right Wheel: {frame.RightWheelPosition} Rotation: {frame.RightWheelRotation}
    Direction  : {frame.Direction}
    Throttle   : {(frame.ThrottleApplied ? "yes" : "no")}");
        }

        static void DumpEvents(Ride ride, Range range)
        {
            try
            {
                var offsets = range.GetOffsetAndLength(ride.Header.FrameCount);
                var events = ride.Events.Skip(offsets.Offset).Take(offsets.Length);
                int idx = offsets.Offset;
                foreach (var f in events)
                {
                    DumpEvent(idx++, f);
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.WriteLine("Invalid event range: Arguments of out range");
            }
        }

        static void DumpEvent(int idx, Event ev)
        {
            Console.WriteLine($@"Event #{idx}
    Time   : {ev.Time}
    Type   : {ev.Type}
    Object : {ev.ObjectID}");
        }

        static Range ParseRange(string range)
        {
            if (range == "..")
            {
                return new Range(Index.FromStart(0), Index.FromEnd(1));
            }

            var parts = range.Split("..");
            var l = parts.Length;
            if (l != 2)
            {
                throw new FormatException("Invalid range expression - invalid count");
            }

            Index startIdx;
            Index endIdx;

            if (string.IsNullOrEmpty(parts[0]))
            {
                startIdx = Index.FromStart(0);
                endIdx = ParseIndex(parts[1]);
            }
            else if (string.IsNullOrEmpty(parts[1]))
            {
                startIdx = ParseIndex(parts[0]);
                endIdx = Index.FromEnd(1);
            }
            else
            {
                startIdx = ParseIndex(parts[0]);
                endIdx = ParseIndex(parts[1]);
            }

            return new Range(startIdx, endIdx);
        }

        static Index ParseIndex(string v)
        {
            bool fromEnd = false;
            if (v.StartsWith("^"))
            {
                fromEnd = true;
                v = v[1..];
            }

            if (int.TryParse(v, out int x))
            {
                return new Index(x, fromEnd);
            }
            else
            {
                throw new FormatException($"Invalid range expression, invalid number: {v}");
            }
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

            Console.WriteLine();
            Console.WriteLine("-----------------------------------");
            var path = Path.Combine(recPath!, "!last.rec");

            Replay newRec;
            try
            {
                using var fileRec = File.OpenRead(path);
                newRec = Replay.ParseFrom(fileRec);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Failed to read new replay: " + ex.Message);
                return;
            }

            Console.WriteLine("New replay for level: " + newRec.MainRide.Header.LevelName);

            if (newRec.MainRide.Header.LevelName != baseRecLevelName)
            {
                Console.WriteLine($"New replay is for a different level than the base replay!");
                return;
            }

            var baseAppleTimes = baseReplay!.MainRide.Events.Where(e => e.Type == EventType.AppleTake).ToList();
            var newAppleTimes = newRec!.MainRide.Events.Where(e => e.Type == EventType.AppleTake).ToList();
            for (int i = 0; i < Math.Max(baseAppleTimes.Count, newAppleTimes.Count); i++)
            {
                Event? bat = i < baseAppleTimes.Count ? baseAppleTimes[i] : null;
                Event? nat = i < newAppleTimes.Count ? newAppleTimes[i] : null;
                var output = new AppleTimeOutput(i, bat, nat);
                Console.WriteLine(output.ToString());
            }

            try
            {
                var outpath = Path.Combine(recPath!, "!automrg.rec");
                var mergeRec = new List<Ride>
            {
                baseReplay!.MainRide,
                newRec.MainRide,
            };
                var merged = new Replay(mergeRec);
                using var os = File.OpenWrite(outpath);
                merged.WriteReplay(os);
                Console.WriteLine($"Created auto-merge file!");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Failed to merge replays: " + ex.Message);
            }
        }

        private class AppleTimeOutput
        {
            private readonly Event? oldAt;
            private readonly Event? newAt;

            private readonly int idx;

            public AppleTimeOutput(int idx, Event? oldAt, Event? newAt)
            {
                this.idx = idx;
                this.oldAt = oldAt;
                this.newAt = newAt;
            }

            public override string ToString()
            {
                if (this.oldAt.HasValue && this.newAt.HasValue)
                {
                    var tOld = oldAt.Value.Time.TotalSeconds;
                    var tNew = newAt.Value.Time.TotalSeconds;
                    var delta = tNew - tOld;
                    return $"Apple #{this.idx:000} - {tOld:0.000} - {tNew:0.000} - Delta: {delta:0.000}s";
                }
                else if (this.oldAt.HasValue)
                {
                    var tOld = oldAt.Value.Time.TotalSeconds;
                    return $"Apple #{this.idx:000} - {tOld:0.000} - ---";
                }
                else if (this.newAt.HasValue)
                {
                    var tNew = newAt.Value.Time.TotalSeconds;
                    return $"Apple #{this.idx:000} - --- - {tNew:0.000}";
                }
                else
                {
                    return $"Apple #{this.idx:000} - --- - ---";
                }
            }
        }
    }
}