using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoEditor.Utility
{
//+-------------------------------------------------------------+--+------------------+
//|                            Header                           |  |  Default values  |
//+-------------------------------------------------------------+--+------------------+
//| Offset(B) | Size(B) | Purpuse                               |  | Color Depth | 24 |
//+-----------+---------+---------------------------------------+--+-------------+----+
//| 0         | 4       | Pixel Width                           |  | Header Size | 9  |
//+-----------+---------+---------------------------------------+--+-------------+----+
//| 4         | 4       | Pixel Height                          |  |             |    |
//+-----------+---------+---------------------------------------+--+-------------+----+
//| 8         | 1       | Spared Channel(Red 0, Green 1, Blue 2)|  |             |    |
//+-----------+---------+---------------------------------------+--+-------------+----+
//|                          ImageData                          |  |             |    |
//+-------------------------------------------------------------+--+-------------+----+
//| 9         | X       | Red Channel Data                      |  |             |    |
//+-----------+---------+---------------------------------------+--+-------------+----+
//| X         | X       | Green Channel Data                    |  |             |    |
//+-----------+---------+---------------------------------------+--+-------------+----+
//| X         | X       | Blue Channel Data                     |  |             |    |
//+-----------+---------+---------------------------------------+--+-------------+----+

    public class DownsampledImage
    {
        private const int headerSize = 9;
        /// <summary>
        /// Image height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Image width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Old width divided by two and rounded on bigger value.
        /// </summary>
        public int DownSampledWidth { get; private set; }
        /// <summary>
        /// Image data as byte array.
        /// </summary>
        public byte[] ImageData { get; private set; }
        private WriteableBitmap image = null;
        /// <summary>
        /// Return current downsampled image as WritableBitmap.
        /// <para>This propery can take different amount of time for returning value, 
        /// time may depend on whether the image is created or not.</para>
        /// </summary>
        public WriteableBitmap Image
        {
            get
            {
                if (image == null)
                    UpSampleImage();
                return image;
            }
            private set { image = value; }
        }
        /// <summary>
        /// Heder length in bytes.
        /// </summary>
        public int HederLengthInBytes { get; private set; }
        /// <summary>
        /// With this value you can define which channel you want to keep in original quality.
        /// Other two channels will be downsampled.
        /// </summary>
        public SpareChannel SparedChannel { get; private set; }
        /// <summary>
        /// This constructor will create downsampled image from passed parametars.
        /// </summary>
        /// <param name="image">Image for downsampling.</param>
        /// <param name="spareChannel">With this value you can define which channel you want to keep in original quality.
        /// <para>Other two channels will be downsampled.</para></param>
        public DownsampledImage(WriteableBitmap image,SpareChannel spareChannel)
        {
            SparedChannel = spareChannel;
            Height = image.PixelHeight;
            Width = image.PixelWidth;
            DownSampledWidth = (int)Math.Ceiling((double)Width / 2.0);
            DownSampleImage(image);
            
        }
        /// <summary>
        /// This constructor will create downsampled image from passed parametars.
        /// </summary>
        /// <param name="imageInBytes">Image data in bytes.</param>
        public DownsampledImage(byte[] imageInBytes)
        {
            ImageData = imageInBytes;
            UpSampleImage();
        }

        private unsafe void DownSampleImage(WriteableBitmap image)
        {
            image.Lock();
            byte* PtrFirstPixel = (byte*)image.BackBuffer;
            int heightInPixels = image.PixelHeight;
            int bytesPerPixel = image.Format.BitsPerPixel / 8;
            int widthInBytes = image.PixelWidth * bytesPerPixel;
            int stride = image.BackBufferStride;


            int lengthInBytesPerChannel = Width * Height;
            int lengthInBytesDownsampledPerChannel = DownSampledWidth * Height;
            int imageDataLength = headerSize + (lengthInBytesDownsampledPerChannel * 2) + lengthInBytesPerChannel;


            ImageData = new byte[imageDataLength];

            //Fix imageData array in memory, actually prevent GC from moving imageData somewhere else in memory.
            fixed (byte* imageDataPointer = ImageData)
            {
                //Fill Header info
                //Width 
                imageDataPointer[0] = (byte)  Width;
                imageDataPointer[1] = (byte) (Width >> 8);
                imageDataPointer[2] = (byte) (Width >> 16);
                imageDataPointer[3] = (byte) (Width >> 24);
                //Height
                imageDataPointer[4] = (byte)Height;
                imageDataPointer[5] = (byte)(Height >> 8);
                imageDataPointer[6] = (byte)(Height >> 16);
                imageDataPointer[7] = (byte)(Height >> 24);
                //Spared Channel
                imageDataPointer[8] = (byte)SparedChannel;// Red 0, Green 1, Blue 2
                //
                //Prepare pointers
                byte* imageDataPointerRed = imageDataPointer + headerSize;
                byte* imageDataPointerGreen;
                byte* imageDataPointerBlue;

                switch (SparedChannel)
                {
                    case SpareChannel.Red:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesDownsampledPerChannel;
                        break;
                    case SpareChannel.Green:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesDownsampledPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesPerChannel;
                        break;
                    case SpareChannel.Blue:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesDownsampledPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesDownsampledPerChannel;
                        break;
                    default://Because of compiler error. Default should never execute.
                        imageDataPointerBlue = imageDataPointerGreen = imageDataPointerRed;
                        break;
                }

                int indexX;
                for(int y = 0; y < Height; y++)
                {
                    byte* currentLine = PtrFirstPixel + (y * stride);
                    indexX = 0;
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        switch (SparedChannel)
                        {
                            case SpareChannel.Red:
                                *imageDataPointerRed = currentLine[x + 2];
                                imageDataPointerRed++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    *imageDataPointerBlue = currentLine[x];
                                    *imageDataPointerGreen = currentLine[x + 1];
                                    imageDataPointerBlue++;
                                    imageDataPointerGreen++;
                                }
                                break;
                            case SpareChannel.Green:
                                *imageDataPointerGreen = currentLine[x + 1];
                                imageDataPointerGreen++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    *imageDataPointerBlue = currentLine[x];
                                    *imageDataPointerRed = currentLine[x + 2];
                                    imageDataPointerBlue++;
                                    imageDataPointerRed++;
                                }
                                break;
                            case SpareChannel.Blue:
                                *imageDataPointerBlue = currentLine[x + 1];
                                imageDataPointerBlue++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    *imageDataPointerGreen = currentLine[x + 1];
                                    *imageDataPointerRed = currentLine[x + 2];
                                    imageDataPointerGreen++;
                                    imageDataPointerRed++;
                                }
                                break;
                        }
                        indexX++;
                    }
                }
                image.Unlock();
            }
        }
        private unsafe void UpSampleImage()
        {
            //Fix imageData array in memory, actually prevent GC from moving imageData somewhere else in memory.
            fixed (byte* imageDataPointer = ImageData)
            {
                //Read Header info
                //Width 
                Width = imageDataPointer[0];
                Width |= (imageDataPointer[1] << 8);
                Width |= (imageDataPointer[2] << 16);
                Width |= (imageDataPointer[3] << 24);
                //Height
                Height = imageDataPointer[4];
                Height |= (imageDataPointer[5] << 8);
                Height |= (imageDataPointer[6] << 16);
                Height |= (imageDataPointer[7] << 24);
                //Spared Channel
                int tmp = imageDataPointer[8];// Red 0, Green 1, Blue 2
                if (tmp == 0)
                    SparedChannel = SpareChannel.Red;
                else if (tmp == 1)
                    SparedChannel = SpareChannel.Green;
                else
                    SparedChannel = SpareChannel.Blue;

                DownSampledWidth = (int)Math.Ceiling((double)Width / 2.0);
                //
                //Create new image based on imageData information.
                image = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Rgb24, null);
                image.Lock();
                byte* PtrFirstPixel = (byte*)image.BackBuffer;
                int heightInPixels = image.PixelHeight;
                int bytesPerPixel = image.Format.BitsPerPixel / 8;
                int widthInBytes = image.PixelWidth * bytesPerPixel;
                int stride = image.BackBufferStride;


                int lengthInBytesPerChannel = Width * Height;
                int lengthInBytesDownsampledPerChannel = DownSampledWidth * Height;
                int imageDataLength = headerSize + (lengthInBytesDownsampledPerChannel * 2) + lengthInBytesPerChannel;
                //
                //Prepare pointers
                byte* imageDataPointerRed = imageDataPointer + headerSize;
                byte* imageDataPointerGreen;
                byte* imageDataPointerBlue;

                switch (SparedChannel)
                {
                    case SpareChannel.Red:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesDownsampledPerChannel;
                        break;
                    case SpareChannel.Green:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesDownsampledPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesPerChannel;
                        break;
                    case SpareChannel.Blue:
                        imageDataPointerGreen = imageDataPointerRed + lengthInBytesDownsampledPerChannel;
                        imageDataPointerBlue = imageDataPointerGreen + lengthInBytesDownsampledPerChannel;
                        break;
                    default://Because of compiler error. Default should never execute.
                        imageDataPointerBlue = imageDataPointerGreen = imageDataPointerRed;
                        break;
                }


                int indexX;
                for (int y = 0; y < Height; y++)
                {
                    byte* currentLine = PtrFirstPixel + (y * stride);
                    indexX = 0;
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        switch (SparedChannel)
                        {
                            case SpareChannel.Red:
                                currentLine[x + 2] = *imageDataPointerRed;
                                imageDataPointerRed++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    currentLine[x] = *imageDataPointerBlue;
                                    currentLine[x + 1] = * imageDataPointerGreen;
                                    imageDataPointerBlue++;
                                    imageDataPointerGreen++;
                                }
                                break;
                            case SpareChannel.Green:
                                currentLine[x + 1] = * imageDataPointerGreen;
                                imageDataPointerGreen++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    currentLine[x] = *imageDataPointerBlue;
                                    currentLine[x + 2] = *imageDataPointerRed;
                                    imageDataPointerBlue++;
                                    imageDataPointerRed++;
                                }
                                break;
                            case SpareChannel.Blue:
                                currentLine[x] = * imageDataPointerBlue;
                                imageDataPointerBlue++;
                                if (indexX % 2 == 0) //Take Value from channel
                                {
                                    currentLine[x + 1] = *imageDataPointerGreen;
                                    currentLine[x + 2] = *imageDataPointerRed;
                                    imageDataPointerGreen++;
                                    imageDataPointerRed++;
                                }
                                break;
                        }
                        indexX++;
                    }
                }
            }
            image.AddDirtyRect(new System.Windows.Int32Rect(0, 0, Width, Height));
            image.Unlock();
        }
        public enum SpareChannel
        {
            Red,Green,Blue
        }
    }
}
