using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using System;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;

namespace PhotoEditor.ViewModel
{
    public class ViewWithChanell : ViewLogic
    {
        public ImageModel RedChanellImage { get; set; }
        public ImageModel GreenChanellImage { get; set; }
        public ImageModel BlueChanellImage { get; set; }
        public ViewWithChanell(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            HistortHelper = histortHelper;
            MainImage = new ImageModel(image);

            RedChanellImage = new ImageModel();
            GreenChanellImage = new ImageModel();
            BlueChanellImage = new ImageModel();

            ImageInfo = new ImageInfoModel(fileName,image.PixelWidth.ToString(),image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }
        public override void Undo()
        {
            HistortHelper.Undo(MainImage);
            RefreshView();
        }
        public override void Redo()
        {
            HistortHelper.Redo(MainImage);
            RefreshView();
        }
        public override void RefreshView()
        {
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }
        /// <summary>
        /// Making three reseized copies for RedChanellImage,GreenChanellImage,BlueChanellImage from passed image.
        /// </summary>
        /// <param name="imageForClone">Source image.</param>
        private void PrepareRGBImages(WriteableBitmap imageForClone)
        {
            //If image is larger then 250 in width then reseize it 
            //TODO:Predict situation when the height is bigger than width
            if (imageForClone.PixelWidth > 250)
            {
                //Making three copies of resized mainImage image 
                double heightScale = imageForClone.PixelWidth / 250.0;
                int height = (int)Math.Floor(imageForClone.PixelHeight / heightScale);
                RedChanellImage.Image = ReseizeImage.LinearReseizeImageUnsafe(imageForClone, 250, height);
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