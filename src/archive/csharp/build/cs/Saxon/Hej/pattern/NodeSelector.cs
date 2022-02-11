using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class NodeSelector : Saxon.Hej.pattern.NodeTest {
        private readonly System.Predicate<Saxon.Hej.om.NodeInfo> predicate;
        private NodeSelector(System.Predicate<Saxon.Hej.om.NodeInfo> predicate)  {
            this.predicate = predicate;
        }
        public static Saxon.Hej.pattern.NodeSelector of(System.Predicate<Saxon.Hej.om.NodeInfo> predicate) {
            return new Saxon.Hej.pattern.NodeSelector(predicate);
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return predicate(node);
        }
        public override double getDefaultPriority() {
            return 0;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("NodePredicate doesn't support this method");
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ANY;
        }
    
    }
}