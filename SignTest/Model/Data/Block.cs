using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignTest.Model.Data
{
    class Block
    {
        public Block(byte[] data, int size, long index)
        {
            Data = data;
            Size = size;
            Index = index;
        }


        public byte[] Data
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            private set;
        }

        public long Index
        {
            get;
            private set;
        }


    }
}
