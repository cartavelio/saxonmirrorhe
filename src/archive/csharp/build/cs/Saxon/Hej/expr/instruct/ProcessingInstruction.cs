using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ProcessingInstruction : Saxon.Hej.expr.instruct.SimpleNodeConstructor {
        private readonly Saxon.Hej.expr.Operand nameOp;
        public ProcessingInstruction(Saxon.Hej.expr.Expression name)  {
            nameOp = new Saxon.Hej.expr.Operand(this, name, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getNameExp() {
            return nameOp.getChildExpression();
        }
        public void setNameExp(Saxon.Hej.expr.Expression nameExp) {
            nameOp.setChildExpression(nameExp);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(selectOp, nameOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_PROCESSING_INSTRUCTION;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ProcessingInstruction exp = new Saxon.Hej.expr.instruct.ProcessingInstruction(getNameExp().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.setSelect(getSelect().copy(rebindings));
            return exp;
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            nameOp.typeCheck(visitor, contextItemType);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "processing-instruction/name", 0);
            setNameExp(visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(getNameExp(), Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, role, visitor));
            Saxon.Hej.expr.Expression nameExp = getNameExp();
            adoptChildExpression(nameExp);
            if (((nameExp is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(nameExp)).getGroundedValue() is Saxon.Hej.value.AtomicValue))) {
                Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(nameExp)).getGroundedValue());
                checkName(val, env.makeEarlyEvaluationContext());
            }
            if ((getSelect() is Saxon.Hej.expr.StringLiteral)) {
                Saxon.Hej.str.UnicodeString s = ((Saxon.Hej.expr.StringLiteral)(getSelect())).getGroundedValue().getUnicodeStringValue();
                Saxon.Hej.str.UnicodeString s2 = checkContent(s, env.makeEarlyEvaluationContext());
                if (!(s2.Equals(s))) {
                    setSelect(new Saxon.Hej.expr.StringLiteral(s2));
                }
            }
        }
        public override int getDependencies() {
            return (getNameExp().getDependencies()|base.getDependencies());
        }
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            string expandedName = evaluateName(context);
            Saxon.Hej.str.UnicodeString data = checkContent(value, context);
            output.processingInstruction(expandedName, data, getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
        }
        protected internal override Saxon.Hej.str.UnicodeString checkContent(Saxon.Hej.str.UnicodeString data, Saxon.Hej.expr.XPathContext context) {
            if (isXSLT()) {
                return checkContentXSLT(data);
            } else {
                try {
                    return checkContentXQuery(data);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setXPathContext(context);
                    err.setLocation(getLocation());
                    throw;
                }
            }
        }
        public static Saxon.Hej.str.UnicodeString checkContentXSLT(Saxon.Hej.str.UnicodeString data) {
            long hh;
            while (((hh = data.indexOf(PI_TERMINATOR, 0))>=0)) {
                data = data.substring(0, (hh+1)).concat(Saxon.Hej.str.StringConstants.SINGLE_SPACE.concat(data.substring((hh+1))));
            }
            return Saxon.Hej.value.Whitespace.removeLeadingWhitespace(data);
        }
        private readonly static Saxon.Hej.str.UnicodeString PI_TERMINATOR = new Saxon.Hej.str.Twine8(Saxon.Hej.str.StringConstants.PI_END);
        public static Saxon.Hej.str.UnicodeString checkContentXQuery(Saxon.Hej.str.UnicodeString data) {
            if ((data.indexOf(PI_TERMINATOR, 0)>=0)) {
                throw new Saxon.Hej.trans.XPathException("Invalid characters (?>) in processing instruction", "XQDY0026");
            }
            return Saxon.Hej.value.Whitespace.removeLeadingWhitespace(data);
        }
        public override Saxon.Hej.om.NodeName evaluateNodeName(Saxon.Hej.expr.XPathContext context) {
            string expandedName = evaluateName(context);
            return new Saxon.Hej.om.NoNamespaceName(expandedName);
        }
        private string evaluateName(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(getNameExp().evaluateItem(context));
            if (((av is Saxon.Hej.value.StringValue) && !(((av is Saxon.Hej.value.AnyURIValue))))) {
                return checkName(av, context);
            } else {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Processing instruction name is not a string");
                e.setXPathContext(context);
                e.setErrorCode("XPTY0004");
                throw dynamicError(getLocation(), e, context);
            }
        }
        private string checkName(Saxon.Hej.value.AtomicValue name, Saxon.Hej.expr.XPathContext context) {
            if (((name is Saxon.Hej.value.StringValue) && !(((name is Saxon.Hej.value.AnyURIValue))))) {
                string expandedName = Saxon.Hej.value.Whitespace.trim(name.getStringValue());
                if (!(Saxon.Hej.om.NameChecker.isValidNCName(expandedName))) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Processing instruction name "+Saxon.Hej.trans.Err.wrap(expandedName))+" is not a valid NCName"));
                    e.setXPathContext(context);
                    e.setErrorCode((isXSLT() ? "XTDE0890" : "XQDY0041"));
                    throw dynamicError(getLocation(), e, context);
                }
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(expandedName, "xml")) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Processing instructions cannot be named 'xml' in any combination of upper/lower case");
                    e.setXPathContext(context);
                    e.setErrorCode((isXSLT() ? "XTDE0890" : "XQDY0064"));
                    throw dynamicError(getLocation(), e, context);
                }
                return expandedName;
            } else {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Processing instruction name "+Saxon.Hej.trans.Err.wrap(name.getUnicodeStringValue()))+" is not of type xs:string or xs:untypedAtomic"));
                e.setXPathContext(context);
                e.setErrorCode("XPTY0004");
                e.setIsTypeError(true);
                throw dynamicError(getLocation(), e, context);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("procInst", this);
            string flags = "";
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            @out.setChildRole("name");
            getNameExp().export(@out);
            @out.setChildRole("select");
            getSelect().export(@out);
            @out.endElement();
        }
    
    }
}