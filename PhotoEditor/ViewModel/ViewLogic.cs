using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;

namespace PhotoEditor.ViewModel
{
    public abstract class ViewLogic
    {
        protected HistoryHelper HistortHelper { get; set; }
        public ImageModel MainImage { get; set; }
        public ImageInfoModel ImageInfo{ get; set; }
        public void AddImageReference(ImageModel imageModel)
        {
            HistortHelper.Archive(imageModel);
        }
        public abstract void RefreshView();
        public abstract void Undo();
        public abstract void Redo();
    }
}
