using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ArrayBlockAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private int consumingArg = -1;
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return base.getWatchMaker(forGrouping);
        }
        private class SquareArrayFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal int consumingArg;
            internal System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members;
            internal System.Collections.Generic.IList<Saxon.Hej.om.Item> consumingMember = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            public SquareArrayFeed(Saxon.Hej.ma.arrays.SquareArrayConstructor exp, int arg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
                this.consumingArg = arg;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.ma.arrays.SquareArrayConstructor block = (Saxon.Hej.ma.arrays.SquareArrayConstructor)(getExpression());
                members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                for (int i = 0; (i<consumingArg); i++) {
                    Saxon.Hej.expr.Expression child = block.getOperanda().getOperand(i).getChildExpression();
                    Saxon.Hej.om.GroundedValue member = Saxon.Hej.value.SequenceExtent.from(child.iterate(getContext()));
                    members.Add(member);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                consumingMember.Add(item);
            }
            public override void close() {
                members.Add(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(consumingMember));
                Saxon.Hej.ma.arrays.SquareArrayConstructor block = (Saxon.Hej.ma.arrays.SquareArrayConstructor)(getExpression());
                Saxon.Hej.@event.Outputter result = getNextOutputter();
                for (int i = (consumingArg+1); (i<block.getOperanda().getNumberOfOperands()); i++) {
                    Saxon.Hej.expr.Expression child = block.getOperanda().getOperand(i).getChildExpression();
                    Saxon.Hej.om.GroundedValue member = Saxon.Hej.value.SequenceExtent.from(child.iterate(getContext()));
                    members.Add(member);
                }
                Saxon.Hej.ma.arrays.ArrayItem resultArray = new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
                result.append(resultArray);
                result.close();
            }
        
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return new Saxon.Eej.ee.stream.adjunct.ArrayBlockAdjunct.SquareArrayFeed((Saxon.Hej.ma.arrays.SquareArrayConstructor)(getExpression()), consumingArg, @out, context);
        }
    
    }
}