using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ViewModel
{
    public interface IUserControl
    {
        WriteableBitmap GetMainImage();
        void InvertFilter();
        void Undo();
        void Redo();
    }
}
