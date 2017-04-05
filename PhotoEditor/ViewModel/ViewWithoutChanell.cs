using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ViewModel
{
    public class ViewWithoutChanell : ViewLogic
    {
        public ViewWithoutChanell(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            HistortHelper = histortHelper;
            MainImage = new ImageModel(image);

            ImageInfo = new ImageInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
        }

        public override void Undo()
        {
            HistortHelper.Undo(MainImage);
        }

        public override void Redo()
        {
            HistortHelper.Redo(MainImage);
        }

        public override void RefreshView()
        {

        }
    }
}
