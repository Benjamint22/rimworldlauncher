using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimWorldLauncher
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns the first index of <paramref name="list"/> where <paramref name="predicate"/> is true.
        /// Returns -1 if <paramref name="predicate"/> returns false on every index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list containing the index to look for.</param>
        /// <param name="predicate">The predicate to test on every element until true is found.</param>
        /// <returns></returns>
        public static int FirstIndex<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            var items = list.GetEnumerator();
            for (int i = 0; items.MoveNext(); i++)
            {
                if (predicate(items.Current))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
