using ImageCompressor.Main.Compressors;
using ImageCompressor.Main.Files;
using ImageCompressor.Main.Utilities;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TinifyAPI;

namespace ImageCompressor
{
    public partial class MainWindow : Window
    {
        private static readonly string API_KEY = "Yk8o9Lni6nYdv3vmc7j5cpAalBufDc_l";

        private TinifyCompressor compressor = new TinifyCompressor(API_KEY);

        private List<AppFile> filesToCompress = new List<AppFile>();

        private int compressCount = 0,
                    maxCompressCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_InputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog().Value)
            {
                TextBox_InputDirectory.Text = folderDialog.SelectedPath;

                TextBox_OutputDirectory.Text = folderDialog.SelectedPath + @"\compressed";

                DisplayFiles();
            }
        }

        private void Button_OutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog().Value)
            {
                TextBox_OutputDirectory.Text = folderDialog.SelectedPath;
            }
        }

        private void DisplayFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(TextBox_InputDirectory.Text);

            if (!directory.Exists)
                return;

            filesToCompress.Clear();

            directory.GetFiles()
                     .Where(n => n.Extension == ".jpg" || n.Extension == ".jpeg" || n.Extension == ".png")
                     .ToList()
                     .ForEach(file =>
                     {
                         filesToCompress.Add(new AppFile(file, TextBox_OutputDirectory.Text));
                     });

            ListBox_Items.ItemsSource = filesToCompress;

            ShowMessage(MessageDictionary.GetFoundMessage(filesToCompress.Count), MessageType.SUCCESS);
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
            if (TextBox_InputDirectory.Text.Length == 0 || !Directory.Exists(TextBox_InputDirectory.Text))
            {
                TextBox_InputDirectory.BorderBrush = new SolidColorBrush(Message.GetColorFromType(MessageType.ERROR));
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
                    TextBox_Width.BorderBrush = new SolidColorBrush(Message.GetColorFromType(MessageType.ERROR));
                    ShowMessage("Width must be a number.", MessageType.ERROR);
                    return;
                }

                if (Convert.ToInt32(TextBox_Width.Text) < 1 || Convert.ToInt32(TextBox_Width.Text) > 10000)
                {
                    TextBox_Width.BorderBrush = new SolidColorBrush(Message.GetColorFromType(MessageType.ERROR));
                    ShowMessage("Width must be between 1 and 10,000, inclusive.", MessageType.ERROR);
                    return;
                }

                if (!Utils.IsInt(TextBox_Height.Text))
                {
                    TextBox_Height.BorderBrush = new SolidColorBrush(Message.GetColorFromType(MessageType.ERROR));
                    ShowMessage("Height must be a number.", MessageType.ERROR);
                    return;
                }

                if (Convert.ToInt32(TextBox_Height.Text) < 1 || Convert.ToInt32(TextBox_Height.Text) > 10000)
                {
                    TextBox_Height.BorderBrush = new SolidColorBrush(Message.GetColorFromType(MessageType.ERROR));
                    ShowMessage("Height must be between 1 and 10,000, inclusive.", MessageType.ERROR);
                    return;
                }
            }

            maxCompressCount = filesToCompress.Count(file => file.Enabled);

            if (maxCompressCount == 0)
            {
                ShowMessage("There are no images to compress.", MessageType.ERROR);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Confirm action.", "Confirm...", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                PreExecution();

                try
                {
                    await AlterImagesAsync(CheckBox_Resize.IsChecked.Value);

                    PostExecution();
                }
                catch (AccountException ex)
                {
                    ShowMessage("Account Exception. Verify API key and account limit. " + ex.Message, MessageType.ERROR);
                }
                catch (ClientException ex)
                {
                    ShowMessage("Client Exception. There was a problem with the source image." + ex.Message, MessageType.ERROR);
                }
                catch (ServerException ex)
                {
                    ShowMessage("Server Exception. A server error occured." + ex.Message, MessageType.ERROR);
                }
                catch (ConnectionException ex)
                {
                    ShowMessage("Server Exception. A server error occured." + ex.Message, MessageType.ERROR);
                }
                catch (System.Exception ex)
                {
                    ShowMessage("Exception. An error occured." + ex.Message, MessageType.ERROR);
                }
            }
        }

        private void PreExecution()
        {
            compressCount = 1;

            Image_Preview.Source = null;

            ListBox_Items.IsEnabled = false;
        }

        private void PostExecution()
        {
            ListBox_Items.IsEnabled = true;
        }

        private async Task AlterImagesAsync(bool doResize)
        {
            List<Task> alterTasks = new List<Task>();

            int width = Convert.ToInt32(TextBox_Width.Text),
                height = Convert.ToInt32(TextBox_Height.Text);

            filesToCompress.ForEach(file =>
            {
                if (file.Enabled)
                {
                    if (doResize)
                    {
                        alterTasks.Add(ResizeImageAsync(file, width, height));
                    }
                    else
                    {
                        alterTasks.Add(CompressImageAsync(file));
                    }

                    Task.Delay(10);
                }
            });

            await Task.WhenAll(alterTasks);
        }

        private async Task ResizeImageAsync(AppFile file, int width, int height)
        {
            await compressor.ResizeAndCompressImageAsync(file.FullPath, file.DestinationPath, width, height);

            ShowExecutionMessage(file);
        }

        private async Task CompressImageAsync(AppFile file)
        {
            await compressor.CompressImageAsync(file.FullPath, file.DestinationPath);

            ShowExecutionMessage(file);
        }

        private void ShowExecutionMessage(AppFile file)
        {
            if (maxCompressCount == compressCount)
            {
                ShowMessage(MessageDictionary.GetCompletionMessage(maxCompressCount, compressCount), MessageType.SUCCESS);
            }
            else
            {
                ShowMessage(MessageDictionary.GetCompressedMessage(file.FileName, maxCompressCount, compressCount++), MessageType.DEFAULT);
            }
        }

        private void ListBox_Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uri uriSource = new Uri(filesToCompress[ListBox_Items.SelectedIndex].FullPath, UriKind.Absolute);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = uriSource;
            image.EndInit();

            Image_Preview.Source = image;
        }

        private void MenuItem_Enable_Click(object sender, RoutedEventArgs e)
        {
            SetFilesEnabled(true);
        }

        private void MenuItem_Disable_Click(object sender, RoutedEventArgs e)
        {
            SetFilesEnabled(false);
        }

        private void SetFilesEnabled(bool enabled)
        {
            foreach (AppFile file in ListBox_Items.SelectedItems)
            {
                file.Enabled = enabled;
                file.Foreground = enabled ? Brushes.Black : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            }

            ShowMessage(MessageDictionary.GetDisabledMessage(filesToCompress.Count, filesToCompress.Count(file => !file.Enabled)), MessageType.SUCCESS);
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            Label_Message.Content = message;

            Label_Message.Foreground = new SolidColorBrush(Message.GetColorFromType(messageType));
        }
    }
}
