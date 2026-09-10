using Saxon.Ejava.lang;

namespace Saxon.Ejava.util {
    public class Objects {
        public static T requireNonNull<T>(T value) {
            if (value == null) {
                throw new NullPointerException();
            }
            else {
                return value;
            }
        }
        
        public static T requireNonNull<T>(T value, string message) {
            if (value == null) {
                throw new NullPointerException(message);
            }
            else {
                return value;
            }
        }
        
        public static T requireNonNullElse<T>(T value, T defaultValue) {
            return value ?? defaultValue;
        }
    }
}