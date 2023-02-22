using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class FilteringFeed : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
        private readonly Saxon.Eej.ee.stream.feed.FilteringFeed.Filter filter;
        private int position;
        private readonly System.Collections.Generic.Stack<bool> matchStack = new System.Collections.Generic.Stack<bool>();
        private bool terminated;
        public FilteringFeed(Saxon.Eej.ee.stream.feed.ItemFeed nextFeed, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.FilteringFeed.Filter filter) : base (nextFeed, context) {
            this.filter = filter;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            getResultFeed().open(terminator);
            position = 0;
            this.terminated = false;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if (!(terminated)) {
                Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult match = filter.matches(node, ++position);
                matchStack.Push((match == Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES));
                if ((match == Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES)) {
                    return getResultFeed().startSelectedParentNode(node, locationId);
                } else {
                    if (((match == Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE) && (getTerminator() != null))) {
                        getNextOutputter().close();
                        terminated = true;
                        getTerminator().terminate();
                    }
                }
            }
            return null;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            if (!(terminated)) {
                bool matches = matchStack.Pop();
                if (matches) {
                    getResultFeed().endSelectedParentNode(locationId);
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if ((!(terminated) && !(hasFailed()))) {
                try {
                    Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult match = filter.matches(item, ++position);
                    if ((match == Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES)) {
                        getNextOutputter().append(item);
                    } else {
                        if (((match == Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE) && (getTerminator() != null))) {
                            getNextOutputter().close();
                            terminated = true;
                            getTerminator().terminate();
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
        }
        public override void close() {
            if ((!(terminated) && !(hasFailed()))) {
                getNextOutputter().close();
            }
        }
        public enum FilterResult {
            SKIP,
            MATCHES,
            DONE,
        }
        public interface Filter {
            Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position);
        
        }
        public delegate Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult FilterDelegate (Saxon.Hej.om.Item item, int position);
        public class FilterLambda : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            private readonly Saxon.Eej.ee.stream.feed.FilteringFeed.FilterDelegate @delegate;
            public FilterLambda(Saxon.Eej.ee.stream.feed.FilteringFeed.FilterDelegate @delegate)  {
                this.@delegate = @delegate;
            }
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                return /*Delegate.matches()*/@delegate(item, position);
            }
        
        }
        public static Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda TransparentFilter = new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES);
        public static Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda OpaqueFilter = new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE);
    
    }
}