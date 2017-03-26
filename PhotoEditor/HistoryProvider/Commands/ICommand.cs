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
        //UnExecute can to return null, 
        //If UnExecute return null that mean the function is not destructive and image passed by parametar going to be changed
        //but if UnExecute return Writeable image that mean the function is destructive on image and UnExecute can be done only if original image is return 
        WriteableBitmap UnExecute(WriteableBitmap imageData);
    }
}
