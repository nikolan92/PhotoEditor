using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;

namespace PhotoEditor.ViewModel
{
    public abstract class ViewControl
    {
        protected HistoryHelper histortHelper;
        protected ImageViewModel mainImage;
        public ImageViewModel MainImage
        {
            get { return mainImage; }
            set { mainImage = value; }
        }
        protected ImageViewModel backUpImage;
        public ImageViewModel BackUpImage
        {
            get { return backUpImage; }
            set { backUpImage = value; }
        }
        protected ImageBasicInfoModel imageBasicInfo;
        public ImageBasicInfoModel ImageBasicInfo
        {
            get { return imageBasicInfo; }
            set { imageBasicInfo = value; }
        }
        public void MakeBackUpImage()
        {
            backUpImage = new ImageViewModel();
            backUpImage.Image = mainImage.Image;
            //mainImage.Image = backUpImage.Image;
        }
        public void RestoreFromBackUpImage()
        {
            mainImage.Image = backUpImage.Image;
        }
        public void PreviewGamma(double gamma)
        {
            mainImage.Image = GammaFilter.GammaFilterUnsafeWithCopy(backUpImage.Image, gamma);
        }
        public abstract void InvertFilter();
        public abstract void Undo();
        public abstract void Redo();
    }
}
