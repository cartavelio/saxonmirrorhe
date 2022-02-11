using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class PatternThatSetsCurrent : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.expr.flwor.LocalVariableBinding binding;
        private Saxon.Hej.pattern.Pattern wrappedPattern;
        public PatternThatSetsCurrent(Saxon.Hej.pattern.Pattern wrappedPattern) : this (wrappedPattern, new Saxon.Hej.expr.flwor.LocalVariableBinding(Saxon.Hej.functions.Current.FN_CURRENT, Saxon.Hej.value.SequenceType.SINGLE_ITEM)) {
        }
        public PatternThatSetsCurrent(Saxon.Hej.pattern.Pattern wrappedPattern, Saxon.Hej.expr.flwor.LocalVariableBinding binding)  {
            this.wrappedPattern = wrappedPattern;
            this.binding = binding;
            binding.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(wrappedPattern.getItemType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            adoptChildExpression(wrappedPattern);
            setPriority(wrappedPattern.getDefaultPriority());
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new Saxon.Hej.expr.Operand(this, wrappedPattern, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.LocalBinding getCurrentBinding() {
            return binding;
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            return (binding == this.binding);
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            slotManager.allocateSlotNumber(Saxon.Hej.functions.Current.FN_CURRENT);
            binding.setSlotNumber(nextFree++);
            return wrappedPattern.allocateSlots(slotManager, nextFree);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            context.setLocalVariable(binding.getLocalSlotNumber(), item);
            return wrappedPattern.matches(item, context);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return wrappedPattern.getItemType();
        }
        public override Saxon.Hej.pattern.Pattern simplify() {
            wrappedPattern = wrappedPattern.simplify();
            return this;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            wrappedPattern = wrappedPattern.typeCheck(visitor, contextItemType);
            return this;
        }
        public override Saxon.Hej.type.UType getUType() {
            return wrappedPattern.getUType();
        }
        public override int getFingerprint() {
            return wrappedPattern.getFingerprint();
        }
        public override string reconstruct() {
            return wrappedPattern.ToString();
        }
        public override Saxon.Api.HostLanguage getHostLanguage() {
            return wrappedPattern.getHostLanguage();
        }
        public override bool isMotionless() {
            return wrappedPattern.isMotionless();
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return wrappedPattern.matchesBeneathAnchor(node, anchor, context);
        }
        public override Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            Saxon.Hej.pattern.Pattern w2 = wrappedPattern.convertToTypedPattern(val);
            if ((w2 == wrappedPattern)) {
                return this;
            } else {
                return new Saxon.Hej.pattern.PatternThatSetsCurrent(w2);
            }
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.LocalVariableBinding newCurrent = new Saxon.Hej.expr.flwor.LocalVariableBinding(Saxon.Hej.functions.Current.FN_CURRENT, Saxon.Hej.value.SequenceType.SINGLE_ITEM);
            rebindings.put(binding, newCurrent);
            Saxon.Hej.pattern.PatternThatSetsCurrent n = new Saxon.Hej.pattern.PatternThatSetsCurrent(wrappedPattern.copy(rebindings), newCurrent);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.withCurrent");
            wrappedPattern.export(presenter);
            presenter.endElement();
        }
        public Saxon.Hej.pattern.Pattern getWrappedPattern() {
            return wrappedPattern;
        }
    
    }
}