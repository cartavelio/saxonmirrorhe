using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class ItemFeed : Saxon.Hej.@event.ProxyOutputter {
        private Saxon.Eej.ee.stream.watch.Terminator terminator;
        private Saxon.Hej.expr.Expression expression;
        private readonly Saxon.Hej.expr.XPathContext context;
        private bool failed = false;
        public ItemFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result) {
            this.context = context;
        }
        public ItemFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result) {
            this.expression = exp;
            this.context = context;
        }
        public ItemFeed(Saxon.Hej.@event.Outputter result, Saxon.Hej.expr.XPathContext context, bool thisIsAnExceptionalCase) : base (result) {
            this.context = context;
        }
        public static void processItems(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.@event.Outputter result) {
            try {
                Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => result.append(it) /* converted from instance method reference */));
            } catch (Saxon.Hej.trans.UncheckedXPathException unxe) {
                Saxon.Hej.trans.XPathException xe = unxe.getXPathException();
                if ((xe is Saxon.Hej.trans.QuitParsingException)) {
                    throw xe;
                } else {
                    if ((result is Saxon.Eej.ee.stream.feed.ItemFeed)) {
                        ((Saxon.Eej.ee.stream.feed.ItemFeed)(result)).dynamicError(xe);
                    } else {
                        throw xe;
                    }
                }
            }
        }
        public void setExpression(Saxon.Hej.expr.Expression exp) {
            this.expression = exp;
        }
        public Saxon.Hej.expr.Expression getExpression() {
            return expression;
        }
        public void setTerminator(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            this.terminator = terminator;
        }
        public virtual void setHasFailed() {
            failed = true;
        }
        public Saxon.Eej.ee.stream.watch.Terminator getTerminator() {
            return terminator;
        }
        public virtual Saxon.Eej.ee.stream.feed.ItemFeed getResultFeed() {
            return (Saxon.Eej.ee.stream.feed.ItemFeed)(getNextOutputter());
        }
        public Saxon.Hej.expr.XPathContext getContext() {
            return context;
        }
        public virtual void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            this.terminator = terminator;
            getResultFeed().open(terminator);
        }
        public virtual Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            append(node);
            return null;
        }
        public abstract override void append(Saxon.Hej.om.Item item);
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            append(item);
        }
        public virtual void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
        }
        public override void close() {
            if (!(hasFailed())) {
                try {
                    getNextOutputter().close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    if ((expression != null)) {
                        e.maybeSetLocation(expression.getLocation());
                    }
                    throw;
                }
            }
        }
        public virtual void dynamicError(Saxon.Hej.trans.XPathException error) {
            setHasFailed();
            if ((expression != null)) {
                error.maybeSetLocation(expression.getLocation());
            }
            Saxon.Eej.ee.stream.feed.ItemFeed next = getResultFeed();
            while (true) {
                if ((next is Saxon.Eej.ee.stream.adjunct.TryCatchAdjunct.TryCatchFeed)) {
                    next.dynamicError(error);
                    return ;
                } else {
                    next.setHasFailed();
                    Saxon.Hej.@event.Outputter result = next.getNextOutputter();
                    if ((result is Saxon.Eej.ee.stream.feed.ItemFeed)) {
                        next = ((Saxon.Eej.ee.stream.feed.ItemFeed)(result));
                    } else {
                        if ((error is Saxon.Hej.type.ValidationException)) {
                            Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(error);
                            xe.setErrorCodeQName(error.getErrorCodeQName());
                            throw xe;
                        }
                        throw error;
                    }
                }
            }
        }
        public bool hasFailed() {
            return failed;
        }
    
    }
}