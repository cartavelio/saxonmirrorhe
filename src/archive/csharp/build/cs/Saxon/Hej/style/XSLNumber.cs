using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLNumber : Saxon.Hej.style.StyleElement {
        private int level;
        private Saxon.Hej.pattern.Pattern count = null;
        private Saxon.Hej.pattern.Pattern from = null;
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.expr.Expression value = null;
        private Saxon.Hej.expr.Expression format = null;
        private Saxon.Hej.expr.Expression groupSize = null;
        private Saxon.Hej.expr.Expression groupSeparator = null;
        private Saxon.Hej.expr.Expression letterValue = null;
        private Saxon.Hej.expr.Expression lang = null;
        private Saxon.Hej.expr.Expression ordinal = null;
        private Saxon.Hej.expr.Expression startAt = null;
        private Saxon.Hej.expr.number.NumberFormatter formatter = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string valueAtt = null;
            string countAtt = null;
            string fromAtt = null;
            string levelAtt = null;
            string formatAtt = null;
            Saxon.Hej.om.AttributeInfo gsizeAtt = null;
            Saxon.Hej.om.AttributeInfo gsepAtt = null;
            string langAtt = null;
            string letterValueAtt = null;
            string ordinalAtt = null;
            string startAtAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string attValue = att.getValue();
                switch (f) {
                  case "select":
                    selectAtt = attValue;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "value":
                    valueAtt = attValue;
                    this.value = makeExpression(valueAtt, att);
                    break ;
                  case "count":
                    countAtt = attValue;
                    break ;
                  case "from":
                    fromAtt = attValue;
                    break ;
                  case "level":
                    levelAtt = Saxon.Hej.value.Whitespace.trim(attValue);
                    break ;
                  case "format":
                    formatAtt = attValue;
                    format = makeAttributeValueTemplate(formatAtt, att);
                    break ;
                  case "lang":
                    langAtt = attValue;
                    lang = makeAttributeValueTemplate(langAtt, att);
                    break ;
                  case "letter-value":
                    letterValueAtt = Saxon.Hej.value.Whitespace.trim(attValue);
                    letterValue = makeAttributeValueTemplate(letterValueAtt, att);
                    break ;
                  case "grouping-size":
                    gsizeAtt = att;
                    break ;
                  case "grouping-separator":
                    gsepAtt = att;
                    break ;
                  case "ordinal":
                    ordinalAtt = attValue;
                    ordinal = makeAttributeValueTemplate(ordinalAtt, att);
                    break ;
                  case "start-at":
                    startAtAtt = attValue;
                    startAt = makeAttributeValueTemplate(startAtAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((valueAtt != null)) {
                if ((selectAtt != null)) {
                    compileError("The select attribute and value attribute must not both be present", "XTSE0975");
                }
                if ((countAtt != null)) {
                    compileError("The count attribute and value attribute must not both be present", "XTSE0975");
                }
                if ((fromAtt != null)) {
                    compileError("The from attribute and value attribute must not both be present", "XTSE0975");
                }
                if ((levelAtt != null)) {
                    compileError("The level attribute and value attribute must not both be present", "XTSE0975");
                }
            }
            if ((countAtt != null)) {
                count = makePattern(countAtt, "count");
            }
            if ((fromAtt != null)) {
                from = makePattern(fromAtt, "from");
            }
            if ((levelAtt == null)) {
                level = Saxon.Hej.expr.instruct.NumberInstruction.SINGLE;
            } else {
                if (levelAtt.Equals("single")) {
                    level = Saxon.Hej.expr.instruct.NumberInstruction.SINGLE;
                } else {
                    if (levelAtt.Equals("multiple")) {
                        level = Saxon.Hej.expr.instruct.NumberInstruction.MULTI;
                    } else {
                        if (levelAtt.Equals("any")) {
                            level = Saxon.Hej.expr.instruct.NumberInstruction.ANY;
                        } else {
                            invalidAttribute("level", "single|any|multiple");
                        }
                    }
                }
            }
            if ((((level == Saxon.Hej.expr.instruct.NumberInstruction.SINGLE) && (from == null)) && (count == null))) {
                level = Saxon.Hej.expr.instruct.NumberInstruction.SIMPLE;
            }
            if ((formatAtt != null)) {
                if ((format is Saxon.Hej.expr.StringLiteral)) {
                    formatter = new Saxon.Hej.expr.number.NumberFormatter();
                    formatter.prepare(((Saxon.Hej.expr.StringLiteral)(format)).stringify());
                }
            } else {
                formatter = new Saxon.Hej.expr.number.NumberFormatter();
                formatter.prepare("1");
            }
            if (((gsepAtt != null) && (gsizeAtt != null))) {
                groupSize = makeAttributeValueTemplate(gsizeAtt.getValue(), gsizeAtt);
                groupSeparator = makeAttributeValueTemplate(gsepAtt.getValue(), gsepAtt);
            }
            if ((startAtAtt != null)) {
                if (((startAtAtt.IndexOf('{')<0) && !(System.Text.RegularExpressions.Regex.IsMatch(startAtAtt, "-?[0-9]+(\\s+-?[0-9]+)*")))) {
                    compileErrorInAttribute("Invalid format for start-at attribute", "XTSE0020", "start-at");
                }
            } else {
                startAt = new Saxon.Hej.expr.StringLiteral("1");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            select = typeCheck("select", select);
            value = typeCheck("value", value);
            format = typeCheck("format", format);
            groupSize = typeCheck("group-size", groupSize);
            groupSeparator = typeCheck("group-separator", groupSeparator);
            letterValue = typeCheck("letter-value", letterValue);
            ordinal = typeCheck("ordinal", ordinal);
            lang = typeCheck("lang", lang);
            from = typeCheck("from", from);
            count = typeCheck("count", count);
            startAt = typeCheck("start-at", startAt);
            string errorCode = "XTTE1000";
            if (((value == null) && (select == null))) {
                errorCode = "XTTE0990";
                Saxon.Hej.expr.ContextItemExpression implicitSelect = new Saxon.Hej.expr.ContextItemExpression();
                implicitSelect.setLocation(allocateLocation());
                implicitSelect.setErrorCodeForUndefinedContext(errorCode, false);
                select = implicitSelect;
            }
            if ((select != null)) {
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:number/select", 0);
                    role.setErrorCode(errorCode);
                    select = getConfiguration().getTypeChecker(false).staticTypeCheck(select, Saxon.Hej.value.SequenceType.SINGLE_NODE, role, makeExpressionVisitor());
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            bool valueSpecified = (value != null);
            if ((value == null)) {
                value = new Saxon.Hej.expr.instruct.NumberInstruction(select, level, count, from);
                value.setLocation(allocateLocation());
            }
            Saxon.Hej.expr.NumberSequenceFormatter numFormatter = new Saxon.Hej.expr.NumberSequenceFormatter(value, format, groupSize, groupSeparator, letterValue, ordinal, startAt, lang, formatter, (xPath10ModeIsEnabled() && valueSpecified));
            numFormatter.setLocation(allocateLocation());
            Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(numFormatter, false, false);
            inst.setLocation(allocateLocation());
            inst.setIsNumberingInstruction();
            return inst;
        }
    
    }
}