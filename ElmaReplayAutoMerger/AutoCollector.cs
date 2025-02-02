namespace ElmaReplayAutoMerger
{
    using ElmaReplayIO;

    static class AutoCollector
    {
        static readonly System.Timers.Timer changeTimer = new(TimeSpan.FromMilliseconds(100));

        static string srcPath = string.Empty;

        static string targetPath = string.Empty;

        public static int Run(string[] args)
        {
            // args[0] is -collect
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid amount of arguments.");
                return 1;
            }

            srcPath = args[1];
            targetPath = args[2];

            if (!Directory.Exists(srcPath))
            {
                Console.WriteLine("Source path does not exist");
                return 2;
            }

            if (!Directory.Exists(targetPath))
            {
                Console.WriteLine("Target path does not exist");
                return 3;
            }

            var fsw = new FileSystemWatcher(srcPath)
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
            var path = Path.Combine(srcPath, "!last.rec");

            Replay newRec;
            try
            {
                using var fileRec = File.OpenRead(path);
                newRec = Replay.ParseFrom(fileRec, path);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Failed to read new replay: " + ex.Message);
                return;
            }

            var levelname = newRec.MainRide.Header.LevelName;
            Console.WriteLine("New replay for level: " + levelname);

            try
            {
                var now = DateTime.Now;
                var folderName = Path.GetFileNameWithoutExtension(levelname);
                var outputPath = Path.Combine(targetPath, folderName);
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                var outputName = $"{now:yyyyMMdd-HHmmss-fff}_{folderName}.rec";
                outputPath = Path.Combine(outputPath, outputName);
                File.Copy(path, outputPath);
                Console.WriteLine($"Replay copied to {outputName}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Failed to copy replay: " + ex.Message);
            }
        }
    }
}