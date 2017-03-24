﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOpetarions
{
    public class ReseizeImage
    {
        public static unsafe WriteableBitmap LinearReseizeImageUnsafe(WriteableBitmap imageData, int newWidth, int newHeight)
        {
            //Creating new writableBitmap with same parametars as the old one exept new resolution
            WriteableBitmap newWritableBitmap = new WriteableBitmap(newWidth,newHeight,imageData.DpiX,imageData.DpiY,imageData.Format,imageData.Palette);

            //Reseize factors
            double nXFactor = (double)imageData.PixelWidth / (double)newWidth;
            double nYFactor = (double)imageData.PixelHeight / (double)newHeight;

            //------------------------------- NewImageParametars ----------------------------------------
            //Reserve the back buffer for updated
            newWritableBitmap.Lock();
            //Pointer to the back buffer of the new image.(IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixelNew = (byte*)newWritableBitmap.BackBuffer;
            //Height in pixels
            int heightInPixelsNew = newWritableBitmap.PixelHeight;
            //Number of bits per pixel divided by eight to get number of bytes per pixel (same for the original)
            int bytesPerPixel = newWritableBitmap.Format.BitsPerPixel / 8;
            //Number of bytes per one row (scan line)
            int widthInBytesNew = newWritableBitmap.PixelWidth * bytesPerPixel;
            //Number of bytes taken to store one row of an image
            int strideNew = newWritableBitmap.BackBufferStride;

            //------------------------------- OriginalImageParametars ----------------------------------------
            //Make sure that other threads can't modify original image during resizeign.
            imageData.Lock();
            //Pointer to the back buffer of the original image.(IntPtr pBackBuffer = imageData.BackBuffer;)
            byte* PtrFirstPixelOriginal = (byte*)imageData.BackBuffer;
            //Number of bytes taken to store one row of an image
            int strideOriginal = imageData.BackBufferStride;

            int x_newPixel;
            for(int y=0; y<heightInPixelsNew; y++)
            {
                byte* currentLine = PtrFirstPixelNew + (y * strideNew);
                //x_newPixel because x in for is increment by bytesPerPixel
                x_newPixel = 0;
                for (int x = 0; x < widthInBytesNew; x = x + bytesPerPixel)
                {
                    //Calculating which pixel to take from the original
                    int x_originalPixel = (int)Math.Floor(x_newPixel * nXFactor);
                    x_originalPixel = x_originalPixel * bytesPerPixel;
                    int y_originalPixel = (int)Math.Floor(y * nYFactor);

                    x_newPixel++;
                    //Pointer to the line of the original image from which we are going to take the pixel (with modified y_originalPixel)
                    byte* currentLineOriginal = PtrFirstPixelOriginal + (y_originalPixel * strideOriginal);


                    int blue = currentLineOriginal[x_originalPixel];
                    int green = currentLineOriginal[x_originalPixel + 1];
                    int red = currentLineOriginal[x_originalPixel + 2];

                    currentLine[x] = (byte)blue;
                    currentLine[x + 1] = (byte)green;
                    currentLine[x + 2] = (byte)red;
                }
            }

            // Release the back buffer and make it available for display. (This method must be caled from UI thread)
            imageData.Unlock();

            //The Unlock method decrements the lock count.When the lock count reaches 0, a render pass is requested if the AddDirtyRect method has been called.
            // Specify the area of the bitmap that changed. (This method must be caled from UI thread)
            newWritableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, newWritableBitmap.PixelWidth, newWritableBitmap.PixelHeight));
            newWritableBitmap.Unlock();
            //Return resiezed image
            return newWritableBitmap;
        }      
    }
}


//The Unlock method decrements the lock count.When the lock count reaches 0, a render pass is requested if the AddDirtyRect method has been called.
// Specify the area of the bitmap that changed. (This method must be caled from UI thread)
//imageData.AddDirtyRect(new System.Windows.Int32Rect(0, 0, imageData.PixelWidth, imageData.PixelHeight));