using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AdjacentTextNodeMergerAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.AdjacentTextNodeMerger scc = (Saxon.Hej.expr.AdjacentTextNodeMerger)(getExpression());
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(scc) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                Saxon.Hej.expr.Expression select = scc.getBaseExpression();
                Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
                if ((selection != null)) {
                    return (watchManager, @out, context) => {
                        Saxon.Eej.ee.stream.feed.SimpleContentFeed action = new Saxon.Eej.ee.stream.feed.SimpleContentFeed(@out);
                        Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                        watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                        return watch;
                    } ;
                }
                return null;
            } else {
                return null;
            }
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.AdjacentTextNodeMergerAdjunct.AdjacentTextNodeMergerFeed(getExpression(), @out, context);
        }
        private class AdjacentTextNodeMergerFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
            private bool prevText = false;
            internal AdjacentTextNodeMergerFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (Saxon.Hej.expr.AdjacentTextNodeMerger.isTextNode(item)) {
                    Saxon.Hej.str.UnicodeString s = item.getUnicodeStringValue();
                    if (!(s.isEmpty())) {
                        buffer.accept(s);
                        prevText = true;
                    }
                } else {
                    flush();
                    getNextOutputter().append(item);
                }
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
                if (!(chars.isEmpty())) {
                    buffer.accept(chars);
                    prevText = true;
                }
            }
            private void flush() {
                if (prevText) {
                    Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(getPipelineConfiguration().getConfiguration());
                    o.setNodeKind(Saxon.Hej.type.Type.TEXT);
                    o.setStringValue(buffer.toUnicodeString());
                    getNextOutputter().append(o);
                    buffer.clear();
                }
                prevText = false;
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
                prevText = false;
                base.startElement(elemName, typeCode, location, properties);
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                prevText = false;
                base.startElement(elemName, type, attributes, namespaces, location, properties);
            }
            public override void endElement() {
                prevText = false;
                base.endElement();
            }
            public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
                prevText = false;
                base.processingInstruction(name, data, location, properties);
            }
            public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
                prevText = false;
                base.comment(content, location, properties);
            }
            public override void close() {
                flush();
                getNextOutputter().close();
            }
        
        }
    
    }
}