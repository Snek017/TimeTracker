using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeTracker
{
    public class MainForm : Form
    {
        private FlowLayoutPanel _appGrid = new FlowLayoutPanel();
        private AppTracker _tracker;
        private NotifyIcon _trayIcon = new NotifyIcon();
        private TabControl _tabControl = new TabControl();
        private System.Windows.Forms.Timer _processCheckTimer = new System.Windows.Forms.Timer();
        private CheckBox _startWithWindowsCheckBox = new CheckBox();

        private const string ServerUrl = "http://45.133.9.62:3000/upload";

        public MainForm(AppTracker tracker)
        {
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            InitializeForm();
        }

        public MainForm() : this(new AppTracker())
        {
        }

        private async void InitializeForm()
        {
            Console.WriteLine("Initializing MainForm...");
            _tracker.LoadData("data.json");
            InitializeUI();
            InitializeTrayIcon();
            StartProcessMonitoring();
            RefreshAppGrid();

            // Upload JSON to server on start
            await UploadJsonToServerAsync();
        }

        private void InitializeUI()
        {
            this.Text = "TimeTracker";
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.ForeColor = Color.White;
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            var addButton = new Button
            {
                Text = "Add App",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => AddNewApp();

            _startWithWindowsCheckBox.Text = "Start with Windows";
            _startWithWindowsCheckBox.Dock = DockStyle.Top;
            _startWithWindowsCheckBox.ForeColor = Color.White;
            _startWithWindowsCheckBox.Checked = IsStartWithWindowsEnabled();
            _startWithWindowsCheckBox.CheckedChanged += (s, e) =>
            {
                SetStartWithWindows(_startWithWindowsCheckBox.Checked);
            };

            _tabControl.Dock = DockStyle.Fill;
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.DrawItem += TabControl_DrawItem;
            _tabControl.MouseDown += TabControl_MouseDown;

            _appGrid.Dock = DockStyle.Fill;
            _appGrid.AutoScroll = true;

            var mainTab = new TabPage("Apps") { BackColor = Color.FromArgb(24, 24, 24) };
            mainTab.Controls.Add(_appGrid);

            _tabControl.TabPages.Add(mainTab);

            this.Controls.Add(_tabControl);
            this.Controls.Add(_startWithWindowsCheckBox);
            this.Controls.Add(addButton);
        }

        private void InitializeTrayIcon()
        {
            _trayIcon.Icon = SystemIcons.Application;
            _trayIcon.Visible = true;
            _trayIcon.Text = "TimeTracker";

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => this.Show());
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            _trayIcon.ContextMenuStrip = contextMenu;

            this.Resize += (sender, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Hide();
                }
            };
        }

        private void AddNewApp()
        {
            var addAppForm = new AddAppForm();
            if (addAppForm.ShowDialog() == DialogResult.OK && addAppForm.NewApp != null)
            {
                _tracker.AddApp(addAppForm.NewApp);
                _tracker.SaveData("data.json");
                RefreshAppGrid();
            }
        }

        private bool IsStartWithWindowsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue("TimeTracker") != null;
        }

        private void SetStartWithWindows(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (enable)
            {
                key?.SetValue("TimeTracker", $"\"{Application.ExecutablePath}\" /minimized");
            }
            else
            {
                key?.DeleteValue("TimeTracker", false);
            }
        }

        private void StartProcessMonitoring()
        {
            _processCheckTimer.Interval = 1000;
            _processCheckTimer.Tick += (s, e) =>
            {
                foreach (var app in _tracker.Apps)
                {
                    var isRunning = Process.GetProcesses().Any(p =>
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(app.Path, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    });

                    if (isRunning)
                    {
                        app.TotalTime += TimeSpan.FromSeconds(1);
                        app.IsCurrentlyRunning = true;
                    }
                    else if (app.IsCurrentlyRunning)
                    {
                        app.IsCurrentlyRunning = false;
                    }
                }
                _tracker.SaveData("data.json");
            };
            _processCheckTimer.Start();
        }

        private async Task UploadJsonToServerAsync()
        {
            try
            {
                if (!File.Exists("data.json"))
                {
                    Console.WriteLine("JSON file not found. Skipping upload.");
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync("data.json");
                using var client = new HttpClient();
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(ServerUrl, new MultipartFormDataContent
                {
                    { content, "content", "data.json" }
                });

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully uploaded JSON to server.");
                }
                else
                {
                    Console.WriteLine($"Failed to upload JSON. Server responded with: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading JSON to server: {ex.Message}");
            }
        }

        private void RefreshAppGrid()
        {
            _appGrid.Controls.Clear();

            foreach (var app in _tracker.Apps)
            {
                var appPanel = new Panel
                {
                    Width = 120,
                    Height = 160,
                    Margin = new Padding(10),
                    BackColor = Color.FromArgb(40, 40, 40)
                };

                var icon = new PictureBox
                {
                    Width = 100,
                    Height = 100,
                    Dock = DockStyle.Top,
                    BackColor = Color.Black,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                if (File.Exists(app.IconPath))
                {
                    try
                    {
                        icon.Image = Image.FromFile(app.IconPath);
                    }
                    catch
                    {
                        MessageBox.Show($"Error loading icon for {app.Name}.", "Error");
                    }
                }

                var label = new Label
                {
                    Text = app.Name,
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White
                };

                icon.Click += (s, e) => ShowAppStatsInTab(app);

                appPanel.Controls.Add(icon);
                appPanel.Controls.Add(label);
                _appGrid.Controls.Add(appPanel);
            }
        }

        private void ShowAppStatsInTab(AppData app)
        {
            var statsTab = new TabPage(app.Name)
            {
                BackColor = Color.FromArgb(24, 24, 24)
            };

            var statsLabel = new Label
            {
                Text = $"App: {app.Name}\nTotal Time: {app.TotalTime}\nLaunch Count: {app.LaunchCount}",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            statsTab.Controls.Add(statsLabel);
            _tabControl.TabPages.Add(statsTab);
            _tabControl.SelectedTab = statsTab;
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = _tabControl.TabPages[e.Index];
            var tabRect = _tabControl.GetTabRect(e.Index);

            e.Graphics.FillRectangle(Brushes.Black, tabRect);

            var textColor = e.Index == _tabControl.SelectedIndex ? Brushes.White : Brushes.Gray;
            e.Graphics.DrawString(tabPage.Text, new Font("Segoe UI", 10), textColor, tabRect.X + 5, tabRect.Y + 5);
        }

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (var i = 0; i < _tabControl.TabPages.Count; i++)
            {
                var tabRect = _tabControl.GetTabRect(i);
                if (tabRect.Contains(e.Location) && i != 0)
                {
                    _tabControl.TabPages.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
