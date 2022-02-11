using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class UnboundFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private System.Collections.Generic.IList<Saxon.Hej.expr.UserFunctionResolvable> unboundFunctionReferences = new System.Collections.Generic.List<Saxon.Hej.expr.UserFunctionResolvable>(20);
        private System.Collections.Generic.IList<Saxon.Hej.expr.StaticContext> correspondingStaticContext = new System.Collections.Generic.List<Saxon.Hej.expr.StaticContext>(20);
        private readonly System.Collections.Generic.IList<System.Collections.Generic.IList<string>> correspondingReasons = new System.Collections.Generic.List<System.Collections.Generic.IList<string>>();
        private bool resolving = false;
        public UnboundFunctionLibrary()  {
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] arguments, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            if (resolving) {
                return null;
            }
            if ((!(reasons.Count==0) && reasons[0].StartsWith("Cannot call the private XQuery function"))) {
                return null;
            }
            Saxon.Hej.expr.UserFunctionCall ufc = new Saxon.Hej.expr.UserFunctionCall();
            ufc.setFunctionName(functionName.getComponentName());
            ufc.setArguments(arguments);
            unboundFunctionReferences.Add(ufc);
            correspondingStaticContext.Add(env);
            correspondingReasons.Add(reasons);
            return ufc;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            if (resolving) {
                return null;
            }
            Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable uc = new Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable(functionName);
            unboundFunctionReferences.Add(uc);
            correspondingStaticContext.Add(null);
            correspondingReasons.Add(new System.Collections.Generic.List<string>());
            Saxon.Hej.functions.CallableFunction fi = new Saxon.Hej.functions.CallableFunction(functionName, uc, Saxon.Hej.type.AnyFunctionType.getInstance());
            return fi;
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName) {
            return false;
        }
        public void bindUnboundFunctionReferences(Saxon.Hej.query.XQueryFunctionBinder lib, Saxon.Hej.Configuration config) {
            resolving = true;
            for (int i = 0; (i<unboundFunctionReferences.Count); i++) {
                Saxon.Hej.expr.UserFunctionResolvable @ref = unboundFunctionReferences[i];
                if ((@ref is Saxon.Hej.expr.UserFunctionCall)) {
                    Saxon.Hej.expr.UserFunctionCall ufc = (Saxon.Hej.expr.UserFunctionCall)(@ref);
                    Saxon.Hej.query.QueryModule importingModule = (Saxon.Hej.query.QueryModule)(correspondingStaticContext[i]);
                    if ((importingModule == null)) {
                        continue ;
                    }
                    correspondingStaticContext[i] = null;
                    Saxon.Hej.om.StructuredQName q = ufc.getFunctionName();
                    int arity = ufc.getArity();
                    Saxon.Hej.query.XQueryFunction fd = lib.getDeclaration(q, arity);
                    if ((fd != null)) {
                        fd.registerReference(ufc);
                        ufc.setStaticType(fd.getResultType());
                    } else {
                        Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((((("Cannot find a "+arity)+"-argument function named ")+q.getEQName())+"()"));
                        System.Collections.Generic.IList<string> reasons = correspondingReasons[i];
                        foreach (string reason in reasons) {
                            sb.append(". ").append(reason);
                        }
                        if (reasons.Count==0) {
                            string supplementary = Saxon.Hej.expr.parser.XPathParser.getMissingFunctionExplanation(q, config);
                            if ((supplementary != null)) {
                                sb.append(". ").append(supplementary);
                            }
                        }
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(sb.ToString(), "XPST0017", ufc.getLocation());
                        err.setIsStaticError(true);
                        throw err;
                    }
                } else {
                    if ((@ref is Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable)) {
                        Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable uc = (Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable)(@ref);
                        Saxon.Hej.om.StructuredQName q = uc.getFunctionName();
                        int arity = uc.getArity();
                        Saxon.Hej.query.XQueryFunction fd = lib.getDeclaration(q, arity);
                        if ((fd != null)) {
                            fd.registerReference(uc);
                        } else {
                            string msg = (((("Cannot find a "+arity)+"-argument function named ")+q.getEQName())+"()");
                            if (!(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS))) {
                                msg += ". Note: external function calls have been disabled";
                            }
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg);
                            err.setErrorCode("XPST0017");
                            err.setIsStaticError(true);
                            throw err;
                        }
                    }
                }
            }
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.query.UnboundFunctionLibrary qfl = new Saxon.Hej.query.UnboundFunctionLibrary();
            qfl.unboundFunctionReferences = new System.Collections.Generic.List<Saxon.Hej.expr.UserFunctionResolvable>(unboundFunctionReferences);
            qfl.correspondingStaticContext = new System.Collections.Generic.List<Saxon.Hej.expr.StaticContext>(correspondingStaticContext);
            qfl.resolving = resolving;
            return qfl;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}