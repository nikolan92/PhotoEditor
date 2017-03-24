using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.HistoryProvider.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
namespace PhotoEditor.ViewModel
{
    public class ViewWithChanellView : IUserControl
    {
        private HistoryHelper histortHelper;
        
        public ViewWithChanellView(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageViewModel(image);
            RedChanellImage = new ImageViewModel();
            GreenChanellImage = new ImageViewModel();
            BlueChanellImage = new ImageViewModel();

            ImageBasicInfo = new ImageBasicInfoModel(fileName,image.PixelWidth.ToString(),image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
            PrepareRGBImages(image);
            DoChanellFilter();
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
        private ImageBasicInfoModel imageBasicInfo;

        public ImageBasicInfoModel ImageBasicInfo
        {
            get { return imageBasicInfo; }
            set { imageBasicInfo = value; }
        }
        //PrepareRGB reseize MainImage and makes three clones images RedChanellImage,GreenChanellImage,BlueChanellImage
        private void PrepareRGBImages(WriteableBitmap imageForClone)
        {
            //If image is larger then 250 in width then reseize it TODO:Predict situation when the height is bigger than width
            if (imageForClone.PixelWidth > 250)
            {
                //Making three copies of resized mainImage image 
                double heightScale = MainImage.Image.PixelWidth / 250.0;
                int height = (int)Math.Floor(MainImage.Image.PixelWidth / heightScale);
                RedChanellImage.Image = ReseizeImage.LinearReseizeImageUnsafe(MainImage.Image, 250, height);
                GreenChanellImage.Image = RedChanellImage.Image.Clone();
                BlueChanellImage.Image = RedChanellImage.Image.Clone();
            }
            else
            {//in this case just make three copies
                RedChanellImage.Image = imageForClone.Clone();
                GreenChanellImage.Image = imageForClone.Clone();
                BlueChanellImage.Image = imageForClone.Clone();
            }
        }
        private void DoChanellFilter()
        {
            ColorChanell.ChanellFilterUnsafe(RedChanellImage.Image, ColorChanell.Chenell.Red);
            ColorChanell.ChanellFilterUnsafe(GreenChanellImage.Image, ColorChanell.Chenell.Green);
            ColorChanell.ChanellFilterUnsafe(BlueChanellImage.Image, ColorChanell.Chenell.Blue);
        }

        //Because InvertFilter is not destructive function
        //there is no need to make fresh copie from MainImage just need to do invert on existing Red,Green and Blue image
        public void InvertFilter()
        {
            histortHelper.AddToHistory(new InvertFilterCommand(), MainImage.Image);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public void Undo()
        {
            histortHelper.Undo(MainImage.Image);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public void Redo()
        {
            histortHelper.Redo(MainImage.Image);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public WriteableBitmap GetMainImage()
        {
            return MainImage.Image;
        }
    }
}

//TODO:
//Get MainImage.Image rezise, make clones, filter by chanell and show.
//int width = (int)((double)mainViewModel.MainImage.Image.PixelWidth * 0.3);
//int height = (int)((double)mainViewModel.MainImage.Image.PixelWidth * 0.3);
//mainViewModel.RedChanellImage.Image = ReseizeImage.LinearReseizeImageUnsafe(mainViewModel.MainImage.Image, width, height);
//            mainViewModel.GreenChanellImage.Image = mainViewModel.RedChanellImage.Image.Clone();
//            mainViewModel.BlueChanellImage.Image = mainViewModel.RedChanellImage.Image.Clone();
//            //GC.Collect();