using PhotoEditor.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.HistoryProvider
{
    public class ImageReferenceKeeper
    {
        private ImageModel backUpImage;

        public ImageReferenceKeeper(ImageModel imageSource)
        {
            backUpImage = new ImageModel(imageSource.Image);
        }

        public void SwapReference(ImageModel imageModel)
        {
            ImageModel tmp = new ImageModel(imageModel.Image);
            imageModel.Image = backUpImage.Image;
            backUpImage.Image = tmp.Image;
        }
    }
}
