using SysExcept = System.Exception;
namespace java.lang {
    public class AssertionError : Exception {
        public AssertionError()  {
           
        }
        public AssertionError(string str) : base(str) {
            
        }
        public AssertionError(SysExcept cause) : base(cause) {

        }
        public AssertionError(string str, SysExcept cause) : base(str, cause) {

        }

    }
}