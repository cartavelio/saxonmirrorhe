using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class MinimaxStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.MinimaxStreamer.MinimaxFeed(getExpression(), @out, context);
        }
        private class MinimaxFeed : Saxon.Eej.ee.stream.feed.AtomicItemFeed {
            private Saxon.Hej.value.AtomicValue min;
            private Saxon.Hej.expr.sort.AtomicComparer atomicComparer;
            private bool foundDouble;
            private bool foundFloat;
            private bool foundNaN;
            private Saxon.Hej.type.AtomicType lowestCommonSuperType;
            private bool ignoreNaN;
            public MinimaxFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SystemFunction fn = call.getTargetFunction();
                ignoreNaN = ((fn is Saxon.Hej.functions.Minimax) && ((Saxon.Hej.functions.Minimax)(call.getTargetFunction())).isIgnoreNaN());
                min = null;
                foundDouble = false;
                foundFloat = false;
                foundNaN = false;
                lowestCommonSuperType = null;
                getResultFeed().open(terminator);
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(hasFailed())) {
                    Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(item);
                    Saxon.Hej.type.TypeHierarchy th = getContext().getConfiguration().getTypeHierarchy();
                    if (value.isUntypedAtomic()) {
                        try {
                            Saxon.Hej.type.StringToDouble converter = getContext().getConfiguration().getConversionRules().getStringToDoubleConverter();
                            value = new Saxon.Hej.value.DoubleValue(converter.stringToNumber(value.getUnicodeStringValue()));
                            foundDouble = true;
                        } catch (Saxon.Ejava.lang.NumberFormatException e) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Failure converting "+Saxon.Hej.trans.Err.wrap(value.getUnicodeStringValue()))+" to a number"));
                            de.setErrorCode("FORG0001");
                            de.setXPathContext(getContext());
                            dynamicError(de);
                            return ;
                        }
                    } else {
                        if ((value is Saxon.Hej.value.DoubleValue)) {
                            foundDouble = true;
                        } else {
                            if ((value is Saxon.Hej.value.FloatValue)) {
                                foundFloat = true;
                            }
                        }
                    }
                    if (value.isNaN()) {
                        if (ignoreNaN) {
                            return ;
                        } else {
                            foundNaN = true;
                        }
                    } else {
                        if (!(value.getPrimitiveType().isOrdered(false))) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Type "+value.getPrimitiveType())+" is not an ordered type"));
                            de.setErrorCode("FORG0006");
                            de.setIsTypeError(true);
                            de.setXPathContext(getContext());
                            dynamicError(de);
                            return ;
                        }
                    }
                    if ((min == null)) {
                        min = value;
                        lowestCommonSuperType = value.getItemType();
                    } else {
                        if ((atomicComparer == null)) {
                            Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                            Saxon.Hej.functions.SystemFunction targetFunction = call.getTargetFunction();
                            bool isMax = false;
                            if ((targetFunction is Saxon.Hej.functions.Minimax)) {
                                atomicComparer = ((Saxon.Hej.functions.Minimax)(targetFunction)).getAtomicComparer(getContext());
                                isMax = (targetFunction is Saxon.Hej.functions.Minimax.Max);
                            } else {
                                System.Diagnostics.Debug.Assert((targetFunction is Saxon.Hej.functions.CollatingFunctionFree));
                                Saxon.Hej.expr.Expression collationArg = call.getArg((call.getArity()-1));
                                string collationName = collationArg.evaluateAsString(getContext()).ToString();
                                Saxon.Hej.lib.StringCollator collator = getContext().getConfiguration().getCollation(collationName);
                                if ((collator == null)) {
                                    dynamicError(new Saxon.Hej.trans.XPathException(("Unknown collation "+collationName), "FOCH0002"));
                                }
                                atomicComparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, getContext());
                                isMax = targetFunction.getFunctionName().getLocalPart().Equals("max");
                            }
                            if (isMax) {
                                atomicComparer = new Saxon.Hej.expr.sort.DescendingComparer(atomicComparer);
                            }
                        }
                        lowestCommonSuperType = (Saxon.Hej.type.AtomicType)(Saxon.Hej.type.Type.getCommonSuperType(lowestCommonSuperType, value.getItemType(), th));
                        try {
                            if ((atomicComparer.compareAtomicValues(value, min)<0)) {
                                min = value;
                            }
                        } catch (Saxon.Hej.trans.XPathException err) {
                            dynamicError(err);
                        } catch (System.InvalidCastException err) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(((("Cannot compare "+min.getItemType())+" with ")+value.getItemType()));
                            de.setErrorCode("FORG0006");
                            de.setIsTypeError(true);
                            de.setXPathContext(getContext());
                            dynamicError(de);
                        }
                    }
                }
            }
            public override void close() {
                if (!(hasFailed())) {
                    if ((min == null)) {
                    } else {
                        if (foundNaN) {
                            min = Saxon.Hej.value.FloatValue.NaN;
                        }
                        Saxon.Hej.lib.ConversionRules rules = getContext().getConfiguration().getConversionRules();
                        if (foundDouble) {
                            if (!(((min is Saxon.Hej.value.DoubleValue)))) {
                                min = Saxon.Hej.type.Converter.convert(min, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, rules);
                            }
                        } else {
                            if (foundFloat) {
                                if (!(((min is Saxon.Hej.value.FloatValue)))) {
                                    min = Saxon.Hej.type.Converter.convert(min, Saxon.Hej.type.BuiltInAtomicType.FLOAT, rules);
                                }
                            }
                        }
                        if ((lowestCommonSuperType != Saxon.Hej.type.NumericType.getInstance())) {
                            min = Saxon.Hej.type.Converter.convert(min, lowestCommonSuperType, rules);
                        }
                        getNextOutputter().append(min);
                    }
                    getNextOutputter().close();
                }
            }
        
        }
    
    }
}