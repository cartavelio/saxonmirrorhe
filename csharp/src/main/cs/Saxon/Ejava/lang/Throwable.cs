using System;
using SysEx = System.Exception;
namespace Saxon.Ejava.lang {
    public class Throwable : SysEx {

        public Throwable() : base () {

        }
        public Throwable(string str) : base (str) {

        }
        public Throwable(string str, SysEx cause) : base (str, cause) {

        }
        
        public Throwable(SysEx cause) : base(cause.Message, cause) {
 
        }

        public virtual string getMessage() {
            return Message;
        }

        public SysEx getCause() {
            return InnerException;
        }
    }
}