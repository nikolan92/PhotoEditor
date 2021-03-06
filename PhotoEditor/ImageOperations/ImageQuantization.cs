﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.ImageOperations
{
    public class ImageQuantization
    {
        private WriteableBitmap imageForQuantization;
        private WriteableBitmap newQuantizedImage;
        private int heightInPixels;
        private int bytesPerPixel;
        private int widthInBytes;
        private int stride;
        private IntPtr backBuffer;
        private IntPtr backBufferNew;

        /// <summary>
        /// Initializes a new instance of the ImageQuantization class with the specified parameters.
        /// </summary>
        /// <param name="imageForQuantization">Image for quantization.</param>
        public ImageQuantization(WriteableBitmap imageForQuantization)
        {
            this.imageForQuantization = imageForQuantization;

            heightInPixels = imageForQuantization.PixelHeight;
            bytesPerPixel = imageForQuantization.Format.BitsPerPixel / 8;
            widthInBytes = imageForQuantization.PixelWidth * bytesPerPixel;
            stride = imageForQuantization.BackBufferStride;
            backBuffer = imageForQuantization.BackBuffer;


            newQuantizedImage = new WriteableBitmap(imageForQuantization.PixelWidth, imageForQuantization.PixelHeight,
            imageForQuantization.DpiX, imageForQuantization.DpiY, imageForQuantization.Format, imageForQuantization.Palette);
            backBufferNew = newQuantizedImage.BackBuffer;
        }
        /// <summary>
        /// Creating new image with limited number of colors, using MedianCut algorithm.
        /// </summary>
        /// <returns></returns>
        public async Task<WriteableBitmap> MedianCutAsync(int maxColors)
        {
            imageForQuantization.Lock();
            newQuantizedImage.Lock();
            // Lock and Unlock must be called from UI thread!

            await Task.Factory.StartNew(() => {
                MyColor[] repColors = FindRepresentativeColors(maxColors);
                QuantizeImageUnsafe(repColors);
            });
            // Lock and Unlock must be called from UI thread!
            imageForQuantization.Unlock();
            newQuantizedImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, newQuantizedImage.PixelWidth, newQuantizedImage.PixelHeight));
            newQuantizedImage.Unlock();
            return newQuantizedImage;
        }
        /// <summary>
        /// Creating new image with limited number of colors, using the most frequently colors from old one.
        /// </summary>
        /// <returns></returns>
        public async Task<WriteableBitmap> MyAlgorithmAsync(int maxColors)
        {
            newQuantizedImage.Lock();
            imageForQuantization.Lock();
            // Lock and Unlock must be called from UI thread!

            await Task.Factory.StartNew(() => {
            Dictionary<int, MyColor> originalColors = FindOriginalColorUnsafe();
                MyColor[] repColors = FindMostFreqColorOnImage(originalColors,maxColors);
                QuantizeImageUnsafe(repColors);
            });

            // Lock and Unlock must be called from UI thread!
            imageForQuantization.Unlock();
            newQuantizedImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, newQuantizedImage.PixelWidth, newQuantizedImage.PixelHeight));
            newQuantizedImage.Unlock();
            return newQuantizedImage;
        }
        /// <summary>
        /// Going trought dictonary and collecting 256 most frequently colors from original image.
        /// </summary>
        /// <param name="originalColors">All colors from original image.</param>
        /// <returns></returns>
        private  MyColor[] FindMostFreqColorOnImage(Dictionary<int,MyColor> originalColors,int maxColors)
        {
                MyColor max = new MyColor(0, 0, 0);
                MyColor[] newColors = new MyColor[maxColors];

                int i = 0;
                int key = 0;
                while (i < maxColors)
                {
                    max.Count = 0;
                    foreach (var color in originalColors)
                    {
                        if (color.Value.Count > max.Count)
                        {
                            max = color.Value;
                            key = color.Key;//item for remove
                        }
                    }

                    originalColors.Remove(key);
                    newColors[i] = max;
                    i++;
                }

                return newColors;
        }
        /// <summary>
        /// Using old image to create new one with limited number of colors.
        /// <para>Importan: You must call Lock() on WriteableBitmap object before calling this method!</para>
        /// </summary>
        /// <param name="newColors">Colors for new image.</param>
        /// <param name="backBuffer">Back baffer of original image.</param>
        /// <param name="backBufferNew">Back baffer of new image.</param>
        /// <returns></returns>
        private unsafe void QuantizeImageUnsafe(MyColor[] newColors)
        {

            byte* PtrFirstPixelNew = (byte*)backBufferNew;
            byte* PtrFirstPixel = (byte*)backBuffer;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                byte* currentLineNew = PtrFirstPixelNew + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    byte b = currentLine[x];
                    byte g = currentLine[x + 1];
                    byte r = currentLine[x + 2];
                    //Finding closest color for replace original one.
                    int index = 0;
                    float closest = DistanceToRGB(r, g, b, newColors[0].rgb[0], newColors[0].rgb[1], newColors[0].rgb[2]);
                    for (int i = 1; i < newColors.Length; i++)
                    {
                        float temp = DistanceToRGB(r, g, b, newColors[i].rgb[0], newColors[i].rgb[1], newColors[i].rgb[2]);
                        if (closest > temp)
                        {
                            closest = temp;
                            index = i;
                        }
                    }
                    //
                    currentLineNew[x] = (byte)newColors[index].rgb[2];
                    currentLineNew[x + 1] = (byte)newColors[index].rgb[1];
                    currentLineNew[x + 2] = (byte)newColors[index].rgb[0];
                }
            }
        }

        /// <summary>
        /// The main loop to subdivide the large colour space into the required amount 
        /// of smaller boxes. Return the array of the average colour for each of these boxes.
        /// <para>Importan: You must call Lock() on WriteableBitmap object before calling this method!</para>
        /// </summary>
        /// <param name="maxColor">Number of new colors.</param>
        /// <param name="PtrFirstPixel">BackBuffer of image.</param>
        /// <returns></returns>
        private MyColor[] FindRepresentativeColors(int maxColor)
        {
            Dictionary<int, MyColor> originalColors = FindOriginalColorUnsafe();

            if (originalColors.Count <= maxColor)
            {
                //num of colours is less than or equal to the max num in the orig image
                //so simply return the colour in an int array
                MyColor[] toReturn = new MyColor[originalColors.Count];
                int index = 0;
                foreach (var color in originalColors)
                {
                    toReturn[index] = color.Value;
                    index++;
                }
                return toReturn;
            }
            else
            {
                //otherwise subdivide the box of colours untl the required number 
                //has been reached
                List<ColorBox> colorBoxes = new List<ColorBox>();
                ColorBox first = new ColorBox(originalColors, 0);
                colorBoxes.Add(first);
                int k = 1;
                bool done = false;

                while (k < maxColor && !done)
                {
                    ColorBox next = FindColorBoxToSplit(colorBoxes);
                    if (!next.Equals(null))
                    {
                        ColorBox[] boxes = SplitBox(next);

                        colorBoxes.Remove(next);

                        colorBoxes.Add(boxes[0]);
                        colorBoxes.Add(boxes[1]);
                        k++;
                    }
                    else
                    {
                        done = true;
                    }
                }

                //get the average colour from each of the boxes in the arraylist
                MyColor[] averageColors = new MyColor[colorBoxes.Count];
                int index = 0;
                foreach (var colorBox in colorBoxes)
                {
                    float[] color = AverageColor(colorBox);
                    averageColors[index] = new MyColor(color[0], color[1], color[2]);
                    index++;
                }
                return averageColors;
            }
        }
        /// <summary>
        /// Takes the list of all candidate boxes and returns the one to be split next.
        /// </summary>
        /// <param name="listOfBoxes">Candidates for spliting.</param>
        /// <returns></returns>
        private ColorBox FindColorBoxToSplit(List<ColorBox> listOfBoxes)
        {
            List<ColorBox> canBeSplit = new List<ColorBox>();

            foreach(var colorBox in listOfBoxes)
            {
                if (colorBox.Colors.Count > 1)
                {
                    canBeSplit.Add(colorBox);
                }
            }
            if (canBeSplit.Count == 0)
            {
                return null;
            }
            else
            {
                ColorBox minBox = canBeSplit[0];
                int minLevel = minBox.Level;

                for (int i = 1; i < canBeSplit.Count; i++)
                {
                    ColorBox test = canBeSplit[i];
                    if (minLevel > test.Level)
                    {
                        minLevel = test.Level;
                        minBox = test;
                    }
                }
                return minBox;
            }
        }
        /// <summary>
        /// Split ColorBox in two new ColorBoxes.
        /// </summary>
        /// <param name="colorBox">ColorBox for spliting.</param>
        /// <returns>Two new ColorBox.</returns>
        private ColorBox[] SplitBox(ColorBox colorBox)
        {
            int currentLevel = colorBox.Level;
            int d = FindMaxDimension(colorBox);

            float c = 0;

            foreach (var color in colorBox.Colors)
            {
                c += color.Value.rgb[d];
            }

            float median = c / colorBox.Colors.Count;

            Dictionary<int, MyColor> left = new Dictionary<int, MyColor>();
            Dictionary<int, MyColor> right = new Dictionary<int, MyColor>();

            MyColor mc;
            foreach (var color in colorBox.Colors)
            {
                mc = color.Value;
                if (mc.rgb[d] <= median)
                {
                    left.Add(color.Key, color.Value);
                } else
                {
                    right.Add(color.Key, color.Value);
                }
            }
            ColorBox[] toReturn = new ColorBox[2];
            toReturn[0] = new ColorBox(left, currentLevel + 1);
            toReturn[1] = new ColorBox(right, currentLevel + 1);

            return toReturn;
        }
        /// <summary>
        /// Finding which color in ColorBox have bigest range.
        /// </summary>
        /// <param name="colorBox">ColorBox.</param>
        /// <returns>0 - RED, 1 - BLUE, 2 - GREEN</returns>
        private int FindMaxDimension(ColorBox colorBox)
        {
            float[] dims = new float[3];
            //the length of each is measured as the (max value - min value)
            dims[0] = colorBox.RMax - colorBox.RMin;
            dims[1] = colorBox.GMax - colorBox.GMin;
            dims[2] = colorBox.BMax - colorBox.BMin;

            float sizeMax = dims.Max();
            if (sizeMax == dims[0])
            {
                return 0;
            }
            else if (sizeMax == dims[1])
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        /// <summary>
        /// Calculate average color for given ColorBox.
        /// </summary>
        /// <param name="colorBox">ColorBox.</param>
        /// <returns>Float array red, green and blue values in the range (0.0 - 1.0).</returns>
        private float[] AverageColor(ColorBox colorBox)
        {
            Dictionary<int, MyColor> colors = colorBox.Colors;
            float[] rgb = { 0, 0, 0 };

            foreach(var color in colors)
            {
                MyColor mc = color.Value;

                rgb[0] += mc.rgb[0];//Red
                rgb[1] += mc.rgb[1];//Green
                rgb[2] += mc.rgb[2];//Blue
            }
            rgb[0] = rgb[0] / colors.Count;
            rgb[1] = rgb[1] / colors.Count;
            rgb[2] = rgb[2] / colors.Count;

            return rgb;
        }
        /// <summary>
        /// Count all colors for image passed from constructor of this class.
        /// <para>Importan: You must call Lock() on WriteableBitmap object before calling this method!</para>
        /// <param name="backBuffer">BackBuffer of image.</param>
        /// </summary>
        private unsafe Dictionary<int, MyColor> FindOriginalColorUnsafe()
        {
            Dictionary<int, MyColor> toReturn = new Dictionary<int, MyColor>();

            byte* PtrFirstPixel = (byte*)backBuffer;

            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = PtrFirstPixel + (y * stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    byte b = currentLine[x];
                    byte g = currentLine[x + 1];
                    byte r = currentLine[x + 2];

                    int color;
                    color = b << 16;
                    color |= g << 8;
                    color |= r;

                    MyColor temp;
                    if (toReturn.TryGetValue(color, out temp))
                    {
                        temp.Increment();
                    }
                    else
                    {
                        temp = new MyColor(r, g, b);
                        toReturn.Add(color, temp);
                    }
                }
            }
            return toReturn;
        }
        /// <summary>
        ///Gets the distance between two colors. 
        /// </summary>
        /// <param name="red1"></param>
        /// <param name="red2"></param>
        /// <param name="green1"></param>
        /// <param name="green2"></param>
        /// <param name="blue1"></param>
        /// <param name="blue2"></param>
        /// <returns></returns>
        private float DistanceToRGB(float red1,float red2, float green1,float green2,float blue1,float blue2)
        {
            float redDiff = Math.Abs(red1 - red2);
            float greenDiff = Math.Abs(green1 - green2);
            float blueDiff = Math.Abs(blue1 - blue2);

            return ((redDiff + greenDiff + blueDiff) / 3);
        }
    }

    /// <summary>
    /// Class to store information about one color.
    /// </summary>
    class MyColor
    {
        public int Count { get; set; }
        public float[] rgb = new float[3];
        public MyColor(float r, float g, float b)
        {
           
            rgb[0] = r;
            rgb[1] = g;
            rgb[2] = b;
            Count = 1;
        }
        public void Increment()
        {
            Count++;
        }
    }
    /// <summary>
    /// Class to store information about set of colors.
    /// </summary>
    class ColorBox
    {
        public float RMin { get; set; }
        public float RMax { get; set; }
        public float GMin { get; set; }
        public float GMax { get; set; }
        public float BMin { get; set; }
        public float BMax { get; set; }

        public int Level { get; set; }
        public Dictionary<int,MyColor> Colors{ get; private set; }
        public ColorBox(Dictionary<int,MyColor> colors, int level)
        {
            Colors = colors;
            Level = level;

            RMax = GMax = BMax = 0;
            RMin = GMin = BMin = 255;
            //Finding Min and Max range.
            foreach (var color in colors)
            {
                MyColor mc = color.Value;

                //Red
                if (mc.rgb[0] > RMax)
                    RMax = mc.rgb[0];
                if (mc.rgb[0] < RMin)
                    RMin = mc.rgb[0];
                //Blue
                if (mc.rgb[1] > BMax)
                    BMax = mc.rgb[1];
                if (mc.rgb[1] < BMin)
                    BMin = mc.rgb[0];
                //Green
                if (mc.rgb[2] > GMax)
                    GMax = mc.rgb[2];
                if (mc.rgb[2] < GMin)
                    GMin = mc.rgb[0];
            }

        }
    }
}
