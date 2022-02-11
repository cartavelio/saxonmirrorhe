using SysExcept = System.Exception;
namespace java.lang {
    public class IllegalArgumentException : Exception {
        public IllegalArgumentException()  {
           
        }
        public IllegalArgumentException(string str) : base(str) {
            
        }
        public IllegalArgumentException(string str, SysExcept cause) : base(str, cause) {

        }
        public IllegalArgumentException(SysExcept cause) : base(cause) {

        }

    }
}