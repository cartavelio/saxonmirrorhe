using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class ImportedFunctionLibrary : Saxon.Hej.functions.FunctionLibrary, Saxon.Hej.query.XQueryFunctionBinder {
        private Saxon.Hej.query.QueryModule importingModule;
        private readonly Saxon.Hej.query.XQueryFunctionLibrary baseLibrary;
        private readonly System.Collections.Generic.HashSet<string> namespaces = new System.Collections.Generic.HashSet<string>(5);
        public ImportedFunctionLibrary(Saxon.Hej.query.QueryModule importingModule, Saxon.Hej.query.XQueryFunctionLibrary baseLibrary)  {
            this.importingModule = importingModule;
            this.baseLibrary = baseLibrary;
        }
        public void addImportedNamespace(string @namespace) {
            namespaces.Add(@namespace);
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.om.StructuredQName functionName = symbolicName.getComponentName();
            string uri = functionName.getURI();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
            foreach (Saxon.Hej.expr.Expression arg in staticArgs) {
                if ((arg.getLocalRetainedStaticContext() == null)) {
                    arg.setRetainedStaticContext(rsc);
                }
            }
            if (namespaces.Contains(uri)) {
                return baseLibrary.bind(symbolicName, staticArgs, env, reasons);
            } else {
                return null;
            }
        }
        public Saxon.Hej.query.XQueryFunction getDeclaration(Saxon.Hej.om.StructuredQName functionName, int staticArgs) {
            string uri = functionName.getURI();
            if (namespaces.Contains(uri)) {
                return baseLibrary.getDeclaration(functionName, staticArgs);
            } else {
                return null;
            }
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.query.ImportedFunctionLibrary lib = new Saxon.Hej.query.ImportedFunctionLibrary(importingModule, baseLibrary);
            foreach (string ns in namespaces) {
                lib.addImportedNamespace(ns);
            }
            return lib;
        }
        public void setImportingModule(Saxon.Hej.query.QueryModule importingModule) {
            this.importingModule = importingModule;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            if (namespaces.Contains(functionName.getComponentName().getURI())) {
                return baseLibrary.getFunctionItem(functionName, staticContext);
            } else {
                return null;
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName) {
            return (namespaces.Contains(functionName.getComponentName().getURI()) && baseLibrary.isAvailable(functionName));
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}