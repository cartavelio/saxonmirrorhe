using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLExpose : Saxon.Hej.style.XSLAcceptExpose {
        protected internal void checkCompatibility(Saxon.Hej.trans.SymbolicName name, Saxon.Hej.trans.Visibility declared, Saxon.Hej.trans.Visibility exposed) {
            if (!(isCompatible(declared, exposed))) {
                string code = "XTSE3010";
                compileError(((((("The "+name)+" is declared as ")+Saxon.Hej.trans.Err.describeVisibility(declared))+" and cannot be exposed as ")+Saxon.Hej.trans.Err.describeVisibility(exposed)), code);
            }
        }
        public static bool isCompatible(Saxon.Hej.trans.Visibility declared, Saxon.Hej.trans.Visibility exposed) {
            if (((declared == exposed) || (declared == Saxon.Hej.trans.Visibility.UNDEFINED))) {
                return true;
            }
            switch (declared) {
              case Saxon.Hej.trans.Visibility.PUBLIC:
                return (((exposed == Saxon.Hej.trans.Visibility.PRIVATE) || (exposed == Saxon.Hej.trans.Visibility.FINAL)) || (exposed == Saxon.Hej.trans.Visibility.HIDDEN));
              case Saxon.Hej.trans.Visibility.ABSTRACT:
                return (exposed == Saxon.Hej.trans.Visibility.HIDDEN);
              case Saxon.Hej.trans.Visibility.FINAL:
                return ((exposed == Saxon.Hej.trans.Visibility.PRIVATE) || (exposed == Saxon.Hej.trans.Visibility.HIDDEN));
              default:
                return false;
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.PrincipalStylesheetModule psm = getPrincipalStylesheetModule();
            Saxon.Hej.trans.Visibility exposedVisibility = getVisibility();
            foreach (Saxon.Hej.trans.ComponentTest test in getExplicitComponentTests()) {
                Saxon.Hej.pattern.QNameTest nameTest = test.getQNameTest();
                if ((nameTest is Saxon.Hej.pattern.NameTest)) {
                    Saxon.Hej.om.StructuredQName qName = ((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName();
                    int kind = test.getComponentKind();
                    Saxon.Hej.trans.SymbolicName sName = ((kind == Saxon.Hej.om.StandardNames.XSL_FUNCTION) ? new Saxon.Hej.trans.SymbolicName.F(((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName(), test.getArity()) : new Saxon.Hej.trans.SymbolicName(kind, ((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName()));
                    bool found = false;
                    switch (kind) {
                      case Saxon.Hej.om.StandardNames.XSL_TEMPLATE:
                        {
                            Saxon.Hej.expr.instruct.NamedTemplate template = psm.getNamedTemplate(qName);
                            found = (template != null);
                            if (found) {
                                Saxon.Hej.trans.Visibility declared = template.getDeclaredVisibility();
                                checkCompatibility(template.getSymbolicName(), declared, exposedVisibility);
                            }
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XSL_VARIABLE:
                        {
                            Saxon.Hej.style.SourceBinding binding = psm.getGlobalVariableBinding(qName);
                            if (((binding != null) && !(((binding.getSourceElement() is Saxon.Hej.style.XSLGlobalParam))))) {
                                found = true;
                            }
                            if (found) {
                                Saxon.Hej.expr.instruct.GlobalVariable var = ((Saxon.Hej.style.XSLGlobalVariable)(binding.getSourceElement())).getCompiledVariable();
                                Saxon.Hej.trans.Visibility declared = var.getDeclaredVisibility();
                                checkCompatibility(var.getSymbolicName(), declared, getVisibility());
                            }
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET:
                        {
                            System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> declarations = psm.getAttributeSetDeclarations(qName);
                            found = ((declarations != null) && !(declarations.Count==0));
                            if (found) {
                                Saxon.Hej.trans.Visibility declared = declarations[0].getSourceElement().getDeclaredVisibility();
                                checkCompatibility(sName, declared, getVisibility());
                            }
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XSL_MODE:
                        Saxon.Hej.trans.Mode mode = psm.getRuleManager().obtainMode(qName, false);
                        found = (mode != null);
                        if (found) {
                            checkCompatibility(sName, mode.getDeclaredVisibility(), getVisibility());
                        }
                        if ((getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                            compileError("The visibility of a mode cannot be abstract");
                        }
                        break ;
                      case Saxon.Hej.om.StandardNames.XSL_FUNCTION:
                        Saxon.Hej.style.StylesheetPackage pack = psm.getStylesheetPackage();
                        if ((test.getArity() == -1)) {
                            for (int i = 0; (i<=pack.getMaxFunctionArity()); i++) {
                                sName = new Saxon.Hej.trans.SymbolicName.F(((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName(), i);
                                Saxon.Hej.expr.Component fn = pack.getComponent(sName);
                                if ((fn != null)) {
                                    found = true;
                                    Saxon.Hej.expr.instruct.UserFunction userFunction = (Saxon.Hej.expr.instruct.UserFunction)(fn.getActor());
                                    checkCompatibility(sName, userFunction.getDeclaredVisibility(), getVisibility());
                                }
                            }
                        } else {
                            Saxon.Hej.expr.Component fn = pack.getComponent(sName);
                            found = (fn != null);
                            if (found) {
                                Saxon.Hej.expr.instruct.UserFunction userFunction = (Saxon.Hej.expr.instruct.UserFunction)(fn.getActor());
                                checkCompatibility(sName, userFunction.getDeclaredVisibility(), getVisibility());
                            }
                        }
                        break ;
                    }
                    if (!(found)) {
                        compileError((("No "+sName.ToString())+" exists in the containing package"), "XTSE3020");
                    }
                }
            }
        }
    
    }
}