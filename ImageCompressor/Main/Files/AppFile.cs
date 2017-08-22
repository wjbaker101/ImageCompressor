using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace ImageCompressor.Main.Files
{
    public class AppFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; private set; }
        public string FullPath { get; private set; }
        public bool Enabled { get; set; }
        public long FileSize { get; private set; }
        public string DestinationPath { get; private set; }

        private SolidColorBrush _foreground { get; set; }

        public AppFile(FileInfo file, string destination)
        {
            this.FileName = file.Name;
            this.FullPath = file.FullName;
            this.Enabled = true;
            this.FileSize = file.Length;
            this.DestinationPath = destination + @"\" + FileName;

            this.Foreground = Brushes.Black;
        }

        public SolidColorBrush Foreground
        {
            get
            {
                return this._foreground;
            }

            set
            {
                this._foreground = value;

                this.OnPropertyChanged("Foreground");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
