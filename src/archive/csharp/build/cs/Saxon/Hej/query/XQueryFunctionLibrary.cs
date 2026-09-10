using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class XQueryFunctionLibrary : Saxon.Hej.functions.FunctionLibrary, Saxon.Hej.query.XQueryFunctionBinder {
        private Saxon.Hej.Configuration config;
        private System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.query.XQueryFunction> functions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.query.XQueryFunction>(20);
        public XQueryFunctionLibrary(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void declareFunction(Saxon.Hej.query.XQueryFunction function) {
            Saxon.Hej.trans.SymbolicName keyObj = function.getIdentificationKey();
            Saxon.Hej.query.XQueryFunction existing = Saxon.Impl.Helpers.MapUtils.Get(functions, keyObj);
            if ((existing == function)) {
                return ;
            }
            if ((existing != null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("Duplicate definition of function "+function.getDisplayName())+" (see line ")+existing.getLineNumber())+" in ")+existing.getSystemId())+')'));
                err.setErrorCode("XQST0034");
                err.setIsStaticError(true);
                err.setLocator(function);
                throw err;
            }
            Saxon.Impl.Helpers.MapUtils.Put(functions, keyObj, function);
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.query.XQueryFunction fd = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName);
            if ((fd != null)) {
                if ((fd.isPrivate() && !(fd.getSystemId().Equals(staticContext.getStaticBaseURI())))) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot call the private function "+functionName.getComponentName().getDisplayName())+" from outside its module"), "XPST0017");
                }
                Saxon.Hej.expr.instruct.UserFunction fn = fd.getUserFunction();
                Saxon.Hej.type.FunctionItemType type = new Saxon.Hej.type.SpecificFunctionType(fd.getArgumentTypes(), fd.getResultType(), fd.getAnnotations());
                if ((fn == null)) {
                    Saxon.Hej.expr.instruct.UserFunction uf = new Saxon.Hej.expr.instruct.UserFunction();
                    uf.setFunctionName(functionName.getComponentName());
                    uf.setResultType(fd.getResultType());
                    uf.setParameterDefinitions(fd.getParameterDefinitions());
                    Saxon.Hej.functions.hof.UserFunctionReference @ref = new Saxon.Hej.functions.hof.UserFunctionReference(uf);
                    fd.registerReference(@ref);
                    return new Saxon.Hej.functions.hof.UnresolvedXQueryFunctionItem(fd, functionName, @ref);
                } else {
                    return fn;
                }
            } else {
                return null;
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return (Saxon.Impl.Helpers.MapUtils.Get(functions, functionName) != null);
        }
        public class UnresolvedCallable : Saxon.Hej.expr.UserFunctionResolvable, Saxon.Hej.expr.Callable {
            internal Saxon.Hej.trans.SymbolicName.F symbolicName;
            internal Saxon.Hej.expr.instruct.UserFunction function;
            public UnresolvedCallable(Saxon.Hej.trans.SymbolicName.F symbolicName)  {
                this.symbolicName = symbolicName;
            }
            public Saxon.Hej.om.StructuredQName getFunctionName() {
                return symbolicName.getComponentName();
            }
            public int getArity() {
                return symbolicName.getArity();
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                if ((function == null)) {
                    throw new Saxon.Hej.trans.XPathException("Forwards reference to XQuery function has not been resolved");
                }
                Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[arguments.Length];
                for (int i = 0; (i<arguments.Length); i++) {
                    args[i] = Saxon.Hej.om.SequenceTool.toGroundedValue(arguments[i].iterate());
                }
                return function.call(context.newCleanContext(), args);
            }
            public void setFunction(Saxon.Hej.expr.instruct.UserFunction function) {
                this.function = function;
            }
            public Saxon.Hej.expr.instruct.UserFunction getFunction() {
                return function;
            }
        
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] arguments, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.query.XQueryFunction fd = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName);
            if ((fd != null)) {
                if ((fd.isPrivate() && (fd.getStaticContext() != env))) {
                    reasons.Add((("Cannot call the private XQuery function "+functionName.getComponentName().getDisplayName())+" from outside its module"));
                    return null;
                }
                Saxon.Hej.expr.UserFunctionCall ufc = new Saxon.Hej.expr.UserFunctionCall();
                ufc.setFunctionName(fd.getFunctionName());
                ufc.setArguments(arguments);
                ufc.setStaticType(fd.getResultType());
                Saxon.Hej.expr.instruct.UserFunction fn = fd.getUserFunction();
                if ((fn == null)) {
                    fd.registerReference(ufc);
                } else {
                    ufc.setFunction(fn);
                }
                return ufc;
            } else {
                return null;
            }
        }
        public Saxon.Hej.query.XQueryFunction getDeclaration(Saxon.Hej.om.StructuredQName functionName, int staticArgs) {
            Saxon.Hej.trans.SymbolicName functionKey = Saxon.Hej.query.XQueryFunction.getIdentificationKey(functionName, staticArgs);
            return Saxon.Impl.Helpers.MapUtils.Get(functions, functionKey);
        }
        public Saxon.Hej.query.XQueryFunction getDeclarationByKey(Saxon.Hej.trans.SymbolicName functionKey) {
            return Saxon.Impl.Helpers.MapUtils.Get(functions, functionKey);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.query.XQueryFunction> getFunctionDefinitions() {
            return functions.Values;
        }
        protected internal void fixupGlobalFunctions(Saxon.Hej.query.QueryModule env) {
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
            foreach (Saxon.Hej.query.XQueryFunction fn in functions.Values) {
                fn.compile();
            }
            foreach (Saxon.Hej.query.XQueryFunction fn in functions.Values) {
                fn.checkReferences(visitor);
            }
        }
        protected internal void optimizeGlobalFunctions(Saxon.Hej.query.QueryModule topModule) {
            foreach (Saxon.Hej.query.XQueryFunction fn in functions.Values) {
                if ((((Saxon.Hej.query.QueryModule)(fn.getStaticContext())).getTopLevelModule() == topModule)) {
                    fn.optimize();
                }
            }
        }
        public void explainGlobalFunctions(Saxon.Hej.trace.ExpressionPresenter @out) {
            foreach (Saxon.Hej.query.XQueryFunction fn in functions.Values) {
                fn.explain(@out);
            }
        }
        public Saxon.Hej.expr.instruct.UserFunction getUserDefinedFunction(string uri, string localName, int arity) {
            Saxon.Hej.trans.SymbolicName functionKey = new Saxon.Hej.trans.SymbolicName.F(new Saxon.Hej.om.StructuredQName("", uri, localName), arity);
            Saxon.Hej.query.XQueryFunction fd = Saxon.Impl.Helpers.MapUtils.Get(functions, functionKey);
            if ((fd == null)) {
                return null;
            }
            return fd.getUserFunction();
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.query.XQueryFunctionLibrary qfl = new Saxon.Hej.query.XQueryFunctionLibrary(config);
            qfl.functions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.query.XQueryFunction>(functions);
            return qfl;
        }
    
    }
}