using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.compat {
    public class GeneralComparison10 : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.Callable {
        protected internal int singletonOperator;
        protected internal Saxon.Hej.expr.sort.AtomicComparer comparer;
        private bool atomize0 = true;
        private bool atomize1 = true;
        private bool maybeBoolean0 = true;
        private bool maybeBoolean1 = true;
        public GeneralComparison10(Saxon.Hej.expr.Expression p0, int op, Saxon.Hej.expr.Expression p1) : base (p0, op, p1) {
            singletonOperator = Saxon.Hej.expr.GeneralComparison.getCorrespondingSingletonOperator(op);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            Saxon.Hej.lib.StringCollator comp = visitor.getConfiguration().getCollation(env.getDefaultCollationName());
            if ((comp == null)) {
                comp = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            Saxon.Hej.expr.XPathContext context = env.makeEarlyEvaluationContext();
            comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(comp, context);
            if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(context), this);
            }
            return this;
        }
        public void setAtomicComparer(Saxon.Hej.expr.sort.AtomicComparer comparer) {
            this.comparer = comparer;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            getLhs().optimize(visitor, contextInfo);
            getRhs().optimize(visitor, contextInfo);
            setLhsExpression(getLhsExpression().unordered(false, false));
            setRhsExpression(getRhsExpression().unordered(false, false));
            if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(env.makeEarlyEvaluationContext()), this);
            }
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.type.ItemType type0 = getLhsExpression().getItemType();
            Saxon.Hej.type.ItemType type1 = getRhsExpression().getItemType();
            if (type0.isPlainType()) {
                atomize0 = false;
            }
            if (type1.isPlainType()) {
                atomize1 = false;
            }
            if ((th.relationship(type0, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) == Saxon.Hej.type.Affinity.DISJOINT)) {
                maybeBoolean0 = false;
            }
            if ((th.relationship(type1, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) == Saxon.Hej.type.Affinity.DISJOINT)) {
                maybeBoolean1 = false;
            }
            if ((!(maybeBoolean0) && !(maybeBoolean1))) {
                if (!(((type0 is Saxon.Hej.type.AtomicType)))) {
                    setLhsExpression(Saxon.Hej.expr.Atomizer.makeAtomizer(getLhsExpression(), null).simplify());
                    type0 = getLhsExpression().getItemType();
                }
                if (!(((type1 is Saxon.Hej.type.AtomicType)))) {
                    setRhsExpression(Saxon.Hej.expr.Atomizer.makeAtomizer(getRhsExpression(), null).simplify());
                    type1 = getRhsExpression().getItemType();
                }
                Saxon.Hej.type.Affinity n0 = th.relationship(type0, Saxon.Hej.type.NumericType.getInstance());
                Saxon.Hej.type.Affinity n1 = th.relationship(type1, Saxon.Hej.type.NumericType.getInstance());
                bool maybeNumeric0 = (n0 != Saxon.Hej.type.Affinity.DISJOINT);
                bool maybeNumeric1 = (n1 != Saxon.Hej.type.Affinity.DISJOINT);
                bool numeric0 = ((n0 == Saxon.Hej.type.Affinity.SUBSUMED_BY) || (n0 == Saxon.Hej.type.Affinity.SAME_TYPE));
                bool numeric1 = ((n1 == Saxon.Hej.type.Affinity.SUBSUMED_BY) || (n1 == Saxon.Hej.type.Affinity.SAME_TYPE));
                if (((@operator == Saxon.Hej.expr.parser.Token.EQUALS) || (@operator == Saxon.Hej.expr.parser.Token.NE))) {
                    if ((((!(maybeNumeric0) && !(maybeNumeric1))) || ((numeric0 && numeric1)))) {
                        Saxon.Hej.expr.GeneralComparison gc = new Saxon.Hej.expr.GeneralComparison20(getLhsExpression(), @operator, getRhsExpression());
                        gc.setRetainedStaticContext(getRetainedStaticContext());
                        gc.setAtomicComparer(comparer);
                        Saxon.Hej.expr.Expression binExp = visitor.obtainOptimizer().optimizeGeneralComparison(visitor, gc, false, contextInfo);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, binExp);
                        return binExp.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                    }
                } else {
                    if ((numeric0 && numeric1)) {
                        Saxon.Hej.expr.GeneralComparison gc = new Saxon.Hej.expr.GeneralComparison20(getLhsExpression(), @operator, getRhsExpression());
                        gc.setRetainedStaticContext(getRetainedStaticContext());
                        Saxon.Hej.expr.Expression binExp = visitor.obtainOptimizer().optimizeGeneralComparison(visitor, gc, false, contextInfo);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, binExp);
                        return binExp.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                    }
                }
            }
            return this;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(arguments[0].iterate(), arguments[1].iterate(), context));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return effectiveBooleanValue(getLhsExpression().iterate(context), getRhsExpression().iterate(context), context);
        }
        private bool effectiveBooleanValue(Saxon.Hej.om.SequenceIterator iter0, Saxon.Hej.om.SequenceIterator iter1, Saxon.Hej.expr.XPathContext context) {
            if (maybeBoolean0) {
                Saxon.Hej.om.Item i01 = iter0.next();
                Saxon.Hej.om.Item i02 = ((i01 == null) ? null : iter0.next());
                if (((i01 is Saxon.Hej.value.BooleanValue) && (i02 == null))) {
                    iter0.close();
                    bool b = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iter1);
                    return compare((Saxon.Hej.value.BooleanValue)(i01), singletonOperator, Saxon.Hej.value.BooleanValue.get(b), comparer, context);
                }
                if (((i01 == null) && !(maybeBoolean1))) {
                    iter0.close();
                    return false;
                }
                if ((i02 != null)) {
                    iter0 = new Saxon.Hej.tree.iter.PrependSequenceIterator(i02, iter0);
                }
                if ((i01 != null)) {
                    iter0 = new Saxon.Hej.tree.iter.PrependSequenceIterator(i01, iter0);
                }
            }
            if (maybeBoolean1) {
                Saxon.Hej.om.Item i11 = iter1.next();
                Saxon.Hej.om.Item i12 = ((i11 == null) ? null : iter1.next());
                if (((i11 is Saxon.Hej.value.BooleanValue) && (i12 == null))) {
                    iter1.close();
                    bool b = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iter0);
                    return compare(Saxon.Hej.value.BooleanValue.get(b), singletonOperator, (Saxon.Hej.value.BooleanValue)(i11), comparer, context);
                }
                if (((i11 == null) && !(maybeBoolean0))) {
                    iter1.close();
                    return false;
                }
                if ((i12 != null)) {
                    iter1 = new Saxon.Hej.tree.iter.PrependSequenceIterator(i12, iter1);
                }
                if ((i11 != null)) {
                    iter1 = new Saxon.Hej.tree.iter.PrependSequenceIterator(i11, iter1);
                }
            }
            if (atomize0) {
                iter0 = Saxon.Hej.expr.Atomizer.getAtomizingIterator(iter0, false);
            }
            if (atomize1) {
                iter1 = Saxon.Hej.expr.Atomizer.getAtomizingIterator(iter1, false);
            }
            if (((iter0 is Saxon.Hej.tree.iter.EmptyIterator) || (iter1 is Saxon.Hej.tree.iter.EmptyIterator))) {
                return false;
            }
            if (((((@operator == Saxon.Hej.expr.parser.Token.LT) || (@operator == Saxon.Hej.expr.parser.Token.LE)) || (@operator == Saxon.Hej.expr.parser.Token.GT)) || (@operator == Saxon.Hej.expr.parser.Token.GE))) {
                Saxon.Hej.Configuration config = context.getConfiguration();
                Saxon.Hej.expr.ItemMappingFunction map = Saxon.Hej.expr.ItemMapper.of(item => Saxon.Hej.functions.Number_1.convert((Saxon.Hej.value.AtomicValue)(item), config));
                iter0 = new Saxon.Hej.expr.ItemMappingIterator(iter0, map, true);
                iter1 = new Saxon.Hej.expr.ItemMappingIterator(iter1, map, true);
            }
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> seq1 = null;
            Saxon.Hej.value.AtomicValue item0;
            while (((item0 = (Saxon.Hej.value.AtomicValue)(iter0.next())) != null)) {
                if ((iter1 != null)) {
                    while (true) {
                        Saxon.Hej.value.AtomicValue item1 = (Saxon.Hej.value.AtomicValue)(iter1.next());
                        if ((item1 == null)) {
                            iter1 = null;
                            if ((seq1 == null)) {
                                return false;
                            }
                            break ;
                        }
                        try {
                            if (compare(item0, singletonOperator, item1, comparer, context)) {
                                return true;
                            }
                            if ((seq1 == null)) {
                                seq1 = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(40);
                            }
                            seq1.Add(item1);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            e.maybeSetLocation(getLocation());
                            e.maybeSetContext(context);
                            throw;
                        }
                    }
                } else {
                    foreach (Saxon.Hej.value.AtomicValue item1 in seq1) {
                        if (compare(item0, singletonOperator, item1, comparer, context)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.compat.GeneralComparison10 gc = new Saxon.Hej.expr.compat.GeneralComparison10(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, gc);
            gc.setRetainedStaticContext(getRetainedStaticContext());
            gc.comparer = comparer;
            gc.atomize0 = atomize0;
            gc.atomize1 = atomize1;
            gc.maybeBoolean0 = maybeBoolean0;
            gc.maybeBoolean1 = maybeBoolean1;
            return gc;
        }
        private static bool compare(Saxon.Hej.value.AtomicValue a0, int @operator, Saxon.Hej.value.AtomicValue a1, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context) {
            comparer = comparer.provideContext(context);
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.type.BuiltInAtomicType t0 = a0.getPrimitiveType();
            Saxon.Hej.type.BuiltInAtomicType t1 = a1.getPrimitiveType();
            if ((t0.isPrimitiveNumeric() || t1.isPrimitiveNumeric())) {
                Saxon.Hej.value.DoubleValue v0 = Saxon.Hej.functions.Number_1.convert(a0, context.getConfiguration());
                Saxon.Hej.value.DoubleValue v1 = Saxon.Hej.functions.Number_1.convert(a1, context.getConfiguration());
                return Saxon.Hej.expr.ValueComparison.compare(v0, @operator, v1, comparer, false);
            }
            if (((t0.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING)) || ((t0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                Saxon.Hej.value.StringValue s0 = new Saxon.Hej.value.StringValue(a0.getUnicodeStringValue());
                Saxon.Hej.value.StringValue s1 = new Saxon.Hej.value.StringValue(a1.getUnicodeStringValue());
                return Saxon.Hej.expr.ValueComparison.compare(s0, @operator, s1, comparer, false);
            }
            if (t0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                a0 = t1.getStringConverter(rules).convert(a0).asAtomic();
            }
            if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                a1 = t0.getStringConverter(rules).convert(a1).asAtomic();
            }
            return Saxon.Hej.expr.ValueComparison.compare(a0, @operator, a1, comparer, false);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("cardinality", "many-to-many (1.0)");
            @out.emitAttribute("comp", comparer.save());
        }
        protected internal override string tag() {
            return "gc10";
        }
    
    }
}