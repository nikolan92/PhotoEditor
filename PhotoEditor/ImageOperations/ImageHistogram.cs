using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class ImageHistogram
    {
        public int[] RedHistogram { get; private set; }
        public int[] GreenHistogram { get; private set; }
        public int[] BlueHistogram { get; private set; }

        public int RedMax { get; private set; }
        public int GreenMax { get; private set; }
        public int BlueMax { get; private set; }
        //public int RedMin { get; private set; }
        //public int GreenMin { get; private set; }
        //public int BlueMin { get; private set; }
        public unsafe ImageHistogram(WriteableBitmap sourceAndDestinationImage)
        {
            RedHistogram = new int[256];
            GreenHistogram = new int[256];
            BlueHistogram = new int[256];
            RedMax = BlueMax = GreenMax = 0;


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
                    int blue = currentLine[x];
                    int green = currentLine[x + 1];
                    int red = currentLine[x + 2];

                    BlueHistogram[blue]++;
                    GreenHistogram[green]++;
                    RedHistogram[red]++;

                    if (red > RedMax)
                        RedMax = red;
                    if (green > GreenMax)
                        GreenMax = green;
                    if (blue > BlueMax)
                        BlueMax = blue;

                    //if (red < RedMin)
                    //    RedMin = red;
                    //if (red < GreenMin)
                    //    GreenMin = green;
                    //if (red < BlueMin)
                    //    BlueMin = blue;
                }
            });
            sourceAndDestinationImage.Unlock();
        }
        public int[] SmoothHistogram(int[] originalValues)
        {
            int[] smoothedValues = new int[originalValues.Length];

            double[] mask = new double[] { 0.25, 0.5, 0.25  };

            for (int i = 1; i < originalValues.Length - 1; i++)
            {
                double smoothedValue = 0;
                for (int j = 0; j < mask.Length; j++)
                {
                    smoothedValue += originalValues[i - 1 + j] * mask[j];
                }
                smoothedValues[i] = (int)smoothedValue;
            }

            return smoothedValues;
        }
    }
}
