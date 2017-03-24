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
    public class ViewWithoutChanellView : IUserControl
    {
        private HistoryHelper histortHelper;
        public ViewWithoutChanellView(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageViewModel(image);

            ImageBasicInfo = new ImageBasicInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
        }
        private ImageViewModel mainImage;
        public ImageViewModel MainImage
        {
            get { return mainImage; }
            set { mainImage = value; }
        }
        private ImageBasicInfoModel imageBasicInfo;

        public ImageBasicInfoModel ImageBasicInfo
        {
            get { return imageBasicInfo; }
            set { imageBasicInfo = value; }
        }
        public void InvertFilter()
        {
            histortHelper.AddToHistory(new InvertFilterCommand(), MainImage.Image);
        }

        public void Undo()
        {
            histortHelper.Undo(MainImage.Image);
        }

        public void Redo()
        {
            histortHelper.Redo(MainImage.Image);
        }
        public WriteableBitmap GetMainImage()
        {
            return MainImage.Image;
        }
    }
}
