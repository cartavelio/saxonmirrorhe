using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class FunctionLibraryList : Saxon.Hej.functions.FunctionLibrary, Saxon.Hej.query.XQueryFunctionBinder {
        public System.Collections.Generic.IList<Saxon.Hej.functions.FunctionLibrary> libraryList = new System.Collections.Generic.List<Saxon.Hej.functions.FunctionLibrary>(8);
        public FunctionLibraryList()  {
        }
        public int addFunctionLibrary(Saxon.Hej.functions.FunctionLibrary lib) {
            libraryList.Add(lib);
            return (libraryList.Count-1);
        }
        public Saxon.Hej.functions.FunctionLibrary get(int n) {
            return libraryList[n];
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                Saxon.Hej.om.FunctionItem fi = lib.getFunctionItem(functionName, staticContext);
                if ((fi != null)) {
                    return fi;
                }
            }
            return null;
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                if (lib.isAvailable(functionName, languageLevel)) {
                    return true;
                }
            }
            return false;
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            bool debug = env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_EXTERNAL_FUNCTIONS);
            Saxon.Hej.lib.Logger err = env.getConfiguration().getLogger();
            if (debug) {
                err.info(((("Looking for function "+functionName.getComponentName().getEQName())+"#")+functionName.getArity()));
            }
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                if (debug) {
                    err.info(("Trying "+lib.GetType().Name));
                }
                Saxon.Hej.expr.Expression func = lib.bind(functionName, staticArgs, keywords, env, reasons);
                if ((func != null)) {
                    return func;
                }
            }
            if (debug) {
                err.info((("Function "+functionName.getComponentName().getEQName())+" not found!"));
            }
            return null;
        }
        public Saxon.Hej.query.XQueryFunction getDeclaration(Saxon.Hej.om.StructuredQName functionName, int staticArgs) {
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                if ((lib is Saxon.Hej.query.XQueryFunctionBinder)) {
                    Saxon.Hej.query.XQueryFunction func = ((Saxon.Hej.query.XQueryFunctionBinder)(lib)).getDeclaration(functionName, staticArgs);
                    if ((func != null)) {
                        return func;
                    }
                }
            }
            return null;
        }
        public bool bindUnboundFunctionCall(Saxon.Hej.expr.UserFunctionCall call, System.Collections.Generic.IList<string> reasons) {
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                if ((lib is Saxon.Hej.query.XQueryFunctionBinder)) {
                    bool found = ((Saxon.Hej.query.XQueryFunctionBinder)(lib)).bindUnboundFunctionCall(call, reasons);
                    if (found) {
                        return true;
                    }
                }
            }
            return false;
        }
        public System.Collections.Generic.IList<Saxon.Hej.functions.FunctionLibrary> getLibraryList() {
            return libraryList;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.functions.FunctionLibraryList fll = new Saxon.Hej.functions.FunctionLibraryList();
            fll.libraryList = emptyFunctionLibraryList(libraryList.Count);
            for (int i = 0; (i<libraryList.Count); i++) {
                fll.libraryList.Add(libraryList[i].copy());
            }
            return fll;
        }
        private static System.Collections.Generic.List<Saxon.Hej.functions.FunctionLibrary> emptyFunctionLibraryList(int allocated) {
            return new System.Collections.Generic.List<Saxon.Hej.functions.FunctionLibrary>(allocated);
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}