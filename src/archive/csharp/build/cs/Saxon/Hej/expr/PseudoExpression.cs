using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class PseudoExpression : Saxon.Hej.expr.Expression {
        private void cannotEvaluate() {
            throw new Saxon.Hej.trans.XPathException(("Cannot evaluate "+this.GetType().Name));
        }
        public override int getImplementationMethod() {
            return 0;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            cannotEvaluate();
            return null;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            cannotEvaluate();
            return false;
        }
        public override Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
            cannotEvaluate();
            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            cannotEvaluate();
            return null;
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            cannotEvaluate();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}