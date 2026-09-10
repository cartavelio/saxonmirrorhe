using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLSortOrMergeKey : Saxon.Hej.style.StyleElement {
        protected internal Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDefinition;
        protected internal Saxon.Hej.expr.Expression select;
        protected internal Saxon.Hej.expr.Expression order;
        protected internal Saxon.Hej.expr.Expression dataType = null;
        protected internal Saxon.Hej.expr.Expression caseOrder;
        protected internal Saxon.Hej.expr.Expression lang;
        protected internal Saxon.Hej.expr.Expression collationName;
        protected internal Saxon.Hej.expr.Expression stable;
        protected internal bool useDefaultCollation = true;
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal virtual string getErrorCode() {
            return "XTSE1015";
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (((select != null) && hasChildNodes())) {
                compileError((("An "+getDisplayName())+" element with a select attribute must be empty"), getErrorCode());
            }
            if (((select == null) && !(hasChildNodes()))) {
                select = new Saxon.Hej.expr.ContextItemExpression();
                select.setRetainedStaticContext(getStaticContext().makeRetainedStaticContext());
            }
            if (useDefaultCollation) {
                collationName = new Saxon.Hej.expr.StringLiteral(getDefaultCollationName());
            }
            Saxon.Hej.lib.StringCollator stringCollator = null;
            if ((collationName is Saxon.Hej.expr.StringLiteral)) {
                string collationString = ((Saxon.Hej.expr.StringLiteral)(collationName)).stringify();
                try {
                    System.Uri collationURI = new System.Uri(collationString, System.UriKind.RelativeOrAbsolute);
                    if (!(collationURI.IsAbsoluteUri)) {
                        System.Uri @base = new System.Uri(getBaseURI(), System.UriKind.RelativeOrAbsolute);
                        collationURI = new System.Uri(@base, collationURI);
                        collationString = collationURI.ToString();
                    }
                } catch (System.UriFormatException err) {
                    compileError((("Collation name '"+collationString)+"' is not a valid URI"));
                    collationString = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
                }
                stringCollator = findCollation(collationString, getBaseURI());
                if ((stringCollator == null)) {
                    compileError((("Collation "+collationString)+" has not been defined"), "XTDE1035");
                    stringCollator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
                }
            }
            select = typeCheck("select", select);
            order = typeCheck("order", order);
            caseOrder = typeCheck("case-order", caseOrder);
            lang = typeCheck("lang", lang);
            dataType = typeCheck("data-type", dataType);
            collationName = typeCheck("collation", collationName);
            if ((select != null)) {
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, (getDisplayName()+"//select"), 0);
                    select = getConfiguration().getTypeChecker(false).staticTypeCheck(select, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, role, makeExpressionVisitor());
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            }
            sortKeyDefinition = new Saxon.Hej.expr.sort.SortKeyDefinition();
            sortKeyDefinition.setOrder(order);
            sortKeyDefinition.setCaseOrder(caseOrder);
            sortKeyDefinition.setLanguage(lang);
            sortKeyDefinition.setSortKey(select, true);
            sortKeyDefinition.setDataTypeExpression(dataType);
            sortKeyDefinition.setCollationNameExpression(collationName);
            sortKeyDefinition.setCollation(stringCollator);
            sortKeyDefinition.setBaseURI(getBaseURI());
            sortKeyDefinition.setStable(stable);
            sortKeyDefinition.setBackwardsCompatible(xPath10ModeIsEnabled());
        }
        protected internal virtual Saxon.Hej.expr.Expression getStable() {
            return stable;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string orderAtt = null;
            string dataTypeAtt = null;
            string caseOrderAtt = null;
            string langAtt = null;
            string collationAtt = null;
            string stableAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "order":
                    orderAtt = Saxon.Hej.value.Whitespace.trim(value);
                    order = makeAttributeValueTemplate(orderAtt, att);
                    break ;
                  case "data-type":
                    dataTypeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    dataType = makeAttributeValueTemplate(dataTypeAtt, att);
                    break ;
                  case "case-order":
                    caseOrderAtt = Saxon.Hej.value.Whitespace.trim(value);
                    caseOrder = makeAttributeValueTemplate(caseOrderAtt, att);
                    break ;
                  case "lang":
                    langAtt = Saxon.Hej.value.Whitespace.trim(value);
                    lang = makeAttributeValueTemplate(langAtt, att);
                    break ;
                  case "collation":
                    collationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    collationName = makeAttributeValueTemplate(collationAtt, att);
                    break ;
                  case "stable":
                    stableAtt = Saxon.Hej.value.Whitespace.trim(value);
                    stable = makeAttributeValueTemplate(stableAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((orderAtt == null)) {
                order = new Saxon.Hej.expr.StringLiteral("ascending");
            } else {
                checkAttributeValue("order", orderAtt, true, new string[]{"ascending", "descending"});
            }
            if ((dataTypeAtt == null)) {
                dataType = null;
            }
            if ((caseOrderAtt == null)) {
                caseOrder = new Saxon.Hej.expr.StringLiteral("#default");
            } else {
                checkAttributeValue("case-order", caseOrderAtt, true, new string[]{"lower-first", "upper-first"});
                useDefaultCollation = false;
            }
            if (((langAtt == null) || langAtt.Equals(""))) {
                lang = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
            } else {
                useDefaultCollation = false;
                if ((lang is Saxon.Hej.expr.StringLiteral)) {
                    Saxon.Hej.str.UnicodeString s = ((Saxon.Hej.expr.StringLiteral)(lang)).getString();
                    if (!(s.isEmpty())) {
                        Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(s);
                        if ((vf != null)) {
                            compileError("The lang attribute must be a valid language code", "XTDE0030");
                            lang = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                        }
                    }
                }
            }
            if ((stableAtt == null)) {
                stable = null;
            } else {
                checkAttributeValue("stable", stableAtt, true, Saxon.Hej.style.StyleElement.YES_NO);
            }
            if ((collationAtt != null)) {
                useDefaultCollation = false;
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                Saxon.Hej.expr.Expression b = compileSequenceConstructor(exec, decl, true);
                if ((b == null)) {
                    b = Saxon.Hej.expr.Literal.makeEmptySequence();
                    b.setRetainedStaticContext(makeRetainedStaticContext());
                }
                try {
                    Saxon.Hej.expr.Expression atomizedSortKey = Saxon.Hej.expr.Atomizer.makeAtomizer(b, null);
                    atomizedSortKey = atomizedSortKey.simplify();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(b, atomizedSortKey);
                    sortKeyDefinition.setSortKey(atomizedSortKey, true);
                    select = atomizedSortKey;
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileError(e);
                }
            }
            sortKeyDefinition = (Saxon.Hej.expr.sort.SortKeyDefinition)(sortKeyDefinition.simplify());
            return null;
        }
        public Saxon.Hej.expr.sort.SortKeyDefinition getSortKeyDefinition() {
            return sortKeyDefinition;
        }
    
    }
}