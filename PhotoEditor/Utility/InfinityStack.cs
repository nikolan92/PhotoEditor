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
            array = new T[Size];
        }
        public InfinityStack(int size)
        {
            top = -1;
            Count = 0;
            Size = size;
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
                    return array[top--];
                }else
                {
                    return array[top--];
                }
            }
            else
            {
                throw new InvalidOperationException("Stack is empty!");
            }
        }

        public void Reseize(int newSize)
        {
            throw new NotImplementedException(this.GetType().ToString());
        }
    }
}
