using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class ValidatingFilter : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Eej.ee.validate.ValidationContext validationContext;
        internal bool locallyInvalid = false;
        internal ValidatingFilter(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public Saxon.Eej.ee.validate.ValidationContext getValidationContext() {
            return validationContext;
        }
        public void setValidationContext(Saxon.Eej.ee.validate.ValidationContext validationContext) {
            this.validationContext = validationContext;
        }
        public void setConstraintChecker(Saxon.Eej.ee.validate.ConstraintChecker checker) {
            validationContext.setConstraintChecker(checker);
        }
        protected internal virtual Saxon.Eej.ee.validate.ConstraintChecker getConstraintChecker() {
            return validationContext.getConstraintChecker();
        }
        public void setStartTagBuffer(Saxon.Eej.ee.validate.AttributeInheritor startTag) {
            validationContext.setAttributeInheritor(startTag);
        }
        public Saxon.Eej.ee.validate.AttributeInheritor getAttributeInheritor() {
            return validationContext.getAttributeInheritor();
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            decompose(item, locationId, copyNamespaces);
        }
        protected internal void reportValidationError(Saxon.Hej.type.ValidationFailure err, bool issueComment, Saxon.Hej.s9api.Location locationId) {
            if (err.hasBeenReported()) {
                return ;
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            if (Saxon.Hej.expr.parser.Loc.isUnknown(locationId)) {
                err.setSourceLocator(validationContext.getInitiatingLocation());
            } else {
                err.setSourceLocator(locationId);
            }
            if ((err.getErrorCodeQName() == null)) {
                err.setErrorCode(validationContext.getErrorCode());
            }
            Saxon.Hej.@event.PathMaintainer pm = (Saxon.Hej.@event.PathMaintainer)(pipe.getComponent("net.sf.saxon.event.PathMaintainer"));
            if ((pm != null)) {
                Saxon.Hej.om.AbsolutePath path = pm.getAbsolutePath();
                if ((locationId is Saxon.Hej.tree.AttributeLocation)) {
                    Saxon.Hej.om.NodeName attName = new Saxon.Hej.om.FingerprintedQName(((Saxon.Hej.tree.AttributeLocation)(locationId)).getAttributeName(), getNamePool());
                    path.appendAttributeName(attName);
                }
                err.setPath(path);
            } else {
                if ((locationId is Saxon.Hej.tree.AttributeLocation)) {
                    string attLoc = ("@"+((Saxon.Hej.tree.AttributeLocation)(locationId)).getAttributeName().getDisplayName());
                    Saxon.Hej.om.StructuredQName elName = ((Saxon.Hej.tree.AttributeLocation)(locationId)).getElementName();
                    if ((elName != null)) {
                        attLoc = ((elName.getDisplayName()+"/")+attLoc);
                    }
                    string message = ((("At "+attLoc)+": ")+err.getMessage());
                    err.setMessage(message);
                }
            }
            Saxon.Hej.expr.XPathContext context = pipe.getXPathContext();
            while ((context != null)) {
                Saxon.Hej.om.Item current = context.getContextItem();
                if ((current is Saxon.Hej.om.NodeInfo)) {
                    err.setContextPath(Saxon.Hej.tree.util.Navigator.getAbsolutePath((Saxon.Hej.om.NodeInfo)(current)));
                    break ;
                }
                context = context.getCaller();
            }
            Saxon.Hej.lib.InvalidityHandler handler = validationContext.getInvalidityHandler();
            if ((handler == null)) {
                handler = new Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter(pipe.getErrorReporter());
            }
            handler.reportInvalidity(err);
            err.setHasBeenReported(true);
            Saxon.Eej.ee.validate.AssertionTreeBuilder atb = validationContext.getAssertionTreeBuilder();
            if ((atb != null)) {
                atb.markAncestorNodesUntyped();
            }
            if (validationContext.incrementErrorCount(err)) {
                throw new Saxon.Hej.trans.XPathException("Validation terminated - too many errors");
            }
            if ((issueComment && pipe.getParseOptions().isAddCommentsAfterValidationErrors())) {
                comment(Saxon.Hej.str.StringView.of((("\nVALIDATION ERROR: "+err.getMessage())+'\n')), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        protected internal void reportIfInvalid() {
            if ((((validationContext != null) && (validationContext.getErrorCount()>0)) && !(getPipelineConfiguration().isRecoverFromValidationErrors()))) {
                string message;
                if ((validationContext.getErrorCount() == 1)) {
                    message = "One validation error was reported: ";
                } else {
                    message = (new Saxon.Hej.expr.number.Numberer_en().toWords(validationContext.getErrorCount())+" validation errors were reported. First error: ");
                }
                message += validationContext.getFirstFailure().getMessage();
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure(message);
                string firstErrorCode = validationContext.getFirstFailure().getErrorCode();
                if (((firstErrorCode != null) && !(firstErrorCode.StartsWith("FO")))) {
                    err.setErrorCode(firstErrorCode);
                } else {
                    if ((validationContext.getErrorCode() != null)) {
                        err.setErrorCode(validationContext.getErrorCode());
                    } else {
                        err.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE1510" : "XQDY0027"));
                    }
                }
                Saxon.Hej.type.ValidationException ve = err.makeException();
                ve.setErrorObject(validationContext.getValidationErrorObject());
                throw ve;
            }
        }
    
    }
}