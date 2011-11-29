using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VisualizeWorld
{
    public static class ListExtensions
    {
        /// <summary>
        /// Multiplies the elements in the collection.
        /// </summary>
        public static decimal Product<T>(this IEnumerable<T> list, Func<T, decimal> selector)
        {
            decimal result = 1;
            foreach (T t in list)
                result *= selector(t);
            return result;
        }

        /// <summary>
        /// Multiplies the elements in the collection.
        /// </summary>
        public static double Product<T>(this IEnumerable<T> list, Func<T, double> selector)
        {
            double result = 1;
            foreach (T t in list)
                result *= selector(t);
            return result;
        }

        /// <summary>
        /// Multiplies the elements in the collection.
        /// </summary>
        public static int Product<T>(this IEnumerable<T> list, Func<T, int> selector)
        {
            int result = 1;
            foreach (T t in list)
                result *= selector(t);
            return result;
        }

        /// <summary>
        /// Finds the maximum value in the specified range in the collection.
        /// </summary>
        /// <param name="startIndex">The inclusive inclusiveStart index.</param>
        /// <param name="endIndex">The exclusive exclusiveEnd index.</param>
        public static decimal Max<T>(this IEnumerable<T> list, int startIndex, int endIndex,
                                    Func<T, decimal> selector)
        {
            Debug.Assert(startIndex < endIndex + 1);
            Debug.Assert(startIndex >= 0);
            Debug.Assert(endIndex > 0 && endIndex <= list.Count());

            decimal max = selector(list.ElementAt(startIndex));
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                decimal element = selector(list.ElementAt(i));
                if (element > max)
                    max = element;
            }

            return max;
        }

        /// <summary>
        /// Finds the minimum value in the specified range in the collection.
        /// </summary>
        /// <param name="startIndex">The inclusive inclusiveStart index.</param>
        /// <param name="endIndex">The exclusive exclusiveEnd index.</param>
        public static decimal Min<T>(this IEnumerable<T> list, int startIndex, int endIndex,
                                    Func<T, decimal> selector)
        {
            Debug.Assert(startIndex < endIndex + 1);
            Debug.Assert(startIndex >= 0);
            Debug.Assert(endIndex > 0 && endIndex <= list.Count());

            decimal min = selector(list.ElementAt(startIndex));
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                decimal element = selector(list.ElementAt(i));
                if (element < min)
                    min = element;
            }

            return min;
        }

        /// <summary>
        /// Returns a subset of the collection starting at the specified
        /// index.
        /// </summary>
        /// <param name="array">The superset to use to create the subset.</param>
        /// <param name="startIndex">The inclusive start index.</param>
        /// <param name="elements">The number of elements to add.</param>
        /// <returns>The new subset.</returns>
        public static IEnumerable<T> Subset<T>(this IEnumerable<T> list, int startIndex, int elements)
        {
            T[] result = new T[elements];
            for (int i = 0; i < elements; i++)
                result[i] = list.ElementAt(startIndex + i);
            return result;
        }


        /// <summary>
        /// Removes the first element from the list.
        /// </summary>
        public static void RemoveFirst<T>(this IList<T> list)
        {
            list.RemoveAt(0);
        }

        /// <summary>
        /// Removes the last element from the list.
        /// </summary>
        public static void RemoveLast<T>(this IList<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }

        /// <summary>
        /// Concatenates the list elements into a single string.
        /// </summary>
        public static string Concatenate<T>(this IEnumerable<T> list)
        {
            return Concatenate(list, s => s.ToString(), "");
        }

        /// <summary>
        /// Concatenates the list elements into a single string.
        /// </summary>
        public static string Concatenate<T>(this IEnumerable<T> list, string separator)
        {
            return Concatenate(list, s => s.ToString(), separator);
        }

        /// <summary>
        /// Concatenates the list elements into a single string.
        /// </summary>
        public static string Concatenate<T>(this IEnumerable<T> list, Func<T, string> selector)
        {
            return Concatenate(list, selector, "");
        }

        /// <summary>
        /// Concatenates the list elements into a single string.
        /// </summary>
        public static string Concatenate<T>(this IEnumerable<T> list, Func<T,string> selector, string separator)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            int max = list.Count();
            foreach (var t in list)
            {
                sb.Append(selector(t));
                count++;

                if (count < max)
                    sb.Append(separator);
            }
            return sb.ToString();
        }
    }
}
