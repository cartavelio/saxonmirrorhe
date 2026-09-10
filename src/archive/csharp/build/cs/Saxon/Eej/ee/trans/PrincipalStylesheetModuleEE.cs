using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class PrincipalStylesheetModuleEE : Saxon.Hej.style.PrincipalStylesheetModule {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.Expression,Saxon.Hej.expr.instruct.GlobalVariable> indexOfVariablesByInitializer = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression, Saxon.Hej.expr.instruct.GlobalVariable>();
        public PrincipalStylesheetModuleEE(Saxon.Hej.style.XSLPackage node) : base (node) {
        }
        protected internal override void importSchemata() {
            bool importsSchemas = false;
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLImportSchema)) {
                    importsSchemas = true;
                    break ;
                }
            }
            if (importsSchemas) {
                lock (getConfiguration()) {
                    for (int i = (topLevel.Count-1); (i>=0); i--) {
                        Saxon.Hej.style.ComponentDeclaration decl = topLevel[i];
                        if ((decl.getSourceElement() is Saxon.Hej.style.XSLImportSchema)) {
                            ((Saxon.Hej.style.XSLImportSchema)(decl.getSourceElement())).readSchema();
                        }
                        if ((decl.getSourceElement() is Saxon.Hej.style.XSLItemType)) {
                            ((Saxon.Hej.style.XSLItemType)(decl.getSourceElement())).indexTypeAlias(decl, this);
                        }
                    }
                    Saxon.Hej.Configuration config = getConfiguration();
                    foreach (Saxon.Hej.om.NamespaceUri ns in getStylesheetPackage().getSchemaNamespaces()) {
                        config.sealNamespace(ns);
                    }
                }
            }
        }
        protected internal override void checkStreamability(Saxon.Hej.expr.instruct.AttributeSet aSet) {
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            Saxon.Hej.expr.Expression body = aSet.getBody();
            if ((body != null)) {
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true));
                info.setContainer(aSet);
                Saxon.Eej.ee.stream.PostureAndSweep s1 = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(body, info, reasons);
                if ((s1.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                    string message = (("Attribute set "+aSet.getObjectName().getDisplayName())+" is declared streamable but is not actually streamable. ");
                    foreach (string s in reasons) {
                        message += ("\n    "+s);
                    }
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, "XTSE3430");
                    err.setLocator(aSet);
                    compileError(err);
                }
            }
        }
        private static bool isVariableReusable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
            Saxon.Hej.expr.Expression select = variable.getBody();
            return ((((select != null) && !(variable.isAssignable())) && !(((variable is Saxon.Hej.expr.instruct.GlobalParam)))) && ((((select.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT)) == 0)));
        }
        public override void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
            base.addGlobalVariable(variable);
            if (isVariableReusable(variable)) {
                Saxon.Impl.Helpers.MapUtils.Put(indexOfVariablesByInitializer, variable.getBody(), variable);
            }
        }
        public override Saxon.Hej.expr.instruct.GlobalVariable getEquivalentVariable(Saxon.Hej.expr.Expression select) {
            return Saxon.Impl.Helpers.MapUtils.Get(indexOfVariablesByInitializer, select);
        }
    
    }
}