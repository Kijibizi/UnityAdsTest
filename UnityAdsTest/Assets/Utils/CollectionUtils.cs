using System.Collections.Generic;

namespace Utils
{
    public static class CollectionUtils
    {
        public static void Fill<T>(this T[] self, T element)
        {
            for (var i = 0; i < self.Length; i++)
            {
                self[i] = element;
            }
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> self, out K key, out V value)
        {
            key = self.Key;
            value = self.Value;
        }
    }
}