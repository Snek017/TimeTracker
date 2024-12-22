using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // Hinzugefügt für Application

namespace TimeTracker
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            // AppTracker initialisieren
            var tracker = new AppTracker();

            // JSON-Dateipfade definieren
            string jsonFilePath = "data.json";  // Pfad zur JSON-Datei
            string serverUrl = "http://45.133.9.62:3000/upload";  // Server-URL mit IP-Adresse

            // Daten laden
            tracker.LoadData(jsonFilePath);

            // JSON an den Server hochladen
            await tracker.UploadJsonToServer(jsonFilePath, serverUrl);

            // App starten
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(tracker));
        }
    }
}
