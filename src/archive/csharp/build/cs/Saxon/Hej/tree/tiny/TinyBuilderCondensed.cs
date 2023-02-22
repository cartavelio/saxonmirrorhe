using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TinyBuilderCondensed : Saxon.Hej.tree.tiny.TinyBuilder {
        public TinyBuilderCondensed(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
        }
        public Saxon.Hej.z.IntHashMap<int[]> textValues = new Saxon.Hej.z.IntHashMap<int[]>(100);
        public override void endElement() {
            Saxon.Hej.tree.tiny.TinyTree tree = getTree();
            base.endElement();
            int last = (tree.numberOfNodes-1);
            bool sameDepth = (tree.depth[last] == getCurrentDepth());
        }
        protected internal override string getAttValue(Saxon.Hej.om.AttributeInfo att) {
            return System.String.Intern(base.getAttValue(att));
        }
    
    }
}