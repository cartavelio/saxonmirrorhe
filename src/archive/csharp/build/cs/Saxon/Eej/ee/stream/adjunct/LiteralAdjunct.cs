using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class LiteralAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.Literal exp = (Saxon.Hej.expr.Literal)(getExpression());
            if (Saxon.Hej.expr.Literal.isEmptySequence(exp)) {
                return new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
            } else {
                return base.toStreamingPattern(config);
            }
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.LiteralAdjunct.LiteralItemFeed(getExpression(), @out, context);
        }
        public class LiteralItemFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal LiteralItemFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (exp, @out, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.@event.Outputter @out = getNextOutputter();
                if (!(Saxon.Hej.expr.Literal.isEmptySequence(getExpression()))) {
                    Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getExpression())).getGroundedValue();
                    if ((value is Saxon.Hej.om.Item)) {
                        @out.append((Saxon.Hej.om.Item)(value));
                    } else {
                        Saxon.Hej.om.SequenceIterator iter = value.iterate();
                        processItems(iter, @out);
                    }
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
            }
        
        }
    
    }
}