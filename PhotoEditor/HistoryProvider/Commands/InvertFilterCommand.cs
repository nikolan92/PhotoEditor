using PhotoEditor.ImageOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.HistoryProvider.Commands
{
    public class InvertFilterCommand : ICommand
    {
        public void Execute(WriteableBitmap imageData)
        {
            InvertFilter.InvertFilterUnsafe(imageData);
        }

        public void UnExecute(WriteableBitmap imageData)
        {
            InvertFilter.InvertFilterUnsafe(imageData);
        }
    }
}
//####Reminder####
//The ref modifier means that:
//The value is already set and
//The method can read and modify it.

//####Reminder####
//The out modifier means that:
//The Value isn't set and can't be read by the method until it is set.
//The method must set it before returning.