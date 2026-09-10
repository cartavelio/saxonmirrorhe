
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

        public bool isEmpty() {
            return !_present;
        }

        public T orElse(T other) {
            return _present ? _value : other;
        }

        public static Optional<T> empty() {
            return new();
        }
        
        public static Optional<T> of(T value) {
            // TODO: require non-null?
            return new Optional<T>(value);
        }
        
        public static Optional<T> ofNullable(T value) {
            return value == null ? new Optional<T>() : new Optional<T>(value);
        }

        public override bool Equals(object other) {
            if (other is Optional<T> o) {
                return _present == o._present && _value.Equals(o._value);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return _present ? _value.GetHashCode() : -1;
        }
    }

}