
namespace java.net {
    public class URISyntaxException : java.lang.Exception {
        public URISyntaxException()  {
           
        }
        public URISyntaxException(string str) : base(str) {
            
        }
        // public URISyntaxException(string str, java.lang.Throwable cause) : base(str, cause) {
        //
        // }
        
        public URISyntaxException(string uri, string message) : base(uri + ": " + message) {

        }

        public string getReason() {
            return getMessage();
        }

        public string getInput() {
            return "";
        }

    }
}