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
        static BitmapSource bitmapSource;

        // Архив камер
        List<ArchiveClient> archiveCameras;
        ArchiveClient archiveClient;

        public MainWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            _ = GetCameraList();
        }

        private async Task GetCameraList()
        {
            try
            {
                cameras = await CAM_Show.GetChannelsInfo();
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
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
                        ConvertBitmapToBitmapSource(frame);
                        mainWindow.Cadr.Source = bitmapSource;
                });
            }
            catch (ObjectDisposedException e)
            {
                camera?.StopTranslation();
                ErrorMessage(e.Message);
            }
        }

        private static void ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
            }
        }

        private void ErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DisplayInfo()
        {
            ListBox archiveBox = (ListBox)FindName("archive");
            if (archiveBox != null)
                archiveBox.Items.Clear();
            int i = 1;
            foreach (var elem in archiveCameras)
            {
                archiveBox.Items.Add(
                    $"Архив {i++}, Начало: {elem.time_from}, Конец: {elem.time_to}"
                );
            }
        }

        private async void ChangeCamera(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox archivebox = (ListBox)FindName("archive");
                archivebox.Items.Clear();
            }
            catch (Exception ex)
            {
            }
            if (camera == null)
            {
                TextBox textBox = (TextBox)FindName("TextBox");
                textBox.Visibility = Visibility.Collapsed;
                ListBox archiveBox = (ListBox)FindName("archive");
                archiveBox.Visibility = Visibility.Visible;
            }
            ListBox elements = (ListBox)sender;
            int index = elements.SelectedIndex;
            camera = cameras[index];
            camera.ImageReady += ImageReady;
            camera.StartTranslation();
            archiveCameras = await CAM_Show.GetArchiveInfo(camera.Id);
            DisplayInfo();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
        
        // Тут надо будет выводить кадр с архива по нажатию
        private void archive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox archive = (ListBox)sender;
            int index = archive.SelectedIndex;
            DateTime? time_in = archiveCameras[index].time_from;
            camera.ImageReady += ImageReady;
            camera.StartTranslation(true, time_in);
        }
    }
}
