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
    public class ViewWithoutChanell : ViewLogic
    {
        public ViewWithoutChanell(WriteableBitmap image, HistoryHelper histortHelper,string fileName)
        {
            this.histortHelper = histortHelper;
            MainImage = new ImageModel(image);
            //BackUpImage = new ImageModel(backUpImage);

            ImageInfo = new ImageInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());
        }

        public override void Undo()
        {
            WriteableBitmap undoImage = histortHelper.Undo(MainImage.Image);
            //If filter was been destructive restore image from backUp image
            //If filter was't been destructive MainImage is already changed inside Undo function. 
            if (undoImage != null)
                MainImage.Image = undoImage;
        }

        public override void Redo()
        {
            histortHelper.Redo(MainImage.Image);
        }

        public override void ExecuteAndAddCommand(ICommand command)
        {
            command.Execute(MainImage.Image);
            histortHelper.Archive(command);
        }

        public override void AddCommand(ICommand command)
        {
            histortHelper.Archive(command);
        }
    }
}
