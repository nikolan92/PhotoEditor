using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;

namespace PhotoEditor.ViewModel
{
    public abstract class ViewLogic
    {
        protected HistoryHelper histortHelper;
        protected ImageModel mainImage;
        public ImageModel MainImage
        {
            get { return mainImage; }
            set { mainImage = value; }
        }
        protected ImageInfoModel imageInfo;
        public ImageInfoModel ImageInfo
        {
            get { return imageInfo; }
            set { imageInfo = value; }
        }
        public abstract void AddImageReference(ImageModel imageModel);
        public abstract void RefreshView();
        public abstract void Undo();
        public abstract void Redo();
    }
}


/// <summary>
/// Before use this function make sure you first call MakeBackUpImage() to save reference of the MainImage.<para />
/// This function will use BackUpImage as a source image to apply gamma and create new image, that new image will be shown as a MainImage.<para />
/// After using this function make sure you call RestoreFromBackUpImage() to restore reference from BackUpImage to the MainImage.
/// </summary>
//public void PreviewGamma(double gamma)
//{
//    mainImage.Image = GammaFilter.GammaFilterUnsafeWithCopy(backUpImage.Image, gamma);
//}
/// <summary>
/// Making back up reference of MainImage to the BackUpImage.
/// </summary>
//public void MakeBackUpImage()
//{
//    backUpImage.Image = mainImage.Image;
//}
///// <summary>
///// Return reference from BackUpImage to the MainImage.
///// </summary>
//public void RestoreFromBackUpImage()
//{
//    mainImage.Image = backUpImage.Image;
//}

//protected ImageModel backUpImage;
//public ImageModel BackUpImage
//{
//    get { return backUpImage; }
//    set { backUpImage = value; }
//}