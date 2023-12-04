using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class NameTestUnion : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private readonly Saxon.Hej.pattern.UnionQNameTest nameTest;
        private readonly int nodeKind;
        public NameTestUnion(Saxon.Hej.pattern.UnionQNameTest nameTest, int nodeKind)  {
            System.Diagnostics.Debug.Assert((((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.ATTRIBUTE))));
            this.nameTest = nameTest;
            this.nodeKind = nodeKind;
        }
        public static Saxon.Hej.pattern.NameTestUnion withTests(System.Collections.Generic.IList<Saxon.Hej.pattern.NodeTest> tests) {
            System.Diagnostics.Debug.Assert(!(tests.Count==0));
            int nodeKind = tests[0].getPrimitiveType();
            System.Diagnostics.Debug.Assert(((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.ATTRIBUTE)));
            System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> qNameTests = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>(tests.Count);
            foreach (Saxon.Hej.pattern.NodeTest test in tests) {
                if ((test is Saxon.Hej.pattern.QNameTest)) {
                    qNameTests.Add((Saxon.Hej.pattern.QNameTest)(test));
                } else {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            return new Saxon.Hej.pattern.NameTestUnion(new Saxon.Hej.pattern.UnionQNameTest(qNameTests), nodeKind);
        }
        public int getNodeKind() {
            return nodeKind;
        }
        public override Saxon.Hej.type.UType getUType() {
            return ((nodeKind == Saxon.Hej.type.Type.ELEMENT) ? Saxon.Hej.type.UType.ELEMENT : Saxon.Hej.type.UType.ATTRIBUTE);
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return ((nodeKind == this.nodeKind) && nameTest.matches(name.getStructuredQName()));
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            Saxon.Hej.om.NamePool pool = tree.getNamePool();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => ((((nodeKindArray[nodeNr]&0x0f)) == nodeKind) && nameTest.matchesFingerprint(pool, (nameCodeArray[nodeNr]&Saxon.Hej.om.NamePool.FP_MASK))));
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return matches(node.getNodeKind(), Saxon.Hej.om.NameOfNode.makeName(node), node.getSchemaType());
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            return nameTest.matches(qname);
        }
        public bool matchesFingerprint(Saxon.Hej.om.NamePool namePool, int fp) {
            return nameTest.matchesFingerprint(namePool, fp);
        }
        public override double getDefaultPriority() {
            return 1.0;
        }
        public override int getFingerprint() {
            return -1;
        }
        public override Saxon.Hej.om.StructuredQName getMatchingNodeName() {
            return null;
        }
        public override int getPrimitiveType() {
            return nodeKind;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.empty();
        }
        public override string ToString() {
            switch (nodeKind) {
              case Saxon.Hej.type.Type.ELEMENT:
                return (("element("+nameTest.ToString())+")");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return (("attribute("+nameTest.ToString())+")");
              default:
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
        }
        public override int GetHashCode() {
            return ((nodeKind<<21)^nameTest.GetHashCode());
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.NameTestUnion) && (((Saxon.Hej.pattern.NameTestUnion)(other)).nodeKind == nodeKind)) && ((Saxon.Hej.pattern.NameTestUnion)(other)).nameTest.Equals(nameTest));
        }
        public override string getFullAlphaCode() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public string exportQNameTest() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            return Saxon.Ejava.util.Optional<string>.of("The node has the wrong name");
        }
        public override string toShortString() {
            return ToString();
        }
    
    }
}