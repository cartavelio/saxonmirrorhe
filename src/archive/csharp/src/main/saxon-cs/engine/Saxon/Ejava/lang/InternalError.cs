namespace Saxon.Ejava.lang {
    public class InternalError : Exception {
        public InternalError()  {
           
        }
        public InternalError(string str) : base(str) {
            
        }
        public InternalError(string str, Throwable cause) : base(str, cause) {

        }

    }
}