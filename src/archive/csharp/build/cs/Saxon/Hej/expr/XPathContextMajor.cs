using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class XPathContextMajor : Saxon.Hej.expr.XPathContextMinor {
        private Saxon.Hej.expr.instruct.ParameterSet localParameters;
        private Saxon.Hej.expr.instruct.ParameterSet tunnelParameters;
        private Saxon.Hej.expr.TailCallLoop.TailCallInfo tailCallInfo;
        private Saxon.Hej.expr.Component.M currentMode;
        private Saxon.Hej.trans.rules.Rule currentTemplate;
        private Saxon.Hej.expr.sort.GroupIterator currentGroupIterator;
        private Saxon.Hej.expr.sort.GroupIterator currentMergeGroupIterator;
        private Saxon.Hej.regex.RegexIterator currentRegexIterator;
        private Saxon.Hej.expr.ContextOriginator origin;
        private Saxon.Hej.expr.XPathContextMajor.ThreadManager threadManager = null;
        private Saxon.Ejavax.xml.transform.URIResolver uriResolver;
        private Saxon.Hej.lib.ErrorReporter errorReporter;
        private Saxon.Hej.expr.Component currentComponent;
        internal Saxon.Hej.trans.XPathException currentException;
        public XPathContextMajor(Saxon.Hej.Controller controller)  {
            this.controller = controller;
            stackFrame = Saxon.Hej.expr.StackFrame.EMPTY;
            origin = controller;
        }
        private XPathContextMajor()  {
        }
        public XPathContextMajor(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.Executable exec)  {
            controller = ((exec is Saxon.Hej.PreparedStylesheet) ? new Saxon.Hej.trans.XsltController(exec.getConfiguration(), (Saxon.Hej.PreparedStylesheet)(exec)) : new Saxon.Hej.Controller(exec.getConfiguration(), exec));
            if ((item != null)) {
                Saxon.Hej.om.SequenceIterator iter = Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item);
                currentIterator = new Saxon.Hej.om.FocusTrackingIterator(iter);
                currentIterator.next();
                last = new Saxon.Hej.expr.XPathContextMinor.LastValue(1);
            }
            origin = controller;
        }
        public override Saxon.Hej.expr.XPathContextMajor newContext() {
            Saxon.Hej.expr.XPathContextMajor c = new Saxon.Hej.expr.XPathContextMajor();
            c.controller = controller;
            c.currentIterator = currentIterator;
            c.stackFrame = stackFrame;
            c.localParameters = localParameters;
            c.tunnelParameters = tunnelParameters;
            c.last = last;
            c.currentDestination = currentDestination;
            c.currentMode = currentMode;
            c.currentTemplate = currentTemplate;
            c.currentRegexIterator = currentRegexIterator;
            c.currentGroupIterator = currentGroupIterator;
            c.currentMergeGroupIterator = currentMergeGroupIterator;
            c.currentException = currentException;
            c.caller = this;
            c.tailCallInfo = null;
            c.temporaryOutputState = temporaryOutputState;
            c.threadManager = threadManager;
            c.currentComponent = currentComponent;
            c.errorReporter = errorReporter;
            c.uriResolver = uriResolver;
            return c;
        }
        public static Saxon.Hej.expr.XPathContextMajor newContext(Saxon.Hej.expr.XPathContextMinor prev) {
            Saxon.Hej.expr.XPathContextMajor c = new Saxon.Hej.expr.XPathContextMajor();
            Saxon.Hej.expr.XPathContext p = prev;
            while (!(((p is Saxon.Hej.expr.XPathContextMajor)))) {
                p = p.getCaller();
            }
            c.controller = p.getController();
            c.currentIterator = prev.getCurrentIterator();
            c.stackFrame = prev.getStackFrame();
            c.localParameters = p.getLocalParameters();
            c.tunnelParameters = p.getTunnelParameters();
            c.last = prev.last;
            c.currentDestination = prev.currentDestination;
            c.currentMode = p.getCurrentMode();
            c.currentTemplate = p.getCurrentTemplateRule();
            c.currentRegexIterator = p.getCurrentRegexIterator();
            c.currentGroupIterator = p.getCurrentGroupIterator();
            c.currentMergeGroupIterator = p.getCurrentMergeGroupIterator();
            c.caller = prev;
            c.tailCallInfo = null;
            c.threadManager = ((Saxon.Hej.expr.XPathContextMajor)(p)).threadManager;
            c.currentComponent = ((Saxon.Hej.expr.XPathContextMajor)(p)).currentComponent;
            c.errorReporter = ((Saxon.Hej.expr.XPathContextMajor)(p)).errorReporter;
            c.uriResolver = ((Saxon.Hej.expr.XPathContextMajor)(p)).uriResolver;
            c.temporaryOutputState = prev.temporaryOutputState;
            return c;
        }
        public static Saxon.Hej.expr.XPathContextMajor newThreadContext(Saxon.Hej.expr.XPathContextMinor prev) {
            Saxon.Hej.expr.XPathContextMajor c = newContext(prev);
            c.stackFrame = prev.stackFrame.copy();
            return c;
        }
        public abstract class ThreadManager {
            public abstract void waitForChildThreads();
        
        }
        public override Saxon.Hej.expr.XPathContextMajor.ThreadManager getThreadManager() {
            return threadManager;
        }
        public void createThreadManager() {
            threadManager = getConfiguration().makeThreadManager();
        }
        public override void waitForChildThreads() {
            if ((threadManager != null)) {
                threadManager.waitForChildThreads();
            }
        }
        public override Saxon.Hej.expr.instruct.ParameterSet getLocalParameters() {
            if ((localParameters == null)) {
                localParameters = new Saxon.Hej.expr.instruct.ParameterSet();
            }
            return localParameters;
        }
        public void setLocalParameters(Saxon.Hej.expr.instruct.ParameterSet localParameters) {
            this.localParameters = localParameters;
        }
        public override Saxon.Hej.expr.instruct.ParameterSet getTunnelParameters() {
            return tunnelParameters;
        }
        public void setTunnelParameters(Saxon.Hej.expr.instruct.ParameterSet tunnelParameters) {
            this.tunnelParameters = tunnelParameters;
        }
        public void setOrigin(Saxon.Hej.expr.ContextOriginator expr) {
            origin = expr;
        }
        public Saxon.Hej.expr.ContextOriginator getOrigin() {
            return origin;
        }
        public void setStackFrame(Saxon.Hej.expr.instruct.SlotManager map, Saxon.Hej.om.Sequence[] variables) {
            stackFrame = new Saxon.Hej.expr.StackFrame(map, variables);
            if (((map != null) && (variables.Length != map.getNumberOfVariables()))) {
                if ((variables.Length>map.getNumberOfVariables())) {
                    throw new Saxon.Ejava.lang.IllegalStateException((((("Attempting to set more local variables ("+variables.Length)+") than the stackframe can accommodate (")+map.getNumberOfVariables())+")"));
                }
                stackFrame.slots = new Saxon.Hej.om.Sequence[map.getNumberOfVariables()];
                Saxon.Ejava.lang.System.arraycopy(variables, 0, stackFrame.slots, 0, variables.Length);
            }
        }
        public void resetStackFrameMap(Saxon.Hej.expr.instruct.SlotManager map, int numberOfParams) {
            stackFrame.map = map;
            if ((stackFrame.slots.Length != map.getNumberOfVariables())) {
                Saxon.Hej.om.Sequence[] v2 = new Saxon.Hej.om.Sequence[map.getNumberOfVariables()];
                Saxon.Ejava.lang.System.arraycopy(stackFrame.slots, 0, v2, 0, numberOfParams);
                stackFrame.slots = v2;
            } else {
                Saxon.Ejava.util.Arrays.fill(stackFrame.slots, numberOfParams, stackFrame.slots.Length, null);
            }
        }
        public Saxon.Hej.om.Sequence[] getAllVariableValues() {
            return stackFrame.getStackFrameValues();
        }
        public void resetAllVariableValues(Saxon.Hej.om.Sequence[] values) {
            stackFrame.setStackFrameValues(values);
        }
        public void resetParameterValues(Saxon.Hej.om.Sequence[] values) {
            Saxon.Ejava.lang.System.arraycopy(values, 0, stackFrame.slots, 0, values.Length);
        }
        public void requestTailCall(Saxon.Hej.expr.TailCallLoop.TailCallInfo targetFn, Saxon.Hej.om.Sequence[] variables) {
            if ((variables != null)) {
                if ((variables.Length>stackFrame.slots.Length)) {
                    stackFrame.slots = Saxon.Ejava.util.Arrays.copyOf(variables, variables.Length);
                } else {
                    Saxon.Ejava.lang.System.arraycopy(variables, 0, stackFrame.slots, 0, variables.Length);
                }
            }
            tailCallInfo = targetFn;
        }
        public Saxon.Hej.expr.TailCallLoop.TailCallInfo getTailCallInfo() {
            Saxon.Hej.expr.TailCallLoop.TailCallInfo fn = tailCallInfo;
            tailCallInfo = null;
            return fn;
        }
        public void openStackFrame(Saxon.Hej.expr.instruct.SlotManager map) {
            int numberOfSlots = map.getNumberOfVariables();
            if ((numberOfSlots == 0)) {
                stackFrame = Saxon.Hej.expr.StackFrame.EMPTY;
            } else {
                stackFrame = new Saxon.Hej.expr.StackFrame(map, new Saxon.Hej.om.Sequence[numberOfSlots]);
            }
        }
        public void openStackFrame(int numberOfVariables) {
            stackFrame = new Saxon.Hej.expr.StackFrame(new Saxon.Hej.expr.instruct.SlotManager(numberOfVariables), Saxon.Hej.om.SequenceTool.makeSequenceArray(numberOfVariables));
        }
        public void setCurrentMode(Saxon.Hej.expr.Component.M mode) {
            this.currentMode = mode;
        }
        public override Saxon.Hej.expr.Component.M getCurrentMode() {
            Saxon.Hej.expr.Component.M m = currentMode;
            if ((m == null)) {
                Saxon.Hej.trans.rules.RuleManager rm = getController().getRuleManager();
                if ((rm != null)) {
                    return rm.getUnnamedMode().getDeclaringComponent();
                } else {
                    return null;
                }
            } else {
                return m;
            }
        }
        public void setCurrentTemplateRule(Saxon.Hej.trans.rules.Rule rule) {
            this.currentTemplate = rule;
        }
        public override Saxon.Hej.trans.rules.Rule getCurrentTemplateRule() {
            return currentTemplate;
        }
        public void setCurrentGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator) {
            this.currentGroupIterator = iterator;
        }
        public override Saxon.Hej.expr.sort.GroupIterator getCurrentGroupIterator() {
            return currentGroupIterator;
        }
        public void setCurrentMergeGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator) {
            this.currentMergeGroupIterator = iterator;
        }
        public override Saxon.Hej.expr.sort.GroupIterator getCurrentMergeGroupIterator() {
            return currentMergeGroupIterator;
        }
        public void setCurrentRegexIterator(Saxon.Hej.regex.RegexIterator currentRegexIterator) {
            this.currentRegexIterator = currentRegexIterator;
        }
        public override Saxon.Hej.regex.RegexIterator getCurrentRegexIterator() {
            return currentRegexIterator;
        }
        public override int useLocalParameter(Saxon.Hej.om.StructuredQName paramName, int slotNumber, bool isTunnel) {
            Saxon.Hej.expr.instruct.ParameterSet @params = (isTunnel ? getTunnelParameters() : localParameters);
            if ((@params == null)) {
                return Saxon.Hej.expr.instruct.ParameterSet.NOT_SUPPLIED;
            }
            int index = @params.getIndex(paramName);
            if ((index<0)) {
                return Saxon.Hej.expr.instruct.ParameterSet.NOT_SUPPLIED;
            }
            Saxon.Hej.om.Sequence val = @params.getValue(index);
            stackFrame.slots[slotNumber] = val;
            bool @checked = @params.isTypeChecked(index);
            return (@checked ? Saxon.Hej.expr.instruct.ParameterSet.SUPPLIED_AND_CHECKED : Saxon.Hej.expr.instruct.ParameterSet.SUPPLIED);
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            uriResolver = resolver;
            if ((resolver is Saxon.Callbacks.StandardURIResolver)) {
                ((Saxon.Callbacks.StandardURIResolver)(resolver)).setConfiguration(getConfiguration());
            }
        }
        public override Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            return ((uriResolver == null) ? controller.getURIResolver() : uriResolver);
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            this.errorReporter = reporter;
        }
        public override Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return ((errorReporter == null) ? controller.getErrorReporter() : errorReporter);
        }
        public void setCurrentException(Saxon.Hej.trans.XPathException exception) {
            currentException = exception;
        }
        public override Saxon.Hej.trans.XPathException getCurrentException() {
            return currentException;
        }
        public override Saxon.Hej.expr.Component getCurrentComponent() {
            return currentComponent;
        }
        public void setCurrentComponent(Saxon.Hej.expr.Component component) {
            currentComponent = component;
        }
        public override Saxon.Hej.expr.Component getTargetComponent(int bindingSlot) {
            try {
                Saxon.Hej.expr.ComponentBinding binding = currentComponent.getComponentBindings()[bindingSlot];
                return binding.getTarget();
            } catch (Saxon.Ejava.lang.NullPointerException e) {
                System.Console.Error.WriteLine(e);
                throw;
            } catch (System.IndexOutOfRangeException e) {
                System.Console.Error.WriteLine(e);
                throw;
            }
        }
    
    }
}