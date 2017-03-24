using System.Windows.Media.Imaging;

namespace PhotoEditor.DataModel
{
    public class ImageViewModel : ObservableObject
    {
        public ImageViewModel(WriteableBitmap bitmapImage)
        {
            Image = bitmapImage;
        }
        private WriteableBitmap image;
        public WriteableBitmap Image
        {
            get { return image; }
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }
    }
}
