using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class EdgeEnhenceFilter
    {

        public static unsafe void EdgeEnhenceUnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage,int nThreshold)
        {
            //int nThreshold = 500;
            //------------------------------- DestinationImageParametars ---------------------------------------------
            destinationImage.Lock();
            byte* PtrFirstPixelNew = (byte*)destinationImage.BackBuffer;
            //------------------------------- SourceImageParametars ----------------------------------------
            sourceImage.Lock();
            byte* PtrFirstPixel = (byte*)sourceImage.BackBuffer;
            int heightInPixels = sourceImage.PixelHeight;
            int bytesPerPixel = sourceImage.Format.BitsPerPixel / 8;
            int widthInBytes = sourceImage.PixelWidth * bytesPerPixel;
            int stride = sourceImage.BackBufferStride;
            //
            int[,] matrixB = new int[3, 3];
            int[,] matrixG = new int[3, 3];
            int[,] matrixR = new int[3, 3];
            //
            byte* currentLine;
            byte* upCurrentLine;
            byte* downCurrentLine;
            byte* currentLineNew;

            for (int y = 1; y < heightInPixels - 1; y++)
            {
                currentLine = PtrFirstPixel + (y * stride);
                upCurrentLine = PtrFirstPixel + ((y - 1) * stride);
                downCurrentLine = PtrFirstPixel + ((y + 1) * stride);

                currentLineNew = PtrFirstPixelNew + (y * stride);

                for (int x = bytesPerPixel; x < widthInBytes - bytesPerPixel; x = x + bytesPerPixel)
                {
                    #region Take matrix
                    matrixB[0, 0] = upCurrentLine[x - bytesPerPixel];
                    matrixG[0, 0] = upCurrentLine[(x - bytesPerPixel) + 1];
                    matrixR[0, 0] = upCurrentLine[(x - bytesPerPixel) + 2];

                    matrixB[0, 1] = upCurrentLine[x];
                    matrixG[0, 1] = upCurrentLine[x + 1];
                    matrixR[0, 1] = upCurrentLine[x + 2];
                    
                    matrixB[0, 2] = upCurrentLine[x + bytesPerPixel];
                    matrixG[0, 2] = upCurrentLine[x + bytesPerPixel + 1];
                    matrixR[0, 2] = upCurrentLine[x + bytesPerPixel + 2];
                    //
                    matrixB[1, 0] = currentLine[x - bytesPerPixel];
                    matrixG[1, 0] = currentLine[(x - bytesPerPixel) + 1];
                    matrixR[1, 0] = upCurrentLine[(x - bytesPerPixel) + 2];

                    matrixB[1, 1] = currentLine[x];
                    matrixG[1, 1] = currentLine[x + 1];
                    matrixR[1, 1] = currentLine[x + 2];

                    matrixB[1, 2] = currentLine[x + bytesPerPixel];
                    matrixG[1, 2] = currentLine[x + bytesPerPixel + 1];
                    matrixR[1, 2] = currentLine[x + bytesPerPixel + 2];
                    //
                    matrixB[2, 0] = downCurrentLine[x - bytesPerPixel];
                    matrixG[2, 0] = downCurrentLine[(x - bytesPerPixel) + 1];
                    matrixR[2, 0] = downCurrentLine[(x - bytesPerPixel) + 2];

                    matrixB[2, 1] = downCurrentLine[x];
                    matrixG[2, 1] = downCurrentLine[x + 1];
                    matrixR[2, 1] = downCurrentLine[x + 2];

                    matrixB[2, 2] = downCurrentLine[x + bytesPerPixel];
                    matrixG[2, 2] = downCurrentLine[x + bytesPerPixel + 1];
                    matrixR[2, 2] = downCurrentLine[x + bytesPerPixel + 2];
                    //
                    #endregion End

                    int[] tmpB = new int[4];
                    int[] tmpG = new int[4];
                    int[] tmpR = new int[4];

                    #region Calculate 
                    ///////// C-G 
                    //Blue
                    tmpB[0] = Math.Abs(matrixB[0, 2] - matrixB[2, 0]);
                    //Green
                    tmpG[0] = Math.Abs(matrixG[0, 2] - matrixG[2, 0]);
                    //Red
                    tmpR[0] = Math.Abs(matrixR[0, 2] - matrixR[2, 0]);
                    ///////// I-A 
                    //Blue
                    tmpB[1] = Math.Abs(matrixB[2, 2] - matrixB[0, 0]);
                    //Gren
                    tmpG[1] = Math.Abs(matrixG[2, 2] - matrixG[0, 0]);
                    //Red
                    tmpR[1] = Math.Abs(matrixR[2, 2] - matrixR[0, 0]);
                    ///////// H-B
                    //Blue
                    tmpB[2] = Math.Abs(matrixB[2, 1] - matrixB[0, 1]);
                    //Gren
                    tmpG[2] = Math.Abs(matrixG[2, 1] - matrixG[0, 1]);
                    //Red
                    tmpR[2] = Math.Abs(matrixR[2, 1] - matrixR[0, 1]);
                    ///////// F-D
                    //Blue
                    tmpB[3] = Math.Abs(matrixB[1, 2] - matrixB[1, 0]);
                    //Gren
                    tmpG[3] = Math.Abs(matrixG[1, 2] - matrixG[1, 0]);
                    //Red
                    tmpR[3] = Math.Abs(matrixR[1, 2] - matrixR[1, 0]);
                    /////////
                    #endregion 

                    int nBMax = tmpB.Max();
                    int nGMax = tmpR.Max();
                    int nRMax = tmpR.Max();

                    if (nBMax > nThreshold && nBMax > matrixB[1, 1])
                    {
                        currentLineNew[x] = (byte)(nBMax);
                    }
                    if (nGMax > nThreshold && nGMax > matrixG[1, 1])
                    {
                        currentLineNew[x + 1] = (byte)(nGMax);
                    }
                    if (nRMax > nThreshold && nRMax > matrixR[1, 1])
                    {
                        currentLineNew[x + 2] = (byte)(nRMax);
                    }
                }
            }
            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }
    }
}
