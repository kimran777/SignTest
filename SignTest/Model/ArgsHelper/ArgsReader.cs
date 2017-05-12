using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignTest.Model.ArgsHelper
{
    class ArgsReader
    {
        public static SignatureInfo Read(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Использование: SignTest.exe [имя входного файла] [размер блока]");
            }

            string inputFileName = args[0].Trim();
            long blockSize = Int64.Parse(args[1]);

            return new SignatureInfo(new FileInfo(inputFileName), blockSize);
        }
    }

}
