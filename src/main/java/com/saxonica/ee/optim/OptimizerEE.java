package com.saxonica.ee.optim;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.bytecode.ByteCodeCandidate;
import com.saxonica.ee.bytecode.CompiledExpression;
import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.parallel.MultithreadedForEach;
import com.saxonica.ee.stream.*;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import com.saxonica.ee.trans.TemplateRuleEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorRegistry;
import net.sf.saxon.expr.accum.AccumulatorRule;
import net.sf.saxon.expr.compat.GeneralComparison10;
import net.sf.saxon.expr.instruct.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.functions.*;
import net.sf.saxon.functions.registry.VendorFunctionSetHE;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;
import net.sf.saxon.lib.*;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.*;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.style.XSLFunction;
import net.sf.saxon.style.XSLTemplate;
import net.sf.saxon.trans.*;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.*;

/**
 * This class performs a number of optimizations that are specific to the Saxon-EE product.
 * The relevant methods override dummy methods (that generally perform no optimization) in the
 * superclass, which is used by Saxon-HE.
 */
public class OptimizerEE extends Optimizer {

    /**
     * Create the Saxon-EE optimizer
     *
     * @param config the Saxon-EE configuration
     */

    public OptimizerEE(Configuration config) {
        super(config);
    }

    /**
     * Optimize a value comparison
     * @param vc the value comparison expression
     * @param visitor the expression visitor
     * @param contextInfo static context item information
     * @return either the original value comparison, or an optimized equivalent
     * @throws XPathException if things go wrong
     */

    @Override
    public Expression optimizeValueComparison(
            ValueComparison vc, ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) 
            throws XPathException {

        Expression lhs = vc.getLhsExpression();
        Expression rhs = vc.getRhsExpression();
        int operator = vc.getOperator();
        boolean isCodepointCollation =
                vc.getRetainedStaticContext().getDefaultCollationName().equals(NamespaceConstant.CODEPOINT_COLLATION_URI);

        Expression e1 = super.optimizeValueComparison(vc, visitor, contextInfo);
        if (e1 != null && e1 != vc) {
            return e1;
        }

        Sequence value0 = null;
        Sequence value1 = null;

        if (lhs instanceof Literal) {
            value0 = ((Literal) lhs).getGroundedValue();
        }

        if (rhs instanceof Literal) {
            value1 = ((Literal) rhs).getGroundedValue();
        }

        if (isOptionSet(OptimizerOptions.CONSTANT_FOLDING)) {

            // evaluate the expression now if both arguments are constant

            if ((value0 != null) && (value1 != null)) {
                try {
                    AtomicValue r = vc.evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext());
                    return Literal.makeLiteral(r == null ? EmptySequence.getInstance() : r, vc);
                } catch (NoDynamicContextException e) {
                    // early evaluation failed, typically because the implicit context isn't available.
                    // Try again at run-time
                    return vc;
                }
            }
        }

        if (!isOptionSet(OptimizerOptions.MISCELLANEOUS)) {
            return vc;
        }

        // optimise count(x) eq N (or gt N, ne N, eq N, etc)
        // TODO: extend this to cases where N is not a literal

        if (lhs.isCallOn(Count.class) && Literal.isAtomic(rhs)) {
            Expression e2 = optimizeCount(vc, visitor, false);
            if (e2 != null) {
                return e2.optimize(visitor, contextInfo);
            }
        } else if (rhs.isCallOn(Count.class) && Literal.isAtomic(lhs)) {
            Expression e2 = optimizeCount(vc, visitor, true);
            if (e2 != null) {
                return e2.optimize(visitor, contextInfo);
            }
        }

        // optimise string-length(x) = 0, >0, !=0 etc

        if (lhs.isCallOn(StringLength_1.class) && isZero(value1)) {
            assert lhs instanceof SystemFunctionCall;
            Expression arg = ((SystemFunctionCall) lhs).getArg(0);
            switch (vc.getOperator()) {
                case Token.FEQ:
                case Token.FLE:
                    return SystemFunction.makeCall("not", vc.getRetainedStaticContext(), arg);
                case Token.FNE:
                case Token.FGT:
                    return SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), arg);
                case Token.FGE:
                    return Literal.makeLiteral(BooleanValue.TRUE, vc);
                case Token.FLT:
                    return Literal.makeLiteral(BooleanValue.FALSE, vc);
            }
        }

        // optimise (0 = string-length(x)), etc

        if (rhs.isCallOn(StringLength_1.class) && isZero(value0)) {
            assert rhs instanceof SystemFunctionCall;
            Expression arg = ((SystemFunctionCall) rhs).getArg(0);
            switch (vc.getOperator()) {
                case Token.FEQ:
                case Token.FGE:
                    return SystemFunction.makeCall("not", vc.getRetainedStaticContext(), arg);
                case Token.FNE:
                case Token.FLT:
                    return SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), arg);
                case Token.FLE:
                    return Literal.makeLiteral(BooleanValue.TRUE, vc);
                case Token.FGT:
                    return Literal.makeLiteral(BooleanValue.FALSE, vc);
            }
        }

        // optimise string="" etc
        // Note we can change S!="" to boolean(S) for cardinality zero-or-one, but we can only
        // change S="" to not(S) for cardinality exactly-one.

        int p0 = lhs.getItemType().getPrimitiveType();
        if ((p0 == StandardNames.XS_STRING ||
                     p0 == StandardNames.XS_ANY_URI ||
                     p0 == StandardNames.XS_UNTYPED_ATOMIC) &&
                vc.getResultWhenEmpty() != BooleanValue.TRUE &&
                rhs instanceof Literal &&
                ((Literal) rhs).getGroundedValue() instanceof StringValue &&
                ((StringValue) ((Literal) rhs).getGroundedValue()).isEmpty() &&
                isCodepointCollation) {

            switch (vc.getOperator()) {
                case Token.FNE:
                case Token.FGT:
                    return SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), lhs);
                case Token.FEQ:
                case Token.FLE:
                    if (lhs.getCardinality() == StaticProperty.EXACTLY_ONE) {
                        return SystemFunction.makeCall("not", vc.getRetainedStaticContext(), lhs);
                    }
                    break;
            }
        }

        // optimize "" = string etc

        int p1 = rhs.getItemType().getPrimitiveType();
        if ((p1 == StandardNames.XS_STRING ||
                     p1 == StandardNames.XS_ANY_URI ||
                     p1 == StandardNames.XS_UNTYPED_ATOMIC) &&
                vc.getResultWhenEmpty() != BooleanValue.TRUE &&
                lhs instanceof Literal &&
                ((Literal) lhs).getGroundedValue() instanceof StringValue &&
                ((StringValue) ((Literal) lhs).getGroundedValue()).isEmpty() &&
                isCodepointCollation) {

            switch (vc.getOperator()) {
                case Token.FNE:
                case Token.FLT:
                    return SystemFunction.makeCall("boolean", vc.getRetainedStaticContext(), rhs);
                case Token.FEQ:
                case Token.FGE:
                    if (rhs.getCardinality() == StaticProperty.EXACTLY_ONE) {
                        return SystemFunction.makeCall("not", vc.getRetainedStaticContext(), rhs);
                    }
                    break;
            }
        }

        // optimize comparison against an integer or string constant

        if (value1 instanceof StringValue &&
                lhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                vc.getResultWhenEmpty() != BooleanValue.TRUE &&
                isCodepointCollation &&
                BuiltInAtomicType.isStringLike(lhs.getItemType())) {
            return new CompareToStringConstant(lhs, vc.getOperator(), ((StringValue) value1).getUnicodeStringValue());
        }

        if (value0 instanceof StringValue &&
                rhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                vc.getResultWhenEmpty() != BooleanValue.TRUE &&
                isCodepointCollation &&
                BuiltInAtomicType.isStringLike(rhs.getItemType())) {
            return new CompareToStringConstant(rhs, Token.inverse(vc.getOperator()), ((StringValue) value0).getUnicodeStringValue());
        }

        // optimize comparison against an integer literal

        if (value1 instanceof Int64Value &&
                lhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                NumericType.isNumericType(lhs.getItemType())) {
            return new CompareToIntegerConstant(lhs, vc.getOperator(), ((Int64Value) value1).longValue());
        }

        if (value0 instanceof Int64Value &&
                rhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                NumericType.isNumericType(rhs.getItemType())) {
            return new CompareToIntegerConstant(rhs, Token.inverse(vc.getOperator()), ((Int64Value) value0).longValue());
        }

        // optimize (boolean expression) == (boolean literal)

        if (p0 == StandardNames.XS_BOOLEAN &&
                p1 == StandardNames.XS_BOOLEAN &&
                (operator == Token.FEQ || operator == Token.FNE) &&
                lhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                rhs.getCardinality() == StaticProperty.EXACTLY_ONE &&
                (lhs instanceof Literal || rhs instanceof Literal)) {
            Literal literal = (Literal) (lhs instanceof Literal ? lhs : rhs);
            Expression other = lhs instanceof Literal ? rhs : lhs;
            boolean negate = (operator == Token.FEQ) != ((BooleanValue) literal.getGroundedValue()).getBooleanValue();
            if (negate) {
                Expression fn = SystemFunction.makeCall("not", vc.getRetainedStaticContext(), other);
                assert fn != null;
                ExpressionTool.copyLocationInfo(vc, fn);
                return fn.optimize(visitor, contextInfo);
            } else {
                return other;
            }
        }


        // optimize generate-id(X) = generate-id(Y) as "X is Y"
        // This construct is often used in XSLT 1.0 stylesheets.
        // Only do this if we know the arguments are singletons, because "is" doesn't
        // do first-value extraction.

        if (lhs.isCallOn(GenerateId_1.class) && rhs.isCallOn(GenerateId_1.class)) {
            assert lhs instanceof SystemFunctionCall;
            SystemFunctionCall f0 = (SystemFunctionCall) lhs;
            assert rhs instanceof SystemFunctionCall;
            SystemFunctionCall f1 = (SystemFunctionCall) rhs;
            if (!Cardinality.allowsMany(f0.getArg(0).getCardinality()) &&
                    !Cardinality.allowsMany(f1.getArg(0).getCardinality()) &&
                    (operator == Token.FEQ)) {
                IdentityComparison id =
                        new IdentityComparison(f0.getArg(0),
                                               Token.IS,
                                               f1.getArg(0));
                id.setGenerateIdEmulation(true);
                ExpressionTool.copyLocationInfo(vc, id);
                return id.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
            }
        }

        return vc;
    }

    /**
     * Test whether an expression is constant zero
     *
     * @param v the value to be tested
     * @return true if the operand is the constant zero (of any numeric data type)
     */

    private static boolean isZero(Sequence v) {
        return v instanceof NumericValue && ((NumericValue) v).compareTo(0) == 0;
    }


    /**
     * Optimize comparisons of count(X) to a literal value. The objective here is to count items in the sequence only
     * until the result of the comparison is deducible; for example to evaluate (count(X)>2) we can stop at the third
     * item in the sequence.
     *
     * @param inverted true if the call to count(X) is the right-hand operand
     * @return the optimized expression, or null if no optimization is possible
     * @throws XPathException if an error occurs
     */

    /*@Nullable*/
    private Expression optimizeCount(ValueComparison vc, ExpressionVisitor visitor, boolean inverted) throws XPathException {
        SystemFunctionCall countFn = (SystemFunctionCall) (inverted ? vc.getRhsExpression() : vc.getLhsExpression());
        Expression sequence = countFn.getArg(0);
        sequence = sequence.unordered(true, false);

        AtomicValue literalOperand = (AtomicValue) ((Literal) (inverted ? vc.getLhsExpression() : vc.getRhsExpression())).getGroundedValue();
        int op = inverted ? Token.inverse(vc.getOperator()) : vc.getOperator();

        if (isZero(literalOperand)) {
            if (op == Token.FEQ || op == Token.FLE) {
                // rewrite count(x)=0 as empty(x)
                Expression result = SystemFunction.makeCall("empty", vc.getRetainedStaticContext(), sequence);
                trace("Rewrite count()=0 as:", result);
                return result;
            } else if (op == Token.FNE || op == Token.FGT) {
                // rewrite count(x)!=0, count(x)>0 as exists(x)
                Expression result = SystemFunction.makeCall("exists", vc.getRetainedStaticContext(), sequence);
                trace("Rewrite count()>0 as:", result);
                return result;
            } else if (op == Token.FGE) {
                // rewrite count(x)>=0 as true()
                return Literal.makeLiteral(BooleanValue.TRUE, vc);
            } else {  // singletonOperator == Token.FLT
                // rewrite count(x)<0 as false()
                return Literal.makeLiteral(BooleanValue.FALSE, vc);
            }
        } else if (literalOperand instanceof NumericValue) {
            long operand;
            if (literalOperand instanceof IntegerValue) {
                operand = ((IntegerValue) literalOperand).longValue();
            } else if (literalOperand.isNaN()) {
                return Literal.makeLiteral(BooleanValue.get(op == Token.FNE), vc);
            } else if (((NumericValue) literalOperand).isWholeNumber()) {
                operand = ((NumericValue) literalOperand).longValue();
            } else if (op == Token.FEQ) {
                return Literal.makeLiteral(BooleanValue.FALSE, vc);
            } else if (op == Token.FNE) {
                return Literal.makeLiteral(BooleanValue.TRUE, vc);
            } else if (op == Token.FGT || op == Token.FGE) {
                operand = ((NumericValue) literalOperand).ceiling().longValue();
                op = Token.FGE;
            } else /*if (op == Token.FLT || op == Token.FLE)*/ {
                operand = ((NumericValue) literalOperand).floor().longValue();
                op = Token.FLE;
            }
            if (operand < 0) {
                switch (op) {
                    case Token.FEQ:
                    case Token.FLT:
                    case Token.FLE:
                        return Literal.makeLiteral(BooleanValue.FALSE, vc);
                    default:
                        return Literal.makeLiteral(BooleanValue.TRUE, vc);
                }
            }
            if (operand > Integer.MAX_VALUE) {
                switch (op) {
                    case Token.FEQ:
                    case Token.FGT:
                    case Token.FGE:
                        return Literal.makeLiteral(BooleanValue.FALSE, vc);
                    default:
                        return Literal.makeLiteral(BooleanValue.TRUE, vc);
                }
            }
            if (sequence instanceof TailExpression || sequence.isCallOn(Subsequence_2.class)) {
                // it's probably the result of a previous optimization
                return null;
            }
            switch (op) {
                case Token.FEQ:
                case Token.FNE:
                case Token.FLE:
                case Token.FLT:
                    // rewrite count(E) op N as count(subsequence(E, 1, N+1)) op N
                    Expression ss = SystemFunction.makeCall("subsequence",
                                                            vc.getRetainedStaticContext(), sequence,
                                                            Literal.makeLiteral(Int64Value.PLUS_ONE, vc),
                                                            Literal.makeLiteral(Int64Value.makeIntegerValue(operand + 1), vc));
                    Expression ct = SystemFunction.makeCall("count", vc.getRetainedStaticContext(), ss);
                    CompareToIntegerConstant ctic = new CompareToIntegerConstant(ct, op, operand);
                    trace("Rewrite count()~N as:", ctic);
                    ExpressionTool.copyLocationInfo(vc, ctic);
                    return ctic;
                case Token.FGE:
                case Token.FGT:
                    // rewrite count(x) gt n as exists(x[n+1])
                    //     and count(x) ge n as exists(x[n])
                    TailExpression tail = new TailExpression(sequence, (int) (op == Token.FGE ? operand : operand + 1));
                    ExpressionTool.copyLocationInfo(vc, tail);
                    Expression result = SystemFunction.makeCall("exists", vc.getRetainedStaticContext(), tail);
                    ExpressionTool.copyLocationInfo(vc, result);
                    trace("Rewrite count()>=N as:", result);
                    return result;
            }
        }
        return null;
    }


    /**
     * Optimize a GeneralComparison expression. Saxon-EE has a number of strategies for handling these;
     * for example when comparing against a constant set of values, the expression is turned into
     * a call on map:contains().
     *
     * @param visitor             the expression visitor
     * @param gc                  the GeneralComparison to be optimized
     * @param backwardsCompatible true if in 1.0 compatibility mode
     * @return the simplified expression
     */

    @Override
    public Expression optimizeGeneralComparison(ExpressionVisitor visitor,
                                                GeneralComparison gc, boolean backwardsCompatible, ContextItemStaticInfo contextItemType) {
        if (gc instanceof GeneralComparisonEE) {
            return gc;
        }
        if (!"EE".equals(visitor.getTargetEdition())) {
            return gc;
        }
        if (gc.getOperator() == Token.EQUALS) {
            Expression p0 = gc.getLhsExpression();
            Expression p1 = gc.getRhsExpression();
            boolean many0 = Cardinality.expectsMany(p0);
            boolean many1 = Cardinality.expectsMany(p1);
            if (many0 || many1) {
                if (gc.getStringCollator() == CodepointCollator.getInstance()) {
                    // Optimize a test of the form ($X = (1, 7, 9, 10, 16, ....)) where the RHS is a constant list
                    if (p1 instanceof Literal && ((Literal) p1).getGroundedValue().getLength() > 8) {
                        Expression mapTest = makeMapTest(visitor, p0, (Literal) p1, contextItemType);
                        if (mapTest != null) {
                            return mapTest;
                        }
                    } else if (p0 instanceof Literal && ((Literal) p0).getGroundedValue().getLength() > 8) {
                        Expression mapTest = makeMapTest(visitor, p1, (Literal) p0, contextItemType);
                        if (mapTest != null) {
                            return mapTest;
                        }
                    }
                }
                GeneralComparisonEE gcee = new GeneralComparisonEE(p0, gc.getOperator(), p1);
                gcee.setRetainedStaticContext(gc.getRetainedStaticContext());
                gcee.setAtomicComparer(gc.getAtomicComparer());
                gcee.setComparisonCardinality(gc.getComparisonCardinality());
                gcee.setNeedsRuntimeCheck(gc.needsRuntimeCheck());
                return gcee;
            }
        }
        return gc;
    }

    /**
     * // Optimize a test of the form ($X = (1, 7, 9, 10, 16, ....)) where the RHS is a constant list
     *
     * @param visitor     the expression visitor
     * @param exp         the "$X" expression
     * @param literal     the list of literal values
     * @param contextInfo information about the context item
     * @return an expression that looks up whether the value of $X exists within a constant map
     */

    private Expression makeMapTest(ExpressionVisitor visitor, Expression exp,
                                   Literal literal, ContextItemStaticInfo contextInfo) {
        try {
            boolean allStrings = true;
            MapItem map;
            for (Item item : literal.getGroundedValue().asIterable()) {
                if (!(item instanceof StringValue)) {
                    allStrings = false;
                    break;
                }
            }
            if (allStrings) {
                DictionaryMap dict = new DictionaryMap();
                for (Item item : literal.getGroundedValue().asIterable()) {
                    dict.initialPut(item.getStringValue(), BooleanValue.TRUE);
                }
                map = dict;
            } else {
                HashTrieMap trie = new HashTrieMap();
                for (Item item : literal.getGroundedValue().asIterable()) {
                    if (item instanceof AtomicValue) {
                        // if the values have complex comparison semantics, give up
                        if (item instanceof CalendarValue && !((CalendarValue) item).hasTimezone()) {
                            return null;
                        }
                        if (((AtomicValue) item).isNaN()) {
                            return null;
                        }
                        if (((AtomicValue)item).isUntypedAtomic()) {
                            return null;
                        }
                        trie.initialPut((AtomicValue) item, BooleanValue.TRUE);
                    } else {
                        return null;
                    }
                }
                map = trie;
            }
            Expression e2;
            TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            SystemFunction containsFn;
            if (allStrings || th.relationship(exp.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) == Affinity.DISJOINT) {
                containsFn = MapFunctionSet.getInstance().makeFunction("contains", 2);
            } else {
                containsFn = VendorFunctionSetHE.getInstance().makeFunction("map-untyped-contains", 2);
            }
            if (exp.getCardinality() == StaticProperty.EXACTLY_ONE) {
                // rewrite $x = (A, B, C) as map{A:(), B:(), C:()} -> contains($x)
                e2 = containsFn.makeFunctionCall(Literal.makeLiteral(map, exp), exp);

            } else {
                // rewrite $x = (A, B, C) as exists($x[map{A:(), B:(), C:()} -> contains(.)])
                Expression contains = containsFn.makeFunctionCall(Literal.makeLiteral(map, exp), new ContextItemExpression());
                FilterExpression fe = new FilterExpression(exp, contains);
                e2 = SystemFunction.makeCall("exists", exp.getRetainedStaticContext(), fe);

            }
            assert e2 != null;
            e2.setRetainedStaticContext(exp.getRetainedStaticContext());
            e2 = e2.typeCheck(visitor, contextInfo);
            e2 = e2.optimize(visitor, contextInfo);
            return e2;
        } catch (XPathException e) {
            return null;
        }
    }

    /**
     * Attempt to optimize a copy operation. Return null if no optimization is possible.
     *
     * @param select the expression that selects the items to be copied
     * @return null if no optimization is possible, or an expression that does an optimized
     * copy of these items otherwise
     */

    /*@Nullable*/
    @Override
    public Expression optimizeSaxonStreamFunction(ExpressionVisitor visitor, ContextItemStaticInfo cisi, Expression select) throws XPathException {
        Expression e = super.optimizeSaxonStreamFunction(visitor, cisi, select);
        if (e != null) {
            return e;
        }
        if (!config.getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            streamingNotPossible("Cannot use saxon:stream(): multithreading is not enabled", select, visitor);
            return null;
        }

        if (!(select instanceof SlashExpression)) {
            if (select instanceof DocumentSorter) {
                streamingNotPossible("Cannot use saxon:stream(): expression is not provably in document order", select, visitor);
            } else {
                streamingNotPossible("Cannot use saxon:stream(): not a path expression", select, visitor);
            }
            return null;
        }
        SlashExpression pexp = (SlashExpression) select;
        Expression start = pexp.getFirstStep();
        if (!(start.isCallOn(Doc.class) || start.isCallOn(DocumentFn.class)) || ((SystemFunctionCall) start).getArity() != 1) {
            streamingNotPossible("Cannot use saxon:stream(): path must start with call on doc#1 or document#1", select, visitor);
            return null;
        }
        SystemFunctionCall documentExp = (SystemFunctionCall) start;
        Expression rest = pexp.getRemainingSteps();
        Expression snap = SystemFunction.makeCall("snapshot", select.getRetainedStaticContext(), rest);
        Set<? extends Accumulator> accList = Collections.emptySet();
        ParseOptions options = new ParseOptions(config.getParseOptions());
        options.setSchemaValidationMode(Validation.STRIP);
        options.setTopLevelType(null);
        options.setApplicableAccumulators(accList);
        StreamInstr stream = new StreamInstr(new ContextItemExpression(), snap, options);
        trace("Using streaming for saxon:stream", stream);
        ForEach each = new ForEach(documentExp.getArg(0), stream);
        return each.optimize(visitor, cisi);

    }

    private void streamingNotPossible(String reason, Expression select, ExpressionVisitor visitor) {
        trace(reason, select);
        visitor.issueWarning(reason, select.getLocation());
    }

    /**
     * In streaming mode, make the copy operation applied to subexpressions of a complex-content
     * sequence constructor into explicit copy-of operations. This makes constructs such as
     * <code>&lt;e&gt;&lt;xsl:sequence select="y"/&gt;&lt;/e&gt;</code> streamable, by using the same run-time
     * logic as if xsl:copy-of had been used.
     *
     * @param parent  The parent expression of the expression that is to be wrapped in an implicit copy-of.
     * @param childOp The operand that is to be wrapped in an implicit copy-of, if it is suitable.
     */
    @Override
    public void makeCopyOperationsExplicit(Expression parent, Operand childOp) {

        /*
        Streaming. 25 Aug 2016. There is 9.7 code at ParentNodeConstructor which calls "makeCopyOperationsExplicit";
        under particular conditions this adds xsl:copy operations to the tree. Removing this code appears to work well.
        But we would like to go further, and remove xsl:copy-of operations from the tree when this is feasible.
        Removing xsl:copy-of as a child of an element constructor works well and gives significant benefits in some
        cases, but it causes problems in others. For example it causes trouble in si-LRE-101, where the selected
        node ends up bypassing the validator associated with the containing element constructor. I think the
        problem here is the element constructor with xsl:validation="strict"; it should ensure that all events
        from its child instructions go to its (validating) receiver, but it fails to do so. - MHK
         */
        Expression child = childOp.getChildExpression();
        boolean atomic = child.getItemType() instanceof AtomicType || child.getItemType() instanceof FunctionItemType;
        boolean replace = false;
        if (child instanceof ContextItemExpression) {
            replace = true;
        }
        if (!atomic && child.hasSpecialProperty(StaticProperty.SUBTREE_NODESET)) {
            replace = true;
        }
        if (child instanceof OnEmptyExpr || child instanceof OnNonEmptyExpr) {
            replace = false;
        }

        if (replace) {
            CopyOf copy = new CopyOf(child, true, Validation.PRESERVE, null, false);
            childOp.setChildExpression(copy);
            return;
        }
        // Expressions such as Choose and Block can pass the implicit copy() wrapping down to their children.
        // More specifically, the request is passed to all operands with usage transmission, except in a couple
        // of special cases.
        if (child instanceof SimpleStepExpression) {
            return; // can't replace the operand here, it must be an axis expression
        } else if (child instanceof Fork) {
            return; // fork isn't allowed to return streamed nodes, creating a copy here would prevent the error
        }

        for (Operand action : child.operands()) {
            if (action.getOperandRole().getUsage() == OperandUsage.TRANSMISSION) {
                makeCopyOperationsExplicit(child, action);
            }
        }
    }

    /**
     * Check the streamability of a template
     *
     * @param sourceTemplate   the source of the template in the stylesheet tree
     * @param compiledTemplate the compiled template
     * @throws XPathException if the template is declared streamable but does not satisfy the straming rules
     */

    @Override
    public void checkStreamability(XSLTemplate sourceTemplate, TemplateRule compiledTemplate) throws XPathException {
        if (compiledTemplate.isDeclaredStreamable()) {
            boolean actuallyStreamable;
            List<String> reasons = new ArrayList<>();
            boolean enabled = getConfiguration().isStreamabilityEnabled();
            if (!enabled) {
                reasons.add("Streaming is disabled for this Saxon Configuration");
                actuallyStreamable = false;
            } else {
                actuallyStreamable = ((TemplateRuleEE)compiledTemplate).isActuallyStreamable(reasons);
            }
            if (!actuallyStreamable) {
                boolean fallback = getConfiguration().getBooleanProperty(Feature.STREAMING_FALLBACK);
                StringBuilder message = new StringBuilder("Template rule is declared streamable but it does not satisfy the streamability rules. ");
                for (String reason : reasons) {
                    message.append("\n  * ").append(reason);
                }
                XPathException err = new XPathException(message.toString());
                err.setErrorCode("XTSE3430");
                err.setLocator(sourceTemplate);
                if (fallback) {
                    message.append("\n  * Falling back to non-streaming implementation");
                    sourceTemplate.getStaticContext().issueWarning(message.toString(), sourceTemplate);
                    compiledTemplate.setDeclaredStreamable(false);
                } else {
                    throw err;
                }
            }

        }
    }

    /**
     * In streaming mode, optimize a ForExpression for streaming. This is possible if the "in" part
     * of the for expression does a downward selection of the streamed input, while the "return" part
     * does not access the streamed input at all (specifically, if it has no dependencies on the context node).
     * The expression is converted to streamable form by rewriting "for $t in S return f($t)" as
     * "for-each S return f(.)". This depends on the generated for-each expression being streamable,
     * which will not always be the case, for example if the return expression makes two downward selections
     * from the range variable, or a non-downward selection.
     *
     * @param expr the expression to be optimized
     * @return the optimized expression, or the original expression if no optimization was possible.
     */

    @Override
    public Expression optimizeForExpressionForStreaming(ForExpression expr) {
        Expression e2 = Streamability.rewriteForExpressionAsMappingExpression(expr);
        if (e2 != null) {
            return e2;
        } else {
            return expr;
        }

//        if (ExpressionInverter.consumesStream(expr.getSequence()) && !(ExpressionTool.dependsOnFocus(expr.getAction()))) {
//            expr.replaceVariable(this, new ContextItemExpression());
//            Expression action = expr.getAction();
//            return new ForEach(expr.getSequence(), action);
//        } else {
//            return expr;
//        }
    }


    /**
     * In streaming mode, optimize a QuantifiedExpression for streaming. This is possible if the "select" part
     * of the quantified expression does a downward selection of the streamed input, while the "satisfies" part
     * does not access the streamed input at all (specifically, if it has no dependencies on the context node).
     * The expression is converted to streamable form by rewriting "some $t in S satisfies f($t)" as
     * "exists(for-each S return boolean(f(.)[.])", and likewise mutatis mutandis for "every $t....". This
     * depends on the generated for-each expression being streamable, which will not always be the case, for example
     * if the satisfies expression makes two downward selections from the range variable, or a non-downward selection.
     *
     * @param expr the expression to be optimized
     * @return the optimized expression, or the original expression if no optimization was possible.
     */

    @Override
    public Expression optimizeQuantifiedExpressionForStreaming(QuantifiedExpression expr) throws XPathException {
        Expression rewrite = Streamability.rewriteQuantifiedExpressionAsFilterExpression(expr);
        if (rewrite != null) {
            return rewrite;
        } else {
            return null;
        }
//        if (ExpressionInverter.consumesStream(expr.getSequence()) && !(ExpressionTool.dependsOnFocus(expr.getAction()))) {
//            if (expr.getOperator() == Token.SOME) {
//                expr.replaceVariable(this, new ContextItemExpression());
//                Expression action = expr.getAction();
//                Expression[] arguments = (new Expression[]{action});
//                BooleanFn ebv = (BooleanFn) SystemFunctionCall.makeSystemFunction("boolean", arguments);
//                assert ebv != null;
//                FilterExpression filter = new FilterExpression(expr.getSequence(), ebv);
//                Expression[] existsArgs = new Expression[]{filter};
//                return SystemFunctionCall.makeSystemFunction("exists", existsArgs);
//            } else {
//                expr.replaceVariable(this, new ContextItemExpression());
//                Expression action = expr.getAction();
//                Expression[] arguments = (new Expression[]{action});
//                NotFn ebv = (NotFn) SystemFunctionCall.makeSystemFunction("not", arguments);
//                assert ebv != null;
//                FilterExpression filter = new FilterExpression(expr.getSequence(), ebv);
//                Expression[] existsArgs = new Expression[]{filter};
//                return SystemFunctionCall.makeSystemFunction("empty", existsArgs);
//            }
//        } else {
//            return expr;
//        }
        // Note: although this rewrite is always safe, if the original expression is not streamable then the
        // resulting expression might not be streamable either, for example if the satisfies clause makes more
        // that one downward selection. This can result in poor diagnostics when the lack of streamability is
        // later reported.
    }


    /**
     * For a path expression of the form doc('a')/ * /(X | Y), promote the Union to the top level.
     * Note that this means undoing previous optimizations, which moved the unions to the lowest possible level.
     * This operation is done only for an expression within a streaming copy.
     *
     * @param path the path expression to be examined
     * @return either the optimized expression, or the original expression if no optimization is possible
     */

    private static Expression promoteUnion(SlashExpression path) {
        Expression start = path.getFirstStep();
        Expression rest = path.getRemainingSteps();
        if (rest instanceof SlashExpression) {
            rest = promoteUnion((SlashExpression) rest);
        }
        List<Expression> operands = new ArrayList<>(3);
        gatherUnionOperands(rest, operands);
        if (operands.size() < 2) {
            return path;
        } else {
            Expression p = ExpressionTool.makePathExpression(start, operands.get(0));
            for (int i = 1; i < operands.size(); i++) {
                p = new VennExpression(p, Token.UNION,
                                       ExpressionTool.makePathExpression(start.copy(new RebindingMap()), operands.get(i)));
            }
            return p;
        }
    }

    private static void gatherUnionOperands(Expression exp, List<Expression> list) {
        if (exp instanceof VennExpression && ((VennExpression) exp).getOperator() == Token.UNION) {
            final VennExpression ve = (VennExpression) exp;
            gatherUnionOperands(ve.getLhsExpression(), list);
            gatherUnionOperands(ve.getRhsExpression(), list);
        } else {
            list.add(exp);
        }
    }


    /**
     * Examine a path expression to see whether it can be replaced by a call on the key() function;
     * if so, generate an appropriate key definition and return the call on key(). If not, return null.
     *
     * @param pathExp The path expression to be converted.
     */

    @Override
    public Expression convertPathExpressionToKey(SlashExpression pathExp, ExpressionVisitor visitor) {
        if (!isOptionSet(OptimizerOptions.CREATE_KEYS)) {
            return null;
        }
        final TypeHierarchy th = getConfiguration().getTypeHierarchy();
        // If the path expression is not absolute, see if we can make it absolute by adding "/" at the front
        Expression copy = pathExp.copy(new RebindingMap());
        if (!(copy instanceof SlashExpression)) {
            return null;
        }
        SlashExpression p = ((SlashExpression)copy).tryToMakeAbsolute();
        if (p != null) {
            Expression match = p.getRemainingSteps();    // the first step selects the document node
            FilterExpression f = null;
            if (match instanceof FilterExpression) {
                f = (FilterExpression) match;
                if (f.getFilter() instanceof BooleanExpression) {
                    f = expandComplexFilter(f.getSelectExpression(), f.getFilter());
                }
            } else if (match instanceof SlashExpression) {
                f = convertToFilterExpression((SlashExpression) match, th);
            }
            if (f != null) {
                return convertFilterExpressionToKey(f, visitor, p.getFirstStep());
            }
        }
        return null;
    }

    /**
     * Try converting a filter expression to a call on the key function. Return the supplied
     * expression unchanged if not possible
     *
     * @param f                 the filter expression to be converted
     * @param visitor           the expression visitor, which must be currently visiting the filter expression f
     * @param indexFirstOperand true if the first operand of the filter comparison is to be indexed;
     * @param contextIsDoc      true if the context item is known to be a document node
     */
    @Override
    public Expression tryIndexedFilter(FilterExpression f, ExpressionVisitor visitor,
                                       boolean indexFirstOperand, boolean contextIsDoc) {
        if ("EE".equals(visitor.getTargetEdition())) {
            if (!Cardinality.allowsMany(f.getLhsExpression().getCardinality())) {
                return f;
            }
            // first try a document-level index (implemented using xsl:key logic)
            Expression k = tryToConvertFilterExpressionToKey(f, visitor, contextIsDoc);
            if (k != null) {
                return k;
            }
            f.restoreParentPointers();
            // failing that, try a variable-level index (an IndexedFilterExpression)
            if (f.getSelectExpression() instanceof VariableReference && isOptionSet(OptimizerOptions.INDEX_VARIABLES)) {
                VariableReference varRef = (VariableReference) f.getSelectExpression();
                Binding binding = varRef.getBinding();
                // don't index unless the filter expression is used more than once in the life of the variable
                if (binding instanceof LocalBinding && ExpressionTool.isLoopingReference(varRef, binding)) {
                    ((LocalBinding) binding).setIndexedVariable();
                    Expression ife = new IndexedFilterExpression(varRef, (ComparisonExpression) f.getFilter(),
                                                                 indexFirstOperand);
                    trace("Created indexed filter expression: ", ife);
                    return ife;
                } else if (binding instanceof GlobalVariable && !(binding instanceof GlobalParam)) {
                    ((GlobalVariable) binding).setIndexedVariable();
                    return new IndexedFilterExpression((VariableReference) f.getSelectExpression(),
                                                       (ComparisonExpression) f.getFilter(),
                                                       indexFirstOperand);
                }
            }
        } else {
            return f;
        }
        return f;
    }

    /**
     * Consider reordering the predicates in a filter expression based on cost estimates and other criteria
     * @param f the filter expression
     * @param visitor expression visitor
     * @param cisi information about the context item type
     * @return either the original expression, or an optimized replacement
     */

    @Override
    public FilterExpression reorderPredicates(FilterExpression f, ExpressionVisitor visitor, ContextItemStaticInfo cisi) throws XPathException {
        Expression base = f.getBase();
        if (!(base instanceof FilterExpression)) {
            return f;
        }
        ArrayList<Expression> filters = new ArrayList<>();
        addTerms(f.getFilter(), filters);
        while (base instanceof FilterExpression && !((FilterExpression)base).isFilterIsPositional()) {
            Expression predicate = ((FilterExpression) base).getFilter();
            addTerms(predicate, filters);
            base = ((FilterExpression)base).getBase();
        }

        filters.sort((f0, f1) -> {
            if (f0 == f1) {
                return 0;
            }
            // A filter that has no dependencies takes precedence over one that does (because it gives
            // a better chance of loop lifting or promotion to global
            boolean d0 = (f0.getDependencies() & ~StaticProperty.DEPENDS_ON_FOCUS) == 0;
            boolean d1 = (f1.getDependencies() & ~StaticProperty.DEPENDS_ON_FOCUS) == 0;
            if (d0 != d1) {
                return d0 ? -1 : +1;
            }
            // An indexable filter takes precedence over a non-indexable filter
            boolean i0 = isIndexableFilter(f0) != 0;
            boolean i1 = isIndexableFilter(f1) != 0;
            if (i0 != i1) {
                return i0 ? -1 : +1;
            }
            // Otherwise, compare estimated cost
            return f0.getCost() < f1.getCost() ? -1 : +1;
        });

        for (Expression e : filters) {
            base = new FilterExpression(base, e);
        }
        return (FilterExpression) base;
    }

    private static void addTerms(Expression predicate, List<Expression> filters) {
        if (predicate instanceof AndExpression) {
            addTerms(((AndExpression) predicate).getRhsExpression(), filters);
            addTerms(((AndExpression) predicate).getLhsExpression(), filters);
        } else {
            filters.add(0, predicate);
        }
    }
    
    private Expression tryToConvertFilterExpressionToKey(FilterExpression f, ExpressionVisitor visitor, boolean contextIsDoc) {
        Expression base = f.getSelectExpression();
        while (true) {
            if (base instanceof SlashExpression) {
                break;

            } else if (base instanceof AxisExpression) {
                if (contextIsDoc) {
                    base = new SlashExpression(new RootExpression(), base);
                    ExpressionTool.copyLocationInfo(f, base);
                }
                break;

            } else if (base instanceof VariableReference) {
                Binding b = ((VariableReference) base).getBinding();
                if (b instanceof LetExpression) {
                    base = ((LetExpression) b).getSequence().copy(new RebindingMap());
                } else {
                    return null;
                }

            } else if (base instanceof DocumentSorter) {
                base = ((DocumentSorter) base).getBaseExpression();

            } else if (base instanceof FilterExpression) {
                // this implies there are earlier filters that were not indexable (otherwise the FilterExpression
                // would by now be an IndexedFilterExpression). Provided the earlier filter is not positional,
                // we can swap the order of predicates
                FilterExpression baseF = (FilterExpression) base;
                if (!baseF.isFilterIsPositional() && isIndexableFilter(baseF.getFilter()) == 0) {
                    Expression baseFB = baseF.getSelectExpression();
                    Expression baseFF = baseF.getFilter();
                    RebindingMap rm = new RebindingMap();
                    FilterExpression fe2 = new FilterExpression(baseFB.copy(rm), f.getFilter().copy(rm));
                    FilterExpression fe3 = new FilterExpression(fe2, baseFF);
                    return tryToConvertFilterExpressionToKey(fe3, visitor, contextIsDoc);
                } else {
                    return null;
                }

            } else {
                return null;
            }
        }
        final TypeHierarchy th = getConfiguration().getTypeHierarchy();
        if (base instanceof SlashExpression) {
            SlashExpression path = (SlashExpression) base;
            SlashExpression p = path.tryToMakeAbsolute();
            if (p == null) {
                return null;
            }
            Expression doc = p.getFirstStep();
            if (!th.isSubType(doc.getItemType(), NodeKindTest.DOCUMENT)) {
                return null;
            }
            return convertFilterExpressionToKey(f, visitor, doc);
        } else {
            return null;
        }

    }

    /**
     * Convert a filter expression to a call on the key function. Return null if not possible
     *
     * @param f       the filter expression to be optimized
     * @param visitor the expression visitor
     * @param doc     expression to get the document being searched
     * @return either the optimized expression, or null if no optimization is possible
     */

    private Expression convertFilterExpressionToKey(FilterExpression f, ExpressionVisitor visitor, Expression doc) {
        if (!isOptionSet(OptimizerOptions.CREATE_KEYS)) {
            return null;
        }
        if (f.getFilter() instanceof BooleanExpression) {
            f = expandComplexFilter(f.getSelectExpression(), f.getFilter());
        }
        final TypeHierarchy th = getConfiguration().getTypeHierarchy();
        final RetainedStaticContext rsc = f.getRetainedStaticContext();
        final int indexableFlag = isIndexableFilter(f.getFilter());
        if (indexableFlag == 0) {
            if (f.isPositional(th)) {
                return null;
            }
            return tryConvertingInnerPredicate(f, visitor, doc);
        }
        boolean indexFirstOperand = indexableFlag > 0;

        Expression path = f.getSelectExpression().copy(new RebindingMap());

        if ((path.getDependencies() &
                ~(StaticProperty.DEPENDS_ON_CONTEXT_ITEM |
                        StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT |
                        StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT |
                          StaticProperty.DEPENDS_ON_STATIC_CONTEXT)) != 0) {
            // don't allow paths that depend on other variables, etc
            return tryConvertingInnerPredicate(f, visitor, doc);
        }

        // If the doc expression (which is the first step in the path) depends on the context item,
        // then we need to remove it from the path, because when it becomes part of the match pattern
        // of the constructed key, it might be evaluated with a different context item. See bugs #1725 and #3769.

        if (path instanceof SlashExpression && !(doc instanceof RootExpression) &&
                (doc.getDependencies() & StaticProperty.DEPENDS_ON_FOCUS) != 0) {
            path = new SlashExpression(new RootExpression(), path);
        }

        ComparisonExpression fc = (ComparisonExpression) f.getFilter().copy(new RebindingMap());
        Expression use = indexFirstOperand ? fc.getLhsExpression() : fc.getRhsExpression();
        Expression value = indexFirstOperand ? fc.getRhsExpression() : fc.getLhsExpression();

        // don't go any further if the use expression has dependencies other than the context item
        if ((use.getDependencies()
                & ~(StaticProperty.DEPENDS_ON_CONTEXT_ITEM | StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT | StaticProperty.DEPENDS_ON_STATIC_CONTEXT)) != 0) {
            return null;
        }
        // TODO: a key definition is no longer constrained to follow the XSLT rules for match patterns;
        // the match part of the key can now be any PathFinder. So this code doesn't need to be so restrictive.

        int props = path.getSpecialProperties();
        if (((props & StaticProperty.NO_NODES_NEWLY_CREATED) == 0)
                || ((props & StaticProperty.SINGLE_DOCUMENT_NODESET) == 0)) {
            // to be indexable, the expression must not create new nodes, and it must return nodes 
            // that are all in the same document
            return null;
        }

        if ((props & StaticProperty.ORDERED_NODESET) == 0) {
            DocumentSorter sorter = new DocumentSorter(path);
            ExpressionTool.copyLocationInfo(path, sorter);
            path = sorter;
        }

        NodeSetPattern pathFinder = new NodeSetPattern(path);
        ItemType useType = use.getItemType();

        if (!useType.isPlainType()) {
            try {
                use = Atomizer.makeAtomizer(use, null).simplify();
            } catch (XPathException e) {
                return null;
            }
            ExpressionTool.copyLocationInfo(f, use);
            useType = use.getItemType();
        }

        // If the operator is eq, keys have the correct type-matching semantics, except that they
        // give "no match" rather than an error when the types are non-comparable.

        // If the operator is "=",
        // untypedAtomic values need to be converted to the type of the other operand. If both sides allow
        // any atomic type, this gives us a problem.
        // If we know the type of the value, we force untyped values
        //   in the use expression to the same type (or double, if it's numeric).
        // If not, we need to index each value both as itself, and as
        //   the result of converting to untypedAtomic, and when we search, we similarly need to search both.

        boolean convertUntypedToOther = fc.convertsUntypedToOther();
        //if (f.getFilter() instanceof GeneralComparison) {
        if (convertUntypedToOther) {
            //convertUntypedToOther = true;
            ItemType valueType = value.getItemType().getAtomizedItemType();
            if (useType.equals(BuiltInAtomicType.ANY_ATOMIC) && valueType.equals(BuiltInAtomicType.ANY_ATOMIC)) {
                //return null;
                // TODO: we can do better, with a multi-type index, as in indexedClosure
            } else if ((useType.equals(BuiltInAtomicType.ANY_ATOMIC) || useType.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) &&
                    !(valueType.equals(BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(valueType, BuiltInAtomicType.STRING) || th.isSubType(valueType, BuiltInAtomicType.ANY_ATOMIC))) {
                AtomicType targetType = (AtomicType) valueType;
                if (NumericType.isNumericType(targetType)) {
                    targetType = BuiltInAtomicType.DOUBLE;
                }
                use = new AtomicSequenceConverter(use, targetType);
                ((AtomicSequenceConverter) use).allocateConverterStatically(config, false);
            } else if ((valueType.equals(BuiltInAtomicType.ANY_ATOMIC) || valueType.equals(BuiltInAtomicType.UNTYPED_ATOMIC)) &&
                    !(useType.equals(BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(useType, BuiltInAtomicType.STRING) || th.isSubType(valueType, BuiltInAtomicType.ANY_ATOMIC))) {
                AtomicType targetType = (AtomicType) valueType;
                if (NumericType.isNumericType(targetType)) {
                    targetType = BuiltInAtomicType.DOUBLE;
                }
                value = new AtomicSequenceConverter(value, targetType);
                ((AtomicSequenceConverter) value).allocateConverterStatically(config, false);
            }
        }

        StaticContext env = visitor.getStaticContext();
        String defaultCollationName = env.getDefaultCollationName();

        KeyManager keyManager = env.getKeyManager();
        StructuredQName keyName;
        KeyDefinitionSet keySet;
        synchronized(keyManager) {
            keySet = keyManager.findKeyDefinition(pathFinder, use, defaultCollationName);

            if (keySet == null) {
                Configuration config = getConfiguration();
                SlotManager slotManager = config.makeSlotManager();
                int slots = ExpressionTool.allocateSlots(pathFinder.getSelectionExpression(), 0, slotManager);
                ExpressionTool.allocateSlots(use, slots, slotManager);
                StringCollator collator;
                try {
                    collator = config.getCollation(defaultCollationName);
                } catch (XPathException e) {
                    return null; // should not happen
                }

                keyName = new StructuredQName("saxon", NamespaceUri.SAXON,
                                              "kk" + (100 + keyManager.getNumberOfKeyDefinitions()));
                SymbolicName symbolicName = new SymbolicName(StandardNames.XSL_KEY, keyName);
                KeyDefinition key = new KeyDefinition(symbolicName, pathFinder, use, defaultCollationName, collator);
                key.setPackageData(f.getPackageData());
                key.setIndexedItemType((BuiltInAtomicType) useType.getPrimitiveItemType());
                key.setConvertUntypedToOther(convertUntypedToOther);
                key.setStackFrameMap(slotManager);

                if (f.getFilter() instanceof ValueComparison) {
                    key.setStrictComparison(true);
                }

                try {
                    keyManager.addKeyDefinition(keyName, key, true, getConfiguration());
                } catch (XPathException err) {
                    throw new AssertionError(err);  // shouldn't happen
                }

                keySet = keyManager.getKeyDefinitionSet(keyName);

            } else {
                keyName = keySet.getKeyName();
            }
        }

        Expression call;
        if (doc.getCardinality() != StaticProperty.EXACTLY_ONE) {
//            RangeVariable decl = new RangeVariable();
//            decl.setRequiredType(SequenceType.makeSequenceType(doc.getItemType(th), doc.getCardinality()));
//            decl.setVariableQName(new StructuredQName("dd", NamespaceConstant.SAXON, "dd" + decl.hashCode()));
            ForExpression forExp = new ForExpression();
            forExp.setRequiredType(SequenceType.makeSequenceType(doc.getItemType(), doc.getCardinality()));
            forExp.setVariableQName(new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, "dd" + forExp.hashCode()));
            Expression docs = doc;
            if (!docs.hasSpecialProperty(StaticProperty.ORDERED_NODESET)) {
                // sort into document order
                docs = new DocumentSorter(docs);
            }
            forExp.setSequence(docs);
            //forExp.setVariableDeclaration(decl);
            LocalVariableReference docVar = new LocalVariableReference(forExp);
            call = KeyFn.internalKeyCall(keyManager, keySet, keyName.getEQName(), value, docVar, rsc);
            forExp.setAction(call);
            call = forExp;
        } else {
            call = KeyFn.internalKeyCall(keyManager, keySet, keyName.getEQName(), value, doc, rsc);
        }
        ExpressionTool.copyLocationInfo(f, call);
        trace("Replaced filter expression with call to key function: ", call);
        return call;
    }

    /**
     * If for some reason we can't convert EXP[P2] into a call on key(), we look to see if EXP
     * is itself a filter expression. The Expression (E[P1])[P2] then gets converted to key()[P2].
     * (This is applied recursively, of course)
     *
     * @param f       the original filter expression that couldn't be converted
     * @param visitor the expression visitor
     * @param doc     the expression selecting the document at the root of the path expression
     * @return the converted expression, or null if it can't be converted
     */

    private Expression tryConvertingInnerPredicate(FilterExpression f, ExpressionVisitor visitor, Expression doc) {
        Expression base = f.getSelectExpression();
        if (base instanceof FilterExpression) {
            // if we can't index on the final predicate, try indexing on an earlier predicate
            Expression pred = f.getFilter();
            Expression k = convertFilterExpressionToKey((FilterExpression) base, visitor, doc);
            if (k != null) {
                FilterExpression kf = new FilterExpression(k, pred);
                ExpressionTool.copyLocationInfo(f, kf);
                return kf;
            } else {
                return null;
            }
        } else {
            return null;
        }
    }

    /**
     * Convert a path expression such as a/b/c[predicate] into a filter expression
     * of the form (a/b/c)[predicate]. This is possible whenever the predicate is non-positional.
     * The conversion is useful in the case where the path expression appears inside a loop,
     * where the predicate depends on the loop variable but a/b/c does not.
     *
     * @param pathExp the path expression to be converted
     * @return the resulting filterexpression if conversion is possible, or null if not
     */

    @Override
    public FilterExpression convertToFilterExpression(SlashExpression pathExp, TypeHierarchy th) {
        Expression head = pathExp.getFirstStep();
        Expression tail = pathExp.getRemainingSteps();
        List<Expression> path = new ArrayList<>(3);
        path.add(head);
        while (tail instanceof SlashExpression) {
            head = ((SlashExpression) tail).getFirstStep();
            path.add(head);
            tail = ((SlashExpression) tail).getRemainingSteps();
        }
        if (tail instanceof FilterExpression) {
            if (!((FilterExpression) tail).isPositional(th)) {
                Expression predicate = ((FilterExpression) tail).getFilter();
                Expression last = ((FilterExpression) tail).getSelectExpression();
                boolean repeat = last instanceof FilterExpression;
                for (int i = path.size() - 1; i >= 0; i--) {
                    last = ExpressionTool.makePathExpression(path.get(i), last);
                    ExpressionTool.copyLocationInfo(pathExp, last);
                }
                if ((last instanceof SlashExpression) && repeat) {
                    // allow for multiple predicates on the last step of the path expression
                    FilterExpression e = convertToFilterExpression((SlashExpression) last, th);
                    if (e != null) {
                        last = e;
                    }
                }
                // if the predicate contains several non-positional conditions and'ed together,
                // break them up into separate predicates

                if (predicate instanceof BooleanExpression) {
                    return expandComplexFilter(last, predicate);
                } else {
                    FilterExpression fe = new FilterExpression(last, predicate);
                    ExpressionTool.copyLocationInfo(pathExp, fe);
                    trace("Moved predicate to outer level of path expression: ", fe);
                    return fe;
                }
            }
        }
        return null;
    }

    /**
     * Change a filter expression in the form EXP[A and B] into the form EXP[A][B].
     * This method must be invoked only if the caller has established that the predicate is non-positional.
     *
     * @param base      the expression being filtered
     * @param predicate the predicate doing the filtering
     * @return a possibly rewritten filter expression
     */

    private FilterExpression expandComplexFilter(Expression base, Expression predicate) {
        List<Expression> list = new ArrayList<>(4);
        BooleanExpression.listAndComponents(predicate, list);
        for (Expression exp : list) {
            FilterExpression fe = new FilterExpression(base, exp);
            ExpressionTool.copyLocationInfo(base, fe);
            base = fe;
        }
        trace("Split composite predicate into multiple predicates: ", base);
        return (FilterExpression) base;
    }


    /**
     * Test whether a filter predicate is indexable.
     *
     * @param filter the predicate expression
     * @return 0 if not indexable; +1 if the predicate is in the form expression=value; -1 if it is in
     * the form value=expression
     */

    @Override
    public int isIndexableFilter(Expression filter) {
        if (filter instanceof ComparisonExpression &&
                ((ComparisonExpression) filter).getSingletonOperator() == Token.FEQ) {
            return isIndexableComparison((ComparisonExpression) filter);
        } else {
            return 0;
        }
    }

    /**
     * Determine whether a filter predicate is capable of being supported by an index
     *
     * @param filter the predicate
     * @return +1 if the predicate is indexable on its left-hand operand, -1 if indexable on its right-hand
     * operand, 0 if not indexable
     */

    private static int isIndexableComparison(ComparisonExpression filter) {
        if (filter instanceof GeneralComparison10) {
            // See bug 2307. A general comparison in a 1.0 stylesheet that follows 2.0
            // semantics will already have been converted to a GeneralComparison20.
            return 0;
        }
        Expression op0 = filter.getLhsExpression();
        Expression op1 = filter.getRhsExpression();
        boolean d0f = ExpressionTool.dependsOnFocus(op0);
        boolean d0v = (op0.getDependencies() & StaticProperty.DEPENDS_ON_LOCAL_VARIABLES) != 0;
        boolean d1f = ExpressionTool.dependsOnFocus(op1);
        boolean d1v = (op1.getDependencies() & StaticProperty.DEPENDS_ON_LOCAL_VARIABLES) != 0;

        if (d0f && !d1f && !d0v) {
            return +1;
        }
        if (d1f && !d0f && !d1v) {
            return -1;
        }
        return 0;
        // TODO: relax this. We're ruling out expressions that depend both on the context and on
        // local variables, because the variable could have different values on different occasions,
        // thus invalidating the index. But some variables would be OK, specifically those that were declared
        // at an outer level to the one being indexed. See seq028 under Saxon-EE.
//        if (d0 && !d1) {
//            return +1;
//        } else if (d1 && !d0) {
//            return -1;
//        } else {
//            return 0;
//        }

    }

    /**
     * Create an indexed value
     */

    @Override
    public GroundedValue makeIndexedValue(SequenceIterator iter) throws XPathException {
        return SearchableValue.makeSearchableValue(iter);
    }

    @Override
    public void optimizeNodeSetPattern(NodeSetPattern pattern) {
        Expression selection = pattern.getSelectionExpression();
        if (selection instanceof GlobalVariableReference) {
            Binding var = ((GlobalVariableReference)selection).getBinding();
            if (var instanceof GlobalVariable) {
                ((GlobalVariable)var).setIndexedVariable();
            }
        }
    }

    /**
     * Prepare an expression for streaming
     *
     * @param exp the expression to be prepared
     */

    @Override
    public void prepareForStreaming(Expression exp) throws XPathException {
        ExpressionTool.processExpressionTree(exp, new Object(), (expr, obj) -> {
            expr.prepareForStreaming();
            return false;
        });
    }

    /**
     * Evaluate the streaming (first) argument of a streamable stylesheet function
     *
     * @param expr    the expression supplied for the value of the streaming argument
     * @param context the XPath evaluation context of the caller
     * @return the (nominal) result of the evaluation
     */

    @Override
    public Sequence evaluateStreamingArgument(Expression expr, XPathContext context) throws XPathException {
        PostureAndSweep ps = Streamability.getPostureAndSweepIfKnown(expr);
        if (ps == null) {
            return super.evaluateStreamingArgument(expr, context);
        } else if (ps.getPosture() == Posture.GROUNDED /*&& ps.getSweep() == Sweep.MOTIONLESS */) {
            return ExpressionTool.eagerEvaluate(expr, context);
        } else {
            // Return a notional value. The actual value will be supplied one item at a time, as it arrives
            return EmptySequence.getInstance();
        }
    }

    /**
     * Make a conditional document sorter. This optimization is attempted
     * when a DocumentSorter is wrapped around a path expression
     *
     * @param sorter an expression that sorts and deduplicates its operand
     * @param path   the slash expression that is the operand of the sorter
     * @return either the original sorter, if no optimization is possible; or a ConditionalSorter
     * which first tests a condition, and performs the sort if the condition is true, or returns
     * the unsorted operand if not. In practice the condition is always a test whether the first
     * item in the path expression contains more than one item
     */

    @Override
    public Expression makeConditionalDocumentSorter(DocumentSorter sorter, SlashExpression path) {
        // if the path expression has the form $a/b/c/d, where b/c/d is naturally sorted, then
        // we add a run-time condition to the sorter: the sort is only necessary if $a is not
        // a singleton. Unfortunately we can't just do sort($a)/b/c/d because that fails when $a
        // isn't a peer nodeset.
        Expression head = path.getFirstStep();
        Expression h2 = head;
        if (head instanceof ItemChecker && ((ItemChecker) head).getRequiredType() instanceof NodeTest) {
            // Allow the variable reference to be wrapped in a "treat as node()", added during type checking
            h2 = ((ItemChecker) head).getBaseExpression();
        }

        Expression steps = path.getRemainingSteps();
        int stepProps = steps.getSpecialProperties();
        if (!Cardinality.allowsMany(h2.getCardinality()) && (stepProps & StaticProperty.ORDERED_NODESET) != 0) {
            return path;
        }
        if (h2 instanceof VariableReference && (stepProps & StaticProperty.ORDERED_NODESET) != 0) {
            Expression ref = head.copy(new RebindingMap());
            TailExpression tailExp = new TailExpression(ref, 2);
            Expression exists = SystemFunction.makeCall("exists", sorter.getRetainedStaticContext(), tailExp);
            return new ConditionalSorter(exists, sorter);
        } else {
            return sorter;
        }
    }

    /**
     * Replace a function call by the body of the function, assuming all conditions for inlining
     * the function are satisfied
     *
     * @param functionCall    the functionCall expression
     * @param visitor         the expression visitor
     * @param contextItemType the static type of the context item
     * @return either the original expression unchanged, or an expression that consists of the inlined
     * function body, with all function parameters bound as required.
     */

    @Override
    public Expression tryInlineFunctionCall(
            UserFunctionCall functionCall, ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) {
        if (!isOptionSet(OptimizerOptions.INLINE_FUNCTIONS)) {
            return functionCall;
        }
        if (functionCall.isBeingInlined()) {
            return functionCall;
        }
        //System.err.println("TryInline UFC " + System.identityHashCode(functionCall));
        UserFunction function = functionCall.getFunction();
        Optional<Boolean> b = function.isInlineable();
        boolean isInlineable;
        // TODO: inline streamable functions where possible. The main difficulty is that the
        // expression that results will do "let $x := (streamable node)" which isn't streamable.
        // Experimented with this June 2017. Generally inlining works if we allow optimization
        // of the Let expression to inline the variable (which is currently prohibited when
        // optimizeForStreaming() is set). However it causes one or two unexplained test failures.
        if (function.getDeclaredStreamability() != FunctionStreamability.UNCLASSIFIED) {
            isInlineable = false;
        } else if (!b.isPresent()) {
            if (function.getBody() == null) {
                // bug 2226
                return functionCall;
            }
            List<UserFunction> calledFunctions = new ArrayList<>(10);
            ExpressionTool.gatherCalledFunctions(function.getBody(), calledFunctions);
            // TODO: the check on not calling other functions is probably unnecessary now
            // that we limit the number of times a function can be inlined; also the function
            // would gradually grow until it hits the size limit
            isInlineable =
                    calledFunctions.isEmpty() &&
                            !ExpressionTool.changesXsltContext(function.getBody()) &&
                            ExpressionTool.expressionSize(function.getBody()) <= config.getConfigurationProperty(Feature.THRESHOLD_FOR_FUNCTION_INLINING) &&
                            !function.isMemoFunction() &&
                            !ExpressionTool.dependsOnFocus(function.getBody()) &&
                            functionCall.getRetainedStaticContext().getPackageData() == function.getPackageData();
            function.setInlineable(isInlineable);
        } else {
            isInlineable = b.get();
        }

        if (isInlineable) {
            functionCall.setBeingInlined(true);
            function.markAsInlined();
            boolean wasInlining = visitor.isInliningFunctions();
            visitor.setInliningFunctions(true);
            Expression newBody;

            //function.getBody().verifyParentPointers("OptimizerEE, before function inline of " + function.getFunctionName());
            newBody = function.getBody().copy(new RebindingMap());
            newBody.setRetainedStaticContext(function.getRetainedStaticContext());

            UserFunctionParameter[] params = function.getParameterDefinitions();
            for (int p = params.length - 1; p >= 0; p--) {
                UserFunctionParameter param = params[p];
                LetExpression let = new LetExpression();
                let.setRequiredType(param.getRequiredType());
                let.setVariableQName(param.getVariableQName());
                let.setSequence(functionCall.getArg(p).copy(new RebindingMap()));
                let.setAction(newBody);
                let.setRetainedStaticContext(function.getRetainedStaticContext());
                ExpressionTool.rebindVariableReferences(newBody, param, let);
                newBody = let;
            }
            try {
                StructuredQName fName = function.getFunctionName();
                trace("Moved function " + (fName == null ? "(anonymous)" : fName.getDisplayName()) + " inline: ", newBody);
                newBody = newBody.simplify();
                boolean saveSuppressed = visitor.isSuppressWarnings();
                visitor.setSuppressWarnings(true);
                newBody = newBody.typeCheck(visitor, contextItemType);
                newBody = ExpressionTool.optimizeComponentBody(newBody, null, visitor, contextItemType, true);
                //newBody = newBody.optimize(visitor, contextItemType);
                if (function.getDeterminism() != UserFunction.Determinism.PROACTIVE) {
                    newBody.setStaticProperty(StaticProperty.NO_NODES_NEWLY_CREATED);
                }
                //newBody.verifyParentPointers("OptimizerEE, after function inline");
                visitor.setSuppressWarnings(saveSuppressed);
                functionCall.setBeingInlined(false);
                return newBody;
            } catch (XPathException e) {
                return functionCall;
            } finally {
                visitor.setInliningFunctions(wasInlining);
            }
        } else {
            return functionCall;
        }
    }

    /**
     * Try to convert a Choose expression (or an XQuery 3.0 switch) into an optimized switch
     *
     * @param choose the Choose expression
     * @return the result of optimizing this (the original expression if no optimization was possible)
     */

    @Override
    public Expression trySwitch(Choose choose, ExpressionVisitor visitor) {
        if (!"EE".equals(visitor.getTargetEdition()) || !isOptionSet(OptimizerOptions.SWITCH)) {
            return choose;
        }
        // Not interested unless there are at least 4 conditions
        if (choose.size() < 4) {
            return choose;
        }
        SwitchExpression.SwitchCaseInfo switchCaseInfo = new SwitchExpression.SwitchCaseInfo();
        // The conditions must all be ValueComparisons or SingletonComparisons
        switchCaseInfo.commonlhs = null;
        switchCaseInfo.commontype = null;
        switchCaseInfo.index = new HashMap<>(choose.size());

        String defaultCollation = choose.getRetainedStaticContext().getDefaultCollationName();
        try {
            switchCaseInfo.collation = getConfiguration().getCollation(defaultCollation);
        } catch (XPathException e) {
            return choose; // should not happen
        }
        switchCaseInfo.defaultAction = Literal.makeEmptySequence();

        int size = choose.size();
        for (int c = 0; c < size; c++) {
            Expression condition = choose.getCondition(c);
            boolean isOtherwise = c == size - 1 && Literal.hasEffectiveBooleanValue(condition, true);
            if (isOtherwise) {
                switchCaseInfo.defaultAction = choose.getAction(c);
            } else {
                if (!defaultCollation.equals(condition.getRetainedStaticContext().getDefaultCollationName())) {
                    return choose;
                }
                if (!processSwitchCondition(condition, choose.getAction(c), switchCaseInfo)) {
                    return choose;
                }
            }
        }
        if (!switchCaseInfo.makeIndex()) {
            return null;
        }
        return new SwitchExpression(switchCaseInfo);
    }


    private boolean processSwitchCondition(Expression condition, Expression action, SwitchExpression.SwitchCaseInfo switchCaseInfo) {
        //This test is required when expanding an OrExpression
        if (condition instanceof OrExpression) {
            // See bug 3261. In 9.7 and 9.8, an action associated with several alternative key values was duplicated.
            // In 9.9.1.1 this changes to introduce an indirection in the data structure so that two key values
            // can point to the same action expression, reducing duplication in the expression tree and in compiled
            // bytecode. The duplication still exists if the switch expression is exported and re-imported.
            boolean b = processSwitchCondition(((OrExpression) condition).getLhsExpression(), action, switchCaseInfo);
            return b && processSwitchCondition(((OrExpression) condition).getRhsExpression(), action, switchCaseInfo);
        }
        if (!(condition instanceof ComparisonExpression)) {
            return false;
        }
        if (condition instanceof GeneralComparison) {
            return false;
        }
        if (((ComparisonExpression) condition).getSingletonOperator() != Token.FEQ) {
            return false;
        }
        Expression lhs = ((ComparisonExpression) condition).getLhsExpression();
        Expression rhs = ((ComparisonExpression) condition).getRhsExpression();
        if (!Literal.isAtomic(rhs)) {
            return false;
        }
        if (((ComparisonExpression) condition).getAtomicComparer() instanceof UntypedNumericComparer ||
                (condition instanceof ValueComparison && ((ValueComparison)condition).convertsUntypedToOther())) {
            // Bug 4738, 5000
            UntypedSequenceConverter usc = new UntypedSequenceConverter(lhs.copy(new RebindingMap()), BuiltInAtomicType.DOUBLE);
            Converter conv = usc.allocateConverter(getConfiguration(), true);
            usc.setConverter(conv);
            lhs = usc;
        }
        if (switchCaseInfo.commonlhs == null) {
            switchCaseInfo.commonlhs = lhs;
            switchCaseInfo.commontype = (BuiltInAtomicType) rhs.getItemType().getPrimitiveItemType();
            if (!switchCaseInfo.commontype.isOrdered(false)) {
                return false;
            }
        } else {
            //assert commonlhs != null;
            if (!switchCaseInfo.commonlhs.isEqual(lhs)) {
                return false;
            }
            if (!switchCaseInfo.commontype.equals(rhs.getItemType().getPrimitiveItemType())) {
                return false;
            }
        }
        AtomicValue value = (AtomicValue) ((Literal) rhs).getGroundedValue();
        if (value instanceof CalendarValue) {
            // don't handle this case because of the problem of implicit timezones, especially if we want to export an XSLT package
            return false;
        }
        switchCaseInfo.values.add(value);
        switchCaseInfo.actions.add(action);
        return true;
    }


    /**
     * Try to convert a collection of comparisons connected by "or" operators into a
     * single GeneralComparison
     *
     * @param commonParts     the number of common subexpressions found at this level
     * @param visitor         the expression visitor
     * @param contextItemType static information about the context item type
     * @param map             a map to be populated with comparison expressions, keyed on the common
     *                        operand
     * @param otherExps       a list to be populated with expressions in the "or" tree that are
     *                        not comparison expressions, so they are retained "as is"
     * @param expr            the Or expression to be optimized
     * @return a tally of the common expressions so far
     */
    public int gatherOrExpressions(
            int commonParts, ExpressionVisitor visitor, ContextItemStaticInfo contextItemType,
            HashMap<Expression, List<Expression>> map, List<Expression> otherExps, Expression expr) {
        if (expr instanceof OrExpression) {
            OrExpression orExpr = (OrExpression) expr;
            commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExps, orExpr.getLhsExpression());
            commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExps, orExpr.getRhsExpression());
            return commonParts;
        } else {
//            Expression e = expr.optimize(visitor, contextItemType);
//
//            if (e != null && e != expr) {
//                expr = e;
//            }
            if (expr instanceof ComparisonExpression) {
                ComparisonExpression compExpr = (ComparisonExpression) expr;
                int op;

                if (compExpr instanceof BinaryExpression) {
                    BinaryExpression exprBin = (BinaryExpression) expr;
                    op = exprBin.getOperator();


                } else if (compExpr instanceof CompareToConstant) {
                    CompareToConstant intConst = (CompareToConstant) expr;
                    op = intConst.getSingletonOperator();

                } else {
                    //This should never happen
                    //throw new XPathException("Unknown ComparisonExpression sub-class" + expr.getExpressionName());
                    otherExps.add(expr);
                    return commonParts;
                }

                Expression left = compExpr.getLhsExpression();
                Expression right = compExpr.getRhsExpression();

                if (!compExpr.convertsUntypedToOther()) {
                    // We're creating a GeneralComparison which converts an untyped atomic operand to the type
                    // of the other. This isn't safe unless we know that if one operand can contain untypedAtomic,
                    // the other can only contain string
                    if (left.getItemType().getUType().overlaps(UType.UNTYPED_ATOMIC) &&
                            !UType.STRING_LIKE.subsumes(right.getItemType().getUType()) ||
                            (right.getItemType().getUType().overlaps(UType.UNTYPED_ATOMIC) &&
                                    !UType.STRING_LIKE.subsumes(left.getItemType().getUType()))) {
                        otherExps.add(expr);
                        return commonParts;
                    }
                }

                if ((op == Token.FEQ) || op == Token.EQUALS) {
                    final TypeHierarchy th = getConfiguration().getTypeHierarchy();
                    commonParts = comparisonExprFixup(commonParts, map, otherExps, th, expr, left, right);
                    return commonParts;
                }
            }
            otherExps.add(expr);
            return commonParts;
        }
    }


    /**
     * Try to convert an Or expression into a comparison with Literal sequence
     *
     * @param visitor         the expression visitor
     * @param contextItemType static information about the context item type
     * @param orExpr          the Choose expression
     * @return the result of optimizing the Or expression (the original expression if no optimization was possible)
     */

    @Override
    public Expression tryGeneralComparison(
            ExpressionVisitor visitor, ContextItemStaticInfo contextItemType, OrExpression orExpr) throws XPathException {

        int commonParts = 0;

        HashMap<Expression, List<Expression>> map = new HashMap<>();
        // map of common left Comparison Expr with the right operands as a list
        List<Expression> otherExp = new ArrayList<>();
        //list of expression that are not Comparison Expressions

        commonParts = gatherOrExpressions(commonParts, visitor, contextItemType, map, otherExp, orExpr);

        if (commonParts <= 1) {
            // no common subexpressions found
            return orExpr;
        }
        Expression newExpr = null;
        Expression previous = null;

        // Try to find a NamespaceResolver, in case there are any comparisons of untypedAtomic values to QNames

        RetainedStaticContext nsResolver = orExpr.getRetainedStaticContext();
//    out:
//        for (List<Expression> expList : map.values()) {
//            for (Expression exp : expList) {
//                if (exp instanceof GeneralComparison) {
//                    nsResolver = exp.getRetainedStaticContext();
//                    break out;
//                }
//            }
//        }

        for (Map.Entry<Expression, List<Expression>> entry : map.entrySet()) {
            Expression comparison = new GeneralComparison20 (
                    entry.getKey(), Token.EQUALS, Block.makeBlock(entry.getValue()));
            comparison.setRetainedStaticContext(nsResolver);
            Expression e = comparison.typeCheck(visitor, contextItemType);
            if (e != null) {
                comparison = e;
            }

            if (previous != null) {
                newExpr = new OrExpression(previous, comparison);
                previous = newExpr;
            } else {
                previous = comparison;
                newExpr = previous;
            }
        }
        previous = newExpr;
        for (Expression expri : otherExp) {
            if (previous != null) {
                newExpr = new OrExpression(previous, expri);
                previous = newExpr;
            } else {
                previous = expri; //this should not happen
            }
        }

        if (newExpr != null) {
            newExpr = newExpr.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            if (newExpr instanceof GeneralComparison) {
                return optimizeGeneralComparison(visitor, (GeneralComparison) newExpr, false, contextItemType);
            } else {
                return newExpr;
            }
        }
        return orExpr;
    }

    private int comparisonExprFixup(
            int commonParts, HashMap<Expression, List<Expression>> map, List<Expression> otherExp,
            TypeHierarchy th, Expression second, Expression left, Expression right) {

        boolean checkSecondExpr = second instanceof ValueComparison;

        boolean checkOperands = checkSecondExpr && (left.getSpecialProperties() & StaticProperty.NOT_UNTYPED_ATOMIC) == 0 &&
                th.relationship(left.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT &&
                ((th.relationship(right.getItemType(), BuiltInAtomicType.STRING) != Affinity.DISJOINT) ||
                        (th.relationship(right.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT));

        checkOperands = checkSecondExpr && (checkOperands || ((right.getSpecialProperties() & StaticProperty.NOT_UNTYPED_ATOMIC) == 0 &&
                th.relationship(right.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT &&
                ((th.relationship(left.getItemType(), BuiltInAtomicType.STRING) != Affinity.DISJOINT) ||
                         (th.relationship(left.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT))));

        if (checkOperands) {
            otherExp.add(second);
        } else if (!map.containsKey(left)) {
            List<Expression> valueList = new ArrayList<>();
            valueList.add(right);
            map.put(left, valueList);
        } else {
            map.get(left).add(right);
            commonParts++;
        }
        return commonParts;
    }


    /**
     * Identify expressions within a function or template body that can be promoted to be
     * evaluated as global variables.
     *
     * @param expr    the expression; either this expression or any subexpression might be promoted
     * @param visitor the expression visitor
     * @return the expression after subexpressions have been promoted to global variables; or null
     * if no action was taken
     */

    @Override
    public Expression promoteExpressionsToGlobal(
            Expression expr, GlobalVariableManager globalVariableManager, ExpressionVisitor visitor) throws XPathException {
        if (isOptionSet(OptimizerOptions.EXTRACT_GLOBALS)) {
            GlobalExtractor extractor = new GlobalExtractor(expr, globalVariableManager, visitor);
            return extractor.extractGlobals();
        } else {
            return null;
        }

    }

    @Override
    public Expression eliminateCommonSubexpressions(Expression in) {
        if (isOptionSet(OptimizerOptions.COMMON_SUBEXPRESSIONS)) {
            return new CommonSubexpressionPromoter(this).promoteCommonSubexpressions(in);
        } else {
            return in;
        }
    }

    /**
     * Generate a multi-threaded version of an instruction.
     * Supported in Saxon-EE only; ignored with no action in Saxon-HE and Saxon-PE
     *
     * @param instruction the instruction to be multi-threaded
     * @return the multi-threaded version of the instruction
     */

    @Override
    public Expression generateMultithreadedInstruction(Expression instruction) {
        if (instruction instanceof ForEach && config.getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            ForEach base = (ForEach) instruction;
            MultithreadedForEach result = new MultithreadedForEach(
                    base.getSelectExpression(), base.getActionExpression(), false, base.getNumberOfThreadsExpression());
            result.setInstruction(base.isInstruction());
            return result;
        } else {
            return instruction;
        }
    }


//#if BYTECODE == true
    /**
     * Generate Java byte code for an expression
     *
     *
     * @param compilerService   the compiler service
     * @param expr              the expression to be compiled
     * @param objectName        the name of the object to be compiled, used as a hint for the Java class/method name
     * @param evaluationMethods The evaluation modes for which code is generated. Currently a subset of
     *                          {@link Expression#PROCESS_METHOD}, {@link Expression#ITERATE_METHOD}. If no code is generated for
     *                          a given evaluation method, the fallback implementation from the superclass is used.
     * @return the compiled expression if possible, or null if compilation is not possible
     */
    @Override
    public Expression compileToByteCode(ICompilerService compilerService, Expression expr, String objectName, int evaluationMethods) {
        if (expr instanceof CompiledExpression || expr instanceof Literal || expr instanceof VariableReference) {
            return expr;
        }
        try {
            return ((CompilerService)compilerService).compileToByteCode(expr, objectName, evaluationMethods);
        } catch (CannotCompileException e) {
            return expr;
        }
    }


    /**
     * Insert a ByteCodeCandidate into the expression tree. A ByteCodeCandidate monitors how many
     * times it is executed, and after reaching a certain threshold, generates bytecode for faster
     * evaluation of its target expression
     * @param owner the owning construct in the expression tree
     * @param expr the child (target) expression
     * @param objectName name to be used in forming the name of the bytecode class
     * @param requiredEvaluationModes the evaluation modes for which code should be generated
     * @return a new ByteCodeCandidate if the expression is suitable for bytecode generation,
     * or the supplied target expression otherwise.
     */

    @Override
    public Expression makeByteCodeCandidate(ExpressionOwner owner, Expression expr, String objectName, int requiredEvaluationModes) {
        if (!isOptionSet(OptimizerOptions.BYTE_CODE) || !config.getBooleanProperty(Feature.GENERATE_BYTE_CODE)) {
            return expr;
        }
        if (owner instanceof TemplateRule && ((TemplateRule) owner).isDeclaredStreamable()) {
            return expr;
        }
        if (expr instanceof CompiledExpression || expr instanceof Literal || expr instanceof VariableReference || expr instanceof ByteCodeCandidate) {
            return expr;
        }
        final ByteCodeCandidate candidate = new ByteCodeCandidate(owner, expr, objectName, requiredEvaluationModes);
        candidate.setCountDown(config.getCountDown());
        if (config.getBooleanProperty(Feature.MONITOR_HOT_SPOT_BYTE_CODE)) {
            candidate.setByteCodeMonitor(((EnterpriseConfiguration)config).obtainByteCodeMonitor());
        }
        return candidate;
    }

    @Override
    public void injectByteCodeCandidates(Expression expression) {
        if (getOptimizerOptions().isSet(OptimizerOptions.BYTE_CODE) && config.getBooleanProperty(Feature.GENERATE_BYTE_CODE)) {
            if (expression instanceof FilterExpression) {
                BinaryExpression owner = (BinaryExpression) expression;
                Operand actionOp = owner.getRhs();
                Expression action = actionOp.getChildExpression();

                owner.setRhsExpression(
                        makeByteCodeCandidate(actionOp, action, expression.getExpressionName(), Expression.EFFECTIVE_BOOLEAN_VALUE)
                );
            } else if (expression instanceof ForExpression) {
                ForExpression owner = (ForExpression) expression;
                Operand actionOp = owner.getActionOp();
                Expression action = actionOp.getChildExpression();

                owner.setAction(
                        makeByteCodeCandidate(actionOp, action, expression.getExpressionName(), 0)
                );
            } else if (expression instanceof TryCatch) {
                TryCatch owner = (TryCatch) expression;
                Operand actionOp = owner.getTryOperand();
                Expression action = actionOp.getChildExpression();

                actionOp.setChildExpression(
                        makeByteCodeCandidate(actionOp, action, expression.getExpressionName(), 0)
                );
            } else if (expression instanceof SortExpression) {
                SortKeyDefinitionList skdl = ((SortExpression) expression).getSortKeyDefinitionList();
                for (SortKeyDefinition skd : skdl) {
                    Operand sk = skd.getSortKeyOperand();
                    sk.setChildExpression(
                            makeByteCodeCandidate(sk, sk.getChildExpression(), expression.getExpressionName(), 0)
                    );
                }
            } else if (expression instanceof StreamInstr) {
                return;
            }
            for (Operand o : expression.operands()) {
                injectByteCodeCandidates(o.getChildExpression());
            }
        }
    }

//#endif

    @Override
    public Expression optimizeNumberInstruction(NumberInstruction ni, ContextItemStaticInfo contextInfo) {
        if (ni.getLevel() == NumberInstruction.ANY && ni.getSelect() instanceof ContextItemExpression) {
            Pattern count = ni.getCount();
            if (count == null) {
                if (contextInfo.getItemType() instanceof NameTest) {
                    // There's no explicit count pattern but we can infer one
                    count = new NodeTestPattern((NameTest) contextInfo.getItemType());
                } else {
                    return null;
                }
            }
            Pattern from = ni.getFrom();
            if (Pattern.patternContainsVariable(count) || Pattern.patternContainsVariable(from)) {
                return null;
            }
            if (count instanceof PatternThatSetsCurrent || from instanceof PatternThatSetsCurrent) {
                return null;
            }
            TypeHierarchy th = getConfiguration().getTypeHierarchy();
            if (th.relationship(contextInfo.getItemType(), NodeKindTest.ATTRIBUTE) != Affinity.DISJOINT) {
                return null;
            }
            if (th.relationship(contextInfo.getItemType(), NodeKindTest.NAMESPACE) != Affinity.DISJOINT) {
                return null;
            }
            // Create an accumulator that maintains the value of the xsl:number result
            SlotManager slots = getConfiguration().makeSlotManager();
            StructuredQName valueName = new StructuredQName("", NamespaceUri.NULL, "value");
            slots.allocateSlotNumber(valueName);
            SuppliedParameterReference ref = new SuppliedParameterReference(0);
            ArithmeticExpression increment = new ArithmeticExpression(ref, Token.PLUS, Literal.makeLiteral(Int64Value.PLUS_ONE, ni));
            increment.setCalculator(Calculator.INTEGER_INTEGER[Calculator.PLUS]);
            increment.setRetainedStaticContext(ni.getRetainedStaticContext());
            ExpressionTool.allocateSlots(increment, 1, slots);
            AccumulatorRule ruleTarget = new AccumulatorRule(increment, slots, false);
            Rule rule = new Rule(count, ruleTarget, 0, 0, 0, 0, 0);
            StructuredQName genName = new StructuredQName("anon", NamespaceUri.ANONYMOUS, "n" + ni.hashCode());
            SimpleMode rules = new SimpleMode(genName);
            rules.addRule(count, rule);

            if (from != null) {
                ruleTarget = new AccumulatorRule(Literal.makeLiteral(Int64Value.ZERO, ni), slots, false);
                rule = new Rule(from, ruleTarget, 1, 0, 1, 0, 0);
                rules.addRule(from, rule);

                if (th.relationship(count.getItemType(), from.getItemType()) != Affinity.DISJOINT) {
                    // A node might match both the count and the from patterns: in this case set the accumulator to 1
                    Pattern both = new IntersectPattern(from, count);
                    ruleTarget = new AccumulatorRule(Literal.makeLiteral(Int64Value.PLUS_ONE, ni), slots, false);
                    rule = new Rule(both, ruleTarget, 2, 0, 2, 0, 0);
                    rules.addRule(both, rule);
                }
            }
            rules.allocateAllPatternSlots();
            try {
                rules.computeRankings(1);
            } catch (XPathException e) {
                return null;
            }
            Accumulator acc = new Accumulator();
            acc.setType(SequenceType.SINGLE_INTEGER);
            acc.setPreDescentRules(rules);
            acc.setAccumulatorName(genName);
            acc.setInitialValueExpression(Literal.makeLiteral(Int64Value.ZERO, ni));
            acc.setSlotManagerForInitialValueExpression(getConfiguration().makeSlotManager());
            acc.setUniversallyApplicable(true);
            StylesheetPackage pack = (StylesheetPackage)ni.getPackageData();
            AccumulatorRegistry registry = pack.getAccumulatorRegistry();
            registry.addAccumulator(acc);
            Component co = Component.makeComponent(acc, Visibility.PRIVATE, VisibilityProvenance.DEFAULTED, pack, pack);
            acc.setDeclaringComponent(co);

            try {
                // Generate a call on accumulator-before()
                SystemFunction fn = XSLT30FunctionSet.getInstance().makeFunction("accumulator-before", 1);
                fn.setRetainedStaticContext(ni.getRetainedStaticContext());
                Expression call = fn.makeFunctionCall(new StringLiteral(genName.getEQName()));
                Expression nonZero = new CompareToIntegerConstant(new ContextItemExpression(), Token.FNE, 0);
                // If the accumulator value is zero, return an empty sequence
                Expression result = new FilterExpression(call, nonZero);
                // If numbering a selected node, make this the context item for accumulator-before()
                if (!(ni.getSelect() instanceof ContextItemExpression)) {
                    result = new SlashExpression(ni.getSelect(), result);
                }
                ExpressionTool.copyLocationInfo(ni, result);
                result.setRetainedStaticContext(ni.getRetainedStaticContext());
                trace("Replaced xsl:number instruction by accumulator", result);
                return result;
            } catch (XPathException e) {
                throw new AssertionError(e);
            }

        }
        return null;
    }

    /**
     * Assess whether a declared-streamable function is actually streamable. This analysis
     * takes no account of the actual function call, which is assessed later in the streaming
     * adjunct for the UserFunctioncall class
     *
     */


    @Override
    public void assessFunctionStreamability(XSLFunction sourceFunction, UserFunction compiledFunction) {
        FunctionStreamability ability = compiledFunction.getDeclaredStreamability();
        if (ability == FunctionStreamability.UNCLASSIFIED) {
            return;
        }
        List<String> reasons = new ArrayList<>();
        PostureAndSweep ps = Streamability.getStreamability(compiledFunction.getBody(), ContextItemStaticInfoEE.ABSENT, reasons);
        switch (ability) {
            case ABSORBING: {
                if (ps.getPosture() == Posture.ROAMING || ps.getSweep() == Sweep.FREE_RANGING) {
                    StringBuilder message = new StringBuilder("Function " + compiledFunction.getFunctionName().getDisplayName() + " is not streamable");
                    for (String reason : reasons) {
                        message.append(". ").append(reason);
                    }
                    unstreamableFunction(sourceFunction, compiledFunction, message.toString());
                } else if (ps.getPosture() != Posture.GROUNDED) {
                    String message = "A function with streamability=absorbing must return grounded items";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
//                } else if (ps.getSweep() != Sweep.CONSUMING) {
//                    compileError("A function with streamability=absorbing must consume the input stream", "XTSE3430");
                }
                return;
            }
            case ASCENT: {
                if (Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                    String message = "The first argument of a function with streamability=ascent " +
                            "must not accept a sequence of more than one item";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getPosture() != Posture.GROUNDED && ps.getPosture() != Posture.CLIMBING) {
                    String message = "A function with streamability=ascent must return either grounded items " +
                            "or nodes reachable via the ancestor-or-self axis";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getSweep() != Sweep.MOTIONLESS) {
                    String message = "A function with streamability=ascent must not consume the input stream";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                }
                return;
            }
            case FILTER: {
                if (Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                    String message = "The first argument of a function with streamability=filter " +
                            "must not accept a sequence of more than one item";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getPosture() != Posture.STRIDING) {
                    String message = "A function with streamability=filter must return striding nodes";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getSweep() != Sweep.MOTIONLESS) {
                    String message = "A function with streamability=filter must not consume the input stream";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                }
                return;
            }
            case INSPECTION: {
                if (Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                    String message = "The first argument of a function with streamability=inspection " +
                            "must not accept a sequence of more than one item";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getPosture() != Posture.GROUNDED) {
                    String message = "A function with streamability=inspection must not return streamed nodes";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getSweep() != Sweep.MOTIONLESS) {
                    String message = "A function with streamability=inspection must not consume the input stream";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                }
                return;
            }
            case SHALLOW_DESCENT: {
                if (Cardinality.allowsMany(compiledFunction.getArgumentType(0).getCardinality())) {
                    String message = "The first argument of a function with streamability=shallow-descent " +
                            "must not accept a sequence of more than one item";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getPosture() != Posture.STRIDING) {
                    String message = "A function with streamability=shallow-descent must be striding " +
                            "(that is, must return nodes reachable via the child or attribute axis)";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                } else if (ps.getSweep() == Sweep.FREE_RANGING) {
                    String message = "The function body is free-ranging";
                    unstreamableFunction(sourceFunction, compiledFunction, message);
                }
                return;
            }
            default:
                // no action
        }

    }

    private void unstreamableFunction(XSLFunction fn, UserFunction compiledFunction, String message) {
        if (config.getBooleanProperty(Feature.STREAMING_FALLBACK)) {
            message += ". Falling back to unstreamed implementation";
            fn.compileWarning(message, "XTSE3430");
            compiledFunction.setDeclaredStreamability(FunctionStreamability.UNCLASSIFIED);
            fn.getCompilation().setFallbackToNonStreaming(true);
        } else {
            fn.compileError(message, "XTSE3430");
        }
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
