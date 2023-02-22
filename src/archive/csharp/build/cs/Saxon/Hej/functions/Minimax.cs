using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class Minimax : Saxon.Hej.functions.CollatingFunctionFixed {
        private Saxon.Hej.type.PlainType argumentType = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        private bool ignoreNaN = false;
        public abstract bool isMaxFunction();
        public void setIgnoreNaN(bool ignore) {
            ignoreNaN = ignore;
        }
        public bool isIgnoreNaN() {
            return ignoreNaN;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getComparer() {
            return getPreAllocatedAtomicComparer();
        }
        public Saxon.Hej.type.PlainType getArgumentType() {
            return argumentType;
        }
        public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.type.ItemType type = arguments[0].getItemType();
            argumentType = type.getAtomizedItemType();
            if ((argumentType is Saxon.Hej.type.AtomicType)) {
                if ((argumentType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    argumentType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                }
                preAllocateComparer((Saxon.Hej.type.AtomicType)(argumentType), (Saxon.Hej.type.AtomicType)(argumentType), visitor.getStaticContext());
            }
        }
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.TypeHierarchy th = getRetainedStaticContext().getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType @base = Saxon.Hej.expr.Atomizer.getAtomizedItemType(args[0], false, th);
            if (@base.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                @base = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
            return @base.getPrimitiveItemType();
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            if (!(Saxon.Hej.value.Cardinality.allowsZero(arguments[0].getCardinality()))) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            int card = arguments[0].getCardinality();
            if (!(Saxon.Hej.value.Cardinality.allowsMany(card))) {
                Saxon.Hej.type.ItemType it = arguments[0].getItemType().getPrimitiveItemType();
                if (((it is Saxon.Hej.type.BuiltInAtomicType) && ((Saxon.Hej.type.BuiltInAtomicType)(it)).isOrdered(false))) {
                    Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                    if ((th.relationship(it, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)) {
                        return Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(visitor.getConfiguration(), arguments[0], Saxon.Hej.type.BuiltInAtomicType.DOUBLE).typeCheck(visitor, contextInfo);
                    } else {
                        return arguments[0];
                    }
                }
            }
            if ((arguments[0] is Saxon.Hej.expr.RangeExpression)) {
                if (isMaxFunction()) {
                    Saxon.Hej.expr.Expression start = ((Saxon.Hej.expr.RangeExpression)(arguments[0])).getStartExpression();
                    Saxon.Hej.expr.Expression end = ((Saxon.Hej.expr.RangeExpression)(arguments[0])).getEndExpression();
                    if (((start is Saxon.Hej.expr.Literal) && (end is Saxon.Hej.expr.Literal))) {
                        return end;
                    }
                    return new Saxon.Hej.expr.LastItemExpression(arguments[0]);
                } else {
                    return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(arguments[0]);
                }
            }
            return null;
        }
        public override Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.AtomicComparer comparer = getPreAllocatedAtomicComparer();
            if ((comparer != null)) {
                return comparer;
            }
            Saxon.Hej.type.PlainType type = ((/*1430*/Saxon.Hej.type.AtomicType)argumentType.getPrimitiveItemType());
            if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                type = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
            Saxon.Hej.type.BuiltInAtomicType prim = (Saxon.Hej.type.BuiltInAtomicType)(type);
            return Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer(prim, prim, getStringCollator(), context);
        }
        public static Saxon.Hej.value.AtomicValue minimax(Saxon.Hej.om.SequenceIterator iter, bool isMaxFunction, Saxon.Hej.expr.sort.AtomicComparer atomicComparer, bool ignoreNaN, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.type.StringToDouble converter = context.getConfiguration().getConversionRules().getStringToDoubleConverter();
            bool foundDouble = false;
            bool foundFloat = false;
            bool foundNaN = false;
            bool foundString = false;
            if (isMaxFunction) {
                atomicComparer = new Saxon.Hej.expr.sort.DescendingComparer(atomicComparer);
            }
            atomicComparer = atomicComparer.provideContext(context);
            Saxon.Hej.value.AtomicValue min;
            Saxon.Hej.value.AtomicValue prim;
            while (true) {
                min = (Saxon.Hej.value.AtomicValue)(iter.next());
                if ((min == null)) {
                    return null;
                }
                prim = min;
                if (min.isUntypedAtomic()) {
                    try {
                        min = new Saxon.Hej.value.DoubleValue(converter.stringToNumber(min.getUnicodeStringValue()));
                        prim = min;
                        foundDouble = true;
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Failure converting "+Saxon.Hej.trans.Err.wrap(min.getUnicodeStringValue()))+" to a number"));
                        de.setErrorCode("FORG0001");
                        de.setXPathContext(context);
                        throw de;
                    }
                } else {
                    if ((prim is Saxon.Hej.value.DoubleValue)) {
                        foundDouble = true;
                    } else {
                        if ((prim is Saxon.Hej.value.FloatValue)) {
                            foundFloat = true;
                        } else {
                            if (((prim is Saxon.Hej.value.StringValue) && !(((prim is Saxon.Hej.value.AnyURIValue))))) {
                                foundString = true;
                            }
                        }
                    }
                }
                if (prim.isNaN()) {
                    if (ignoreNaN) {
                    } else {
                        if ((prim is Saxon.Hej.value.DoubleValue)) {
                            return min;
                        } else {
                            foundNaN = true;
                            min = Saxon.Hej.value.FloatValue.NaN;
                            break ;
                        }
                    }
                } else {
                    if (!(prim.getPrimitiveType().isOrdered(false))) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Type "+prim.getPrimitiveType())+" is not an ordered type"));
                        de.setErrorCode("FORG0006");
                        de.setIsTypeError(true);
                        de.setXPathContext(context);
                        throw de;
                    }
                    break ;
                }
            }
            while (true) {
                Saxon.Hej.value.AtomicValue test = (Saxon.Hej.value.AtomicValue)(iter.next());
                if ((test == null)) {
                    break ;
                }
                Saxon.Hej.value.AtomicValue test2 = test;
                prim = test2;
                if (test.isUntypedAtomic()) {
                    try {
                        test2 = new Saxon.Hej.value.DoubleValue(converter.stringToNumber(test.getUnicodeStringValue()));
                        if (foundNaN) {
                            return Saxon.Hej.value.DoubleValue.NaN;
                        }
                        prim = test2;
                        foundDouble = true;
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Failure converting "+Saxon.Hej.trans.Err.wrap(test.getStringValue()))+" to a number"));
                        de.setErrorCode("FORG0001");
                        de.setXPathContext(context);
                        throw de;
                    }
                } else {
                    if ((prim is Saxon.Hej.value.DoubleValue)) {
                        if (foundNaN) {
                            return Saxon.Hej.value.DoubleValue.NaN;
                        }
                        foundDouble = true;
                    } else {
                        if ((prim is Saxon.Hej.value.FloatValue)) {
                            foundFloat = true;
                        } else {
                            if (((prim is Saxon.Hej.value.StringValue) && !(((prim is Saxon.Hej.value.AnyURIValue))))) {
                                foundString = true;
                            }
                        }
                    }
                }
                if (prim.isNaN()) {
                    if (ignoreNaN) {
                    } else {
                        if (foundDouble) {
                            return Saxon.Hej.value.DoubleValue.NaN;
                        } else {
                            foundNaN = true;
                        }
                    }
                } else {
                    try {
                        if ((atomicComparer.compareAtomicValues(prim, min)<0)) {
                            min = test2;
                        }
                    } catch (System.InvalidCastException err) {
                        if ((min.getItemType() == test2.getItemType())) {
                            throw;
                        } else {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(((("Cannot compare "+min.getItemType())+" with ")+test2.getItemType()));
                            de.setErrorCode("FORG0006");
                            de.setIsTypeError(true);
                            de.setXPathContext(context);
                            throw de;
                        }
                    }
                }
            }
            if (foundNaN) {
                return Saxon.Hej.value.FloatValue.NaN;
            }
            if (foundDouble) {
                if (!(((min is Saxon.Hej.value.DoubleValue)))) {
                    min = Saxon.Hej.type.Converter.convert(min, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, rules);
                }
            } else {
                if (foundFloat) {
                    if (!(((min is Saxon.Hej.value.FloatValue)))) {
                        min = Saxon.Hej.type.Converter.convert(min, Saxon.Hej.type.BuiltInAtomicType.FLOAT, rules);
                    }
                } else {
                    if (((min is Saxon.Hej.value.AnyURIValue) && foundString)) {
                        min = Saxon.Hej.type.Converter.convert(min, Saxon.Hej.type.BuiltInAtomicType.STRING, rules);
                    }
                }
            }
            return min;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(minimax(arguments[0].iterate(), isMaxFunction(), getAtomicComparer(context), ignoreNaN, context));
        }
        public override void exportAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            base.exportAttributes(@out);
            if (ignoreNaN) {
                @out.emitAttribute("flags", "i");
            }
        }
        public override void importAttributes(Saxon.Ejava.util.Properties attributes) {
            base.importAttributes(attributes);
            string flags = attributes.getProperty("flags");
            if (((flags != null) && flags.Contains("i"))) {
                setIgnoreNaN(true);
            }
        }
        public override string getStreamerName() {
            return "Minimax";
        }
        public class Min : Saxon.Hej.functions.Minimax {
            public override bool isMaxFunction() {
                return false;
            }
        
        }
        public class Max : Saxon.Hej.functions.Minimax {
            public override bool isMaxFunction() {
                return true;
            }
        
        }
    
    }
}