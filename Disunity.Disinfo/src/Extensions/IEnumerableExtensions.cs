using System;
using System.Collections.Generic;
using System.Linq;


namespace Disunity.Disinfo.Extensions {

    public static class IEnumerableExtensions {

        public static (IEnumerable<T> matches, IEnumerable<T> nonMatches) Fork<T>(
            this IEnumerable<T> source,
            Func<T, bool> pred)
        {
            var groupedByMatching = source.ToLookup(pred);
            return (groupedByMatching[true], groupedByMatching[false]);
        }


    }

}