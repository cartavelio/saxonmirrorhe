namespace Saxon.Ejava.lang {
    public class RuntimeException : Exception {
        public RuntimeException()  {
           
        }
        public RuntimeException(string str) : base(str) {
            
        }
        public RuntimeException(string str, Throwable cause) : base(str, cause) {

        }
        public RuntimeException(Throwable cause) : base(cause) {

        }

    }
}