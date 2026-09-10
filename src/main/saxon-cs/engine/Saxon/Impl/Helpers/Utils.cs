using System;
using java.lang;

namespace Saxon.Impl.Helpers {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
        }
        public static long unsignedRightShift(long i, int shift) {
            return (long) ((ulong) i >> shift);
        }

        public static string intToHexString(int i) {
            return i.ToString("X");
        }
        
        public static string longToHexString(long i) {
            return i.ToString("X");
        }
        
        // parse an integer literal whose radix is not known statically
        public static int ParseInt(string input, int radix) {
            if (input.StartsWith("0x")) {
                throw new NumberFormatException("Leading 0x not allowed in number");
            }
            if (radix == 2 || radix == 8 || radix == 10 || radix == 16) {
                return System.Convert.ToInt32(input, radix);
            }

            throw new NotSupportedException("Unsupported number radix " + radix);
        }
        
 
        // Test "instanceof" a generic type. For example,
        // IsInstanceOfGenericType(new List<string>(), typeof(List<>))
        // returns true
        static bool IsInstanceOfGenericType(object instance, Type genericType)
        {
            Type type = instance.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }


    }
}