using PhotoEditor.ImageOperations;
using System.Windows.Media.Imaging;

namespace PhotoEditor.DataModel
{
    public class MainViewModel
    {
        public MainViewModel(WriteableBitmap image)
        {
            MainImage = new ImageViewModel(image);

            //Making three copies of loaded image (resize needed)
            int width = (int)((double)MainImage.Image.PixelWidth * 0.3);
            int height = (int)((double)MainImage.Image.PixelWidth * 0.3);
            RedChanellImage = new ImageViewModel(ReseizeImage.LinearReseizeImageUnsafe(MainImage.Image, width, height));
            GreenChanellImage = new ImageViewModel(RedChanellImage.Image.Clone());
            BlueChanellImage = new ImageViewModel(RedChanellImage.Image.Clone());

        }
        private ImageViewModel mainImage;
        public ImageViewModel MainImage
        {
            get { return mainImage; }
            set { mainImage = value; }
        }
        private ImageViewModel redChanellImage;

        public ImageViewModel RedChanellImage
        {
            get { return redChanellImage; }
            set { redChanellImage = value; }
        }
        private ImageViewModel greenChanellImage;

        public ImageViewModel GreenChanellImage
        {
            get { return greenChanellImage; }
            set { greenChanellImage = value; }
        }
        private ImageViewModel blueChanellImage;

        public ImageViewModel BlueChanellImage
        {
            get { return blueChanellImage; }
            set { blueChanellImage = value; }
        }
    }
}
