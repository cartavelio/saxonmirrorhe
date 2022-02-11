using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class ValidationException : Saxon.Hej.trans.XPathException {
        private Saxon.Hej.type.ValidationFailure failure;
        public ValidationException(System.Exception exception) : base (exception) {
        }
        public ValidationException(string message, System.Exception exception) : base (message, exception) {
        }
        public ValidationException(string message, Saxon.Hej.s9api.Location locator) : base (message, null, locator) {
        }
        public ValidationException(Saxon.Hej.type.ValidationFailure failure) : base (failure.getMessage(), failure.getErrorCode(), failure.getLocator()) {
            this.failure = failure;
        }
        public override string getMessage() {
            if ((failure != null)) {
                return failure.getMessage();
            } else {
                return base.Message;
            }
        }
        public Saxon.Hej.type.ValidationFailure getValidationFailure() {
            if ((failure != null)) {
                return failure;
            } else {
                Saxon.Hej.type.ValidationFailure failure = new Saxon.Hej.type.ValidationFailure(Message);
                failure.setErrorCodeQName(getErrorCodeQName());
                failure.setLocator(getLocator());
                return failure;
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder("ValidationException: ");
            string message = Message;
            if ((message != null)) {
                sb.append(message);
            }
            return sb.ToString();
        }
        public Saxon.Hej.om.NodeInfo getNode() {
            if ((failure != null)) {
                return failure.getInvalidNode();
            } else {
                return null;
            }
        }
        public string getPath() {
            Saxon.Hej.om.AbsolutePath ap = getAbsolutePath();
            if ((ap == null)) {
                Saxon.Hej.om.NodeInfo node = getNode();
                if ((node != null)) {
                    return Saxon.Hej.tree.util.Navigator.getPath(node);
                } else {
                    return null;
                }
            } else {
                return ap.getPathUsingAbbreviatedUris();
            }
        }
        public Saxon.Hej.om.AbsolutePath getAbsolutePath() {
            if ((failure != null)) {
                return failure.getPath();
            } else {
                return null;
            }
        }
    
    }
}