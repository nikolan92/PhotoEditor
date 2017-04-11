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

            //Calculating middle pixels of the image.
            for (int y=1; y < heightInPixels - 1; y++)
            {
                currentLine = PtrFirstPixel + (y * stride);
                upCurrentLine = PtrFirstPixel + ((y - 1) * stride);
                downCurrentLine = PtrFirstPixel + ((y + 1) * stride);

                currentLineNew = PtrFirstPixelNew + (y * stride);

                //Calculate first left pixel of the image.
                int[] pixel = CalculateEdgePixel(0, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[0] = (byte)pixel[0];
                currentLineNew[1] = (byte)pixel[1];
                currentLineNew[2] = (byte)pixel[2];

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

                //Calculate last right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - bytesPerPixel, y, widthInBytes, heightInPixels-1, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[widthInBytes - bytesPerPixel] = (byte)pixel[0];
                currentLineNew[(widthInBytes - bytesPerPixel) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - bytesPerPixel) + 2] = (byte)pixel[2];
            }
            //Calculating last row.
            currentLine = PtrFirstPixel + ((heightInPixels - 1) * stride);
            currentLineNew = PtrFirstPixelNew + ((heightInPixels - 1) * stride);

            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                int[] pixel = CalculateEdgePixel(x, heightInPixels-1, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 3);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];
            }

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }

        /// <summary>
        /// Sharpen filter, using source image to get image data, perform sharpen filter on it and copy modified data into destinationImage. 
        /// </summary>
        /// <param name="sourceImage">Source image. This image will not be changed.</param>
        /// <param name="destinationImage">Destination image. This image will be modified with sharpen filter.</param>
        /// <param name="n">Sharpen amount.</param>
        public static unsafe void SharpenFilter5x5UnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int n)
        {

            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = -1;

            int factor = n - 12;

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
            byte* upCurrentLine,up2CurrentLine;
            byte* downCurrentLine,down2CurrentLine;
            byte* currentLineNew = PtrFirstPixelNew;

            //
            //
            int pBTopMid, pGTopMid, pRTopMid,
                pBTop2Mid, pGTop2Mid, pRTop2Mid,
                pBTop2MidR, pGTop2MidR, pRTop2MidR,
                pBMidLeft, pGMidLeft, pRMidLeft,
                pBMid2Left, pGMid2Left, pRMid2Left,
                pBMid2LeftUp, pGMid2LeftUp, pRMid2LeftUp,
                pBMidRight, pGMidRight, pRMidRight,
                pBMid2Right, pGMid2Right, pRMid2Right,
                pBMid2RightDown, pGMid2RightDown, pRMid2RightDown,
                pBCentar, pGCentar, pRCentar,
                pBBottomMid, pGBottomMid, pRBottomMid,
                pBBottom2Mid, pGBottom2Mid, pRBottom2Mid,
                pBBottom2MidL, pGBottom2MidL, pRBottom2MidL;
            int sB, sG, sR;
            //

            //Calculating first two rows.
            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                int[] pixel = CalculateEdgePixel(x, 0, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];

                pixel = CalculateEdgePixel(x, 1, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine + stride, 5);

                currentLineNew[x + stride] = (byte)pixel[0];
                currentLineNew[x + stride + 1] = (byte)pixel[1];
                currentLineNew[x + stride + 2] = (byte)pixel[2];
            }

            //Calculating middle pixels of the image.
            for (int y = 2; y < heightInPixels - 2; y++)
            {
                currentLine = PtrFirstPixel + (y * stride);
                upCurrentLine = PtrFirstPixel + ((y - 1) * stride);
                downCurrentLine = PtrFirstPixel + ((y + 1) * stride);

                up2CurrentLine = PtrFirstPixel + ((y - 2) * stride);
                down2CurrentLine = PtrFirstPixel + ((y + 2) * stride);

                currentLineNew = PtrFirstPixelNew + (y * stride);

                //Calculate first left pixel of the image.
                int[] pixel = CalculateEdgePixel(0, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);

                currentLineNew[0] = (byte)pixel[0];
                currentLineNew[1] = (byte)pixel[1];
                currentLineNew[2] = (byte)pixel[2];
                //Calculate socond left pixel of the image.
                pixel = CalculateEdgePixel(bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);

                currentLineNew[bytesPerPixel] = (byte)pixel[0];
                currentLineNew[bytesPerPixel + 1] = (byte)pixel[1];
                currentLineNew[bytesPerPixel + 2] = (byte)pixel[2];

                for (int x = bytesPerPixel * 2; x < widthInBytes - (2 * bytesPerPixel); x = x + bytesPerPixel)
                {
                    //Top
                    pBTopMid = upCurrentLine[x];
                    pGTopMid = upCurrentLine[x + 1];
                    pRTopMid = upCurrentLine[x + 2];
                    //5x5
                    pBTop2Mid = up2CurrentLine[x];//
                    pGTop2Mid = up2CurrentLine[x + 1];
                    pRTop2Mid = up2CurrentLine[x + 2];

                    pBTop2MidR = up2CurrentLine[x + bytesPerPixel];//
                    pGTop2MidR = up2CurrentLine[x + bytesPerPixel + 1];
                    pRTop2MidR = up2CurrentLine[x + bytesPerPixel + 2];
                    
                    //Middle left
                    pBMidLeft = currentLine[x - bytesPerPixel];
                    pGMidLeft = currentLine[(x + 1) - bytesPerPixel];
                    pRMidLeft = currentLine[(x + 2) - bytesPerPixel];
                    //5x5
                    pBMid2Left = currentLine[x - (2*bytesPerPixel)];//
                    pGMid2Left = currentLine[(x + 1) - (2 * bytesPerPixel)];
                    pRMid2Left = currentLine[(x + 2) - (2 * bytesPerPixel)];

                    pBMid2LeftUp = upCurrentLine[x - (2 * bytesPerPixel)];//
                    pGMid2LeftUp = upCurrentLine[(x + 1) - (2 * bytesPerPixel)];
                    pRMid2LeftUp = upCurrentLine[(x + 2) - (2 * bytesPerPixel)];

                    //Middle right
                    pBMidRight = currentLine[bytesPerPixel + x];
                    pGMidRight = currentLine[bytesPerPixel + x + 1];
                    pRMidRight = currentLine[bytesPerPixel + x + 2];
                    //5x5
                    pBMid2Right = currentLine[(bytesPerPixel * 2) + x];//
                    pGMid2Right = currentLine[(bytesPerPixel * 2) + x + 1];
                    pRMid2Right = currentLine[(bytesPerPixel * 2) + x + 2];

                    pBMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x];//
                    pGMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x + 1];
                    pRMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x + 2];

                    //Bottom
                    pBBottomMid = downCurrentLine[x];
                    pGBottomMid = downCurrentLine[x + 1];
                    pRBottomMid = downCurrentLine[x + 2];
                    //5x5
                    pBBottom2Mid = down2CurrentLine[x];//
                    pGBottom2Mid = down2CurrentLine[x + 1];
                    pRBottom2Mid = down2CurrentLine[x + 2];

                    pBBottom2MidL = down2CurrentLine[x - bytesPerPixel];//
                    pGBottom2MidL = down2CurrentLine[x - bytesPerPixel + 1];
                    pRBottom2MidL = down2CurrentLine[x - bytesPerPixel + 2];
                    //Center
                    pBCentar = currentLine[x];
                    pGCentar = currentLine[x + 1];
                    pRCentar = currentLine[x + 2];

                    sB = (pBTopMid * topMid) + (pBTop2Mid * topMid) + (pBTop2MidR * topMid) +
                         (pBMidLeft * midLeft) + (pBMid2Left * midLeft) + (pBMid2LeftUp * midLeft) +
                         (pBMidRight * midRight) + (pBMid2Right * midRight) + (pBMid2RightDown * midRight) +
                         (pBBottomMid * bottomMid) + (pBBottom2Mid * bottomMid) + (pBBottom2MidL * bottomMid) +
                         (pBCentar * n);
                    sG = (pGTopMid * topMid) + (pGTop2Mid * topMid) + (pGTop2MidR * topMid) +
                         (pGMidLeft * midLeft) + (pGMid2Left * midLeft) + (pGMid2LeftUp * midLeft) +
                         (pGMidRight * midRight) + (pGMid2Right * midRight) + (pGMid2RightDown * midRight) +
                         (pGBottomMid * bottomMid) + (pGBottom2Mid * bottomMid) + (pGBottom2MidL * bottomMid) +
                         (pGCentar * n);
                    sR = (pRTopMid * topMid) + (pRTop2Mid * topMid) + (pRTop2MidR * topMid) +
                         (pRMidLeft * midLeft) + (pRMid2Left * midLeft) + (pRMid2LeftUp * midLeft) +
                         (pRMidRight * midRight) + (pRMid2Right * midRight) + (pRMid2RightDown * midRight) +
                         (pRBottomMid * bottomMid) + (pRBottom2Mid * bottomMid) + (pRBottom2MidL * bottomMid) +
                         (pRCentar * n);

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

                //Calculate last right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);

                currentLineNew[widthInBytes - bytesPerPixel] = (byte)pixel[0];
                currentLineNew[(widthInBytes - bytesPerPixel) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - bytesPerPixel) + 2] = (byte)pixel[2];

                //Calculate penultimate right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - (2 * bytesPerPixel), y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);
                currentLineNew[widthInBytes - (2 * bytesPerPixel)] = (byte)pixel[0];
                currentLineNew[(widthInBytes - (2 * bytesPerPixel)) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - (2 * bytesPerPixel)) + 2] = (byte)pixel[2];
            }
            //Calculating last two rows.
            currentLine = PtrFirstPixel + ((heightInPixels - 1) * stride);  
            currentLineNew = PtrFirstPixelNew + ((heightInPixels - 1) * stride);

            byte* upCurrentLineNew = currentLineNew - stride;
            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                //Penultimate row.
                int[] pixel = CalculateEdgePixel(x, heightInPixels - 2, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine-stride, 5);

                upCurrentLineNew[x] = (byte)pixel[0];
                upCurrentLineNew[x + 1] = (byte)pixel[1];
                upCurrentLineNew[x + 2] = (byte)pixel[2];

                //Last row.
                pixel = CalculateEdgePixel(x, heightInPixels - 1, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 5);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];
            }

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }
        /// <summary>
        /// Sharpen filter, using source image to get image data, perform sharpen filter on it and copy modified data into destinationImage. 
        /// </summary>
        /// <param name="sourceImage">Source image. This image will not be changed.</param>
        /// <param name="destinationImage">Destination image. This image will be modified with sharpen filter.</param>
        /// <param name="n">Sharpen amount.</param>
        public static unsafe void SharpenFilter7x7UnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int n)
        {

            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = -1;

            int factor = n - 20;

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
            byte* upCurrentLine, up2CurrentLine, up3CurrentLine;
            byte* downCurrentLine, down2CurrentLine, down3CurrentLine;
            byte* currentLineNew = PtrFirstPixelNew;

            //
            //
            int pBTopMid, pGTopMid, pRTopMid,
                pBTop2Mid, pGTop2Mid, pRTop2Mid,
                pBTop2MidR, pGTop2MidR, pRTop2MidR,
                pBTop3Mid, pGTop3Mid, pRTop3Mid,
                pBTop3MidR, pGTop3MidR, pRTop3MidR,
                pBMidLeft, pGMidLeft, pRMidLeft,
                pBMid2Left, pGMid2Left, pRMid2Left,
                pBMid2LeftUp, pGMid2LeftUp, pRMid2LeftUp,
                pBMid3Left, pGMid3Left, pRMid3Left,
                pBMid3LeftUp, pGMid3LeftUp, pRMid3LeftUp,
                pBMidRight, pGMidRight, pRMidRight,
                pBMid2Right, pGMid2Right, pRMid2Right,
                pBMid2RightDown, pGMid2RightDown, pRMid2RightDown,
                pBMid3Right, pGMid3Right, pRMid3Right,
                pBMid3RightDown, pGMid3RightDown, pRMid3RightDown,
                pBCentar, pGCentar, pRCentar,
                pBBottomMid, pGBottomMid, pRBottomMid,
                pBBottom2Mid, pGBottom2Mid, pRBottom2Mid,
                pBBottom2MidL, pGBottom2MidL, pRBottom2MidL,
                pBBottom3Mid, pGBottom3Mid, pRBottom3Mid,
                pBBottom3MidL, pGBottom3MidL, pRBottom3MidL;

            int sB, sG, sR;
            //

            //Calculating first three rows.
            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                int[] pixel = CalculateEdgePixel(x, 0, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];

                pixel = CalculateEdgePixel(x, 1, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine + stride, 7);

                currentLineNew[x + stride] = (byte)pixel[0];
                currentLineNew[x + stride + 1] = (byte)pixel[1];
                currentLineNew[x + stride + 2] = (byte)pixel[2];

                pixel = CalculateEdgePixel(x, 2, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine + (2 * stride), 7);

                currentLineNew[x + (2 * stride)] = (byte)pixel[0];
                currentLineNew[x + (2 * stride) + 1] = (byte)pixel[1];
                currentLineNew[x + (2 * stride) + 2] = (byte)pixel[2];
            }

            //Calculating middle pixels of the image.
            for (int y = 3; y < heightInPixels - 3; y++)
            {
                currentLine = PtrFirstPixel + (y * stride);
                upCurrentLine = PtrFirstPixel + ((y - 1) * stride);
                downCurrentLine = PtrFirstPixel + ((y + 1) * stride);
                //5x5
                up2CurrentLine = PtrFirstPixel + ((y - 2) * stride);
                down2CurrentLine = PtrFirstPixel + ((y + 2) * stride);
                //7x7
                up3CurrentLine = PtrFirstPixel + ((y - 3) * stride);
                down3CurrentLine = PtrFirstPixel + ((y + 3) * stride);

                //current line new
                currentLineNew = PtrFirstPixelNew + (y * stride);

                //Calculate first left pixel of the image.
                int[] pixel = CalculateEdgePixel(0, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[0] = (byte)pixel[0];
                currentLineNew[1] = (byte)pixel[1];
                currentLineNew[2] = (byte)pixel[2];
                //Calculate socond left pixel of the image.
                pixel = CalculateEdgePixel(bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[bytesPerPixel] = (byte)pixel[0];
                currentLineNew[bytesPerPixel + 1] = (byte)pixel[1];
                currentLineNew[bytesPerPixel + 2] = (byte)pixel[2];

                //Calculate third left pixel of the image.
                pixel = CalculateEdgePixel(2 * bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[(2 * bytesPerPixel)] = (byte)pixel[0];
                currentLineNew[(2 * bytesPerPixel) + 1] = (byte)pixel[1];
                currentLineNew[(2 * bytesPerPixel) + 2] = (byte)pixel[2];

                for (int x = bytesPerPixel * 3; x < widthInBytes - (3 * bytesPerPixel); x = x + bytesPerPixel)
                {
                    //Top
                    pBTopMid = upCurrentLine[x];
                    pGTopMid = upCurrentLine[x + 1];
                    pRTopMid = upCurrentLine[x + 2];
                    //5x5
                    pBTop2Mid = up2CurrentLine[x];//
                    pGTop2Mid = up2CurrentLine[x + 1];
                    pRTop2Mid = up2CurrentLine[x + 2];

                    pBTop2MidR = up2CurrentLine[x + bytesPerPixel];//
                    pGTop2MidR = up2CurrentLine[x + bytesPerPixel + 1];
                    pRTop2MidR = up2CurrentLine[x + bytesPerPixel + 2];
                    //7x7
                    pBTop3Mid = up3CurrentLine[x];//
                    pGTop3Mid = up3CurrentLine[x + 1];
                    pRTop3Mid = up3CurrentLine[x + 2];

                    pBTop3MidR = up3CurrentLine[x + bytesPerPixel];//
                    pGTop3MidR = up3CurrentLine[x + bytesPerPixel + 1];
                    pRTop3MidR = up3CurrentLine[x + bytesPerPixel + 2];

                    //Middle left
                    pBMidLeft = currentLine[x - bytesPerPixel];
                    pGMidLeft = currentLine[(x + 1) - bytesPerPixel];
                    pRMidLeft = currentLine[(x + 2) - bytesPerPixel];
                    //5x5
                    pBMid2Left = currentLine[x - (2 * bytesPerPixel)];//
                    pGMid2Left = currentLine[(x + 1) - (2 * bytesPerPixel)];
                    pRMid2Left = currentLine[(x + 2) - (2 * bytesPerPixel)];

                    pBMid2LeftUp = upCurrentLine[x - (2 * bytesPerPixel)];//
                    pGMid2LeftUp = upCurrentLine[(x + 1) - (2 * bytesPerPixel)];
                    pRMid2LeftUp = upCurrentLine[(x + 2) - (2 * bytesPerPixel)];
                    //7x7
                    pBMid3Left = currentLine[x - (3 * bytesPerPixel)];//
                    pGMid3Left = currentLine[(x + 1) - (3 * bytesPerPixel)];
                    pRMid3Left = currentLine[(x + 2) - (3 * bytesPerPixel)];

                    pBMid3LeftUp = upCurrentLine[x - (3 * bytesPerPixel)];//
                    pGMid3LeftUp = upCurrentLine[(x + 1) - (3 * bytesPerPixel)];
                    pRMid3LeftUp = upCurrentLine[(x + 2) - (3 * bytesPerPixel)];

                    //Middle right
                    pBMidRight = currentLine[bytesPerPixel + x];
                    pGMidRight = currentLine[bytesPerPixel + x + 1];
                    pRMidRight = currentLine[bytesPerPixel + x + 2];
                    //5x5
                    pBMid2Right = currentLine[(bytesPerPixel * 2) + x];//
                    pGMid2Right = currentLine[(bytesPerPixel * 2) + x + 1];
                    pRMid2Right = currentLine[(bytesPerPixel * 2) + x + 2];

                    pBMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x];//
                    pGMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x + 1];
                    pRMid2RightDown = downCurrentLine[(bytesPerPixel * 2) + x + 2];
                    //7x7
                    pBMid3Right = currentLine[(bytesPerPixel * 3) + x];//
                    pGMid3Right = currentLine[(bytesPerPixel * 3) + x + 1];
                    pRMid3Right = currentLine[(bytesPerPixel * 3) + x + 2];

                    pBMid3RightDown = downCurrentLine[(bytesPerPixel * 3) + x];//
                    pGMid3RightDown = downCurrentLine[(bytesPerPixel * 3) + x + 1];
                    pRMid3RightDown = downCurrentLine[(bytesPerPixel * 3) + x + 2];

                    //Bottom
                    pBBottomMid = downCurrentLine[x];
                    pGBottomMid = downCurrentLine[x + 1];
                    pRBottomMid = downCurrentLine[x + 2];
                    //5x5
                    pBBottom2Mid = down2CurrentLine[x];//
                    pGBottom2Mid = down2CurrentLine[x + 1];
                    pRBottom2Mid = down2CurrentLine[x + 2];

                    pBBottom2MidL = down2CurrentLine[x - bytesPerPixel];//
                    pGBottom2MidL = down2CurrentLine[x - bytesPerPixel + 1];
                    pRBottom2MidL = down2CurrentLine[x - bytesPerPixel + 2];
                    //7x7
                    pBBottom3Mid = down3CurrentLine[x];//
                    pGBottom3Mid = down3CurrentLine[x + 1];
                    pRBottom3Mid = down3CurrentLine[x + 2];

                    pBBottom3MidL = down3CurrentLine[x - bytesPerPixel];//
                    pGBottom3MidL = down3CurrentLine[x - bytesPerPixel + 1];
                    pRBottom3MidL = down3CurrentLine[x - bytesPerPixel + 2];

                    //Center
                    pBCentar = currentLine[x];
                    pGCentar = currentLine[x + 1];
                    pRCentar = currentLine[x + 2];

                    sB = (pBTopMid * topMid) + (pBTop2Mid * topMid) + (pBTop2MidR * topMid) + 
                         (pBTop3Mid * topMid) + (pBTop3MidR * topMid) +//7x7
                         (pBMidLeft * midLeft) + (pBMid2Left * midLeft) + (pBMid2LeftUp * midLeft) +
                         (pBMid3Left * midLeft) + (pBMid3LeftUp * midLeft) +//7x7
                         (pBMidRight * midRight) + (pBMid2Right * midRight) + (pBMid2RightDown * midRight) +
                         (pBMid3Right * midRight) + (pBMid3RightDown * midRight) +//7x7
                         (pBBottomMid * bottomMid) + (pBBottom2Mid * bottomMid) + (pBBottom2MidL * bottomMid) +
                         (pBBottom3Mid * bottomMid) + (pBBottom3MidL * bottomMid) +//7x7
                         (pBCentar * n);
                    sG = (pGTopMid * topMid) + (pGTop2Mid * topMid) + (pGTop2MidR * topMid) +
                         (pGTop3Mid * topMid) + (pGTop3MidR * topMid) +//7x7
                         (pGMidLeft * midLeft) + (pGMid2Left * midLeft) + (pGMid2LeftUp * midLeft) +
                         (pGMid3Left * midLeft) + (pGMid3LeftUp * midLeft) +//7x7
                         (pGMidRight * midRight) + (pGMid2Right * midRight) + (pGMid2RightDown * midRight) +
                         (pGMid3Right * midRight) + (pGMid3RightDown * midRight) +//7x7
                         (pGBottomMid * bottomMid) + (pGBottom2Mid * bottomMid) + (pGBottom2MidL * bottomMid) +
                         (pGBottom3Mid * bottomMid) + (pGBottom3MidL * bottomMid) +//7x7
                         (pGCentar * n);
                    sR = (pRTopMid * topMid) + (pRTop2Mid * topMid) + (pRTop2MidR * topMid) +
                         (pRTop3Mid * topMid) + (pRTop3MidR * topMid) +//7x7
                         (pRMidLeft * midLeft) + (pRMid2Left * midLeft) + (pRMid2LeftUp * midLeft) +
                         (pRMid3Left * midLeft) + (pRMid3LeftUp * midLeft) +//7x7
                         (pRMidRight * midRight) + (pRMid2Right * midRight) + (pRMid2RightDown * midRight) +
                         (pRMid3Right * midRight) + (pRMid3RightDown * midRight) +//7x7
                         (pRBottomMid * bottomMid) + (pRBottom2Mid * bottomMid) + (pRBottom2MidL * bottomMid) +
                         (pRBottom3Mid * bottomMid) + (pRBottom3MidL * bottomMid) +//7x7
                         (pRCentar * n);

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

                //Calculate last right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - bytesPerPixel, y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[widthInBytes - bytesPerPixel] = (byte)pixel[0];
                currentLineNew[(widthInBytes - bytesPerPixel) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - bytesPerPixel) + 2] = (byte)pixel[2];

                //Calculate penultimate right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - (2 * bytesPerPixel), y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);
                currentLineNew[widthInBytes - (2 * bytesPerPixel)] = (byte)pixel[0];
                currentLineNew[(widthInBytes - (2 * bytesPerPixel)) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - (2 * bytesPerPixel)) + 2] = (byte)pixel[2];

                //Calculate penultimate penultimate right pixel of the image.
                pixel = CalculateEdgePixel(widthInBytes - (3 * bytesPerPixel), y, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);
                currentLineNew[widthInBytes - (3 * bytesPerPixel)] = (byte)pixel[0];
                currentLineNew[(widthInBytes - (3 * bytesPerPixel)) + 1] = (byte)pixel[1];
                currentLineNew[(widthInBytes - (3 * bytesPerPixel)) + 2] = (byte)pixel[2];
            }
            //Calculating last three rows.
            currentLine = PtrFirstPixel + ((heightInPixels - 1) * stride);

            currentLineNew = PtrFirstPixelNew + ((heightInPixels - 1) * stride);
            byte* upCurrentLineNew = currentLineNew - stride;
            byte* up2CurrentLineNew = currentLineNew - (2 * stride);

            for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
            {
                //Last row.
                int[] pixel = CalculateEdgePixel(x, heightInPixels - 1, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine, 7);

                currentLineNew[x] = (byte)pixel[0];
                currentLineNew[x + 1] = (byte)pixel[1];
                currentLineNew[x + 2] = (byte)pixel[2];

                //Penultimate row.
                pixel = CalculateEdgePixel(x, heightInPixels - 2, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine - stride, 7);

                upCurrentLineNew[x] = (byte)pixel[0];
                upCurrentLineNew[x + 1] = (byte)pixel[1];
                upCurrentLineNew[x + 2] = (byte)pixel[2];

                //Penultimate penultimate row.
                pixel = CalculateEdgePixel(x, heightInPixels - 3, widthInBytes, heightInPixels, bytesPerPixel, stride, factor, n, currentLine - (2 * stride), 7);

                up2CurrentLineNew[x] = (byte)pixel[0];
                up2CurrentLineNew[x + 1] = (byte)pixel[1];
                up2CurrentLineNew[x + 2] = (byte)pixel[2];

            }

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }

        /// <summary>
        /// Helper function for kernel image processing and specificly for sharpen filter. Based on passed information this function will deside
        /// when to use real pixel information and when not. <para> For example if 3x3 matrix is placed on top left pixel, left middle and top middle pixel does't exist
        /// so it will be replaced with zero.</para>
        /// </summary>
        /// <param name="x">Current column position (0, bytesPerPixel, bytesPerPixel * 2 ... widthBytes).</param>
        /// <param name="y">Current row position (0,1,2 ... PixelHeight)</param>
        /// <param name="widthInBytes">Width in bytes.</param>
        /// <param name="heightInPixel">Height in pixel.</param>
        /// <param name="bytesPerPixel">Number of bytes per one pixel.</param>
        /// <param name="stride">Stride.</param>
        /// <param name="factor">Factor.</param>
        /// <param name="n">N.</param>
        /// <param name="currentLine">Pointer on current line in the row.</param>
        /// <param name="matrixDim">Matrix dimensions supported dimensions 3x3,5x5 and 7x7.</param>
        /// <returns>Pixel value Blue, Green and Red in int array respectively.</returns>
        private static unsafe int[] CalculateEdgePixel(int x, int y, int widthInBytes, int heightInPixel, int bytesPerPixel, int stride, int factor, int n, byte* currentLine, int matrixDim)
        {
            //-2 for 3x3 and -1 for 5x5 and 7x7.
            int matrixValue = -2;
            if (matrixDim > 3)
                matrixValue = -1;


            //Matrix
            int topMid, midLeft, midRight, bottomMid;
            topMid = midLeft = midRight = bottomMid = matrixValue;
            //Pixels 3x3
            int pBTopMid, pGTopMid, pRTopMid,
            pBMidLeft, pGMidLeft, pRMidLeft,
            pBMidRight, pGMidRight, pRMidRight,
            pBBottomMid, pGBottomMid, pRBottomMid;

            int sR, sG, sB, pBCentar, pGCentar, pRCentar;

            int replaceValue = 0;

            sR = sG = sB = 0;

            pBCentar = currentLine[x];
            pGCentar = currentLine[x + 1];
            pRCentar = currentLine[x + 2];

            if (matrixDim >= 5)
            {
                //Up part of matrix.
                if (y >= 2)
                {   //Top middle pixel.
                    sB += currentLine[x - (2 * stride)] * topMid;
                    sG += currentLine[x - (2 * stride) + 1] * topMid;
                    sR += currentLine[x - (2 * stride) + 2] * topMid;

                    if (x <= widthInBytes - (2 * bytesPerPixel))
                    {
                        //Top middle right pixel.
                        sB += currentLine[x - (2 * stride) + bytesPerPixel] * topMid;
                        sG += currentLine[x - (2 * stride) + bytesPerPixel + 1] * topMid;
                        sR += currentLine[x - (2 * stride) + bytesPerPixel + 2] * topMid;
                    }

                    if (x <= widthInBytes - (3 * bytesPerPixel))
                    {
                        //Right middle pixel.
                        sB += currentLine[x + (2 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + (2 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + (2 * bytesPerPixel) + 2] * midRight;
                    }

                    //Left middle and up pixel.
                    if (x > bytesPerPixel)//or x >= (2 * bytesPerPixel)
                    {
                        //Left middle pixel.
                        sB += currentLine[x - (2 * bytesPerPixel)] * midLeft;
                        sG += currentLine[x - (2 * bytesPerPixel) + 1] * midLeft;
                        sR += currentLine[x - (2 * bytesPerPixel) + 2] * midLeft;
                        //Up pixel.
                        sB += currentLine[x - (stride - (2 * bytesPerPixel))] * midLeft;
                        sG += currentLine[x - (stride - (2 * bytesPerPixel)) + 1] * midLeft;
                        sR += currentLine[x - (stride - (2 * bytesPerPixel)) + 2] * midLeft;
                    }
                } else if (y == 1)//Matrix without top pixels.
                {
                    if (x <= widthInBytes - (3 * bytesPerPixel))
                    {
                        //Right middle pixel.
                        sB += currentLine[x + (2 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + (2 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + (2 * bytesPerPixel) + 2] * midRight;
                    }
                    //Left middle and up pixel.
                    if (x > bytesPerPixel)//or x >= (2 * bytesPerPixel)
                    {
                        //Left middle pixel.
                        sB += currentLine[x - (2 * bytesPerPixel)] * midLeft;
                        sG += currentLine[x - (2 * bytesPerPixel) + 1] * midLeft;
                        sR += currentLine[x - (2 * bytesPerPixel) + 2] * midLeft;
                        //Up left pixel.
                        sB += currentLine[x - (stride - (2 * bytesPerPixel))] * midLeft;
                        sG += currentLine[x - (stride - (2 * bytesPerPixel)) + 1] * midLeft;
                        sR += currentLine[x - (stride - (2 * bytesPerPixel)) + 2] * midLeft;
                    }
                }
                //Down part of matrix
                if (y <= (heightInPixel - 4))
                {
                    //Down middle pixel.
                    sB += currentLine[x + (2 * stride)] * bottomMid;
                    sG += currentLine[x + (2 * stride) + 1] * bottomMid;
                    sR += currentLine[x + (2 * stride) + 2] * bottomMid;

                    //Left middle left pixel.
                    if (x >= bytesPerPixel)
                    {
                        sB += currentLine[x + ((2 * stride) - bytesPerPixel)] * bottomMid;
                        sG += currentLine[x + ((2 * stride) - bytesPerPixel) + 1] * bottomMid;
                        sR += currentLine[x + ((2 * stride) - bytesPerPixel) + 2] * bottomMid;
                    }

                    //Right middle down pixel.
                    if (x <= widthInBytes - (3 * bytesPerPixel))
                    {
                        sB += currentLine[x + stride + (2 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + stride + (2 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + stride + (2 * bytesPerPixel) + 2] * midRight;
                    }
                } else if (y == (heightInPixel - 3))
                {
                    //Right middle down pixel.
                    if (x <= widthInBytes - (3 * bytesPerPixel))
                    {
                        sB += currentLine[x + stride + (2 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + stride + (2 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + stride + (2 * bytesPerPixel) + 2] * midRight;
                    }
                }
            }
            if (matrixDim == 7)
            {
                //Up part of matrix.
                if (y >= 3)
                {   //Top middle pixel.
                    sB += currentLine[x - (3 * stride)] * topMid;
                    sG += currentLine[x - (3 * stride) + 1] * topMid;
                    sR += currentLine[x - (3 * stride) + 2] * topMid;

                    if (x < widthInBytes - bytesPerPixel)//x <= widthInBytes - (2 * bytesPerPixel)
                    {
                        //Top middle right pixel.
                        sB += currentLine[x - (3 * stride) + bytesPerPixel] * topMid;
                        sG += currentLine[x - (3 * stride) + bytesPerPixel + 1] * topMid;
                        sR += currentLine[x - (3 * stride) + bytesPerPixel + 2] * topMid;
                    }
                    if (x < widthInBytes - (3 * bytesPerPixel))//x <= widthInBytes - (4 * bytesPerPixel)
                    {
                        //Right middle pixel.
                        sB += currentLine[x + (3 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + (3 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + (3 * bytesPerPixel) + 2] * midRight;
                    }
                    //Left middle and up pixel.
                    if (x > 3 * bytesPerPixel)//x >= (3 * bytesPerPixel)
                    {
                        //Left middle.
                        sB += currentLine[x - (3 * bytesPerPixel)] * midLeft;
                        sG += currentLine[x - (3 * bytesPerPixel) + 1] * midLeft;
                        sR += currentLine[x - (3 * bytesPerPixel) + 2] * midLeft;
                        //Up pixel.
                        sB += currentLine[x - (stride - (3 * bytesPerPixel))] * midLeft;
                        sG += currentLine[x - (stride - (3 * bytesPerPixel)) + 1] * midLeft;
                        sR += currentLine[x - (stride - (3 * bytesPerPixel)) + 2] * midLeft;
                    }
                }
                else if (y == 2) //Matrix without top pixels.
                {
                    if (x < widthInBytes - (3 * bytesPerPixel))//x <= widthInBytes - (4 * bytesPerPixel)
                    {
                        //Right middle pixel.
                        sB += currentLine[x + (3 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + (3 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + (3 * bytesPerPixel) + 2] * midRight;
                    }
                    //Left middle and up pixel.
                    if (x > (2 * bytesPerPixel))//or x >= (3 * bytesPerPixel)
                    {
                        //Left middle
                        sB += currentLine[x - (3 * bytesPerPixel)] * midLeft;
                        sG += currentLine[x - (3 * bytesPerPixel) + 1] * midLeft;
                        sR += currentLine[x - (3 * bytesPerPixel) + 2] * midLeft;
                        //Up pixel
                        sB += currentLine[x - (stride - (3 * bytesPerPixel))] * midLeft;
                        sG += currentLine[x - (stride - (3 * bytesPerPixel)) + 1] * midLeft;
                        sR += currentLine[x - (stride - (3 * bytesPerPixel)) + 2] * midLeft;
                    }
                }
                //Down part of matrix
                if (y <= (heightInPixel - 4))
                {
                    //Down middle pixel.
                    sB += currentLine[x + (3 * stride)] * bottomMid;
                    sG += currentLine[x + (3 * stride) + 1] * bottomMid;
                    sR += currentLine[x + (3 * stride) + 2] * bottomMid;

                    //Down left middle pixel.
                    if (x >= bytesPerPixel)
                    {
                        sB += currentLine[x + ((3 * stride) - bytesPerPixel)] * bottomMid;
                        sG += currentLine[x + ((3 * stride) - bytesPerPixel) + 1] * bottomMid;
                        sR += currentLine[x + ((3 * stride) - bytesPerPixel) + 2] * bottomMid;
                    }

                    //Right middle down pixel.
                    if (x < widthInBytes - (3 * bytesPerPixel))//x <= widthInBytes - (4 * bytesPerPixel)
                    {
                        sB += currentLine[x + stride + (3 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + stride + (3 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + stride + (3 * bytesPerPixel) + 2] * midRight;
                    }
                }
                else if (y == (heightInPixel - 3) || y == (heightInPixel - 2)) //Take right middle down pixel even if last two rows of matrix are unreachable.
                {
                    //Right middle down pixel.
                    if (x < widthInBytes - (3 * bytesPerPixel))//x <= widthInBytes - (4 * bytesPerPixel)
                    {
                        sB += currentLine[x + stride + (3 * bytesPerPixel)] * midRight;
                        sG += currentLine[x + stride + (3 * bytesPerPixel) + 1] * midRight;
                        sR += currentLine[x + stride + (3 * bytesPerPixel) + 2] * midRight;
                    }
                }
            }
            if (matrixDim >= 3)
            {
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

                }
                else if (y == heightInPixel-1) // last pixel row
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

                sB += (pBTopMid * topMid) + (pBMidLeft * midLeft) + (pBMidRight * midRight) + (pBBottomMid * bottomMid) + (pBCentar * n);
                sG += (pGTopMid * topMid) + (pGMidLeft * midLeft) + (pGMidRight * midRight) + (pGBottomMid * bottomMid) + (pGCentar * n);
                sR += (pRTopMid * topMid) + (pRMidLeft * midLeft) + (pRMidRight * midRight) + (pRBottomMid * bottomMid) + (pRCentar * n);

            }

            sB = sB / factor;
            sG = sG / factor;
            sR = sR / factor;

            //Test
            //int[] tmp = { sB, sG, sR };
            //int[] correctPixel = NormalizeEdgePixels(tmp);
            //sB = tmp[0];
            //sG = tmp[1];
            //sR = tmp[2];
            //Test

            if (sB > 255)
                sB = 255;
            else if (sB <0)
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
        
        /// <summary>
        /// Experimantal fuction for normalizing edge pixels.
        /// </summary>
        /// <param name="s">Sum array.</param>
        /// <returns>Normalized sum array.</returns>
        private static int[] NormalizeEdgePixels(int[] s)
        {
            float max = s.Max();
            float d = max / 256;

            s[0] = (int)((double)s[0] / d);
            s[1] = (int)((double)s[1] / d);
            s[2] = (int)((double)s[2] / d);

            return s;
        }
    }
}
