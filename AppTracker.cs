using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeTracker
{
    public class AppTracker
    {
        private static readonly HttpClient client = new HttpClient();
        public List<AppData> Apps { get; private set; } = new List<AppData>();

        public void LoadData(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    Apps = JsonConvert.DeserializeObject<List<AppData>>(json) ?? new List<AppData>();
                    Console.WriteLine("Data loaded successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        public void SaveData(string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(Apps, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        public void AddApp(AppData app)
        {
            if (app != null)
            {
                Apps.Add(app);
                Console.WriteLine($"App added: {app.Name}");
            }
        }

        public void ConvertTxtToJson(string txtFilePath, string jsonFilePath)
        {
            try
            {
                if (!File.Exists(txtFilePath))
                {
                    Console.WriteLine($"TXT file not found: {txtFilePath}");
                    return;
                }

                var apps = new List<AppData>();
                var lines = File.ReadAllLines(txtFilePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var app = AppData.FromString(line);
                        if (app != null) apps.Add(app);
                    }
                }

                var jsonData = JsonConvert.SerializeObject(apps, Formatting.Indented);
                File.WriteAllText(jsonFilePath, jsonData);
                Console.WriteLine($"Converted {txtFilePath} to {jsonFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting TXT to JSON: {ex.Message}");
            }
        }

        public async Task UploadJsonToServer(string jsonFilePath, string serverUrl)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    Console.WriteLine($"JSON file not found: {jsonFilePath}");
                    return;
                }

                var jsonData = File.ReadAllText(jsonFilePath);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                Console.WriteLine($"Uploading {jsonFilePath} to {serverUrl}...");
                var response = await client.PostAsync(serverUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Upload successful!");
                }
                else
                {
                    Console.WriteLine($"Upload failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading JSON: {ex.Message}");
            }
        }

        public async Task ProcessActivityData(string txtFilePath, string jsonFilePath, string serverUrl)
        {
            try
            {
                ConvertTxtToJson(txtFilePath, jsonFilePath);
                await UploadJsonToServer(jsonFilePath, serverUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing activity data: {ex.Message}");
            }
        }
    }
}
