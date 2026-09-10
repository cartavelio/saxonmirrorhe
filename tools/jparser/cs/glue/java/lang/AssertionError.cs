namespace java.lang {
    public class AssertionError : Exception {
        public AssertionError()  {
           
        }
        public AssertionError(string str) : base(str) {
            
        }
        public AssertionError(Throwable cause) : base(cause) {

        }
        public AssertionError(string str, Throwable cause) : base(str, cause) {

        }

    }
}