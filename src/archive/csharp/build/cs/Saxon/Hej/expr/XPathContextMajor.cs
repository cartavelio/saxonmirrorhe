using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class XPathContextMajor : Saxon.Hej.expr.XPathContextMinor {
        private Saxon.Hej.expr.instruct.ParameterSet localParameters;
        private Saxon.Hej.expr.instruct.ParameterSet tunnelParameters;
        private Saxon.Hej.expr.TailCallLoop.TailCallInfo tailCallInfo;
        private Saxon.Hej.expr.XPathContextMajor.ContextProperty firstProperty;
        private Saxon.Hej.expr.ContextOriginator origin;
        private Saxon.Hej.expr.XPathContextMajor.ThreadManager threadManager = null;
        private Saxon.Hej.expr.Component currentComponent;
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
                currentIterator = Saxon.Hej.om.SequenceTool.focusTracker(iter);
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
            c.firstProperty = firstProperty;
            c.caller = this;
            c.tailCallInfo = null;
            c.temporaryOutputState = temporaryOutputState;
            c.threadManager = threadManager;
            c.currentComponent = currentComponent;
            return c;
        }
        public static Saxon.Hej.expr.XPathContextMajor newContext(Saxon.Hej.expr.XPathContextMinor prev) {
            Saxon.Hej.expr.XPathContextMajor c = new Saxon.Hej.expr.XPathContextMajor();
            Saxon.Hej.expr.XPathContextMajor p = prev.getMajorContext();
            c.controller = p.getController();
            c.currentIterator = prev.getCurrentIterator();
            c.stackFrame = prev.getStackFrame();
            c.localParameters = p.getLocalParameters();
            c.tunnelParameters = p.getTunnelParameters();
            c.last = prev.last;
            c.currentDestination = prev.currentDestination;
            c.firstProperty = p.firstProperty;
            c.caller = prev;
            c.tailCallInfo = null;
            c.threadManager = p.threadManager;
            c.currentComponent = p.currentComponent;
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
            if ((mode != getCurrentMode())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentMode(mode, firstProperty);
            }
        }
        public override Saxon.Hej.expr.Component.M getCurrentMode() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentMode)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentMode)(p)).mode;
                }
                p = p.next;
            }
            Saxon.Hej.trans.rules.RuleManager rm = getController().getRuleManager();
            if ((rm != null)) {
                return rm.getUnnamedMode().getDeclaringComponent();
            } else {
                return null;
            }
        }
        public void setCurrentTemplateRule(Saxon.Hej.trans.rules.Rule rule) {
            if ((rule != getCurrentTemplateRule())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentTemplateRule(rule, firstProperty);
            }
        }
        public override Saxon.Hej.trans.rules.Rule getCurrentTemplateRule() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentTemplateRule)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentTemplateRule)(p)).currentRule;
                }
                p = p.next;
            }
            return null;
        }
        public void setCurrentGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator) {
            if ((iterator != getCurrentGroupIterator())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentGroupIterator(iterator, firstProperty);
            }
        }
        public override Saxon.Hej.expr.sort.GroupIterator getCurrentGroupIterator() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentGroupIterator)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentGroupIterator)(p)).iterator;
                }
                p = p.next;
            }
            return null;
        }
        public void setCurrentMergeGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator) {
            if ((iterator != getCurrentMergeGroupIterator())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentMergeGroupIterator(iterator, firstProperty);
            }
        }
        public override Saxon.Hej.expr.sort.GroupIterator getCurrentMergeGroupIterator() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentMergeGroupIterator)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentMergeGroupIterator)(p)).iterator;
                }
                p = p.next;
            }
            return null;
        }
        public void setCurrentRegexIterator(Saxon.Hej.regex.RegexIterator currentRegexIterator) {
            if ((currentRegexIterator != getCurrentRegexIterator())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentRegexIterator(currentRegexIterator, firstProperty);
            }
        }
        public override Saxon.Hej.regex.RegexIterator getCurrentRegexIterator() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentRegexIterator)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentRegexIterator)(p)).iterator;
                }
                p = p.next;
            }
            return null;
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
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            if ((resolver != getResourceResolver())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentResourceResolver(resolver, firstProperty);
            }
        }
        public override Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentResourceResolver)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentResourceResolver)(p)).resolver;
                }
                p = p.next;
            }
            return controller.getResourceResolver();
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            if ((reporter != getErrorReporter())) {
                firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentErrorReporter(reporter, firstProperty);
            }
        }
        public override Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentErrorReporter)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentErrorReporter)(p)).errorReporter;
                }
                p = p.next;
            }
            return controller.getErrorReporter();
        }
        public void setCurrentException(Saxon.Hej.trans.XPathException exception) {
            if ((exception != getCurrentException())) {
                this.firstProperty = new Saxon.Hej.expr.XPathContextMajor.CurrentException(exception, firstProperty);
            }
        }
        public override Saxon.Hej.trans.XPathException getCurrentException() {
            Saxon.Hej.expr.XPathContextMajor.ContextProperty p = firstProperty;
            while ((p != null)) {
                if ((p is Saxon.Hej.expr.XPathContextMajor.CurrentException)) {
                    return ((Saxon.Hej.expr.XPathContextMajor.CurrentException)(p)).exception;
                }
                p = p.next;
            }
            return null;
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
        private abstract class ContextProperty {
            public Saxon.Hej.expr.XPathContextMajor.ContextProperty next;
        
        }
        private class CurrentRegexIterator : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.regex.RegexIterator iterator;
            public CurrentRegexIterator(Saxon.Hej.regex.RegexIterator iterator, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.iterator = iterator;
                this.next = next;
            }
        
        }
        private class CurrentGroupIterator : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.expr.sort.GroupIterator iterator;
            public CurrentGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.iterator = iterator;
                this.next = next;
            }
        
        }
        private class CurrentMergeGroupIterator : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.expr.sort.GroupIterator iterator;
            public CurrentMergeGroupIterator(Saxon.Hej.expr.sort.GroupIterator iterator, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.iterator = iterator;
                this.next = next;
            }
        
        }
        private class CurrentTemplateRule : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.trans.rules.Rule currentRule;
            public CurrentTemplateRule(Saxon.Hej.trans.rules.Rule currentRule, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.currentRule = currentRule;
                this.next = next;
            }
        
        }
        private class CurrentMode : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.expr.Component.M mode;
            public CurrentMode(Saxon.Hej.expr.Component.M mode, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.mode = mode;
                this.next = next;
            }
        
        }
        private class CurrentException : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.trans.XPathException exception;
            public CurrentException(Saxon.Hej.trans.XPathException exception, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.exception = exception;
                this.next = next;
            }
        
        }
        private class CurrentErrorReporter : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.lib.ErrorReporter errorReporter;
            public CurrentErrorReporter(Saxon.Hej.lib.ErrorReporter errorReporter, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.errorReporter = errorReporter;
                this.next = next;
            }
        
        }
        private class CurrentResourceResolver : Saxon.Hej.expr.XPathContextMajor.ContextProperty {
            public Saxon.Hej.lib.JJ_ResourceResolver resolver;
            public CurrentResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver, Saxon.Hej.expr.XPathContextMajor.ContextProperty next)  {
                this.resolver = resolver;
                this.next = next;
            }
        
        }
    
    }
}