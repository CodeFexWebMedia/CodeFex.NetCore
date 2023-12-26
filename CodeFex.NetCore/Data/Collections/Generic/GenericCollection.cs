using System.Collections.Generic;

namespace CodeFex.NetCore.Data.Collections.Generic
{
    public class GenericCollection<K, V> : Dictionary<K, V> where K : class where V : class
    {
        public GenericCollection()
        {
        }

        public GenericCollection(IEqualityComparer<K> comparer) : base(comparer)
        {
        }

        public new bool ContainsKey(K value)
        {
            return this[value] != null;
        }

        public new V this[K key]
        {
            get
            {
                V result = null;
                if (key != null)
                {
                    TryGetValue(key, out result);
                }
                return result;
            }

            set
            {
                if (key != null)
                {
                    if (value != null)
                    {
                        if (ContainsKey(key))
                        {
                            Remove(key);
                        }
                        base[key] = value;
                    }
                    else
                    {
                        Remove(key);
                    }
                }
            }
        }
    }
}
