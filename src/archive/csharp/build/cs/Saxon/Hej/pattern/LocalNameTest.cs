using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public sealed class LocalNameTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private readonly Saxon.Hej.om.NamePool namePool;
        private readonly int nodeKind;
        private readonly string localName;
        private readonly Saxon.Hej.type.UType uType;
        public LocalNameTest(Saxon.Hej.om.NamePool pool, int nodeKind, string localName)  {
            this.namePool = pool;
            this.nodeKind = nodeKind;
            this.localName = localName;
            uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
        }
        public int getNodeKind() {
            return nodeKind;
        }
        public override Saxon.Hej.type.UType getUType() {
            return uType;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.empty();
        }
        public override string getFullAlphaCode() {
            return ((getBasicAlphaCode()+" n*:")+localName);
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return (((name != null) && (nodeKind == this.nodeKind)) && localName.Equals(name.getLocalPart()));
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            if (((nodeKind == Saxon.Hej.type.Type.ELEMENT) && (tree is Saxon.Hej.tree.tiny.TinyTree))) {
                System.Collections.Generic.IDictionary<string,Saxon.Hej.z.IntSet> localNameIndex = ((Saxon.Hej.tree.tiny.TinyTree)(tree)).getLocalNameIndex();
                Saxon.Hej.z.IntSet intSet = Saxon.Impl.Helpers.MapUtils.Get(localNameIndex, localName);
                if ((intSet == null)) {
                    return Saxon.Hej.z.IntPredicateLambda.of(n => false);
                } else {
                    return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => (intSet.contains((nameCodeArray[nodeNr]&Saxon.Hej.om.NamePool.FP_MASK)) && (((nodeKindArray[nodeNr]&0x0f)) == Saxon.Hej.type.Type.ELEMENT)));
                }
            } else {
                return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => ((((nodeKindArray[nodeNr]&0x0f)) == nodeKind) && localName.Equals(namePool.getLocalName((nameCodeArray[nodeNr]&Saxon.Hej.om.NamePool.FP_MASK)))));
            }
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return (localName.Equals(node.getLocalPart()) && (nodeKind == node.getNodeKind()));
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            return localName.Equals(qname.getLocalPart());
        }
        public override double getDefaultPriority() {
            return -(0.25);
        }
        public string getLocalName() {
            return localName;
        }
        public override int getPrimitiveType() {
            return nodeKind;
        }
        public override string ToString() {
            switch (nodeKind) {
              case Saxon.Hej.type.Type.ELEMENT:
                return ("*:"+localName);
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ("@*:"+localName);
              default:
                return ((("(*"+nodeKind)+"*):")+localName);
            }
        }
        public override int GetHashCode() {
            return ((nodeKind<<20)^localName.GetHashCode());
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.LocalNameTest) && (((Saxon.Hej.pattern.LocalNameTest)(other)).nodeKind == nodeKind)) && ((Saxon.Hej.pattern.LocalNameTest)(other)).localName.Equals(localName));
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return namePool;
        }
        public string exportQNameTest() {
            return ("*:"+localName);
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            return (("q.local==='"+localName)+"'");
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            return Saxon.Ejava.util.Optional<string>.of("The node has the wrong local name");
        }
    
    }
}