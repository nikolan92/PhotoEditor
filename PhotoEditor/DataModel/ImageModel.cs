using System.Windows.Media.Imaging;

namespace PhotoEditor.DataModel
{
    public class ImageModel : ObservableObject
    {
        public ImageModel()
        {
            Image = null;
        }
        public ImageModel(WriteableBitmap bitmapImage)
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
