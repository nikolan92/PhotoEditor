using Microsoft.Win32;
using PhotoEditor.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.Utility
{
    public class LoadAndSaveHelper
    {
        private string lastUsedPath;
        private MainWindow mainWindow;
        public string LastUsedPath
        {
            get { return lastUsedPath; }
            private set { lastUsedPath = value; }
        }

        public LoadAndSaveHelper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            //Default open path is MyPicture
            LastUsedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public WriteableBitmap LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png *.jpeg ...)|*.png; *.jpeg; *.jpg; |All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                //Change lastUsedPath to the new one
                LastUsedPath = Path.GetDirectoryName(openFileDialog.FileName);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(openFileDialog.FileName);
                bitmapImage.EndInit();

                return new WriteableBitmap(bitmapImage);
            }
            return null;
        }
        public void SaveImage(WriteableBitmap image)
        {

        }
    }
}
