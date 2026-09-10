using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class XQueryFunctionLibrary : Saxon.Hej.functions.FunctionLibrary, Saxon.Hej.query.XQueryFunctionBinder {
        private Saxon.Hej.Configuration config;
        private System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.query.XQueryFunction> functions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.query.XQueryFunction>(20);
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,System.Collections.Generic.IList<Saxon.Hej.query.XQueryFunction>> functionsByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, System.Collections.Generic.IList<Saxon.Hej.query.XQueryFunction>>(20);
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
            Saxon.Hej.om.StructuredQName functionName = function.getFunctionName();
            System.Collections.Generic.IList<Saxon.Hej.query.XQueryFunction> existingFunctions = Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(functionsByName, functionName, k => new System.Collections.Generic.List<Saxon.Hej.query.XQueryFunction>(2));
            foreach (Saxon.Hej.query.XQueryFunction existing in existingFunctions) {
                if ((existing == function)) {
                    return ;
                }
                if (hasOverlappingArity(function, existing)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("Conflicting definition of function "+function.getDisplayName())+" (see line ")+existing.getLineNumber())+" in ")+existing.getSystemId())+')'));
                    err.setErrorCode("XQST0034");
                    err.setIsStaticError(true);
                    err.setLocator(function);
                    throw err;
                }
            }
            Saxon.Impl.Helpers.MapUtils.Put(functions, keyObj, function);
            existingFunctions.Add(function);
        }
        private static bool hasOverlappingArity(Saxon.Hej.query.XQueryFunction f1, Saxon.Hej.query.XQueryFunction f2) {
            return ((f1.getMinimumArity()<=f2.getNumberOfParameters()) && (f2.getMinimumArity()<=f1.getNumberOfParameters()));
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.query.XQueryFunction fd = getDeclaration(functionName.getComponentName(), functionName.getArity());
            if ((fd != null)) {
                if ((fd.isPrivate() && !(fd.getSystemId().Equals(staticContext.getStaticBaseURI())))) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot call the private function "+functionName.getComponentName().getDisplayName())+" from outside its module"), "XPST0017");
                }
                Saxon.Hej.expr.instruct.UserFunction fn = fd.getUserFunction();
                if ((fn == null)) {
                    Saxon.Hej.expr.instruct.UserFunction uf = new Saxon.Hej.expr.instruct.UserFunction();
                    uf.setFunctionName(functionName.getComponentName());
                    uf.setResultType(fd.getResultType());
                    uf.setParameterDefinitions(fd.getParameterDefinitions());
                    Saxon.Hej.functions.hof.UserFunctionReference @ref = new Saxon.Hej.functions.hof.UserFunctionReference(uf, functionName);
                    fd.registerReference(@ref);
                    return new Saxon.Hej.functions.hof.UnresolvedXQueryFunctionItem(fd, functionName, @ref);
                } else {
                    if ((functionName.getArity() == fd.getNumberOfParameters())) {
                        return fn;
                    } else {
                        Saxon.Hej.expr.Callable callable = new Saxon.Hej.query.XQueryFunctionLibrary.ReducedArityCallable(fd, fn);
                        Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[functionName.getArity()];
                        for (int i = 0; (i<functionName.getArity()); i++) {
                            argTypes[i] = fd.getArgumentTypes()[i];
                        }
                        Saxon.Hej.type.SpecificFunctionType functionType = new Saxon.Hej.type.SpecificFunctionType(argTypes, fd.getResultType());
                        return new Saxon.Hej.functions.CallableFunction(functionName, callable, functionType);
                    }
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
                    args[i] = arguments[i].materialize();
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
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] arguments, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.query.XQueryFunction fd = getDeclaration(functionName.getComponentName(), arguments.Length);
            if ((fd != null)) {
                if ((fd.isPrivate() && (fd.getStaticContext() != env))) {
                    reasons.Add((("Cannot call the private XQuery function "+functionName.getComponentName().getDisplayName())+" from outside its module"));
                    return null;
                }
                Saxon.Hej.expr.UserFunctionCall ufc = new Saxon.Hej.expr.UserFunctionCall();
                ufc.setFunctionName(fd.getFunctionName());
                int maxArity = fd.getNumberOfParameters();
                if (((arguments.Length == maxArity) && (((keywords == null) || keywords.Count==0)))) {
                    ufc.setArguments(arguments);
                } else {
                    Saxon.Hej.expr.Expression[] expandedArgs = Saxon.Hej.expr.instruct.UserFunction.makeExpandedArgumentArray(arguments, keywords, fd);
                    ufc.setArguments(expandedArgs);
                }
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
            System.Collections.Generic.IList<Saxon.Hej.query.XQueryFunction> homonyms = Saxon.Impl.Helpers.MapUtils.Get(functionsByName, functionName);
            if ((homonyms != null)) {
                foreach (Saxon.Hej.query.XQueryFunction f in homonyms) {
                    if (((f.getMinimumArity()<=staticArgs) && (f.getNumberOfParameters()>=staticArgs))) {
                        return f;
                    }
                }
            }
            return null;
        }
        public bool bindUnboundFunctionCall(Saxon.Hej.expr.UserFunctionCall ufc, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.query.UnboundFunctionLibrary.UnboundFunctionCallDetails details = ufc.getUnboundCallDetails();
            System.Diagnostics.Debug.Assert((details != null));
            Saxon.Hej.om.StructuredQName functionName = details.functionName.getComponentName();
            Saxon.Hej.expr.Expression[] arguments = details.arguments;
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords = details.keywords;
            Saxon.Hej.query.XQueryFunction fd = getDeclaration(functionName, arguments.Length);
            if ((fd != null)) {
                if ((fd.isPrivate() && (fd.getStaticContext() != details.env))) {
                    reasons.Add((("Cannot call the private XQuery function "+functionName.getDisplayName())+" from outside its module"));
                    return false;
                }
                ufc.setFunctionName(fd.getFunctionName());
                int maxArity = fd.getNumberOfParameters();
                if (((arguments.Length == maxArity) && (((details.keywords == null) || details.keywords.Count==0)))) {
                    ufc.setArguments(arguments);
                } else {
                    Saxon.Hej.expr.Expression[] expandedArgs = Saxon.Ejava.util.Arrays.copyOf(arguments, maxArity);
                    if ((keywords != null)) {
                        int positionalArgs = (arguments.Length-keywords.Count);
                        foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,int> entry in keywords) {
                            Saxon.Hej.om.StructuredQName key = entry.Key;
                            int argPos = entry.Value;
                            int paramPos = fd.getPositionOfParameter(key);
                            if ((paramPos<0)) {
                                throw new Saxon.Hej.trans.UncheckedXPathException((("Keyword "+key)+" does not match the name of any declared parameter"), "XPST0142");
                            }
                            if ((paramPos<positionalArgs)) {
                                throw new Saxon.Hej.trans.UncheckedXPathException((("Parameter "+key)+" is supplied both by position and by keyword"), "XPST0141");
                            }
                            Saxon.Hej.expr.Expression supplied = arguments[paramPos];
                            expandedArgs[argPos] = null;
                            expandedArgs[paramPos] = supplied;
                        }
                    }
                    for (int a = 0; (a<maxArity); a++) {
                        if ((expandedArgs[a] == null)) {
                            expandedArgs[a] = fd.getParameterDefinitions()[a].getDefaultValueExpression();
                        }
                    }
                    ufc.setArguments(expandedArgs);
                }
                ufc.setStaticType(fd.getResultType());
                Saxon.Hej.expr.instruct.UserFunction fn = fd.getUserFunction();
                if ((fn == null)) {
                    fd.registerReference(ufc);
                } else {
                    ufc.setFunction(fn);
                }
                return true;
            } else {
                return false;
            }
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
        public Saxon.Hej.expr.instruct.UserFunction getUserDefinedFunction(Saxon.Hej.om.NamespaceUri uri, string localName, int arity) {
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
        private class ReducedArityCallable : Saxon.Hej.expr.Callable {
            private readonly Saxon.Hej.query.XQueryFunction declaredFunction;
            private readonly Saxon.Hej.expr.instruct.UserFunction userFunction;
            public ReducedArityCallable(Saxon.Hej.query.XQueryFunction fd, Saxon.Hej.expr.instruct.UserFunction fn)  {
                this.declaredFunction = fd;
                this.userFunction = fn;
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Sequence[] extendedArguments = Saxon.Ejava.util.Arrays.copyOf(arguments, userFunction.getArity());
                for (int i = arguments.Length; (i<userFunction.getArity()); i++) {
                    extendedArguments[i] = declaredFunction.getParameterDefinitions()[i].getDefaultValueExpression().makeElaborator().eagerly().evaluate(context);
                }
                return userFunction.call(context, extendedArguments);
            }
        
        }
    
    }
}