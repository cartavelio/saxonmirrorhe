using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ResultDocumentAdjunct : Saxon.Eej.ee.stream.adjunct.ParentConstructorAdjunct {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            Saxon.Hej.expr.instruct.ResultDocument inst = (Saxon.Hej.expr.instruct.ResultDocument)(getExpression());
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(inst.getContentExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => new Saxon.Eej.ee.stream.adjunct.ResultDocumentAdjunct.ResultDocumentFeed(getExpression(), @out, context));
            } else {
                return base.getFeedMaker(arg);
            }
        }
        public class ResultDocumentFeed : Saxon.Eej.ee.stream.feed.ComplexNodeEventFeed {
            public ResultDocumentFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            protected internal override void processLeft(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.expr.instruct.ResultDocument instr = (Saxon.Hej.expr.instruct.ResultDocument)(getExpression());
                next = instr.processLeft(context);
            }
            protected internal override void processRight(Saxon.Hej.expr.XPathContext context) {
                try {
                    next.endDocument();
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setXPathContext(context);
                    throw;
                }
                next.close();
            }
        
        }
    
    }
}