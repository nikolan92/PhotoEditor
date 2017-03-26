using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.DataModel
{
    public class ImageInfoModel : ObservableObject
    {
        public ImageInfoModel(string fileName,string width,string height,string bitDepth)
        {
            FileName = fileName;
            Resolution = width + " x " + height;
            BitDepth = bitDepth + "-bit color";
        }
        private string resolution;
        public string Resolution
        {
            get { return resolution; }
            set
            {
                resolution = value;
                OnPropertyChanged("Resolution");
            }
        }
        private string bitDepth;
        public string BitDepth
        {
            get { return bitDepth; }
            set
            {
                bitDepth = value;
                OnPropertyChanged("BitDepth");
            }
        }
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

    }
}
