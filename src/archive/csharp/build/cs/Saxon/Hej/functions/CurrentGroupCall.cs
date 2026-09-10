using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentGroupCall : Saxon.Hej.expr.Expression, Saxon.Hej.expr.Callable {
        private bool inHigherOrderOperand = false;
        private Saxon.Hej.type.ItemType itemType = Saxon.Hej.type.AnyItemType.getInstance();
        private Saxon.Hej.expr.instruct.ForEachGroup controllingInstruction = null;
        public override Saxon.Hej.expr.Expression getScopingExpression() {
            return getControllingInstruction();
        }
        public void setControllingInstruction(Saxon.Hej.expr.instruct.ForEachGroup instruction, Saxon.Hej.type.ItemType itemType, bool isHigherOrder) {
            resetLocalStaticProperties();
            this.controllingInstruction = instruction;
            this.inHigherOrderOperand = isHigherOrder;
            this.itemType = itemType;
        }
        public override void resetLocalStaticProperties() {
            base.resetLocalStaticProperties();
            this.controllingInstruction = null;
            this.itemType = Saxon.Hej.type.AnyItemType.getInstance();
        }
        public Saxon.Hej.expr.instruct.ForEachGroup getControllingInstruction() {
            if ((controllingInstruction == null)) {
                controllingInstruction = findControllingInstruction(this);
            }
            return controllingInstruction;
        }
        public static Saxon.Hej.expr.instruct.ForEachGroup findControllingInstruction(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.Expression child = exp;
            Saxon.Hej.expr.Expression parent = exp.getParentExpression();
            while ((parent != null)) {
                if (((parent is Saxon.Hej.expr.instruct.ForEachGroup) && (((child == ((Saxon.Hej.expr.instruct.ForEachGroup)(parent)).getActionExpression()) || (child == ((Saxon.Hej.expr.instruct.ForEachGroup)(parent)).getSortKeyDefinitionList()))))) {
                    return (Saxon.Hej.expr.instruct.ForEachGroup)(parent);
                }
                child = parent;
                parent = parent.getParentExpression();
            }
            return null;
        }
        public bool isInHigherOrderOperand() {
            return inHigherOrderOperand;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if (((itemType == Saxon.Hej.type.AnyItemType.getInstance()) && (controllingInstruction != null))) {
                itemType = controllingInstruction.getSelectExpression().getItemType();
            }
            return itemType;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_GROUP;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("currentGroup");
            @out.endElement();
        }
        protected internal override int computeSpecialProperties() {
            if ((getControllingInstruction() == null)) {
                return 0;
            } else {
                return controllingInstruction.getSelectExpression().getSpecialProperties();
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.functions.CurrentGroupCall cg = new Saxon.Hej.functions.CurrentGroupCall();
            cg.inHigherOrderOperand = inHigherOrderOperand;
            cg.itemType = itemType;
            cg.controllingInstruction = controllingInstruction;
            return cg;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.sort.GroupIterator gi = context.getCurrentGroupIterator();
            if ((gi == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("There is no current group", "XTDE1061");
                err.setLocation(getLocation());
                throw err;
            }
            return gi.currentGroup();
        }
        public override string ToString() {
            return "current-group()";
        }
        public override string toShortString() {
            return ToString();
        }
        public override string getStreamerName() {
            return "CurrentGroup";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.CurrentGroupCall.CurrentGroupCallElaborator();
        }
        private class CurrentGroupCallElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.functions.CurrentGroupCall expr = (Saxon.Hej.functions.CurrentGroupCall)(getExpression());
                return context => {
                    Saxon.Hej.expr.sort.GroupIterator gi = context.getCurrentGroupIterator();
                    if ((gi == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("There is no current group", "XTDE1061");
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    return gi.currentGroup().iterate();
                } ;
            }
        
        }
    
    }
}