using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using System.IO;
using ImageCompressor.Main;

namespace ImageCompressor
{
    public partial class MainWindow : Window
    {
        private List<FileInfo> filesToCompress;

        public MainWindow()
        {
            InitializeComponent();

            filesToCompress = new List<FileInfo>();
        }

        private void Button_InputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            var result = folderDialog.ShowDialog();

            if (result == true)
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

            Label_Message.Content = "Found " + filesToCompress.Count + (filesToCompress.Count == 1 ? " image" : " images.");
        }

        private void CheckBox_Resize_CheckedChanged(object sender, RoutedEventArgs e)
        {
            List<Control> relatedControls = new List<Control>()
            {
                TextBox_Width,
                TextBox_Height
            };

            relatedControls.ForEach(control => control.IsEnabled = CheckBox_Resize.IsChecked.Value);
        }

        private async void Button_Compress_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(TextBox_InputDirectory.Text))
            {
                ShowMessage("Input directory does not exist.");
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
                    ShowMessage("Width must be a number.");
                    return;
                }

                if (Convert.ToInt32(TextBox_Width.Text) < 1 || Convert.ToInt32(TextBox_Width.Text) > 10000)
                {
                    ShowMessage("Width must be between 1 and 10,000, inclusive.");
                    return;
                }

                if (!Utils.IsInt(TextBox_Height.Text))
                {
                    ShowMessage("Height must be a number.");
                    return;
                }

                if (Convert.ToInt32(TextBox_Height.Text) < 1 || Convert.ToInt32(TextBox_Height.Text) > 10000)
                {
                    ShowMessage("Height must be between 1 and 10,000, inclusive.");
                    return;
                }
            }

            MessageBoxResult result = MessageBox.Show("Confirm action.", "Confirm...", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
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

                resizeTasks.Add(Compressor.Instance.ResizeImage(fileName, destination, options));
            });

            await Task.WhenAll(resizeTasks);
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

                compressTasks.Add(Compressor.Instance.CompressImage(fileName, destination));
            });

            await Task.WhenAll(compressTasks);
        }

        private void ShowMessage(string message)
        {
            Label_Message.Content = message;
        }
    }
}
