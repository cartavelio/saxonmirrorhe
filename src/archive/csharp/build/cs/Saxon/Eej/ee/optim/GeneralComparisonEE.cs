using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class GeneralComparisonEE : Saxon.Hej.expr.GeneralComparison {
        public GeneralComparisonEE(Saxon.Hej.expr.Expression p0, int op, Saxon.Hej.expr.Expression p1) : base (p0, op, p1) {
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if (((singletonOperator == Saxon.Hej.expr.parser.Token.FEQ) && !(((getParentExpression() is Saxon.Hej.expr.FilterExpression))))) {
                Saxon.Hej.expr.Expression lhs = getLhsExpression();
                Saxon.Hej.expr.Expression rhs = getRhsExpression();
                Saxon.Hej.expr.Expression e2 = tryIndexedComparison(lhs, rhs);
                if ((e2 != null)) {
                    return e2.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                }
                e2 = tryIndexedComparison(rhs, lhs);
                if ((e2 != null)) {
                    return e2.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                }
            }
            return base.optimize(visitor, contextItemType);
        }
        private Saxon.Hej.expr.Expression tryIndexedComparison(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            if ((((lhs is Saxon.Hej.expr.VariableReference) && Saxon.Hej.value.Cardinality.allowsMany(lhs.getCardinality())) && (((rhs.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) == 0))) {
                Saxon.Hej.expr.Expression predicate = new Saxon.Eej.ee.optim.GeneralComparisonEE(new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.expr.parser.Token.EQUALS, rhs);
                Saxon.Hej.expr.Expression filterExp = new Saxon.Hej.expr.FilterExpression(lhs, predicate);
                return Saxon.Hej.functions.SystemFunction.makeCall("exists", getRetainedStaticContext(), filterExp);
            } else {
                return null;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.optim.GeneralComparisonEE gc = new Saxon.Eej.ee.optim.GeneralComparisonEE(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, gc);
            gc.setRetainedStaticContext(getRetainedStaticContext());
            gc.comparer = comparer;
            gc.singletonOperator = singletonOperator;
            gc.runtimeCheckNeeded = runtimeCheckNeeded;
            gc.comparisonCardinality = comparisonCardinality;
            return gc;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator lhsIter = getLhsExpression().iterate(context);
            Saxon.Hej.om.Item lhsFirst = lhsIter.next();
            if ((lhsFirst == null)) {
                return false;
            }
            Saxon.Hej.om.SequenceIterator rhsIter = getRhsExpression().iterate(context);
            Saxon.Hej.om.Item rhsFirst = rhsIter.next();
            if ((rhsFirst == null)) {
                lhsIter.close();
                return false;
            }
            if (compareOneToOne((Saxon.Hej.value.AtomicValue)(lhsFirst), (Saxon.Hej.value.AtomicValue)(rhsFirst), context)) {
                lhsIter.close();
                rhsIter.close();
                return true;
            }
            Saxon.Hej.om.Item rhsSecond = rhsIter.next();
            if ((rhsSecond == null)) {
                return compareManyToOne(lhsIter, (Saxon.Hej.value.AtomicValue)(rhsFirst), context);
            }
            Saxon.Hej.om.Item lhsSecond = lhsIter.next();
            if ((lhsSecond == null)) {
                if (compareOneToOne((Saxon.Hej.value.AtomicValue)(lhsFirst), (Saxon.Hej.value.AtomicValue)(rhsSecond), context)) {
                    lhsIter.close();
                    rhsIter.close();
                    return true;
                }
                return compareOneToMany((Saxon.Hej.value.AtomicValue)(lhsFirst), rhsIter, context);
            }
            return hashJoin(prependTwo(lhsFirst, lhsSecond, lhsIter), prependTwo(rhsFirst, rhsSecond, rhsIter), context, comparer);
        }
        private static Saxon.Hej.om.SequenceIterator prependTwo(Saxon.Hej.om.Item first, Saxon.Hej.om.Item second, Saxon.Hej.om.SequenceIterator rest) {
            return new Saxon.Hej.functions.InsertBefore.InsertIterator(rest, new Saxon.Hej.tree.iter.TwoItemIterator(first, second), 1);
        }
        private bool compareOneToOne(Saxon.Hej.value.AtomicValue lhsValue, Saxon.Hej.value.AtomicValue rhsValue, Saxon.Hej.expr.XPathContext context) {
            if ((runtimeCheckNeeded && !(Saxon.Hej.type.UType.isGenerallyComparable(lhsValue.getUType(), rhsValue.getUType())))) {
                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(((("Cannot compare "+Saxon.Hej.type.Type.displayTypeName(lhsValue))+" to ")+Saxon.Hej.type.Type.displayTypeName(rhsValue)));
                e2.setErrorCode("XPTY0004");
                e2.setLocation(getLocation());
                e2.setIsTypeError(true);
                throw e2;
            }
            return compare(lhsValue, singletonOperator, rhsValue, comparer.provideContext(context), false, context, getRetainedStaticContext());
        }
        private bool compareManyToOne(Saxon.Hej.om.SequenceIterator lhsIter, Saxon.Hej.value.AtomicValue rhsValue, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue lhsValue;
            while (((lhsValue = (Saxon.Hej.value.AtomicValue)(lhsIter.next())) != null)) {
                if (compareOneToOne(lhsValue, rhsValue, context)) {
                    lhsIter.close();
                    return true;
                }
            }
            return false;
        }
        private bool compareOneToMany(Saxon.Hej.value.AtomicValue lhsValue, Saxon.Hej.om.SequenceIterator rhsIter, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue rhsValue;
            while (((rhsValue = (Saxon.Hej.value.AtomicValue)(rhsIter.next())) != null)) {
                if (compareOneToOne(lhsValue, rhsValue, context)) {
                    rhsIter.close();
                    return true;
                }
            }
            return false;
        }
        private bool hashJoin(Saxon.Hej.om.SequenceIterator iter1, Saxon.Hej.om.SequenceIterator iter2, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer) {
            Saxon.Hej.value.AtomicValue first1 = (Saxon.Hej.value.AtomicValue)(iter1.next());
            if ((first1 == null)) {
                iter2.close();
                return false;
            }
            Saxon.Hej.value.AtomicValue first2 = (Saxon.Hej.value.AtomicValue)(iter2.next());
            if ((first2 == null)) {
                iter1.close();
                return false;
            }
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.expr.sort.AtomicComparer boundComparer = comparer.provideContext(context);
            Saxon.Hej.type.UType ut1 = first1.getItemType().getUType();
            if (Saxon.Hej.type.UType.NUMERIC.subsumes(ut1)) {
                ut1 = Saxon.Hej.type.UType.NUMERIC;
            }
            Saxon.Hej.type.UType ut2 = first2.getItemType().getUType();
            if (Saxon.Hej.type.UType.NUMERIC.subsumes(ut2)) {
                ut2 = Saxon.Hej.type.UType.NUMERIC;
            }
            Saxon.Hej.type.UType hashTableType;
            if (ut1.Equals(ut2)) {
                hashTableType = ut1;
            } else {
                if (ut1.Equals(Saxon.Hej.type.UType.UNTYPED_ATOMIC)) {
                    hashTableType = ut2;
                } else {
                    hashTableType = ut1;
                }
            }
            Saxon.Hej.type.StringConverter untypedConverter;
            if (hashTableType.Equals(Saxon.Hej.type.UType.NUMERIC)) {
                untypedConverter = rules.getStringToDoubleConverter();
            } else {
                untypedConverter = ((Saxon.Hej.type.BuiltInAtomicType)(hashTableType.toItemType())).getStringConverter(rules);
            }
            Saxon.Hej.lib.StringCollator collator = comparer.getCollator();
            int implicitTimezone = context.getImplicitTimezone();
            System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> set1 = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(20);
            System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> set2 = null;
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> strays1 = null;
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> strays2 = null;
            bool exhausted1 = false;
            bool exhausted2 = false;
            while (true) {
                if (!(exhausted1)) {
                    Saxon.Hej.type.UType t1 = ut1;
                    Saxon.Hej.value.AtomicValue v1 = first1;
                    if (((t1 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) && (hashTableType != Saxon.Hej.type.UType.UNTYPED_ATOMIC))) {
                        t1 = hashTableType;
                        Saxon.Hej.type.ConversionResult result = untypedConverter.convertString(v1.getUnicodeStringValue());
                        if ((result is Saxon.Hej.type.ValidationFailure)) {
                            string msg = ((("Failure converting untypedAtomic value to "+hashTableType)+" during comparison operation. ")+((Saxon.Hej.type.ValidationFailure)(result)).getMessage());
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, "FORG0001", context);
                            err.setLocation(getLocation());
                            throw err;
                        } else {
                            v1 = (Saxon.Hej.value.AtomicValue)(result);
                        }
                    }
                    if ((t1 == hashTableType)) {
                        Saxon.Hej.expr.sort.AtomicMatchKey k1 = v1.getXPathMatchKey(collator, implicitTimezone);
                        if (((set2 != null) && set2.Contains(k1))) {
                            iter1.close();
                            iter2.close();
                            return true;
                        } else {
                            if (!(exhausted2)) {
                                set1.Add(k1);
                            }
                        }
                    } else {
                        if (((hashTableType != Saxon.Hej.type.UType.UNTYPED_ATOMIC) && !(Saxon.Hej.type.UType.isPossiblyComparable(t1, hashTableType, false)))) {
                            string msg = ((("Cannot compare "+t1)+" to ")+hashTableType);
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, "XPTY0004", context);
                            err.setLocation(getLocation());
                            throw err;
                        }
                    }
                    if ((ut1 != hashTableType)) {
                        if ((strays2 != null)) {
                            foreach (Saxon.Hej.value.AtomicValue stray2 in strays2) {
                                try {
                                    if (compare(first1, Saxon.Hej.expr.parser.Token.FEQ, stray2, boundComparer, false, context, getRetainedStaticContext())) {
                                        return true;
                                    }
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    Saxon.Hej.type.BuiltInAtomicType it1 = first1.getPrimitiveType();
                                    Saxon.Hej.type.BuiltInAtomicType it2 = stray2.getPrimitiveType();
                                    Saxon.Hej.trans.XPathException err;
                                    if (it1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                        err = new Saxon.Hej.trans.XPathException((("Cannot convert untypedAtomic value to type of second operand ("+it2)+")"));
                                        err.setErrorCode("FORG0001");
                                    } else {
                                        if (it2.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                            err = new Saxon.Hej.trans.XPathException((("Cannot convert untypedAtomic value to type of first operand ("+it1)+")"));
                                            err.setErrorCode("FORG0001");
                                        } else {
                                            err = new Saxon.Hej.trans.XPathException(((("Cannot compare "+it1)+" to ")+it2));
                                            err.setErrorCode("XPTY0004");
                                        }
                                    }
                                    err.setXPathContext(context);
                                    err.setLocation(getLocation());
                                    throw err;
                                }
                            }
                        }
                    }
                    if (((ut1 != hashTableType) || (ut1 == Saxon.Hej.type.UType.UNTYPED_ATOMIC))) {
                        if (!(exhausted2)) {
                            if ((strays1 == null)) {
                                strays1 = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(10);
                            }
                            strays1.Add(first1);
                        }
                    }
                    first1 = (Saxon.Hej.value.AtomicValue)(iter1.next());
                    if ((first1 == null)) {
                        exhausted1 = true;
                    } else {
                        ut1 = first1.getItemType().getUType();
                        if (Saxon.Hej.type.UType.NUMERIC.subsumes(ut1)) {
                            ut1 = Saxon.Hej.type.UType.NUMERIC;
                        }
                    }
                }
                if (!(exhausted2)) {
                    Saxon.Hej.type.UType t2 = ut2;
                    Saxon.Hej.value.AtomicValue v2 = first2;
                    if (((t2 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) && (hashTableType != Saxon.Hej.type.UType.UNTYPED_ATOMIC))) {
                        t2 = hashTableType;
                        Saxon.Hej.type.ConversionResult result = untypedConverter.convertString(v2.getUnicodeStringValue());
                        if ((result is Saxon.Hej.type.ValidationFailure)) {
                            string msg = ((("Failure converting untypedAtomic value to "+hashTableType)+" during comparison operation. ")+((Saxon.Hej.type.ValidationFailure)(result)).getMessage());
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(msg, "FORG0001", context);
                            de.setLocation(getLocation());
                            throw de;
                        } else {
                            v2 = (Saxon.Hej.value.AtomicValue)(result);
                        }
                    }
                    if ((t2 == hashTableType)) {
                        Saxon.Hej.expr.sort.AtomicMatchKey k2 = v2.getXPathMatchKey(collator, implicitTimezone);
                        if (set1.Contains(k2)) {
                            iter1.close();
                            iter2.close();
                            return true;
                        } else {
                            if (!(exhausted1)) {
                                if ((set2 == null)) {
                                    set2 = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(20);
                                }
                                set2.Add(k2);
                            }
                        }
                    }
                    if ((ut2 != hashTableType)) {
                        if ((strays1 != null)) {
                            foreach (Saxon.Hej.value.AtomicValue stray1 in strays1) {
                                try {
                                    if (compare(first2, Saxon.Hej.expr.parser.Token.FEQ, stray1, boundComparer, false, context, getRetainedStaticContext())) {
                                        return true;
                                    }
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    Saxon.Hej.type.BuiltInAtomicType it2 = first2.getPrimitiveType();
                                    Saxon.Hej.type.BuiltInAtomicType it1 = stray1.getPrimitiveType();
                                    Saxon.Hej.trans.XPathException err;
                                    if (it1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                        err = new Saxon.Hej.trans.XPathException((("Cannot convert untypedAtomic value to type of second operand ("+it2)+")"));
                                        err.setErrorCode("FORG0001");
                                    } else {
                                        if (it2.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                            err = new Saxon.Hej.trans.XPathException((("Cannot convert untypedAtomic value to type of first operand ("+it1)+")"));
                                            err.setErrorCode("FORG0001");
                                        } else {
                                            err = new Saxon.Hej.trans.XPathException(((("Cannot compare "+it1)+" to ")+it2));
                                            err.setErrorCode("XPTY0004");
                                        }
                                    }
                                    err.setXPathContext(context);
                                    err.setLocation(getLocation());
                                    throw err;
                                }
                            }
                        }
                    }
                    if (((ut2 != hashTableType) || (ut2 == Saxon.Hej.type.UType.UNTYPED_ATOMIC))) {
                        if (!(exhausted1)) {
                            if ((strays2 == null)) {
                                strays2 = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(10);
                            }
                            strays2.Add(first2);
                        }
                    }
                    first2 = (Saxon.Hej.value.AtomicValue)(iter2.next());
                    if ((first2 == null)) {
                        exhausted2 = true;
                    } else {
                        ut2 = first2.getItemType().getUType();
                        if (Saxon.Hej.type.UType.NUMERIC.subsumes(ut2)) {
                            ut2 = Saxon.Hej.type.UType.NUMERIC;
                        }
                    }
                }
                if ((exhausted1 && exhausted2)) {
                    return false;
                }
            }
        }
        protected internal override Saxon.Hej.expr.GeneralComparison getInverseComparison() {
            Saxon.Eej.ee.optim.GeneralComparisonEE gc = new Saxon.Eej.ee.optim.GeneralComparisonEE(getRhsExpression(), Saxon.Hej.expr.parser.Token.inverse(@operator), getLhsExpression());
            gc.setRetainedStaticContext(getRetainedStaticContext());
            return gc;
        }
        protected internal override string tag() {
            return "gcEE";
        }
    
    }
}