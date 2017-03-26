using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class InvertFilter
    {
        /// <summary>
        /// Invert filter, using sourceAndDestinationImage image to get image data, do invert on it and copy back modified data into sourceAndDestinationImage. 
        /// </summary>
        /// <param name="sourceAndDestinationImage">Image for proccessing.</param>
        public static unsafe void InvertFilterUnsafe(WriteableBitmap sourceAndDestinationImage)
        {
            //Reserve the back buffer for updated
            sourceAndDestinationImage.Lock();
            //Pointer to the back buffer (IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixel = (byte*)sourceAndDestinationImage.BackBuffer;
            //Height in pixels
            int heightInPixels = sourceAndDestinationImage.PixelHeight;
            //Number of bits per pixel divided by eight to get number of bytes per pixel
            int bytesPerPixel = sourceAndDestinationImage.Format.BitsPerPixel/8;
            //Number of bytes per one row (scan line)
            int widthInBytes = sourceAndDestinationImage.PixelWidth * bytesPerPixel;
            //Number of bytes taken to store one row of an image
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
            // Specify the area of the bitmap that changed. (This method must be caled from UI thread)
            sourceAndDestinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceAndDestinationImage.PixelWidth, sourceAndDestinationImage.PixelHeight));
            // Release the back buffer and make it available for display. (This method must be caled from UI thread)
            sourceAndDestinationImage.Unlock();
        }
    }
}
