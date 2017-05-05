using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using AForge.Imaging.Filters;
using System.Windows.Interop;

namespace ImageInfoViewer
{
    class ResultParameter
    {
        public List<BindingImage> Images;
        public string ElapsedTime;
        public ResultParameter()
        {
            Images = new List<BindingImage>();
            ElapsedTime = null;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker, backgroundWorker2;
        private BindingList<BindingImage> ImageList;
        private Microsoft.Win32.OpenFileDialog ofd;
        public int ImagesCount = 0;
        private List<string> paths;
        private string currentFileName;
        private UpdateProgressBarDelegate updProgress;
        private double progressValue;
        private object previousItem;
        private bool isPressed;
        private int[,] LoG = new int[5, 5] {{ 0,  0, -1,  0,  0},
                                            { 0, -1, -2, -1,  0},
                                            {-1, -2, 16, -2, -1},
                                            { 0, -1, -2, -1,  0},
                                            { 0,  0, -1,  0,  0}};

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            backgroundWorker2 = ((BackgroundWorker)this.FindResource("backgroundWorker2"));
            ImageList = new BindingList<BindingImage>();
            ofd = new Microsoft.Win32.OpenFileDialog();
            this.ImageGrid.ItemsSource = ImageList;
            paths = new List<string>();
            updProgress = new UpdateProgressBarDelegate(progress.SetValue);
            r2.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "All Supported Image Files|*.jpg;*.jpeg;*.jfif;*.tif;*.tiff;*.dib;*.rle;*.bmp;*.png;*.ico;*.gif;*.pcx;*.exif";
            ofd.ShowReadOnly = true;
            ofd.Multiselect = true;
            ofd.Title = "Выберите изображения";
            if (ofd.ShowDialog() == true)
                backgroundWorker.RunWorkerAsync(ofd);
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog backOfd = (Microsoft.Win32.OpenFileDialog)e.Argument;
            ResultParameter rp = new ResultParameter();
            int count = 0;
            double step = 100.0 / (double)backOfd.FileNames.Length;
            FileStream SourceStream = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (string FileName in backOfd.FileNames)
            {
                try
                {
                    paths.Add(FileName);
                    count++;
                    backgroundWorker.ReportProgress((int)(count * step), ofd.SafeFileNames[count - 1]);
                    SourceStream = File.Open(FileName, FileMode.Open);
                    if (ofd.SafeFileNames[count - 1].Split('.')[1] == "pcx")
                        rp.Images.Add(new BindingImage(++ImagesCount, backOfd.FileNames[count - 1], SourceStream));
                    else
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(SourceStream, false, false);
                        rp.Images.Add(new BindingImage(++ImagesCount, backOfd.FileNames[count - 1], img));
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (SourceStream != null)
                        SourceStream.Close();
                }
            }

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            rp.ElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            e.Result = rp;
        }

        private void WorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            ResultParameter rp = (ResultParameter)e.Result;
            foreach (BindingImage img in rp.Images)
                ImageList.Add(img);
            int TotalImages = ofd.FileNames.Length;
            int Errors = TotalImages - rp.Images.Count;
            StateBar.Value = 0;
            ProcessLabel.Text = "Complete! " + TotalImages + " new files loaded with " + Errors + " errors. " + "RunTime: " + rp.ElapsedTime;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessLabel.Text = "Processing " + (string)e.UserState;
            if (e.ProgressPercentage > 100)
                StateBar.Value = 100;
            else
                StateBar.Value = e.ProgressPercentage;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ImageList.Clear();
            ImagesCount = 0;
            ProcessLabel.Text = "Complete!";
            paths.Clear();
            if (image1.Source != null)
                image1.Source = null;
            canvas.Children.Clear();
        }

        private void DoWork2(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                open.IsEnabled = false;
                Clear_Button.IsEnabled = false;
                equalization.IsEnabled = false;
                grid.IsEnabled = false;
                smooth.IsEnabled = false;
            }));
            Bitmap bm = (Bitmap)e.Argument;
            int[] hist = GetHistogram(bm);
            ShowHistogram(hist);
            backgroundWorker2.ReportProgress(0, hist);
            Dispatcher.Invoke(new Action(() =>
            {
                open.IsEnabled = true;
                Clear_Button.IsEnabled = true;
                equalization.IsEnabled = true;
                grid.IsEnabled = true;
                smooth.IsEnabled = true;
            }));
        }

        private void WorkDone2(object sender, RunWorkerCompletedEventArgs e)
        {
            progressValue = 0;
            progress.Value = 0;
        }

        private int[] GetHistogram(Bitmap image)
        {
            int[] values = new int[257];
            int pixelsCount = image.Height * image.Width;
            values[256] = pixelsCount;
            int br;
            double step = 100.0 / image.Width;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    br = Convert.ToInt16(image.GetPixel(i, j).GetBrightness() * 100);
                    values[br]++;
                }
                Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, progressValue += step });
            }

            bool? isChecked = null;
            smooth.Dispatcher.Invoke(new Action(() => isChecked = smooth.IsChecked));
            if (isChecked == true)
                return SmoothHistogram(values);
            return values;
        }

        private void ShowHistogram(int[] values)
        {
            canvas.Dispatcher.Invoke(new Action(() => canvas.Children.Clear()));
            double columnHeight;
            int pixelsCount = values[256];
            Line line = null;
            for (int i = 0; i < 256; i++)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    line = new Line();
                    line.Stroke = System.Windows.Media.Brushes.Black;
                    line.Fill = System.Windows.Media.Brushes.Black;
                    line.StrokeThickness = 0.9;
                    line.X1 = i * 2;
                    line.Y1 = canvas.Height;
                    line.X2 = i * 2;
                    columnHeight = values[i] * canvas.Height / pixelsCount * 10;
                    line.Y2 = (canvas.Height > columnHeight ? canvas.Height - columnHeight : 0);
                }));

                canvas.Dispatcher.Invoke(new Action(() => canvas.Children.Add(line)));
            }
        }

        private int[] SmoothHistogram(int[] originalValues)
        {
            int[] smoothedValues = new int[originalValues.Length];
            smoothedValues[256] = originalValues[256];
            double[] mask = new double[] { 0.25, 0.5, 0.25 };
            for (int bin = 1; bin < originalValues.Length - 2; bin++)
            {
                double smoothedValue = 0;
                for (int i = 0; i < mask.Length; i++)
                    smoothedValue += originalValues[bin - 1 + i] * mask[i];
                smoothedValues[bin] = (int)smoothedValue;
            }
            return smoothedValues;
        }

        private void sharpness()
        {
            if (image1.Source != null)
            {
                Bitmap bm = new Bitmap(currentFileName);
                BitmapSource bms;
                Convolution filter;
                int[,] matrix = new int[3, 3];

                if (r2.IsChecked == true)
                {
                    matrix[0, 0] = Convert.ToInt16(t00.Text);
                    matrix[0, 1] = Convert.ToInt16(t01.Text);
                    matrix[0, 2] = Convert.ToInt16(t02.Text);
                    matrix[1, 0] = Convert.ToInt16(t10.Text);
                    matrix[1, 1] = Convert.ToInt16(t11.Text);
                    matrix[1, 2] = Convert.ToInt16(t12.Text);
                    matrix[2, 0] = Convert.ToInt16(t20.Text);
                    matrix[2, 1] = Convert.ToInt16(t21.Text);
                    matrix[2, 2] = Convert.ToInt16(t22.Text);

                    filter = new Convolution(matrix);
                    //bm = sharpen(bm, matrix, 3);
                    bm = filter.Apply(bm);
                }
                else
                {
                    filter = new Convolution(LoG);
                    //bm = sharpen(bm, LoG, 5);
                    bm = filter.Apply(bm);
                }

                bms = BitmapToBitmapSource(bm);
                image1.Source = bms;
                backgroundWorker2.RunWorkerAsync(bm);
            }
        }

        private Bitmap sharpen(Bitmap image, int[,] filter, int size)
        {
            Bitmap sharpenImage = new Bitmap(image.Width, image.Height);
            int w = image.Width, h = image.Height;

            double factor = 1.0, bias = 0.0;

            System.Drawing.Color[,] result = new System.Drawing.Color[image.Width, image.Height];

            for (int x = 0; x < w; ++x)
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < size; filterX++)
                    {
                        for (int filterY = 0; filterY < size; filterY++)
                        {
                            int imageX = (x - size / 2 + filterX + w) % w;
                            int imageY = (y - size / 2 + filterY + h) % h;

                            System.Drawing.Color imageColor = image.GetPixel(imageX, imageY);

                            red += imageColor.R * filter[filterX, filterY];
                            green += imageColor.G * filter[filterX, filterY];
                            blue += imageColor.B * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255),
                            g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255),
                            b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = System.Drawing.Color.FromArgb(r, g, b);
                    }
                }

            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                    sharpenImage.SetPixel(i, j, result[i, j]);

            return sharpenImage;
        }

        private void clickRow(object sender, MouseButtonEventArgs e)
        {
            isPressed = false;
            if (ImageGrid.SelectedItem != null && ImageGrid.SelectedItem != previousItem && !backgroundWorker2.IsBusy)
            {
                previousItem = ImageGrid.SelectedItem;
                int index = ImageGrid.SelectedIndex;
                string format = ImageList[index].Format;

                if (format == "Bmp" || format == "Jpg" || format == "Jpeg" || format == "Tiff" || format == "Png" || format == "Ico")
                {
                    Bitmap bm = new Bitmap(ImageList[index].Name);
                    BitmapImage bmi = new BitmapImage(new Uri(ImageList[index].Name));
                    image1.BeginInit();
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    image1.Source = bmi;
                    image1.EndInit();
                    currentFileName = ImageList[index].Name;
                    backgroundWorker2.RunWorkerAsync(bm);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (isPressed == false)
            {
                isPressed = true;
                previousItem = null;
                if (image1.Source != null)
                {
                    HistogramEqualization filter = new HistogramEqualization();
                    Bitmap bm = new Bitmap(currentFileName);
                    bm = filter.Apply(bm);
                    BitmapSource bms = BitmapToBitmapSource(bm);
                    image1.Source = bms;
                    backgroundWorker2.RunWorkerAsync(bm);
                }
            }
        }

        private BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            BitmapSource bms = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bms;
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private void click(object sender, RoutedEventArgs e)
        {
            previousItem = null;
            isPressed = false;
        }

        private void sharp_Click(object sender, RoutedEventArgs e)
        {
            grid.IsEnabled = false;
            sharpness();
            grid.IsEnabled = true;
        }

        private void r1_Checked(object sender, RoutedEventArgs e)
        {
            matr.IsEnabled = false;
        }

        private void r2_Checked(object sender, RoutedEventArgs e)
        {
            matr.IsEnabled = true;
        }

    }
}