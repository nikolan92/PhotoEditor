using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class ChannelHistogramFilter
    {
        public static unsafe void ChannelHistogramUnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int minValue,int maxValue)
        {
            //------------------------------- DestinationImageParametars -----------------------------------
            destinationImage.Lock();
            byte* PtrFirstPixelNew = (byte*)destinationImage.BackBuffer;
            //------------------------------- SourceImageParametars ----------------------------------------
            sourceImage.Lock();
            byte* PtrFirstPixel = (byte*)sourceImage.BackBuffer;
            int heightInPixels = sourceImage.PixelHeight;
            int bytesPerPixel = sourceImage.Format.BitsPerPixel / 8;
            int widthInBytes = sourceImage.PixelWidth * bytesPerPixel;
            int stride = sourceImage.BackBufferStride;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                byte* currentLineNew = PtrFirstPixelNew + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = currentLine[x];
                    int oldGreen = currentLine[x + 1];
                    int oldRed = currentLine[x + 2];

                    if(oldBlue > maxValue)
                        oldBlue = maxValue;
                    if (oldGreen > maxValue)
                        oldGreen = maxValue;
                    if (oldRed > maxValue)
                        oldRed = maxValue;

                    if (oldBlue < minValue)
                        oldBlue = minValue;
                    if (oldGreen < minValue)
                        oldGreen = minValue;
                    if (oldRed < minValue)
                        oldRed = minValue;


                    currentLineNew[x] = (byte)oldBlue;
                    currentLineNew[x + 1] = (byte)oldGreen;
                    currentLineNew[x + 2] = (byte)oldRed;
                }
            });

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }

    }
}
