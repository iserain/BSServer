using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BSServer.BSSystem
{
    public class BSRandomProvider
    {
        private static int seed = Environment.TickCount;
        private static ThreadLocal<Random> RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static Random GetThreadRadom()
        {
            return RandomWrapper.Value;
        }

        public int Next()
        {
            return RandomWrapper.Value.Next();
        }
        public int Next(int maxValue)
        {
            return RandomWrapper.Value.Next(maxValue);
        }
        public int Next(int minValue, int maxValue)
        {
            return RandomWrapper.Value.Next(minValue, maxValue);
        }
    }
}