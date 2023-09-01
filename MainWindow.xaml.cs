using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;   
using System.Windows.Interop;


namespace VideoCaptureApplication
{
    public delegate void ImageReadyHandler(Bitmap image);

    public partial class MainWindow : Window
    {
        private CAM_Show? camera;
        private List<CAM_Show>? cameras;
        private Bitmap? frame;

        public MainWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            _ = GetCameraList();
        }

        private async Task GetCameraList()
        {
            while (true)
            {
                try
                {
                    cameras = await CAM_Show.GetChannelsInfo();
                    break;
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
            }
            ListBox elements = (ListBox)FindName("elements");
            foreach (var cam in cameras)
            {
                elements.Items.Add(cam.Name);
            }
        }

        private void ImageReady(Bitmap image)
        {
            frame = image;
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                    if (mainWindow != null)
                        mainWindow.Cadr.Source = ConvertBitmapToBitmapSource(frame);
                });
            }
            catch (ObjectDisposedException e)
            {
                camera?.StopTranslation();
                ErrorMessage(e.Message);
            }
        }
        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }

        private void ErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ChangeCamera(object sender, SelectionChangedEventArgs e)
        {
            if (camera == null)
            {
                TextBox textBox = (TextBox)FindName("TextBox");
                textBox.Visibility = Visibility.Collapsed;
            }
            ListBox elements = (ListBox)sender;
            int index = elements.SelectedIndex;
            camera = cameras[index];
            camera.ImageReady += ImageReady;
            camera.StartTranslation();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
