using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Outermost : Saxon.Hej.functions.SystemFunction {
        internal bool presorted = false;
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if ((((arguments[0].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.PEER_NODESET)) != 0)) {
                return arguments[0];
            }
            presorted = (((arguments[0].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0);
            return null;
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator @in = arguments[0].iterate();
            if (!(presorted)) {
                @in = new Saxon.Hej.expr.sort.DocumentOrderIterator(@in, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
            }
            Saxon.Hej.om.SequenceIterator @out = new Saxon.Hej.functions.Outermost.OutermostIterator(@in);
            return Saxon.Hej.om.SequenceTool.toLazySequence(@out);
        }
        public override void exportAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            base.exportAttributes(@out);
            if (presorted) {
                @out.emitAttribute("flags", "p");
            }
        }
        public override void importAttributes(Saxon.Ejava.util.Properties attributes) {
            base.importAttributes(attributes);
            string flags = attributes.getProperty("flags");
            if (((flags != null) && flags.Contains("p"))) {
                presorted = true;
            }
        }
        private class OutermostIterator : Saxon.Hej.om.SequenceIterator {
            internal Saxon.Hej.om.SequenceIterator @in;
            internal Saxon.Hej.om.NodeInfo current = null;
            internal int position = 0;
            public OutermostIterator(Saxon.Hej.om.SequenceIterator @in)  {
                this.@in = @in;
            }
            public Saxon.Hej.om.Item next() {
                while (true) {
                    Saxon.Hej.om.NodeInfo next = (Saxon.Hej.om.NodeInfo)(@in.next());
                    if ((next == null)) {
                        current = null;
                        position = -1;
                        return null;
                    }
                    if (((current == null) || !(Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(current, next)))) {
                        current = next;
                        position++;
                        return current;
                    }
                }
            }
            public void close() {
                @in.close();
            }
            public void discharge() {
                @in.discharge();
            }
        
        }
        public override string getStreamerName() {
            return "Outermost";
        }
    
    }
}