using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Utility
{
    public class InfinityStack<T>
    {
        private int top;
        private T[] array;
        private T emptyT;

        private int size;
        public int Size
        {
            get { return size; }
            private set { size = value; }
        }
        private int count;
        public int Count
        {
            get { return count; }
            private set { count = value; }
        }
        public void Clear()
        {
            top = -1;
            Count = 0;
            //for (int i=0; i < size; i++)
            //    array[i] = emptyT;
            array = new T[Size];
        }
        public InfinityStack(int size,T emptyElement)
        {
            top = -1;
            Count = 0;
            Size = size;
            emptyT = emptyElement;
            array = new T[size];
        }
        public void Push(T item)
        {
            if (Count < Size)
                Count++;

            top++;
            if(top == Size)
            {
                top = 0;
            }
            array[top] = item;
        }
        public T Pop()
        {
            if (Count > 0)
            {
                Count--;  
                if (top == -1)
                {
                    top = Size-1;
                    T popItem = array[top--];
                    //Release memory
                    array[top+1] = emptyT;

                    return popItem;
                }else
                {
                    T popItem = array[top--];
                    //Release memory
                    array[top + 1] = emptyT;
                    return popItem;
                }
            }
            else
            {
                throw new InvalidOperationException("Stack is empty!");
            }
        }

        public void Reseize(int newSize)
        {
            if (newSize == Size)
                return;

            if(Count <= newSize)
            {
                T[] tmp = new T[newSize];
                int countTmp = Count;
                for(int i = 0; i <= countTmp; i++)
                {
                    try
                    {
                        tmp[i] = Pop();
                    }
                    catch (InvalidOperationException)
                    {
                        Count = countTmp;
                        top = i-1;
                        Size = newSize;
                        array = tmp;
                    }
                }
            }
            else // Count > newSize
            {
                T[] tmp = new T[newSize];
                for (int i = 0; i < newSize; i++)
                {
                        tmp[i] = Pop();
                }
                Count = newSize;
                top = newSize-1;
                Size = newSize;
                array = tmp;
            }
        }
    }
}
