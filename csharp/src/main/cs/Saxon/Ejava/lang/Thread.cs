using System;
using SysThread = System.Threading;
namespace Saxon.Ejava.lang {
    public class Thread : Runnable {
        private SysThread.Thread _thread;
        private Runnable _runnable;

        public Thread(SysThread.Thread thread) {
            this._thread = thread;
        }

        public Thread(Runnable runnable) {
            this._runnable = runnable;
        }

        public void start() {
            this._thread.Start();
        }

        public void run() {
            if (_runnable != null) {
                _runnable.run();
            }
            else {
                throw new NotImplementedException();
            }
        }

        public void interrupt() {
            this._thread.Interrupt();
        }

        public bool isInterrupted() {
            return this._thread.ThreadState == SysThread.ThreadState.Aborted;
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