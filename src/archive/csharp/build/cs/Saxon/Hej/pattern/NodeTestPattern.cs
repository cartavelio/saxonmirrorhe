using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class NodeTestPattern : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        public NodeTestPattern(Saxon.Hej.pattern.NodeTest test)  {
            nodeTest = test;
            setPriority(test.getDefaultPriority());
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return ((item is Saxon.Hej.om.NodeInfo) && nodeTest.test((Saxon.Hej.om.NodeInfo)(item)));
        }
        public override Saxon.Hej.pattern.NodeTest getItemType() {
            return nodeTest;
        }
        public override Saxon.Hej.type.UType getUType() {
            return nodeTest.getUType();
        }
        public override int getFingerprint() {
            return nodeTest.getFingerprint();
        }
        public override string reconstruct() {
            return nodeTest.ToString();
        }
        public override string toShortString() {
            return nodeTest.toShortString();
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.NodeTestPattern)) && ((Saxon.Hej.pattern.NodeTestPattern)(other)).nodeTest.Equals(nodeTest));
        }
        protected internal override int computeHashCode() {
            return (0x7aeffea8^nodeTest.GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            if (((nodeTest is Saxon.Hej.pattern.NameTest) && (nodeTest.getUType() == Saxon.Hej.type.UType.ELEMENT))) {
                Saxon.Hej.type.SchemaDeclaration decl = getConfiguration().getElementDeclaration(nodeTest.getMatchingNodeName());
                if ((decl == null)) {
                    if ("lax".Equals(val)) {
                        return this;
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("The mode specifies typed='strict', "+"but there is no schema element declaration named ")+nodeTest), "XTSE3105");
                    }
                } else {
                    Saxon.Hej.pattern.NodeTest schemaNodeTest = decl.makeSchemaNodeTest();
                    return new Saxon.Hej.pattern.NodeTestPattern(schemaNodeTest);
                }
            } else {
                return this;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.nodeTest");
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(nodeTest));
            presenter.endElement();
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.NodeTestPattern n = new Saxon.Hej.pattern.NodeTestPattern(nodeTest.copy());
            n.setPriority(getDefaultPriority());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public Saxon.Hej.pattern.NodeTest getNodeTest() {
            return nodeTest;
        }
    
    }
}