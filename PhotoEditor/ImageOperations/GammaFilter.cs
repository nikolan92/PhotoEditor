using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class GammaFilter
    {
        public static unsafe void GammaFilterUnsafe(WriteableBitmap imageData, double gamma)
        {
            //Define gamma range
            if (gamma < 0.05)
                gamma = 0.05;
            else if (gamma > 7.0)
                gamma = 7.0;

            gamma = 1 / gamma;

            //Reserve the back buffer for updated
            imageData.Lock();
            //Pointer to the back buffer (IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixel = (byte*)imageData.BackBuffer;
            //Height in pixels
            int heightInPixels = imageData.PixelHeight;
            //Number of bits per pixel divided by eight to get number of bytes per pixel
            int bytesPerPixel = imageData.Format.BitsPerPixel / 8;
            //Number of bytes per one row (scan line)
            int widthInBytes = imageData.PixelWidth * bytesPerPixel;
            //Number of bytes taken to store one row of an image
            int stride = imageData.BackBufferStride;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = currentLine[x];
                    int oldGreen = currentLine[x + 1];
                    int oldRed = currentLine[x + 2];

                    oldBlue =(int)(255.0 * Math.Pow(oldBlue / 255.0, gamma));
                    oldGreen = (int)(255.0 * Math.Pow(oldGreen / 255.0, gamma));
                    oldRed = (int)(255.0 * Math.Pow(oldRed / 255.0, gamma));

                    currentLine[x] = (byte)oldBlue;
                    currentLine[x + 1] = (byte)oldGreen;
                    currentLine[x + 2] = (byte)oldRed;
                }
            });
            // Specify the area of the bitmap that changed. (This method must be caled from UI thread)
            imageData.AddDirtyRect(new System.Windows.Int32Rect(0, 0, imageData.PixelWidth, imageData.PixelHeight));
            // Release the back buffer and make it available for display. (This method must be caled from UI thread)
            imageData.Unlock();
        }
    }
}
