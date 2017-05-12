using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SignTest.Model.Data
{
    class SHA256Block : Block
    {
        public SHA256Block(byte[] data, int size, long index) : base(data, size, index)
        {
            CalculateSHA256();
        }

        public SHA256Block(Block block) : base(block.Data, block.Size, block.Index)
        {
            CalculateSHA256();
        }

        public byte[] ByteSHA256
        {
            get; private set;
        }

        public string StrSHA256
        {
            get;
            private set;
        }

        private void CalculateSHA256()
        {
            SHA256 sha256 = SHA256.Create();

            ByteSHA256 = sha256.ComputeHash(Data, 0, Size);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in ByteSHA256)
                stringBuilder.AppendFormat("{0:X2}", b);

            StrSHA256 = stringBuilder.ToString();
        }

    }
}
