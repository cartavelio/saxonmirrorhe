using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class XmlProcessingIncident : Saxon.Hej.s9api.XmlProcessingError {
        private readonly string message;
        private string errorCode;
        private System.Exception cause;
        private Saxon.Hej.s9api.Location locator = null;
        private bool _isWarning;
        private bool _isTypeError;
        private string fatalErrorMessage;
        private bool _hasBeenReported = false;
        private Saxon.Api.HostLanguage hostLanguage = Saxon.Api.HostLanguage.UNKNOWN;
        private bool _isStaticError;
        public XmlProcessingIncident(string message, string errorCode, Saxon.Hej.s9api.Location location)  {
            Saxon.Ejava.util.Objects.requireNonNull(message);
            Saxon.Ejava.util.Objects.requireNonNull(errorCode);
            Saxon.Ejava.util.Objects.requireNonNull(location);
            this.message = message;
            setErrorCodeAsEQName(errorCode);
            this.locator = location;
            this._isWarning = false;
        }
        public XmlProcessingIncident(string message)  {
            this.message = message;
        }
        public XmlProcessingIncident(string message, string errorCode)  {
            this.message = message;
            setErrorCodeAsEQName(errorCode);
        }
        public XmlProcessingIncident(Saxon.Ejavax.xml.transform.TransformerException err, bool isWarning)  {
            Saxon.Hej.trans.XPathException exception = Saxon.Hej.trans.XPathException.makeXPathException(err);
            message = exception.Message;
            errorCode = exception.getErrorCodeQName().getEQName();
            locator = exception.getLocator();
            this._isWarning = isWarning;
        }
        public void setWarning(bool warning) {
            _isWarning = warning;
        }
        public Saxon.Hej.s9api.XmlProcessingError asWarning() {
            _isWarning = true;
            return this;
        }
        public void setTerminationMessage(string message) {
            fatalErrorMessage = message;
        }
        public string getTerminationMessage() {
            return fatalErrorMessage;
        }
        public bool isAlreadyReported() {
            return _hasBeenReported;
        }
        public void setAlreadyReported(bool reported) {
            this._hasBeenReported = reported;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return hostLanguage;
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            this.hostLanguage = language;
        }
        public bool isTypeError() {
            return _isTypeError;
        }
        public void setTypeError(bool isTypeError) {
            this._isTypeError = isTypeError;
        }
        public bool isStaticError() {
            return _isStaticError;
        }
        public void setStaticError(bool isStaticError) {
            this._isStaticError = isStaticError;
        }
        public Saxon.Api.QName getErrorCode() {
            if ((errorCode == null)) {
                return null;
            }
            return new Saxon.Api.QName(Saxon.Hej.om.StructuredQName.fromEQName((errorCode)));
        }
        public void setErrorCodeAsEQName(string code) {
            if (code.StartsWith("Q{")) {
                this.errorCode = code;
            } else {
                if (Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(code))) {
                    this.errorCode = ((("Q{"+Saxon.Hej.lib.NamespaceConstant.ERR)+"}")+code);
                } else {
                    this.errorCode = (("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}invalid-error-code");
                }
            }
        }
        public string getMessage() {
            return message;
        }
        public string getModuleUri() {
            return getLocation().getSystemId();
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return locator;
        }
        public void setLocation(Saxon.Hej.s9api.Location loc) {
            this.locator = loc;
        }
        public int getColumnNumber() {
            Saxon.Hej.s9api.Location locator = getLocation();
            if ((locator != null)) {
                return locator.getColumnNumber();
            }
            return -1;
        }
        public int getLineNumber() {
            Saxon.Hej.s9api.Location locator = getLocation();
            if ((locator != null)) {
                return locator.getLineNumber();
            }
            return -1;
        }
        public string getInstructionName() {
            return ((Saxon.Hej.om.NodeInfo)(locator)).getDisplayName();
        }
        public bool isWarning() {
            return _isWarning;
        }
        public string getPath() {
            if ((locator is Saxon.Hej.om.NodeInfo)) {
                return Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(locator));
            } else {
                return null;
            }
        }
        public System.Exception getCause() {
            return cause;
        }
        public void setCause(System.Exception cause) {
            this.cause = cause;
        }
        public static void maybeSetHostLanguage(Saxon.Hej.s9api.XmlProcessingError error, Saxon.Api.HostLanguage lang) {
            if ((error.getHostLanguage() == Saxon.Api.HostLanguage.UNKNOWN)) {
                if ((error is Saxon.Hej.trans.XmlProcessingIncident)) {
                    ((Saxon.Hej.trans.XmlProcessingIncident)(error)).setHostLanguage(lang);
                } else {
                    if ((error is Saxon.Hej.trans.XmlProcessingException)) {
                        ((Saxon.Hej.trans.XmlProcessingException)(error)).getXPathException().setHostLanguage(lang);
                    }
                }
            }
        }
        public static void maybeSetLocation(Saxon.Hej.s9api.XmlProcessingError error, Saxon.Hej.s9api.Location loc) {
            if ((error.getLocation() == null)) {
                if ((error is Saxon.Hej.trans.XmlProcessingIncident)) {
                    ((Saxon.Hej.trans.XmlProcessingIncident)(error)).setLocation(loc);
                } else {
                    if ((error is Saxon.Hej.trans.XmlProcessingException)) {
                        ((Saxon.Hej.trans.XmlProcessingException)(error)).getXPathException().setLocation(loc);
                    }
                }
            }
        }
    
    }
}