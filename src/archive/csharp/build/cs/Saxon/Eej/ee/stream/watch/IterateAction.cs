using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class IterateAction : Saxon.Eej.ee.stream.watch.ForEachAction {
        internal bool broken = false;
        internal Saxon.Eej.ee.stream.watch.Terminator terminator;
        public IterateAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (watchManager, expression, result, context) {
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            this.terminator = terminator;
            Saxon.Hej.expr.XPathContext outerContext = getContext();
            ((Saxon.Hej.expr.instruct.IterateInstr)(getExpression())).getInitiallyExp().process(getNextOutputter(), outerContext);
            makeBodyContext();
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (broken) {
                return null;
            }
            return base.startSelectedParentNode(node, locationId);
        }
        public override void append(Saxon.Hej.om.Item it) {
            if (broken) {
                return ;
            }
            base.append(it);
            checkForBreakInstruction();
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (broken) {
                return ;
            }
            base.endSelectedParentNode(locationId);
            checkForBreakInstruction();
        }
        private void checkForBreakInstruction() {
            Saxon.Hej.expr.XPathContextMajor cm = getContext().getMajorContext();
            Saxon.Hej.expr.TailCallLoop.TailCallInfo tail = cm.getTailCallInfo();
            if ((tail is Saxon.Hej.expr.instruct.BreakInstr)) {
                broken = true;
                if (getWatchManager().allowsEarlyExit()) {
                    terminator.terminate();
                }
            }
        }
        public override void close() {
            if (!(broken)) {
                Saxon.Hej.expr.Expression onCompletion = ((Saxon.Hej.expr.instruct.IterateInstr)(getExpression())).getOnCompletion();
                Saxon.Hej.om.SequenceIterator iter = onCompletion.iterate(getContext());
                processItems(iter, getNextOutputter());
            }
            broken = false;
            base.close();
        }
    
    }
}