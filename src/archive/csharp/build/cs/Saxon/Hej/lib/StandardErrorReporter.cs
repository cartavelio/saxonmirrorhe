using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardErrorReporter : Saxon.Hej.lib.StandardDiagnostics, Saxon.Hej.lib.ErrorReporter {
        private int warningCount = 0;
        private int maximumNumberOfWarnings = 25;
        private int errorCount = 0;
        private int maximumNumberOfErrors = 1000;
        private int maxOrdinaryCharacter = 255;
        private int stackTraceDetail = 2;
        private readonly System.Collections.Generic.ISet<string> warningsIssued = new System.Collections.Generic.HashSet<string>();
        protected internal Saxon.Hej.lib.Logger logger = new Saxon.Callbacks.StandardLogger();
        private Saxon.Hej.s9api.XmlProcessingError latestError;
        private bool outputErrorCodes = true;
        private System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> suppressedWarnings;
        public StandardErrorReporter()  {
        }
        public void setLogger(Saxon.Hej.lib.Logger logger) {
            this.logger = logger;
        }
        public Saxon.Hej.lib.Logger getLogger() {
            return logger;
        }
        public void setMaximumNumberOfWarnings(int max) {
            this.maximumNumberOfWarnings = max;
        }
        public int getMaximumNumberOfWarnings() {
            return this.maximumNumberOfWarnings;
        }
        public void setMaximumNumberOfErrors(int max) {
            this.maximumNumberOfErrors = max;
        }
        public int getMaximumNumberOfErrors() {
            return this.maximumNumberOfErrors;
        }
        public void setMaxOrdinaryCharacter(int max) {
            maxOrdinaryCharacter = max;
        }
        public int getMaxOrdinaryCharacter() {
            return maxOrdinaryCharacter;
        }
        public void setStackTraceDetail(int level) {
            stackTraceDetail = level;
        }
        public int getStackTraceDetail() {
            return stackTraceDetail;
        }
        public void setOutputErrorCodes(bool include) {
            this.outputErrorCodes = include;
        }
        public void suppressWarning(string code) {
            if ((suppressedWarnings == null)) {
                suppressedWarnings = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
            }
            if (code.StartsWith("Q{")) {
                suppressedWarnings.Add(Saxon.Hej.om.StructuredQName.fromEQName(code));
            } else {
                suppressedWarnings.Add(new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, code));
            }
        }
        public bool isSuppressedWarning(Saxon.Hej.om.StructuredQName errorCode) {
            return ((suppressedWarnings != null) && suppressedWarnings.Contains(errorCode));
        }
        public void report(Saxon.Hej.s9api.XmlProcessingError processingError) {
            if ((processingError != latestError)) {
                latestError = processingError;
                if (processingError.isWarning()) {
                    if (((processingError.getErrorCode() == null) || !(isSuppressedWarning(processingError.getErrorCode().GetStructuredQName())))) {
                        warning(processingError);
                    }
                } else {
                    error(processingError);
                }
            }
        }
        protected internal void warning(Saxon.Hej.s9api.XmlProcessingError error) {
            if ((logger == null)) {
                logger = new Saxon.Callbacks.StandardLogger();
            }
            string message = constructMessage(error, "", "Warning ");
            if (!(warningsIssued.Contains(message))) {
                logger.warning(message);
                warningCount++;
                if ((warningCount>getMaximumNumberOfWarnings())) {
                    logger.info("No more warnings will be displayed");
                }
                warningsIssued.Add(message);
            }
        }
        public bool isReportingWarnings() {
            return (warningCount<getMaximumNumberOfWarnings());
        }
        public int getNumberOfWarnings() {
            return warningCount;
        }
        protected internal void error(Saxon.Hej.s9api.XmlProcessingError err) {
            if ((errorCount++>maximumNumberOfErrors)) {
                err.setTerminationMessage("Too many errors reported");
            }
            if ((logger == null)) {
                logger = new Saxon.Callbacks.StandardLogger();
            }
            string message;
            Saxon.Api.HostLanguage lang = err.getHostLanguage();
            string langText = "";
            if ((lang != Saxon.Api.HostLanguage.UNKNOWN)) {
                switch (lang) {
                  case Saxon.Api.HostLanguage.XSLT:
                    break ;
                  case Saxon.Api.HostLanguage.XQUERY:
                    langText = "in query ";
                    break ;
                  case Saxon.Api.HostLanguage.XPATH:
                    langText = "in expression ";
                    break ;
                  case Saxon.Api.HostLanguage.XML_SCHEMA:
                    langText = "in schema ";
                    break ;
                  case Saxon.Api.HostLanguage.XSLT_PATTERN:
                    langText = "in pattern ";
                    break ;
                }
            }
            string kind = "Error ";
            if (err.isTypeError()) {
                kind = "Type error ";
            } else {
                if (err.isStaticError()) {
                    kind = "Static error ";
                }
            }
            message = constructMessage(err, langText, kind);
            logger.error(message);
            if ((err is Saxon.Hej.trans.XmlProcessingException)) {
                Saxon.Hej.trans.XPathException exception = ((Saxon.Hej.trans.XmlProcessingException)(err)).getXPathException();
                Saxon.Hej.expr.XPathContext context = exception.getXPathContext();
                if (((context != null) && !(((context is Saxon.Hej.expr.EarlyEvaluationContext))))) {
                    outputStackTrace(logger, context);
                }
            }
        }
        public int getNumberOfErrors() {
            return errorCount;
        }
        public string constructMessage(Saxon.Hej.s9api.XmlProcessingError exception, string langText, string kind) {
            return ((constructFirstLine(exception, langText, kind)+"\n  ")+constructSecondLine(exception));
        }
        public string constructFirstLine(Saxon.Hej.s9api.XmlProcessingError error, string langText, string kind) {
            Saxon.Hej.s9api.Location locator = error.getLocation();
            if ((locator is Saxon.Hej.tree.AttributeLocation)) {
                return ((kind+langText)+getLocationMessageText(locator));
            } else {
                if ((locator is Saxon.Hej.expr.parser.XPathParser.NestedLocation)) {
                    Saxon.Hej.expr.parser.XPathParser.NestedLocation nestedLoc = (Saxon.Hej.expr.parser.XPathParser.NestedLocation)(locator);
                    Saxon.Hej.s9api.Location outerLoc = nestedLoc.getContainingLocation();
                    int line = nestedLoc.getLocalLineNumber();
                    int column = nestedLoc.getColumnNumber();
                    string lineInfo = ((line<=0) ? "" : (("on line "+line)+' '));
                    string columnInfo = ((column<=0) ? "" : ((("at "+(((line<=0) ? "char " : "column ")))+column)+' '));
                    string nearBy = nestedLoc.getNearbyText();
                    Saxon.Hej.expr.Expression failingExpression = null;
                    string extraContext = formatExtraContext(failingExpression, nearBy);
                    if ((outerLoc is Saxon.Hej.tree.AttributeLocation)) {
                        string innerLoc = ((lineInfo+extraContext)+columnInfo);
                        return (((kind+innerLoc)+langText)+getLocationMessageText(outerLoc));
                    } else {
                        string innerLoc = (lineInfo+columnInfo);
                        if ((outerLoc.getLineNumber()>1)) {
                            innerLoc += (((("("+langText)+"on line ")+outerLoc.getLineNumber())+") ");
                        }
                        if ((outerLoc.getSystemId() != null)) {
                            innerLoc += (("of "+outerLoc.getSystemId())+" ");
                        }
                        return ((kind+extraContext)+innerLoc);
                    }
                } else {
                    return (kind+getLocationMessage(error));
                }
            }
        }
        public string formatExtraContext(Saxon.Hej.expr.Expression failingExpression, string nearBy) {
            if ((failingExpression != null)) {
                if (failingExpression.isCallOn(typeof(Saxon.Hej.functions.Error))) {
                    return "signaled by call to error() ";
                } else {
                    return (("evaluating ("+failingExpression.toShortString())+") ");
                }
            } else {
                if (((nearBy != null) && !(nearBy.Length==0))) {
                    return (((((nearBy.StartsWith("...") ? "near" : "in"))+' ')+Saxon.Hej.trans.Err.wrap(nearBy))+" ");
                } else {
                    return "";
                }
            }
        }
        public string constructSecondLine(Saxon.Hej.s9api.XmlProcessingError err) {
            return expandSpecialCharacters(wordWrap(getExpandedMessage(err)));
        }
        protected internal string getLocationMessage(Saxon.Hej.s9api.XmlProcessingError err) {
            Saxon.Hej.s9api.Location loc = err.getLocation();
            return getLocationMessageText(loc);
        }
        public string getExpandedMessage(Saxon.Hej.s9api.XmlProcessingError err) {
            string message = ((formatErrorCode(err)+" ")+err.getMessage());
            message = formatNestedMessages(err, message);
            return message;
        }
        public string formatNestedMessages(Saxon.Hej.s9api.XmlProcessingError err, string message) {
            if ((err.getCause() == null)) {
                return message;
            } else {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(message);
                System.Exception e = err.getCause();
                while ((e != null)) {
                    if (!((isSAXParseException(e)))) {
                        if ((e is System.Exception)) {
                            System.IO.StringWriter sw = new System.IO.StringWriter();
                            appendStackTrace(e, sw);
                            sb.append('\n').append(sw);
                        } else {
                            if (!(message.Contains(e.Message))) {
                                sb.append(". Caused by ").append(e.GetType().Name);
                            }
                        }
                    }
                    string next = e.Message;
                    if ((next != null)) {
                        sb.append(": ").append(next);
                    }
                    e = e.InnerException;
                }
                return sb.ToString();
            }
        }
        private void appendStackTrace(System.Exception e, System.IO.StringWriter sw) {
            sw.WriteLine(e.StackTrace);
        }
        private bool isSAXParseException(System.Exception err) {
            return false;
        }
        public string formatErrorCode(Saxon.Hej.s9api.XmlProcessingError err) {
            if (outputErrorCodes) {
                Saxon.Api.QName qCode = err.getErrorCode();
                if ((qCode != null)) {
                    if (qCode.NamespaceUri.Equals(Saxon.Hej.om.NamespaceUri.ERR)) {
                        return (qCode.LocalName+" ");
                    } else {
                        return (qCode.ToString()+" ");
                    }
                }
            }
            return "";
        }
        public string expandSpecialCharacters(string @in) {
            if (logger.isUnicodeAware()) {
                return @in;
            } else {
                return expandSpecialCharacters(@in, maxOrdinaryCharacter);
            }
        }
        protected internal void outputStackTrace(Saxon.Hej.lib.Logger @out, Saxon.Hej.expr.XPathContext context) {
            logStackTrace(context, @out, stackTraceDetail);
        }
    
    }
}