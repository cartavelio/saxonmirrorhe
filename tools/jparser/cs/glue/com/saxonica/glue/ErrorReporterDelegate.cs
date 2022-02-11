using net.sf.saxon.s9api;
using SysAction = System.Action;

namespace com.saxonica.glue {
    public class ErrorReportingAction : net.sf.saxon.lib.ErrorReporter {
        private System.Action<net.sf.saxon.s9api.XmlProcessingError> reporter;
        
        public ErrorReportingAction(System.Action<net.sf.saxon.s9api.XmlProcessingError> reporter) {
            this.reporter = reporter;
        }

        public void report(XmlProcessingError error) {
            reporter(error);
        }
    }
}