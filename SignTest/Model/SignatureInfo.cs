using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignTest.Model
{
    class SignatureInfo
    {
        public SignatureInfo(FileInfo _inputFile, long _blockSize)
        {
            InputFile = _inputFile;
            BlockSize = _blockSize;
        }
        
        public FileInfo InputFile
        {
            get;
            private set;
        }

        public long BlockSize
        {
            get;
            private set;
        }



    }
}
