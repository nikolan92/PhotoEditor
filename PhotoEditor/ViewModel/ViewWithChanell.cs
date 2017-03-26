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
    public class ViewWithChanell : ViewLogic
    {
        
        public ViewWithChanell(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageModel(image);

            RedChanellImage = new ImageModel();
            GreenChanellImage = new ImageModel();
            BlueChanellImage = new ImageModel();

            ImageInfo = new ImageInfoModel(fileName,image.PixelWidth.ToString(),image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
            PrepareRGBImages(image);
            DoChanellFilter();
        }
        private ImageModel redChanellImage;
        public ImageModel RedChanellImage
        {
            get { return redChanellImage; }
            set { redChanellImage = value; }
        }
        private ImageModel greenChanellImage;
        public ImageModel GreenChanellImage
        {
            get { return greenChanellImage; }
            set { greenChanellImage = value; }
        }
        private ImageModel blueChanellImage;
        public ImageModel BlueChanellImage
        {
            get { return blueChanellImage; }
            set { blueChanellImage = value; }
        }
        //PrepareRGB reseize MainImage and makes three clones images RedChanellImage,GreenChanellImage,BlueChanellImage
        private void PrepareRGBImages(WriteableBitmap imageForClone)
        {
            //If image is larger then 250 in width then reseize it TODO:Predict situation when the height is bigger than width
            if (imageForClone.PixelWidth > 250)
            {
                //Making three copies of resized mainImage image 
                double heightScale = MainImage.Image.PixelWidth / 250.0;
                int height = (int)Math.Floor(MainImage.Image.PixelHeight / heightScale);
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

        public override void Undo()
        {
            WriteableBitmap undoImage = histortHelper.Undo(MainImage.Image);
            //If filter was destructive restore image from backUp image
            //If filter was't destructive MainImage is already changed inside Undo function. 
            if (undoImage != null)
                MainImage.Image = undoImage;

            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public override void Redo()
        {
            histortHelper.Redo(MainImage.Image);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public override void ExecuteAndAddCommand(ICommand command)
        {
            command.Execute(MainImage.Image);
            histortHelper.Archive(command);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
        }

        public override void AddCommand(ICommand command)
        {
            histortHelper.Archive(command);
            PrepareRGBImages(MainImage.Image);
            DoChanellFilter();
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