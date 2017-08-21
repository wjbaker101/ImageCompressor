using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using System.IO;
using ImageCompressor.Main;
using System.Windows.Media.Imaging;
using ImageCompressor.Main.Utilities;
using System.Windows.Media;

namespace ImageCompressor
{
    public partial class MainWindow : Window
    {
        private List<FileInfo> filesToCompress;

        private int compressCount = 0;

        private long sizeBefore = 0,
                     sizeAfter = 0;

        public MainWindow()
        {
            InitializeComponent();

            filesToCompress = new List<FileInfo>();
        }

        private void Button_InputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog().Value)
            {
                TextBox_InputDirectory.Text = folderDialog.SelectedPath;

                if (TextBox_OutputDirectory.Text.Length == 0)
                {
                    TextBox_OutputDirectory.Text = folderDialog.SelectedPath + @"\compressed";
                }

                DisplayFiles();
            }
        }

        private void Button_OutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            var result = folderDialog.ShowDialog();

            if (result == true)
            {
                TextBox_OutputDirectory.Text = folderDialog.SelectedPath;
            }
        }

        private void DisplayFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(TextBox_InputDirectory.Text);

            if (!directory.Exists)
                return;

            filesToCompress = directory.GetFiles().Where(n => n.Extension == ".jpg" || n.Extension == ".jpeg" || n.Extension == ".png").ToList();

            ListBox_Items.ItemsSource = filesToCompress;

            ShowMessage("Found " + filesToCompress.Count + (filesToCompress.Count == 1 ? " image" : " images."), MessageType.SUCCESS);
        }

        private void CheckBox_Resize_CheckedChanged(object sender, RoutedEventArgs e)
        {
            List<Control> relatedControls = new List<Control>()
            {
                TextBox_Width,
                TextBox_Height
            };

            relatedControls.ForEach(control => control.IsEnabled = CheckBox_Resize.IsChecked.Value);

            if (Button_Compress == null)
                return;

            if (CheckBox_Resize.IsChecked.Value)
                Button_Compress.Content = "Compress and Resize";
            else
                Button_Compress.Content = "Compress";
        }

        private async void Button_Compress_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(TextBox_InputDirectory.Text))
            {
                ShowMessage("Input directory does not exist.", MessageType.ERROR);
                return;
            }

            DirectoryInfo directory = new DirectoryInfo(TextBox_OutputDirectory.Text);

            if (!directory.Exists)
            {
                directory.Create();
            }

            if (CheckBox_Resize.IsChecked.Value)
            {
                if (!Utils.IsInt(TextBox_Width.Text))
                {
                    ShowMessage("Width must be a number.", MessageType.ERROR);
                    return;
                }

                if (Convert.ToInt32(TextBox_Width.Text) < 1 || Convert.ToInt32(TextBox_Width.Text) > 10000)
                {
                    ShowMessage("Width must be between 1 and 10,000, inclusive.", MessageType.ERROR);
                    return;
                }

                if (!Utils.IsInt(TextBox_Height.Text))
                {
                    ShowMessage("Height must be a number.", MessageType.ERROR);
                    return;
                }

                if (Convert.ToInt32(TextBox_Height.Text) < 1 || Convert.ToInt32(TextBox_Height.Text) > 10000)
                {
                    ShowMessage("Height must be between 1 and 10,000, inclusive.", MessageType.ERROR);
                    return;
                }
            }

            MessageBoxResult result = MessageBox.Show("Confirm action.", "Confirm...", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                compressCount = 0;

                sizeBefore = 0;
                filesToCompress.ForEach(file => sizeBefore += file.Length);

                Image_Preview.Source = null;

                if (CheckBox_Resize.IsChecked.Value)
                {
                    await ResizeImages();
                }
                else
                {
                    await CompressImages();
                }
            }
        }

        private async Task ResizeImages()
        {
            var resizeTasks = new List<Task>();

            string fileName = "",
                   destination = "";

            dynamic options = new
            {
                method = "cover",
                width = Convert.ToInt32(TextBox_Width.Text),
                height = Convert.ToInt32(TextBox_Height.Text)
            };

            filesToCompress.ForEach(file =>
            {
                fileName = file.FullName;
                destination = TextBox_OutputDirectory.Text + @"\" + file.Name;

                resizeTasks.Add(ResizeImage(fileName, destination, options));
            });

            await Task.WhenAll(resizeTasks);
        }

        private async Task ResizeImage(string fileLocation, string destinationFile, dynamic options)
        {
            await Compressor.Instance.ResizeImageAsync(fileLocation, destinationFile, options);

            ShowMessage("Resized " + ++compressCount + (filesToCompress.Count == 1 ? " image" : " images."), MessageType.DEFAULT);
        }

        private async Task CompressImages()
        {
            var compressTasks = new List<Task>();

            string fileName = "",
                   destination = "";

            filesToCompress.ForEach(file =>
            {
                fileName = file.FullName;
                destination = TextBox_OutputDirectory.Text + @"\" + file.Name;

                compressTasks.Add(CompressImage(fileName, destination));
            });

            await Task.WhenAll(compressTasks);
        }

        private async Task CompressImage(string fileLocation, string destinationFile)
        {
            await Compressor.Instance.CompressImageAsync(fileLocation, destinationFile);

            ShowMessage("Compressed " + ++compressCount + (filesToCompress.Count == 1 ? " image" : " images."), MessageType.DEFAULT);
        }

        private void ListBox_Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uri uriSource = new Uri(filesToCompress[ListBox_Items.SelectedIndex].FullName, UriKind.Absolute);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = uriSource;
            image.EndInit();

            Image_Preview.Source = image;
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            Label_Message.Content = message;

            Label_Message.Foreground = new SolidColorBrush(Message.GetColorFromType(messageType));
        }
    }
}
