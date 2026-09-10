using System;
using SysEx = System.Exception;
namespace java.lang {
    public class Throwable : SysEx {
        private readonly SysEx _inner;
        public Throwable() {
            _inner = new SysEx();
        }
        public Throwable(string str) {
            _inner = new SysEx(str);
        }
        public Throwable(string str, SysEx cause) {
            _inner = new SysEx(str, cause);
        }

        public string getMessage() {
            return _inner.Message;
        }

        public Throwable getCause() {
            return (Throwable) _inner.InnerException;
        }
    }
}