using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5.Intervals.Tests
{
    class TestHelper
    {
        private Random Random;

        private int Count;

        public TestHelper(int? seed = null)
        {
            // Generate random seed
            if (seed == null)
            {
                var bytes = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(bytes);
                seed = BitConverter.ToInt32(bytes, 0);
            }

            createRandom((int) seed);
        }

        private void createRandom(int seed)
        {
            Random = new Random(seed);
            Console.Out.WriteLine("Seed: {0}", seed);

            Count = Random.Next(10, 20);
        }

        public int RandomInt
        {
            get
            {
                return Random.Next(Int32.MinValue, Int32.MaxValue);
            }
        }
    }
}
