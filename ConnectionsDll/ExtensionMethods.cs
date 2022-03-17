using System.Collections.Generic;
using System.Linq;


namespace Connections
{

    public static class ExtensionMethods
    {
        public static T LastOrDefaultValue<T>(this List<T> self)
        {
            T value = default(T);

            if (self == null || self.Count == 0)
            {
                return value;
            }

            return self[self.Count - 1];
        }

        public static List<string> ToListValue(this string[] self)
        {
            List<string> retorno = new List<string>();

            if (self != null && self.Count() > 0)
            {
                foreach (string value in self)
                {
                    retorno.Add(value);
                }
            }

            return retorno;
        }

        public static KeyValuePair<K, V> FirstValue<K, V>(this Dictionary<K, V> self)
        {
            foreach (KeyValuePair<K, V> pair in self)
            {
                return pair;
            }

            return default(KeyValuePair<K, V>);
        }
    }

}