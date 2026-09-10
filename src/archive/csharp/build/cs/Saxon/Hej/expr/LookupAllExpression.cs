using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class LookupAllExpression : Saxon.Hej.expr.UnaryExpression {
        public LookupAllExpression(Saxon.Hej.expr.Expression lhs) : base (lhs) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.INSPECT;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType lhs = getBaseExpression().getItemType();
            if ((lhs is Saxon.Hej.ma.map.MapType)) {
                return ((Saxon.Hej.ma.map.MapType)(lhs)).getValueType().getPrimaryType();
            } else {
                if ((lhs is Saxon.Hej.ma.arrays.ArrayItemType)) {
                    return ((Saxon.Hej.ma.arrays.ArrayItemType)(lhs)).getMemberType().getPrimaryType();
                } else {
                    return Saxon.Hej.type.AnyItemType.getInstance();
                }
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getItemType().getUType();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType containerType = getBaseExpression().getItemType();
            bool isArrayLookup = (containerType is Saxon.Hej.ma.arrays.ArrayItemType);
            bool isMapLookup = ((containerType is Saxon.Hej.ma.map.MapType) || (containerType is Saxon.Hej.ma.map.TupleType));
            if ((!(isArrayLookup) && !(isMapLookup))) {
                if (((th.relationship(containerType, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE) == Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(containerType, Saxon.Hej.ma.arrays.ArrayItemType.getInstance()) == Saxon.Hej.type.Affinity.DISJOINT))) {
                    if (Saxon.Hej.value.Cardinality.allowsZero(getBaseExpression().getCardinality())) {
                        visitor.issueWarning(("The left-hand operand of '?' must be a map or an array; the expression can succeed only if the operand is an empty sequence "+containerType), getLocation());
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("The left-hand operand of '?' must be a map or an array; the supplied expression is of type "+containerType), "XPTY0004");
                        err.setLocation(getLocation());
                        err.setIsTypeError(true);
                        err.setFailingExpression(this);
                        throw err;
                    }
                }
            }
            if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                return new Saxon.Hej.expr.Literal(Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.makeDynamicContext())));
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            getOperand().optimize(visitor, contextItemType);
            if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                return new Saxon.Hej.expr.Literal(Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.makeDynamicContext())));
            }
            if ((getBaseExpression() is Saxon.Hej.ma.arrays.SquareArrayConstructor)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                foreach (Saxon.Hej.expr.Operand o in getBaseExpression().operands()) {
                    children.Add(o.getChildExpression().copy(new Saxon.Hej.expr.parser.RebindingMap()));
                }
                Saxon.Hej.expr.Expression[] childExpressions = children.ToArray();
                Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(childExpressions);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, block);
                return block;
            }
            return this;
        }
        public override double getCost() {
            return (getBaseExpression().getCost()+1);
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.LookupAllExpression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.expr.LookupAllExpression(getBaseExpression().copy(rebindings));
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.expr.LookupAllExpression)))) {
                return false;
            }
            Saxon.Hej.expr.LookupAllExpression p = (Saxon.Hej.expr.LookupAllExpression)(other);
            return getBaseExpression().isEqual(p.getBaseExpression());
        }
        protected internal override int computeHashCode() {
            return ("LookupAll".GetHashCode()^getBaseExpression().GetHashCode());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Inner_SequenceIterator_d867e835(this, context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("lookupAll", this);
            getBaseExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return (Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getBaseExpression())+"?*");
        }
        public override string toShortString() {
            return (getBaseExpression().toShortString()+"?*");
        }
    
        private class Inner_SequenceIterator_d867e835 : Saxon.Hej.om.SequenceIterator {
            private readonly LookupAllExpression outerInstance;
            public Inner_SequenceIterator_d867e835(LookupAllExpression outerInstance, Saxon.Hej.expr.XPathContext context) {
                this.outerInstance = outerInstance;
                this.context = context;
                level0 = /*guess*/outerInstance.getBaseExpression().iterate(/*extraParam*/context);
                level1forArrays = null;
                level1forMaps = null;
                level2 = null;
            }
            private Saxon.Hej.expr.XPathContext context;
            readonly Saxon.Hej.om.SequenceIterator level0;
            internal System.Collections.Generic.IEnumerator<Saxon.Hej.om.GroundedValue> level1forArrays;
            internal System.Collections.Generic.IEnumerator<Saxon.Hej.ma.map.KeyValuePair> level1forMaps;
            internal Saxon.Hej.om.SequenceIterator level2;
            public Saxon.Hej.om.Item next() {
                if ((/*field*/level2 == null)) {
                    if (((/*field*/level1forArrays == null) && (/*field*/level1forMaps == null))) {
                        Saxon.Hej.om.Item lhs = /*field*/level0.next();
                        if ((/*localvar*/lhs == null)) {
                            return null;
                        } else {
                            if ((/*localvar*/lhs is Saxon.Hej.ma.arrays.ArrayItem)) {
                                /*field*/level1forArrays = ((Saxon.Hej.ma.arrays.ArrayItem)(/*localvar*/lhs)).members().GetEnumerator();
                                return /*method*/next();
                            } else {
                                if ((/*localvar*/lhs is Saxon.Hej.ma.map.MapItem)) {
                                    /*field*/level1forMaps = ((Saxon.Hej.ma.map.MapItem)(/*localvar*/lhs)).keyValuePairs().GetEnumerator();
                                    return /*method*/next();
                                } else {
                                    try {
                                        Saxon.Hej.expr.LookupExpression.mustBeArrayOrMap(this.outerInstance, /*localvar*/lhs);
                                    } catch (Saxon.Hej.trans.XPathException e) {
                                        throw new Saxon.Hej.trans.UncheckedXPathException(/*exception*/e);
                                    }
                                    return null;
                                }
                            }
                        }
                    } else {
                        if (((/*field*/level1forArrays != null) && /*field*/level1forArrays.MoveNext() /*UNSAFE CONVERSION*/)) {
                            Saxon.Hej.om.GroundedValue nextEntry = /*field*/level1forArrays.Current;
                            /*field*/level2 = /*localvar*/nextEntry.iterate();
                        } else {
                            if (((/*field*/level1forMaps != null) && /*field*/level1forMaps.MoveNext() /*UNSAFE CONVERSION*/)) {
                                Saxon.Hej.ma.map.KeyValuePair nextEntry = /*field*/level1forMaps.Current;
                                Saxon.Hej.om.GroundedValue value = /*localvar*/nextEntry.value;
                                /*field*/level2 = /*localvar*/value.iterate();
                            } else {
                                /*field*/level1forMaps = null;
                                /*field*/level1forArrays = null;
                            }
                        }
                    }
                    return /*method*/next();
                } else {
                    Saxon.Hej.om.Item nextItem = /*field*/level2.next();
                    if ((/*localvar*/nextItem == null)) {
                        /*field*/level2 = null;
                        return /*method*/next();
                    } else {
                        return /*localvar*/nextItem;
                    }
                }
            }
            public void close() {
                if ((/*field*/level0 != null)) {
                    /*field*/level0.close();
                }
                if ((/*field*/level2 != null)) {
                    /*field*/level2.close();
                }
            }
        }

    }
}