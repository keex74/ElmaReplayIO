namespace ElmaReplayAutoMerger
{
    using ElmaReplayIO;

    static class AppleStats
    {
        public static int Run(string[] args)
        {
            // [0] = -applestats
            int i = 1;
            int state = 0;
            string path = string.Empty;
            int avg = -1;
            while (i < args.Length)
            {
                var a = args[i++];
                int value;
                var hasVal = int.TryParse(a, out value);

                if (state == 0 && a == "-avg")
                {
                    state = 1;
                }
                else if (state == 0 && a == "-p")
                {
                    state = 2;
                }
                else if (state == 1)
                {
                    state = 0;
                    if (hasVal)
                    {
                        avg = value;
                    }
                }
                else if (state == 2)
                {
                    state = 0;
                    path = a;
                }
            }

            if (avg <= 0)
            {
                avg = 10;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("Path not found");
                return 1;
            }

            var recs = Directory.GetFiles(path, "*.rec").ToList();
            recs.Sort();
            double apples = 0.0;
            double durationCount = 0.0;
            var appleAvgCount = 0;
            int idx = 0;
            var totalCounts = new SortedDictionary<int, int>();

            while (idx < recs.Count)
            {
                var recPath = recs[idx++];
                using var fs = File.OpenRead(recPath);
                var rec = ElmaReplayIO.Replay.ParseFrom(fs, recPath);
                var numApples = rec[0].Events.Count(e => e.Type == EventType.AppleTake);
                var duration = rec[0].Header.FrameCount / 33.333;

                if (!totalCounts.ContainsKey(numApples))
                {
                    totalCounts.Add(numApples, 0);
                }

                totalCounts[numApples] += 1;

                durationCount += duration;
                apples += numApples;
                appleAvgCount += 1;
                if (appleAvgCount == avg)
                {
                    apples /= appleAvgCount;
                    durationCount /= appleAvgCount;

                    var fi = new FileInfo(recPath);
                    Console.WriteLine($"{idx:00000}    {fi.CreationTimeUtc:yyyy-MM-dd HH:mm:ss}    {apples}    {durationCount}");
                    apples = 0;
                    appleAvgCount = 0;
                    durationCount = 0;
                }
            }

            if (appleAvgCount > 0)
            {
                apples /= appleAvgCount;
                durationCount /= appleAvgCount;
                var fi = new FileInfo(recs.Last());
                Console.WriteLine($"{idx:00000}    {fi.CreationTimeUtc:yyyy-MM-dd HH:mm:ss}    {apples}    {durationCount}");
            }

            Console.WriteLine();
            var nRuns = recs.Count;
            foreach (var k in totalCounts.Keys)
            {
                var perc = (double)totalCounts[k] / nRuns * 100.0;
                Console.WriteLine($"{k}    {perc}");
            }


            return 0;
        }
    }
}