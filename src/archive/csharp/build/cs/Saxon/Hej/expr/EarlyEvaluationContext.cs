using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class EarlyEvaluationContext : Saxon.Hej.expr.XPathContext {
        private readonly Saxon.Hej.Configuration config;
        public EarlyEvaluationContext(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public Saxon.Hej.om.Sequence evaluateLocalVariable(int slotnumber) {
            notAllowed();
            return null;
        }
        public Saxon.Hej.expr.XPathContext getCaller() {
            return null;
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return config.getResourceResolver();
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return config.makeErrorReporter();
        }
        public Saxon.Hej.expr.Component getCurrentComponent() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.om.Item getContextItem() {
            return null;
        }
        public Saxon.Hej.Controller getController() {
            return null;
        }
        public Saxon.Hej.expr.sort.GroupIterator getCurrentGroupIterator() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.expr.sort.GroupIterator getCurrentMergeGroupIterator() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.om.FocusIterator getCurrentIterator() {
            return null;
        }
        public Saxon.Hej.expr.Component.M getCurrentMode() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.regex.RegexIterator getCurrentRegexIterator() {
            return null;
        }
        public Saxon.Hej.trans.rules.Rule getCurrentTemplateRule() {
            return null;
        }
        public int getLast() {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item is absent");
            err.setErrorCode("XPDY0002");
            throw new Saxon.Hej.trans.UncheckedXPathException(err);
        }
        public Saxon.Hej.expr.instruct.ParameterSet getLocalParameters() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return config.getNamePool();
        }
        public Saxon.Hej.expr.StackFrame getStackFrame() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.expr.instruct.ParameterSet getTunnelParameters() {
            notAllowed();
            return null;
        }
        public bool isAtLast() {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item is absent");
            err.setErrorCode("XPDY0002");
            throw err;
        }
        public Saxon.Hej.expr.XPathContextMajor newCleanContext() {
            notAllowed();
            return null;
        }
        public Saxon.Hej.expr.XPathContextMajor newContext() {
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(config);
            return controller.newXPathContext();
        }
        public Saxon.Hej.expr.XPathContextMinor newMinorContext() {
            return newContext().newMinorContext();
        }
        public void setCaller(Saxon.Hej.expr.XPathContext caller) {
        }
        public void setCurrentIterator(Saxon.Hej.om.FocusIterator iter) {
            notAllowed();
        }
        public Saxon.Hej.om.FocusIterator trackFocus(Saxon.Hej.om.SequenceIterator iter) {
            notAllowed();
            return null;
        }
        public void setLocalVariable(int slotNumber, Saxon.Hej.om.Sequence value) {
            notAllowed();
        }
        public int useLocalParameter(Saxon.Hej.om.StructuredQName parameterId, int slotNumber, bool isTunnel) {
            return Saxon.Hej.expr.instruct.ParameterSet.NOT_SUPPLIED;
        }
        public Saxon.Hej.value.DateTimeValue getCurrentDateTime() {
            throw new Saxon.Hej.trans.NoDynamicContextException("current-dateTime");
        }
        public int getImplicitTimezone() {
            return Saxon.Hej.value.CalendarValue.MISSING_TIMEZONE;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.trace.ContextStackFrame> iterateStackFrames() {
            return new System.Collections.Generic.List<Saxon.Hej.trace.ContextStackFrame>().GetEnumerator();;
        }
        public Saxon.Hej.trans.XPathException getCurrentException() {
            return null;
        }
        public void waitForChildThreads() {
            getCaller().waitForChildThreads();
        }
        public void setTemporaryOutputState(int temporary) {
        }
        public int getTemporaryOutputState() {
            return 0;
        }
        public void setCurrentOutputUri(string uri) {
        }
        public string getCurrentOutputUri() {
            return null;
        }
        private void notAllowed() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException(new Saxon.Hej.trans.NoDynamicContextException("Internal error: early evaluation of subexpression with no context"));
        }
        public Saxon.Hej.expr.XPathContextMajor.ThreadManager getThreadManager() {
            return null;
        }
        public Saxon.Hej.expr.Component getTargetComponent(int bindingSlot) {
            return null;
        }
    
    }
}