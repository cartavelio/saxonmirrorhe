using Saxon.Ejava.lang;

namespace Saxon.Ejavax.xml.transform
{
    public class TransformerException : Saxon.Ejava.lang.Exception {
        private SourceLocator _locator;
        
        public TransformerException(string message) : base(message) {
            
        }
        public TransformerException(System.Exception cause) : base(cause) {
            
        }
        public TransformerException(string message, SourceLocator locator) : base(message) {
            this._locator = locator;
        }
        public TransformerException(string message, SourceLocator locator, System.Exception cause) : base(message, cause) {
            this._locator = locator;
        }
        public TransformerException(string message, System.Exception cause) : base(message, cause) {
            
        }
        public TransformerException(Saxon.Ejava.lang.Throwable cause) : base("", cause) {
            
        }

        public virtual SourceLocator getLocator() {
            return _locator;
        }

        public void setLocator(SourceLocator loc) {
            this._locator = loc;
        }

        public System.Exception getException() {
            return this.getCause();
        }

        public string getMessageAndLocation() {
            return "Error at " + _locator.getSystemId() + "#" + _locator.getLineNumber() + ": " +
                   getMessage();
        }
        
    }
}