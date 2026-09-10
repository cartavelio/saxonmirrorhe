using System.Threading;

namespace Saxon.Impl.Helpers {
    public class AtomicInteger {

        private int value;

        public AtomicInteger() {
            value = 0;
        }

        public AtomicInteger(int value) {
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