using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class ContextStackIterator : System.Collections.Generic.IEnumerator<Saxon.Hej.trace.ContextStackFrame> {
        private Saxon.Hej.expr.XPathContextMajor nextFrame;
        public ContextStackIterator(Saxon.Hej.expr.XPathContext context)  {
            if (!(((context is Saxon.Hej.expr.XPathContextMajor)))) {
                context = getMajorCaller(context);
            }
            nextFrame = (Saxon.Hej.expr.XPathContextMajor)(context);
        }
        public bool hasNext() {
            return (nextFrame != null);
        }
        public Saxon.Hej.trace.ContextStackFrame next() {
            Saxon.Hej.expr.XPathContextMajor context = nextFrame;
            if ((context == null)) {
                return null;
            }
            Saxon.Hej.expr.ContextOriginator origin = context.getOrigin();
            if ((origin is Saxon.Hej.Controller)) {
                nextFrame = getMajorCaller(context);
                return new Saxon.Hej.trace.ContextStackFrame.CallingApplication();
            } else {
                if ((origin is Saxon.Hej.trans.rules.BuiltInRuleSet)) {
                    nextFrame = getMajorCaller(context);
                    return new Saxon.Hej.trace.ContextStackFrame.BuiltInTemplateRule(context);
                } else {
                    if ((origin is Saxon.Hej.expr.instruct.UserFunction)) {
                        Saxon.Hej.trace.ContextStackFrame.FunctionCall sf = new Saxon.Hej.trace.ContextStackFrame.FunctionCall();
                        Saxon.Hej.expr.instruct.UserFunction ufc = (Saxon.Hej.expr.instruct.UserFunction)(origin);
                        sf.setLocation(ufc.getLocation());
                        sf.setFunctionName(ufc.getFunctionName());
                        sf.setContextItem(context.getContextItem());
                        sf.setContext(context);
                        nextFrame = getMajorCaller(context);
                        return sf;
                    } else {
                        if ((origin is Saxon.Hej.expr.UserFunctionCall)) {
                            Saxon.Hej.trace.ContextStackFrame.FunctionCall sf = new Saxon.Hej.trace.ContextStackFrame.FunctionCall();
                            Saxon.Hej.expr.UserFunctionCall ufc = (Saxon.Hej.expr.UserFunctionCall)(origin);
                            sf.setLocation(ufc.getLocation());
                            sf.setFunctionName(ufc.getFunctionName());
                            sf.setContextItem(context.getContextItem());
                            sf.setContext(context);
                            nextFrame = getMajorCaller(context);
                            return sf;
                        } else {
                            if ((origin is Saxon.Hej.expr.instruct.ApplyTemplates)) {
                                Saxon.Hej.trace.ContextStackFrame.ApplyTemplates sf = new Saxon.Hej.trace.ContextStackFrame.ApplyTemplates();
                                Saxon.Hej.expr.instruct.ApplyTemplates loc = (Saxon.Hej.expr.instruct.ApplyTemplates)(origin);
                                sf.setLocation(loc.getLocation());
                                sf.setContextItem(context.getContextItem());
                                sf.setContext(context);
                                nextFrame = getMajorCaller(context);
                                return sf;
                            } else {
                                if ((origin is Saxon.Hej.expr.instruct.CallTemplate)) {
                                    Saxon.Hej.trace.ContextStackFrame.CallTemplate sf = new Saxon.Hej.trace.ContextStackFrame.CallTemplate();
                                    Saxon.Hej.expr.instruct.CallTemplate loc = (Saxon.Hej.expr.instruct.CallTemplate)(origin);
                                    sf.setLocation(loc.getLocation());
                                    sf.setTemplateName(loc.getObjectName());
                                    sf.setContextItem(context.getContextItem());
                                    sf.setContext(context);
                                    nextFrame = getMajorCaller(context);
                                    return sf;
                                } else {
                                    if ((origin is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                        Saxon.Hej.trace.ContextStackFrame.VariableEvaluation sf = new Saxon.Hej.trace.ContextStackFrame.VariableEvaluation();
                                        Saxon.Hej.expr.instruct.GlobalVariable var = (Saxon.Hej.expr.instruct.GlobalVariable)(origin);
                                        sf.setLocation(var.getLocation());
                                        sf.setContextItem(context.getContextItem());
                                        sf.setVariableName(var.getVariableQName());
                                        sf.setComponent(var);
                                        sf.setContext(context);
                                        nextFrame = getMajorCaller(context);
                                        return sf;
                                    } else {
                                        nextFrame = getMajorCaller(context);
                                        Saxon.Hej.trace.ContextStackFrame csf = next();
                                        if ((csf == null)) {
                                            return new Saxon.Hej.trace.ContextStackFrame.CallingApplication();
                                        } else {
                                            return csf;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static Saxon.Hej.expr.XPathContextMajor getMajorCaller(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext caller = context.getCaller();
            return caller.getMajorContext();
        }
        public void remove() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        
    /* Add methods to implement IEnumerable */

        object System.Collections.IEnumerator.Current => Current;
        public Saxon.Hej.trace.ContextStackFrame Current { get; private set; }
        public bool MoveNext() {
            if (hasNext()) {Current = next(); return true;} else return false;
        }
        public void Reset() {throw new NotImplementedException();}
        public void Dispose() {}
      
    }
}