using System;

namespace TimeTracker
{
   public class AppData
{
    public required string Name { get; set; }  // Required ensures it must be set
    public TimeSpan TotalTime { get; set; }
    public int LaunchCount { get; set; }
    public string? Path { get; set; }  // Nullable
    public string? IconPath { get; set; }  // Nullable

    public bool IsCurrentlyRunning { get; set; }  // New property to fix errors in MainForm.cs

    // Parse a line from the TXT file into an AppData object
    public static AppData FromString(string line)
    {
        try
        {
            var parts = line.Split(':');
            if (parts.Length < 2) throw new FormatException("Invalid line format.");

            return new AppData
            {
                Name = parts[0].Trim(),
                TotalTime = TimeSpan.Parse(parts[1].Trim()),
                LaunchCount = parts.Length > 2 && int.TryParse(parts[2].Trim(), out var count) ? count : 0,
                IsCurrentlyRunning = false  // Default value
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing line: {line}. {ex.Message}");
            return null;
        }
    }
}
}