using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class XPathContextMinor : Saxon.Hej.expr.XPathContext {
        internal Saxon.Hej.Controller controller;
        internal Saxon.Hej.om.FocusIterator currentIterator;
        internal Saxon.Hej.expr.XPathContextMinor.LastValue last = null;
        internal Saxon.Hej.expr.XPathContext caller = null;
        protected internal Saxon.Hej.expr.StackFrame stackFrame;
        protected internal string currentDestination = "";
        protected internal int temporaryOutputState = 0;
        protected internal XPathContextMinor()  {
        }
        public virtual Saxon.Hej.expr.XPathContextMajor newContext() {
            return Saxon.Hej.expr.XPathContextMajor.newContext(this);
        }
        public Saxon.Hej.expr.XPathContextMinor newMinorContext() {
            Saxon.Hej.expr.XPathContextMinor c = new Saxon.Hej.expr.XPathContextMinor();
            c.controller = controller;
            c.caller = this;
            c.currentIterator = currentIterator;
            c.last = last;
            c.stackFrame = stackFrame;
            c.currentDestination = currentDestination;
            c.temporaryOutputState = temporaryOutputState;
            return c;
        }
        public void setCaller(Saxon.Hej.expr.XPathContext caller) {
            this.caller = caller;
        }
        public Saxon.Hej.expr.XPathContextMajor newCleanContext() {
            Saxon.Hej.expr.XPathContextMajor c = new Saxon.Hej.expr.XPathContextMajor(getController());
            c.setCaller(this);
            return c;
        }
        public virtual Saxon.Hej.expr.instruct.ParameterSet getLocalParameters() {
            return getMajorContext().getLocalParameters();
        }
        public virtual Saxon.Hej.expr.instruct.ParameterSet getTunnelParameters() {
            return getMajorContext().getTunnelParameters();
        }
        public Saxon.Hej.Controller getController() {
            return controller;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return controller.getConfiguration();
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return controller.getConfiguration().getNamePool();
        }
        public Saxon.Hej.expr.XPathContext getCaller() {
            return caller;
        }
        public Saxon.Hej.expr.XPathContextMajor getMajorContext() {
            Saxon.Hej.expr.XPathContext c = this;
            while (((c != null) && !(((c is Saxon.Hej.expr.XPathContextMajor))))) {
                c = c.getCaller();
            }
            return (Saxon.Hej.expr.XPathContextMajor)(c);
        }
        public void setCurrentIterator(Saxon.Hej.om.FocusIterator iter) {
            currentIterator = iter;
            last = null;
        }
        public Saxon.Hej.om.FocusIterator trackFocus(Saxon.Hej.om.SequenceIterator iter) {
            System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> factory = controller.getFocusTrackerFactory(false);
            Saxon.Hej.om.FocusIterator fit = factory(iter);
            setCurrentIterator(fit);
            return fit;
        }
        public Saxon.Hej.om.FocusIterator trackFocusMultithreaded(Saxon.Hej.om.SequenceIterator iter) {
            System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> factory = controller.getFocusTrackerFactory(true);
            Saxon.Hej.om.FocusIterator fit = factory(iter);
            setCurrentIterator(fit);
            return fit;
        }
        public Saxon.Hej.om.FocusIterator getCurrentIterator() {
            return currentIterator;
        }
        public Saxon.Hej.om.Item getContextItem() {
            if ((currentIterator == null)) {
                return null;
            }
            if ((currentIterator is Saxon.Hej.om.FocusTrackingIterator)) {
                return ((Saxon.Hej.om.FocusTrackingIterator)(currentIterator)).current();
            } else {
                return currentIterator.current();
            }
        }
        public int getLast() {
            if ((currentIterator == null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("The context item is absent, so last() is undefined");
                e.setXPathContext(this);
                e.setErrorCode("XPDY0002");
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
            if ((last != null)) {
                return last.value;
            }
            try {
                int length = currentIterator.getLength();
                last = new Saxon.Hej.expr.XPathContextMinor.LastValue(length);
                return length;
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.UncheckedXPathException(err);
            }
        }
        public bool isAtLast() {
            if (((currentIterator is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(currentIterator)).supportsHasNext())) {
                return !(((Saxon.Hej.tree.iter.LookaheadIterator)(currentIterator)).hasNext());
            }
            try {
                return (currentIterator.position() == getLast());
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        public virtual Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return caller.getResourceResolver();
        }
        public virtual Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return caller.getErrorReporter();
        }
        public virtual Saxon.Hej.trans.XPathException getCurrentException() {
            return caller.getCurrentException();
        }
        public virtual Saxon.Hej.expr.XPathContextMajor.ThreadManager getThreadManager() {
            return caller.getThreadManager();
        }
        public virtual Saxon.Hej.expr.Component getCurrentComponent() {
            return caller.getCurrentComponent();
        }
        public Saxon.Hej.expr.StackFrame getStackFrame() {
            return stackFrame;
        }
        public void makeStackFrameMutable() {
            if ((stackFrame == Saxon.Hej.expr.StackFrame.EMPTY)) {
                stackFrame = new Saxon.Hej.expr.StackFrame(null, Saxon.Hej.om.SequenceTool.makeSequenceArray(0));
            }
        }
        public Saxon.Hej.om.Sequence evaluateLocalVariable(int slot) {
            return stackFrame.slots[slot];
        }
        public void setLocalVariable(int slotNumber, Saxon.Hej.om.Sequence value) {
            value = value.makeRepeatable();
            try {
                stackFrame.slots[slotNumber] = value;
            } catch (System.IndexOutOfRangeException e) {
                if ((slotNumber == -999)) {
                    throw new Saxon.Ejava.lang.AssertionError("Internal error: Cannot set local variable: no slot allocated");
                } else {
                    throw new Saxon.Ejava.lang.AssertionError((((("Internal error: Cannot set local variable (slot "+slotNumber)+" of ")+getStackFrame().getStackFrameValues().Length)+")"));
                }
            }
        }
        public virtual void waitForChildThreads() { lock(this) {
            getMajorContext().waitForChildThreads();
        }}
        public void setTemporaryOutputState(int temporary) {
            temporaryOutputState = temporary;
        }
        public int getTemporaryOutputState() {
            return temporaryOutputState;
        }
        public void setCurrentOutputUri(string uri) {
            currentDestination = uri;
        }
        public string getCurrentOutputUri() {
            return currentDestination;
        }
        public virtual int useLocalParameter(Saxon.Hej.om.StructuredQName parameterId, int slotNumber, bool isTunnel) {
            return getMajorContext().useLocalParameter(parameterId, slotNumber, isTunnel);
        }
        public virtual Saxon.Hej.expr.Component.M getCurrentMode() {
            return getMajorContext().getCurrentMode();
        }
        public virtual Saxon.Hej.trans.rules.Rule getCurrentTemplateRule() {
            return null;
        }
        public virtual Saxon.Hej.expr.sort.GroupIterator getCurrentGroupIterator() {
            return getMajorContext().getCurrentGroupIterator();
        }
        public virtual Saxon.Hej.expr.sort.GroupIterator getCurrentMergeGroupIterator() {
            return getMajorContext().getCurrentMergeGroupIterator();
        }
        public virtual Saxon.Hej.regex.RegexIterator getCurrentRegexIterator() {
            return getMajorContext().getCurrentRegexIterator();
        }
        public Saxon.Hej.value.DateTimeValue getCurrentDateTime() {
            return controller.getCurrentDateTime();
        }
        public int getImplicitTimezone() {
            return controller.getImplicitTimezone();
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.trace.ContextStackFrame> iterateStackFrames() {
            return new Saxon.Hej.trace.ContextStackIterator(this);
        }
        public virtual Saxon.Hej.expr.Component getTargetComponent(int bindingSlot) {
            return getMajorContext().getTargetComponent(bindingSlot);
        }
        protected internal class LastValue {
            public readonly int value;
            public LastValue(int count)  {
                value = count;
            }
        
        }
    
    }
}