using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonAssign : Saxon.Hej.style.XSLGeneralVariable {
        private Saxon.Eej.xsltextn.instruct.Assign instruction;
        private readonly static Saxon.Hej.style.SourceBinding.BindingProperty allowedAttributes = (Saxon.Hej.style.SourceBinding.BindingProperty.SELECT);
        public SaxonAssign()  {
        }
        protected internal override void prepareAttributes() {
            sourceBinding.prepareAttributes(allowedAttributes);
        }
        public override bool isInstruction() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            instruction = new Saxon.Eej.xsltextn.instruct.Assign(makeExpression("()", null));
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:assign", getPackageData().getLocalLicenseId());
            if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING)) {
                compileWarning(("saxon:assign should not be used in a configuration "+"that allows multi-threading: see FeatureKeys.ALLOW_MULTITHREADING"), Saxon.Hej.trans.SaxonErrorCode.SXWN9016);
            }
            Saxon.Hej.om.StructuredQName name = getSourceBinding().getVariableQName();
            Saxon.Hej.style.SourceBinding globalVariable;
            try {
                globalVariable = bindVariable(name);
                if ((globalVariable == null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Variable "+name.getDisplayName())+" has not been declared"));
                    err.setErrorCode("XPST0008");
                    err.setIsStaticError(true);
                    throw err;
                }
                globalVariable.registerReference(instruction);
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err.Message);
                return ;
            }
            if (!(globalVariable.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.GLOBAL))) {
                compileError((("Variable "+name.getDisplayName())+" is not global"));
                return ;
            }
            if (!(globalVariable.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE))) {
                compileError((("Variable "+name.getDisplayName())+" is not marked as assignable"));
            }
            Saxon.Hej.style.XSLGlobalVariable sourceVar = (Saxon.Hej.style.XSLGlobalVariable)(globalVariable.getSourceElement());
            Saxon.Hej.expr.instruct.GlobalVariable compiledVar = sourceVar.getCompiledVariable();
            if ((compiledVar != null)) {
                instruction.fixup(sourceVar.getCompiledVariable());
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((sourceBinding.getSelectExpression() != null)) {
                instruction.setSelectExpr(sourceBinding.getSelectExpression());
            } else {
                instruction.setSelectExpr(compileSequenceConstructor(exec, decl, false));
            }
            return instruction;
        }
    
    }
}