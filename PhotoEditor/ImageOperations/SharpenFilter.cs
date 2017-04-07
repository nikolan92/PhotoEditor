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
        /// Sharpen filter, using source image to get image data, perform sharpen filter on it and copy modified data into destinationImage. 
        /// </summary>
        /// <param name="sourceImage">Source image. This image will not be changed.</param>
        /// <param name="destinationImage">Destination image. This image will be modified with sharpen filter.</param>
        /// <param name="n">Sharpen amount</param>
        public static unsafe void SharpenFilter3x3UnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int n)
        {

            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = -2;

            int factor = n - 8;

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
            //
            byte* currentLine = PtrFirstPixel;
            byte* upCurrentLine;
            byte* downCurrentLine;
            byte* currentLineNew = PtrFirstPixelNew;

            //
            //
            int pBTopMid, pGTopMid, pRTopMid,
                pBMidLeft, pGMidLeft, pRMidLeft,
                pBMidRight, pGMidRight, pRMidRight,
                pBCentar, pGCentar, pRCentar,
                pBBottomMid, pGBottomMid, pRBottomMid;
            int sB, sG, sR;
            //

            //Calculating first row.
            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                int []pixel = CalculateEdgePixel(x, 0, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];
            }

            //Calculating middle of the image pixels. 
            for (int y=1; y < heightInPixels - 1; y++)
            {
                currentLine = PtrFirstPixel + (y * stride);
                upCurrentLine = PtrFirstPixel + ((y - 1) * stride);
                downCurrentLine = PtrFirstPixel + ((y + 1) * stride);

                currentLineNew = PtrFirstPixelNew + (y * stride);

                //calculate first left pixel of the image
                int[] pixel = CalculateEdgePixel(0, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[0] = (byte)pixel[0];
                currentLineNew[1] = (byte)pixel[1];
                currentLineNew[2] = (byte)pixel[2];

                //TODO:Calculate first pixel in row
                for (int x = bytesPerPixel; x < widthInBytes - bytesPerPixel; x = x + bytesPerPixel)
                {
                    pBTopMid = upCurrentLine[x];
                    pGTopMid = upCurrentLine[x + 1];
                    pRTopMid = upCurrentLine[x + 2];

                    pBMidLeft = currentLine[x - bytesPerPixel];
                    pGMidLeft = currentLine[(x + 1) - bytesPerPixel];
                    pRMidLeft = currentLine[(x + 2) - bytesPerPixel];

                    pBMidRight = currentLine[bytesPerPixel + x];
                    pGMidRight = currentLine[bytesPerPixel + x + 1];
                    pRMidRight = currentLine[bytesPerPixel + x + 2];

                    pBCentar = currentLine[x];
                    pGCentar = currentLine[x + 1];
                    pRCentar = currentLine[x + 2];

                    pBBottomMid = downCurrentLine[x];
                    pGBottomMid = downCurrentLine[x + 1];
                    pRBottomMid = downCurrentLine[x + 2];


                    sB = (pBTopMid * topMid) + (pBMidLeft * midLeft) + (pBMidRight * midRight) + (pBBottomMid * bottomMid) + (pBCentar * n);
                    sG = (pGTopMid * topMid) + (pGMidLeft * midLeft) + (pGMidRight * midRight) + (pGBottomMid * bottomMid) + (pGCentar * n);
                    sR = (pRTopMid * topMid) + (pRMidLeft * midLeft) + (pRMidRight * midRight) + (pRBottomMid * bottomMid) + (pRCentar * n);

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

                //calculate last right pixel of the image
                pixel = CalculateEdgePixel(widthInBytes - bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[widthInBytes - bytesPerPixel] = (byte)pixel[0];
                currentLineNew[(widthInBytes - bytesPerPixel) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - bytesPerPixel) + 2] = (byte)pixel[2];
            }
            //Calculating last row.
            currentLine = PtrFirstPixel + ((heightInPixels - 1) * stride);
            currentLineNew = PtrFirstPixelNew + ((heightInPixels - 1) * stride);

            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                int[] pixel = CalculateEdgePixel(x, heightInPixels, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];
            }

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }

        /// <summary>
        /// Helper fnction for kernel image processing and specificly for sharpen filter. Based on passed information this function will deside
        /// when to use real pixel information and when not.<para> For example if 3x3 matrix is placed on top left pixel, left middle and top middle pixel does't exist
        /// so it will be replaced with zero.</para>
        /// </summary>
        /// <param name="x">Current column position (0, bytesPerPixel, bytesPerPixel * 2 ... widthBytes). </param>
        /// <param name="y">Current row position (0,1,2 ... PixelHeight)</param>
        /// <param name="widthInBytes">Width in bytes.</param>
        /// <param name="heightInPixel">Height in pixel.</param
        /// <param name="bytesPerPixel">Number of bytes per one pixel.</param>
        /// <param name="stride">Stride.</param>
        /// <param name="currentLine">Pointer on current line in the row.</param>
        /// <param name="matrixDim">Matrix dimensions supported dimensions 3x3,5x5 and 7x7.</param>
        /// <returns>Pixel value Blue, Green and Red in int array respectively.</returns>
        private static unsafe int[] CalculateEdgePixel(int x,int y, int widthInBytes, int heightInPixel, int bytesPerPixel,int stride,int factor,int n, byte* currentLine, int matrixDim)
        {
            int sR, sG, sB, pBCentar, pGCentar, pRCentar;

            int replaceValue = 0;

            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = -2;

            sR = sG = sB = 0;

            pBCentar = currentLine[x];
            pGCentar = currentLine[x + 1];
            pRCentar = currentLine[x + 2];
            switch (matrixDim)
            {
                case 3:// 3x3 matrix
                    int pBTopMid, pGTopMid, pRTopMid,
                    pBMidLeft, pGMidLeft, pRMidLeft,
                    pBMidRight, pGMidRight, pRMidRight,
                    pBBottomMid, pGBottomMid, pRBottomMid;

                    if (x == 0) // first pixel in collumn
                    {
                        pRMidLeft = pGMidLeft = pBMidLeft = replaceValue;

                        pBMidRight = currentLine[x];
                        pGMidRight = currentLine[x + 1];
                        pRMidRight = currentLine[x + 2];
                    }
                    else if (x == widthInBytes - bytesPerPixel) //last pixel in collumn
                    {
                        pBMidRight = pGMidRight = pRMidRight = replaceValue;

                        pBMidLeft = currentLine[x - bytesPerPixel];
                        pGMidLeft = currentLine[(x - bytesPerPixel) + 1];
                        pRMidLeft = currentLine[(x - bytesPerPixel) + 2];
                    }
                    else // somewhere in the middle of collumn
                    {
                        pBMidRight = currentLine[x];
                        pGMidRight = currentLine[x + 1];
                        pRMidRight = currentLine[x + 2];

                        pBMidLeft = currentLine[x - bytesPerPixel];
                        pGMidLeft = currentLine[(x - bytesPerPixel) + 1];
                        pRMidLeft = currentLine[(x - bytesPerPixel) + 2];
                    }

                    if (y == 0) // first pixel row
                    {
                        pRTopMid = pGTopMid = pBTopMid = replaceValue;

                        pBBottomMid = currentLine[x + stride];
                        pGBottomMid = currentLine[x + stride + 1];
                        pRBottomMid = currentLine[x + stride + 2];

                    } else if (y == heightInPixel) // last pixel row
                    {
                        pRBottomMid = pGBottomMid = pBBottomMid = replaceValue;

                        pBTopMid = currentLine[x - stride];
                        pGTopMid = currentLine[(x - stride) + 1];
                        pRTopMid = currentLine[(x - stride) + 2];
                    }
                    else
                    {
                        pBTopMid = currentLine[x - stride];
                        pGTopMid = currentLine[(x - stride) + 1];
                        pRTopMid = currentLine[(x - stride) + 2];

                        pBBottomMid = currentLine[x + stride];
                        pGBottomMid = currentLine[x + stride + 1];
                        pRBottomMid = currentLine[x + stride + 2];
                    }

                    sB = (pBTopMid * topMid) + (pBMidLeft * midLeft) + (pBMidRight * midRight) + (pBBottomMid * bottomMid) + (pBCentar * n);
                    sG = (pGTopMid * topMid) + (pGMidLeft * midLeft) + (pGMidRight * midRight) + (pGBottomMid * bottomMid) + (pGCentar * n);
                    sR = (pRTopMid * topMid) + (pRMidLeft * midLeft) + (pRMidRight * midRight) + (pRBottomMid * bottomMid) + (pRCentar * n);

                    break;
                case 5:
                    break;
                case 7:
                    break;
                default:
                    throw new InvalidOperationException("Allowed matrix dimensions are 3x3,5x5,7x7 !");
            }

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

            int[] result = new int[3];

            result[0] = sB;
            result[1] = sG;
            result[2] = sR;
            return result;
        } 
    }
}
