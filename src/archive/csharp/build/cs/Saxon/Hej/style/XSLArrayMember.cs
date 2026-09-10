using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLArrayMember : Saxon.Hej.style.XSLSequence {
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (isInXsltNamespace()) {
                requireXslt40(null);
            } else {
                if (!(getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                    compileError("xsl:array requires a Saxon-PE or -EE license");
                }
            }
            if ((getSelectExpression() == null)) {
                setSelectExpression(compileSequenceConstructor(exec, decl, false));
            }
            Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeEmptySequence();
            Saxon.Hej.functions.SystemFunction arrayMem = /*134*/Saxon.Eej.functions.registry.XPath40FunctionSet.getInstance().makeFunction("parcel", 1);
            result = arrayMem.makeFunctionCall(getSelectExpression());
            return result;
        }
    
    }
}