using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class SimpleNodeConstructor : Saxon.Hej.expr.instruct.Instruction {
        protected internal Saxon.Hej.expr.Operand selectOp;
        public SimpleNodeConstructor()  {
            Saxon.Hej.expr.Expression select = Saxon.Hej.expr.Literal.makeEmptySequence();
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return selectOp;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override bool alwaysCreatesNewNodes() {
            return true;
        }
        protected internal override int computeCardinality() {
            return getSelect().getCardinality();
        }
        protected internal override int computeSpecialProperties() {
            return (base.computeSpecialProperties()|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public abstract void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType);
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            localTypeCheck(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if ((getSelect() is Saxon.Hej.expr.instruct.ValueOf)) {
                Saxon.Hej.expr.Expression valSelect = ((Saxon.Hej.expr.instruct.ValueOf)(getSelect())).getSelect();
                if ((th.isSubType(valSelect.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING) && !(Saxon.Hej.value.Cardinality.allowsMany(valSelect.getCardinality())))) {
                    setSelect(valSelect);
                }
            }
            if (getSelect().isCallOn(typeof(Saxon.Hej.functions.String_1))) {
                Saxon.Hej.expr.SystemFunctionCall fn = (Saxon.Hej.expr.SystemFunctionCall)(getSelect());
                Saxon.Hej.expr.Expression arg = fn.getArg(0);
                if (((arg.getItemType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && !(Saxon.Hej.value.Cardinality.allowsMany(arg.getCardinality())))) {
                    setSelect(arg);
                }
            } else {
                if (((getSelect() is Saxon.Hej.expr.CastExpression) && (((Saxon.Hej.expr.CastExpression)(getSelect())).getTargetType() == Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                    Saxon.Hej.expr.Expression arg = ((Saxon.Hej.expr.CastExpression)(getSelect())).getBaseExpression();
                    if (((arg.getItemType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && !(Saxon.Hej.value.Cardinality.allowsMany(arg.getCardinality())))) {
                        setSelect(arg);
                    }
                }
            }
            adoptChildExpression(getSelect());
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            optimizeChildren(visitor, contextItemType);
            if (getSelect().isCallOn(typeof(Saxon.Hej.functions.String_1))) {
                Saxon.Hej.expr.SystemFunctionCall sf = (Saxon.Hej.expr.SystemFunctionCall)(getSelect());
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                if ((th.isSubType(sf.getArg(0).getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING) && !(Saxon.Hej.value.Cardinality.allowsMany(sf.getArg(0).getCardinality())))) {
                    setSelect(sf.getArg(0));
                }
            }
            return this;
        }
        public override string getStreamerName() {
            return "SimpleNodeConstructor";
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString value = getSelect().evaluateAsString(context);
            try {
                processValue(value, output, context);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
            return null;
        }
        public abstract void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context);
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item contentItem = getSelect().evaluateItem(context);
            Saxon.Hej.str.UnicodeString content;
            if ((contentItem == null)) {
                content = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                content = contentItem.getUnicodeStringValue();
                content = checkContent(content, context);
            }
            Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            o.setNodeKind((short)(getItemType().getPrimitiveType()));
            o.setStringValue(content);
            o.setNodeName(evaluateNodeName(context));
            return o;
        }
        protected internal virtual Saxon.Hej.str.UnicodeString checkContent(Saxon.Hej.str.UnicodeString data, Saxon.Hej.expr.XPathContext context) {
            return data;
        }
        public virtual Saxon.Hej.om.NodeName evaluateNodeName(Saxon.Hej.expr.XPathContext context) {
            return null;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(evaluateItem(context));
        }
        public bool isLocal() {
            return Saxon.Hej.expr.parser.ExpressionTool.isLocalConstructor(this);
        }
    
    }
}