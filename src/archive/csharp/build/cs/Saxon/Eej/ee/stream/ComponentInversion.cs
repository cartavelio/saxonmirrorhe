using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class ComponentInversion {
        private readonly Saxon.Hej.expr.Expression body;
        private Saxon.Eej.ee.stream.Inversion inversion;
        private Saxon.Hej.expr.instruct.SlotManager stackFrame = null;
        public ComponentInversion(Saxon.Hej.expr.Expression body)  {
            this.body = body;
        }
        public Saxon.Eej.ee.stream.watch.Watch getWatch(Saxon.Eej.ee.stream.watch.WatchManager wm, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) {
            return inversion.getWatch(wm, result, context);
        }
        public void invert(bool isW3CStreamable) {
            try {
                inversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(body, false);
            } catch (Saxon.Hej.trans.XPathException e) {
                string frag = (isW3CStreamable ? "although it is" : "and is not");
                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(((("Component cannot be streamed, "+frag)+" guaranteed streamable according to W3C rules. ")+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXST0062);
                e2.setErrorCode(e.getErrorCodeLocalPart());
                e2.setLocator(body.getLocation());
                e2.setXPathContext(e.getXPathContext());
                throw e2;
            }
        }
        public void setStackFrame(Saxon.Hej.expr.instruct.SlotManager frame) {
            this.stackFrame = frame;
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrame() {
            return stackFrame;
        }
        public Saxon.Eej.ee.stream.Inversion getInversion() {
            return inversion;
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter presenter) {
            Saxon.Hej.expr.Expression exp = body;
            presenter.emitAttribute("line", (exp.getLocation().getLineNumber()+""));
            presenter.emitAttribute("module", exp.getLocation().getSystemId());
            presenter.emitAttribute("streaming", "true");
            inversion.explain(presenter);
            exp.export(presenter);
        }
    
    }
}