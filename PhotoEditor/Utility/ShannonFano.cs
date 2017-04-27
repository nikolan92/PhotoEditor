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
        
        /// <summary>
        /// Compress passed data.
        /// </summary>
        /// <param name="data">Data for compressing.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] Compress(byte[] data)
        {
            string s = "ABRACADABRA";
            //data = Encoding.ASCII.GetBytes(s);

            TableItem[]freqTable = CreateFrequencyTableAndSort(data);
            freqTable = RemoveUnusedValues(freqTable);

            CreateBinaryTree(freqTable, "", 0, freqTable.Length-1);

            int newLen =0;
            for(int i = 0; i < freqTable.Length; i++)
            {
                newLen += freqTable[i].occurrence * freqTable[i].code.Length;
            }

            //Dictionary<byte, string> codes = new Dictionary<byte, string>(256);
            //double
            return null;
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
                //codes = "";
                return;
            }
            else
            {
                freqTable[minIndex].code = codes;
                codes = "";
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
                if (max >= totalProbablility / 3) //Half of the total freq is used, stop.
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
        }
    }
}
