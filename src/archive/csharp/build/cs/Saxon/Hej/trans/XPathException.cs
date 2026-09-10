using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class XPathException : Saxon.Ejavax.xml.transform.TransformerException {
        private bool _isTypeError = false;
        private bool _isSyntaxError = false;
        private bool _isStaticError = false;
        private bool _isGlobalError = false;
        private string hostLanguage = null;
        private Saxon.Hej.om.StructuredQName errorCode;
        private Saxon.Hej.om.Sequence errorObject;
        private Saxon.Hej.expr.Expression failingExpression;
        private bool _hasBeenReported = false;
        Saxon.Hej.expr.XPathContext context;
        public XPathException(string message) : base (message) {
            breakPoint();
        }
        public XPathException(System.Exception err) : base (err) {
            breakPoint();
        }
        public XPathException(string message, System.Exception err) : base (message, err) {
            breakPoint();
        }
        public XPathException(string message, string errorCode, Saxon.Hej.s9api.Location loc) : this (message, errorCode) {
            setLocator(loc);
            breakPoint();
        }
        public XPathException(string message, Saxon.Hej.s9api.Location loc, System.Exception err) : base (message, loc, err) {
            breakPoint();
        }
        public XPathException(string message, string errorCode) : base (message) {
            setErrorCode(errorCode);
            breakPoint();
        }
        public XPathException(string message, string errorCode, Saxon.Hej.expr.XPathContext context) : base (message) {
            setErrorCode(errorCode);
            setXPathContext(context);
            breakPoint();
        }
        private static void breakPoint() {
            System.Diagnostics.Debug.Assert(true);
        }
        public static Saxon.Hej.trans.XPathException makeXPathException(System.Exception err) {
            if ((err is Saxon.Hej.trans.XPathException)) {
                return (Saxon.Hej.trans.XPathException)(err);
            } else {
                if ((err.InnerException is Saxon.Hej.trans.XPathException)) {
                    return (Saxon.Hej.trans.XPathException)(err.InnerException);
                } else {
                    if ((err is Saxon.Ejavax.xml.transform.TransformerException)) {
                        Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(err.Message, err);
                        xe.setLocator(((Saxon.Ejavax.xml.transform.TransformerException)(err)).getLocator());
                        return xe;
                    } else {
                        return new Saxon.Hej.trans.XPathException(err);
                    }
                }
            }
        }
        public static Saxon.Hej.trans.XPathException fromXmlProcessingError(Saxon.Hej.s9api.XmlProcessingError error) {
            if ((error is Saxon.Hej.trans.XmlProcessingException)) {
                return ((Saxon.Hej.trans.XmlProcessingException)(error)).getXPathException();
            } else {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(error.getMessage());
                e.setLocation(error.getLocation());
                e.setHostLanguage(error.getHostLanguage());
                e.setIsStaticError(error.isStaticError());
                e.setIsTypeError(error.isTypeError());
                Saxon.Api.QName code = error.getErrorCode();
                if ((code != null)) {
                    e.setErrorCodeQName(code.GetStructuredQName());
                }
                return e;
            }
        }
        public override  /* REPLACING public virtual */ string getMessage() {
            if ((InnerException == null)) {
                return base.Message;
            } else {
                return ((base.Message+": ")+InnerException.Message);
            }
        }
        public void setXPathContext(Saxon.Hej.expr.XPathContext context) {
            this.context = context;
        }
        public void setLocation(Saxon.Hej.s9api.Location loc) {
            if ((loc != null)) {
                setLocator(loc.saveLocation());
            }
        }
        public Saxon.Hej.expr.Expression getFailingExpression() {
            return failingExpression;
        }
        public void setFailingExpression(Saxon.Hej.expr.Expression failingExpression) {
            this.failingExpression = failingExpression;
        }
        public void maybeSetFailingExpression(Saxon.Hej.expr.Expression failingExpression) {
            if ((this.failingExpression == null)) {
                this.failingExpression = failingExpression;
            }
            maybeSetLocation(failingExpression.getLocation());
        }
        public override  /* REPLACING public */ Saxon.Hej.s9api.Location getLocator() {
            Saxon.Ejavax.xml.transform.SourceLocator locator = base.getLocator();
            if ((locator == null)) {
                return null;
            } else {
                if ((locator is Saxon.Hej.s9api.Location)) {
                    return (Saxon.Hej.s9api.Location)(locator);
                } else {
                    return new Saxon.Hej.expr.parser.Loc(locator);
                }
            }
        }
        public Saxon.Hej.expr.XPathContext getXPathContext() {
            return context;
        }
        public void setIsStaticError(bool @is) {
            _isStaticError = @is;
        }
        public bool isStaticError() {
            return _isStaticError;
        }
        public void setIsSyntaxError(bool @is) {
            if (@is) {
                _isStaticError = true;
            }
            _isSyntaxError = @is;
        }
        public bool isSyntaxError() {
            return _isSyntaxError;
        }
        public void setIsTypeError(bool @is) {
            _isTypeError = @is;
        }
        public bool isTypeError() {
            return _isTypeError;
        }
        public void setIsGlobalError(bool @is) {
            _isGlobalError = @is;
        }
        public bool isGlobalError() {
            return _isGlobalError;
        }
        public void setHostLanguage(string language) {
            this.hostLanguage = language;
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            this.hostLanguage = ((language == Saxon.Api.HostLanguage.UNKNOWN) ? null : language.ToString());
        }
        public string getHostLanguage() {
            return hostLanguage;
        }
        public void setErrorCode(string code) {
            if ((code != null)) {
                errorCode = new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.om.NamespaceUri.ERR, code);
            }
        }
        public void maybeSetErrorCode(string code) {
            if (((errorCode == null) && (code != null))) {
                errorCode = new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.om.NamespaceUri.ERR, code);
            }
        }
        public void setErrorCodeQName(Saxon.Hej.om.StructuredQName code) {
            errorCode = code;
        }
        public Saxon.Hej.om.StructuredQName getErrorCodeQName() {
            return errorCode;
        }
        public string getErrorCodeLocalPart() {
            return ((errorCode == null) ? null : errorCode.getLocalPart());
        }
        public Saxon.Hej.om.NamespaceUri getErrorCodeNamespace() {
            return ((errorCode == null) ? null : errorCode.getNamespaceUri());
        }
        public void setErrorObject(Saxon.Hej.om.Sequence value) {
            errorObject = value;
        }
        public Saxon.Hej.om.Sequence getErrorObject() {
            return errorObject;
        }
        public void setHasBeenReported(bool reported) {
            _hasBeenReported = reported;
        }
        public bool hasBeenReported() {
            return _hasBeenReported;
        }
        public void maybeSetLocation(Saxon.Hej.s9api.Location here) {
            if ((here != null)) {
                if ((getLocator() == null)) {
                    setLocator(here.saveLocation());
                } else {
                    if (((getLocator().getLineNumber() == -1) && !(((((getLocator().getSystemId() != null) && (here.getSystemId() != null)) && !(getLocator().getSystemId().Equals(here.getSystemId()))))))) {
                        setLocator(here.saveLocation());
                    }
                }
            }
        }
        public void maybeSetContext(Saxon.Hej.expr.XPathContext context) {
            if ((getXPathContext() == null)) {
                setXPathContext(context);
            }
        }
        public bool isReportableStatically() {
            if ((isStaticError() || isTypeError())) {
                return true;
            }
            Saxon.Hej.om.StructuredQName err = errorCode;
            if (((err != null) && err.hasURI(Saxon.Hej.om.NamespaceUri.ERR))) {
                string local = err.getLocalPart();
                return ((((((local.Equals("XTDE1260") || local.Equals("XTDE1280")) || local.Equals("XTDE1390")) || local.Equals("XTDE1400")) || local.Equals("XTDE1428")) || local.Equals("XTDE1440")) || local.Equals("XTDE1460"));
            }
            return false;
        }
        public class Circularity : Saxon.Hej.trans.XPathException {
            public Circularity(string message) : base (message) {
            }
        
        }
        public class StackOverflow : Saxon.Hej.trans.XPathException {
            public StackOverflow(string message, string errorCode, Saxon.Hej.s9api.Location location) : base (message, errorCode, location) {
            }
        
        }
    
    }
}