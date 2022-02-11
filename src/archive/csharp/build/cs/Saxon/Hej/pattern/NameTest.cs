using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class NameTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.QNameTest {
        private readonly int nodeKind;
        private readonly int fingerprint;
        private readonly Saxon.Hej.type.UType uType;
        private readonly Saxon.Hej.om.NamePool namePool;
        private string uri = null;
        private string localName = null;
        public NameTest(int nodeKind, string uri, string localName, Saxon.Hej.om.NamePool namePool)  {
            this.uri = uri;
            this.localName = localName;
            this.nodeKind = nodeKind;
            this.fingerprint = (namePool.allocateFingerprint(uri, localName)&Saxon.Hej.om.NamePool.FP_MASK);
            this.namePool = namePool;
            this.uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
        }
        public NameTest(int nodeKind, int nameCode, Saxon.Hej.om.NamePool namePool)  {
            this.nodeKind = nodeKind;
            this.fingerprint = (nameCode&Saxon.Hej.om.NamePool.FP_MASK);
            this.namePool = namePool;
            this.uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
        }
        public NameTest(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.om.NamePool pool)  {
            this.uri = name.getURI();
            this.localName = name.getLocalPart();
            this.nodeKind = nodeKind;
            this.fingerprint = name.obtainFingerprint(pool);
            this.namePool = pool;
            this.uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return namePool;
        }
        public int getNodeKind() {
            return nodeKind;
        }
        public override Saxon.Hej.type.UType getUType() {
            return uType;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            if ((nodeKind != this.nodeKind)) {
                return false;
            }
            if (name.hasFingerprint()) {
                return (name.getFingerprint() == this.fingerprint);
            } else {
                computeUriAndLocal();
                return (name.hasURI(uri) && name.getLocalPart().Equals(localName));
            }
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => ((((nameCodeArray[nodeNr]&unchecked((int)0xfffff))) == fingerprint) && (((nodeKindArray[nodeNr]&0x0f)) == nodeKind)));
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            if ((node.getNodeKind() != nodeKind)) {
                return false;
            }
            if (node.hasFingerprint()) {
                return (node.getFingerprint() == fingerprint);
            } else {
                computeUriAndLocal();
                return (localName.Equals(node.getLocalPart()) && uri.Equals(node.getURI()));
            }
        }
        private void computeUriAndLocal() {
            if (((uri == null) || (localName == null))) {
                Saxon.Hej.om.StructuredQName name = namePool.getUnprefixedQName(fingerprint);
                uri = name.getURI();
                localName = name.getLocalPart();
            }
        }
        public bool matches(Saxon.Hej.om.StructuredQName qname) {
            computeUriAndLocal();
            return (qname.getLocalPart().Equals(localName) && qname.hasURI(uri));
        }
        public override double getDefaultPriority() {
            return 0.0;
        }
        public override int getFingerprint() {
            return fingerprint;
        }
        public override Saxon.Hej.om.StructuredQName getMatchingNodeName() {
            computeUriAndLocal();
            return new Saxon.Hej.om.StructuredQName("", uri, localName);
        }
        public override int getPrimitiveType() {
            return nodeKind;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(new Saxon.Hej.z.IntSingletonSet(fingerprint));
        }
        public string getNamespaceURI() {
            computeUriAndLocal();
            return uri;
        }
        public string getLocalPart() {
            computeUriAndLocal();
            return localName;
        }
        public override string ToString() {
            switch (nodeKind) {
              case Saxon.Hej.type.Type.ELEMENT:
                return (("element("+namePool.getEQName(fingerprint))+")");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return (("attribute("+namePool.getEQName(fingerprint))+")");
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return (("processing-instruction("+namePool.getLocalName(fingerprint))+')');
              case Saxon.Hej.type.Type.NAMESPACE:
                return (("namespace-node("+namePool.getLocalName(fingerprint))+')');
            }
            return namePool.getEQName(fingerprint);
        }
        public override int GetHashCode() {
            return ((nodeKind<<20)^fingerprint);
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.pattern.NameTest) && (((Saxon.Hej.pattern.NameTest)(other)).namePool == namePool)) && (((Saxon.Hej.pattern.NameTest)(other)).nodeKind == nodeKind)) && (((Saxon.Hej.pattern.NameTest)(other)).fingerprint == fingerprint));
        }
        public override string getFullAlphaCode() {
            return ((getBasicAlphaCode()+" n")+getMatchingNodeName().getEQName());
        }
        public string exportQNameTest() {
            return getMatchingNodeName().getEQName();
        }
        public string generateJavaScriptNameTest(int targetVersion) {
            computeUriAndLocal();
            return (((("q.uri==='"+Saxon.Hej.trace.ExpressionPresenter.jsEscape(uri))+"'&&q.local==='")+localName)+"'");
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            return Saxon.Ejava.util.Optional<string>.of("The node has the wrong name");
        }
        public override string toShortString() {
            switch (nodeKind) {
              case Saxon.Hej.type.Type.ELEMENT:
                return (getNamespaceURI().Length==0 ? namePool.getLocalName(getFingerprint()) : ToString());
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ("@"+((getNamespaceURI().Length==0 ? namePool.getLocalName(getFingerprint()) : ToString())));
              default:
                return ToString();
            }
        }
    
    }
}