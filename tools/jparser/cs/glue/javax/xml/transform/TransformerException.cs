using System.Management.Instrumentation;

namespace javax.xml.transform
{
    public class TransformerException : java.lang.Exception {
        private SourceLocator _locator;
        
        public TransformerException(string message) : base(message) {
            
        }
        public TransformerException(string message, SourceLocator locator) : base(message) {
            this._locator = locator;
        }
        public TransformerException(string message, SourceLocator locator, java.lang.Throwable cause) : base(message, cause) {
            this._locator = locator;
        }
        public TransformerException(string message, java.lang.Throwable cause) : base(message, cause) {
            
        }
        public TransformerException(java.lang.Throwable cause) : base("", cause) {
            
        }

        public SourceLocator getLocator() {
            return _locator;
        }

        public void setLocator(SourceLocator loc) {
            this._locator = loc;
        }
        
    }
}