using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private static int TileSize = 35;
        private static int GridSize = 10;
        private int ImageSize = TileSize * GridSize;
        private Bitmap sourceImage;
        private Bitmap destinationImage;
        private PictureBox sourcePictureBox;
        private PictureBox destinationPictureBox;
        private Rectangle selectionRect;
        private Image originalImage;
        private List<Bitmap> layerImages;

        public Form1()
        {
            InitializeComponent();
            sourceImage = new Bitmap(ImageSize, ImageSize);
            destinationImage = new Bitmap(ImageSize, ImageSize);

            sourcePictureBox = new PictureBox
            {
                Size = new Size(ImageSize, ImageSize),
                BorderStyle = BorderStyle.FixedSingle,
                Image = sourceImage,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Panel sourcePanel = new Panel
            {
                Location = new Point(10, 286),
                Size = new Size(300, 300),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                AutoScrollMinSize = new Size(0, 256)
            };
            sourcePanel.Controls.Add(sourcePictureBox);

            sourcePictureBox.MouseDown += SourcePictureBox_MouseDown;
            sourcePictureBox.MouseMove += SourcePictureBox_MouseMove;
            sourcePictureBox.Paint += SourcePictureBox_Paint;

            Controls.Add(sourcePanel);

            destinationPictureBox = new PictureBox
            {
                Location = new Point(350, 30),
                Size = new Size(ImageSize, ImageSize),
                BorderStyle = BorderStyle.FixedSingle,
                Image = destinationImage
            };
            destinationPictureBox.MouseDown += DestinationPictureBox_MouseDown;
            destinationPictureBox.Paint += DestinationPictureBox_Paint;
            Controls.Add(destinationPictureBox);

            layerImages = new List<Bitmap>();
            layerImages.Add(new Bitmap(ImageSize, ImageSize));
            layerImages.Add(new Bitmap(ImageSize, ImageSize));
            layerImages.Add(new Bitmap(ImageSize, ImageSize));

            checkedListBox1.Items.Add("Слой 1");
            checkedListBox1.Items.Add("Слой 2");
            checkedListBox1.Items.Add("Слой 3");

            checkedListBox1.SetItemChecked(0, true);
        }

        private void SourcePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            int tileX = e.X / TileSize;
            int tileY = e.Y / TileSize;
            selectionRect = new Rectangle(tileX * TileSize, tileY * TileSize, TileSize, TileSize);
            sourcePictureBox.Invalidate();
        }

        private void SourcePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int tileX = e.X / TileSize;
                int tileY = e.Y / TileSize;
                selectionRect = new Rectangle(tileX * TileSize, tileY * TileSize, TileSize, TileSize);
                sourcePictureBox.Invalidate();
            }
        }

        private void SourcePictureBox_Paint(object sender, PaintEventArgs e)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    Rectangle tileRect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                    e.Graphics.DrawRectangle(Pens.Black, tileRect);
                }
            }

            if (!selectionRect.IsEmpty)
            {
                e.Graphics.DrawRectangle(Pens.Red, selectionRect);
            }
        }

        private void DestinationPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            int tileX = e.X / TileSize;
            int tileY = e.Y / TileSize;

            if (e.Button == MouseButtons.Right)
            {
                foreach (int selectedIndex in checkedListBox1.CheckedIndices)
                {
                    using (Graphics g = Graphics.FromImage(layerImages[selectedIndex]))
                    {
                        Rectangle destRect = new Rectangle(tileX * TileSize, tileY * TileSize, TileSize, TileSize);
                        g.FillRectangle(Brushes.White, destRect);
                    }
                }

                destinationPictureBox.Invalidate();
            }
            else
            {
                if (selectionRect != Rectangle.Empty && destinationPictureBox.Image != null)
                {
                    foreach (int selectedIndex in checkedListBox1.CheckedIndices)
                    {
                        using (Graphics g = Graphics.FromImage(layerImages[selectedIndex]))
                        {
                            int sourceX = selectionRect.X / TileSize;
                            int sourceY = selectionRect.Y / TileSize;
                            Rectangle sourceRect = new Rectangle(sourceX * TileSize, sourceY * TileSize, TileSize, TileSize);
                            Rectangle destRect = new Rectangle(tileX * TileSize, tileY * TileSize, TileSize, TileSize);
                            g.DrawImage(sourceImage, destRect, sourceRect, GraphicsUnit.Pixel);
                        }
                    }
                    destinationPictureBox.Invalidate();
                }
            }
        }

        private void DestinationPictureBox_Paint(object sender, PaintEventArgs e)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    Rectangle tileRect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                    e.Graphics.DrawRectangle(Pens.Black, tileRect);
                }
            }

            for (int i = 0; i < layerImages.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    Image layerImage = layerImages[i];

                    float scaleFactor = i > 0 ? 0.9f - (i * 0.2f) : 1.0f;

                    int scaledSize = (int)(TileSize * scaleFactor);
                    int offset = (TileSize - scaledSize) / 2;

                    for (int x = 0; x < GridSize; x++)
                    {
                        for (int y = 0; y < GridSize; y++)
                        {
                            Rectangle sourceRect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                            Rectangle destRect = new Rectangle(x * TileSize + offset, y * TileSize + offset, scaledSize, scaledSize);
                            e.Graphics.DrawImage(layerImage, destRect, sourceRect, GraphicsUnit.Pixel);
                        }
                    }
                }
            }
        }

        private void LoadImageFromFile(string filePath)
        {
            try
            {
                string imageName = Path.GetFileName(filePath);
                string targetPath = Path.Combine(Application.StartupPath, "image", imageName);

                File.Copy(filePath, targetPath, true);

                listBox1.Items.Add(targetPath);

                sourceImage = new Bitmap(filePath);
                sourcePictureBox.Image = sourceImage;

                Image loadedImage = Image.FromFile(filePath);
                sourceImage = new Bitmap(loadedImage, ImageSize, ImageSize);
                sourcePictureBox.Image = sourceImage;

                destinationImage = new Bitmap(TileSize * GridSize, TileSize * GridSize);
                Graphics graphics = Graphics.FromImage(destinationImage);

                graphics.DrawImage(destinationPictureBox.Image, 0, 0);

                graphics.Dispose();

                destinationPictureBox.Image = destinationImage;
                destinationPictureBox.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadImageFromCsv(string filename)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    // Очищаем все изображения слоев перед загрузкой новых данных
                    for (int i = 0; i < layerImages.Count; i++)
                    {
                        layerImages[i] = new Bitmap(TileSize * GridSize, TileSize * GridSize);
                    }

                    destinationImage = new Bitmap(TileSize * GridSize, TileSize * GridSize);
                    Graphics graphics = Graphics.FromImage(destinationImage);
                    graphics.Clear(Color.White);
                    destinationPictureBox.Image = destinationImage;
                    destinationPictureBox.Invalidate();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(',');

                        if (values.Length == 8)
                        {
                            int layerIndex = int.Parse(values[0]);
                            int gridX = int.Parse(values[1]);
                            int gridY = int.Parse(values[2]);
                            int tilePixelX = int.Parse(values[3]);
                            int tilePixelY = int.Parse(values[4]);
                            int red = int.Parse(values[5]);
                            int green = int.Parse(values[6]);
                            int blue = int.Parse(values[7]);

                            if (layerIndex >= 0 && layerIndex < layerImages.Count)
                            {
                                int startX = gridX * TileSize + tilePixelX;
                                int startY = gridY * TileSize + tilePixelY;

                                layerImages[layerIndex].SetPixel(startX, startY, Color.FromArgb(red, green, blue));
                            }
                        }
                    }

                    destinationPictureBox.Invalidate();
                    MessageBox.Show("Загрузка завершена.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);

            if (image != null)
            {
                using (Graphics g = Graphics.FromImage(resizedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, width, height);
                }
            }
            return resizedImage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            destinationImage = new Bitmap(ImageSize, ImageSize);
            destinationPictureBox.Image = destinationImage;

            string[] imageFiles = Directory.GetFiles(Path.Combine(Application.StartupPath, "image"));

            foreach (string filePath in imageFiles)
            {
                string imageName = Path.GetFileName(filePath);
                listBox1.Items.Add(imageName);
            }

            listBox1.Update();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;)|*.png;*.jpg;*.jpeg;|CSV файлы (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string extension = Path.GetExtension(openFileDialog.FileName);
                if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    LoadImageFromCsv(openFileDialog.FileName);
                }
                else
                {
                    LoadImageFromFile(openFileDialog.FileName);
                }
            }
            originalImage = sourceImage;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csvWriter.WriteField($"{GridSize},{TileSize}");
                        csvWriter.NextRecord();

                        for (int y = 0; y < GridSize; y++)
                        {
                            for (int x = 0; x < GridSize; x++)
                            {
                                int tileX = x * TileSize;
                                int tileY = y * TileSize;

                                List<int> layerIndices = new List<int>();
                                List<int> tilePixelXValues = new List<int>();
                                List<int> tilePixelYValues = new List<int>();
                                List<int> redValues = new List<int>();
                                List<int> greenValues = new List<int>();
                                List<int> blueValues = new List<int>();

                                for (int i = 0; i < layerImages.Count; i++)
                                {
                                    if (checkedListBox1.GetItemChecked(i))
                                    {
                                        for (int tilePixelY = 0; tilePixelY < TileSize; tilePixelY++)
                                        {
                                            for (int tilePixelX = 0; tilePixelX < TileSize; tilePixelX++)
                                            {
                                                int pixelX = tileX + tilePixelX;
                                                int pixelY = tileY + tilePixelY;

                                                Color pixelColor = layerImages[i].GetPixel(pixelX, pixelY);
                                                int red = pixelColor.R;
                                                int green = pixelColor.G;
                                                int blue = pixelColor.B;

                                                if (red == 0 && green == 0 && blue == 0)
                                                {
                                                    red = 255;
                                                    green = 255;
                                                    blue = 255;
                                                }

                                                layerIndices.Add(i);
                                                tilePixelXValues.Add(tilePixelX);
                                                tilePixelYValues.Add(tilePixelY);
                                                redValues.Add(red);
                                                greenValues.Add(green);
                                                blueValues.Add(blue);
                                            }
                                        }
                                    }
                                }

                                if (layerIndices.Count > 0)
                                {
                                    for (int i = 0; i < layerIndices.Count; i++)
                                    {
                                        csvWriter.WriteField(layerIndices[i]);
                                        csvWriter.WriteField(x);
                                        csvWriter.WriteField(y);
                                        csvWriter.WriteField(tilePixelXValues[i]);
                                        csvWriter.WriteField(tilePixelYValues[i]);
                                        csvWriter.WriteField(redValues[i]);
                                        csvWriter.WriteField(greenValues[i]);
                                        csvWriter.WriteField(blueValues[i]);
                                        csvWriter.NextRecord();
                                    }
                                }
                                else
                                {
                                    csvWriter.WriteField(-1);
                                    csvWriter.WriteField(x);
                                    csvWriter.WriteField(y);
                                    csvWriter.WriteField(0);
                                    csvWriter.WriteField(0);
                                    csvWriter.WriteField(255);
                                    csvWriter.WriteField(255);
                                    csvWriter.WriteField(255);
                                    csvWriter.NextRecord();
                                }
                            }
                        }
                    }
                    MessageBox.Show("Сохранение завершено.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetLayerIndexForTile(int pixelX, int pixelY)
        {
            for (int i = 0; i < layerImages.Count; i++)
            {
                int tileImageSize = TileSize * GridSize;
                if (pixelX >= 0 && pixelX < tileImageSize && pixelY >= 0 && pixelY < tileImageSize)
                {
                    return i;
                }
                tileImageSize += TileSpacing;
            }
            return -1;
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            destinationImage = new Bitmap(ImageSize, ImageSize);
            Graphics graphics = Graphics.FromImage(destinationImage);
            graphics.Clear(Color.White);
            destinationPictureBox.Image = destinationImage;
            destinationPictureBox.Invalidate();

            layerImages.Clear();
        }

        private void editSizeGridAndTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 childForm = new Form2();

            if (childForm.ShowDialog() == DialogResult.OK)
            {
                int newGridSize = childForm.SizeGrid;
                int newTileSize = childForm.SizeTile;
                int newSizeImage = newGridSize * newTileSize;

                GridSize = newGridSize;
                TileSize = newTileSize;
                ImageSize = newSizeImage;

                int maxTileSize = Math.Min(TileSize, newTileSize);
                for (int i = 0; i < layerImages.Count; i++)
                {
                    Bitmap currentLayerImage = layerImages[i];
                    Bitmap resizedLayerImage = new Bitmap(maxTileSize * newGridSize, maxTileSize * newGridSize);
                    using (Graphics g = Graphics.FromImage(resizedLayerImage))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(currentLayerImage, 0, 0, maxTileSize * newGridSize, maxTileSize * newGridSize);
                    }
                    layerImages[i] = resizedLayerImage;
                }

                ResizeTileImages();
                sourcePictureBox.Invalidate();
                destinationPictureBox.Invalidate();
            }
        }

        private void ResizeTileImages()
        {
            sourceImage = ResizeImage(originalImage, TileSize * GridSize, TileSize * GridSize);
            sourcePictureBox.Image = sourceImage;

            destinationImage = new Bitmap(TileSize * GridSize, TileSize * GridSize);
            destinationPictureBox.Image = destinationImage;
            sourcePictureBox.Size = new Size(TileSize * GridSize, TileSize * GridSize);
            destinationPictureBox.Size = new Size(TileSize * GridSize, TileSize * GridSize);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedFileName = listBox1.SelectedItem.ToString();
                string imagePath = Path.Combine(Application.StartupPath, "image", selectedFileName);

                if (File.Exists(imagePath))
                {
                    using (FileStream stream = new FileStream(imagePath, FileMode.Open))
                    {
                        Image selectedImage = Image.FromStream(stream);

                        originalImage = new Bitmap(selectedImage, ImageSize, ImageSize);
                        sourceImage = ResizeImage(originalImage, TileSize * GridSize, TileSize * GridSize);
                        sourcePictureBox.Image = sourceImage;
                    }
                }
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            destinationPictureBox.Invalidate();
        }

        private void saveLayersToDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;

                for (int i = 0; i < layerImages.Count; i++)
                {
                    string fileName = Path.Combine(folderPath, $"Layer{i + 1}.bin");

                    using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        Bitmap layerImage = layerImages[i];

                        for (int x = 0; x < layerImage.Width; x++)
                        {
                            for (int y = 0; y < layerImage.Height; y++)
                            {
                                Color pixelColor = layerImage.GetPixel(x, y);

                                binaryWriter.Write(pixelColor.R);
                                binaryWriter.Write(pixelColor.G);
                                binaryWriter.Write(pixelColor.B);
                            }
                        }
                    }
                }

                MessageBox.Show("Сохранение слоёв.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}