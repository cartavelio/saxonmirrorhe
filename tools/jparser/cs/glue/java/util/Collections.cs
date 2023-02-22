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

        public static List<object> emptyList() {
            return new ArrayList<object>();
        }
    }
}