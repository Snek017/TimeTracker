using System;

namespace TimeTracker
{
    public class AppData
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        public int LaunchCount { get; set; } = 0;
        public bool IsCurrentlyRunning { get; set; } = false; // Neues Feld für Zustand

        public override string ToString()
        {
            return $"{Name}|{Path}|{IconPath}|{TotalTime}|{LaunchCount}|{IsCurrentlyRunning}";
        }

        public static AppData FromString(string data)
        {
            var parts = data.Split('|');
            return new AppData
            {
                Name = parts[0],
                Path = parts[1],
                IconPath = parts[2],
                TotalTime = TimeSpan.Parse(parts[3]),
                LaunchCount = int.Parse(parts[4]),
                IsCurrentlyRunning = parts.Length > 5 && bool.TryParse(parts[5], out var running) && running
            };
        }
    }
}
