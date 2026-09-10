using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMap : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.expr.Expression onDuplicates = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal Saxon.Hej.type.ItemType getReturnedItemType() {
            return Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (attName.getLocalPart().Equals("on-duplicates")) {
                    if (attName.getNamespaceUri().isEmpty()) {
                        requireXslt40("on-duplicates");
                        onDuplicates = makeExpression(value, att);
                    } else {
                        if (attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON)) {
                            if (getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                                onDuplicates = makeExpression(value, att);
                            } else {
                                issueWarning("saxon:on-duplicates ignored - requires Saxon-PE license", Saxon.Hej.trans.SaxonErrorCode.SXWN9013);
                            }
                        }
                    }
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            select = compileSequenceConstructor(exec, decl, false);
            select = select.simplify();
            Saxon.Hej.expr.parser.TypeChecker tc = getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "xsl:map sequence constructor", 0, "XTTE3375");
            select = tc.staticTypeCheck(select, Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE), role, makeExpressionVisitor());
            Saxon.Hej.expr.Expression optionsExp;
            if ((onDuplicates != null)) {
                optionsExp = Saxon.Hej.ma.map.MapFunctionSet.getInstance(31).makeFunction("entry", 2).makeFunctionCall(Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.SAXON, "on-duplicates")), onDuplicates);
            } else {
                Saxon.Impl.DataModel.ImmMap options = new Saxon.Impl.DataModel.ImmMap();
                options.initialPut(Saxon.Hej.value.StringValue.bmp("duplicates"), Saxon.Hej.value.StringValue.bmp("reject"));
                options.initialPut(new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.SAXON, "duplicates-error-code"), Saxon.Hej.value.StringValue.bmp("XTDE3365"));
                optionsExp = Saxon.Hej.expr.Literal.makeLiteral(options, select);
            }
            Saxon.Hej.expr.Expression exp = Saxon.Hej.ma.map.MapFunctionSet.getInstance(31).makeFunction("merge", 2).makeFunctionCall(select, optionsExp);
            if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) {
                exp = new Saxon.Hej.expr.instruct.SequenceInstr(exp);
            }
            return exp;
        }
    
    }
}