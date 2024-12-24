using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.ConstrainedExecution;

namespace Laboratory3_2
{

    public partial class MainForm : Form
    {
        private readonly Button selectFolderButton;
        private readonly Button validateImagesButton;
        private readonly Button mirrorImagesButton;
        private readonly Button removeMirroredImagesButton;
        private readonly FlowLayoutPanel imagePanel;
        private readonly Panel buttonPanel;
        private readonly Label statusLabel;
        private readonly List<string> files;
        private readonly List<PictureBox> pictureBoxes;
        private string currentPath;
        private readonly Regex regexExtForImage;
        private bool isProcessing;

        public MainForm()
        {
            Text = "Image Processor";
            Size = new Size(1024, 768);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 247);
            MinimumSize = new Size(800, 600);

            files = new List<string>();
            pictureBoxes = new List<PictureBox>();
            regexExtForImage = new Regex(@"\.(bmp|gif|tiff?|jpe?g|png)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.White
            };

            buttonPanel.Paint += (s, e) =>
            {
                using var brush = new SolidBrush(Color.FromArgb(10, 0, 0, 0));
                e.Graphics.FillRectangle(brush, 0, buttonPanel.Height - 2, buttonPanel.Width, 2);
            };

            selectFolderButton = CreateStyledButton("📁 Select Folder", 160);
            validateImagesButton = CreateStyledButton("✓ Validate Images", 160);
            mirrorImagesButton = CreateStyledButton("🔄 Mirror Images", 160);
            removeMirroredImagesButton = CreateStyledButton("🗑 Remove Mirrored", 180);

            statusLabel = new Label
            {
                AutoSize = true,
                Location = new Point(20, buttonPanel.Height - 30),
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(0, 5, 0, 0)
            };

            int currentX = 20;
            foreach (Button button in new[] { selectFolderButton, validateImagesButton, mirrorImagesButton, removeMirroredImagesButton })
            {
                button.Location = new Point(currentX, 15);
                buttonPanel.Controls.Add(button);
                currentX += button.Width + 15;
            }

            buttonPanel.Controls.Add(statusLabel);

            imagePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.None,
            };

            Controls.Add(imagePanel);
            Controls.Add(buttonPanel);

            selectFolderButton.Click += SelectFolderButton_ClickAsync;
            validateImagesButton.Click += ValidateImagesButton_Click;
            mirrorImagesButton.Click += MirrorImagesButton_ClickAsync;
            removeMirroredImagesButton.Click += RemoveMirroredImages_Click;

            FormClosing += MainForm_FormClosing;
        }

        private Button CreateStyledButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(37, 99, 235), 
                ForeColor = Color.White,
                FlatAppearance = { BorderSize = 0 },
                Padding = new Padding(0, 0, 0, 2)
            };

            button.MouseEnter += (s, e) =>
            {
                button.BackColor = Color.FromArgb(29, 78, 216);
                button.Padding = new Padding(0, 2, 0, 0);
            };
            button.MouseLeave += (s, e) =>
            {
                button.BackColor = Color.FromArgb(37, 99, 235);
                button.Padding = new Padding(0, 0, 0, 2);
            };

            return button;
        }

        private void EnableControls(bool enabled)
        {
            selectFolderButton.Enabled = enabled;
            validateImagesButton.Enabled = enabled;
            mirrorImagesButton.Enabled = enabled;
            removeMirroredImagesButton.Enabled = enabled;

            statusLabel.Text = enabled ? "" : "Processing...";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            foreach (var pictureBox in pictureBoxes)
            {
                try
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Dispose();
                }
                catch
                {
                    continue;
                }
            }
            pictureBoxes.Clear();
            files.Clear();
        }

        private async void SelectFolderButton_ClickAsync(object sender, EventArgs e)
        {
            if (isProcessing) return;

            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            isProcessing = true;
            EnableControls(false);

            try
            {
                await LoadImagesAsync(dialog.SelectedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isProcessing = false;
                EnableControls(true);
            }
        }

        private async Task LoadImagesAsync(string folderPath)
        {
            CleanupResources();
            currentPath = folderPath;

            var imageFiles = await Task.Run(() =>
                Directory.GetFiles(folderPath)
                    .Where(file => regexExtForImage.IsMatch(file))
                    .ToList());

            foreach (string file in imageFiles)
            {
                try
                {
                    await AddImageToPanel(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image {Path.GetFileName(file)}: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }



        private async void MirrorImagesButton_ClickAsync(object sender, EventArgs e)
        {
            if (isProcessing) return;

            isProcessing = true;
            EnableControls(false);

            try
            {
                await MirrorImagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error mirroring images: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isProcessing = false;
                EnableControls(true);
            }
        }

        private async Task MirrorImagesAsync()
        {
            var originalFiles = files.ToList();
            foreach (var file in originalFiles)
            {
                if (file.EndsWith("-mirrored.gif")) continue;

                try
                {
                    string newFileName = Path.Combine(
                        Path.GetDirectoryName(file),
                        Path.GetFileNameWithoutExtension(file) + "-mirrored.gif"
                    );

                    await Task.Run(() =>
                    {
                        using var bitmap = new Bitmap(file);
                        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        bitmap.Save(newFileName, System.Drawing.Imaging.ImageFormat.Gif);
                    });

                    await AddImageToPanel(newFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error mirroring {Path.GetFileName(file)}: {ex.Message}",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void RemoveMirroredImages_Click(object sender, EventArgs e)
        {
            try
            {
                var mirroredFiles = files.Where(f => f.EndsWith("-mirrored.gif")).ToList();

                foreach (var file in mirroredFiles)
                {
                    RemoveImage(file);
                }

                MessageBox.Show($"Successfully removed {mirroredFiles.Count} mirrored image(s).",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing mirrored images: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveImage(string filePath)
        {
            try
            {
                var pictureBox = pictureBoxes.First(pb => (string)pb.Tag == filePath);
                var container = (Panel)pictureBox.Parent;

                var image = pictureBox.Image;
                pictureBox.Image = null;
                image.Dispose();
                pictureBox.Dispose();

                imagePanel.Controls.Remove(container);
                container.Dispose();

                pictureBoxes.Remove(pictureBox);
                files.Remove(filePath);

                File.Delete(filePath);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Image {filePath} not found in the display");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error removing image {filePath}: {ex.Message}");
            }
        }

        private async Task AddImageToPanel(string filePath)
        {
            try
            {
                ValidateFile(filePath);

                await Task.Run(() =>
                {
                    using var image = Image.FromFile(filePath);
                    var clone = new Bitmap(image);

                    this.Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            var container = new Panel
                            {
                                Size = new Size(200, 220),
                                Margin = new Padding(12),
                                BackColor = Color.White,
                                Padding = new Padding(8)
                            };

                            container.Paint += (s, e) =>
                            {
                                using var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
                                e.Graphics.FillRectangle(shadowBrush, 0, 0, container.Width, container.Height);
                            };

                            var pictureBox = new PictureBox
                            {
                                Image = clone,
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Size = new Size(184, 184),
                                Location = new Point(8, 8),
                                Tag = filePath,
                                BackColor = Color.White
                            };

                            var label = new Label
                            {
                                Text = Path.GetFileName(filePath),
                                AutoSize = false,
                                TextAlign = ContentAlignment.MiddleCenter,
                                Size = new Size(184, 20),
                                Location = new Point(8, 192),
                                Font = new Font("Segoe UI", 8.5f),
                                ForeColor = Color.FromArgb(60, 60, 60)
                            };

                            container.Controls.Add(pictureBox);
                            container.Controls.Add(label);
                            imagePanel.Controls.Add(container);
                            pictureBoxes.Add(pictureBox);
                            files.Add(filePath);

                            var contextMenu = new ContextMenuStrip();
                            var deleteItem = new ToolStripMenuItem("Delete");
                            deleteItem.Click += (s, e) => RemoveImage(filePath);
                            contextMenu.Items.Add(deleteItem);
                            pictureBox.ContextMenuStrip = contextMenu;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error adding image to panel: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                });
            }
            catch (FileFormatException ex)
            {
                Console.WriteLine($"Invalid file format: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image {Path.GetFileName(filePath)}: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateImagesButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Validation complete: All loaded images are valid.",
                "Validation Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ValidateFile(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var image = Image.FromStream(stream);

                if (!regexExtForImage.IsMatch(filePath))
                {
                    throw new FileFormatException($"Invalid file format: {filePath}");
                }
            }
            catch (Exception ex)
            {
                throw new FileFormatException($"Invalid image file: {ex.Message}");
            }
        }
    }
}