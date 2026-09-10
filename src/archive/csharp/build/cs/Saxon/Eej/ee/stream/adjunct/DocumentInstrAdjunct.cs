using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class DocumentInstrAdjunct : Saxon.Eej.ee.stream.adjunct.ParentConstructorAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.DocumentInstrAdjunct.DocumentInstrFeed(getExpression(), @out, context);
        }
        public class DocumentInstrFeed : Saxon.Eej.ee.stream.feed.ComplexNodeEventFeed {
            public DocumentInstrFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            protected internal override void processLeft(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.expr.instruct.DocumentInstr exp = (Saxon.Hej.expr.instruct.DocumentInstr)(getExpression());
                try {
                    if (!(exp.isPreservingTypes())) {
                        Saxon.Hej.@event.Receiver validator = context.getConfiguration().getDocumentValidator(next, exp.getStaticBaseURIString(), exp.getValidationOptions(), exp.getLocation());
                        next = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                    }
                    next.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(exp.getLocation());
                    e.maybeSetContext(context);
                    dynamicError(e);
                }
            }
            protected internal override void processRight(Saxon.Hej.expr.XPathContext context) {
                next.endDocument();
            }
        
        }
    
    }
}