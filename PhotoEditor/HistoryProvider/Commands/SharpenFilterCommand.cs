using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
namespace PhotoEditor.HistoryProvider.Commands
{
    public class SharpenFilterCommand : ICommand
    {
        private WriteableBitmap backUpImage;
        private int n;

        public SharpenFilterCommand(WriteableBitmap imageSource, int n)
        {
            backUpImage = imageSource;
            this.n = n;
        }

        public void Execute(WriteableBitmap imageSourceAndDestination)
        {
            backUpImage = imageSourceAndDestination.Clone();
            SharpenFilter.SharpenFilterUnsafeWithCopy(backUpImage, imageSourceAndDestination, n);
        }

        public WriteableBitmap UnExecute(WriteableBitmap imageData)
        {
            return backUpImage;
        }
    }
}
