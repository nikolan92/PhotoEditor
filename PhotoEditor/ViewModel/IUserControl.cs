using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.ViewModel
{
    public interface IUserControl
    {
        void InvertFilter();
        void Undo();
        void Redo();
    }
}
