using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class AbsorptionFeed : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
        private readonly System.Collections.Generic.Stack<Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem> valuesUnderConstructionStack = new System.Collections.Generic.Stack<Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem>();
        internal System.Collections.Generic.IList<Saxon.Hej.om.Sequence> valuesReadyToBeOutput = new System.Collections.Generic.List<Saxon.Hej.om.Sequence>(5);
        public AbsorptionFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem item = makePendingItem(valuesReadyToBeOutput.Count, node);
            valuesUnderConstructionStack.Push(item);
            valuesReadyToBeOutput.Add(Saxon.Hej.value.StringValue.EMPTY_STRING);
            return item.getGatherer();
        }
        protected internal abstract Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node);
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (!(hasFailed())) {
                Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem item = valuesUnderConstructionStack.Pop();
                int seq = item.getSequenceNr();
                valuesReadyToBeOutput[seq] = item.deliver();
                if ((valuesUnderConstructionStack.Count==0)) {
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    foreach (Saxon.Hej.om.Sequence val in valuesReadyToBeOutput) {
                        processItems(val.iterate(), @out);
                    }
                    valuesReadyToBeOutput.Clear();
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                Saxon.Hej.om.Sequence value = processGroundedItem(item);
                if (!(hasFailed())) {
                    if ((valuesUnderConstructionStack.Count==0)) {
                        processItems(value.iterate(), getNextOutputter());
                    } else {
                        valuesReadyToBeOutput.Add(value);
                    }
                }
            }
        }
        protected internal abstract Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item);
        public override void setHasFailed() {
            if (!((valuesUnderConstructionStack.Count==0))) {
                Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem item = valuesUnderConstructionStack.Peek();
                item.rollback();
            }
            base.setHasFailed();
        }
        public abstract class PendingItem {
            internal int sequenceNr;
            public PendingItem(int sequenceNr)  {
                this.sequenceNr = sequenceNr;
            }
            public int getSequenceNr() {
                return sequenceNr;
            }
            public abstract Saxon.Hej.@event.Receiver getGatherer();
            public abstract Saxon.Hej.om.Sequence deliver();
            public abstract void rollback();
        
        }
    
    }
}