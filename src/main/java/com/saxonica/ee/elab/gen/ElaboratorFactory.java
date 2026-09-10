package com.saxonica.ee.elab.gen;


import com.saxonica.ee.elab.expr.*;
import com.saxonica.ee.elab.fn.*;
import com.saxonica.ee.elab.insn.*;
import com.saxonica.ee.optim.SwitchExpression;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.expr.sort.ConditionalSorter;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.expr.sort.SortExpression;

import java.util.HashMap;
import java.util.function.Supplier;

/**
 * The {@code ElaboratorFactory} is a factory class used to construct an
 * {@link Elaborator} for a given {@link Expression}.
 */

public class ElaboratorFactory {

    private static final HashMap<String, Supplier<Elaborator>> map = new HashMap<>();

    static {
        Supplier<Elaborator> useFallback = FallbackElaborator::new;
        map.put("mergeAdj", () -> new AdjacentTextNodeMerger.AdjacentTextNodeMergerElaborator());
        map.put("analyzeString", () -> new AnalyzeStringElaborator());
        map.put("and-expression", () -> new AndExpression.AndElaborator());
        map.put("xsl:apply-imports", useFallback);
        map.put("xsl:apply-templates", useFallback);
        map.put("arithmetic", () -> new ArithmeticExpression.ArithmeticElaborator());
        map.put("attGetter", () -> new AttributeGetter.AttributeGetterElaborator());
        map.put("bytecode", () -> new ByteCodeCandidateElaborator());
        map.put("convert", () -> new AtomicSequenceConverter.AtomicSequenceConverterElaborator());
        map.put("data", () -> new Atomizer.AtomizerElaborator());
        map.put("axisStep", () -> new AxisExpression.AxisExpressionElaborator());
        map.put("sequence", () -> new Block.BlockElaborator());
        map.put("xsl:break", useFallback);
        map.put("xsl:call-template", useFallback);
        map.put("CheckCardinality", () -> new CardinalityChecker.CardinalityCheckerElaborator());
        map.put("cast", () -> new CastExpression.CastExprElaborator());
        //map.put("castable", new CastableExpressionCompiler());
        map.put("choose", () -> new Choose.ChooseExprElaborator());
        map.put("xsl:comment", useFallback);
        map.put("compareToInt", () -> new CompareToIntegerConstant.CompareToIntegerConstantElaborator());
        map.put("compareToString", () -> new CompareToStringConstant.CompareToStringConstantElaborator());
        //map.put("xsl:attribute", new ComputedAttributeCompiler());
        //map.put("xsl:element", new ComputedElementCompiler());
        map.put("conditionalSort", () -> new ConditionalSorter.ConditionalSorterElaborator());
        map.put("convertUntyped", () -> new UntypedSequenceConverter.UntypedSequenceConverterElaborator());
        map.put("dot", () -> new ContextItemExpression.ContextItemElaborator());
        //map.put("xsl:copy", new CopyCompiler());
        map.put("xsl:document", useFallback);
        map.put("docOrder", () -> new DocumentSorter.DocumentSorterElaborator());
        map.put("emptyTextNodeRemover", () -> new EmptyTextNodeRemover.EmptyTextNodeRemoverElaborator());
        //map.put("equivalent", new EquivalenceComparisonCompiler());
        map.put("errorExpr", useFallback);
        map.put("FLWOR", useFallback);
        map.put("filter", () -> new FilterExpression.FilterExprElaborator());
        map.put("first", () -> new FirstItemExpression.FirstItemExprElaborator());
        map.put("att", () -> new FallbackElaborator());
        map.put("element", () -> new FixedElementElaborator());
        map.put("forEach", () -> new ForEachElaborator());
        //map.put("xsl:for-each-group", new ForEachGroupCompiler());
        map.put("for", () -> new ForExpression.ForExprElaborator());
        //map.put("namedFunctionRef", new FunctionLiteralElaborator());
        //map.put("fnCoercer", new FunctionSequenceCoercerCompiler());
        map.put("GeneralComparison", () -> new GeneralComparison.GeneralComparisonElaborator());
        map.put("gVarRef", () -> new GlobalVariableReference.GlobalVariableReferenceElaborator());
        map.put("homCheck", () -> new HomogeneityChecker.HomogeneityCheckerElaborator());
        map.put("nodeComparison", () -> new IdentityComparison.IdentityComparisonElaborator());
        //map.put("indexedFilter", new IndexedFilterExpressionCompiler());
        map.put("instance", () -> new InstanceOfExpression.InstanceOfElaborator());
        //map.put("intRangeTest", new IntegerRangeTestCompiler());
        map.put("isLast", () -> new IsLastExpression.IsLastElaborator());

        //map.put("xsl:iterate", new IterateInstrCompiler());
        //map.put("javaCall", new JavaExtensionFunctionCallCompiler());
        map.put("lastOf", () -> new LastItemExpression.LastItemExprElaborator());
        map.put("let", () -> new LetExpression.LetExprElaborator());
        map.put("literal", () -> new Literal.LiteralElaborator());
        //map.put("param", new LocalParamCompiler());
        //map.put("params", new LocalParamBlockCompiler());
        map.put("locVarRef", () -> new LocalVariableReference.LocalVariableReferenceElaborator());
        //map.put("xsl:namespace", new NamespaceConstructorCompiler());
        map.put("minus", () -> new NegateExpression.NegateElaborator());
        //map.put("xsl:next-iteration", new NextIterationCompiler());
        //map.put("xsl:next-match", new NextMatchCompiler());
        map.put("or-expression", () -> new OrExpression.OrElaborator());
        //map.put("outerFor", new OuterForExpressionCompiler());
        //map.put("partialApply", new PartialApplyCompiler());
        map.put("xsl:processing-instruction", useFallback);
        map.put("some", () -> new QuantifiedExpression.QuantifiedExprElaborator());
        map.put("every", () -> new QuantifiedExpression.QuantifiedExprElaborator());
        //map.put("range", new RangeExpressionCompiler());
        //map.put("xsl:result-document", new ResultDocumentCompiler());
        map.put("root", () -> new RootExpression.RootExprElaborator());
        map.put("simpleStep", () -> new SimpleStepExpression.SimpleStepExprElaborator());
        //map.put("atomizeSingleton", new SingletonAtomizerCompiler());
        map.put("pathExpression", () -> new SlashExpression.SlashExprElaborator());
        map.put("sort", () -> new SortExpression.SortExprElaborator());
        //map.put("SquareArrayConstructor", new SquareArrayConstructorCompiler());
        //map.put("staticFunctionCall", new StaticFunctionCallCompiler());
        map.put("subscript", () -> new SubscriptExpression.SubscriptExprElaborator());
        //map.put("supplied", new SuppliedParameterReferenceCompiler());
        map.put("switch", () -> new SwitchExpression.SwitchExprElaborator());
        map.put("sysFuncCall", null); // Handle this one specially
        //map.put("tailCallLoop", new TailCallLoopCompiler());
        map.put("tail", () -> new TailExpression.TailExprElaborator());
        //map.put("trace", new TraceExpressionCompiler());
        map.put("treatAs", () -> new ItemChecker.ItemCheckerElaborator());
        //map.put("useAS", new UseAttributeSetCompiler());
        //map.put("userFunctionCall", new UserFunctionCallCompiler());
        map.put("ValueComparison", () -> new ValueComparison.ValueComparisonElaborator());
        map.put("xsl:text", useFallback);
        map.put("xsl:value-of", useFallback);
        map.put("xsl:number", useFallback);
        //map.put("varRef", new VariableReferenceCompiler());
        map.put("union", () -> new VennExpression.VennElaborator());
        map.put("intersect", () -> new VennExpression.VennElaborator());
        map.put("singleton-intersect", () -> new SingletonIntersectExpression.SingletonIntersectElaborator());
        map.put("except", () -> new VennExpression.VennElaborator());

        // System functions

        map.put("AccessorFnCompiler", () -> new AccessorFnElaborator());
        map.put("BaseURICompiler", () -> new BaseUriFnElaborator());
        map.put("BooleanFnCompiler", () -> new BooleanFnElaborator());
        //map.put("ConcatCompiler", new ConcatCompiler());
        map.put("ContainsCompiler", () -> new ContainsFnElaborator());
        map.put("CountCompiler", () -> new CountFnElaborator());
        map.put("EmptyCompiler", () -> new EmptyFnElaborator());
        map.put("DateTimeConstructorCompiler", () -> new DateTimeFnElaborator());
        map.put("ExistsCompiler", () -> new ExistsFnElaborator());
        map.put("ForceCaseCompiler", () -> new ForceCaseFnElaborator());
        map.put("GenerateIdCompiler", () -> new NodePropertyElaborator());
        map.put("LastCompiler", () -> new LastFnElaborator());
        map.put("LocalNameCompiler", () -> new NodePropertyElaborator());
        map.put("MatchesCompiler", () -> new MatchesFnElaborator());
        map.put("NameCompiler", () -> new NodePropertyElaborator());
        map.put("NamespaceUriFnCompiler", () -> new NamespaceUriFnElaborator());
        map.put("NormalizeSpaceCompiler", () -> new NormalizeSpaceFnElaborator());
        map.put("NodeNameFnCompiler", () -> new NodeNameFnElaborator());
        map.put("NotFnCompiler", () -> new NotFnElaborator());
        map.put("NumberFnCompiler", () -> new NumberFnElaborator());
        map.put("PositionCompiler", () -> new PositionFnElaborator());
        map.put("QNameFnCompiler", () -> new QNameFnElaborator());
        map.put("RootFunctionCompiler", () -> new RootFnElaborator());
        map.put("RoundingCompiler", () -> new RoundingFnElaborator());
        map.put("StartsWithCompiler", () -> new ContainsFnElaborator());
        map.put("StringFnCompiler", () -> new StringFnElaborator());
        //map.put("StringJoinCompiler", new StringJoinCompiler());
        map.put("StringLengthCompiler", () -> new StringLengthFnElaborator());
        map.put("SubstringAfterCompiler", () -> new SubstringAfterFnElaborator());
        map.put("SubstringCompiler", () -> new SubstringFnElaborator());
        map.put("SubstringBeforeCompiler", () -> new SubstringBeforeFnElaborator());
        map.put("SumCompiler", () -> new SumFnElaborator());
        map.put("TranslateCompiler", () -> new TranslateFnElaborator());


    }

    public static Supplier<Elaborator> getElaborator(String label) {
        if (label == null) {
            return null;
        }
        return map.get(label);
    }

}

// Copyright (c) 2018-2022 Saxonica Limited

