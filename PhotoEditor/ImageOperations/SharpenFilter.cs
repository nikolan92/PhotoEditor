using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class SharpenFilter
    {
        /// <summary>
        /// Sharpen filter, using source image to get image data, perform gamma on it and copy modified data into destinationImage. 
        /// </summary>
        /// <param name="sourceImage">Source image. This image will not be changed.</param>
        /// <param name="destinationImage">Destination image. This image will be modified with gamma filter.</param>
        /// <param name="n">Sharpen amount</param>
        public static unsafe void SharpenFilterUnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int n)
        {

            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = -2;

            int factor = n - 8;

            //int offset = 0;

            //------------------------------- DestinationImageParametars ---------------------------------------------
            destinationImage.Lock();
            byte* PtrFirstPixelNew = (byte*)destinationImage.BackBuffer;
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

            //Temporary solution escape image edges
            for (int y = 1; y < heightInPixels-1; y++)
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                byte* upCurrentLine = PtrFirstPixel + ((y-1) * stride);
                byte* downCurrentLine = PtrFirstPixel + ((y+1) * stride);

                byte* currentLineNew = PtrFirstPixelNew + (y * stride);

                for (int x = bytesPerPixel; x < widthInBytes - bytesPerPixel; x = x + bytesPerPixel)
                {
                    int pBTopMid = upCurrentLine[x];
                    int pGTopMid = upCurrentLine[x + 1];
                    int pRTopMid = upCurrentLine[x + 2];

                    int pBMidLeft = currentLine[x - bytesPerPixel];
                    int pGMidLeft = currentLine[(x + 1) - bytesPerPixel];
                    int pRMidLeft = currentLine[(x + 2) - bytesPerPixel];

                    int pBMidRight = currentLine[bytesPerPixel + x];
                    int pGMidRight = currentLine[bytesPerPixel + x + 1];
                    int pRMidRight = currentLine[bytesPerPixel + x + 2];

                    int pBCentar = currentLine[x];
                    int pGCentar = currentLine[x + 1];
                    int pRCentar = currentLine[x + 2];

                    int pBBottomMid = downCurrentLine[x];
                    int pGBottomMid = downCurrentLine[x + 1];
                    int pRBottomMid = downCurrentLine[x + 2];


                    int sB = (pBTopMid * topMid) + (pBMidLeft * midLeft) + (pBMidRight * midRight) + (pBBottomMid * bottomMid) + (pBCentar * n);
                    int sG = (pGTopMid * topMid) + (pGMidLeft * midLeft) + (pGMidRight * midRight) + (pGBottomMid * bottomMid) + (pGCentar * n);
                    int sR = (pRTopMid * topMid) + (pRMidLeft * midLeft) + (pRMidRight * midRight) + (pRBottomMid * bottomMid) + (pRCentar * n);

                    sB = sB / factor;
                    sG = sG / factor;
                    sR = sR / factor;

                    if (sB > 255)
                        sB = 255;
                    else if (sB < 0)
                        sB = 0;

                    if (sG > 255)
                        sG = 255;
                    else if (sG < 0)
                        sG = 0;

                    if (sR > 255)
                        sR = 255;
                    else if (sR < 0)
                        sR = 0;

                    currentLineNew[x] = (byte)(sB);
                    currentLineNew[x + 1] = (byte)(sG);
                    currentLineNew[x + 2] = (byte)(sR);
                }
            }
            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }
    }
}
