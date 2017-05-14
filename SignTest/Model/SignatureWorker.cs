using SignTest.Model.Data;
using SignTest.Model.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SignTest.Model
{
    class SignatureWorker
    { 
        private readonly int _threadForHash;
        private Stream _inputStream;
        private QueueWithLock<SHA256Block> _hashProducersConsumers;
        private long _maxQueuePerThread;
        private long _maxBufferSize = 104857600;
        private Queue<Exception> _errors = new Queue<Exception>();

        public long BlockSize
        {
            get;
            private set;
        }
        public FileInfo InputFileInfo
        {
            get;
            private set;
        }

        public SignatureWorker(FileInfo inputFileInfo, long blockSize)
        {
            if(inputFileInfo == null)
            {
                throw new ArgumentNullException("inputFileInfo");
            }
            InputFileInfo = inputFileInfo;
            
            if (blockSize == 0)
            {
                throw new ArgumentException("Размер блока должен быть больше 0", "blockSize");
            }

            BlockSize = blockSize;

            _threadForHash = GetCountThreadForHash();

            //with small blocks too many memory needed
            long denum = BlockSize > 512 ? BlockSize : 512;

            _maxQueuePerThread = _maxBufferSize /  denum;
            
            if (_maxQueuePerThread == 0)
            {
                _maxQueuePerThread = 1;
            }


            _hashProducersConsumers = new QueueWithLock<SHA256Block>(_maxQueuePerThread);
            
        }



        public void Start()
        {
            _inputStream = _inputStream = InputFileInfo.OpenRead();

            var workerThreads = ThreadManager.GetSafeThreads(_threadForHash, HashWork, ExceptionHandler);
            workerThreads.StartThreads();
            
            Thread writerThreads = ThreadManager.GetSafeThread(WriteOutHash, ExceptionHandler);
            writerThreads.Start();
                        
            workerThreads.WaitThreads().ContinueWithOneTime(() =>
            {
                _hashProducersConsumers.Stop();
            });

            writerThreads.Join();

            _inputStream.Close();

            if (_errors.Any())
            {
                throw _errors.First();
            }

        }

        int blockIter = 0;
        private void HashWork()
        {
            byte[] blockBuffer = new byte[BlockSize];
            int readedData = 0;
            int blockIndex = 0;
            while (true)
            {
                lock (_inputStream)
                {
                    blockIndex = blockIter;
                    readedData = _inputStream.Read(blockBuffer, 0, blockBuffer.Length);

                    if (readedData == 0 && blockIndex != 0)
                    {
                        break;
                    }
                    blockIter++;
                }


                if(blockIndex == 1500)
                {
                    throw new NotImplementedException();
                }

                _hashProducersConsumers.Enqueue(new SHA256Block(blockBuffer, readedData, blockIndex));

            }
        }

        private void WriteOutHash()
        {

            while(true)
            {
                var block = _hashProducersConsumers.Dequeue();
                if (block == null)
                {
                    return;
                }

                Console.WriteLine("block #{0,-15}: {1}", block.Index, block.StrSHA256);
                block = null;
            }

        }
        
        private int GetCountThreadForHash()
        {
            if (Environment.ProcessorCount > 1)
            {
                return Environment.ProcessorCount - 1;
            }
            else
            {
                return 1;
            }
        }
                
                
        private void ExceptionHandler(Exception exception)
        {
            lock (_errors)
            {
                _errors.Enqueue(exception);
            }

            _hashProducersConsumers.Abort();

            Thread.CurrentThread.Abort();
        }

    }
}
