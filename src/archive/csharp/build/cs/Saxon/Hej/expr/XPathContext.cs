using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface XPathContext {
        Saxon.Hej.expr.XPathContextMajor newContext();
        Saxon.Hej.expr.XPathContextMajor newCleanContext();
        Saxon.Hej.expr.XPathContextMinor newMinorContext();
        Saxon.Hej.expr.instruct.ParameterSet getLocalParameters();
        Saxon.Hej.expr.instruct.ParameterSet getTunnelParameters();
        Saxon.Hej.Controller getController();
        Saxon.Hej.Configuration getConfiguration();
        Saxon.Hej.om.NamePool getNamePool();
        void setCaller(Saxon.Hej.expr.XPathContext caller);
        Saxon.Hej.expr.XPathContext getCaller();
        Saxon.Hej.om.FocusIterator trackFocus(Saxon.Hej.om.SequenceIterator iter);
        void setCurrentIterator(Saxon.Hej.om.FocusIterator iter);
        Saxon.Hej.om.FocusIterator getCurrentIterator();
        Saxon.Hej.om.Item getContextItem();
        int getLast();
        bool isAtLast();
        Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver();
        Saxon.Hej.lib.ErrorReporter getErrorReporter();
        Saxon.Hej.expr.Component getCurrentComponent();
        int useLocalParameter(Saxon.Hej.om.StructuredQName parameterId, int slotNumber, bool isTunnel);
        Saxon.Hej.expr.StackFrame getStackFrame();
        Saxon.Hej.om.Sequence evaluateLocalVariable(int slotnumber);
        void setLocalVariable(int slotNumber, Saxon.Hej.om.Sequence value);
        void setTemporaryOutputState(int temporary);
        int getTemporaryOutputState();
        void setCurrentOutputUri(string uri);
        string getCurrentOutputUri();
        Saxon.Hej.expr.Component.M getCurrentMode();
        Saxon.Hej.trans.rules.Rule getCurrentTemplateRule();
        Saxon.Hej.expr.sort.GroupIterator getCurrentGroupIterator();
        Saxon.Hej.expr.sort.GroupIterator getCurrentMergeGroupIterator();
        Saxon.Hej.regex.RegexIterator getCurrentRegexIterator();
        Saxon.Hej.value.DateTimeValue getCurrentDateTime();
        int getImplicitTimezone();
        System.Collections.Generic.IEnumerator<Saxon.Hej.trace.ContextStackFrame> iterateStackFrames();
        Saxon.Hej.trans.XPathException getCurrentException();
        Saxon.Hej.expr.XPathContextMajor.ThreadManager getThreadManager();
        void waitForChildThreads();
        Saxon.Hej.expr.Component getTargetComponent(int bindingSlot);
    
    }
}