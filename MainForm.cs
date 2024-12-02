using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TimeTracker
{
    public class MainForm : Form
    {
        private FlowLayoutPanel _appGrid = new FlowLayoutPanel();
        private AppTracker _tracker = new AppTracker();
        private NotifyIcon _trayIcon = new NotifyIcon();
        private TabControl _tabControl = new TabControl();
        private System.Windows.Forms.Timer _processCheckTimer = new System.Windows.Forms.Timer();
        private CheckBox _startWithWindowsCheckBox = new CheckBox();

        public MainForm()
        {
            try
            {
                Console.WriteLine("Initializing MainForm...");
                _tracker.LoadData("data.txt");
                InitializeUI();
                InitializeTrayIcon();
                StartProcessMonitoring();
                RefreshAppGrid();
                Console.WriteLine("MainForm initialized successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void InitializeUI()
        {
            this.Text = "TimeTracker";
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.ForeColor = Color.White;
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Add App Button
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
            addButton.MouseEnter += (s, e) => addButton.BackColor = Color.FromArgb(50, 50, 50);
            addButton.MouseLeave += (s, e) => addButton.BackColor = Color.FromArgb(30, 30, 30);
            addButton.Click += (s, e) => AddNewApp();

            // Start with Windows Checkbox
            _startWithWindowsCheckBox.Text = "Start with Windows";
            _startWithWindowsCheckBox.Dock = DockStyle.Top;
            _startWithWindowsCheckBox.ForeColor = Color.White;
            _startWithWindowsCheckBox.BackColor = Color.FromArgb(18, 18, 18);
            _startWithWindowsCheckBox.Checked = IsStartWithWindowsEnabled();
            _startWithWindowsCheckBox.CheckedChanged += (s, e) =>
            {
                SetStartWithWindows(_startWithWindowsCheckBox.Checked);
            };

            // Tab Control
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.Padding = new Point(20, 4);
            _tabControl.DrawItem += TabControl_DrawItem;
            _tabControl.MouseDown += TabControl_MouseDown;

            // App Grid
            _appGrid.Dock = DockStyle.Fill;
            _appGrid.AutoScroll = true;
            _appGrid.Padding = new Padding(10);
            _appGrid.BackColor = Color.FromArgb(24, 24, 24);

            // Main Tab
            var mainTab = new TabPage("Apps")
            {
                BackColor = Color.FromArgb(24, 24, 24)
            };
            mainTab.Controls.Add(_appGrid);

            _tabControl.TabPages.Add(mainTab);

            this.Controls.Add(_tabControl);
            this.Controls.Add(_startWithWindowsCheckBox);
            this.Controls.Add(addButton);
        }

        private void InitializeTrayIcon()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize TrayIcon: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewApp()
        {
            var addAppForm = new AddAppForm();
            if (addAppForm.ShowDialog() == DialogResult.OK && addAppForm.NewApp != null)
            {
                _tracker.AddApp(addAppForm.NewApp);
                _tracker.SaveData("data.txt");
                RefreshAppGrid();
            }
        }

        private bool IsStartWithWindowsEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("TimeTracker") != null;
            }
            catch
            {
                return false;
            }
        }

        private void SetStartWithWindows(bool enable)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update Windows startup settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartProcessMonitoring()
        {
            _processCheckTimer.Interval = 1000; // Überprüfung jede Sekunde
            _processCheckTimer.Tick += (s, e) =>
            {
                try
                {
                    var runningProcesses = Process.GetProcesses();

                    foreach (var app in _tracker.Apps)
                    {
                        bool isRunning = false;

                        foreach (var process in runningProcesses)
                        {
                            try
                            {
                                if (process.MainModule?.FileName != null &&
                                    string.Equals(process.MainModule.FileName, app.Path, StringComparison.OrdinalIgnoreCase))
                                {
                                    isRunning = true;

                                    if (!app.IsCurrentlyRunning)
                                    {
                                        app.LaunchCount++;
                                        app.IsCurrentlyRunning = true;
                                        Console.WriteLine($"App started: {app.Name}");
                                    }

                                    break;
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (isRunning)
                        {
                            app.TotalTime += TimeSpan.FromSeconds(1);
                        }
                        else
                        {
                            if (app.IsCurrentlyRunning)
                            {
                                Console.WriteLine($"App stopped: {app.Name}");
                            }
                            app.IsCurrentlyRunning = false;
                        }
                    }

                    _tracker.SaveData("data.txt");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in process monitoring: {ex.Message}");
                }
            };
            _processCheckTimer.Start();
        }

        private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tabPage = _tabControl.TabPages[e.Index];
            var tabRect = _tabControl.GetTabRect(e.Index);

            e.Graphics.FillRectangle(Brushes.Black, tabRect);

            var textColor = e.Index == _tabControl.SelectedIndex ? Brushes.White : Brushes.Gray;
            e.Graphics.DrawString(tabPage.Text, new Font("Segoe UI", 10, FontStyle.Regular), textColor, tabRect.X + 5, tabRect.Y + 5);

            var closeButtonRect = new Rectangle(tabRect.Right - 20, tabRect.Top + (tabRect.Height - 15) / 2, 15, 15);
            e.Graphics.FillRectangle(Brushes.DarkGray, closeButtonRect);
            e.Graphics.DrawString("X", new Font("Segoe UI", 8, FontStyle.Bold), Brushes.Red, closeButtonRect.Left + 3, closeButtonRect.Top + 1);

            e.Graphics.DrawRectangle(Pens.White, closeButtonRect);
        }

        private void TabControl_MouseDown(object? sender, MouseEventArgs e)
        {
            for (var i = 0; i < _tabControl.TabPages.Count; i++)
            {
                var tabRect = _tabControl.GetTabRect(i);
                var closeButtonRect = new Rectangle(tabRect.Right - 20, tabRect.Top + (tabRect.Height - 15) / 2, 15, 15);

                if (closeButtonRect.Contains(e.Location) && i != 0) // Tab schließen außer Haupttab
                {
                    _tabControl.TabPages.RemoveAt(i);
                    break;
                }
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

                if (File.Exists(app.IconPath) && app.IconPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        icon.Image = Image.FromFile(app.IconPath);
                    }
                    catch
                    {
                        MessageBox.Show($"Error loading icon for {app.Name}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Icon for {app.Name} must be a PNG file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                var label = new Label
                {
                    Text = app.Name,
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Regular)
                };

                icon.Click += (s, e) => ShowAppStatsInTab(app);

                appPanel.Controls.Add(label);
                appPanel.Controls.Add(icon);
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
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            statsTab.Controls.Add(statsLabel);
            _tabControl.TabPages.Add(statsTab);
            _tabControl.SelectedTab = statsTab;
        }
    }
}
