using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        protected ImageBasicInfoModel imageBasicInfo;
        public ImageBasicInfoModel ImageBasicInfo
        {
            get { return imageBasicInfo; }
            set { imageBasicInfo = value; }
        }
        public abstract void InvertFilter();
        public abstract void Undo();
        public abstract void Redo();
    }
}
