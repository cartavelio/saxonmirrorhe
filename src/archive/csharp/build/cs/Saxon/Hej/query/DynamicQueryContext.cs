using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class DynamicQueryContext {
        private Saxon.Hej.om.Item contextItem;
        private Saxon.Hej.expr.instruct.GlobalParameterSet parameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Ejavax.xml.transform.URIResolver uriResolver;
        private Saxon.Hej.lib.ErrorReporter errorReporter;
        private Saxon.Hej.lib.TraceListener traceListener;
        private Saxon.Hej.lib.UnparsedTextURIResolver unparsedTextURIResolver;
        private Saxon.Hej.value.DateTimeValue currentDateTime;
        private Saxon.Hej.lib.Logger traceFunctionDestination;
        private int validationMode = Saxon.Hej.lib.Validation.DEFAULT;
        private bool applyConversionRules = true;
        public DynamicQueryContext(Saxon.Hej.Configuration config)  {
            this.config = config;
            uriResolver = config.getURIResolver();
            errorReporter = new Saxon.Hej.lib.StandardErrorReporter();
            traceFunctionDestination = config.getLogger();
        }
        public int getSchemaValidationMode() {
            return validationMode;
        }
        public void setSchemaValidationMode(int validationMode) {
            this.validationMode = validationMode;
        }
        public void setApplyFunctionConversionRulesToExternalVariables(bool convert) {
            applyConversionRules = convert;
        }
        public bool isApplyFunctionConversionRulesToExternalVariables() {
            return applyConversionRules;
        }
        public void setContextItem(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Context item cannot be null");
            }
            if ((item is Saxon.Hej.om.NodeInfo)) {
                if (!(((Saxon.Hej.om.NodeInfo)(item)).getConfiguration().isCompatible(config))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied node must be built using the same or a compatible Configuration");
                }
            }
            contextItem = item;
        }
        public Saxon.Hej.om.Item getContextItem() {
            return contextItem;
        }
        public void setParameter(Saxon.Hej.om.StructuredQName expandedName, Saxon.Hej.om.GroundedValue value) {
            if ((parameters == null)) {
                parameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            }
            parameters.put(expandedName, value);
        }
        public void clearParameters() {
            parameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        }
        public Saxon.Hej.om.GroundedValue getParameter(Saxon.Hej.om.StructuredQName expandedName) {
            if ((parameters == null)) {
                return null;
            }
            return parameters.get(expandedName);
        }
        public Saxon.Hej.expr.instruct.GlobalParameterSet getParameters() {
            if ((parameters == null)) {
                return new Saxon.Hej.expr.instruct.GlobalParameterSet();
            } else {
                return parameters;
            }
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            uriResolver = resolver;
        }
        public Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            return uriResolver;
        }
        public void setUnparsedTextURIResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            unparsedTextURIResolver = resolver;
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return unparsedTextURIResolver;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            errorReporter = reporter;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return errorReporter;
        }
        public void setTraceListener(Saxon.Hej.lib.TraceListener listener) {
            traceListener = listener;
        }
        public Saxon.Hej.lib.TraceListener getTraceListener() {
            return traceListener;
        }
        public void setTraceFunctionDestination(Saxon.Hej.lib.Logger stream) {
            traceFunctionDestination = stream;
        }
        public Saxon.Hej.lib.Logger getTraceFunctionDestination() {
            return traceFunctionDestination;
        }
        public Saxon.Hej.value.DateTimeValue getCurrentDateTime() {
            return currentDateTime;
        }
        public void setCurrentDateTime(Saxon.Hej.value.DateTimeValue dateTime) {
            currentDateTime = dateTime;
            if ((dateTime.getComponent(Saxon.Hej.functions.AccessorFn.Component.TIMEZONE) == null)) {
                throw new Saxon.Hej.trans.XPathException("Supplied date/time must include a timezone");
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void initializeController(Saxon.Hej.Controller controller) {
            controller.setURIResolver(getURIResolver());
            controller.setErrorReporter(getErrorReporter());
            controller.addTraceListener(getTraceListener());
            if ((unparsedTextURIResolver != null)) {
                controller.setUnparsedTextURIResolver(unparsedTextURIResolver);
            }
            controller.setTraceFunctionDestination(getTraceFunctionDestination());
            controller.setSchemaValidationMode(getSchemaValidationMode());
            Saxon.Hej.value.DateTimeValue currentDateTime = getCurrentDateTime();
            if ((currentDateTime != null)) {
                try {
                    controller.setCurrentDateTime(currentDateTime);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            controller.setGlobalContextItem(contextItem);
            controller.initializeController(parameters);
            controller.setApplyFunctionConversionRulesToExternalVariables(applyConversionRules);
        }
    
    }
}