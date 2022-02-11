using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class InvalidityHandlerWrappingErrorReporter : Saxon.Hej.lib.InvalidityHandler {
        private readonly Saxon.Hej.lib.ErrorReporter errorReporter;
        public InvalidityHandlerWrappingErrorReporter(Saxon.Hej.lib.ErrorReporter errorReporter)  {
            this.errorReporter = errorReporter;
        }
        public void startReporting(string systemId) {
        }
        public void reportInvalidity(Saxon.Hej.lib.Invalidity failure) {
            errorReporter.report(new Saxon.Hej.trans.XmlProcessingException(((Saxon.Hej.type.ValidationFailure)(failure)).makeException()));
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return errorReporter;
        }
        public Saxon.Hej.om.Sequence endReporting() {
            return null;
        }
    
    }
}