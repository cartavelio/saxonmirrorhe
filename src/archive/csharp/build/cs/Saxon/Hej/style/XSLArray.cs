using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLArray : Saxon.Hej.style.XSLSequence {
        private bool composite;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    setSelectExpression(makeExpression(value, att));
                } else {
                    if (f.Equals("composite")) {
                        composite = processBooleanAttribute("composite", value);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (getURI().Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                requireXslt40(null);
            } else {
                if (!(getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                    compileError("saxon:array requires a Saxon-PE or -EE license");
                }
            }
            if ((getSelectExpression() == null)) {
                setSelectExpression(compileSequenceConstructor(exec, decl, false));
            }
            Saxon.Hej.expr.Expression functionCall;
            if (composite) {
                Saxon.Hej.functions.SystemFunction arrayOf = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction("of", 1);
                functionCall = arrayOf.makeFunctionCall(getSelectExpression());
            } else {
                Saxon.Hej.functions.SystemFunction arrayFromSeq = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction("_from-sequence", 1);
                functionCall = arrayFromSeq.makeFunctionCall(getSelectExpression());
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(getSelectExpression(), functionCall);
            return functionCall;
        }
    
    }
}