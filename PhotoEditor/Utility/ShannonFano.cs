using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Utility
{
    //+----------------------------------------------------------------------------------------+
    //|                                   Shannon-Fano Coding                                  |
    //+----------------------------------------------------------------------------------------+
    //| 1. | The length of the uncompressed data.                                              |
    //+----+-----------------------------------------------------------------------------------+
    //| 2. | Data telling how to construct the binary tree that was used to compress the data. |
    //+----+-----------------------------------------------------------------------------------+
    //| 3. | The compressed data.                                                              |
    //+----+-----------------------------------------------------------------------------------+

    public class ShannonFano
    {
        private static int bitIndex;
        private static int byteIndex;
        private static int byteBuffer;

        /// <summary>
        /// Compress passed data.
        /// </summary>
        /// <param name="data">Data for compressing.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] Compress(byte[] data)
        {
            
            bitIndex = byteIndex = byteBuffer = 0;

            TableItem[] freqTable = CreateFrequencyTableAndSort(data);
            freqTable = RemoveUnusedValues(freqTable);

            CreateBinaryTree(freqTable, "", 0, freqTable.Length-1);

            //Create code table for compress.
            string[] codeTable = new string[256];

            for (int i = 0; i < freqTable.Length; i++)
            {
                codeTable[freqTable[i].value] = freqTable[i].code;
            }

            //Callculate new Length.
            int newLength = 0;
            for(int i = 0; i < freqTable.Length; i++)
            {
                newLength += freqTable[i].occurrence * freqTable[i].code.Length;
            }
            //Round to the biggest number after division.
            newLength = (int)Math.Ceiling((double)newLength / 8);
            
            byte[] compressedData = new byte[newLength];

            //Compress data
            for (int i = 0; i < data.Length; i++)
            {
                WriteBitInByte(compressedData, codeTable[data[i]]);
            }
            //Write last byte for any case, maybe buffer is not full and in that case it wil not copy with function WriteBitInByte.
            compressedData[newLength - 1] = (byte)byteBuffer;

            return compressedData;
        }


        /// <summary>
        /// Dompress passed data.
        /// </summary>
        /// <param name="data">Data for decompressing.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] Decompress(byte[] data)
        {
            return null;
        }
        /// <summary>
        /// This function write string code in final compress data, and take care about overflow in string code.
        /// <para>If string code contains 12 bits, then this function will write first 8 bits in one byte and that byte in final copress data and increment byteIndex.</para> 
        /// <para>Other 4 bits will stay in buffer until its filled,
        /// when byteBuffer is full it will be copy in final compress data and increment byteIndex.</para>
        /// </summary>
        /// <param name="data">Final compress data.</param>
        /// <param name="code">String code.</param>
        private static void WriteBitInByte(byte[] data, string code)
        {
            char[] bits = code.ToCharArray();

            for (int i = 0; i < bits.Length; i++)
            {
                if (bitIndex != 7)
                {
                    if (bits[i].Equals('0'))
                        byteBuffer = byteBuffer << 1;
                    else
                        byteBuffer = (byteBuffer << 1) | 1;
                    bitIndex++;
                }
                else
                {
                    if (bits[i].Equals('0'))
                        byteBuffer = byteBuffer << 1;
                    else
                        byteBuffer = (byteBuffer << 1) | 1;

                    data[byteIndex] = (byte)byteBuffer;
                    byteIndex++;
                    bitIndex = 0;
                    byteBuffer = 0;
                }
            }

        }
        /// <summary>
        /// This function create binary tree by Shannon-Pano algorithm.
        /// </summary>
        /// <param name="freqTable">Sorted frequency table.</param>
        /// <param name="codes">Initial code is empty string.</param>
        /// <param name="minIndex">Inital index is zero.</param>
        /// <param name="maxIndex">Initial index is frequency table length - 1.</param>
        private static void CreateBinaryTree(TableItem[] freqTable, string codes, int minIndex, int maxIndex)
        {
            int splitPoint = CalculateSplitPoint(freqTable, minIndex, maxIndex);
            
            if(maxIndex - minIndex !=1 && maxIndex - minIndex != 0) //
            {
                CreateBinaryTree(freqTable, codes + "0", minIndex, splitPoint - 1);//left part of the tree
                CreateBinaryTree(freqTable, codes + "1", splitPoint, maxIndex);//right part of the tree
            }
            else if(maxIndex - minIndex == 1)
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
        /// Calculating split point for frequency table from mixIndex to maxIndex including maxIndex.
        /// <para>You have to take care of index boundarise.</para>
        /// </summary>
        /// <param name="freqTable">Frequency table.</param>
        /// <param name="minIndex">Start index.</param>
        /// <param name="maxIndex">Stop index.</param>
        /// <returns>Split point.</returns>
        private static int CalculateSplitPoint(TableItem[] freqTable,int minIndex,int maxIndex)
        {
            int totalProbablility = CalculateTotalProbability(freqTable, minIndex, maxIndex);

            int max = 0;
            int split = minIndex;
            for(int i = minIndex; i <= maxIndex; i++)
            {
                max += freqTable[i].occurrence;
                split++;
                if (max >= totalProbablility / 2) //Half of the total freq is used, stop.
                    break;
            }
            return split;
        }
        /// <summary>
        /// This function remove all unused values from freqTable.
        /// <para>Unused values are values which occurrence is equal zero.</para>
        /// </summary>
        /// <param name="freqTable"></param>
        /// <returns></returns>
        private static TableItem[] RemoveUnusedValues(TableItem[] freqTable)
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
        /// This function simply calculate sum of all probabilities. Iteration going from mixIndex to maxIndex including maxIndex.
        /// <para>You have to take care of index boundarise.</para>
        /// </summary>
        /// <param name="freqTable">Frequency table.</param>
        /// <param name="minIndex">Start index.</param>
        /// <param name="maxIndex">Stop index.</param>
        /// <returns>Total propability.</returns>
        private static int CalculateTotalProbability(TableItem[] freqTable,int minIndex,int maxIndex)
        {
            int totalFreq = 0;
            for(int i=minIndex; i <= maxIndex; i++)
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
        private static TableItem[] CreateFrequencyTableAndSort(byte[] data)
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
        private static TableItem[] InitFreqTable()
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
            ShanNode left;
            ShanNode right;
            ShanNode parent;
            byte symbol;
        }
        /// <summary>
        /// Helper class for storing number of occurrence and symbol value.
        /// </summary>
        private class TableItem
        {
            public string code;
            public byte value;
            public int occurrence;
            public void SetCode(string code)
            {
                this.code = code;
            }
            public TableItem(int occurrence, byte value)
            {
                this.occurrence = occurrence;
                this.value = value;
            }
            public TableItem(int occurrence, byte value,string code)
            {
                this.occurrence = occurrence;
                this.value = value;
                this.code = code;
            }
        }
    }
}
