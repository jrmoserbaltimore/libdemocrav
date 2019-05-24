using System;
using System.Collections.Generic;
using System.Text;

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

        private readonly Dictionary<int, List<WeakReference<T>>> hashTable
            = new Dictionary<int, List<WeakReference<T>>>();

        int ExcessCountdown = 0;

        public T this[T index]
        {
            get
            {
                if (!TryGetValue(index, out T output))
                {
                    // Look up the object through a creator if supplied
                    if (objectCreator is null)
                        output = index;
                    else
                        output = objectCreator(index);
                    // If we still can't find it, store what we found into the table
                    if (!TryGetValue(output, out T testout))
                    {
                        // Get or create the bucket
                        if (!hashTable.TryGetValue(output.GetHashCode(), out List<WeakReference<T>> bucket))
                            bucket = hashTable[output.GetHashCode()] = new List<WeakReference<T>>();
                        // add it to the bucket and return it as the object.
                        bucket.Add(new WeakReference<T>(output));
                        ExcessCountdown--;
                    }
                    else
                        output = testout;
                }
                return output;
            }
        }

        public bool TryGetValue(T key, out T value)
        {
            List<WeakReference<T>> toRemove = new List<WeakReference<T>>();

            if (ExcessCountdown <= 0)
            {
                TrimExcess();
                if (ExcessCountdown < 10000)
                    ExcessCountdown = 10000;
            }

            value = null;
            // Get or create the bucket
            if (!hashTable.TryGetValue(key.GetHashCode(), out List<WeakReference<T>> bucket))
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
                else
                {
                    // Remove collected items as we encounter them
                    toRemove.Add(item);
                }
            }

            // Remove the identified items
            foreach (WeakReference<T> item in toRemove)
                bucket.Remove(item);
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
            List<WeakReference<T>> toRemove = new List<WeakReference<T>>();

            foreach (int i in hashTable.Keys)
            {
                List<WeakReference<T>> bucket = hashTable[i];
                foreach (WeakReference<T> item in bucket)
                {
                    if (!item.TryGetTarget(out T output))
                        toRemove.Add(item);
                }
                // Remove the identified items
                foreach (WeakReference<T> item in toRemove)
                {
                    bucket.Remove(item);
                    ExcessCountdown++;
                }
                // Empty bucket?  Delete the list object.
                if (bucket.Count == 0)
                    hashTable.Remove(i);
                toRemove.Clear();
            }
        }
    }
}
