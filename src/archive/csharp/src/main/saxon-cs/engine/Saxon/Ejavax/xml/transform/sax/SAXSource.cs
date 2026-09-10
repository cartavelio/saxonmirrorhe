
namespace Saxon.Ejavax.xml.transform.sax {
    
    // Not used in Saxon-CS, except in "instance of" tests which always fail
    public class SAXSource : Source {

        public string getSystemId() {
            throw new System.NotImplementedException();
        }

        public void setSystemId(string uri) {
            throw new System.NotImplementedException();
        }
    }
}