using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class FilterIterator : Saxon.Hej.om.SequenceIterator {
        protected internal Saxon.Hej.om.FocusIterator @base;
        protected internal Saxon.Hej.expr.Expression filter;
        protected internal Saxon.Hej.expr.XPathContext filterContext;
        public FilterIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.Expression filter, Saxon.Hej.expr.XPathContext context)  {
            this.filter = filter;
            filterContext = context.newMinorContext();
            this.@base = filterContext.trackFocus(@base);
        }
        public void setSequence(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.XPathContext context) {
            filterContext = context.newMinorContext();
            this.@base = filterContext.trackFocus(@base);
        }
        public Saxon.Hej.om.Item next() {
            try {
                return getNextMatchingItem();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        protected internal Saxon.Hej.om.Item getNextMatchingItem() {
            Saxon.Hej.om.Item next;
            while (((next = @base.next()) != null)) {
                if (matches()) {
                    return next;
                }
            }
            return null;
        }
        protected internal virtual bool matches() {
            Saxon.Hej.om.SequenceIterator iterator = filter.iterate(filterContext);
            return testPredicateValue(iterator, @base.position(), filter);
        }
        public static bool testPredicateValue(Saxon.Hej.om.SequenceIterator iterator, long position, Saxon.Hej.expr.Expression filter) {
            Saxon.Hej.om.Item first = iterator.next();
            if ((first == null)) {
                return false;
            }
            if ((first is Saxon.Hej.om.NodeInfo)) {
                return true;
            } else {
                if ((first is Saxon.Hej.value.BooleanValue)) {
                    if ((iterator.next() != null)) {
                        Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with a boolean", filter);
                    }
                    return ((Saxon.Hej.value.BooleanValue)(first)).getBooleanValue();
                } else {
                    if ((first is Saxon.Hej.value.StringValue)) {
                        if ((iterator.next() != null)) {
                            Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with a string", filter);
                        }
                        return !(((Saxon.Hej.value.StringValue)(first)).isEmpty());
                    } else {
                        if ((first is Saxon.Hej.value.Int64Value)) {
                            if ((iterator.next() != null)) {
                                Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with a numeric value", filter);
                            }
                            return (((Saxon.Hej.value.Int64Value)(first)).longValue() == position);
                        } else {
                            if ((first is Saxon.Hej.value.NumericValue)) {
                                if ((iterator.next() != null)) {
                                    Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with a numeric value", filter);
                                }
                                return (((Saxon.Hej.value.NumericValue)(first)).compareTo(position) == 0);
                            } else {
                                if ((first is Saxon.Hej.value.AtomicValue)) {
                                    Saxon.Hej.expr.parser.ExpressionTool.ebvError((((("a sequence starting with an atomic value of type "+((Saxon.Hej.value.AtomicValue)(first)).getPrimitiveType().getDisplayName())+" (")+first.toShortString())+")"), filter);
                                    return false;
                                } else {
                                    Saxon.Hej.expr.parser.ExpressionTool.ebvError((((("a sequence starting with "+Saxon.Hej.trans.Err.describeGenre(first.getGenre()))+" (")+first.toShortString())+")"), filter);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
        public sealed class NonNumeric : Saxon.Hej.expr.FilterIterator {
            public NonNumeric(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.Expression filter, Saxon.Hej.expr.XPathContext context) : base (@base, filter, context) {
            }
            protected internal override bool matches() {
                return this.filter.effectiveBooleanValue(this.filterContext);
            }
        
        }
    
    }
}