using java.lang;

namespace java.util {
    public class Objects {
        static public void requireNonNull(object value) {
            if (value == null) {
                throw new NullPointerException();
            }
        }
    }
}