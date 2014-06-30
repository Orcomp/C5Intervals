using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace C5.Tests.Sorting
{
    [TestFixture]
    class Timsort
    {
        [Test]
        public void Test()
        {
            var array = new int[5000];
            C5.Sorting.Timsort(array, 0, 2112);
        }

        [Test]
        public void Sorted()
        {
            const int count = 10;
            var array = new int[count];
            for (var i = 0; i < count; i++)
                array[i] = i;

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void ReverseSorted()
        {
            const int count = 10;
            var array = new int[count];
            for (var i = 0; i < count; i++)
                array[i] = count - i;

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void SortedChunks()
        {
            const int count = 10;
            const int chunk = 3;
            var array = new int[count];
            for (var i = 0; i < count; i++)
                array[i] = i % chunk;

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void InfoPulseExample()
        {
            var array = new[] { 1, 2, 3, 4, 5, 3, 4, 9, 7, 6, 5, 4, 3, 2, 1 };

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void StackSize()
        {
            Console.Out.WriteLine(Int32.MaxValue);
            Console.Out.WriteLine(Int64.MaxValue);

            var stacksize = 2;
            var a = 1;
            var b = 2;
            var sum = a + b;
            var afirst = true;

            while (sum > 0)
            {
                stacksize++;
                if (afirst)
                    sum += (a += b + 1);
                else
                    sum += b += a + 1;

                afirst = !afirst;

                Console.Out.WriteLine("Sum: " + sum);
                Console.Out.WriteLine("Stacksize: " + stacksize);
            }
        }

        [Test]
        public void ReverseSortedChunks()
        {
            const int count = 10;
            const int chunk = 3;
            var array = new int[count];
            for (var i = 0; i < count; i++)
                array[i] = chunk - i % chunk;

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void AlternatingAscendingDescending()
        {
            var array = new[] { 1, 5, 4, 2, 7, 8, 6, 3, 9, 10 };

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void Random()
        {
            var seed = new Random().Next();
            Console.Out.WriteLine("Seed: " + seed);

            var random = new Random(seed);
            const int count = 10;
            var array = new int[count];

            for (var i = 0; i < count; i++)
                array[i] = random.Next(0, count);

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }

        [Test]
        public void BigRandom()
        {
            var seed = new Random().Next();
            Console.Out.WriteLine("Seed: " + seed);

            var random = new Random(seed);
            const int count = 10000;
            var array = new int[count];

            for (var i = 0; i < count; i++)
                array[i] = random.Next(0, count);

            C5.Sorting.Timsort(array);

            CollectionAssert.IsOrdered(array);
        }
    }
}
