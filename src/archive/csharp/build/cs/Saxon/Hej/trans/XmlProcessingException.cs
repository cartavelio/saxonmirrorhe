using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class XmlProcessingException : Saxon.Hej.s9api.XmlProcessingError {
        private readonly Saxon.Hej.trans.XPathException exception;
        private bool _isWarning;
        private string fatalErrorMessage;
        public XmlProcessingException(Saxon.Hej.trans.XPathException exception)  {
            this.exception = exception;
        }
        public Saxon.Hej.trans.XPathException getXPathException() {
            return exception;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            Saxon.Hej.s9api.Location loc = getLocation();
            if (((loc is Saxon.Hej.expr.instruct.Instruction) || (loc is Saxon.Hej.tree.AttributeLocation))) {
                return Saxon.Api.HostLanguage.XSLT;
            } else {
                return Saxon.Api.HostLanguage.XPATH;
            }
        }
        public bool isStaticError() {
            return exception.isStaticError();
        }
        public bool isTypeError() {
            return exception.isTypeError();
        }
        public Saxon.Api.QName getErrorCode() {
            Saxon.Hej.om.StructuredQName errorCodeQName = exception.getErrorCodeQName();
            return ((errorCodeQName == null) ? null : new Saxon.Api.QName(errorCodeQName));
        }
        public string getMessage() {
            return exception.Message;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return ((exception.getLocator() == null) ? Saxon.Hej.expr.parser.Loc.NONE : exception.getLocator());
        }
        public bool isWarning() {
            return _isWarning;
        }
        public string getPath() {
            return null;
        }
        public System.Exception getCause() {
            return exception.InnerException;
        }
        public void setWarning(bool warning) {
            _isWarning = warning;
        }
        public Saxon.Hej.s9api.XmlProcessingError asWarning() {
            Saxon.Hej.trans.XmlProcessingException e2 = new Saxon.Hej.trans.XmlProcessingException(exception);
            e2.setWarning(true);
            return e2;
        }
        public void setTerminationMessage(string message) {
            this.fatalErrorMessage = message;
        }
        public string getTerminationMessage() {
            return this.fatalErrorMessage;
        }
        public bool isAlreadyReported() {
            return exception.hasBeenReported();
        }
        public void setAlreadyReported(bool reported) {
            exception.setHasBeenReported(reported);
        }
    
        /* Expanding default method getModuleUri from interface XmlProcessingError */
        public virtual string getModuleUri() {
            return getLocation().getSystemId();
        }

    }
}