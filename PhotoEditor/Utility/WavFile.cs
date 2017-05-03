using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Utility
{
    public class WavFile
    {

        private const int HEADER_OFFSET = 44;

        /// <summary>
        /// Number of channels <para>Exemple: Mono = 1, Stereo = 2, etc.</para>
        /// </summary>
        public short NumChannels { get; private set; }
        /// <summary>
        /// Sample rate. <para>Exemple: 8000, 44100, etc.</para>
        /// </summary>
        public int SampleRate { get; private set; }
        /// <summary>
        /// Byte rate. <para>ByteRate = SampleRate * NumChannels * BitsPerSample/8</para>
        /// </summary>
        public int ByteRate { get; private set; }
        /// <summary>
        /// Bytes per sample. <para>BytesPerSample = BitsPerSample / 8.</para>
        /// </summary>
        public short BytesPerSample { get; private set; }
        private byte[] wavData;
        /// <summary>
        /// Whole wav file as byte array.
        /// </summary>
        public byte[] WavData
        {
            get { return wavData; }
            private set { wavData = value; }
        }
        /// <summary>
        /// Data length without header size.
        /// <para> DataLength = WavData.Length - 44.</para>
        /// </summary>
        public int DataLength { get; private set; }

        public WavFile(byte [] data)
        {
            wavData = data;
            ReadHeader();
        }
        private void ReadHeader()
        {
            NumChannels = BitConverter.ToInt16(wavData, 22);
            SampleRate = BitConverter.ToInt32(wavData, 24);
            ByteRate = BitConverter.ToInt32(wavData, 28);
            BytesPerSample = BitConverter.ToInt16(wavData, 34);
            BytesPerSample /= 8;
            DataLength = BitConverter.ToInt32(wavData, 40);
        }
        /// <summary>
        /// Evry n-th sample is replaced with previous and next sample average value. 
        /// <para>Parameter 'n' must be grater than 0.</para>
        /// </summary>
        /// <param name="n">N-th sampe.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SmoothWav(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException("n");

            int step = NumChannels * n;

            for (int i = HEADER_OFFSET + NumChannels; i < WavData.Length - NumChannels; i = i + step)
            {
                for (int j = 0; j < NumChannels; j++)
                {
                    int averageSample = wavData[i - NumChannels] + wavData[i + NumChannels] / 2;
                    wavData[i + j] = (byte) averageSample;
                }
            }
        }
        /// <summary>
        /// Create one single wav file from passed wav array. Concatanation starts from index 0 to the last.
        /// </summary>
        /// <param name="wavFiles">Files for concat.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <returns>New wavFile in bytes.</returns>
        public static byte[] Concat(WavFile[] wavFiles)
        {
            int newLength = wavFiles[0].WavData.Length; //First file whole length.
            for (int i = 1; i < wavFiles.Length; i++)
            {
                if (wavFiles[i].NumChannels != wavFiles[0].NumChannels || wavFiles[i].SampleRate != wavFiles[0].SampleRate)
                    throw new InvalidOperationException("NumChannel and SampleRate must be the same!");
                //Calculating new wavFile length.
                //I don't know why but some file have wrong dataLength information so better way to calculate new length is to substract whole file size by 44(header size).

                newLength += wavFiles[i].DataLength;
            }
            //Create new byte array.
            byte[] newWavFile = new byte[newLength];

            int dstOffset = 0;
            //Copy whole first file (with header), this will be done manualy.
            Buffer.BlockCopy(wavFiles[0].WavData, 0, newWavFile, dstOffset, wavFiles[0].WavData.Length);


            //Subchunk2Size = NumSamples * NumChannels * BitsPerSample/8
            //This is the number of bytes in the data.
            //You can also think of this as the size
            //of the read of the subchunk following this number.
            int subchunk2Size = newLength - HEADER_OFFSET;

            //This is the size of the 
            //entire file in bytes minus 8 bytes for the
            //two fields not included in this count:
            //ChunkID and ChunkSize.
            int chunkSize = subchunk2Size - 8;
            //Change header info. (Actualy change only number of bytes in the data.)
            newWavFile[40] = (byte)chunkSize;
            newWavFile[41] = (byte)(chunkSize >> 8);
            newWavFile[42] = (byte)(chunkSize >> 16);
            newWavFile[43] = (byte)(chunkSize >> 24);

            newWavFile[4] = (byte)subchunk2Size;
            newWavFile[5] = (byte) (subchunk2Size >> 8);
            newWavFile[6] = (byte) (subchunk2Size >> 16);
            newWavFile[7] = (byte) (subchunk2Size >> 24);

            //Calculate offset.
            dstOffset = wavFiles[0].WavData.Length;

            //Copy rest data.
            for (int i = 1; i < wavFiles.Length; i++)
            {
                //Copy current file to the new one.
                Buffer.BlockCopy(wavFiles[i].WavData, HEADER_OFFSET, newWavFile, dstOffset, wavFiles[i].DataLength);
                //Calculate new offset
                dstOffset += wavFiles[i].DataLength;
            }

            return newWavFile;
        }
    }
}
