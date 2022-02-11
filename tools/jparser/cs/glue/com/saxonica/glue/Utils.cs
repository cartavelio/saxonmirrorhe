using System;

namespace com.saxonica.glue {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
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