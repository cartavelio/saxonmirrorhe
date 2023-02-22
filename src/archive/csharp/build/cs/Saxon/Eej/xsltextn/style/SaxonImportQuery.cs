using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonImportQuery : Saxon.Hej.style.StyleElement {
        private string href;
        private string moduleURI;
        protected internal override void prepareAttributes() {
            if (((href != null) || (moduleURI != null))) {
                return ;
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("href")) {
                    href = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (f.Equals("namespace")) {
                        moduleURI = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        checkUnknownAttribute(attName);
                        moduleURI = "";
                    }
                }
            }
            if ((moduleURI == null)) {
                reportAbsence("namespace");
            }
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            prepareAttributes();
            if ((moduleURI == null)) {
                return ;
            }
            try {
                Saxon.Hej.style.PrincipalStylesheetModule top1 = getPrincipalStylesheetModule();
                Saxon.Hej.query.QueryModule importedModule = getQueryLibrary(moduleURI);
                if ((importedModule == null)) {
                    if ((href == null)) {
                        compileError(("Cannot locate query module for namespace "+moduleURI), "XQST0059");
                        return ;
                    }
                    importedModule = loadModule();
                }
                foreach (Saxon.Hej.expr.instruct.GlobalVariable queryVar in importedModule.getPackageData().getGlobalVariableList()) {
                    if ((!(queryVar.isPrivate()) && (top1.getGlobalVariableBinding(queryVar.getVariableQName()) != null))) {
                        throw new Saxon.Hej.trans.XPathException(("Duplicate global variable declaration "+queryVar.getVariableQName().getDisplayName()), "XQST0049");
                    }
                    top1.addGlobalVariable(queryVar, (queryVar.isPrivate() ? Saxon.Hej.trans.Visibility.HIDDEN : Saxon.Hej.trans.Visibility.PRIVATE));
                }
                foreach (Saxon.Hej.query.XQueryFunction def in importedModule.getGlobalFunctionLibrary().getFunctionDefinitions()) {
                    if ((def.getFunctionName().hasURI(moduleURI) && !(def.isPrivate()))) {
                        top1.declareXQueryFunction(def);
                        Saxon.Hej.expr.instruct.UserFunction fn = def.getUserFunction();
                        Saxon.Hej.style.StylesheetPackage pack = top1.getStylesheetPackage();
                        Saxon.Hej.expr.Component component = fn.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, pack);
                        if ((fn.getPackageData() == null)) {
                            fn.setPackageData(pack);
                        }
                        Saxon.Impl.Helpers.MapUtils.Put(pack.getComponentIndex(), def.getIdentificationKey(), component);
                    }
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            checkTopLevel("XTSE0010", false);
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:import-query", getPackageData().getLocalLicenseId());
        }
        private Saxon.Hej.query.QueryLibrary getQueryLibrary(string @namespace) {
            Saxon.Hej.trans.CompilerInfo info = getCompilation().getCompilerInfo();
            System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> libs = info.getQueryLibraries();
            if ((libs != null)) {
                foreach (Saxon.Hej.query.QueryLibrary lib in libs) {
                    if (lib.getModuleNamespace().Equals(@namespace)) {
                        return lib;
                    }
                }
            }
            return null;
        }
        private Saxon.Hej.query.QueryModule loadModule() {
            Saxon.Hej.query.StaticQueryContext sqc = getConfiguration().newStaticQueryContext();
            sqc.setBaseURI(getBaseURI());
            sqc.setErrorReporter(getCompilation().getCompilerInfo().getErrorReporter());
            string mainModule = (((("import module namespace m = \""+moduleURI)+"\" at \"")+href)+"\"; ()");
            Saxon.Hej.query.XQueryExpression exp = sqc.compileQuery(mainModule);
            return exp.getMainModule();
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
    
    }
}