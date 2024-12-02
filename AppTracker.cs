using System;
using System.Collections.Generic;
using System.IO;

namespace TimeTracker
{
    public class AppTracker
    {
        public List<AppData> Apps { get; private set; } = new List<AppData>();

        /// <summary>
        /// Lädt die App-Daten aus der angegebenen Datei.
        /// Wenn die Datei nicht existiert, wird sie erstellt.
        /// </summary>
        public void LoadData(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Data file not found. Creating a new one.");
                    File.WriteAllText(filePath, string.Empty);
                }
                else
                {
                    Console.WriteLine($"Loading data from {filePath}...");
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Apps.Add(AppData.FromString(line));
                        }
                    }
                    Console.WriteLine($"Loaded {Apps.Count} apps.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Speichert die App-Daten in der angegebenen Datei.
        /// </summary>
        public void SaveData(string filePath)
        {
            try
            {
                Console.WriteLine($"Saving data to {filePath}...");
                File.WriteAllLines(filePath, Apps.ConvertAll(app => app.ToString()));
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fügt eine neue App zur Überwachungsliste hinzu.
        /// </summary>
        public void AddApp(AppData app)
        {
            try
            {
                if (app == null)
                {
                    throw new ArgumentNullException(nameof(app), "App cannot be null.");
                }

                Console.WriteLine($"Adding app: {app.Name}");
                Apps.Add(app);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding app: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Aktualisiert die Nutzungsdaten einer bestehenden App.
        /// </summary>
        public void UpdateApp(string appName, TimeSpan sessionTime)
        {
            try
            {
                var app = Apps.Find(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
                if (app != null)
                {
                    Console.WriteLine($"Updating app: {app.Name}");
                    app.TotalTime += sessionTime;
                    app.LaunchCount++;
                }
                else
                {
                    Console.WriteLine($"App not found: {appName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating app: {ex.Message}");
                throw;
            }
        }
    }
}
