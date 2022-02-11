using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class NamespaceConstructor : Saxon.Hej.expr.instruct.SimpleNodeConstructor {
        private readonly Saxon.Hej.expr.Operand nameOp;
        public NamespaceConstructor(Saxon.Hej.expr.Expression name)  {
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
            return Saxon.Hej.om.StandardNames.XSL_NAMESPACE;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.NAMESPACE;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            nameOp.typeCheck(visitor, contextItemType);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "namespace/name", 0);
            setNameExp(env.getConfiguration().getTypeChecker(false).staticTypeCheck(getNameExp(), Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC, role, visitor));
            adoptChildExpression(getNameExp());
            if ((getNameExp() is Saxon.Hej.expr.Literal)) {
                evaluatePrefix(env.makeEarlyEvaluationContext());
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.NamespaceConstructor exp = new Saxon.Hej.expr.instruct.NamespaceConstructor(getNameExp().copy(rebindings));
            exp.setSelect(getSelect().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.om.NodeName evaluateNodeName(Saxon.Hej.expr.XPathContext context) {
            string prefix = evaluatePrefix(context);
            return new Saxon.Hej.om.NoNamespaceName(prefix);
        }
        private string evaluatePrefix(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(getNameExp().evaluateItem(context));
            if ((value == null)) {
                return "";
            }
            if ((!(((value is Saxon.Hej.value.StringValue))) || (value is Saxon.Hej.value.AnyURIValue))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Namespace prefix is not an xs:string or xs:untypedAtomic", "XPTY0004", getLocation());
                err.setIsTypeError(true);
                throw dynamicError(getLocation(), err, context);
            }
            string prefix = Saxon.Hej.value.Whitespace.trim(value.getStringValue());
            if (!(((prefix.Length==0 || Saxon.Hej.om.NameChecker.isValidNCName(prefix))))) {
                string errorCode = (isXSLT() ? "XTDE0920" : "XQDY0074");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Namespace prefix is invalid: "+prefix), errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            if (prefix.Equals("xmlns")) {
                string errorCode = (isXSLT() ? "XTDE0920" : "XQDY0101");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Namespace prefix 'xmlns' is not allowed", errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            return prefix;
        }
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            string prefix = evaluatePrefix(context);
            string uri = value.ToString();
            checkPrefixAndUri(prefix, uri, context);
            output.@namespace(prefix, uri, Saxon.Hej.@event.ReceiverOption.REJECT_DUPLICATES);
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(base.evaluateItem(context));
            System.Diagnostics.Debug.Assert((node != null));
            string prefix = node.getLocalPart();
            string uri = node.getStringValue();
            checkPrefixAndUri(prefix, uri, context);
            return node;
        }
        private void checkPrefixAndUri(string prefix, string uri, Saxon.Hej.expr.XPathContext context) {
            if ((prefix.Equals("xml") != uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                string errorCode = (isXSLT() ? "XTDE0925" : "XQDY0101");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Namespace prefix 'xml' and namespace uri "+Saxon.Hej.lib.NamespaceConstant.XML)+" must only be used together"), errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            if (uri.Length==0) {
                string errorCode = (isXSLT() ? "XTDE0930" : "XQDY0101");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Namespace URI is an empty string", errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS)) {
                string errorCode = (isXSLT() ? "XTDE0905" : "XQDY0101");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("A namespace node cannot have the reserved namespace "+Saxon.Hej.lib.NamespaceConstant.XMLNS), errorCode, getLocation());
                throw dynamicError(getLocation(), err, context);
            }
            if (((context.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10) && !(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(uri)))) {
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("The string value of the constructed namespace node must be a valid URI", "XTDE0905", getLocation());
                throw dynamicError(getLocation(), de, context);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("namespace", this);
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