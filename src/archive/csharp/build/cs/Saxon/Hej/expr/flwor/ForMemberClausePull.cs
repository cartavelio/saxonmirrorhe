using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForMemberClausePull : Saxon.Hej.expr.flwor.ForClausePull {
        internal Saxon.Hej.ma.arrays.ArrayItem arrayItem;
        public ForMemberClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.ForMemberClause forClause) : base (@base, forClause) {
        }
        protected internal override Saxon.Hej.om.SequenceIterator getIterator(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression sequence = forClause.getSequence();
            arrayItem = (Saxon.Hej.ma.arrays.ArrayItem)(sequence.evaluateItem(context));
            System.Collections.Generic.IEnumerator<Saxon.Hej.om.GroundedValue> members = arrayItem.members().GetEnumerator();
            return new Saxon.Hej.tree.iter.SequenceIteratorOverJavaIterator<Saxon.Hej.om.GroundedValue>(members, mem => new Saxon.Hej.value.ObjectValue<Saxon.Hej.om.GroundedValue>(mem));
        }
        protected internal override Saxon.Hej.om.GroundedValue variableValue(Saxon.Hej.om.Item item) {
            return ((Saxon.Hej.value.ObjectValue<Saxon.Hej.om.GroundedValue>)(item)).getObject();
        }
    
    }
}