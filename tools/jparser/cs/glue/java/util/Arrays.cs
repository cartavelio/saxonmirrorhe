using SysArray = System.Array;

namespace java.util
{
    public class Arrays
    {
        public static void fill<T>(T[] array, T value)
        {
            //SysArray.Fill<T>(array, value);  // Needs .NET 5.
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static java.util.List<T> asList(T[] array) {
            
        }
    }
}