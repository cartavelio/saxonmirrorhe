using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public sealed class AnyNodeTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private static readonly Saxon.Hej.pattern.AnyNodeTest THE_INSTANCE = new Saxon.Hej.pattern.AnyNodeTest();
        public static Saxon.Hej.pattern.AnyNodeTest getInstance() {
            return THE_INSTANCE;
        }
        private AnyNodeTest()  {
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ANY_NODE;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return (nodeKind != Saxon.Hej.type.Type.PARENT_POINTER);
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => (nodeKindArray[nodeNr] != Saxon.Hej.type.Type.PARENT_POINTER));
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return true;
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            return true;
        }
        public override double getDefaultPriority() {
            return -(0.5);
        }
        public override string ToString() {
            return "node()";
        }
        public string exportQNameTest() {
            return "*";
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            return "true";
        }
    
    }
}