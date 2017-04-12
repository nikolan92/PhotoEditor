using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class PixelateImage
    {

        public static unsafe void PixelateImageUnsafe(WriteableBitmap sourceAndDestinationImage)
        {

            sourceAndDestinationImage.Lock();
            byte* PtrFirstPixel = (byte*)sourceAndDestinationImage.BackBuffer;
            int heightInPixels = sourceAndDestinationImage.PixelHeight;
            int bytesPerPixel = sourceAndDestinationImage.Format.BitsPerPixel / 8;
            int widthInBytes = sourceAndDestinationImage.PixelWidth * bytesPerPixel;
            int stride = sourceAndDestinationImage.BackBufferStride;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = currentLine[x];
                    int oldGreen = currentLine[x + 1];
                    int oldRed = currentLine[x + 2];

                    oldBlue = 255 - oldBlue;
                    oldGreen = 255 - oldGreen;
                    oldRed = 255 - oldRed;
                    currentLine[x] = (byte)oldBlue;
                    currentLine[x + 1] = (byte)oldGreen;
                    currentLine[x + 2] = (byte)oldRed;
                }
            });

            sourceAndDestinationImage.Unlock();
        }
    }
}
