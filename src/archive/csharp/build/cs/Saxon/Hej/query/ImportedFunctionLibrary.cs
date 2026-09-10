using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class ImportedFunctionLibrary : Saxon.Hej.functions.FunctionLibrary, Saxon.Hej.query.XQueryFunctionBinder {
        private Saxon.Hej.query.QueryModule importingModule;
        private readonly Saxon.Hej.query.XQueryFunctionLibrary baseLibrary;
        private readonly System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri> namespaces = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(5);
        public ImportedFunctionLibrary(Saxon.Hej.query.QueryModule importingModule, Saxon.Hej.query.XQueryFunctionLibrary baseLibrary)  {
            this.importingModule = importingModule;
            this.baseLibrary = baseLibrary;
        }
        public void addImportedNamespace(Saxon.Hej.om.NamespaceUri @namespace) {
            namespaces.Add(@namespace);
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.om.StructuredQName functionName = symbolicName.getComponentName();
            Saxon.Hej.om.NamespaceUri uri = functionName.getNamespaceUri();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
            foreach (Saxon.Hej.expr.Expression arg in staticArgs) {
                if ((arg.getLocalRetainedStaticContext() == null)) {
                    arg.setRetainedStaticContext(rsc);
                }
            }
            if (namespaces.Contains(uri)) {
                return baseLibrary.bind(symbolicName, staticArgs, null, env, reasons);
            } else {
                return null;
            }
        }
        public Saxon.Hej.query.XQueryFunction getDeclaration(Saxon.Hej.om.StructuredQName functionName, int staticArgs) {
            Saxon.Hej.om.NamespaceUri uri = functionName.getNamespaceUri();
            if (namespaces.Contains(uri)) {
                return baseLibrary.getDeclaration(functionName, staticArgs);
            } else {
                return null;
            }
        }
        public bool bindUnboundFunctionCall(Saxon.Hej.expr.UserFunctionCall call, System.Collections.Generic.IList<string> reasons) {
            return baseLibrary.bindUnboundFunctionCall(call, reasons);
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.query.ImportedFunctionLibrary lib = new Saxon.Hej.query.ImportedFunctionLibrary(importingModule, baseLibrary);
            foreach (Saxon.Hej.om.NamespaceUri ns in namespaces) {
                lib.addImportedNamespace(ns);
            }
            return lib;
        }
        public void setImportingModule(Saxon.Hej.query.QueryModule importingModule) {
            this.importingModule = importingModule;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            if (namespaces.Contains(functionName.getComponentName().getNamespaceUri())) {
                return baseLibrary.getFunctionItem(functionName, staticContext);
            } else {
                return null;
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return (namespaces.Contains(functionName.getComponentName().getNamespaceUri()) && baseLibrary.isAvailable(functionName, languageLevel));
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}