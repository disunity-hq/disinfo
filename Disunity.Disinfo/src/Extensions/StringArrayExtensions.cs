using System.Collections.Generic;
using System.Linq;


namespace Disunity.Disinfo.Extensions {

    public static class StringArrayExtensions {

        public static void Deconstruct<T>(this T[] list, out T first, out T[] rest) {

            first = list.Length > 0 ? list[0] : default; // or throw
            rest = list.ToList().Skip(1).ToArray();
        }

        public static void Deconstruct<T>(this T[] list, out T first, out T second, out T[] rest) {
            first = list.Length> 0 ? list[0] : default; // or throw
            second = list.Length> 1 ? list[1] : default; // or throw
            rest = list.ToList().Skip(2).ToArray();
        }

    }

}