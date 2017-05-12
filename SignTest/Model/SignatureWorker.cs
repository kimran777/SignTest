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
        private event EventHandler<ExceptionEventArgs> ExcInThreadHappened;
        private readonly int _threadForHash;
        private QueueWithLock<SHA256Block>[] _hashProducersConsumers;
        private long _maxQueuePerThread;
        private long _maxBufferSize = 104857600;
        private List<Exception> _errors = new List<Exception>();
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
            InputFileInfo = inputFileInfo;

            if (blockSize == 0)
            {
                throw new ArgumentException("Размер блока должен быть больше 0", "blockSize");
            }
            BlockSize = blockSize;

            _threadForHash = GetCountThreadForHash();

            //with small blocks too many memory needed
            long denum = BlockSize > 512 ? BlockSize : 512;

            _maxQueuePerThread = _maxBufferSize / _threadForHash / denum;

            if (_maxQueuePerThread == 0)
            {
                _maxQueuePerThread = 1;
            }


            _hashProducersConsumers = new QueueWithLock<Data.SHA256Block>[_threadForHash];
            for (int i = 0; i < _threadForHash; i++)
            {
                _hashProducersConsumers[i] = new QueueWithLock<Data.SHA256Block>(_maxQueuePerThread);
            }

        }



        public void Start()
        {
            if (!InputFileInfo.Exists)
            {
                throw new FileNotFoundException(string.Format("Файл {0} не найден", InputFileInfo.FullName));
            }


            Thread[] workerThreads = new Thread[_threadForHash];
            for (int i = 0; i < _threadForHash; i++)
            {
                workerThreads[i] = new Thread(HashWork);
            }

            long blockCounts = GetBlockCounts(InputFileInfo.Length, BlockSize);

            Thread writerThreads = new Thread(WriteOutHash);
            writerThreads.Start(blockCounts);


            ExcInThreadHappened += (sender, arg) =>
            {
                lock (_errors)
                {
                    _errors.Add(arg.ExceptionInThread);
                }

                for (int i = 0; i < _threadForHash; i++)
                {
                    _hashProducersConsumers[i].Stop();
                }

            };


            for (int i = 0; i < _threadForHash; i++)
            {
                workerThreads[i].Start(Tuple.Create(i, blockCounts));
            }

            for (int i = 0; i < _threadForHash; i++)
            {
                workerThreads[i].Join();
            }

            for (int i = 0; i < _threadForHash; i++)
            {
                _hashProducersConsumers[i].Stop();
            }


            writerThreads.Join();


            lock (_errors)
            {
                if (_errors.Count > 0)
                {
                    throw _errors.First();
                }
            }

        }



        private void HashWork(object threadIndexAndBlockCounts)
        {

            try
            {
                Tuple<int, long> param = threadIndexAndBlockCounts as Tuple<int, long>;
                int threadIndex = param.First;
                long blockCounts = param.Second;

                using (var threadInputStream = InputFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    threadInputStream.Seek(threadIndex * BlockSize, SeekOrigin.Begin);

                    long iterOffset = (_threadForHash - 1) * BlockSize;

                    int readedData = 0;

                    byte[] blockBuffer = new byte[BlockSize];



                    for (long blockIndex = threadIndex; blockIndex < blockCounts; blockIndex += _threadForHash)
                    {
                        readedData = threadInputStream.Read(blockBuffer, 0, blockBuffer.Length);

                        _hashProducersConsumers[threadIndex].Enqueue(new SHA256Block(blockBuffer, readedData, blockIndex));

                        //if (threadInputStream.Position + iterOffset >= threadInputStream.Length)
                        //{
                        //    break;
                        //}

                        threadInputStream.Seek(iterOffset, SeekOrigin.Current);
                    }



                }

            }
            catch (Exception exc)
            {
                RaiseExcInThreadHappened(exc);
            }
        }

        private void WriteOutHash(object blocksCountParameter)
        {
            long blocksCount = (long)blocksCountParameter;

            for (long i = 0; i < blocksCount; i += _threadForHash)
            {
                for (int thId = 0; thId < _threadForHash; thId++)
                {

                    if (i + thId >= blocksCount)
                    {
                        break;
                    }


                    var block = _hashProducersConsumers[thId].Dequeue();

                    if (block == null)
                    {
                        return;
                    }


                    Console.WriteLine("block #{0,-15}: {1}", block.Index, block.StrSHA256);

                }

            }

        }



        private int GetCountThreadForHash()
        {
            if (Environment.ProcessorCount >= 2)
            {
                return Environment.ProcessorCount - 1;
            }
            else
            {
                return 1;
            }
        }
        private long GetBlockCounts(long length, long blockSize)
        {
            if (length == 0)
            {
                return 1;
            }


            if (length % blockSize == 0)
            {
                return length / blockSize;
            }
            else
            {
                return length / blockSize + 1;
            }

        }



        private void RaiseExcInThreadHappened(Exception e)
        {
            if (ExcInThreadHappened != null)
            {
                ExcInThreadHappened(this, new ExceptionEventArgs(e));
            }
        }
    }
}
