using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class AssertionTreeBuilder : Saxon.Hej.@event.TeeOutputter {
        private readonly Saxon.Hej.@event.Sink sink;
        private readonly bool retainComments;
        private Saxon.Hej.tree.tiny.TinyBuilder builder;
        public AssertionTreeBuilder(Saxon.Hej.@event.Receiver next, bool retainComments) : base (next, new Saxon.Hej.@event.Sink(next.getPipelineConfiguration())) {
            sink = (Saxon.Hej.@event.Sink)(getSecondDestination());
            this.retainComments = retainComments;
        }
        public void activate(Saxon.Hej.tree.tiny.TinyBuilder builder) {
            this.builder = builder;
            setSecondDestination((retainComments ? builder : new Saxon.Hej.@event.CommentStripper(builder)));
        }
        public void deactivate() {
            this.builder = null;
            setSecondDestination(sink);
        }
        internal void markAncestorNodesUntyped() {
            if ((builder != null)) {
                Saxon.Hej.tree.tiny.TinyTree tree = builder.getTree();
                Saxon.Hej.tree.tiny.TinyNodeImpl last = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode((tree.getNumberOfNodes()-1)));
                Saxon.Hej.type.SchemaType[] typeArray = tree.getTypeArray();
                if ((typeArray != null)) {
                    while ((last != null)) {
                        typeArray[last.getNodeNumber()] = Saxon.Hej.type.AnyType.getInstance();
                        last = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)last.getParent());
                    }
                }
            }
        }
    
    }
}