using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class ColorChanell
    {

        public enum Chenell
        {
            Red,Green,Blue
        }
        /// <summary>
        /// Chanell filter, using sourceAndDestinationImage image to get image data, after chanell separation modified data are copy back into sourceAndDestinationImage. 
        /// </summary>
        /// <param name="sourceAndDestinationImage">Image for processing.</param>
        /// <param name="chanell">Define which color chanell you want to separate.</param>
        public static unsafe void ChanellFilterUnsafe(WriteableBitmap sourceAndDestinationImage, Chenell chanell)
        {
            //Reserve the back buffer for updated
            sourceAndDestinationImage.Lock();
            //Pointer to the back buffer (IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixel = (byte*)sourceAndDestinationImage.BackBuffer;
            //Height in pixels
            int heightInPixels = sourceAndDestinationImage.PixelHeight;
            //Number of bits per pixel divided by eight to get number of bytes per pixel
            int bytesPerPixel = sourceAndDestinationImage.Format.BitsPerPixel / 8;
            //Number of bytes per one row (scan line)
            int widthInBytes = sourceAndDestinationImage.PixelWidth * bytesPerPixel;
            //Number of bytes taken to store one row of an image
            int stride = sourceAndDestinationImage.BackBufferStride;

            if (chanell == Chenell.Red)
            {
                //Red chanell
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];

                        oldBlue = 0;
                        oldGreen = 0;

                        currentLine[x] = (byte)oldBlue;
                        currentLine[x + 1] = (byte)oldGreen;
                    }
                });
            }else if(chanell == Chenell.Green)
            {
                //Green chanell
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldRed = currentLine[x + 2];

                        oldBlue = 0;
                        oldRed = 0;

                        currentLine[x] = (byte)oldBlue;
                        currentLine[x + 2] = (byte)oldRed;
                    }
                });
            }else
            {
                //Blue chanell
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        oldGreen = 0;
                        oldRed = 0;

                        currentLine[x + 1] = (byte)oldGreen;
                        currentLine[x + 2] = (byte)oldRed;
                    }
                });
            }
            // Specify the area of the bitmap that changed. (This method must be caled from UI thread)
            sourceAndDestinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceAndDestinationImage.PixelWidth, sourceAndDestinationImage.PixelHeight));
            // Release the back buffer and make it available for display. (This method must be caled from UI thread)
            sourceAndDestinationImage.Unlock();
        }
    }
}
