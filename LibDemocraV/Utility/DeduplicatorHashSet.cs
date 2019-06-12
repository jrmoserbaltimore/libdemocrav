using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// Stores WeakReferences to objects and attempts to deduplicate those
    /// objects when Object.Equals() and Object.GetHashCode() both match.
    /// </summary>
    /// <typeparam name="T">The type to deduplicate</typeparam>
    public class DeduplicatorHashSet<T>
        where T : class
    {
        public delegate T ObjectCreator(T reference);
        private readonly ObjectCreator objectCreator = null;
        private readonly object trimLock = new object();

        private readonly ConcurrentDictionary<int, ConcurrentBag<WeakReference<T>>> hashTable
            = new ConcurrentDictionary<int, ConcurrentBag<WeakReference<T>>>();

        int ExcessCountdown = 0;

        public T this[T index]
        {
            get
            {
                Interlocked.Decrement(ref ExcessCountdown);
                if (!TryGetValue(index, out T output))
                {
                    // Look up the object through a creator if supplied
                    if (objectCreator is null)
                        output = index;
                    else // FIXME:  Thread safety?
                        output = objectCreator(index);
                    // If we still can't find it, store what we found into the table
                    if (!TryGetValue(output, out T testout))
                    {
                        ConcurrentBag<WeakReference<T>> bucket;
                        // Get or create the bucket
                        bucket = hashTable.GetOrAdd(output.GetHashCode(), new ConcurrentBag<WeakReference<T>>());
                        // add it to the bucket and return it as the object.
                        bucket.Add(new WeakReference<T>(output));
                    }
                    else
                        output = testout;
                }
                return output;
            }
        }

        public bool TryGetValue(T key, out T value)
        {
            lock (trimLock)
            {
                if (ExcessCountdown <= 0)
                {
                    TrimExcess();
                    if (ExcessCountdown < 100000)
                        ExcessCountdown = 100000;
                }
            }

            value = null;
            // Get or create the bucket
            if (!hashTable.TryGetValue(key.GetHashCode(), out ConcurrentBag<WeakReference<T>> bucket))
                return false;

            foreach (WeakReference<T> item in bucket)
            {
                if (item.TryGetTarget(out T output))
                {
                    // Actual object match?
                    if (output.Equals(key))
                    {
                        value = output;
                        return true;
                    }
                }
            }

            // didn't find it in the bucket, so fail
            return false;
        }

        public DeduplicatorHashSet()
            : this(null)
        {
        }
        public DeduplicatorHashSet(ObjectCreator objectCreator)
        {
            this.objectCreator = objectCreator;
        }

        private void TrimExcess()
        {
            // Query the dictionary in parallel; bags should be small
            var q = from x in hashTable.AsParallel()
                    from y in x.Value
                    where !y.TryGetTarget(out T output)
                    group y by x.Key into g
                    select g;

            // Resolve the query first so we're not updating hashTable while querying it
            Parallel.ForEach(q.ToArray(), (i) =>
            {
                ConcurrentBag<WeakReference<T>> bucket;
                if (hashTable.TryGetValue(i.Key, out bucket))
                {
                    var bag = new ConcurrentBag<WeakReference<T>>(bucket.Except(i).Distinct());

                    if (bag.Any())
                        hashTable.TryUpdate(i.Key, bag, bucket);
                    else
                        hashTable.TryRemove(i.Key, out bag);
                    Interlocked.Add(ref ExcessCountdown, bag.Count);
                }
            });
        }
    }
}
