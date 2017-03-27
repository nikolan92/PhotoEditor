using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ViewModel
{
    public class ViewWithoutChanell : ViewLogic
    {
        public ViewWithoutChanell(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageModel(image);

            ImageInfo = new ImageInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
        }

        public override void Undo()
        {
            histortHelper.Undo(MainImage);
        }

        public override void Redo()
        {
            histortHelper.Redo(MainImage);
        }

        public override void AddImageReference(ImageModel imageModel)
        {
            histortHelper.Archive(imageModel);
        }

        public override void RefreshView()
        {

        }
    }
}
