using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class UniversalPattern : Saxon.Hej.pattern.Pattern {
        public UniversalPattern()  {
            setPriority(-1);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return true;
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ANY;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override int getFingerprint() {
            return -1;
        }
        public override string reconstruct() {
            return ".";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.any");
            presenter.endElement();
        }
        public override bool Equals(System.Object other) {
            return (other is Saxon.Hej.pattern.UniversalPattern);
        }
        protected internal override int computeHashCode() {
            return 0x7aeccea8;
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return this;
        }
    
    }
}