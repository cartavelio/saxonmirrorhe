using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ErrorExpression : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.s9api.XmlProcessingError exception;
        private Saxon.Hej.expr.Expression original;
        public ErrorExpression() : this ("Unspecified error", "XXXX9999", false) {
        }
        public ErrorExpression(string message, string errorCode, bool isTypeError) : this (new Saxon.Hej.trans.XmlProcessingIncident(message, errorCode)) {
            ((Saxon.Hej.trans.XmlProcessingIncident)(exception)).setTypeError(isTypeError);
        }
        public ErrorExpression(Saxon.Hej.s9api.XmlProcessingError exception)  {
            this.exception = exception;
        }
        public Saxon.Hej.s9api.XmlProcessingError getException() {
            return exception;
        }
        public bool isTypeError() {
            return exception.isTypeError();
        }
        public string getMessage() {
            return exception.getMessage();
        }
        public string getErrorCodeLocalPart() {
            return exception.getErrorCode().LocalName;
        }
        public void setOriginalExpression(Saxon.Hej.expr.Expression original) {
            this.original = original;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override int getImplementationMethod() {
            return (EVALUATE_METHOD|ITERATE_METHOD);
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(exception.getMessage());
            err.setLocation(exception.getLocation());
            err.maybeSetLocation(getLocation());
            if ((exception.getErrorCode() != null)) {
                err.setErrorCodeQName(exception.getErrorCode().GetStructuredQName());
            }
            err.maybeSetContext(context);
            err.setIsTypeError(exception.isTypeError());
            throw err;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            evaluateItem(context);
            return null;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ErrorExpression e2 = new Saxon.Hej.expr.ErrorExpression(exception);
            e2.setOriginalExpression(original);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
            return e2;
        }
        public override string getExpressionName() {
            return "errorExpr";
        }
        public override string ToString() {
            if ((original != null)) {
                return original.ToString();
            } else {
                return (("error(\""+getMessage())+"\")");
            }
        }
        public override string toShortString() {
            if ((original != null)) {
                return original.toShortString();
            } else {
                return (("error(\""+getMessage())+"\")");
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("error", this);
            destination.emitAttribute("message", exception.getMessage());
            destination.emitAttribute("code", exception.getErrorCode().LocalName);
            destination.emitAttribute("isTypeErr", (exception.isTypeError() ? "0" : "1"));
            destination.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.ErrorExpression.ErrorExpressionElaborator();
        }
        private class ErrorExpressionElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.ErrorExpression expr = (Saxon.Hej.expr.ErrorExpression)(getExpression());
                Saxon.Hej.s9api.XmlProcessingError exception = expr.getException();
                return context => {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(exception.getMessage());
                    err.setLocation(exception.getLocation());
                    err.maybeSetLocation(expr.getLocation());
                    if ((exception.getErrorCode() != null)) {
                        err.setErrorCodeQName(exception.getErrorCode().GetStructuredQName());
                    }
                    err.maybeSetContext(context);
                    err.setIsTypeError(exception.isTypeError());
                    throw err;
                } ;
            }
        
        }
    
    }
}