using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class ItemTypePattern : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.type.ItemType itemType;
        public ItemTypePattern(Saxon.Hej.type.ItemType test)  {
            itemType = test;
            setPriority(test.getDefaultPriority());
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return itemType.matches(item, context.getConfiguration().getTypeHierarchy());
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return itemType;
        }
        public override Saxon.Hej.type.UType getUType() {
            return itemType.getUType();
        }
        public override string reconstruct() {
            return itemType.ToString();
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.ItemTypePattern)) && ((Saxon.Hej.pattern.ItemTypePattern)(other)).itemType.Equals(itemType));
        }
        protected internal override int computeHashCode() {
            return (0x7a83d1a8^itemType.GetHashCode());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.nodeTest");
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(itemType));
            presenter.endElement();
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.Pattern n = new Saxon.Hej.pattern.ItemTypePattern(itemType);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
    
    }
}