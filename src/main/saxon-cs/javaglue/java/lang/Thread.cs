using SysThread = System.Threading;
namespace java.lang {
    public class Thread {
        private SysThread.Thread _thread;

        public Thread(SysThread.Thread thread) {
            this._thread = thread;
        }

        public long getId() {
            return _thread.GetHashCode();
        }

        public static void sleep(long ms) {
            SysThread.Thread.Sleep((int)ms);
        }
        
        public static Thread currentThread() {
            return new Thread(SysThread.Thread.CurrentThread);
        }
    }
}