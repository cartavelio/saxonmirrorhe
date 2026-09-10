using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class TryCatchAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.TryCatchAdjunct.TryCatchFeed(getExpression(), @out, context);
        }
        public class TryCatchFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly Saxon.Eej.ee.stream.feed.ItemFeed originalResult;
            private bool done = false;
            private bool opened = false;
            internal TryCatchFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (new Saxon.Hej.@event.ComplexContentOutputter(new Saxon.Hej.@event.SequenceCollector(@out.getPipelineConfiguration())), context, true) {
                setExpression(exp);
                originalResult = @out;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                originalResult.open(terminator);
                getNextOutputter().open();
                opened = true;
            }
            public override void append(Saxon.Hej.om.Item item) {
                getNextOutputter().append(item);
            }
            public override void close() {
                Saxon.Hej.@event.ComplexContentOutputter @out = (Saxon.Hej.@event.ComplexContentOutputter)(getNextOutputter());
                Saxon.Hej.@event.SequenceCollector collector = (Saxon.Hej.@event.SequenceCollector)(@out.getReceiver());
                collector.close();
                try {
                    foreach (Saxon.Hej.om.Item i in collector.getList()) {
                        originalResult.append(i);
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
                originalResult.close();
            }
            public override void dynamicError(Saxon.Hej.trans.XPathException err) {
                if (err.isGlobalError()) {
                    err.setIsGlobalError(false);
                } else {
                    if (done) {
                        return ;
                    } else {
                        if (!(opened)) {
                            open(new Saxon.Eej.ee.stream.watch.Terminator());
                        }
                        done = true;
                        Saxon.Hej.om.StructuredQName code = err.getErrorCodeQName();
                        foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in ((Saxon.Hej.expr.TryCatch)(getExpression())).getCatchClauses()) {
                            if ((code == null)) {
                                code = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "XXXX9999");
                            }
                            if (clause.nameTest.matches(code)) {
                                Saxon.Hej.expr.Expression caught = clause.catchOp.getChildExpression();
                                Saxon.Hej.expr.XPathContextMajor c2 = getContext().newContext();
                                c2.setCurrentException(err);
                                Saxon.Hej.om.SequenceIterator iter = caught.iterate(c2);
                                processItems(iter, originalResult);
                                originalResult.close();
                                return ;
                            }
                        }
                    }
                }
                err.setHasBeenReported(false);
                throw err;
            }
        
        }
    
    }
}