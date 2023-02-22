using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class ValidationContext : Saxon.Hej.lib.ConversionRules {
        private Saxon.Hej.type.ValidationParams validationParams;
        private Saxon.Hej.Controller controller;
        private Saxon.Hej.lib.InvalidityHandler invalidityHandler;
        private Saxon.Hej.type.SchemaType xsiType;
        private Saxon.Eej.ee.validate.AttributeInheritor attributeInheritor;
        private Saxon.Eej.ee.validate.ConstraintChecker constraintChecker;
        private string errorCode;
        private int errorCount = 0;
        private int errorLimit = System.Int32.MaxValue;
        private Saxon.Hej.s9api.Location initiatingLocation;
        private Saxon.Hej.type.ValidationFailure firstFailure = null;
        private Saxon.Eej.ee.validate.AssertionTreeBuilder assertionTreeBuilder = null;
        public ValidationContext(Saxon.Hej.lib.ConversionRules rules)  {
            rules.copyTo(this);
        }
        public Saxon.Hej.Controller getController() {
            return controller;
        }
        public void setController(Saxon.Hej.Controller controller) {
            this.controller = controller;
        }
        public Saxon.Hej.lib.InvalidityHandler getInvalidityHandler() {
            return invalidityHandler;
        }
        public void setInvalidityHandler(Saxon.Hej.lib.InvalidityHandler invalidityHandler) {
            this.invalidityHandler = invalidityHandler;
        }
        public Saxon.Eej.ee.validate.AssertionTreeBuilder getAssertionTreeBuilder() {
            return assertionTreeBuilder;
        }
        public void setAssertionTreeBuilder(Saxon.Eej.ee.validate.AssertionTreeBuilder assertionTreeBuilder) {
            this.assertionTreeBuilder = assertionTreeBuilder;
        }
        public void startValidation(string systemId) {
            if ((invalidityHandler != null)) {
                try {
                    invalidityHandler.startReporting(systemId);
                } catch (Saxon.Hej.trans.XPathException err) {
                    invalidityHandler = null;
                }
            }
        }
        public Saxon.Hej.om.Sequence getValidationErrorObject() {
            if ((invalidityHandler != null)) {
                try {
                    return invalidityHandler.endReporting();
                } catch (Saxon.Hej.trans.XPathException e) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
            return null;
        }
        public Saxon.Hej.type.ValidationParams getValidationParams() {
            return validationParams;
        }
        public bool hasValidationParams() {
            return ((validationParams != null) && (validationParams.Count != 0));
        }
        public void setValidationParams(Saxon.Hej.type.ValidationParams validationParams) {
            this.validationParams = validationParams;
        }
        public void setConstraintChecker(Saxon.Eej.ee.validate.ConstraintChecker checker) {
            this.constraintChecker = checker;
        }
        protected internal Saxon.Eej.ee.validate.ConstraintChecker getConstraintChecker() {
            return this.constraintChecker;
        }
        public void setAttributeInheritor(Saxon.Eej.ee.validate.AttributeInheritor inheritor) {
            attributeInheritor = inheritor;
        }
        public Saxon.Eej.ee.validate.AttributeInheritor getAttributeInheritor() {
            return attributeInheritor;
        }
        public void setXSIType(Saxon.Hej.type.SchemaType xsiType) {
            this.xsiType = xsiType;
        }
        public Saxon.Hej.type.SchemaType getXSIType() {
            return xsiType;
        }
        public void setErrorCode(string errorCode) {
            this.errorCode = errorCode;
        }
        protected internal string getErrorCode() {
            return errorCode;
        }
        public void setErrorLimit(int limit) {
            this.errorLimit = limit;
        }
        public bool incrementErrorCount(Saxon.Hej.type.ValidationFailure failure) {
            if ((firstFailure == null)) {
                firstFailure = failure;
            }
            return (++errorCount>=errorLimit);
        }
        public Saxon.Hej.type.ValidationFailure getFirstFailure() {
            return firstFailure;
        }
        public int getErrorCount() {
            return errorCount;
        }
        public Saxon.Hej.s9api.Location getInitiatingLocation() {
            return initiatingLocation;
        }
        public void setInitiatingLocation(Saxon.Hej.s9api.Location initiatingLocation) {
            this.initiatingLocation = initiatingLocation;
        }
    
    }
}