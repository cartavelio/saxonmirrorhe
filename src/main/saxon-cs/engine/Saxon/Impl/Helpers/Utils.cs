using System;

namespace Saxon.Impl.Helpers {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
        }
        public static long unsignedRightShift(long i, int shift) {
            return (long) ((ulong) i >> shift);
        }

        public static string intToHexString(int i) {
            throw new NotImplementedException();
        }
        
        public static string longToHexString(long i) {
            throw new NotImplementedException();
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