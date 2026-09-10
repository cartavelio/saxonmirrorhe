
namespace Saxon.Ejava.net {
    public class URISyntaxException : Saxon.Ejava.lang.Exception {
        public URISyntaxException()  {
           
        }
        public URISyntaxException(string str) : base(str) {
            
        }
        // public URISyntaxException(string str, Saxon.Ejava.lang.Throwable cause) : base(str, cause) {
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