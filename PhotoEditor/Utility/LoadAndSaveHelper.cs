using Microsoft.Win32;
using PhotoEditor.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
namespace PhotoEditor.Utility
{
    public class LoadAndSaveHelper
    {
        private string lastUsedPath;
        private string lastUsedFileName;
        public string LastUsedPath
        {
            get { return lastUsedPath; }
            private set { lastUsedPath = value; }
        }
        public string LastUsedFileName
        {
            get { return lastUsedFileName; }
            private set { lastUsedFileName = value; }
        }

        public LoadAndSaveHelper()
        {
            //Default open path is MyPicture
            LastUsedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public WriteableBitmap LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files(*.bmp *.jpeg *.png *.nn)|*.bmp; *.png; *.jpeg; *.jpg; *.nn";
            openFileDialog.InitialDirectory = LastUsedPath;
            if (openFileDialog.ShowDialog().Equals(true))
            {
                //Change lastUsedPath to the new one
                LastUsedPath = Path.GetDirectoryName(openFileDialog.FileName);
                LastUsedFileName = Path.GetFileName(openFileDialog.FileName);

                if (openFileDialog.FilterIndex == 5)//Custom Image
                {
                    return null;
                }
                else
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(openFileDialog.FileName);
                    bitmapImage.EndInit();
                    return new WriteableBitmap(bitmapImage);
                }
                
            }
            return null;
        }
        public void SaveImage(WriteableBitmap imageForSaving)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "BMP (*.bmp)|*.bmp|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png";

            saveFileDialog.InitialDirectory = LastUsedPath;
            
            if (saveFileDialog.ShowDialog().Equals(true))
            {
                //Change lastUsedPath to the new one
                LastUsedPath = Path.GetDirectoryName(saveFileDialog.FileName);

                string path = Path.GetDirectoryName(saveFileDialog.FileName);
                string fileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                string finalFilePath = path +"\\"+ fileName;

                BitmapEncoder encoder;
                switch (saveFileDialog.FilterIndex)
                {
                    case 1: //bmp
                        encoder = new BmpBitmapEncoder();
                        finalFilePath += ".bmp";
                        break;
                    case 2: // jpeg
                        encoder = new JpegBitmapEncoder();
                        finalFilePath += ".jpeg";
                        break;
                    case 3: // png
                        encoder = new PngBitmapEncoder();
                        finalFilePath += ".jpeg";
                        break;
                    default:
                        encoder = new JpegBitmapEncoder();
                        break;
                }

                using (FileStream fs = new FileStream(finalFilePath, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(imageForSaving));
                    encoder.Save(fs);
                }
            }
        }

        private byte[] LoadImageAsByteArray(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }
        public void SaveImageAsByteArray(byte[] image)
        {
            
        }
    }
}
