using Sys = System
namespace java.lang.@ref {
    public class WeakReference<T> {
        private Sys.WeakReference<T> _inner;

        public WeakReference(T o) {
            _inner = new Sys.WeakReference<>(o);
        }

        public T get() {
            return _inner
        }
    }
}