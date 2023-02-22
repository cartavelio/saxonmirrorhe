using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class ValidationFailure : Saxon.Hej.s9api.Location, Saxon.Hej.type.ConversionResult, Saxon.Hej.lib.Invalidity {
        private string message;
        private string systemId;
        private string publicId;
        private int lineNumber = -1;
        private int columnNumber = -1;
        private Saxon.Hej.om.AbsolutePath path;
        private Saxon.Hej.om.AbsolutePath contextPath;
        private Saxon.Hej.om.NodeInfo invalidNode;
        private System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> offendingNodes;
        private int schemaPart = -1;
        private string constraintName;
        private string clause;
        private Saxon.Hej.type.SchemaType schemaType;
        private Saxon.Hej.om.StructuredQName errorCode;
        private Saxon.Hej.type.ValidationException exception;
        private bool errorHasBeenReported;
        public ValidationFailure(string message)  {
            this.message = message;
            setErrorCode("FORG0001");
        }
        public static Saxon.Hej.type.ValidationFailure fromException(System.Exception exception) {
            if ((exception is Saxon.Hej.type.ValidationException)) {
                return ((Saxon.Hej.type.ValidationException)(exception)).getValidationFailure();
            } else {
                if ((exception is Saxon.Hej.trans.XPathException)) {
                    Saxon.Hej.type.ValidationFailure failure = new Saxon.Hej.type.ValidationFailure(exception.Message);
                    if ((((Saxon.Hej.trans.XPathException)(exception)).getErrorCodeQName() == null)) {
                        failure.setErrorCode("FORG0001");
                    } else {
                        failure.setErrorCodeQName(((Saxon.Hej.trans.XPathException)(exception)).getErrorCodeQName());
                    }
                    failure.setLocator(((Saxon.Hej.trans.XPathException)(exception)).getLocator());
                    return failure;
                } else {
                    return new Saxon.Hej.type.ValidationFailure(exception.Message);
                }
            }
        }
        public void setConstraintReference(int schemaPart, string constraintName, string clause) {
            this.schemaPart = schemaPart;
            this.constraintName = constraintName;
            this.clause = clause;
        }
        public void setConstraintReference(Saxon.Hej.type.ValidationFailure e) {
            schemaPart = e.schemaPart;
            constraintName = e.constraintName;
            clause = e.clause;
        }
        public int getSchemaPart() {
            return schemaPart;
        }
        public string getConstraintName() {
            return constraintName;
        }
        public string getConstraintClauseNumber() {
            return clause;
        }
        public string getConstraintReference() {
            return ((constraintName+'.')+clause);
        }
        public string getConstraintReferenceMessage() {
            if ((schemaPart == -1)) {
                return null;
            }
            return ((((("See http://www.w3.org/TR/xmlschema11-"+schemaPart)+"/#")+constraintName)+" clause ")+clause);
        }
        public void addOffendingNode(Saxon.Hej.om.NodeInfo node) {
            if ((offendingNodes == null)) {
                offendingNodes = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            }
            offendingNodes.Add(node);
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> getOffendingNodes() {
            if ((offendingNodes == null)) {
                return new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            } else {
                return offendingNodes;
            }
        }
        public Saxon.Hej.om.AbsolutePath getPath() {
            return path;
        }
        public void setPath(Saxon.Hej.om.AbsolutePath path) {
            this.path = path;
        }
        public Saxon.Hej.om.AbsolutePath getContextPath() {
            return contextPath;
        }
        public void setContextPath(Saxon.Hej.om.AbsolutePath contextPath) {
            this.contextPath = contextPath;
        }
        public Saxon.Hej.om.NodeInfo getInvalidNode() {
            return invalidNode;
        }
        public void setInvalidNode(Saxon.Hej.om.NodeInfo invalidNode) {
            this.invalidNode = invalidNode;
        }
        public string getMessage() {
            return message;
        }
        public void setMessage(string message) {
            this.message = message;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder("ValidationException: ");
            string message = getMessage();
            if ((message != null)) {
                sb.append(message);
            }
            return sb.ToString();
        }
        public string getPublicId() {
            Saxon.Ejavax.xml.transform.SourceLocator loc = getLocator();
            if ((((publicId == null) && (loc != null)) && (loc != this))) {
                return loc.getPublicId();
            } else {
                return publicId;
            }
        }
        public string getSystemId() {
            Saxon.Ejavax.xml.transform.SourceLocator loc = getLocator();
            if ((((systemId == null) && (loc != null)) && (loc != this))) {
                return loc.getSystemId();
            } else {
                return systemId;
            }
        }
        public int getLineNumber() {
            Saxon.Ejavax.xml.transform.SourceLocator loc = getLocator();
            if ((((lineNumber == -1) && (loc != null)) && (loc != this))) {
                return loc.getLineNumber();
            } else {
                return lineNumber;
            }
        }
        public int getColumnNumber() {
            Saxon.Ejavax.xml.transform.SourceLocator loc = getLocator();
            if ((((columnNumber == -1) && (loc != null)) && (loc != this))) {
                return loc.getColumnNumber();
            } else {
                return columnNumber;
            }
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return new Saxon.Hej.expr.parser.Loc(this);
        }
        public void setPublicId(string id) {
            publicId = id;
        }
        public void setSystemId(string id) {
            systemId = id;
        }
        public void setLineNumber(int line) {
            lineNumber = line;
        }
        public void setColumnNumber(int column) {
            columnNumber = column;
        }
        public void setLocator(Saxon.Ejavax.xml.transform.SourceLocator locator) {
            if ((locator != null)) {
                setPublicId(locator.getPublicId());
                setSystemId(locator.getSystemId());
                setLineNumber(locator.getLineNumber());
                setColumnNumber(locator.getColumnNumber());
            }
        }
        public void setSourceLocator(Saxon.Ejavax.xml.transform.SourceLocator locator) {
            if ((locator != null)) {
                setPublicId(locator.getPublicId());
                setSystemId(locator.getSystemId());
                setLineNumber(locator.getLineNumber());
                setColumnNumber(locator.getColumnNumber());
            }
        }
        public Saxon.Hej.s9api.Location getLocator() {
            return this;
        }
        public void setErrorCode(string errorCode) {
            if ((errorCode == null)) {
                this.errorCode = null;
            } else {
                this.errorCode = new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.om.NamespaceUri.ERR, errorCode);
            }
        }
        public void setErrorCodeQName(Saxon.Hej.om.StructuredQName errorCode) {
            this.errorCode = errorCode;
        }
        public string getErrorCode() {
            if ((errorCode == null)) {
                return null;
            } else {
                if (errorCode.hasURI(Saxon.Hej.om.NamespaceUri.ERR)) {
                    return errorCode.getLocalPart();
                } else {
                    return errorCode.getEQName();
                }
            }
        }
        public Saxon.Hej.om.StructuredQName getErrorCodeQName() {
            return errorCode;
        }
        public void setSchemaType(Saxon.Hej.type.SchemaType type) {
            schemaType = type;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        public Saxon.Hej.type.ValidationException makeException() {
            if ((exception != null)) {
                exception.maybeSetLocation(this);
                return exception;
            }
            Saxon.Hej.type.ValidationException ve = new Saxon.Hej.type.ValidationException(this);
            if ((errorCode == null)) {
                ve.setErrorCode("FORG0001");
            } else {
                ve.setErrorCodeQName(errorCode);
            }
            ve.setHasBeenReported(errorHasBeenReported);
            exception = ve;
            return ve;
        }
        public Saxon.Hej.value.AtomicValue asAtomic() {
            throw makeException();
        }
        public bool hasBeenReported() {
            return errorHasBeenReported;
        }
        public void setHasBeenReported(bool reported) {
            errorHasBeenReported = reported;
            if ((exception != null)) {
                exception.setHasBeenReported(reported);
            }
        }
        public string getValidationLocationText() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            Saxon.Hej.om.AbsolutePath valPath = getAbsolutePath();
            if ((valPath != null)) {
                fsb.append("Validating ");
                fsb.append(valPath.getPathUsingPrefixes());
                if ((valPath.getSystemId() != null)) {
                    fsb.append(" in ");
                    fsb.append(valPath.getSystemId());
                }
            }
            return fsb.ToString();
        }
        public string getContextLocationText() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            Saxon.Hej.om.AbsolutePath contextPath = getContextPath();
            if ((contextPath != null)) {
                fsb.append("Currently processing ");
                fsb.append(contextPath.getPathUsingPrefixes());
                if ((contextPath.getSystemId() != null)) {
                    fsb.append(" in ");
                    fsb.append(contextPath.getSystemId());
                }
            }
            return fsb.ToString();
        }
        public Saxon.Hej.om.AbsolutePath getAbsolutePath() {
            if ((path != null)) {
                return path;
            } else {
                return null;
            }
        }
    
    }
}