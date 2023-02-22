using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class CardinalityCheckingIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private readonly Saxon.Hej.s9api.Location locator;
        private Saxon.Hej.om.Item first = null;
        private Saxon.Hej.om.Item second = null;
        private int position = 0;
        public CardinalityCheckingIterator(Saxon.Hej.om.SequenceIterator @base, int requiredCardinality, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.s9api.Location locator)  {
            this.@base = @base;
            this.locator = locator;
            try {
                first = @base.next();
                if ((first == null)) {
                    if (!(Saxon.Hej.value.Cardinality.allowsZero(requiredCardinality))) {
                        typeError(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode());
                    }
                } else {
                    if ((requiredCardinality == Saxon.Hej.expr.StaticProperty.EMPTY)) {
                        typeError((("The only value allowed for the "+role.getMessage())+" is an empty sequence"), role.getErrorCode());
                    }
                    second = @base.next();
                    if (((second != null) && !(Saxon.Hej.value.Cardinality.allowsMany(requiredCardinality)))) {
                        Saxon.Hej.om.Item[] leaders = new Saxon.Hej.om.Item[]{first, second};
                        typeError((("A sequence of more than one item is not allowed as the "+role.getMessage())+Saxon.Hej.expr.CardinalityChecker.depictSequenceStart(new Saxon.Hej.tree.iter.TwoItemIterator(first, second), 2)), role.getErrorCode());
                    }
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public Saxon.Hej.om.Item next() {
            if ((position<2)) {
                if ((position == 0)) {
                    Saxon.Hej.om.Item current = first;
                    position = ((first == null) ? -1 : 1);
                    return current;
                } else {
                    if ((position == 1)) {
                        Saxon.Hej.om.Item current = second;
                        position = ((second == null) ? -1 : 2);
                        return current;
                    } else {
                        return null;
                    }
                }
            }
            Saxon.Hej.om.Item nextBase = @base.next();
            if ((nextBase == null)) {
                position = -1;
            } else {
                position++;
            }
            return nextBase;
        }
        public void close() {
            @base.close();
        }
        private void typeError(string message, string errorCode) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, errorCode, locator);
            e.setIsTypeError(!(errorCode.StartsWith("FORG")));
            throw e;
        }
    
    }
}