using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker; // Sicherstellen, dass der richtige Namensraum für AppData verwendet wird

namespace TimeTracker
{
    public class AddAppForm : Form
    {
        public AppData? NewApp { get; private set; }

        public AddAppForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Add New App";
            this.BackColor = Color.FromArgb(30, 30, 30); // Sehr dunkles Theme
            this.ForeColor = Color.White;
            this.Size = new Size(350, 250);

            var nameLabel = new Label { Text = "App Name:", Dock = DockStyle.Top, ForeColor = Color.White };
            var nameBox = new TextBox { Dock = DockStyle.Top };

            var pathLabel = new Label { Text = "App File Path:", Dock = DockStyle.Top, ForeColor = Color.White };
            var pathBox = new TextBox { Dock = DockStyle.Top };

            var iconLabel = new Label { Text = "Icon File Path:", Dock = DockStyle.Top, ForeColor = Color.White };
            var iconBox = new TextBox { Dock = DockStyle.Top };

            var saveButton = new Button
            {
                Text = "Save",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text) ||
                    string.IsNullOrWhiteSpace(pathBox.Text) ||
                    string.IsNullOrWhiteSpace(iconBox.Text))
                {
                    MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                NewApp = new AppData
                {
                    Name = nameBox.Text,
                    Path = pathBox.Text,
                    IconPath = iconBox.Text,
                    TotalTime = TimeSpan.Zero,
                    LaunchCount = 0
                };
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(saveButton);
            this.Controls.Add(iconBox);
            this.Controls.Add(iconLabel);
            this.Controls.Add(pathBox);
            this.Controls.Add(pathLabel);
            this.Controls.Add(nameBox);
            this.Controls.Add(nameLabel);
        }
    }
}
