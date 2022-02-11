using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FunctionLookup : Saxon.Hej.functions.ContextAccessorFunction {
        private Saxon.Hej.expr.XPathContext boundContext = null;
        public FunctionLookup()  {
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.PackageData pack = getRetainedStaticContext().getPackageData();
            if ((pack is Saxon.Hej.style.StylesheetPackage)) {
                ((Saxon.Hej.style.StylesheetPackage)(pack)).setRetainUnusedFunctions();
            }
            return base.makeFunctionCall(arguments);
        }
        public override bool Equals(System.Object o) {
            return (base.Equals(o) && Saxon.Hej.expr.parser.ExpressionTool.equalOrNull(getRetainedStaticContext(), ((Saxon.Hej.functions.hof.FunctionLookup)(o)).getRetainedStaticContext()));
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override Saxon.Hej.om.Function bindContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.functions.hof.FunctionLookup bound = (Saxon.Hej.functions.hof.FunctionLookup)(Saxon.Hej.functions.SystemFunction.makeFunction("function-lookup", getRetainedStaticContext(), 2));
            Saxon.Hej.om.FocusIterator focusIterator = context.getCurrentIterator();
            if ((focusIterator != null)) {
                Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                Saxon.Hej.tree.iter.ManualIterator mi = new Saxon.Hej.tree.iter.ManualIterator(context.getContextItem(), focusIterator.position());
                c2.setCurrentIterator(mi);
                bound.boundContext = c2;
            } else {
                bound.boundContext = context;
            }
            return bound;
        }
        public Saxon.Hej.om.Function lookup(Saxon.Hej.om.StructuredQName name, int arity, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.expr.instruct.Executable exec = controller.getExecutable();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = getRetainedStaticContext();
            Saxon.Hej.expr.PackageData pd = rsc.getPackageData();
            Saxon.Hej.functions.FunctionLibrary lib = ((pd is Saxon.Hej.style.StylesheetPackage) ? ((Saxon.Hej.style.StylesheetPackage)(pd)).getFunctionLibrary() : exec.getFunctionLibrary());
            Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(name, arity);
            Saxon.Hej.sxpath.IndependentContext ic = new Saxon.Hej.sxpath.IndependentContext(controller.getConfiguration());
            ic.setDefaultCollationName(rsc.getDefaultCollationName());
            ic.setBaseURI(rsc.getStaticBaseUriString());
            ic.setDecimalFormatManager(rsc.getDecimalFormatManager());
            ic.setNamespaceResolver(rsc);
            ic.setPackageData(pd);
            try {
                Saxon.Hej.om.Function fi = lib.getFunctionItem(sn, ic);
                if ((fi is Saxon.Hej.expr.instruct.UserFunction)) {
                    Saxon.Hej.trans.Visibility vis = ((Saxon.Hej.expr.instruct.UserFunction)(fi)).getDeclaredVisibility();
                    if ((vis == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        return null;
                    }
                }
                if ((fi is Saxon.Hej.functions.CallableFunction)) {
                    ((Saxon.Hej.functions.CallableFunction)(fi)).setCallable(new Saxon.Hej.functions.hof.CallableWithBoundFocus(((Saxon.Hej.functions.CallableFunction)(fi)).getCallable(), context));
                } else {
                    if ((fi is Saxon.Hej.functions.ContextItemAccessorFunction)) {
                        return ((Saxon.Hej.functions.ContextItemAccessorFunction)(fi)).bindContext(context);
                    } else {
                        if (((fi is Saxon.Hej.functions.SystemFunction) && ((Saxon.Hej.functions.SystemFunction)(fi)).dependsOnContextItem())) {
                            return new Saxon.Hej.functions.hof.SystemFunctionWithBoundContextItem((Saxon.Hej.functions.SystemFunction)(fi), context);
                        }
                    }
                }
                return fi;
            } catch (Saxon.Hej.trans.XPathException e) {
                if ("XPST0017".Equals(e.getErrorCodeLocalPart())) {
                    return null;
                }
                throw;
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContext c = ((boundContext == null) ? context : boundContext);
            Saxon.Hej.value.QNameValue qname = (Saxon.Hej.value.QNameValue)(arguments[0].head());
            Saxon.Hej.value.IntegerValue arity = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
            Saxon.Hej.om.Function fi = lookup(qname.getStructuredQName(), (int)(arity.longValue()), c);
            if ((fi == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            if ((fi is Saxon.Hej.functions.ContextAccessorFunction)) {
                fi = ((Saxon.Hej.functions.ContextAccessorFunction)(fi)).bindContext(c);
            }
            Saxon.Hej.expr.Component target = ((fi is Saxon.Hej.expr.instruct.UserFunction) ? ((Saxon.Hej.expr.instruct.UserFunction)(fi)).getDeclaringComponent() : null);
            Saxon.Hej.expr.ExportAgent agent = new Saxon.Hej.functions.hof.FunctionLookup.FunctionLookupExportAgent(this, qname, arity);
            return new Saxon.Hej.functions.hof.UserFunctionReference.BoundUserFunction(agent, fi, target, c.getController());
        }
        public class FunctionLookupExportAgent : Saxon.Hej.expr.ExportAgent {
            private readonly Saxon.Hej.value.QNameValue qName;
            private readonly Saxon.Hej.value.IntegerValue arity;
            private readonly Saxon.Hej.functions.hof.FunctionLookup container;
            public FunctionLookupExportAgent(Saxon.Hej.functions.hof.FunctionLookup container, Saxon.Hej.value.QNameValue qName, Saxon.Hej.value.IntegerValue arity)  {
                this.arity = arity;
                this.qName = qName;
                this.container = container;
            }
            public void export(Saxon.Hej.trace.ExpressionPresenter @out) {
                container.makeFunctionCall(Saxon.Hej.expr.Literal.makeLiteral(qName), Saxon.Hej.expr.Literal.makeLiteral(arity)).export(@out);
            }
        
        }
    
    }
}