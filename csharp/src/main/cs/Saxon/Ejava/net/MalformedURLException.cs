namespace Saxon.Ejava.net {
    public class MalformedURLException : Saxon.Ejava.lang.Exception {
        public MalformedURLException()  {
           
        }
        public MalformedURLException(string str) : base(str) {
            
        }
        // public URISyntaxException(string str, Saxon.Ejava.lang.Throwable cause) : base(str, cause) {
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