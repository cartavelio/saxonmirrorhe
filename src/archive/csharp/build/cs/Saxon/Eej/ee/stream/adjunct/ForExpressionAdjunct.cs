using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ForExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.ForExpression exp = (Saxon.Hej.expr.ForExpression)(getExpression());
            Saxon.Eej.ee.stream.Posture p = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getSequence(), contextInfo, reasons).getPosture();
            if ((p == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return base.computeStreamability(contextInfo, reasons);
            } else {
                if ((reasons != null)) {
                    string desc = (exp.getVariableQName().hasURI(Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE) ? exp.getSequence().toShortString() : ("bound to the variable $"+exp.getVariableName()));
                    reasons.Add((((("The expression "+desc)+" is capable of delivering streamed nodes (line ")+getExpression().getLocation().getLineNumber())+")"));
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.ForExpressionAdjunct.ForExpressionFeed(getExpression(), @out, context);
        }
        public class ForExpressionFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            public ForExpressionFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (expr, @out, context) {
            }
            public override void append(Saxon.Hej.om.Item item) {
                getContext().setLocalVariable(((Saxon.Hej.expr.ForExpression)(getExpression())).getLocalSlotNumber(), item);
                Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.expr.ForExpression)(getExpression())).getAction().iterate(getContext());
                processItems(iter, getNextOutputter());
            }
        
        }
    
    }
}