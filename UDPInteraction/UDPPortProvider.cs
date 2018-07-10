using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPInteraction
{
    public static class UDPPortProvider
    {
        public static Dictionary<int, int> PortNumbers { get; private set; }

        private static Random randomGenerator;
        private static object lockObject;

        public static int GetFreePortNumber()
        {
           InitObjects();

            lock (lockObject)
            {
                do
                {
                    int rnd = randomGenerator.Next(49152, 65534);
                    if (IsPortAvailable(rnd))
                    {
                        PortNumbers.Add(rnd, Thread.CurrentThread.ManagedThreadId);

                        return rnd;
                    }
                } while (true);
            }
        }

        private static bool IsPortAvailable(int port)
        {
            int result = PortNumbers.Keys.Where(i => i.Equals(port)).ToList().Count();

            return result <= 0;
        }

        private static void InitObjects()
        {
            if (lockObject == null)
                lockObject = new object();

            if (randomGenerator == null)
                randomGenerator = new Random();

            if (PortNumbers == null)
                PortNumbers = new Dictionary<int, int>();
        }
    }
}
