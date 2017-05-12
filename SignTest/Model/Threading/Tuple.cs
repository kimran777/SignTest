using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignTest.Model.Threading
{
    class Tuple<T1, T2>
    {
        public T1 First
        {
            get; private set;
        }
        public T2 Second
        {
            get; private set;
        }

        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    class Tuple
    {
        public static Tuple<T1, T2> Create<T1,T2>(T1 first, T2 second)
        {
            return new Tuple<T1, T2>(first, second);
        }
    }

}
