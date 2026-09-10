package com.saxonica.ee.bytecode.util;


import com.saxonica.ee.bytecode.*;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.instruct.IterateInstr;
import net.sf.saxon.expr.parser.ICompilerService;
import net.sf.saxon.functions.ContextItemAccessorFunction;

import java.util.HashMap;

public class ExpressionCompilerMap {

    private static final HashMap<String, ExpressionCompiler> map = new HashMap<>();

    static {
        map.put("mergeAdj", new AdjacentTextNodeMergerCompiler());
        map.put("analyzeString", new AnalyzeStringCompiler());
        map.put("and-expression", new AndExpressionCompiler());
        map.put("xsl:apply-imports", new ApplyImportsCompiler());
        map.put("xsl:apply-templates", new ApplyTemplatesCompiler());
        map.put("arithmetic", new ArithmeticCompiler());
        map.put("attGetter", new AttributeGetterCompiler());
        map.put("convert", new AtomicSequenceConverterCompiler());
        map.put("data", new AtomizerCompiler());
        map.put("axisStep", new AxisExpressionCompiler());
        map.put("sequence", new BlockCompiler());
        map.put("xsl:break", new BreakInstrCompiler());
        map.put("xsl:call-template", new CallTemplateCompiler());
        map.put("CheckCardinality", new CardinalityCheckerCompiler());
        map.put("cast", new CastExpressionCompiler());
        map.put("castable", new CastableExpressionCompiler());
        map.put("choose", new ChooseCompiler());
        map.put("xsl:comment", new CommentCompiler());
        map.put("compareToInt", new CompareToIntegerConstantCompiler());
        map.put("compareToString", new CompareToStringConstantCompiler());
        map.put("xsl:attribute", new ComputedAttributeCompiler());
        map.put("xsl:element", new ComputedElementCompiler());
        map.put("conditionalSort", new ConditionalSorterCompiler());
        map.put("consume", new SuppliedParameterReferenceCompiler());
        map.put("convertUntyped", new AtomicSequenceConverterCompiler());
        map.put("dot", new ContextItemCompiler());
        map.put("xsl:copy", new CopyCompiler());
        map.put("xsl:document", new DocumentInstrCompiler());
        map.put("docOrder", new DocumentSorterCompiler());
        map.put("emptyTextNodeRemover", new EmptyTextNodeRemoverCompiler());
        map.put("equivalent", new EquivalenceComparisonCompiler());
        map.put("errorExpr", new ErrorExpressionCompiler());
        map.put("FLWOR", new FLWORExpressionCompiler());
        map.put("filter", new FilterExpressionCompiler());
        map.put("first", new FirstItemExpressionCompiler());
        map.put("att", new FixedAttributeCompiler());
        map.put("element", new FixedElementCompiler());
        map.put("forEach", new SlashExpressionCompiler());
        map.put("xsl:for-each-group", new ForEachGroupCompiler());
        map.put("for", new ForExpressionCompiler());
        map.put("namedFunctionRef", new FunctionLiteralCompiler());
        map.put("fnCoercer", new FunctionSequenceCoercerCompiler());
        map.put("GeneralComparison", new GeneralComparisonCompiler());
        map.put("homCheck", new HomogeneityCheckerCompiler());
        map.put("nodeComparison", new IdentityComparisonCompiler());
        map.put("indexedFilter", new IndexedFilterExpressionCompiler());
        map.put("instance", new InstanceOfCompiler());
        map.put("intRangeTest", new IntegerRangeTestCompiler());
        map.put("isLast", new IsLastExpressionCompiler());
        map.put("treatAs", new ItemCheckerCompiler());
        map.put("xsl:iterate", new IterateInstrCompiler());
        map.put("javaCall", new JavaExtensionFunctionCallCompiler());
        map.put("lastOf", new LastItemExpressionCompiler());
        map.put("let", new LetExpressionCompiler());
        map.put("literal", new LiteralCompiler());
        map.put("param", new LocalParamCompiler());
        map.put("params", new LocalParamBlockCompiler());
        map.put("locVarRef", new LocalVariableReferenceCompiler());
        map.put("xsl:namespace", new NamespaceConstructorCompiler());
        map.put("minus", new NegateExpressionCompiler());
        map.put("xsl:next-iteration", new NextIterationCompiler());
        map.put("xsl:next-match", new NextMatchCompiler());
        map.put("or-expression", new OrExpressionCompiler());
        map.put("outerFor", new OuterForExpressionCompiler());
        map.put("partialApply", new PartialApplyCompiler());
        map.put("xsl:processing-instruction", new ProcessingInstructionCompiler());
        map.put("some", new QuantifiedExpressionCompiler());
        map.put("every", new QuantifiedExpressionCompiler());
        map.put("range", new RangeExpressionCompiler());
        map.put("xsl:result-document", new ResultDocumentCompiler());
        map.put("root", new RootExpressionCompiler());
        map.put("simpleStep", new SimpleStepExpressionCompiler());
        map.put("atomizeSingleton", new SingletonAtomizerCompiler());
        map.put("pathExpression", new SlashExpressionCompiler());
        map.put("sort", new SortExpressionCompiler());
        map.put("SquareArrayConstructor", new SquareArrayConstructorCompiler());
        map.put("staticFunctionCall", new StaticFunctionCallCompiler());
        map.put("subscript", new SubscriptExpressionCompiler());
        map.put("supplied", new SuppliedParameterReferenceCompiler());
        map.put("switch", new SwitchExpressionCompiler());
        map.put("sysFuncCall", null); // Handle this one specially
        map.put("tailCallLoop", new TailCallLoopCompiler());
        map.put("tail", new TailExpressionCompiler());
        map.put("trace", new TraceExpressionCompiler());
        map.put("useAS", new UseAttributeSetCompiler());
        map.put("userFunctionCall", new UserFunctionCallCompiler());
        map.put("ValueComparison", new ValueComparisonCompiler());
        map.put("xsl:text", new ValueOfCompiler());
        map.put("xsl:value-of", new ValueOfCompiler());
        map.put("xsl:number", new ValueOfCompiler());
        map.put("varRef", new VariableReferenceCompiler());
        map.put("union", new VennExpressionCompiler());
        map.put("intersect", new VennExpressionCompiler());
        map.put("singleton-intersect", new SingletonIntersectExpressionCompiler());
        map.put("except", new VennExpressionCompiler());

    }

    public static ExpressionCompiler getExpressionCompiler(Expression expr, ICompilerService compiler) throws CannotCompileException {
        if (expr instanceof ByteCodeCandidate) {
            expr = ((ByteCodeCandidate) expr).getBaseExpression();
        }
        if (expr instanceof SystemFunctionCall) {
            SystemFunctionCall fc = (SystemFunctionCall) expr;
            String compilerName = fc.getTargetFunction().getCompilerName();
            if (compilerName == null) {
                if (fc.getTargetFunction() instanceof ContextItemAccessorFunction) {
                    Expression exp = ((ContextItemAccessorFunction) fc.getTargetFunction()).makeContextItemExplicit();
                    return getExpressionCompiler(exp, compiler);
                }
            } else {
                return ((CompilerService) compiler).getNamedExpressionCompiler(compilerName);
            }
        } else if (expr instanceof CompiledExpression) {
            // This happens when we need to compile an expression such as F(E), and E has
            // already been compiled
            return new CompiledExpressionCompiler();
        } else if (expr instanceof IterateInstr) {
            if (((IterateInstr)expr).isCompilable()) {
                return new IterateInstrCompiler();
            } else {
                return new InterpretedExpressionCompiler();
            }
        }
        String expressionName = expr.getExpressionName();
        ExpressionCompiler ec = map.get(expressionName);
        if (ec == null) {
            if (expr instanceof Callable) {
                return new CallableExpressionCompiler();
            } else {
                return new InterpretedExpressionCompiler();
            }
        }
        return ec;

    }
}

// Copyright (c) 2018-2022 Saxonica Limited

