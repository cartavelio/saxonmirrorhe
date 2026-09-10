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
    }
}