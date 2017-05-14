using SignTest.Model;
using SignTest.Model.ArgsHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SignTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var signInfo = ArgsReader.Read(args);

                SignatureWorker signWorker = new SignatureWorker(signInfo.InputFile, signInfo.BlockSize);
                signWorker.Start();
                
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
            }
            
        }
    }
}
