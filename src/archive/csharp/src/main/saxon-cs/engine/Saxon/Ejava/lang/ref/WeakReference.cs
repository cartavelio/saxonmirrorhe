using Sys = System;
namespace Saxon.Ejava.lang.@ref {
    public class WeakReference<T> where T : class {
        private readonly Sys.WeakReference<T> _inner;

        public WeakReference(T o) {
            _inner = new Sys.WeakReference<T>(o);
        }

        public T get() {
            bool exists = _inner.TryGetTarget(out var target);
            return exists ? target : null;
        }
    }
}