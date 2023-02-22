using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public abstract class BasicUpdatingExpression : Saxon.Hej.expr.Expression {
        public override bool isUpdatingExpression() {
            return true;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override int getImplementationMethod() {
            return UPDATE_METHOD;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot evaluate an updating expression");
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot evaluate an updating expression");
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot evaluate an updating expression");
        }
    
    }
}