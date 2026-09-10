using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLForEachGroup : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.expr.Expression groupBy = null;
        private Saxon.Hej.expr.Expression groupAdjacent = null;
        private Saxon.Hej.expr.Expression breakWhen = null;
        private Saxon.Hej.pattern.Pattern starting = null;
        private Saxon.Hej.pattern.Pattern ending = null;
        private Saxon.Hej.expr.Expression collationName;
        private bool composite = false;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return (child is Saxon.Hej.style.XSLSort);
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string groupByAtt = null;
            string groupAdjacentAtt = null;
            string startingAtt = null;
            string endingAtt = null;
            string breakWhenAtt = null;
            string collationAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "select":
                    select = makeExpression(value, att);
                    break ;
                  case "array":
                    requireXslt40("array");
                    select = arrayToSequence(makeExpression(value, att));
                    break ;
                  case "map":
                    requireXslt40("map");
                    select = mapToSequence(makeExpression(value, att));
                    break ;
                  case "group-by":
                    groupByAtt = value;
                    groupBy = makeExpression(groupByAtt, att);
                    break ;
                  case "group-adjacent":
                    groupAdjacentAtt = value;
                    groupAdjacent = makeExpression(groupAdjacentAtt, att);
                    break ;
                  case "group-starting-with":
                    startingAtt = value;
                    break ;
                  case "group-ending-with":
                    endingAtt = value;
                    break ;
                  case "break-when":
                    requireXslt40("break-when");
                    breakWhenAtt = ((("function($group as item()*, $next as item()) as Q{http://www.w3.org/2001/XMLSchema}boolean "+"{ Q{http://www.w3.org/2005/xpath-functions}boolean(")+value)+") }");
                    breakWhen = makeExpression(breakWhenAtt, att);
                    break ;
                  case "collation":
                    collationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    collationName = makeAttributeValueTemplate(collationAtt, att);
                    break ;
                  case "composite":
                    composite = processBooleanAttribute("composite", value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((select == null)) {
                select = Saxon.Hej.expr.Literal.makeEmptySequence();
                if ((getEffectiveVersion()>=40)) {
                    compileError("One of the attributes select|array|map must be present");
                } else {
                    reportAbsence("select");
                }
            }
            int c = (((((((groupByAtt == null) ? 0 : 1))+(((groupAdjacentAtt == null) ? 0 : 1)))+(((startingAtt == null) ? 0 : 1)))+(((endingAtt == null) ? 0 : 1)))+(((breakWhenAtt == null) ? 0 : 1)));
            if ((c != 1)) {
                compileError(("Exactly one of the attributes group-by, group-adjacent, group-starting-with, "+"and group-ending-with must be specified"), "XTSE1080");
            }
            if ((startingAtt != null)) {
                starting = makePattern(startingAtt, "group-starting-with");
            }
            if ((endingAtt != null)) {
                ending = makePattern(endingAtt, "group-ending-with");
            }
            if ((collationAtt != null)) {
                if (((groupBy == null) && (groupAdjacent == null))) {
                    compileError("A collation may be specified only if group-by or group-adjacent is specified", "XTSE1090");
                } else {
                    if ((collationName is Saxon.Hej.expr.StringLiteral)) {
                        string collation = ((Saxon.Hej.expr.StringLiteral)(collationName)).stringify();
                        System.Uri collationURI;
                        try {
                            collationURI = new System.Uri(collation, System.UriKind.RelativeOrAbsolute);
                            if (!(collationURI.IsAbsoluteUri)) {
                                System.Uri @base = new System.Uri(getBaseURI(), System.UriKind.RelativeOrAbsolute);
                                collationURI = new System.Uri(@base, collationURI);
                                collationName = new Saxon.Hej.expr.StringLiteral(collationURI.ToString());
                            }
                        } catch (System.UriFormatException err) {
                            compileError((("Collation name '"+collationName)+"' is not a valid URI"), "XTDE1110");
                            collationName = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI);
                        }
                    }
                }
            } else {
                string defaultCollation = getDefaultCollationName();
                if ((defaultCollation != null)) {
                    collationName = new Saxon.Hej.expr.StringLiteral(defaultCollation);
                }
            }
            if ((composite && (((starting != null) || (ending != null))))) {
                compileError(("The composite attribute cannot be used with "+(((starting == null) ? "grouping-ending-with" : "group-starting-with"))), "XTSE1090");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkSortComesFirst(false);
            Saxon.Hej.expr.parser.TypeChecker tc = getConfiguration().getTypeChecker(false);
            select = typeCheck("select", select);
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
            if ((groupBy != null)) {
                groupBy = typeCheck("group-by", groupBy);
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:for-each-group/group-by", 0);
                    groupBy = tc.staticTypeCheck(groupBy, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, role, visitor);
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            } else {
                if ((groupAdjacent != null)) {
                    groupAdjacent = typeCheck("group-adjacent", groupAdjacent);
                    try {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:for-each-group/group-adjacent", 0);
                        role.setErrorCode("XTTE1100");
                        groupAdjacent = tc.staticTypeCheck(groupAdjacent, (composite ? Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE : Saxon.Hej.value.SequenceType.SINGLE_ATOMIC), role, visitor);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        compileError(err);
                    }
                } else {
                    if ((breakWhen != null)) {
                        breakWhen = typeCheck("break-when", breakWhen);
                        try {
                            Saxon.Hej.type.SpecificFunctionType breakWhenType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE, Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
                            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:for-each-group/break-when", 0);
                            role.setErrorCode("XTTE1100");
                            breakWhen = tc.staticTypeCheck(breakWhen, Saxon.Hej.value.SequenceType.makeSequenceType(breakWhenType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), role, visitor);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            compileError(err);
                        }
                    }
                }
            }
            starting = typeCheck("group-starting-with", starting);
            ending = typeCheck("group-ending-with", ending);
            if (((((starting != null) || (ending != null))) && (visitor.getStaticContext().getXPathVersion()<30))) {
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:for-each-group/select", 0);
                    role.setErrorCode("XTTE1120");
                    select = tc.staticTypeCheck(select, Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role, visitor);
                } catch (Saxon.Hej.trans.XPathException err) {
                    string prefix = ((starting != null) ? "With group-starting-with attribute: " : "With group-ending-with attribute: ");
                    compileError((prefix+err.Message), err.getErrorCodeQName());
                }
            }
            if (!(hasChildNodes())) {
                compileWarning("An empty xsl:for-each-group instruction has no effect", Saxon.Hej.trans.SaxonErrorCode.SXWN9009);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.lib.StringCollator collator = null;
            if ((collationName is Saxon.Hej.expr.StringLiteral)) {
                string uri = ((Saxon.Hej.expr.StringLiteral)(collationName)).stringify();
                collator = findCollation(uri, getBaseURI());
                if ((collator == null)) {
                    compileError((("The collation name '"+collationName)+"' has not been defined"), "XTDE1110");
                }
            }
            byte algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY;
            Saxon.Hej.expr.Expression key = null;
            if ((groupBy != null)) {
                algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY;
                key = groupBy;
            } else {
                if ((groupAdjacent != null)) {
                    algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ADJACENT;
                    key = groupAdjacent;
                } else {
                    if ((starting != null)) {
                        algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_STARTING;
                        key = starting;
                    } else {
                        if ((ending != null)) {
                            algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ENDING;
                            key = ending;
                        } else {
                            if ((breakWhen != null)) {
                                algorithm = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BREAKING_WHEN;
                                key = breakWhen;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(compilation, decl, true);
            if ((action == null)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            try {
                Saxon.Hej.expr.instruct.ForEachGroup instr = new Saxon.Hej.expr.instruct.ForEachGroup(select, action.simplify(), algorithm, key, collator, collationName, makeSortKeys(compilation, decl));
                instr.setIsInFork((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getFingerprint() == Saxon.Hej.om.StandardNames.XSL_FORK));
                instr.setComposite(composite);
                return instr;
            } catch (Saxon.Hej.trans.XPathException e) {
                compileError(e);
                return null;
            }
        }
    
    }
}