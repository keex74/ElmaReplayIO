/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    using System.IO;

    internal static class LevTools
    {
        /// <summary>
        /// Get an elma level associated with the given ride if it can be found.
        /// </summary>
        /// <param name="ride">The ride to find a level for.</param>
        /// <param name="recPath">The full path to the replay file.</param>
        /// <returns>The level or null if none were found.</returns>
        public static ElmaLevel? FindLevel(ReplayHeader header, string recPath)
        {
            try
            {
                var dirname = Path.GetDirectoryName(recPath);
                if (dirname == null)
                {
                    return null;
                }

                var levelDir = Path.Combine(dirname, "..", "lev");
                var opts = new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                };
                var levs = Directory.GetFiles(levelDir, header.LevelName, opts);

                foreach (var l in levs)
                {
                    using var fs = File.OpenRead(l);
                    var level = ElmaLevel.ParseFrom(fs);
                    if (level.Link == header.Link)
                    {
                        return level;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load level", ex);
            }
        }
    }
}