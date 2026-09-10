using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ForMemberExpression : Saxon.Hej.expr.Assignation {
        public ForMemberExpression()  {
        }
        public override string getExpressionName() {
            return "forMember";
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getSequenceOp().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "'in' clause of for-member", 0);
            Saxon.Hej.expr.Expression operand = visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(getSequence(), Saxon.Hej.ma.arrays.ArrayItemType.SINGLE_ARRAY, role, visitor);
            setSequence(operand);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                return getAction();
            }
            getActionOp().typeCheck(visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getAction().getIntegerBounds();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ForMemberExpression forExp = new Saxon.Hej.expr.ForMemberExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, forExp);
            forExp.setRequiredType(requiredType);
            forExp.setVariableQName(variableName);
            forExp.setSequence(getSequence().copy(rebindings));
            Saxon.Hej.expr.Expression newAction = getAction().copy(rebindings);
            forExp.setAction(newAction);
            forExp.variableName = variableName;
            forExp.slotNumber = slotNumber;
            Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(newAction, this, forExp);
            return forExp;
        }
        public override bool isVacuousExpression() {
            return getAction().isVacuousExpression();
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|PROCESS_METHOD);
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getAction().checkPermittedContents(parentType, false);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            int slot = getLocalSlotNumber();
            Saxon.Hej.ma.arrays.ArrayItem theArray = ((Saxon.Hej.ma.arrays.ArrayItem)(getSequence().evaluateItem(context)));
            int length = theArray.arrayLength();
            Saxon.Hej.expr.AscendingRangeIterator @base = new Saxon.Hej.expr.AscendingRangeIterator(0, 1, (length-1));
            return Saxon.Hej.expr.MappingIterator.map(@base, index => {
                Saxon.Hej.om.Sequence member = theArray.get((int)(((Saxon.Hej.value.IntegerValue)(index)).longValue()));
                context.setLocalVariable(slot, member);
                return getAction().iterate(context);
            } );
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            int slot = getLocalSlotNumber();
            Saxon.Hej.ma.arrays.ArrayItem theArray = ((Saxon.Hej.ma.arrays.ArrayItem)(getSequence().evaluateItem(context)));
            foreach (Saxon.Hej.om.GroundedValue member in theArray.members()) {
                context.setLocalVariable(slot, member);
                getAction().process(output, context);
            }
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            int slot = getLocalSlotNumber();
            Saxon.Hej.ma.arrays.ArrayItem theArray = ((Saxon.Hej.ma.arrays.ArrayItem)(getSequence().evaluateItem(context)));
            foreach (Saxon.Hej.om.GroundedValue member in theArray.members()) {
                context.setLocalVariable(slot, member);
                getAction().evaluatePendingUpdates(context, pul);
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getAction().getStaticUType(contextItemType);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override string ToString() {
            return ((((("for member $"+getVariableEQName())+" in ")+(((getSequence() == null) ? "(...)" : getSequence().ToString())))+" return ")+(((getAction() == null) ? "(...)" : Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getAction()))));
        }
        public override string toShortString() {
            return ((((("for member $"+getVariableQName().getDisplayName())+" in ")+(((getSequence() == null) ? "(...)" : getSequence().toShortString())))+" return ")+(((getAction() == null) ? "(...)" : getAction().toShortString())));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("forMember", this);
            @out.emitAttribute("var", getVariableQName());
            @out.emitAttribute("slot", (""+getLocalSlotNumber()));
            @out.setChildRole("in");
            getSequence().export(@out);
            @out.setChildRole("return");
            getAction().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "ForMemberExpression";
        }
    
    }
}