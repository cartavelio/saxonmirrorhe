using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public sealed class NamespaceTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private readonly Saxon.Hej.om.NamePool namePool;
        private readonly int nodeKind;
        private readonly Saxon.Hej.type.UType uType;
        private readonly Saxon.Hej.om.NamespaceUri uri;
        public NamespaceTest(Saxon.Hej.om.NamePool pool, int nodeKind, Saxon.Hej.om.NamespaceUri uri)  {
            namePool = pool;
            this.nodeKind = nodeKind;
            this.uri = uri;
            this.uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
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
            return (((getBasicAlphaCode()+" nQ{")+uri)+"}*");
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return ((name != null) && name.hasURI(uri));
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => {
                int fp = (nameCodeArray[nodeNr]&unchecked((int)0xfffff));
                return (((fp != -1) && (((nodeKindArray[nodeNr]&0x0f)) == nodeKind)) && uri.Equals(namePool.getURI(fp)));
            } );
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return ((node.getNodeKind() == nodeKind) && node.getNamespaceUri().Equals(uri));
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            return qname.hasURI(uri);
        }
        public override double getDefaultPriority() {
            return -(0.25);
        }
        public override int getPrimitiveType() {
            return nodeKind;
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceURI() {
            return uri;
        }
        public override string ToString() {
            switch (nodeKind) {
              case Saxon.Hej.type.Type.ELEMENT:
                return (("Q{"+uri)+"}*");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return (("@Q{"+uri)+"}*");
              default:
                return (((("(*"+nodeKind)+"*)Q{")+uri)+"}*");
            }
        }
        public override int GetHashCode() {
            return (uri.GetHashCode()<<(5+nodeKind));
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.pattern.NamespaceTest) && (((Saxon.Hej.pattern.NamespaceTest)(other)).namePool == namePool)) && (((Saxon.Hej.pattern.NamespaceTest)(other)).nodeKind == nodeKind)) && ((Saxon.Hej.pattern.NamespaceTest)(other)).uri.Equals(uri));
        }
        public string exportQNameTest() {
            return (("Q{"+uri)+"}*");
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            return (("q.uri==='"+Saxon.Hej.trace.ExpressionPresenter.jsEscape(uri.ToString()))+"'");
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            return Saxon.Ejava.util.Optional<string>.of("The node is in the wrong namespace");
        }
    
    }
}