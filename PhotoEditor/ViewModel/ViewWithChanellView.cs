using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.HistoryProvider.Commands;
using PhotoEditor.ImageOpetarions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ViewModel
{
    public class ViewWithChanellView : IUserControl
    {
        private HistoryHelper histortHelper;
        public ViewWithChanellView(WriteableBitmap image, HistoryHelper histortHelper)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageViewModel(image);
            PrepareRGBImages(image);
           
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

        private void PrepareRGBImages(WriteableBitmap imageForClone)
        {
            //If image is larger then 250 in width then reseize it TODO:Predict situation when the height is bigger than width
            if (imageForClone.PixelWidth > 250)
            {
                //Making three copies of resized mainImage image 
                double heightScale = MainImage.Image.PixelWidth / 250.0;
                int height = (int)Math.Floor(MainImage.Image.PixelWidth / heightScale);
                RedChanellImage = new ImageViewModel(ReseizeImage.LinearReseizeImageUnsafe(MainImage.Image, 250, height));
                GreenChanellImage = new ImageViewModel(RedChanellImage.Image.Clone());
                BlueChanellImage = new ImageViewModel(RedChanellImage.Image.Clone());
            }
            else
            {//in this case just make three copies
                RedChanellImage = new ImageViewModel(imageForClone.Clone());
                GreenChanellImage = new ImageViewModel(imageForClone.Clone());
                BlueChanellImage = new ImageViewModel(imageForClone.Clone());
            }
        }
        public void InvertFilter()
        {
            histortHelper.AddToHistory(new InvertFilterCommand(), MainImage.Image);
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Redo()
        {
            throw new NotImplementedException();
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