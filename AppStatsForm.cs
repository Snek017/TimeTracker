using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeTracker
{
    public class AppStatsForm : Form
    {
        public AppStatsForm(AppData app)
        {
            InitializeUI(app);
        }

        private void InitializeUI(AppData app)
        {
            this.Text = $"Statistics for {app.Name}";
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark Theme
            this.ForeColor = Color.White;
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title Label
            var titleLabel = new Label
            {
                Text = $"App: {app.Name}",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 50,
                ForeColor = Color.White
            };

            // Total Time Label
            var totalTimeLabel = new Label
            {
                Text = $"Total Time Used: {app.TotalTime}",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 40,
                ForeColor = Color.White
            };

            // Launch Count Label
            var launchCountLabel = new Label
            {
                Text = $"Launch Count: {app.LaunchCount}",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 40,
                ForeColor = Color.White
            };

            // Close Button
            var closeButton = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, e) => this.Close();

            // Layout Panel
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                AutoScroll = true
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(totalTimeLabel);
            panel.Controls.Add(launchCountLabel);
            this.Controls.Add(panel);
            this.Controls.Add(closeButton);
        }
    }
}
