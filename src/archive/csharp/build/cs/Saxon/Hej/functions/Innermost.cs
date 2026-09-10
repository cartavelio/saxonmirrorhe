using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Innermost : Saxon.Hej.functions.SystemFunction {
        internal bool presorted = false;
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if ((((arguments[0].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.PEER_NODESET)) != 0)) {
                return arguments[0];
            }
            if ((((arguments[0].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0)) {
                presorted = true;
            }
            return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(innermost(arguments[0].iterate()));
        }
        public Saxon.Hej.om.SequenceIterator innermost(Saxon.Hej.om.SequenceIterator @in) {
            if (!(presorted)) {
                @in = new Saxon.Hej.expr.sort.DocumentOrderIterator(@in, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
            }
            return new Saxon.Hej.functions.Innermost.InnermostIterator(@in);
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
        private class InnermostIterator : Saxon.Hej.om.SequenceIterator {
            internal Saxon.Hej.om.SequenceIterator @in;
            internal Saxon.Hej.om.NodeInfo pending = null;
            internal int position = 0;
            public InnermostIterator(Saxon.Hej.om.SequenceIterator @in)  {
                this.@in = @in;
                pending = (Saxon.Hej.om.NodeInfo)(@in.next());
            }
            public Saxon.Hej.om.Item next() {
                if ((pending == null)) {
                    position = -1;
                    return null;
                } else {
                    while (true) {
                        Saxon.Hej.om.NodeInfo next = (Saxon.Hej.om.NodeInfo)(@in.next());
                        if ((next == null)) {
                            Saxon.Hej.om.NodeInfo current = pending;
                            position++;
                            pending = null;
                            return current;
                        }
                        if (Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(pending, next)) {
                            pending = next;
                        } else {
                            position++;
                            Saxon.Hej.om.NodeInfo current = pending;
                            pending = next;
                            return current;
                        }
                    }
                }
            }
            public void close() {
                @in.close();
            }
        
        }
    
    }
}