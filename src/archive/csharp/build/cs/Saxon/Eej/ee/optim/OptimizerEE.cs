using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class OptimizerEE : Saxon.Hej.expr.parser.Optimizer {
        public OptimizerEE(Saxon.Hej.Configuration config) : base (config) {
        }
        public override Saxon.Hej.expr.Expression optimizeValueComparison(Saxon.Hej.expr.ValueComparison vc, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression lhs = vc.getLhsExpression();
            Saxon.Hej.expr.Expression rhs = vc.getRhsExpression();
            int @operator = vc.getOperator();
            bool isCodepointCollation = vc.getRetainedStaticContext().getDefaultCollationName().Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI);
            Saxon.Hej.expr.Expression e1 = base.optimizeValueComparison(vc, visitor, contextInfo);
            if (((e1 != null) && (e1 != vc))) {
                return e1;
            }
            Saxon.Hej.om.Sequence value0 = null;
            Saxon.Hej.om.Sequence value1 = null;
            if ((lhs is Saxon.Hej.expr.Literal)) {
                value0 = ((Saxon.Hej.expr.Literal)(lhs)).getGroundedValue();
            }
            if ((rhs is Saxon.Hej.expr.Literal)) {
                value1 = ((Saxon.Hej.expr.Literal)(rhs)).getGroundedValue();
            }
            if (isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                if ((((value0 != null)) && ((value1 != null)))) {
                    try {
                        Saxon.Hej.value.AtomicValue r = vc.evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext());
                        return Saxon.Hej.expr.Literal.makeLiteral(((r == null) ? Saxon.Hej.value.EmptySequence.getInstance() : r), vc);
                    } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                        return vc;
                    }
                }
            }
            if (!(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS))) {
                return vc;
            }
            if ((lhs.isCallOn(typeof(Saxon.Hej.functions.Count)) && Saxon.Hej.expr.Literal.isAtomic(rhs))) {
                Saxon.Hej.expr.Expression e2 = optimizeCount(vc, visitor, false);
                if ((e2 != null)) {
                    return e2.optimize(visitor, contextInfo);
                }
            } else {
                if ((rhs.isCallOn(typeof(Saxon.Hej.functions.Count)) && Saxon.Hej.expr.Literal.isAtomic(lhs))) {
                    Saxon.Hej.expr.Expression e2 = optimizeCount(vc, visitor, true);
                    if ((e2 != null)) {
                        return e2.optimize(visitor, contextInfo);
                    }
                }
            }
            if ((lhs.isCallOn(typeof(Saxon.Hej.functions.StringLength_1)) && isZero(value1))) {
                System.Diagnostics.Debug.Assert((lhs is Saxon.Hej.expr.SystemFunctionCall));
                Saxon.Hej.expr.Expression arg = ((Saxon.Hej.expr.SystemFunctionCall)(lhs)).getArg(0);
                switch (vc.getOperator()) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                  case Saxon.Hej.expr.parser.Token.FLE:
                    return Saxon.Hej.functions.SystemFunction.makeCall("not", vc.getRetainedStaticContext(), arg);
                  case Saxon.Hej.expr.parser.Token.FNE:
                  case Saxon.Hej.expr.parser.Token.FGT:
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), arg);
                  case Saxon.Hej.expr.parser.Token.FGE:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                  case Saxon.Hej.expr.parser.Token.FLT:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                }
            }
            if ((rhs.isCallOn(typeof(Saxon.Hej.functions.StringLength_1)) && isZero(value0))) {
                System.Diagnostics.Debug.Assert((rhs is Saxon.Hej.expr.SystemFunctionCall));
                Saxon.Hej.expr.Expression arg = ((Saxon.Hej.expr.SystemFunctionCall)(rhs)).getArg(0);
                switch (vc.getOperator()) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                  case Saxon.Hej.expr.parser.Token.FGE:
                    return Saxon.Hej.functions.SystemFunction.makeCall("not", vc.getRetainedStaticContext(), arg);
                  case Saxon.Hej.expr.parser.Token.FNE:
                  case Saxon.Hej.expr.parser.Token.FLT:
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), arg);
                  case Saxon.Hej.expr.parser.Token.FLE:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                  case Saxon.Hej.expr.parser.Token.FGT:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                }
            }
            int p0 = lhs.getItemType().getPrimitiveType();
            if ((((((((((p0 == Saxon.Hej.om.StandardNames.XS_STRING) || (p0 == Saxon.Hej.om.StandardNames.XS_ANY_URI)) || (p0 == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))) && (vc.getResultWhenEmpty() != Saxon.Hej.value.BooleanValue.TRUE)) && (rhs is Saxon.Hej.expr.Literal)) && (((Saxon.Hej.expr.Literal)(rhs)).getGroundedValue() is Saxon.Hej.value.StringValue)) && ((Saxon.Hej.value.StringValue)(((Saxon.Hej.expr.Literal)(rhs)).getGroundedValue())).isEmpty()) && isCodepointCollation)) {
                switch (vc.getOperator()) {
                  case Saxon.Hej.expr.parser.Token.FNE:
                  case Saxon.Hej.expr.parser.Token.FGT:
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), lhs);
                  case Saxon.Hej.expr.parser.Token.FEQ:
                  case Saxon.Hej.expr.parser.Token.FLE:
                    if ((lhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                        return Saxon.Hej.functions.SystemFunction.makeCall("not", vc.getRetainedStaticContext(), lhs);
                    }
                    break ;
                }
            }
            int p1 = rhs.getItemType().getPrimitiveType();
            if ((((((((((p1 == Saxon.Hej.om.StandardNames.XS_STRING) || (p1 == Saxon.Hej.om.StandardNames.XS_ANY_URI)) || (p1 == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))) && (vc.getResultWhenEmpty() != Saxon.Hej.value.BooleanValue.TRUE)) && (lhs is Saxon.Hej.expr.Literal)) && (((Saxon.Hej.expr.Literal)(lhs)).getGroundedValue() is Saxon.Hej.value.StringValue)) && ((Saxon.Hej.value.StringValue)(((Saxon.Hej.expr.Literal)(lhs)).getGroundedValue())).isEmpty()) && isCodepointCollation)) {
                switch (vc.getOperator()) {
                  case Saxon.Hej.expr.parser.Token.FNE:
                  case Saxon.Hej.expr.parser.Token.FLT:
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), rhs);
                  case Saxon.Hej.expr.parser.Token.FEQ:
                  case Saxon.Hej.expr.parser.Token.FGE:
                    if ((rhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                        return Saxon.Hej.functions.SystemFunction.makeCall("not", vc.getRetainedStaticContext(), rhs);
                    }
                    break ;
                }
            }
            if ((((((value1 is Saxon.Hej.value.StringValue) && (lhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && (vc.getResultWhenEmpty() != Saxon.Hej.value.BooleanValue.TRUE)) && isCodepointCollation) && Saxon.Hej.type.BuiltInAtomicType.isStringLike(lhs.getItemType()))) {
                return new Saxon.Hej.expr.CompareToStringConstant(lhs, vc.getOperator(), ((Saxon.Hej.value.StringValue)(value1)).getUnicodeStringValue());
            }
            if ((((((value0 is Saxon.Hej.value.StringValue) && (rhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && (vc.getResultWhenEmpty() != Saxon.Hej.value.BooleanValue.TRUE)) && isCodepointCollation) && Saxon.Hej.type.BuiltInAtomicType.isStringLike(rhs.getItemType()))) {
                return new Saxon.Hej.expr.CompareToStringConstant(rhs, Saxon.Hej.expr.parser.Token.inverse(vc.getOperator()), ((Saxon.Hej.value.StringValue)(value0)).getUnicodeStringValue());
            }
            if ((((value1 is Saxon.Hej.value.Int64Value) && (lhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && Saxon.Hej.type.NumericType.isNumericType(lhs.getItemType()))) {
                return new Saxon.Hej.expr.CompareToIntegerConstant(lhs, vc.getOperator(), ((Saxon.Hej.value.Int64Value)(value1)).longValue());
            }
            if ((((value0 is Saxon.Hej.value.Int64Value) && (rhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && Saxon.Hej.type.NumericType.isNumericType(rhs.getItemType()))) {
                return new Saxon.Hej.expr.CompareToIntegerConstant(rhs, Saxon.Hej.expr.parser.Token.inverse(vc.getOperator()), ((Saxon.Hej.value.Int64Value)(value0)).longValue());
            }
            if (((((((p0 == Saxon.Hej.om.StandardNames.XS_BOOLEAN) && (p1 == Saxon.Hej.om.StandardNames.XS_BOOLEAN)) && (((@operator == Saxon.Hej.expr.parser.Token.FEQ) || (@operator == Saxon.Hej.expr.parser.Token.FNE)))) && (lhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && (rhs.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && (((lhs is Saxon.Hej.expr.Literal) || (rhs is Saxon.Hej.expr.Literal))))) {
                Saxon.Hej.expr.Literal literal = (Saxon.Hej.expr.Literal)((((lhs is Saxon.Hej.expr.Literal) ? lhs : rhs)));
                Saxon.Hej.expr.Expression other = ((lhs is Saxon.Hej.expr.Literal) ? rhs : lhs);
                bool negate = (((@operator == Saxon.Hej.expr.parser.Token.FEQ)) != ((Saxon.Hej.value.BooleanValue)(literal.getGroundedValue())).getBooleanValue());
                if (negate) {
                    Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("not", vc.getRetainedStaticContext(), other);
                    System.Diagnostics.Debug.Assert((fn != null));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(vc, fn);
                    return fn.optimize(visitor, contextInfo);
                } else {
                    return other;
                }
            }
            if ((lhs.isCallOn(typeof(Saxon.Hej.functions.GenerateId_1)) && rhs.isCallOn(typeof(Saxon.Hej.functions.GenerateId_1)))) {
                System.Diagnostics.Debug.Assert((lhs is Saxon.Hej.expr.SystemFunctionCall));
                Saxon.Hej.expr.SystemFunctionCall f0 = (Saxon.Hej.expr.SystemFunctionCall)(lhs);
                System.Diagnostics.Debug.Assert((rhs is Saxon.Hej.expr.SystemFunctionCall));
                Saxon.Hej.expr.SystemFunctionCall f1 = (Saxon.Hej.expr.SystemFunctionCall)(rhs);
                if (((!(Saxon.Hej.value.Cardinality.allowsMany(f0.getArg(0).getCardinality())) && !(Saxon.Hej.value.Cardinality.allowsMany(f1.getArg(0).getCardinality()))) && ((@operator == Saxon.Hej.expr.parser.Token.FEQ)))) {
                    Saxon.Hej.expr.IdentityComparison id = new Saxon.Hej.expr.IdentityComparison(f0.getArg(0), Saxon.Hej.expr.parser.Token.IS, f1.getArg(0));
                    id.setGenerateIdEmulation(true);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(vc, id);
                    return id.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                }
            }
            return vc;
        }
        private static bool isZero(Saxon.Hej.om.Sequence v) {
            return ((v is Saxon.Hej.value.NumericValue) && (((Saxon.Hej.value.NumericValue)(v)).compareTo(0) == 0));
        }
        private Saxon.Hej.expr.Expression optimizeCount(Saxon.Hej.expr.ValueComparison vc, Saxon.Hej.expr.parser.ExpressionVisitor visitor, bool inverted) {
            Saxon.Hej.expr.SystemFunctionCall countFn = (Saxon.Hej.expr.SystemFunctionCall)(((inverted ? vc.getRhsExpression() : vc.getLhsExpression())));
            Saxon.Hej.expr.Expression sequence = countFn.getArg(0);
            sequence = sequence.unordered(true, false);
            Saxon.Hej.value.AtomicValue literalOperand = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(((inverted ? vc.getLhsExpression() : vc.getRhsExpression())))).getGroundedValue());
            int op = (inverted ? Saxon.Hej.expr.parser.Token.inverse(vc.getOperator()) : vc.getOperator());
            if (isZero(literalOperand)) {
                if (((op == Saxon.Hej.expr.parser.Token.FEQ) || (op == Saxon.Hej.expr.parser.Token.FLE))) {
                    Saxon.Hej.expr.Expression result = Saxon.Hej.functions.SystemFunction.makeCall("empty", vc.getRetainedStaticContext(), sequence);
                    trace("Rewrite count()=0 as:", result);
                    return result;
                } else {
                    if (((op == Saxon.Hej.expr.parser.Token.FNE) || (op == Saxon.Hej.expr.parser.Token.FGT))) {
                        Saxon.Hej.expr.Expression result = Saxon.Hej.functions.SystemFunction.makeCall("exists", vc.getRetainedStaticContext(), sequence);
                        trace("Rewrite count()>0 as:", result);
                        return result;
                    } else {
                        if ((op == Saxon.Hej.expr.parser.Token.FGE)) {
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                        } else {
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                        }
                    }
                }
            } else {
                if ((literalOperand is Saxon.Hej.value.NumericValue)) {
                    long operand;
                    if ((literalOperand is Saxon.Hej.value.IntegerValue)) {
                        operand = ((Saxon.Hej.value.IntegerValue)(literalOperand)).longValue();
                    } else {
                        if (literalOperand.isNaN()) {
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get((op == Saxon.Hej.expr.parser.Token.FNE)), vc);
                        } else {
                            if (((Saxon.Hej.value.NumericValue)(literalOperand)).isWholeNumber()) {
                                operand = ((Saxon.Hej.value.NumericValue)(literalOperand)).longValue();
                            } else {
                                if ((op == Saxon.Hej.expr.parser.Token.FEQ)) {
                                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                                } else {
                                    if ((op == Saxon.Hej.expr.parser.Token.FNE)) {
                                        return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                                    } else {
                                        if (((op == Saxon.Hej.expr.parser.Token.FGT) || (op == Saxon.Hej.expr.parser.Token.FGE))) {
                                            operand = ((Saxon.Hej.value.NumericValue)(literalOperand)).ceiling().longValue();
                                            op = Saxon.Hej.expr.parser.Token.FGE;
                                        } else {
                                            operand = ((Saxon.Hej.value.NumericValue)(literalOperand)).floor().longValue();
                                            op = Saxon.Hej.expr.parser.Token.FLE;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if ((operand<0)) {
                        switch (op) {
                          case Saxon.Hej.expr.parser.Token.FEQ:
                          case Saxon.Hej.expr.parser.Token.FLT:
                          case Saxon.Hej.expr.parser.Token.FLE:
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                          default:
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                        }
                    }
                    if ((operand>System.Int32.MaxValue)) {
                        switch (op) {
                          case Saxon.Hej.expr.parser.Token.FEQ:
                          case Saxon.Hej.expr.parser.Token.FGT:
                          case Saxon.Hej.expr.parser.Token.FGE:
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, vc);
                          default:
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, vc);
                        }
                    }
                    if (((sequence is Saxon.Hej.expr.TailExpression) || sequence.isCallOn(typeof(Saxon.Hej.functions.Subsequence_2)))) {
                        return null;
                    }
                    switch (op) {
                      case Saxon.Hej.expr.parser.Token.FEQ:
                      case Saxon.Hej.expr.parser.Token.FNE:
                      case Saxon.Hej.expr.parser.Token.FLE:
                      case Saxon.Hej.expr.parser.Token.FLT:
                        Saxon.Hej.expr.Expression ss = Saxon.Hej.functions.SystemFunction.makeCall("subsequence", vc.getRetainedStaticContext(), sequence, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.PLUS_ONE, vc), Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue((operand+1)), vc));
                        Saxon.Hej.expr.Expression ct = Saxon.Hej.functions.SystemFunction.makeCall("count", vc.getRetainedStaticContext(), ss);
                        Saxon.Hej.expr.CompareToIntegerConstant ctic = new Saxon.Hej.expr.CompareToIntegerConstant(ct, op, operand);
                        trace("Rewrite count()~N as:", ctic);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(vc, ctic);
                        return ctic;
                      case Saxon.Hej.expr.parser.Token.FGE:
                      case Saxon.Hej.expr.parser.Token.FGT:
                        Saxon.Hej.expr.TailExpression tail = new Saxon.Hej.expr.TailExpression(sequence, (int)((((op == Saxon.Hej.expr.parser.Token.FGE) ? operand : (operand+1)))));
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(vc, tail);
                        Saxon.Hej.expr.Expression result = Saxon.Hej.functions.SystemFunction.makeCall("exists", vc.getRetainedStaticContext(), tail);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(vc, result);
                        trace("Rewrite count()>=N as:", result);
                        return result;
                    }
                }
            }
            return null;
        }
        public override Saxon.Hej.expr.Expression optimizeGeneralComparison(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.GeneralComparison gc, bool backwardsCompatible, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((gc is Saxon.Eej.ee.optim.GeneralEqualityEE)) {
                return gc;
            }
            if (!("EE".Equals(visitor.getTargetEdition()))) {
                return gc;
            }
            if ((gc.getOperator() == Saxon.Hej.expr.parser.Token.EQUALS)) {
                Saxon.Hej.expr.Expression p0 = gc.getLhsExpression();
                Saxon.Hej.expr.Expression p1 = gc.getRhsExpression();
                bool many0 = Saxon.Hej.value.Cardinality.expectsMany(p0);
                bool many1 = Saxon.Hej.value.Cardinality.expectsMany(p1);
                if ((many0 || many1)) {
                    if (((gc.getStringCollator() == null) || (gc.getStringCollator() == Saxon.Hej.expr.sort.CodepointCollator.getInstance()))) {
                        if (((p1 is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(p1)).getGroundedValue().getLength()>8))) {
                            Saxon.Hej.expr.Expression mapTest = makeMapTest(visitor, p0, (Saxon.Hej.expr.Literal)(p1), contextItemType);
                            if ((mapTest != null)) {
                                return mapTest;
                            }
                        } else {
                            if (((p0 is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(p0)).getGroundedValue().getLength()>8))) {
                                Saxon.Hej.expr.Expression mapTest = makeMapTest(visitor, p1, (Saxon.Hej.expr.Literal)(p0), contextItemType);
                                if ((mapTest != null)) {
                                    return mapTest;
                                }
                            }
                        }
                    }
                    Saxon.Eej.ee.optim.GeneralEqualityEE gcee = new Saxon.Eej.ee.optim.GeneralEqualityEE(p0, gc.getOperator(), p1);
                    gcee.setRetainedStaticContext(gc.getRetainedStaticContext());
                    gcee.setAtomicComparer(gc.getAtomicComparer());
                    gcee.setComparisonCardinality(gc.getComparisonCardinality());
                    gcee.setNeedsRuntimeCheck(gc.needsRuntimeCheck());
                    return gcee;
                }
            }
            return gc;
        }
        private Saxon.Hej.expr.Expression makeMapTest(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Literal literal, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            try {
                bool allStrings = true;
                Saxon.Hej.ma.map.MapItem map;
                foreach (Saxon.Hej.om.Item item in literal.getGroundedValue().asIterable()) {
                    if (!(((item is Saxon.Hej.value.StringValue)))) {
                        allStrings = false;
                        break ;
                    }
                }
                if (allStrings) {
                    Saxon.Hej.ma.map.DictionaryMap dict = new Saxon.Hej.ma.map.DictionaryMap();
                    foreach (Saxon.Hej.om.Item item in literal.getGroundedValue().asIterable()) {
                        dict.initialPut(item.getStringValue(), Saxon.Hej.value.BooleanValue.TRUE);
                    }
                    map = dict;
                } else {
                    Saxon.Impl.DataModel.ImmMap trie = new Saxon.Impl.DataModel.ImmMap();
                    foreach (Saxon.Hej.om.Item item in literal.getGroundedValue().asIterable()) {
                        if ((item is Saxon.Hej.value.AtomicValue)) {
                            if (((item is Saxon.Hej.value.CalendarValue) && !(((Saxon.Hej.value.CalendarValue)(item)).hasTimezone()))) {
                                return null;
                            }
                            if (((Saxon.Hej.value.AtomicValue)(item)).isNaN()) {
                                return null;
                            }
                            if (((Saxon.Hej.value.AtomicValue)(item)).isUntypedAtomic()) {
                                return null;
                            }
                            trie.initialPut((Saxon.Hej.value.AtomicValue)(item), Saxon.Hej.value.BooleanValue.TRUE);
                        } else {
                            return null;
                        }
                    }
                    map = trie;
                }
                Saxon.Hej.expr.Expression e2;
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                Saxon.Hej.functions.SystemFunction containsFn;
                if ((allStrings || (th.relationship(exp.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) == Saxon.Hej.type.Affinity.DISJOINT))) {
                    containsFn = Saxon.Hej.ma.map.MapFunctionSet.getInstance(31).makeFunction("contains", 2);
                } else {
                    containsFn = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("map-untyped-contains", 2);
                }
                if ((exp.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                    e2 = containsFn.makeFunctionCall(Saxon.Hej.expr.Literal.makeLiteral(map, exp), exp);
                } else {
                    Saxon.Hej.expr.Expression contains = containsFn.makeFunctionCall(Saxon.Hej.expr.Literal.makeLiteral(map, exp), new Saxon.Hej.expr.ContextItemExpression());
                    Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(exp, contains);
                    e2 = Saxon.Hej.functions.SystemFunction.makeCall("exists", exp.getRetainedStaticContext(), fe);
                }
                System.Diagnostics.Debug.Assert((e2 != null));
                e2.setRetainedStaticContext(exp.getRetainedStaticContext());
                e2 = e2.typeCheck(visitor, contextInfo);
                e2 = e2.optimize(visitor, contextInfo);
                return e2;
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public override Saxon.Hej.expr.Expression optimizeSaxonStreamFunction(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi, Saxon.Hej.expr.Expression select) {
            Saxon.Hej.expr.Expression e = base.optimizeSaxonStreamFunction(visitor, cisi, select);
            if ((e != null)) {
                return e;
            }
            if (!(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING))) {
                streamingNotPossible("Cannot use saxon:stream(): multithreading is not enabled", select, visitor);
                return null;
            }
            if (!(((select is Saxon.Hej.expr.SlashExpression)))) {
                if ((select is Saxon.Hej.expr.sort.DocumentSorter)) {
                    streamingNotPossible("Cannot use saxon:stream(): expression is not provably in document order", select, visitor);
                } else {
                    streamingNotPossible("Cannot use saxon:stream(): not a path expression", select, visitor);
                }
                return null;
            }
            Saxon.Hej.expr.SlashExpression pexp = (Saxon.Hej.expr.SlashExpression)(select);
            Saxon.Hej.expr.Expression start = pexp.getFirstStep();
            if ((!(((start.isCallOn(typeof(Saxon.Hej.functions.Doc)) || start.isCallOn(typeof(Saxon.Hej.functions.DocumentFn))))) || (((Saxon.Hej.expr.SystemFunctionCall)(start)).getArity() != 1))) {
                streamingNotPossible("Cannot use saxon:stream(): path must start with call on doc#1 or document#1", select, visitor);
                return null;
            }
            Saxon.Hej.expr.SystemFunctionCall documentExp = (Saxon.Hej.expr.SystemFunctionCall)(start);
            Saxon.Hej.expr.Expression rest = pexp.getRemainingSteps();
            Saxon.Hej.expr.Expression snap = Saxon.Hej.functions.SystemFunction.makeCall("snapshot", select.getRetainedStaticContext(), rest);
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accList = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
            Saxon.Hej.lib.ParseOptions options = config.getParseOptions().withSchemaValidationMode(Saxon.Hej.lib.Validation.STRIP).withTopLevelType(null).withApplicableAccumulators(accList);
            Saxon.Eej.ee.stream.StreamInstr stream = new Saxon.Eej.ee.stream.StreamInstr(new Saxon.Hej.expr.ContextItemExpression(), snap, options);
            trace("Using streaming for saxon:stream", stream);
            Saxon.Hej.expr.instruct.ForEach each = new Saxon.Hej.expr.instruct.ForEach(documentExp.getArg(0), stream);
            return each.optimize(visitor, cisi);
        }
        private void streamingNotPossible(string reason, Saxon.Hej.expr.Expression select, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            trace(reason, select);
            visitor.issueWarning(reason, Saxon.Hej.trans.SaxonErrorCode.SXWN9024, select.getLocation());
        }
        public override void makeCopyOperationsExplicit(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Operand childOp) {
            Saxon.Hej.expr.Expression child = childOp.getChildExpression();
            bool atomic = ((child.getItemType() is Saxon.Hej.type.AtomicType) || (child.getItemType() is Saxon.Hej.type.FunctionItemType));
            bool replace = false;
            if ((child is Saxon.Hej.expr.ContextItemExpression)) {
                replace = true;
            }
            if ((!(atomic) && child.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET))) {
                replace = true;
            }
            if (((child is Saxon.Hej.expr.instruct.OnEmptyExpr) || (child is Saxon.Hej.expr.instruct.OnNonEmptyExpr))) {
                replace = false;
            }
            if (replace) {
                Saxon.Hej.expr.instruct.CopyOf copy = new Saxon.Hej.expr.instruct.CopyOf(child, true, Saxon.Hej.lib.Validation.PRESERVE, null, false);
                childOp.setChildExpression(copy);
                return ;
            }
            if ((child is Saxon.Hej.expr.SimpleStepExpression)) {
                return ;
            } else {
                if ((child is Saxon.Hej.expr.instruct.Fork)) {
                    return ;
                }
            }
            foreach (Saxon.Hej.expr.Operand action in child.operands()) {
                if ((action.getOperandRole().getUsage() == Saxon.Hej.expr.OperandUsage.TRANSMISSION)) {
                    makeCopyOperationsExplicit(child, action);
                }
            }
        }
        public override void checkStreamability(Saxon.Hej.style.XSLTemplate sourceTemplate, Saxon.Hej.expr.instruct.TemplateRule compiledTemplate) {
            if (compiledTemplate.isDeclaredStreamable()) {
                bool actuallyStreamable;
                System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                bool enabled = getConfiguration().isStreamabilityEnabled();
                if (!(enabled)) {
                    reasons.Add("Streaming is disabled for this Saxon Configuration");
                    actuallyStreamable = false;
                } else {
                    actuallyStreamable = ((Saxon.Eej.ee.trans.TemplateRuleEE)(compiledTemplate)).isActuallyStreamable(reasons);
                }
                if (!(actuallyStreamable)) {
                    bool fallback = getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK);
                    Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder("Template rule is declared streamable but it does not satisfy the streamability rules. ");
                    foreach (string reason in reasons) {
                        message.append("\n  * ").append(reason);
                    }
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message.ToString());
                    err.setErrorCode("XTSE3430");
                    err.setLocator(sourceTemplate);
                    if (fallback) {
                        message.append("\n  * Falling back to non-streaming implementation");
                        sourceTemplate.getStaticContext().issueWarning(message.ToString(), Saxon.Hej.trans.SaxonErrorCode.SXWN9024, sourceTemplate);
                        compiledTemplate.setDeclaredStreamable(false);
                    } else {
                        throw err;
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimizeForExpressionForStreaming(Saxon.Hej.expr.ForExpression expr) {
            Saxon.Hej.expr.Expression e2 = /*134*/Saxon.Eej.ee.stream.Streamability.rewriteForExpressionAsMappingExpression(expr);
            if ((e2 != null)) {
                return e2;
            } else {
                return expr;
            }
        }
        public override Saxon.Hej.expr.Expression optimizeQuantifiedExpressionForStreaming(Saxon.Hej.expr.QuantifiedExpression expr) {
            Saxon.Hej.expr.Expression rewrite = /*134*/Saxon.Eej.ee.stream.Streamability.rewriteQuantifiedExpressionAsFilterExpression(expr);
            if ((rewrite != null)) {
                return rewrite;
            } else {
                return null;
            }
        }
        private static Saxon.Hej.expr.Expression promoteUnion(Saxon.Hej.expr.SlashExpression path) {
            Saxon.Hej.expr.Expression start = path.getFirstStep();
            Saxon.Hej.expr.Expression rest = path.getRemainingSteps();
            if ((rest is Saxon.Hej.expr.SlashExpression)) {
                rest = promoteUnion((Saxon.Hej.expr.SlashExpression)(rest));
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> operands = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(3);
            gatherUnionOperands(rest, operands);
            if ((operands.Count<2)) {
                return path;
            } else {
                Saxon.Hej.expr.Expression p = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(start, operands[0]);
                for (int i = 1; (i<operands.Count); i++) {
                    p = new Saxon.Hej.expr.VennExpression(p, Saxon.Hej.expr.parser.Token.UNION, Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(start.copy(new Saxon.Hej.expr.parser.RebindingMap()), operands[i]));
                }
                return p;
            }
        }
        private static void gatherUnionOperands(Saxon.Hej.expr.Expression exp, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if (((exp is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(exp)).getOperator() == Saxon.Hej.expr.parser.Token.UNION))) {
                Saxon.Hej.expr.VennExpression ve = (Saxon.Hej.expr.VennExpression)(exp);
                gatherUnionOperands(ve.getLhsExpression(), list);
                gatherUnionOperands(ve.getRhsExpression(), list);
            } else {
                list.Add(exp);
            }
        }
        public override Saxon.Hej.expr.Expression convertPathExpressionToKey(Saxon.Hej.expr.SlashExpression pathExp, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if (!(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CREATE_KEYS))) {
                return null;
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.Expression copy = pathExp.copy(new Saxon.Hej.expr.parser.RebindingMap());
            if (!(((copy is Saxon.Hej.expr.SlashExpression)))) {
                return null;
            }
            Saxon.Hej.expr.SlashExpression p = ((Saxon.Hej.expr.SlashExpression)(copy)).tryToMakeAbsolute();
            if ((p != null)) {
                Saxon.Hej.expr.Expression match = p.getRemainingSteps();
                Saxon.Hej.expr.FilterExpression f = null;
                if ((match is Saxon.Hej.expr.FilterExpression)) {
                    f = (Saxon.Hej.expr.FilterExpression)(match);
                    if ((f.getFilter() is Saxon.Hej.expr.BooleanExpression)) {
                        f = expandComplexFilter(f.getSelectExpression(), f.getFilter());
                    }
                } else {
                    if ((match is Saxon.Hej.expr.SlashExpression)) {
                        f = convertToFilterExpression((Saxon.Hej.expr.SlashExpression)(match), th);
                    }
                }
                if ((f != null)) {
                    return convertFilterExpressionToKey(f, visitor, p.getFirstStep());
                }
            }
            return null;
        }
        public override Saxon.Hej.expr.Expression tryIndexedFilter(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, bool indexFirstOperand, bool contextIsDoc) {
            if ("EE".Equals(visitor.getTargetEdition())) {
                if (!(Saxon.Hej.value.Cardinality.allowsMany(f.getLhsExpression().getCardinality()))) {
                    return f;
                }
                Saxon.Hej.expr.Expression k = tryToConvertFilterExpressionToKey(f, visitor, contextIsDoc);
                if ((k != null)) {
                    return k;
                }
                f.restoreParentPointers();
                if (((f.getSelectExpression() is Saxon.Hej.expr.VariableReference) && isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.INDEX_VARIABLES))) {
                    Saxon.Hej.expr.VariableReference varRef = (Saxon.Hej.expr.VariableReference)(f.getSelectExpression());
                    Saxon.Hej.expr.Binding binding = varRef.getBinding();
                    if (((binding is Saxon.Hej.expr.LocalBinding) && Saxon.Hej.expr.parser.ExpressionTool.isLoopingReference(varRef, binding))) {
                        ((Saxon.Hej.expr.LocalBinding)(binding)).setIndexedVariable();
                        Saxon.Hej.expr.Expression ife = new Saxon.Eej.ee.optim.IndexedFilterExpression(varRef, (Saxon.Hej.expr.ComparisonExpression)(f.getFilter()), indexFirstOperand);
                        trace("Created indexed filter expression: ", ife);
                        return ife;
                    } else {
                        if (((binding is Saxon.Hej.expr.instruct.GlobalVariable) && !(((binding is Saxon.Hej.expr.instruct.GlobalParam))))) {
                            ((Saxon.Hej.expr.instruct.GlobalVariable)(binding)).setIndexedVariable();
                            return new Saxon.Eej.ee.optim.IndexedFilterExpression((Saxon.Hej.expr.VariableReference)(f.getSelectExpression()), (Saxon.Hej.expr.ComparisonExpression)(f.getFilter()), indexFirstOperand);
                        }
                    }
                }
            } else {
                return f;
            }
            return f;
        }
        public override Saxon.Hej.expr.FilterExpression reorderPredicates(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi) {
            if (!(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.REORDER_PREDICATES))) {
                return f;
            }
            Saxon.Hej.expr.Expression @base = f.getBase();
            if (!(((@base is Saxon.Hej.expr.FilterExpression)))) {
                return f;
            }
            System.Collections.Generic.List<Saxon.Hej.expr.Expression> filters = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            addTerms(f.getFilter(), filters);
            while (((@base is Saxon.Hej.expr.FilterExpression) && !(((Saxon.Hej.expr.FilterExpression)(@base)).isFilterIsPositional()))) {
                Saxon.Hej.expr.Expression predicate = ((Saxon.Hej.expr.FilterExpression)(@base)).getFilter();
                addTerms(predicate, filters);
                @base = ((Saxon.Hej.expr.FilterExpression)(@base)).getBase();
            }
            Saxon.Impl.Helpers.ListUtils.Sort(filters, (f0, f1) => {
    if ((f0 == f1)) {
        return 0;
    }
    bool d0 = (((f0.getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS))) == 0);
    bool d1 = (((f1.getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS))) == 0);
    if ((d0 != d1)) {
        return (d0 ? -1 : +(1));
    }
    bool i0 = (isIndexableFilter(f0) != 0);
    bool i1 = (isIndexableFilter(f1) != 0);
    if ((i0 != i1)) {
        return (i0 ? -1 : +(1));
    }
    return ((f0.getCost()<f1.getCost()) ? -1 : +(1));
} );
            Saxon.Hej.expr.Expression newPredicate = null;
            foreach (Saxon.Hej.expr.Expression e in filters) {
                newPredicate = ((newPredicate == null) ? e : new Saxon.Hej.expr.AndExpression(newPredicate, e));
            }
            return new Saxon.Hej.expr.FilterExpression(@base, newPredicate);
        }
        private static void addTerms(Saxon.Hej.expr.Expression predicate, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> filters) {
            if ((predicate is Saxon.Hej.expr.AndExpression)) {
                addTerms(((Saxon.Hej.expr.AndExpression)(predicate)).getRhsExpression(), filters);
                addTerms(((Saxon.Hej.expr.AndExpression)(predicate)).getLhsExpression(), filters);
            } else {
                filters.Insert(0, predicate);
            }
        }
        private Saxon.Hej.expr.Expression tryToConvertFilterExpressionToKey(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, bool contextIsDoc) {
            Saxon.Hej.expr.Expression @base = f.getSelectExpression();
            while (true) {
                if ((@base is Saxon.Hej.expr.SlashExpression)) {
                    break ;
                } else {
                    if ((@base is Saxon.Hej.expr.AxisExpression)) {
                        if (contextIsDoc) {
                            @base = new Saxon.Hej.expr.SlashExpression(new Saxon.Hej.expr.RootExpression(), @base);
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(f, @base);
                        }
                        break ;
                    } else {
                        if ((@base is Saxon.Hej.expr.VariableReference)) {
                            Saxon.Hej.expr.Binding b = ((Saxon.Hej.expr.VariableReference)(@base)).getBinding();
                            if ((b is Saxon.Hej.expr.LetExpression)) {
                                @base = ((Saxon.Hej.expr.LetExpression)(b)).getSequence().copy(new Saxon.Hej.expr.parser.RebindingMap());
                            } else {
                                return null;
                            }
                        } else {
                            if ((@base is Saxon.Hej.expr.sort.DocumentSorter)) {
                                @base = ((Saxon.Hej.expr.sort.DocumentSorter)(@base)).getBaseExpression();
                            } else {
                                if ((@base is Saxon.Hej.expr.FilterExpression)) {
                                    Saxon.Hej.expr.FilterExpression baseF = (Saxon.Hej.expr.FilterExpression)(@base);
                                    if ((!(baseF.isFilterIsPositional()) && (isIndexableFilter(baseF.getFilter()) == 0))) {
                                        Saxon.Hej.expr.Expression baseFB = baseF.getSelectExpression();
                                        Saxon.Hej.expr.Expression baseFF = baseF.getFilter();
                                        Saxon.Hej.expr.parser.RebindingMap rm = new Saxon.Hej.expr.parser.RebindingMap();
                                        Saxon.Hej.expr.FilterExpression fe2 = new Saxon.Hej.expr.FilterExpression(baseFB.copy(rm), f.getFilter().copy(rm));
                                        Saxon.Hej.expr.FilterExpression fe3 = new Saxon.Hej.expr.FilterExpression(fe2, baseFF);
                                        return tryToConvertFilterExpressionToKey(fe3, visitor, contextIsDoc);
                                    } else {
                                        return null;
                                    }
                                } else {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            if ((@base is Saxon.Hej.expr.SlashExpression)) {
                Saxon.Hej.expr.SlashExpression path = (Saxon.Hej.expr.SlashExpression)(@base);
                Saxon.Hej.expr.SlashExpression p = path.tryToMakeAbsolute();
                if ((p == null)) {
                    return null;
                }
                Saxon.Hej.expr.Expression doc = p.getFirstStep();
                if (!(th.isSubType(doc.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT))) {
                    return null;
                }
                return convertFilterExpressionToKey(f, visitor, doc);
            } else {
                return null;
            }
        }
        private Saxon.Hej.expr.Expression convertFilterExpressionToKey(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Expression doc) {
            if (!(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CREATE_KEYS))) {
                return null;
            }
            if ((f.getFilter() is Saxon.Hej.expr.BooleanExpression)) {
                f = expandComplexFilter(f.getSelectExpression(), f.getFilter());
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = f.getRetainedStaticContext();
            int indexableFlag = isIndexableFilter(f.getFilter());
            if ((indexableFlag == 0)) {
                if (f.isPositional(th)) {
                    return null;
                }
                return tryConvertingInnerPredicate(f, visitor, doc);
            }
            bool indexFirstOperand = (indexableFlag>0);
            Saxon.Hej.expr.Expression path = f.getSelectExpression().copy(new Saxon.Hej.expr.parser.RebindingMap());
            if ((((path.getDependencies()&~(((((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT))))) != 0)) {
                return tryConvertingInnerPredicate(f, visitor, doc);
            }
            if ((((path is Saxon.Hej.expr.SlashExpression) && !(((doc is Saxon.Hej.expr.RootExpression)))) && (((doc.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) != 0))) {
                path = new Saxon.Hej.expr.SlashExpression(new Saxon.Hej.expr.RootExpression(), path);
            }
            Saxon.Hej.expr.ComparisonExpression fc = (Saxon.Hej.expr.ComparisonExpression)(f.getFilter().copy(new Saxon.Hej.expr.parser.RebindingMap()));
            Saxon.Hej.expr.Expression use = (indexFirstOperand ? fc.getLhsExpression() : fc.getRhsExpression());
            Saxon.Hej.expr.Expression value = (indexFirstOperand ? fc.getRhsExpression() : fc.getLhsExpression());
            if ((((use.getDependencies()&~((((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT))))) != 0)) {
                return null;
            }
            int props = path.getSpecialProperties();
            if ((((((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0)) || ((((props&Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)) == 0)))) {
                return null;
            }
            if ((((props&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) == 0)) {
                Saxon.Hej.expr.sort.DocumentSorter sorter = new Saxon.Hej.expr.sort.DocumentSorter(path);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(path, sorter);
                path = sorter;
            }
            Saxon.Hej.pattern.NodeSetPattern pathFinder = new Saxon.Hej.pattern.NodeSetPattern(path);
            Saxon.Hej.type.ItemType useType = use.getItemType();
            if (!(useType.isPlainType())) {
                try {
                    use = Saxon.Hej.expr.Atomizer.makeAtomizer(use, null).simplify();
                } catch (Saxon.Hej.trans.XPathException e) {
                    return null;
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(f, use);
                useType = use.getItemType();
            }
            bool convertUntypedToOther = fc.convertsUntypedToOther();
            if (convertUntypedToOther) {
                Saxon.Hej.type.ItemType valueType = value.getItemType().getAtomizedItemType();
                if ((useType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) && valueType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))) {
                } else {
                    if ((((useType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || useType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) && !((((valueType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(valueType, Saxon.Hej.type.BuiltInAtomicType.STRING)) || th.isSubType(valueType, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)))))) {
                        Saxon.Hej.type.AtomicType targetType = (Saxon.Hej.type.AtomicType)(valueType);
                        if (Saxon.Hej.type.NumericType.isNumericType(targetType)) {
                            targetType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                        }
                        use = new Saxon.Hej.expr.AtomicSequenceConverter(use, targetType);
                        ((Saxon.Hej.expr.AtomicSequenceConverter)(use)).allocateConverterStatically(config, false);
                    } else {
                        if ((((valueType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || valueType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) && !((((useType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(useType, Saxon.Hej.type.BuiltInAtomicType.STRING)) || th.isSubType(valueType, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)))))) {
                            Saxon.Hej.type.AtomicType targetType = (Saxon.Hej.type.AtomicType)(valueType);
                            if (Saxon.Hej.type.NumericType.isNumericType(targetType)) {
                                targetType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                            }
                            value = new Saxon.Hej.expr.AtomicSequenceConverter(value, targetType);
                            ((Saxon.Hej.expr.AtomicSequenceConverter)(value)).allocateConverterStatically(config, false);
                        }
                    }
                }
            }
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            string defaultCollationName = env.getDefaultCollationName();
            Saxon.Hej.trans.KeyManager keyManager = env.getKeyManager();
            Saxon.Hej.om.StructuredQName keyName;
            Saxon.Hej.trans.KeyDefinitionSet keySet;
            lock (keyManager) {
                keySet = keyManager.findKeyDefinition(pathFinder, use, defaultCollationName);
                if ((keySet == null)) {
                    Saxon.Hej.Configuration config = getConfiguration();
                    Saxon.Hej.expr.instruct.SlotManager slotManager = config.makeSlotManager();
                    int slots = Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(pathFinder.getSelectionExpression(), 0, slotManager);
                    Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(use, slots, slotManager);
                    Saxon.Hej.lib.StringCollator collator;
                    try {
                        collator = config.getCollation(defaultCollationName);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        return null;
                    }
                    keyName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, ("kk"+((100+keyManager.getNumberOfKeyDefinitions()))));
                    Saxon.Hej.trans.SymbolicName symbolicName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_KEY, keyName);
                    Saxon.Hej.trans.KeyDefinition key = new Saxon.Hej.trans.KeyDefinition(symbolicName, pathFinder, use, defaultCollationName, collator);
                    key.setPackageData(f.getPackageData());
                    key.setIndexedItemType((Saxon.Hej.type.BuiltInAtomicType)(useType.getPrimitiveItemType()));
                    key.setConvertUntypedToOther(convertUntypedToOther);
                    key.setStackFrameMap(slotManager);
                    if ((f.getFilter() is Saxon.Hej.expr.ValueComparison)) {
                        key.setStrictComparison(true);
                    }
                    try {
                        keyManager.addKeyDefinition(keyName, key, true, getConfiguration());
                    } catch (Saxon.Hej.trans.XPathException err) {
                        throw new Saxon.Ejava.lang.AssertionError(err);
                    }
                    keySet = keyManager.getKeyDefinitionSet(keyName);
                } else {
                    keyName = keySet.getKeyName();
                }
            }
            Saxon.Hej.expr.Expression call;
            if ((doc.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                Saxon.Hej.expr.ForExpression forExp = new Saxon.Hej.expr.ForExpression();
                forExp.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(doc.getItemType(), doc.getCardinality()));
                forExp.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("dd"+forExp.GetHashCode())));
                Saxon.Hej.expr.Expression docs = doc;
                if (!(docs.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET))) {
                    docs = new Saxon.Hej.expr.sort.DocumentSorter(docs);
                }
                forExp.setSequence(docs);
                Saxon.Hej.expr.LocalVariableReference docVar = new Saxon.Hej.expr.LocalVariableReference(forExp);
                call = Saxon.Hej.functions.KeyFn.internalKeyCall(keyManager, keySet, keyName.getEQName(), value, docVar, rsc);
                forExp.setAction(call);
                call = forExp;
            } else {
                call = Saxon.Hej.functions.KeyFn.internalKeyCall(keyManager, keySet, keyName.getEQName(), value, doc, rsc);
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(f, call);
            trace("Replaced filter expression with call to key function: ", call);
            return call;
        }
        private Saxon.Hej.expr.Expression tryConvertingInnerPredicate(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Expression doc) {
            Saxon.Hej.expr.Expression @base = f.getSelectExpression();
            if ((@base is Saxon.Hej.expr.FilterExpression)) {
                Saxon.Hej.expr.Expression pred = f.getFilter();
                Saxon.Hej.expr.Expression k = convertFilterExpressionToKey((Saxon.Hej.expr.FilterExpression)(@base), visitor, doc);
                if ((k != null)) {
                    Saxon.Hej.expr.FilterExpression kf = new Saxon.Hej.expr.FilterExpression(k, pred);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(f, kf);
                    return kf;
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }
        public override Saxon.Hej.expr.FilterExpression convertToFilterExpression(Saxon.Hej.expr.SlashExpression pathExp, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.expr.Expression head = pathExp.getFirstStep();
            Saxon.Hej.expr.Expression tail = pathExp.getRemainingSteps();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> path = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(3);
            path.Add(head);
            while ((tail is Saxon.Hej.expr.SlashExpression)) {
                head = ((Saxon.Hej.expr.SlashExpression)(tail)).getFirstStep();
                path.Add(head);
                tail = ((Saxon.Hej.expr.SlashExpression)(tail)).getRemainingSteps();
            }
            if ((tail is Saxon.Hej.expr.FilterExpression)) {
                if (!(((Saxon.Hej.expr.FilterExpression)(tail)).isPositional(th))) {
                    Saxon.Hej.expr.Expression predicate = ((Saxon.Hej.expr.FilterExpression)(tail)).getFilter();
                    Saxon.Hej.expr.Expression last = ((Saxon.Hej.expr.FilterExpression)(tail)).getSelectExpression();
                    bool repeat = (last is Saxon.Hej.expr.FilterExpression);
                    for (int i = (path.Count-1); (i>=0); i--) {
                        last = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(path[i], last);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(pathExp, last);
                    }
                    if ((((last is Saxon.Hej.expr.SlashExpression)) && repeat)) {
                        Saxon.Hej.expr.FilterExpression e = convertToFilterExpression((Saxon.Hej.expr.SlashExpression)(last), th);
                        if ((e != null)) {
                            last = e;
                        }
                    }
                    if ((predicate is Saxon.Hej.expr.BooleanExpression)) {
                        return expandComplexFilter(last, predicate);
                    } else {
                        Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(last, predicate);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(pathExp, fe);
                        trace("Moved predicate to outer level of path expression: ", fe);
                        return fe;
                    }
                }
            }
            return null;
        }
        private Saxon.Hej.expr.FilterExpression expandComplexFilter(Saxon.Hej.expr.Expression @base, Saxon.Hej.expr.Expression predicate) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(4);
            Saxon.Hej.expr.BooleanExpression.listAndComponents(predicate, list);
            foreach (Saxon.Hej.expr.Expression exp in list) {
                Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(@base, exp);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(@base, fe);
                @base = fe;
            }
            trace("Split composite predicate into multiple predicates: ", @base);
            return (Saxon.Hej.expr.FilterExpression)(@base);
        }
        public override int isIndexableFilter(Saxon.Hej.expr.Expression filter) {
            if (((filter is Saxon.Hej.expr.ComparisonExpression) && (((Saxon.Hej.expr.ComparisonExpression)(filter)).getSingletonOperator() == Saxon.Hej.expr.parser.Token.FEQ))) {
                return isIndexableComparison((Saxon.Hej.expr.ComparisonExpression)(filter));
            } else {
                return 0;
            }
        }
        private static int isIndexableComparison(Saxon.Hej.expr.ComparisonExpression filter) {
            if ((filter is Saxon.Hej.expr.compat.GeneralComparison10)) {
                return 0;
            }
            Saxon.Hej.expr.Expression op0 = filter.getLhsExpression();
            Saxon.Hej.expr.Expression op1 = filter.getRhsExpression();
            bool d0f = Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(op0);
            bool d0v = (((op0.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) != 0);
            bool d1f = Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(op1);
            bool d1v = (((op1.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) != 0);
            if (((d0f && !(d1f)) && !(d0v))) {
                return +(1);
            }
            if (((d1f && !(d0f)) && !(d1v))) {
                return -1;
            }
            return 0;
        }
        public override Saxon.Hej.om.GroundedValue makeIndexedValue(Saxon.Hej.om.SequenceIterator iter) {
            return /*134*/Saxon.Eej.ee.optim.SearchableValue.makeSearchableValue(iter);
        }
        public override void optimizeNodeSetPattern(Saxon.Hej.pattern.NodeSetPattern pattern) {
            Saxon.Hej.expr.Expression selection = pattern.getSelectionExpression();
            if ((selection is Saxon.Hej.expr.GlobalVariableReference)) {
                Saxon.Hej.expr.Binding var = ((Saxon.Hej.expr.GlobalVariableReference)(selection)).getBinding();
                if ((var is Saxon.Hej.expr.instruct.GlobalVariable)) {
                    ((Saxon.Hej.expr.instruct.GlobalVariable)(var)).setIndexedVariable();
                }
            }
        }
        public override void prepareForStreaming(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(exp, new System.Object(), (expr, obj) => {
                expr.prepareForStreaming();
                return false;
            } );
        }
        public override Saxon.Hej.om.Sequence evaluateStreamingArgument(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(expr);
            if ((ps == null)) {
                return base.evaluateStreamingArgument(expr, context);
            } else {
                if ((ps.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                    return Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(expr, context);
                } else {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
        }
        public override Saxon.Hej.expr.Expression makeConditionalDocumentSorter(Saxon.Hej.expr.sort.DocumentSorter sorter, Saxon.Hej.expr.SlashExpression path) {
            Saxon.Hej.expr.Expression head = path.getFirstStep();
            Saxon.Hej.expr.Expression h2 = head;
            if (((head is Saxon.Hej.expr.ItemChecker) && (((Saxon.Hej.expr.ItemChecker)(head)).getRequiredType() is Saxon.Hej.pattern.NodeTest))) {
                h2 = ((Saxon.Hej.expr.ItemChecker)(head)).getBaseExpression();
            }
            Saxon.Hej.expr.Expression steps = path.getRemainingSteps();
            int stepProps = steps.getSpecialProperties();
            if ((!(Saxon.Hej.value.Cardinality.allowsMany(h2.getCardinality())) && (((stepProps&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0))) {
                return path;
            }
            if (((h2 is Saxon.Hej.expr.VariableReference) && (((stepProps&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0))) {
                Saxon.Hej.expr.Expression @ref = head.copy(new Saxon.Hej.expr.parser.RebindingMap());
                Saxon.Hej.expr.TailExpression tailExp = new Saxon.Hej.expr.TailExpression(@ref, 2);
                Saxon.Hej.expr.Expression exists = Saxon.Hej.functions.SystemFunction.makeCall("exists", sorter.getRetainedStaticContext(), tailExp);
                return new Saxon.Hej.expr.sort.ConditionalSorter(exists, sorter);
            } else {
                return sorter;
            }
        }
        public override Saxon.Hej.expr.Expression tryInlineFunctionCall(Saxon.Hej.expr.UserFunctionCall functionCall, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if (!(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.INLINE_FUNCTIONS))) {
                return functionCall;
            }
            if (functionCall.isBeingInlined()) {
                return functionCall;
            }
            Saxon.Hej.expr.instruct.UserFunction function = functionCall.getFunction();
            Saxon.Ejava.util.Optional<bool> functionInlineable = function.isInlineable();
            bool isInlineable;
            if ((function.getDeclaredStreamability() != Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                isInlineable = false;
            } else {
                if (functionInlineable.isEmpty()) {
                    if ((function.getBody() == null)) {
                        return functionCall;
                    }
                    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunction> calledFunctions = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunction>(10);
                    Saxon.Hej.expr.parser.ExpressionTool.gatherCalledFunctions(function.getBody(), calledFunctions);
                    int maxInlineSize = config.getConfigurationProperty(Saxon.Hej.lib.Feature<int>.THRESHOLD_FOR_FUNCTION_INLINING);
                    isInlineable = (((((calledFunctions.Count==0 && !(Saxon.Hej.expr.parser.ExpressionTool.changesXsltContext(function.getBody()))) && (Saxon.Hej.expr.parser.ExpressionTool.expressionSize(function.getBody())<=maxInlineSize)) && !(function.isMemoFunction())) && !(Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(function.getBody()))) && (functionCall.getRetainedStaticContext().getPackageData() == function.getPackageData()));
                    function.setInlineable(isInlineable);
                } else {
                    isInlineable = functionInlineable.get();
                }
            }
            if ((isInlineable && (function.getNumberOfParameters()>function.getMinimumArity()))) {
                isInlineable = false;
            }
            if (isInlineable) {
                functionCall.setBeingInlined(true);
                function.markAsInlined();
                bool wasInlining = visitor.isInliningFunctions();
                visitor.setInliningFunctions(true);
                Saxon.Hej.expr.Expression newBody;
                newBody = function.getBody().copy(new Saxon.Hej.expr.parser.RebindingMap());
                newBody.setRetainedStaticContext(function.getRetainedStaticContext());
                Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = function.getParameterDefinitions();
                for (int p = (@params.Length-1); (p>=0); p--) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter param = @params[p];
                    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                    let.setRequiredType(param.getRequiredType());
                    let.setVariableQName(param.getVariableQName());
                    let.setSequence(functionCall.getArg(p).copy(new Saxon.Hej.expr.parser.RebindingMap()));
                    let.setAction(newBody);
                    let.setRetainedStaticContext(function.getRetainedStaticContext());
                    Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(newBody, param, let);
                    newBody = let;
                }
                try {
                    Saxon.Hej.om.StructuredQName fName = function.getFunctionName();
                    trace((("Moved function "+(((fName == null) ? "(anonymous)" : fName.getDisplayName())))+" inline: "), newBody);
                    newBody = newBody.simplify();
                    bool saveSuppressed = visitor.isSuppressWarnings();
                    visitor.setSuppressWarnings(true);
                    newBody = newBody.typeCheck(visitor, contextItemType);
                    newBody = Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(newBody, null, visitor, contextItemType, true);
                    if ((function.getDeterminism() != Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE)) {
                        newBody.setStaticProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
                    }
                    visitor.setSuppressWarnings(saveSuppressed);
                    functionCall.setBeingInlined(false);
                    return newBody;
                } catch (Saxon.Hej.trans.XPathException e) {
                    return functionCall;
                } finally {
                    visitor.setInliningFunctions(wasInlining);
                }
            } else {
                return functionCall;
            }
        }
        public override Saxon.Hej.expr.Expression trySwitch(Saxon.Hej.expr.instruct.Choose choose, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if ((!("EE".Equals(visitor.getTargetEdition())) || !(isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.SWITCH)))) {
                return choose;
            }
            if ((choose.size()<4)) {
                return choose;
            }
            Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo switchCaseInfo = new Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo();
            switchCaseInfo.commonlhs = null;
            switchCaseInfo.commontype = null;
            switchCaseInfo.index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.expr.Expression>(choose.size());
            string defaultCollation = choose.getRetainedStaticContext().getDefaultCollationName();
            try {
                switchCaseInfo.collation = getConfiguration().getCollation(defaultCollation);
            } catch (Saxon.Hej.trans.XPathException e) {
                return choose;
            }
            switchCaseInfo.defaultAction = Saxon.Hej.expr.Literal.makeEmptySequence();
            int size = choose.size();
            for (int c = 0; (c<size); c++) {
                Saxon.Hej.expr.Expression condition = choose.getCondition(c);
                bool isOtherwise = ((c == (size-1)) && Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(condition, true));
                if (isOtherwise) {
                    switchCaseInfo.defaultAction = choose.getAction(c);
                } else {
                    if (!(defaultCollation.Equals(condition.getRetainedStaticContext().getDefaultCollationName()))) {
                        return choose;
                    }
                    if (!(processSwitchCondition(condition, choose.getAction(c), switchCaseInfo))) {
                        return choose;
                    }
                }
            }
            if (!(switchCaseInfo.makeIndex())) {
                return null;
            }
            return new Saxon.Eej.ee.optim.SwitchExpression(switchCaseInfo);
        }
        private bool processSwitchCondition(Saxon.Hej.expr.Expression condition, Saxon.Hej.expr.Expression action, Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo switchCaseInfo) {
            if ((condition is Saxon.Hej.expr.OrExpression)) {
                bool b = processSwitchCondition(((Saxon.Hej.expr.OrExpression)(condition)).getLhsExpression(), action, switchCaseInfo);
                return (b && processSwitchCondition(((Saxon.Hej.expr.OrExpression)(condition)).getRhsExpression(), action, switchCaseInfo));
            }
            if (!(((condition is Saxon.Hej.expr.ComparisonExpression)))) {
                return false;
            }
            if ((condition is Saxon.Hej.expr.GeneralComparison)) {
                return false;
            }
            if ((((Saxon.Hej.expr.ComparisonExpression)(condition)).getSingletonOperator() != Saxon.Hej.expr.parser.Token.FEQ)) {
                return false;
            }
            Saxon.Hej.expr.Expression lhs = ((Saxon.Hej.expr.ComparisonExpression)(condition)).getLhsExpression();
            Saxon.Hej.expr.Expression rhs = ((Saxon.Hej.expr.ComparisonExpression)(condition)).getRhsExpression();
            if (!(Saxon.Hej.expr.Literal.isAtomic(rhs))) {
                return false;
            }
            if (((((Saxon.Hej.expr.ComparisonExpression)(condition)).getAtomicComparer() is Saxon.Hej.expr.sort.UntypedNumericComparer) || (((condition is Saxon.Hej.expr.ValueComparison) && ((Saxon.Hej.expr.ValueComparison)(condition)).convertsUntypedToOther())))) {
                Saxon.Hej.expr.UntypedSequenceConverter usc = new Saxon.Hej.expr.UntypedSequenceConverter(lhs.copy(new Saxon.Hej.expr.parser.RebindingMap()), Saxon.Hej.type.BuiltInAtomicType.DOUBLE);
                Saxon.Hej.type.Converter conv = usc.allocateConverter(getConfiguration(), true);
                usc.setConverter(conv);
                lhs = usc;
            }
            if ((switchCaseInfo.commonlhs == null)) {
                switchCaseInfo.commonlhs = lhs;
                switchCaseInfo.commontype = (Saxon.Hej.type.BuiltInAtomicType)(rhs.getItemType().getPrimitiveItemType());
                if (!(switchCaseInfo.commontype.isOrdered(false))) {
                    return false;
                }
            } else {
                if (!(switchCaseInfo.commonlhs.isEqual(lhs))) {
                    return false;
                }
                if (!(switchCaseInfo.commontype.Equals(rhs.getItemType().getPrimitiveItemType()))) {
                    return false;
                }
            }
            Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(rhs)).getGroundedValue());
            if ((value is Saxon.Hej.value.CalendarValue)) {
                return false;
            }
            switchCaseInfo.values.Add(value);
            switchCaseInfo.actions.Add(action);
            return true;
        }
        public int gatherOrExpressions(int commonParts, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Expression>> map, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> otherExps, Saxon.Hej.expr.Expression expr) {
            if ((expr is Saxon.Hej.expr.OrExpression)) {
                Saxon.Hej.expr.OrExpression orExpr = (Saxon.Hej.expr.OrExpression)(expr);
                commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExps, orExpr.getLhsExpression());
                commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExps, orExpr.getRhsExpression());
                return commonParts;
            } else {
                if ((expr is Saxon.Hej.expr.ComparisonExpression)) {
                    Saxon.Hej.expr.ComparisonExpression compExpr = (Saxon.Hej.expr.ComparisonExpression)(expr);
                    int op;
                    if ((compExpr is Saxon.Hej.expr.BinaryExpression)) {
                        Saxon.Hej.expr.BinaryExpression exprBin = (Saxon.Hej.expr.BinaryExpression)(expr);
                        op = exprBin.getOperator();
                    } else {
                        if ((compExpr is Saxon.Hej.expr.CompareToConstant)) {
                            Saxon.Hej.expr.CompareToConstant intConst = (Saxon.Hej.expr.CompareToConstant)(expr);
                            op = intConst.getSingletonOperator();
                        } else {
                            otherExps.Add(expr);
                            return commonParts;
                        }
                    }
                    Saxon.Hej.expr.Expression left = compExpr.getLhsExpression();
                    Saxon.Hej.expr.Expression right = compExpr.getRhsExpression();
                    if (!(compExpr.convertsUntypedToOther())) {
                        if (((left.getItemType().getUType().overlaps(Saxon.Hej.type.UType.UNTYPED_ATOMIC) && !(Saxon.Hej.type.UType.STRING_LIKE.subsumes(right.getItemType().getUType()))) || ((right.getItemType().getUType().overlaps(Saxon.Hej.type.UType.UNTYPED_ATOMIC) && !(Saxon.Hej.type.UType.STRING_LIKE.subsumes(left.getItemType().getUType())))))) {
                            otherExps.Add(expr);
                            return commonParts;
                        }
                    }
                    if ((((op == Saxon.Hej.expr.parser.Token.FEQ)) || (op == Saxon.Hej.expr.parser.Token.EQUALS))) {
                        Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                        commonParts = comparisonExprFixup(commonParts, map, otherExps, th, expr, left, right);
                        return commonParts;
                    }
                }
                otherExps.Add(expr);
                return commonParts;
            }
        }
        public override Saxon.Hej.expr.Expression tryGeneralComparison(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.OrExpression orExpr) {
            int commonParts = 0;
            System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Expression>> map = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression, System.Collections.Generic.IList<Saxon.Hej.expr.Expression>>();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> otherExp = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExp, orExpr);
            if ((commonParts<=1)) {
                return orExpr;
            }
            Saxon.Hej.expr.Expression newExpr = null;
            Saxon.Hej.expr.Expression previous = null;
            Saxon.Hej.expr.parser.RetainedStaticContext nsResolver = orExpr.getRetainedStaticContext();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Expression>> entry in map) {
                Saxon.Hej.expr.Expression comparison = new Saxon.Hej.expr.GeneralComparison20(entry.Key, Saxon.Hej.expr.parser.Token.EQUALS, Saxon.Hej.expr.instruct.Block.makeBlock(entry.Value));
                comparison.setRetainedStaticContext(nsResolver);
                Saxon.Hej.expr.Expression e = comparison.typeCheck(visitor, contextItemType);
                if ((e != null)) {
                    comparison = e;
                }
                if ((previous != null)) {
                    newExpr = new Saxon.Hej.expr.OrExpression(previous, comparison);
                    previous = newExpr;
                } else {
                    previous = comparison;
                    newExpr = previous;
                }
            }
            previous = newExpr;
            foreach (Saxon.Hej.expr.Expression expri in otherExp) {
                if ((previous != null)) {
                    newExpr = new Saxon.Hej.expr.OrExpression(previous, expri);
                    previous = newExpr;
                } else {
                    previous = expri;
                }
            }
            if ((newExpr != null)) {
                newExpr = newExpr.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                if ((newExpr is Saxon.Hej.expr.GeneralComparison)) {
                    return optimizeGeneralComparison(visitor, (Saxon.Hej.expr.GeneralComparison)(newExpr), false, contextItemType);
                } else {
                    return newExpr;
                }
            }
            return orExpr;
        }
        private int comparisonExprFixup(int commonParts, System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Expression>> map, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> otherExp, Saxon.Hej.type.TypeHierarchy th, Saxon.Hej.expr.Expression second, Saxon.Hej.expr.Expression left, Saxon.Hej.expr.Expression right) {
            bool checkSecondExpr = (second is Saxon.Hej.expr.ValueComparison);
            bool checkOperands = (((checkSecondExpr && (((left.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)) == 0)) && (th.relationship(left.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)) && ((((th.relationship(right.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING) != Saxon.Hej.type.Affinity.DISJOINT)) || ((th.relationship(right.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)))));
            checkOperands = (checkSecondExpr && ((checkOperands || ((((((right.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)) == 0) && (th.relationship(right.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)) && ((((th.relationship(left.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING) != Saxon.Hej.type.Affinity.DISJOINT)) || ((th.relationship(left.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)))))))));
            if (checkOperands) {
                otherExp.Add(second);
            } else {
                if (!(map.ContainsKey(left))) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> valueList = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                    valueList.Add(right);
                    Saxon.Impl.Helpers.MapUtils.Put(map, left, valueList);
                } else {
                    Saxon.Impl.Helpers.MapUtils.Get(map, left).Add(right);
                    commonParts++;
                }
            }
            return commonParts;
        }
        public override Saxon.Hej.expr.Expression promoteExpressionsToGlobal(Saxon.Hej.expr.Expression expr, Saxon.Hej.trans.GlobalVariableManager globalVariableManager, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if (isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.EXTRACT_GLOBALS)) {
                Saxon.Eej.ee.optim.GlobalExtractor extractor = new Saxon.Eej.ee.optim.GlobalExtractor(expr, globalVariableManager, visitor);
                return extractor.extractGlobals();
            } else {
                return null;
            }
        }
        public override Saxon.Hej.expr.Expression eliminateCommonSubexpressions(Saxon.Hej.expr.Expression @in) {
            if (isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.COMMON_SUBEXPRESSIONS)) {
                return new Saxon.Eej.ee.optim.CommonSubexpressionPromoter(this).promoteCommonSubexpressions(@in);
            } else {
                return @in;
            }
        }
        public override Saxon.Hej.expr.Expression generateMultithreadedInstruction(Saxon.Hej.expr.Expression instruction) {
            if (((instruction is Saxon.Hej.expr.instruct.ForEach) && config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING))) {
                Saxon.Hej.expr.instruct.ForEach @base = (Saxon.Hej.expr.instruct.ForEach)(instruction);
                Saxon.Eej.ee.parallel.MultithreadedForEach result = new Saxon.Eej.ee.parallel.MultithreadedForEach(@base.getSelectExpression(), @base.getActionExpression(), false, @base.getNumberOfThreadsExpression());
                result.setInstruction(@base.isInstruction());
                return result;
            } else {
                return instruction;
            }
        }
        public override Saxon.Hej.expr.Expression optimizeNumberInstruction(Saxon.Hej.expr.instruct.NumberInstruction ni, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if (((ni.getLevel() == Saxon.Hej.expr.instruct.NumberInstruction.ANY) && (ni.getSelect() is Saxon.Hej.expr.ContextItemExpression))) {
                Saxon.Hej.pattern.Pattern count = ni.getCount();
                if ((count == null)) {
                    if ((contextInfo.getItemType() is Saxon.Hej.pattern.NameTest)) {
                        count = new Saxon.Hej.pattern.NodeTestPattern((Saxon.Hej.pattern.NameTest)(contextInfo.getItemType()));
                    } else {
                        return null;
                    }
                }
                Saxon.Hej.pattern.Pattern from = ni.getFrom();
                if ((Saxon.Hej.pattern.Pattern.patternContainsVariable(count) || Saxon.Hej.pattern.Pattern.patternContainsVariable(from))) {
                    return null;
                }
                if (((count is Saxon.Hej.pattern.PatternThatSetsCurrent) || (from is Saxon.Hej.pattern.PatternThatSetsCurrent))) {
                    return null;
                }
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                if ((th.relationship(contextInfo.getItemType(), Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE) != Saxon.Hej.type.Affinity.DISJOINT)) {
                    return null;
                }
                if ((th.relationship(contextInfo.getItemType(), Saxon.Hej.pattern.NodeKindTest.NAMESPACE) != Saxon.Hej.type.Affinity.DISJOINT)) {
                    return null;
                }
                Saxon.Hej.expr.instruct.SlotManager slots = getConfiguration().makeSlotManager();
                Saxon.Hej.om.StructuredQName valueName = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "value");
                slots.allocateSlotNumber(valueName);
                Saxon.Hej.expr.SuppliedParameterReference @ref = new Saxon.Hej.expr.SuppliedParameterReference(0);
                Saxon.Hej.expr.ArithmeticExpression increment = new Saxon.Hej.expr.ArithmeticExpression(@ref, Saxon.Hej.expr.parser.Token.PLUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.PLUS_ONE, ni));
                increment.setCalculator(Saxon.Hej.expr.Calculator.INTEGER_INTEGER[Saxon.Hej.expr.Calculator.PLUS]);
                increment.setRetainedStaticContext(ni.getRetainedStaticContext());
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(increment, 1, slots);
                Saxon.Hej.expr.accum.AccumulatorRule ruleTarget = new Saxon.Hej.expr.accum.AccumulatorRule(increment, slots, false);
                Saxon.Hej.trans.rules.Rule rule = new Saxon.Hej.trans.rules.Rule(count, ruleTarget, 0, 0, 0, 0, 0);
                Saxon.Hej.om.StructuredQName genName = new Saxon.Hej.om.StructuredQName("anon", Saxon.Hej.om.NamespaceUri.ANONYMOUS, ("n"+ni.GetHashCode()));
                Saxon.Hej.trans.SimpleMode rules = new Saxon.Hej.trans.SimpleMode(genName);
                rules.addRule(count, rule);
                if ((from != null)) {
                    ruleTarget = new Saxon.Hej.expr.accum.AccumulatorRule(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.ZERO, ni), slots, false);
                    rule = new Saxon.Hej.trans.rules.Rule(from, ruleTarget, 1, 0, 1, 0, 0);
                    rules.addRule(from, rule);
                    if ((th.relationship(count.getItemType(), from.getItemType()) != Saxon.Hej.type.Affinity.DISJOINT)) {
                        Saxon.Hej.pattern.Pattern both = new Saxon.Hej.pattern.IntersectPattern(from, count);
                        ruleTarget = new Saxon.Hej.expr.accum.AccumulatorRule(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.PLUS_ONE, ni), slots, false);
                        rule = new Saxon.Hej.trans.rules.Rule(both, ruleTarget, 2, 0, 2, 0, 0);
                        rules.addRule(both, rule);
                    }
                }
                rules.allocateAllPatternSlots();
                try {
                    rules.computeRankings(1);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return null;
                }
                Saxon.Hej.expr.accum.Accumulator acc = new Saxon.Hej.expr.accum.Accumulator();
                acc.setType(Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
                acc.setPreDescentRules(rules);
                acc.setAccumulatorName(genName);
                acc.setInitialValueExpression(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.ZERO, ni));
                acc.setSlotManagerForInitialValueExpression(getConfiguration().makeSlotManager());
                acc.setUniversallyApplicable(true);
                Saxon.Hej.style.StylesheetPackage pack = (Saxon.Hej.style.StylesheetPackage)(ni.getPackageData());
                Saxon.Hej.expr.accum.AccumulatorRegistry registry = pack.getAccumulatorRegistry();
                registry.addAccumulator(acc);
                Saxon.Hej.expr.Component co = Saxon.Hej.expr.Component.makeComponent(acc, Saxon.Hej.trans.Visibility.PRIVATE, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED, pack, pack);
                acc.setDeclaringComponent(co);
                try {
                    Saxon.Hej.functions.SystemFunction fn = Saxon.Hej.functions.registry.XSLT30FunctionSet.getInstance().makeFunction("accumulator-before", 1);
                    fn.setRetainedStaticContext(ni.getRetainedStaticContext());
                    Saxon.Hej.expr.Expression call = fn.makeFunctionCall(new Saxon.Hej.expr.StringLiteral(genName.getEQName()));
                    Saxon.Hej.expr.Expression nonZero = new Saxon.Hej.expr.CompareToIntegerConstant(new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.expr.parser.Token.FNE, 0);
                    Saxon.Hej.expr.Expression result = new Saxon.Hej.expr.FilterExpression(call, nonZero);
                    if (!(((ni.getSelect() is Saxon.Hej.expr.ContextItemExpression)))) {
                        result = new Saxon.Hej.expr.SlashExpression(ni.getSelect(), result);
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(ni, result);
                    result.setRetainedStaticContext(ni.getRetainedStaticContext());
                    trace("Replaced xsl:number instruction by accumulator", result);
                    return result;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            return null;
        }
        public override void assessFunctionStreamability(Saxon.Hej.style.XSLFunction sourceFunction, Saxon.Hej.expr.instruct.UserFunction compiledFunction) {
            Saxon.Hej.trans.FunctionStreamability ability = compiledFunction.getDeclaredStreamability();
            if ((ability == Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                return ;
            }
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(compiledFunction.getBody(), Saxon.Eej.ee.trans.ContextItemStaticInfoEE.ABSENT, reasons);
            switch (ability.innerEnumValue) {
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ABSORBING:
                {
                    if (((ps.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) || (ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                        Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder((("Function "+compiledFunction.getFunctionName().getDisplayName())+" is not streamable"));
                        foreach (string reason in reasons) {
                            message.append(". ").append(reason);
                        }
                        unstreamableFunction(sourceFunction, compiledFunction, message.ToString());
                    } else {
                        if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                            string message = "A function with streamability=absorbing must return grounded items";
                            unstreamableFunction(sourceFunction, compiledFunction, message);
                        }
                    }
                    return ;
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ASCENT:
                {
                    if (Saxon.Hej.value.Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                        string message = ("The first argument of a function with streamability=ascent "+"must not accept a sequence of more than one item");
                        unstreamableFunction(sourceFunction, compiledFunction, message);
                    } else {
                        if (((ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED) && (ps.getPosture() != Saxon.Eej.ee.stream.Posture.CLIMBING))) {
                            string message = ("A function with streamability=ascent must return either grounded items "+"or nodes reachable via the ancestor-or-self axis");
                            unstreamableFunction(sourceFunction, compiledFunction, message);
                        } else {
                            if ((ps.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                                string message = "A function with streamability=ascent must not consume the input stream";
                                unstreamableFunction(sourceFunction, compiledFunction, message);
                            }
                        }
                    }
                    return ;
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.FILTER:
                {
                    if (Saxon.Hej.value.Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                        string message = ("The first argument of a function with streamability=filter "+"must not accept a sequence of more than one item");
                        unstreamableFunction(sourceFunction, compiledFunction, message);
                    } else {
                        if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.STRIDING)) {
                            string message = "A function with streamability=filter must return striding nodes";
                            unstreamableFunction(sourceFunction, compiledFunction, message);
                        } else {
                            if ((ps.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                                string message = "A function with streamability=filter must not consume the input stream";
                                unstreamableFunction(sourceFunction, compiledFunction, message);
                            }
                        }
                    }
                    return ;
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.INSPECTION:
                {
                    if (Saxon.Hej.value.Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                        string message = ("The first argument of a function with streamability=inspection "+"must not accept a sequence of more than one item");
                        unstreamableFunction(sourceFunction, compiledFunction, message);
                    } else {
                        if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                            string message = "A function with streamability=inspection must not return streamed nodes";
                            unstreamableFunction(sourceFunction, compiledFunction, message);
                        } else {
                            if ((ps.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                                string message = "A function with streamability=inspection must not consume the input stream";
                                unstreamableFunction(sourceFunction, compiledFunction, message);
                            }
                        }
                    }
                    return ;
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.SHALLOW_DESCENT:
                {
                    if (Saxon.Hej.value.Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                        string message = ("The first argument of a function with streamability=shallow-descent "+"must not accept a sequence of more than one item");
                        unstreamableFunction(sourceFunction, compiledFunction, message);
                    } else {
                        if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.STRIDING)) {
                            string message = ("A function with streamability=shallow-descent must be striding "+"(that is, must return nodes reachable via the child or attribute axis)");
                            unstreamableFunction(sourceFunction, compiledFunction, message);
                        } else {
                            if ((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING)) {
                                string message = "The function body is free-ranging";
                                unstreamableFunction(sourceFunction, compiledFunction, message);
                            }
                        }
                    }
                    return ;
                }
              default:
                break;
            }
        }
        private void unstreamableFunction(Saxon.Hej.style.XSLFunction fn, Saxon.Hej.expr.instruct.UserFunction compiledFunction, string message) {
            if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK)) {
                message += ". Falling back to unstreamed implementation";
                fn.issueWarning(message, "XTSE3430");
                compiledFunction.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
                fn.getCompilation().setFallbackToNonStreaming(true);
            } else {
                fn.compileError(message, "XTSE3430");
            }
        }
    
    }
}