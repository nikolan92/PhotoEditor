using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Utility
{
    //+---------------------------------------------------------------------------------------------------------+
    //|                                          Shannon Fano                                                   |
    //+---------------------------------------------------------------------------------------------------------+
    //| Offset(B) | Size(B) | Purpuse                                                                           |
    //+-----------+---------+-----------------------------------------------------------------------------------+
    //| 0         | 4       | The length of the uncompressed data.                                              |
    //+-----------+---------+-----------------------------------------------------------------------------------+
    //| 4         | 2       | Number of codes.                                                                  |
    //+-----------+---------+-----------------------------------------------------------------------------------+
    //| 6         | X       | Data telling how to construct the binary tree that was used to compress the data. |
    //+-----------+---------+-----------------------------------------------------------------------------------+
    //| X         | X       | The compressed data.                                                              |
    //+-----------+---------+-----------------------------------------------------------------------------------+

    //+---------------------------------------------------------------------------------------------------------------------------+
    //|                      Data telling how to construct the binary tree that was used to compress the data                     |
    //+---------------------------------------------------------------------------------------------------------------------------+
    //| BitsLength(1B)  | Bits Informations(Variable length defined in first byte)                        | Real data value. (1B) |
    //|                 | Example: If bitslength is 12 that means two bytes is used to store one code.    |                       |
    //+-----------------+---------------------------------------------------------------------------------+-----------------------+

    public class ShannonFano
    {
        private int bitPosition;
        private int bytePosition;
        private int byteBuffer;
        private int mask;
        private int lenghtOfUncompressData;

        public ShannonFano()
        {
            bitPosition = bytePosition = byteBuffer = 0;
        }
        /// <summary>
        /// Compress passed data, async method.
        /// </summary>
        /// <param name="data">Data for compressing.</param>
        /// <param name="progress">Object for updating UI. See <see cref="Progress{T}"/> for more.</param>
        /// <returns>Compress data.</returns>
        public Task<byte[]> CompressAsync(byte[] data, IProgress<string> progress)
        {
            return Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                progress.Report("Creating frequency table.");
                TableItem[] freqTable = CreateFrequencyTableAndSort(data);
                freqTable = RemoveUnusedValues(freqTable);

                progress.Report("Creating code table.");
                CreateCodeTableForCompress(freqTable, "", 0, freqTable.Length - 1);

                //Create code table for compress.
                string[] codeTable = new string[256];

                for (int i = 0; i < freqTable.Length; i++)
                {
                    codeTable[freqTable[i].value] = freqTable[i].code;
                }

                //Compress data length.
                int newDataLength = 0;
                //Length of data that teling how to reconstruct binary tree.
                int headerLength = 6; //6  reserve data for lengths. See above.
                                      //Callculate new data Length.
                for (int i = 0; i < freqTable.Length; i++)
                {
                    //bytes needed for compressed data.
                    newDataLength += freqTable[i].occurrence * freqTable[i].code.Length;
                    //bytes needed for header part. If code.length is equal 7 then 1B is needed, if 12 then 2B is needed for storing code and so on.
                    headerLength += (int)Math.Ceiling((double)freqTable[i].code.Length / 8);
                }
                //Plus bytes needed per code for storing code length and original value.
                headerLength += freqTable.Length * 2;

                //Round to the biggest number after division with 8, because we sum bits length and we need to round to the bigger number. 
                newDataLength = (int)Math.Ceiling((double)newDataLength / 8);

                //Create new data array with final length.
                byte[] compressedData = new byte[headerLength + newDataLength];

                //Remember old dataLength.
                lenghtOfUncompressData = data.Length;
                //Write header info.
                WriteHeader(compressedData, freqTable);

                progress.Report("Comressing data.");
                //Compress data.
                for (int i = 0; i < data.Length; i++)
                {
                    WriteBitsInByte(compressedData, codeTable[data[i]]);
                }
                //Write last byte for any case, maybe buffer is not full and in that case it wil not copy with function WriteBitInByte.
                //Also we need to align bits to be BigEndian for decompress 
                //Exemple: is we code las byte to be 000101 and our code is just 101 then we must shift byteBuffer for 3 space left.
                //Final result will be 101000, this is important because we later read data for decompress from left to right.

                // We need to check last writen bitPosition and substract with 8 to get how much position we need to shift.
                int shift = 8 - bitPosition;
                compressedData[(headerLength + newDataLength) - 1] = (byte)(byteBuffer << shift);

                //Reset buffer.
                byteBuffer = 0;

                //Stop stopwatch.
                sw.Stop();
                progress.Report("Data compression finish in:" + sw.ElapsedMilliseconds + "ms");

                return compressedData;
            });
        }
        /// <summary>
        /// Compress passed data, async method.
        /// </summary>
        /// <param name="data">Data for compressing.</param>
        /// <returns>Compress data.</returns>
        public Task<byte[]> CompressAsync(byte[] data)
        {
            return Task.Run(() =>
            {
                TableItem[] freqTable = CreateFrequencyTableAndSort(data);
                freqTable = RemoveUnusedValues(freqTable);

                CreateCodeTableForCompress(freqTable, "", 0, freqTable.Length - 1);

                //Create code table for compress.
                string[] codeTable = new string[256];

                for (int i = 0; i < freqTable.Length; i++)
                {
                    codeTable[freqTable[i].value] = freqTable[i].code;
                }

                //Compress data length.
                int newDataLength = 0;
                //Length of data that teling how to reconstruct binary tree.
                int headerLength = 6; //6  reserve data for lengths. See above.
                                      //Callculate new data Length.
                for (int i = 0; i < freqTable.Length; i++)
                {
                    //bytes needed for compressed data.
                    newDataLength += freqTable[i].occurrence * freqTable[i].code.Length;
                    //bytes needed for header part. If code.length is equal 7 then 1B is needed, if 12 then 2B is needed for storing code and so on.
                    headerLength += (int)Math.Ceiling((double)freqTable[i].code.Length / 8);
                }
                //Plus bytes needed per code for storing code length and original value.
                headerLength += freqTable.Length * 2;

                //Round to the biggest number after division with 8, because we sum bits length and we need to round to the bigger number. 
                newDataLength = (int)Math.Ceiling((double)newDataLength / 8);

                //Create new data array with final length.
                byte[] compressedData = new byte[headerLength + newDataLength];

                //Remember old dataLength.
                lenghtOfUncompressData = data.Length;
                //Write header info.
                WriteHeader(compressedData, freqTable);
                //Compress data.
                for (int i = 0; i < data.Length; i++)
                {
                    WriteBitsInByte(compressedData, codeTable[data[i]]);
                }
                //Write last byte for any case, maybe buffer is not full and in that case it wil not copy with function WriteBitInByte.
                //Also we need to align bits to be BigEndian for decompress 
                //Exemple: is we code las byte to be 000101 and our code is just 101 then we must shift byteBuffer for 3 space left.
                //Final result will be 101000, this is important because we later read data for decompress from left to right.

                // We need to check last writen bitPosition and substract with 8 to get how much position we need to shift.
                int shift = 8 - bitPosition;
                compressedData[(headerLength + newDataLength) - 1] = (byte)(byteBuffer << shift);

                //Reset buffer.
                byteBuffer = 0;

                return compressedData;
            });
        }
        /// <summary>
        /// Decompress passed data, async method.
        /// </summary>
        /// <param name="data">Data for decompresion.</param>
        /// <param name="progress">Object for updating UI. See <see cref="Progress{T}"/> for more.</param>
        /// <returns></returns>
        public Task<byte[]> DecomressAsync(byte[] data, IProgress<string> progress)
        {
            return Task.Run(() => {

                Stopwatch sw = new Stopwatch();
                sw.Start();

                byteBuffer = 0;
                progress.Report("Creating code table.");
                //After ReadHeader(data) codeTable will contain all codes and appropriate original value for each code.
                TableItem[] codeTable = ReadHeader(data);

                progress.Report("Creating binnary tree.");
                ShanNode root = CreateBinaryTreeForDecopress(codeTable);

                byte[] decompressData = new byte[lenghtOfUncompressData];
                progress.Report("Decompressing data.");
                //Important prepare byteBuffer, need only once.
                byteBuffer = data[bytePosition++];
                //Reset mask for ReadBitFromByte.
                mask = 128;
                for (int i = 0; i < lenghtOfUncompressData; i++)
                {
                    decompressData[i] = DecompressByte(root, data);
                }

                //Stop stopwatch.
                sw.Stop();
                progress.Report("Data decompression finish in:"+ sw.ElapsedMilliseconds + "ms");

                return decompressData;
            });
        }
        /// <summary>
        /// Decompress passed data, async method.
        /// </summary>
        /// <param name="data">Data for decompressing.</param>
        /// <returns>Decompressed data.</returns>
        public Task<byte[]> DecomressAsync(byte[] data)
        {
            return Task.Run(() => {

                byteBuffer = 0;
                //After ReadHeader(data) codeTable will contain all codes and appropriate original value for each code.
                TableItem[] codeTable = ReadHeader(data);

                ShanNode root = CreateBinaryTreeForDecopress(codeTable);

                byte[] decompressData = new byte[lenghtOfUncompressData];
                //Important prepare byteBuffer, need only once.
                byteBuffer = data[bytePosition++];
                //Reset mask for ReadBitFromByte.
                mask = 128;
                for (int i = 0; i < lenghtOfUncompressData; i++)
                {
                    decompressData[i] = DecompressByte(root, data);
                }

                return decompressData;
            });
        }
        /// <summary>
        /// Compress passed data.
        /// </summary>
        /// <param name="data">Data for compressing.</param>
        /// <returns>Compressed data.</returns>
        public byte[] Compress(byte[] data)
        {

            TableItem[] freqTable = CreateFrequencyTableAndSort(data);
            freqTable = RemoveUnusedValues(freqTable);

            CreateCodeTableForCompress(freqTable, "", 0, freqTable.Length - 1);

            //Create code table for compress.
            string[] codeTable = new string[256];

            for (int i = 0; i < freqTable.Length; i++)
            {
                codeTable[freqTable[i].value] = freqTable[i].code;
            }

            //Compress data length.
            int newDataLength = 0;
            //Length of data that teling how to reconstruct binary tree.
            int headerLength = 6; //6  reserve data for lengths. See above.
            //Callculate new data Length.
            for (int i = 0; i < freqTable.Length; i++)
            {
                //bytes needed for compressed data.
                newDataLength += freqTable[i].occurrence * freqTable[i].code.Length;
                //bytes needed for header part. If code.length is equal 7 then 1B is needed, if 12 then 2B is needed for storing code and so on.
                headerLength += (int)Math.Ceiling((double)freqTable[i].code.Length / 8);
            }
            //Plus bytes needed per code for storing code length and original value.
            headerLength += freqTable.Length * 2;

            //Round to the biggest number after division with 8, because we sum bits length and we need to round to the bigger number. 
            newDataLength = (int)Math.Ceiling((double)newDataLength / 8);

            //Create new data array with final length.
            byte[] compressedData = new byte[headerLength + newDataLength];

            //Remember old dataLength.
            lenghtOfUncompressData = data.Length;
            //Write header info.
            WriteHeader(compressedData, freqTable);
            //Compress data.
            for (int i = 0; i < data.Length; i++)
            {
                WriteBitsInByte(compressedData, codeTable[data[i]]);
            }
            //Write last byte for any case, maybe buffer is not full and in that case it wil not copy with function WriteBitInByte.
            //Also we need to align bits to be BigEndian for decompress 
            //Exemple: is we code las byte to be 000101 and our code is just 101 then we must shift byteBuffer for 3 space left.
            //Final result will be 101000, this is important because we later read data for decompress from left to right.

            // We need to check last writen bitPosition and substract with 8 to get how much position we need to shift.
            int shift = 8 - bitPosition;
            compressedData[(headerLength + newDataLength) - 1] = (byte)(byteBuffer << shift);

            //Reset buffer.
            byteBuffer = 0;

            return compressedData;
        }
        /// <summary>
        /// Decompress passed data.
        /// </summary>
        /// <param name="data">Data for decompressing.</param>
        /// <returns>Decompressed data.</returns>
        public byte[] Decompress(byte[] data)
        {
            byteBuffer = 0;
            //After ReadHeader(data) codeTable will contain all codes and appropriate original value for each code.
            TableItem[] codeTable = ReadHeader(data);

            ShanNode root = CreateBinaryTreeForDecopress(codeTable);

            byte[] decompressData = new byte[lenghtOfUncompressData];

            //Important prepare byteBuffer, need only once.
            byteBuffer = data[bytePosition++];
            //Reset mask for ReadBitFromByte.
            mask = 128;
            for (int i = 0; i < lenghtOfUncompressData; i++)
            {
                decompressData[i] = DecompressByte(root, data);
            }
            return decompressData;
        }
        /// <summary>
        /// Decompress one byte for final decompressed data.
        /// </summary>
        /// <param name="root">Root node of ShannonFano tree.</param>
        /// <param name="data">Data for decompress.</param>
        /// <returns></returns>
        private byte DecompressByte(ShanNode root, byte[] data)
        {
            ShanNode currentNode = root;

            while (!(currentNode.left == null && currentNode.right == null))//Leaf node, read value.
            {
                if (ReadBitFromByte(data))//if true go right else left
                    currentNode = currentNode.right;
                else
                    currentNode = currentNode.left;
            }

            return currentNode.value;
        }
        /// <summary>
        /// Write header in compress data, for more details see table above class definition.
        /// </summary>
        /// <param name="data">Compress data array.</param>
        /// <param name="lenghtOfUncompressData">Length of uncomress data.</param>
        /// <param name="codeTable">Code tabel must contains values and codes.</param>
        private void WriteHeader(byte[] data, TableItem[] codeTable)
        {
            //Reset byteBuffer and bitPosition.
            byteBuffer = bytePosition = 0;

            //Number of codes is maximum 256;
            int numberOfCodes = codeTable.Length;

            //Store original data length. (4B)
            data[bytePosition++] = (byte)lenghtOfUncompressData;
            data[bytePosition++] = (byte)(lenghtOfUncompressData >> 8);
            data[bytePosition++] = (byte)(lenghtOfUncompressData >> 16);
            data[bytePosition++] = (byte)(lenghtOfUncompressData >> 24);
            //Store number of codes. (2B)
            data[bytePosition++] = (byte)numberOfCodes;
            data[bytePosition++] = (byte)(numberOfCodes >> 8);

            //Store each code length, code and original value.
            for (int i = 0; i < codeTable.Length; i++)
            {
                //Store code length.
                data[bytePosition++] = (byte)codeTable[i].code.Length;
                //Store code.
                WriteBitsInByte(data, codeTable[i].code);
                //In case when bitPosition is not zero that means the byteBuffer still keep bits and it need's to be copied in final compress data,
                //and increment bytePosition to get ready for new code length and code value.
                //Otherwise if bitPosition is zero byteBuffer is already copied.
                if (bitPosition != 0)
                {
                    int shift = 8 - bitPosition;
                    //Shift code to be writen in BigEndian format (BigEndian for bits not bytes some kind of bigEndian).
                    data[bytePosition++] = (byte)(byteBuffer << shift);
                    //Reset values.
                    byteBuffer = bitPosition = 0;
                }
                //Store original value.
                data[bytePosition++] = codeTable[i].value;
            }
        }
        /// <summary>
        /// Read header data from passed data, and create code table based on heder info.
        /// </summary>
        /// <param name="data">Compress data.</param>
        /// <returns>Code table.</returns>
        private TableItem[] ReadHeader(byte[] data)
        {
            //Reset byteBuffer and bitPosition.
            byteBuffer = bytePosition = 0;

            //Read original data length. (4B)
            lenghtOfUncompressData = data[bytePosition++];
            lenghtOfUncompressData |= (data[bytePosition++] << 8);
            lenghtOfUncompressData |= (data[bytePosition++] << 16);
            lenghtOfUncompressData |= (data[bytePosition++] << 24);
            //Read number of codes. (2B)
            int numberOfCodes;
            numberOfCodes = data[bytePosition++];
            numberOfCodes |= (data[bytePosition++] << 8);

            TableItem[] codeTable = new TableItem[numberOfCodes];

            //Init table.
            for (int i = 0; i < codeTable.Length; i++)
            {
                codeTable[i] = new TableItem(0, "");
            }

            //Temporary value for storing code length.
            byte codeLength;
            //Read each code length and code.
            for (int i = 0; i < numberOfCodes; i++)
            {
                //Read code length.
                codeLength = data[bytePosition++];
                //Read codes.
                codeTable[i].code = ReadBitsFromByte(data, codeLength);
                //Read original value.
                codeTable[i].value = data[bytePosition++];
            }

            return codeTable;
        }
        /// <summary>
        /// Write string code in final compress data, and take care about overflow in string code.
        /// <para>If string code contains 12 bits, then this function will write first 8 bits in one byte and that byte in final copress data and increment bytePosition.</para> 
        /// <para>Important: Other 4 bits will stay in buffer until its filled,
        /// when byteBuffer is full it will be copy in final compress data and increment bytePosition.</para>
        /// <para>Important: When you reach last byte for compress you need manualy to copy byteBuffer into last positionon of copressData array.</para>
        /// </summary>
        /// <param name="data">Final compress data.</param>
        /// <param name="code">String code.</param>
        private void WriteBitsInByte(byte[] data, string code)
        {
            char[] bits = code.ToCharArray();

            for (int i = 0; i < bits.Length; i++)
            {
                if (bitPosition != 7)
                {
                    if (bits[i].Equals('0'))
                        byteBuffer = byteBuffer << 1;
                    else
                        byteBuffer = (byteBuffer << 1) | 1;
                    bitPosition++;
                }
                else
                {
                    if (bits[i].Equals('0'))
                        byteBuffer = byteBuffer << 1;
                    else
                        byteBuffer = (byteBuffer << 1) | 1;

                    data[bytePosition++] = (byte)byteBuffer;
                    bitPosition = 0;
                    byteBuffer = 0;
                }
            }
        }
        /// <summary>
        /// Read bits from data at current bytePosition. After read function increment bytePosition for appropriate ammount of byte position.
        /// <para>If code length is 8 then move bytePosition for one, if codeLength is 12 move bytePosition for two and so on.</para>
        /// <para>Important: Code is readed in reverse direction!</para>
        /// </summary>
        /// <param name="data">Data for read.</param>
        /// <param name="codeLength">Code length.</param>
        /// <returns>Code as string value.</returns>
        private string ReadBitsFromByte(byte[] data, byte codeLength)
        {
            mask = 128;
            string code = "";
            int bit;

            //Read first byte.
            byteBuffer = data[bytePosition++];
            for (int i = 0; i < codeLength; i++)
            {
                if (mask != 0) //If we read the whole byte then read new byte in byteBuffer and keep reading bits.
                {
                    //Apply mask.
                    bit = byteBuffer & mask;

                    if (bit == 0)
                        code += "0";
                    else
                        code += "1";
                    //Shift mask.
                    mask = mask >> 1;
                }
                else
                {
                    byteBuffer = data[bytePosition++];
                    //Reset mask.
                    mask = 128;

                    //Need to read here because if we don't we are losing iteration.
                    //Apply mask.
                    bit = byteBuffer & mask;

                    if (bit == 0)
                        code += "0";
                    else
                        code += "1";
                    //Shift mask.
                    mask = mask >> 1;
                }
            }

            return code;
        }
        /// <summary>
        /// Read single bit from data, ased on bitPosition and bytePosition info.
        /// <para>Important: Initialy this function needs byteBuffer filled, also mask must be set on 128.</para>
        /// </summary>
        /// <param name="data">Data for read.</param>
        /// <returns>Single bit</returns>
        private bool ReadBitFromByte(byte[] data)
        {
            bool result;
            int bit;

            if (mask != 0) //If we read the whole byte then read new byte in byteBuffer and keep reading bits.
            {
                //Apply mask.
                bit = byteBuffer & mask;

                if (bit == 0)
                    result = false;
                else
                    result = true;
                //Shift mask.
                mask = mask >> 1;
            }
            else
            {
                byteBuffer = data[bytePosition++];
                //Reset mask.
                mask = 128;

                //Apply mask.
                bit = byteBuffer & mask;

                if (bit == 0)
                    result = false;
                else
                    result = true;
                //Shift mask.
                mask = mask >> 1;
            }
            return result;
        }
        /// <summary>
        /// Create code table and store each code in TableItem.code field as string of one and zero values.
        /// </summary>
        /// <param name="freqTable">Sorted frequency table.</param>
        /// <param name="codes">Initial code is empty string.</param>
        /// <param name="minIndex">Inital index is zero.</param>
        /// <param name="maxIndex">Initial index is frequency table length - 1.</param>
        private void CreateCodeTableForCompress(TableItem[] freqTable, string codes, int minIndex, int maxIndex)
        {
            int splitPoint = CalculateSplitPoint(freqTable, minIndex, maxIndex);

            if (maxIndex - minIndex != 1 && maxIndex - minIndex != 0) //
            {
                CreateCodeTableForCompress(freqTable, codes + "0", minIndex, splitPoint - 1);//left part of the tree
                CreateCodeTableForCompress(freqTable, codes + "1", splitPoint, maxIndex);//right part of the tree
            }
            else if (maxIndex - minIndex == 1)
            {
                freqTable[minIndex].code = codes + "0";
                freqTable[maxIndex].code = codes + "1";
                return;
            }
            else
            {
                freqTable[minIndex].code = codes;
                return;
            }

        }
        /// <summary>
        /// Create binary tree by Shannon-Pano algorithm based on passed codeTable.
        /// </summary>
        /// <param name="codeTable">Table must contain codes and original values.</param>
        /// <returns>Root of the tree.</returns>
        private ShanNode CreateBinaryTreeForDecopress(TableItem[] codeTable)
        {
            ShanNode root = new ShanNode();
            for (int i = 0; i < codeTable.Length; i++)
            {
                //Create branch for each code.
                CreateTreeBranch(root, codeTable[i].value, codeTable[i].code.ToCharArray(), 0);
            }
            return root;
        }
        /// <summary>
        /// Create singe branch for binary tree, branch reprezent one code.
        /// </summary>
        /// <param name="node">Current node. Initial value root node.</param>
        /// <param name="value">Original data value.</param>
        /// <param name="code">Code.</param>
        /// <param name="codeIndex">Code index. Initial value 0.</param>
        private void CreateTreeBranch(ShanNode node, byte value, char[] code, int codeIndex)
        {
            if (codeIndex != code.Length - 1)
            {
                //Create new node if does't exist.
                if (code[codeIndex].Equals('0'))
                {
                    if (node.left == null)
                    {
                        node.left = new ShanNode();
                        CreateTreeBranch(node.left, value, code, codeIndex + 1);
                    }
                    else
                    {
                        CreateTreeBranch(node.left, value, code, codeIndex + 1);
                    }
                }
                else //Case when code is equal 1.
                {
                    if (node.right == null)
                    {
                        node.right = new ShanNode();
                        CreateTreeBranch(node.right, value, code, codeIndex + 1);
                    }
                    else
                    {
                        CreateTreeBranch(node.right, value, code, codeIndex + 1);
                    }
                }
            }
            else //Create leaf node and write value inside.
            {
                if (code[codeIndex].Equals('0'))
                {
                    node.left = new ShanNode();
                    node.left.value = value;
                    return;
                }
                else //Case when code is equal 1.
                {
                    node.right = new ShanNode();
                    node.right.value = value;
                    return;
                }
            }
        }
        /// <summary>
        /// Calculating split point for frequency table from mixIndex to maxIndex including maxIndex.
        /// <para>You have to take care of index boundarise.</para>
        /// </summary>
        /// <param name="freqTable">Frequency table.</param>
        /// <param name="minIndex">Start index.</param>
        /// <param name="maxIndex">Stop index.</param>
        /// <returns>Split point.</returns>
        private int CalculateSplitPoint(TableItem[] freqTable, int minIndex, int maxIndex)
        {
            int totalProbablility = CalculateTotalProbability(freqTable, minIndex, maxIndex);

            int max = 0;
            int split = minIndex;
            for (int i = minIndex; i <= maxIndex; i++)
            {
                max += freqTable[i].occurrence;
                split++;
                if (max >= totalProbablility / 2) //Half of the total freq is used, stop.
                    break;
            }
            return split;
        }
        /// <summary>
        /// Remove all unused values from freqTable.
        /// <para>Unused values are values which occurrence is equal zero.</para>
        /// </summary>
        /// <param name="freqTable"></param>
        /// <returns></returns>
        private TableItem[] RemoveUnusedValues(TableItem[] freqTable)
        {
            TableItem[] newFreqTable = null;
            for (int i = 255; i > 0; i--)
            {
                if (freqTable[i].occurrence != 0)
                {
                    newFreqTable = new TableItem[i + 1];
                    break;
                }
            }
            for (int i = 0; i < newFreqTable.Length; i++)
            {
                newFreqTable[i] = freqTable[i];
            }
            return newFreqTable;
        }
        /// <summary>
        /// Simply calculate sum of all probabilities. Iteration going from mixIndex to maxIndex including maxIndex.
        /// <para>You have to take care of index boundarise.</para>
        /// </summary>
        /// <param name="freqTable">Frequency table.</param>
        /// <param name="minIndex">Start index.</param>
        /// <param name="maxIndex">Stop index.</param>
        /// <returns>Total propability.</returns>
        private int CalculateTotalProbability(TableItem[] freqTable, int minIndex, int maxIndex)
        {
            int totalFreq = 0;
            for (int i = minIndex; i <= maxIndex; i++)
            {
                totalFreq += freqTable[i].occurrence;
            }
            return totalFreq;
        }
        /// <summary>
        /// Create Frequency table.
        /// </summary>
        /// <param name="data">Data for analysing.</param>
        /// <returns></returns>
        private TableItem[] CreateFrequencyTableAndSort(byte[] data)
        {
            TableItem[] freqTable = InitFreqTable();

            for (int i = 0; i < data.Length; i++)
            {
                freqTable[data[i]].occurrence++;
            }
            return freqTable.OrderByDescending(val => val.occurrence).ToArray();
        }
        /// <summary>
        /// Create new array with 256 items of TableItem object with initial value.
        /// <para>Initial values: value = (0-255), occurrence = 0.</para>
        /// </summary>
        /// <returns></returns>
        private TableItem[] InitFreqTable()
        {
            TableItem[] freqTable;

            freqTable = new TableItem[256];
            for (int i = 0; i < 256; i++)
            {
                freqTable[i] = new TableItem(0, (byte)i);
            }
            return freqTable;
        }
        /// <summary>
        /// Helper class, node for binnary tree.
        /// </summary>
        private class ShanNode
        {
            public ShanNode left;
            public ShanNode right;
            public byte value;
            public ShanNode()
            {
                left = right = null;
            }
            public ShanNode(ShanNode left, ShanNode right)
            {
                this.left = left;
                this.right = right;
            }
            public ShanNode(ShanNode left, ShanNode right, byte code)
            {
                this.left = left;
                this.right = right;
                this.value = code;
            }
        }
        /// <summary>
        /// Helper class for storing number of occurrence and symbol value.
        /// </summary>
        private class TableItem
        {
            public string code;
            public byte value;
            public int occurrence;
            public TableItem(int occurrence, byte value)
            {
                this.occurrence = occurrence;
                this.value = value;
            }
            public TableItem(byte value, string code)
            {
                this.value = value;
                this.code = code;
            }
            public TableItem(int occurrence, byte value, string code)
            {
                this.occurrence = occurrence;
                this.value = value;
                this.code = code;
            }
        }
    }
}
