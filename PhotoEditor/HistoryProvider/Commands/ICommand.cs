using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.HistoryProvider.Commands
{
    public interface ICommand
    {
        void Execute(WriteableBitmap imageData);
        void UnExecute(WriteableBitmap imageData);
    }
}
