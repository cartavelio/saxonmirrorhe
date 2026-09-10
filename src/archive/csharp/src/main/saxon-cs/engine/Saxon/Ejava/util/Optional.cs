
 /*
  * A C# emulation of Saxon.Ejava.util.Optional, incomplete but sufficient for use in Saxon
  */
 
namespace Saxon.Ejava.util {
    public sealed class Optional<T> {
        private readonly T _value;
        private readonly bool _present;

        public Optional(T value) {
            _value = value;
            _present = true;
        }
        private Optional() {
            _value = default(T);
            _present = false;
        }
        public T get() {
            return _value;
        }
        public bool isPresent() {
            return _present;
        }

        public static Optional<T> empty() {
            return new();
        }
        
        public static Optional<T> of(T value) {
            // TODO: require non-null?
            return new Optional<T>(value);
        }
    }

}