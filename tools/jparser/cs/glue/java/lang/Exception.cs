using SysExcept = System.Exception;
namespace java.lang {
    public class Exception : Throwable {
        public Exception()  {
           
        }
        public Exception(string str) : base(str) {
            
        }
        public Exception(Throwable cause) : base("", cause) {

        }
        public Exception(string str, Throwable cause) : base(str, cause) {

        }

    }
}