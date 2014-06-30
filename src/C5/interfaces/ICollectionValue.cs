using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.interfaces
{
    [ContractClassFor(typeof(ICollectionValue<>))]
    abstract class CollectionValueContract<T> : ICollectionValue<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract string ToString(string format, IFormatProvider formatProvider);
        public abstract bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
        public abstract EventTypeEnum ListenableEvents { get; }
        public abstract EventTypeEnum ActiveEvents { get; }
        public abstract event CollectionChangedHandler<T> CollectionChanged;
        public abstract event CollectionClearedHandler<T> CollectionCleared;
        public abstract event ItemsAddedHandler<T> ItemsAdded;
        public abstract event ItemInsertedHandler<T> ItemInserted;
        public abstract event ItemsRemovedHandler<T> ItemsRemoved;
        public abstract event ItemRemovedAtHandler<T> ItemRemovedAt;

        [Pure]
        public bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (Count == 0));

                throw new NotImplementedException();
            }

        }

        [Pure]
        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(IsEmpty || Contract.Result<int>() > 0);
                Contract.Ensures(Enumerable.Count(this) == Contract.Result<int>());

                throw new NotImplementedException();
            }
        }

        public abstract Speed CountSpeed { get; }
        public abstract void CopyTo(T[] array, int index);
        public abstract T[] ToArray();
        public abstract void Apply(Action<T> action);

        [Pure]
        public bool Exists(Func<T, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<bool>() == this.Any(predicate));

            throw new NotImplementedException();
        }

        [Pure]
        public bool Find(Func<T, bool> predicate, out T item)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(!Contract.Result<bool>() || predicate(Contract.ValueAtReturn(out item)));

            throw new NotImplementedException();
        }

        [Pure]
        public bool All(Func<T, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<bool>() == Enumerable.All(this, predicate));

            throw new NotImplementedException();
        }

        [Pure]
        public T Choose()
        {
            Contract.EnsuresOnThrow<NoSuchItemException>(IsEmpty);
            Contract.Ensures(IsEmpty || Contract.Result<T>() != null);
            // TODO: the contract fails for HashBag. This may make sense for simple types, but there are still problems with objects.
            //Contract.Ensures(IsEmpty || Contract.Exists(this, x => ReferenceEquals(x, Contract.Result<T>())));

            throw new NotImplementedException();
        }

        [Pure]
        public IEnumerable<T> Filter(Func<T, bool> filter)
        {
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>().All(filter));

            throw new NotImplementedException();
        }
    }
}
