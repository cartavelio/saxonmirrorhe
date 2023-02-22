namespace java.net {
    public class MalformedURLException : java.lang.Exception {
        public MalformedURLException()  {
           
        }
        public MalformedURLException(string str) : base(str) {
            
        }
        // public URISyntaxException(string str, java.lang.Throwable cause) : base(str, cause) {
        //
        // }
        
        public MalformedURLException(string uri, string message) : base(uri + ": " + message) {

        }

        public string getReason() {
            return getMessage();
        }

        public string getInput() {
            return "";
        }

    }
}