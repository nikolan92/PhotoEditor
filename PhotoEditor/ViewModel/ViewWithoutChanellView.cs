using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.HistoryProvider.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ViewModel
{
    public class ViewWithoutChanellView : ViewControl
    {
        public ViewWithoutChanellView(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageViewModel(image);

            ImageBasicInfo = new ImageBasicInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
        }
        public override void InvertFilter()
        {
            histortHelper.AddToHistory(new InvertFilterCommand(), MainImage.Image);
        }

        public override void Undo()
        {
            histortHelper.Undo(MainImage.Image);
        }

        public override void Redo()
        {
            histortHelper.Redo(MainImage.Image);
        }
    }
}
