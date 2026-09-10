using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class Outcome<T>  {
        private T result;
        private System.Exception exception;
        public Outcome(T result)  {
            this.result = result;
        }
        public Outcome(System.Exception exception)  {
            this.exception = exception;
        }
        public bool isSuccess() {
            return (result != null);
        }
        public T getResult() {
            return result;
        }
        public System.Exception getException() {
            return exception;
        }
    
    }
}