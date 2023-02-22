using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class CastingExpression : Saxon.Hej.expr.UnaryExpression {
        private Saxon.Hej.type.AtomicType targetType;
        private readonly Saxon.Hej.type.AtomicType targetPrimitiveType;
        private bool allowEmpty = false;
        protected internal Saxon.Hej.type.Converter converter;
        private bool operandIsStringLiteral = false;
        public CastingExpression(Saxon.Hej.expr.Expression source, Saxon.Hej.type.AtomicType target, bool allowEmpty) : base (source) {
            this.allowEmpty = allowEmpty;
            targetType = target;
            targetPrimitiveType = (Saxon.Hej.type.AtomicType)(target.getPrimitiveItemType());
        }
        public Saxon.Hej.type.AtomicType getTargetPrimitiveType() {
            return targetPrimitiveType;
        }
        public void setTargetType(Saxon.Hej.type.AtomicType type) {
            targetType = type;
        }
        public Saxon.Hej.type.AtomicType getTargetType() {
            return targetType;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public void setAllowEmpty(bool allow) {
            allowEmpty = allow;
        }
        public bool allowsEmpty() {
            return allowEmpty;
        }
        public void setOperandIsStringLiteral(bool option) {
            operandIsStringLiteral = option;
        }
        public bool isOperandIsStringLiteral() {
            return operandIsStringLiteral;
        }
        public Saxon.Hej.type.Converter getConverter() {
            return converter;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return getRetainedStaticContext();
        }
        public override int getIntrinsicDependencies() {
            return (getTargetType().isNamespaceSensitive() ? Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT : 0);
        }
        public override Saxon.Hej.expr.Expression simplify() {
            if ((targetType is Saxon.Hej.type.BuiltInAtomicType)) {
                string s = Saxon.Hej.expr.parser.XPathParser.whyDisallowedType(getPackageData(), (Saxon.Hej.type.BuiltInAtomicType)(targetType));
                if ((s != null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(s, "XPST0080", this.getLocation());
                    err.setIsStaticError(true);
                    throw err;
                }
            }
            setBaseExpression(getBaseExpression().simplify());
            return this;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        protected internal void export(Saxon.Hej.trace.ExpressionPresenter @out, string elemName) {
            @out.startElement(elemName, this);
            int card = (allowsEmpty() ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE : Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(getTargetType(), card);
            @out.emitAttribute("flags", ("a"+((allowsEmpty() ? "e" : ""))));
            @out.emitAttribute("as", st.toAlphaCode());
            getBaseExpression().export(@out);
            @out.endElement();
        }
    
    }
}