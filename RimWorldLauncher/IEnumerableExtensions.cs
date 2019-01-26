using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimWorldLauncher
{
    public static class IEnumerableExtensions
    {
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
