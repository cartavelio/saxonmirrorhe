using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class SuppliedParameterReference : Saxon.Hej.expr.Expression {
        internal int slotNumber;
        internal Saxon.Hej.value.SequenceType type;
        public SuppliedParameterReference(int slot)  {
            slotNumber = slot;
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        public void setSuppliedType(Saxon.Hej.value.SequenceType type) {
            this.type = type;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((type != null)) {
                return type.getPrimaryType();
            } else {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES;
        }
        protected internal override int computeCardinality() {
            if ((type != null)) {
                return type.getCardinality();
            } else {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.SuppliedParameterReference exp = new Saxon.Hej.expr.SuppliedParameterReference(slotNumber);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return (EVALUATE_METHOD|ITERATE_METHOD);
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext c) {
            if ((slotNumber == -1)) {
                return c.getStackFrame().popDynamicValue();
            }
            try {
                return c.evaluateLocalVariable(slotNumber);
            } catch (Saxon.Ejava.lang.AssertionError e) {
                new Saxon.Hej.lib.StandardDiagnostics().logStackTrace(c, c.getConfiguration().getLogger(), 2);
                throw new Saxon.Ejava.lang.AssertionError(((e.Message+". No value has been set for parameter ")+slotNumber));
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return evaluateVariable(context).iterate();
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return evaluateVariable(context).head();
        }
        public override string getExpressionName() {
            return "supplied";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("supplied", this);
            destination.emitAttribute("slot", (slotNumber+""));
            destination.endElement();
        }
        public override string ToString() {
            return (("suppliedParam("+slotNumber)+")");
        }
    
    }
}