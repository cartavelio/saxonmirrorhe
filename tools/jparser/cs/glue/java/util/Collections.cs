namespace java.util
{
    public class Collections
    {
        public static void addAll<T>(Collection<T> c, params T[] additions)
        {
            foreach (T extra in additions)
            {
                c.add(extra);
            }
        }
        
        public static void addAll<T>(Collection<T> c, Collection<T> additions)
        {
            foreach (T extra in additions)
            {
                c.add(extra);
            }
        }

        public static List<T> emptyList<T>() {
            return new ArrayList<T>();
        }
        
        public static List<T> singletonList<T>(T entry) {
            ArrayList<T> a =  new ArrayList<T>();
            a.add(entry);
            return a;
        }
        
        public static Set<T> emptySet<T>() {
            return new HashSet<T>();
        }
        
        public static java.util.Iterator<T> emptyIterator<T>() {
            return new EmptyIterator<T>();
        }
        
        public static java.util.Map<K,V> emptyMap<K,V>() {
            return new HashMap<K, V>();
        }

        public static java.util.Set<T> singleton<T>(T o) {
            // TODO: it should be immutable, and a custom implementation would be better
            java.util.HashSet<T> theSet = new HashSet<T>(1);
            theSet.add(o);
            return theSet;
        }
    }

    public class EmptyIterator<T> : java.util.Iterator<T> {
        public bool hasNext() {
            return false;
        }

        public T next() {
            return default(T);
        }
    }
}