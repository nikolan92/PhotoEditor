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

        public static unsafe void PixelateImageUnsafeWithCopy(WriteableBitmap sourceImage, WriteableBitmap destinationImage, int pixelSize)
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
            int widthInPixels = sourceImage.PixelWidth;
            int stride = sourceImage.BackBufferStride;

            //int stepX, stepY;
            //stepX = widthInPixels/pixelSize;
            //stepY = heightInPixels/pixelSize;
            int pixelSizeInBytesPerPixel;
            int aB, aG, aR, divider,tmpDivider;
            tmpDivider = divider = pixelSize*pixelSize;
            aB = aG = aR = 0;

            int counterX, counterY;
            counterY = counterX = 0;

            int pixelSizeY = pixelSize;
            for(int y = 0; y < heightInPixels; y = y + pixelSize)
            {
                counterY++;

                byte* currentLine = PtrFirstPixel + (y * stride);
                byte* currentLineNew = PtrFirstPixelNew + (y * stride);

                byte* tmpCurrentLine = currentLine;
                byte* tmpCurrentLineNew = currentLineNew;


                //Reset for x iteration
                counterX = 0;
                pixelSizeInBytesPerPixel = bytesPerPixel * pixelSize;
                //If pixelSizeY is changed divider is also changed (must be here)
                tmpDivider = divider;// restore old divider

                //If last line is reached then see how mouch is left and change Y boundary
                if (pixelSize * counterY > heightInPixels)
                {
                    pixelSizeY = heightInPixels - (pixelSize * (counterY - 1));
                    tmpDivider = pixelSize * pixelSizeY;
                }

                for (int x = 0; x < widthInPixels; x = x + pixelSize)
                {
                    counterX++;
                    aB = aG = aR = 0;

                    currentLine = tmpCurrentLine;
                    currentLineNew = tmpCurrentLineNew;

                    currentLine += bytesPerPixel * x;
                    currentLineNew += bytesPerPixel * x;

                    //If last pixel in line is reached then see how mouch is left and change Y boundary
                    if (pixelSizeInBytesPerPixel * counterX > widthInBytes)
                    {
                        pixelSizeInBytesPerPixel = widthInBytes - (pixelSizeInBytesPerPixel * (counterX - 1));
                        tmpDivider = (pixelSizeInBytesPerPixel / bytesPerPixel) * pixelSizeY;
                    }

                    for (int i = 0; i < pixelSizeY; i++)
                    {
                        for (int j = 0; j < pixelSizeInBytesPerPixel; j = j + bytesPerPixel)
                        {
                            aB += currentLine[j];
                            aG += currentLine[j + 1];
                            aR += currentLine[j + 2];
                        }
                        currentLine += stride;
                    }
                    aB = aB/tmpDivider;
                    aG = aG/tmpDivider;
                    aR = aR/tmpDivider;
                    //Copy new pixel value.
                    for (int i = 0; i < pixelSizeY; i++)
                    {   
                        for (int j = 0; j < pixelSizeInBytesPerPixel; j = j + bytesPerPixel)
                        {
                            currentLineNew[j] = (byte)aB;
                            currentLineNew[j + 1] = (byte)aG;
                            currentLineNew[j + 2] = (byte)aR;
                        }
                        currentLineNew += stride;
                    }
                }
            }

            sourceImage.Unlock();
            destinationImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight));
            destinationImage.Unlock();
        }
    }
}
