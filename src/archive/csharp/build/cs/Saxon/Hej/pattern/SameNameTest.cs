using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class SameNameTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private readonly Saxon.Hej.om.NodeInfo origin;
        public SameNameTest(Saxon.Hej.om.NodeInfo origin)  {
            this.origin = origin;
        }
        public int getNodeKind() {
            return origin.getNodeKind();
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.fromTypeCode(origin.getNodeKind());
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            if ((nodeKind != origin.getNodeKind())) {
                return false;
            }
            if ((name.hasFingerprint() && origin.hasFingerprint())) {
                return (name.getFingerprint() == origin.getFingerprint());
            } else {
                return (name.hasURI(origin.getNamespaceUri()) && name.getLocalPart().Equals(origin.getLocalPart()));
            }
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => {
                int k = (nodeKindArray[nodeNr]&0x0f);
                if ((k == Saxon.Hej.type.Type.WHITESPACE_TEXT)) {
                    k = Saxon.Hej.type.Type.TEXT;
                }
                if ((k != origin.getNodeKind())) {
                    return false;
                } else {
                    if (origin.hasFingerprint()) {
                        return (((nameCodeArray[nodeNr]&unchecked((int)0xfffff))) == origin.getFingerprint());
                    } else {
                        return Saxon.Hej.tree.util.Navigator.haveSameName(tree.getNode(nodeNr), origin);
                    }
                }
            } );
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return ((node == origin) || (((node.getNodeKind() == origin.getNodeKind()) && Saxon.Hej.tree.util.Navigator.haveSameName(node, origin))));
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            return Saxon.Hej.om.NameOfNode.makeName(origin).getStructuredQName().Equals(qname);
        }
        public override double getDefaultPriority() {
            return 0.0;
        }
        public override int getFingerprint() {
            if (origin.hasFingerprint()) {
                return origin.getFingerprint();
            } else {
                Saxon.Hej.om.NamePool pool = origin.getConfiguration().getNamePool();
                return pool.allocateFingerprint(origin.getNamespaceUri(), origin.getLocalPart());
            }
        }
        public override int getPrimitiveType() {
            return origin.getNodeKind();
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(new Saxon.Hej.z.IntSingletonSet(getFingerprint()));
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceURI() {
            return origin.getNamespaceUri();
        }
        public string getLocalPart() {
            return origin.getLocalPart();
        }
        public override string ToString() {
            switch (origin.getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
                return (("element("+Saxon.Hej.om.NameOfNode.makeName(origin).getStructuredQName().getEQName())+")");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return (("attribute("+Saxon.Hej.om.NameOfNode.makeName(origin).getStructuredQName().getEQName())+")");
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return (("processing-instruction("+origin.getLocalPart())+')');
              case Saxon.Hej.type.Type.NAMESPACE:
                return (("namespace-node("+origin.getLocalPart())+')');
              case Saxon.Hej.type.Type.COMMENT:
                return "comment()";
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document-node()";
              case Saxon.Hej.type.Type.TEXT:
                return "text()";
              default:
                return "***";
            }
        }
        public override int GetHashCode() {
            return (((origin.getNodeKind()<<20)^origin.getNamespaceUri().GetHashCode())^origin.getLocalPart().GetHashCode());
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.pattern.SameNameTest) && test(((Saxon.Hej.pattern.SameNameTest)(other)).origin));
        }
        public Saxon.Hej.pattern.NameTest getEquivalentNameTest() {
            return new Saxon.Hej.pattern.NameTest(origin.getNodeKind(), origin.getNamespaceUri(), origin.getLocalPart(), origin.getConfiguration().getNamePool());
        }
        public string exportQNameTest() {
            return "";
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            return "false";
        }
    
    }
}