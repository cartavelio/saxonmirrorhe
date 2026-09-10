using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLAccept : Saxon.Hej.style.XSLAcceptExpose {
        protected internal override void prepareAttributes() {
            base.prepareAttributes();
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if (!(((parent is Saxon.Hej.style.XSLUsePackage)))) {
                compileError("Parent of xsl:accept must be xsl:use-package");
                return ;
            }
            Saxon.Hej.style.StylesheetPackage pack = ((Saxon.Hej.style.XSLUsePackage)(parent)).getUsedPackage();
            if ((pack != null)) {
                foreach (Saxon.Hej.trans.ComponentTest test in getExplicitComponentTests()) {
                    Saxon.Hej.pattern.QNameTest nameTest = test.getQNameTest();
                    if ((nameTest is Saxon.Hej.pattern.NameTest)) {
                        int kind = test.getComponentKind();
                        Saxon.Hej.trans.SymbolicName sName = ((kind == Saxon.Hej.om.StandardNames.XSL_FUNCTION) ? new Saxon.Hej.trans.SymbolicName.F(((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName(), test.getArity()) : new Saxon.Hej.trans.SymbolicName(kind, ((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName()));
                        Saxon.Hej.expr.Component comp = pack.getComponent(sName);
                        bool found = false;
                        if ((comp == null)) {
                            if (((kind == Saxon.Hej.om.StandardNames.XSL_FUNCTION) && (test.getArity() == -1))) {
                                for (int i = 0; (i<=pack.getMaxFunctionArity()); i++) {
                                    sName = new Saxon.Hej.trans.SymbolicName.F(((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName(), i);
                                    comp = pack.getComponent(sName);
                                    if ((comp != null)) {
                                        checkCompatibility(sName, comp.getVisibility(), getVisibility());
                                        found = true;
                                    }
                                }
                            }
                        } else {
                            checkCompatibility(sName, comp.getVisibility(), getVisibility());
                            found = true;
                        }
                        if (!(found)) {
                            compileError((("No "+sName.ToString())+" exists in the used package"), "XTSE3030");
                        }
                    }
                }
            }
        }
        protected internal void checkCompatibility(Saxon.Hej.trans.SymbolicName name, Saxon.Hej.trans.Visibility declared, Saxon.Hej.trans.Visibility exposed) {
            if (!(isCompatible(declared, exposed))) {
                string code = "XTSE3040";
                compileError(((((("The "+name)+" is declared as ")+Saxon.Hej.trans.Err.describeVisibility(declared))+" and cannot be accepted as ")+Saxon.Hej.trans.Err.describeVisibility(exposed)), code);
            }
        }
        public static bool isCompatible(Saxon.Hej.trans.Visibility declared, Saxon.Hej.trans.Visibility exposed) {
            switch (declared) {
              case Saxon.Hej.trans.Visibility.PUBLIC:
                return ((((exposed == Saxon.Hej.trans.Visibility.PUBLIC) || (exposed == Saxon.Hej.trans.Visibility.PRIVATE)) || (exposed == Saxon.Hej.trans.Visibility.FINAL)) || (exposed == Saxon.Hej.trans.Visibility.HIDDEN));
              case Saxon.Hej.trans.Visibility.ABSTRACT:
                return ((exposed == Saxon.Hej.trans.Visibility.ABSTRACT) || (exposed == Saxon.Hej.trans.Visibility.HIDDEN));
              case Saxon.Hej.trans.Visibility.FINAL:
                return (((exposed == Saxon.Hej.trans.Visibility.PRIVATE) || (exposed == Saxon.Hej.trans.Visibility.FINAL)) || (exposed == Saxon.Hej.trans.Visibility.HIDDEN));
              case Saxon.Hej.trans.Visibility.UNDEFINED:
                return true;
              default:
                return false;
            }
        }
    
    }
}