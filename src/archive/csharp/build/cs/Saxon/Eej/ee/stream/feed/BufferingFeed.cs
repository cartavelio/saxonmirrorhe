using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class BufferingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
        public BufferingFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
        }
        public override void append(Saxon.Hej.om.Item item) {
            inputValue.Add(item);
            if ((inputValue.Count == 2)) {
                Saxon.Hej.expr.XPathContext c = getContext();
                c.getController().getErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident((("Execution of "+getExpression().toShortString())+" is not fully streamed"), Saxon.Hej.trans.SaxonErrorCode.SXST0067, getExpression().getLocation()).asWarning()));
            }
        }
        public override void close() {
            ((Saxon.Hej.expr.XPathContextMinor)(getContext())).makeStackFrameMutable();
            getContext().getStackFrame().pushDynamicValue(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(inputValue));
            inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
            Saxon.Hej.om.SequenceIterator iter = getExpression().iterate(getContext());
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            try {
                Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */@out.append));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
            @out.close();
        }
    
    }
}