using SysExcept = System.Exception;
namespace java.lang {
    public class Exception : Throwable {
        public Exception()  {
           
        }
        public Exception(string str) : base(str) {
            
        }
        public Exception(SysExcept cause) : base("", cause) {

        }
        public Exception(string str, SysExcept cause) : base(str, cause) {

        }

    }
}