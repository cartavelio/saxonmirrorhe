using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLItemType : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.om.StructuredQName itemTypeName;
        private bool resolved = false;
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            if ((itemTypeName != null)) {
                return ;
            }
            string typeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "name":
                    itemTypeName = makeQName(value, null, "name");
                    break ;
                  case "as":
                    typeAtt = value;
                    break ;
                  case "visibility":
                    if (!(value.Equals("private"))) {
                        compileErrorInAttribute("Not implemented", "XTSE0010", "visibility");
                    }
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((itemTypeName == null)) {
                reportAbsence("name");
            }
            if ((typeAtt == null)) {
                reportAbsence("as");
            }
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            if ((itemTypeName == null)) {
                prepareAttributes();
            }
            return itemTypeName;
        }
        public void indexTypeAlias(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            prepareAttributes();
            top.getTypeAliasManager().processDeclaration(decl);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            checkTopLevel("XTSE0010", false);
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:item-type", getPackageData().getLocalLicenseId());
        }
        public Saxon.Hej.type.ItemType tryToResolve() {
            Saxon.Hej.expr.StaticContext env = new Saxon.Hej.style.XSLItemType.TypeAliasContext(this);
            resolved = true;
            Saxon.Hej.type.ItemType type = makeItemType(env);
            return (resolved ? type : null);
        }
        private void markUnresolved() {
            resolved = false;
        }
        private Saxon.Hej.type.ItemType makeItemType(Saxon.Hej.expr.StaticContext env) {
            try {
                Saxon.Hej.expr.parser.XPathParser parser = getConfiguration().newExpressionParser("XP", false);
                Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver()).withAcceptEQName(true).withErrorOnBadSyntax("XPST0003").withErrorOnUnresolvedPrefix("XPST0081");
                parser.setQNameParser(qp);
                string typeAtt = getAttributeValue("as");
                if ((typeAtt == null)) {
                    reportAbsence("as");
                    typeAtt = "item()";
                }
                Saxon.Hej.value.SequenceType st = parser.parseExtendedSequenceType(typeAtt, env);
                if ((st.getCardinality() != Saxon.Hej.expr.StaticProperty.ALLOWS_ONE)) {
                    compileError("Item type must not include an occurrence indicator");
                }
                return st.getPrimaryType();
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
        private class TypeAliasContext : Saxon.Hej.style.ExpressionContext {
            public TypeAliasContext(Saxon.Hej.style.XSLItemType declaration) : base (declaration, new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "as")) {
            }
            public override Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName) {
                Saxon.Hej.type.ItemType resolved = base.resolveTypeAlias(typeName);
                if ((resolved == null)) {
                    ((Saxon.Hej.style.XSLItemType)(getStyleElement())).markUnresolved();
                    return Saxon.Hej.type.AnyItemType.getInstance();
                } else {
                    return resolved;
                }
            }
        
        }
    
    }
}