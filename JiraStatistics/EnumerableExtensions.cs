using System;
using System.Collections.Generic;

namespace JiraStatistics
{
    public interface ILag<T>
    {
        T Previous { get; }
        T Next { get; }

        void Deconstruct(out T previous, out T next);
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<ILag<T>> Lag<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Lag(source, AlwaysTruePredicate);
        }

        private static bool AlwaysTruePredicate<T>(T prev, T next)
        {
            return true;
        }

        public static IEnumerable<ILag<T>> Lag<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return LagIterator(source, predicate);
        }

        private static IEnumerable<ILag<T>> LagIterator<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            var previous = default(T);
            var isFirstIteration = true;

            foreach (var element in source)
            {
                if (isFirstIteration)
                {
                    previous = element;
                    isFirstIteration = false;
                }
                else
                {
                    if (predicate(previous, element))
                        yield return new LagElement<T>(previous, element);

                    previous = element;
                }
            }
        }

        private class LagElement<T> : ILag<T>
        {
            public LagElement(T previous, T current)
            {
                this.Previous = previous;
                this.Next = current;
            }

            public T Previous { get; }

            public T Next { get; }

            public void Deconstruct(out T previous, out T next)
            {
                previous = this.Previous;
                next = this.Next;
            }
        }
    }
}