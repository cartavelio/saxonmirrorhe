using SysCollGen = System.Collections.Generic;
namespace Saxon.Ejava.util
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
        
        public static void addAll<T>(SysCollGen.IList<T> c, params T[] additions)
        {
            foreach (T extra in additions)
            {
                c.Add(extra);
            }
        }
        
        public static void addAll<T>(SysCollGen.ISet<T> c, SysCollGen.ISet<T> additions)
        {
            foreach (T extra in additions)
            {
                c.Add(extra);
            }
        }
        
        public static void addAll<T>(SysCollGen.ISet<T> c, T[] additions)
        {
            foreach (T extra in additions)
            {
                c.Add(extra);
            }
        }
        
        public static void addAll<T>(Collection<T> c, Collection<T> additions)
        {
            foreach (T extra in additions)
            {
                c.add(extra);
            }
        }
        
        public static bool disjoint<T>(SysCollGen.ISet<T> a, SysCollGen.ISet<T> b)
        // More restrictive than the JDK method, but adequate for our purposes
        {
            foreach (T item in a) {
                if (b.Contains(item)) {
                    return false;
                }
            }

            return true;
        }

        public static SysCollGen.List<T> emptyList<T>() {
            return new SysCollGen.List<T>();
        }
        
        public static void reverse<T>(System.Collections.Generic.IList<T> list) {
            throw new System.NotImplementedException();
        }
        
        public static SysCollGen.List<T> singletonList<T>(T entry) {
            SysCollGen.List<T> a =  new SysCollGen.List<T>();
            a.Add(entry);
            return a;
        }
        
        public static SysCollGen.ISet<T> emptySet<T>() {
            return new SysCollGen.HashSet<T>();
        }
        
        public static Saxon.Ejava.util.Iterator<T> emptyIterator<T>() {
            return new EmptyIterator<T>();
        }
        
        public static SysCollGen.IDictionary<K,V> emptyMap<K,V>() {
            return new SysCollGen.Dictionary<K, V>();
        }

        public static SysCollGen.HashSet<T> singleton<T>(T o) {
            // TODO: it should be immutable, and a custom implementation would be better
            SysCollGen.HashSet<T> theSet = new SysCollGen.HashSet<T>(1);
            theSet.Add(o);
            return theSet;
        }
    }

    public class EmptyIterator<T> : Saxon.Ejava.util.Iterator<T> {
        public bool hasNext() {
            return false;
        }

        public T next() {
            return default(T);
        }
    }
}