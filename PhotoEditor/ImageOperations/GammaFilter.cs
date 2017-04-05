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
        /// <summary>
        /// Gamma filter, using sourceAndDestinationImage image to get image data, perform gamma on it and copy back modified data into sourceAndDestinationImage. 
        /// </summary>
        /// <param name="sourceAndDestinationImage">Image for proccessing.</param>
        /// <param name="gammaValue">Gamma value between 0.05-7.0.</param>
        public static unsafe void GammaFilterUnsafe(WriteableBitmap sourceAndDestinationImage, double gammaValue)
        {
            //Define gamma range
            if (gammaValue < 0.05)
                gammaValue = 0.05;
            else if (gammaValue > 7.0)
                gammaValue = 7.0;

            gammaValue = 1 / gammaValue;

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

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = currentLine[x];
                    int oldGreen = currentLine[x + 1];
                    int oldRed = currentLine[x + 2];

                    oldBlue = (int)(255.0 * Math.Pow(oldBlue / 255.0, gammaValue));
                    oldGreen = (int)(255.0 * Math.Pow(oldGreen / 255.0, gammaValue));
                    oldRed = (int)(255.0 * Math.Pow(oldRed / 255.0, gammaValue));

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
        /// <summary>
        /// Gamma filter, using source image to get image data, perform gamma on it and copy modified data into destinationImage. 
        /// </summary>
        /// <param name="sourceImage">Source image. This image will not be changed.</param>
        /// <param name="destinationImage">Destination image. This image will be modified with gamma filter.</param>
        /// <param name="gammaValue">Gamma value between 0.05-7.0.</param>
        public static unsafe void GammaFilterUnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, double gammaValue)
        {
            //WriteableBitmap newImage = new WriteableBitmap(imageData.PixelWidth, imageData.PixelHeight, imageData.DpiX, imageData.DpiY, imageData.Format, imageData.Palette);
            //Define gamma range
            if (gammaValue < 0.05)
                gammaValue = 0.05;
            else if (gammaValue > 7.0)
                gammaValue = 7.0;

            gammaValue = 1 / gammaValue;
            //------------------------------- DestinationImageParametars -----------------------------------
            destinationImage.Lock();
            byte* PtrFirstPixelNew = (byte*) destinationImage.BackBuffer;
            //------------------------------- SourceImageParametars ----------------------------------------
            //Reserve the back buffer for updated
            sourceImage.Lock();
            //Pointer to the back buffer (IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixel = (byte*)sourceImage.BackBuffer;
            //Height in pixels
            int heightInPixels = sourceImage.PixelHeight;
            //Number of bits per pixel divided by eight to get number of bytes per pixel
            int bytesPerPixel = sourceImage.Format.BitsPerPixel / 8;
            //Number of bytes per one row (scan line)
            int widthInBytes = sourceImage.PixelWidth * bytesPerPixel;
            //Number of bytes taken to store one row of an image
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

                    oldBlue = (int)(255.0 * Math.Pow(oldBlue / 255.0, gammaValue));
                    oldGreen = (int)(255.0 * Math.Pow(oldGreen / 255.0, gammaValue));
                    oldRed = (int)(255.0 * Math.Pow(oldRed / 255.0, gammaValue));

                    currentLineNew[x] = (byte)oldBlue;
                    currentLineNew[x + 1] = (byte)oldGreen;
                    currentLineNew[x + 2] = (byte)oldRed;
                }
            });
            // Specify the area of the bitmap that changed. (This method must be caled from UI thread)
            //imageData.AddDirtyRect(new System.Windows.Int32Rect(0, 0, imageData.PixelWidth, imageData.PixelHeight));
            // Release the back buffer and make it available for display. (This method must be caled from UI thread)
            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }
    }
}
