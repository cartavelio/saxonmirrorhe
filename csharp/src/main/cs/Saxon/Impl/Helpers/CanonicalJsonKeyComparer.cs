using System.Threading;

namespace Saxon.Impl.Helpers {
    public class CanonicalJsonKeyComparer {

        private int value;

        public CanonicalJsonKeyComparer() {
            value = 0;
        }

        public CanonicalJsonKeyComparer(int value) {
            this.value = value;
        }

        public int incrementAndGet() {
            return Interlocked.Increment(ref value);
        }
        
        public int get() {
            return Volatile.Read(ref value);
        }
    }
}