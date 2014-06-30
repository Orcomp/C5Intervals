using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace C5
{
    public partial class Sorting
    {
        public static void Timsort<T>(T[] array)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Ensures(Contract.ForAll(1, array.Length, i => Comparer<T>.Default.Compare(array[i - 1], array[i]) <= 0));

            new TimSorter<T>(array).Timsort(0, array.Length);
        }

        public static void Timsort<T>(T[] array, IComparer<T> comparer)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Ensures(Contract.ForAll(1, array.Length, i => (comparer ?? Comparer<T>.Default).Compare(array[i - 1], array[i]) <= 0));

            new TimSorter<T>(array, comparer).Timsort(0, array.Length);
        }

        public static void Timsort<T>(T[] array, int start, int length, IComparer<T> comparer = null)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Requires(0 <= start, "start must be non-negative.");
            Contract.Requires(0 <= length, "length must be non-negative.");
            Contract.Requires(start + length <= array.Length);
            Contract.Ensures(Contract.ForAll(1, array.Length, i => (comparer ?? Comparer<T>.Default).Compare(array[i - 1], array[i]) <= 0));

            new TimSorter<T>(array, comparer).Timsort(start, length);
        }

        private struct Run
        {
            public int Start;
            public int Length;

            public Run(int start, int length)
                : this()
            {
                Start = start;
                Length = length;
            }

            public override string ToString()
            {
                return String.Format("Start: {0,2} Length: {1,2}", Start, Length);
            }
        }

        class TimSorter<T>
        {
            private readonly T[] _array;
            private readonly Comparison<T> compare;
            private T[] _aux;

            private int _minrun;

            private Run[] _stack;
            private int _stackIndex = 0;

            private int _index;
            private int _end;

            private int gallopingMinimum = 7;

            public TimSorter(T[] array, IComparer<T> comparer = null)
            {
                _array = array;
                compare = (comparer ?? Comparer<T>.Default).Compare;
            }

            public void Timsort(int start, int length)
            {
                // Less than two elements will always be sorted
                if (length < 2)
                    return;

                // Calculate the minimum run size
                _minrun = calcMinRun(length);

                // Calculate stack size
                var stackSize = 43;
                _stack = new Run[stackSize];


                _index = start;
                _end = start + length;

                // Use binary insertion sort for small sorts
                if (length == _minrun)
                {
                    binaryInsertionSort(0, length, 1);
                    return;
                }
                int runLength, runEnd;
                while (_index < _end)
                {
                    // Get the next run
                    runLength = getRunLength();
                    runEnd = _index + runLength;

                    // Check if its shorter than minrun
                    if (runLength < _minrun && runEnd < _end)
                    {
                        // Extend the length to minrun while avoiding overflow
                        runLength += Math.Min(_minrun - runLength, _end - runEnd);

                        // Sort run using insertion sort
                        binaryInsertionSort(_index, _index + runLength, runEnd);
                    }

                    // Add run to stack
                    _stack[_stackIndex++] = new Run(_index, runLength);
                    _index += runLength;

                    mergeCollapse();
                }

                while (_stackIndex > 1)
                {
                    var b = _stack[--_stackIndex];
                    var a = _stack[_stackIndex - 1];

                    mergeHigh(a, b);

                    _stack[_stackIndex - 1].Length += b.Length;
                }
            }

            private void binaryInsertionSort(int start, int end, int current)
            {
                if (current == start)
                    current++;

                T next;
                int high;

                for (; current < end; current++)
                {
                    // Continue if next doesn't need moving
                    if (compare(_array[high = current - 1], next = _array[current]) <= 0)
                        continue;

                    // Search for position
                    var low = start;
                    while (low < high)
                    {
                        var middle = low + (high - low >> 1);

                        if (compare(next, _array[middle]) < 0)
                            high = middle;
                        else
                            low = middle + 1;
                    }

                    Contract.Assert(low == high);

                    // Move items that are in the wrong place
                    Array.Copy(_array, high, _array, high + 1, current - high);

                    _array[low] = next;
                }
            }

            private static int calcMinRun(int length)
            {
                Contract.Requires(2 <= length);
                Contract.Ensures(64 <= length || Contract.Result<int>() == length, "If length is less than 64, result will be equal to length.");
                Contract.Ensures(length < 64 || 32 <= Contract.Result<int>() || Contract.Result<int>() <= 64);

                var tail = 0;

                while (64 <= length)
                {
                    tail |= length & 1;
                    length >>= 1;
                }

                return length + tail;
            }

            private int getRunLength()
            {
                //
                var high = _index + 1;

                // Check if we have reached end
                if (high >= _end)
                    return _end - _index;

                // The run is in non-descending order
                if (compare(_array[_index], _array[high++]) <= 0)
                {
                    while (high < _end && compare(_array[high - 1], _array[high]) <= 0)
                        high++;
                }
                // The run is in strict decreasing order
                else
                {
                    while (high < _end && compare(_array[high - 1], _array[high]) > 0)
                        high++;

                    // Revers the order of the run
                    Array.Reverse(_array, _index, high - _index);
                }

                return high - _index;
            }

            private void mergeCollapse()
            {
                while (_stackIndex > 1)
                {
                    if (_stackIndex == 2)
                    {
                        var b = _stack[1];
                        var a = _stack[0];

                        // Check if we need to merge
                        if (a.Length <= b.Length)
                        {
                            // Merge low as a is smallest
                            mergeLow(a, b);

                            // Add b's Length to a on the stack
                            _stack[0].Length += b.Length;
                            _stackIndex = 1;
                        }

                        return;
                    }
                    else
                    {
                        // TODO: Which invariant should be first?

                        var c = _stack[--_stackIndex];
                        var b = _stack[--_stackIndex];
                        var a = _stack[--_stackIndex];

                        // Check first invariant
                        if (a.Length <= b.Length + c.Length)
                        {
                            // Check which runs we need to merge
                            if (a.Length < c.Length)
                            {
                                // Merge a & b
                                if (a.Length < b.Length)
                                    mergeLow(a, b);
                                else
                                    mergeHigh(a, b);

                                // Add b's Length to a on the stack
                                _stack[_stackIndex++].Length += b.Length;
                                // Put c back on stack
                                _stack[_stackIndex++] = c;
                            }
                            else
                            {
                                // Merge b & c
                                if (b.Length < c.Length)
                                    mergeLow(b, c);
                                else
                                    mergeHigh(b, c);

                                // Leave a on the stack
                                _stackIndex++;
                                // Add c's Length to b on the stack
                                _stack[_stackIndex++].Length += c.Length;
                            }

                            continue;
                        }

                        // Check second invariant
                        if (b.Length <= c.Length)
                        {
                            // Merge low as b is smallest
                            mergeLow(b, c);

                            // Leave a on the stack
                            _stackIndex++;
                            // Add b's Length to a on the stack
                            _stack[_stackIndex++].Length += c.Length;

                            continue;
                        }

                        _stackIndex += 3;

                        return;
                    }
                }
            }

            private void mergeLow(Run a, Run b)
            {
                Contract.Requires(a.Start + a.Length == b.Start, "The runs must be consecutive to keep the sort stable.");
                Contract.Requires(a.Length >= _minrun && b.Length >= _minrun, "Runs being merged must have a length of at least minrun.");

                // Skip merging if already sorted
                // TODO: Search for b[0]'s index in a and start from there. This might skip the merge completely or make the area of a smaller.

                checkAux(a.Length);

                // Move elements from a to aux
                Array.Copy(_array, a.Start, _aux, 0, a.Length);

                // ai is first index in aux
                var ai = 0;
                // bi is first index of b in array
                var bi = b.Start;

                // The end of b dictates the end of the merge
                var end = b.Start + b.Length;

                //var takingFromA = true;
                //int aStrike = 0, bStrike = 0;

                for (var i = a.Start; ai < a.Length && i < end; i++)
                {
                    // Check if we are done with b
                    if (bi == end)
                    {
                        // Copy a to the end
                        Array.Copy(_aux, ai, _array, i, a.Length - ai);
                        break;
                    }

                    //if (gallopingMinimum <= (takingFromA ? aStrike : bStrike))
                    //{
                    //    var nextIndex = gallop();
                    //
                    //}

                    // TODO: Handle galloping
                    // TODO: Catch exceptions with try-catch and move all elements from aux back to array
                    // Prefer a to keep sort stable
                    if (/*takingFromA =*/ (compare(_aux[ai], _array[bi]) <= 0))
                    {
                        _array[i] = _aux[ai++];
                        //aStrike++;
                        //bStrike = 0;
                    }
                    else
                    {
                        _array[i] = _array[bi++];

                        //bStrike++;
                        //aStrike = 0;
                    }

                    //gallopCount++;
                }
            }

            private void checkAux(int length)
            {
                if (_aux == null || _aux.Length < length)
                {
                    // TODO: Find good way to resize this!
                    _aux = new T[length];
                }
            }

            private void mergeHigh(Run a, Run b)
            {
                Contract.Requires(a.Start + a.Length == b.Start, "The runs must be consecutive to keep the sort stable.");
                Contract.Requires(a.Length >= _minrun && b.Length >= _minrun, "Runs being merged must have a length of at least minrun.");

                // Skip merging if already sorted
                // TODO: Search for b[0]'s index in a and start from there. This might skip the merge completely or make the area of a smaller.

                checkAux(b.Length);

                // Move elements from a to aux
                Array.Copy(_array, b.Start, _aux, 0, b.Length);

                // ai is first index of a in array
                var ai = a.Start + a.Length - 1;
                // bi is first last in aux
                var bi = b.Length - 1;

                // The start of a dictates the end of the merge
                var start = a.Start;

                for (var i = b.Start + b.Length - 1; bi >= 0 && i >= start; i--)
                {
                    // Check if we are done with a
                    if (ai < start)
                    {
                        // Copy b to the beginning
                        Array.Copy(_aux, 0, _array, start, bi + 1);
                        break;
                    }

                    // TODO: Handle galloping
                    // TODO: Catch exceptions with try-catch and move all elements from aux back to array
                    // Prefer b to keep sort stable
                    if (compare(_array[ai], _aux[bi]) <= 0)
                        _array[i] = _aux[bi--];
                    else
                        _array[i] = _array[ai--];
                }
            }
        }
    }
}
