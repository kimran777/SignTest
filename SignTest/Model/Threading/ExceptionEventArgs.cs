using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignTest.Model.Threading
{
    class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception excInThread)
        {
            ExceptionInThread = excInThread;
        }
        public Exception ExceptionInThread { get; private set; }
    }
}
