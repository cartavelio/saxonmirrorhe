using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.trans {
    public class XSLArray : Saxon.Hej.style.XSLSequence {
        private Saxon.Hej.expr.Expression use;
        public override bool isInXsltNamespace() {
            return getNodeName().hasURI(Saxon.Hej.om.NamespaceUri.XSLT);
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "select":
                    setSelectExpression(makeExpression(value, att));
                    break ;
                  case "use":
                    use = makeExpression(value, att);
                    break ;
                  case "composite":
                    compileWarning("XSLT 4.0 spec change: replace composite='yes' with use='?value'", "SXWN0001");
                    bool composite = processBooleanAttribute("composite", value);
                    if (composite) {
                        use = makeExpression("?value", att);
                    }
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            use = typeCheck("use", use);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (isInXsltNamespace()) {
                requireXslt40Element();
            } else {
                if (!(getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                    compileError("saxon:array requires a Saxon-PE or -EE license");
                }
            }
            if ((getSelectExpression() == null)) {
                setSelectExpression(compileSequenceConstructor(exec, decl, false));
            }
            Saxon.Hej.expr.Expression functionCall;
            if (((use == null) || (use is Saxon.Hej.expr.ContextItemExpression))) {
                Saxon.Hej.functions.SystemFunction build = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(40).makeFunction("build", 1);
                functionCall = build.makeFunctionCall(getSelectExpression());
            } else {
                Saxon.Hej.functions.SystemFunction mapEntry = Saxon.Hej.ma.map.MapFunctionSet.getInstance(40).makeFunction("entry", 2);
                Saxon.Hej.expr.Expression mapEntryCall = mapEntry.makeFunctionCall(new Saxon.Hej.expr.StringLiteral("value"), use);
                Saxon.Hej.expr.instruct.ForEach bangExpression = new Saxon.Hej.expr.instruct.ForEach(getSelectExpression(), mapEntryCall);
                Saxon.Hej.functions.SystemFunction arrayOf = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(40).makeFunction("of-members", 1);
                functionCall = arrayOf.makeFunctionCall(bangExpression);
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(getSelectExpression(), functionCall);
            return functionCall;
        }
    
    }
}