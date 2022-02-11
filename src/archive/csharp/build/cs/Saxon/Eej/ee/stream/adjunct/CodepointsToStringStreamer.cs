using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CodepointsToStringStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Inner_ItemFeed_d207e80(this, getExpression(), @out, context);
        }
    
        private class Inner_ItemFeed_d207e80 : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly CodepointsToStringStreamer outerInstance;
            public Inner_ItemFeed_d207e80(CodepointsToStringStreamer outerInstance, /*AB*/Saxon.Hej.expr.Expression arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
                buffer = new Saxon.Ejava.lang.StringBuilder(64);
                checker = /*superMethod*/getConfiguration().getValidCharacterChecker();
            }
            readonly Saxon.Ejava.lang.StringBuilder buffer;
            readonly Saxon.Hej.z.IntPredicateProxy checker;
            public override void append(Saxon.Hej.om.Item item) {
                if (!(/*superMethod*/hasFailed())) {
                    long next = ((Saxon.Hej.value.IntegerValue)(/*localparam*/item)).longValue();
                    if ((((/*localvar*/next<0) || (/*localvar*/next>System.Int32.MaxValue)) || !(/*field*/checker.test((int)(/*localvar*/next))))) {
                        /*superMethod*/dynamicError(new Saxon.Hej.trans.XPathException((("Invalid XML character [x "+Saxon.Impl.Helpers.Utils.intToHexString((int)(/*localvar*/next)))+']'), "FOCH0001"));
                        return ;
                    }
                    /*field*/buffer.appendCodePoint((int)(((Saxon.Hej.value.IntegerValue)(/*localparam*/item)).longValue()));
                }
            }
            public override void close() {
                if (!(/*superMethod*/hasFailed())) {
                    /*superMethod*/getNextOutputter().append(new Saxon.Hej.value.StringValue(/*field*/buffer.ToString()));
                    base.close();
                }
                /*field*/buffer.setLength(0);
            }
        }

    }
}