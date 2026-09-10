package com.saxonica.ee.elab.gen;


import com.saxonica.ee.elab.expr.*;
import com.saxonica.ee.elab.fn.*;
import com.saxonica.ee.elab.insn.*;

import java.util.HashMap;

public class ElaboratorMap {

    private static final HashMap<String, Elaborator> map = new HashMap<>();

    static {
        //map.put("mergeAdj", new AdjacentTextNodeMergerCompiler());
        //map.put("analyzeString", new AnalyzeStringCompiler());
        map.put("and-expression", new AndElaborator());
        //map.put("xsl:apply-imports", new ApplyImportsCompiler());
        //map.put("xsl:apply-templates", new ApplyTemplatesCompiler());
        map.put("arithmetic", new ArithmeticElaborator());
        map.put("attGetter", new AttributeGetterElaborator());
        //map.put("convert", new AtomicSequenceConverterCompiler());
        map.put("data", new AtomizerElaborator());
        map.put("axisStep", new AxisExpressionElaborator());
        //map.put("sequence", new BlockCompiler());
        //map.put("xsl:break", new BreakInstrCompiler());
        //map.put("xsl:call-template", new CallTemplateCompiler());
        //map.put("CheckCardinality", new CardinalityCheckerCompiler());
        //map.put("cast", new CastExpressionCompiler());
        //map.put("castable", new CastableExpressionCompiler());
        map.put("choose", new ChooseExprElaborator());
        map.put("xsl:comment", new CommentElaborator());
        map.put("compareToInt", new CompareToIntegerConstantElaborator());
        map.put("compareToString", new CompareToStringConstantElaborator());
        //map.put("xsl:attribute", new ComputedAttributeCompiler());
        //map.put("xsl:element", new ComputedElementCompiler());
        //map.put("conditionalSort", new ConditionalSorterCompiler());
        //map.put("convertUntyped", new AtomicSequenceConverterCompiler());
        map.put("dot", new ContextItemElaborator());
        //map.put("xsl:copy", new CopyCompiler());
        map.put("xsl:document", new DocumentInstrElaborator());
        //map.put("docOrder", new DocumentSorterCompiler());
        //map.put("emptyTextNodeRemover", new EmptyTextNodeRemoverCompiler());
        //map.put("equivalent", new EquivalenceComparisonCompiler());
        //map.put("errorExpr", new ErrorExpressionCompiler());
        //map.put("FLWOR", new FLWORExpressionCompiler());
        map.put("filter", new FilterExprElaborator());
        map.put("first", new FirstItemExprElaborator());
        //map.put("att", new FixedAttributeCompiler());
        map.put("element", new FixedElementElaborator());
        map.put("forEach", new ForEachElaborator());
        //map.put("xsl:for-each-group", new ForEachGroupCompiler());
        map.put("for", new ForExprElaborator());
        //map.put("namedFunctionRef", new FunctionLiteralCompiler());
        //map.put("fnCoercer", new FunctionSequenceCoercerCompiler());
        //map.put("GeneralComparison", new GeneralComparisonCompiler());
        //map.put("homCheck", new HomogeneityCheckerCompiler());
        map.put("nodeComparison", new IdentityComparisonElaborator());
        //map.put("indexedFilter", new IndexedFilterExpressionCompiler());
        map.put("instance", new InstanceOfElaborator());
        //map.put("intRangeTest", new IntegerRangeTestCompiler());
        map.put("isLast", new IsLastElaborator());
        //map.put("treatAs", new ItemCheckerCompiler());
        //map.put("xsl:iterate", new IterateInstrCompiler());
        //map.put("javaCall", new JavaExtensionFunctionCallCompiler());
        map.put("lastOf", new LastItemExprElaborator());
        map.put("let", new LetExprElaborator());
        map.put("literal", new LiteralElaborator());
        //map.put("param", new LocalParamCompiler());
        //map.put("params", new LocalParamBlockCompiler());
        map.put("locVarRef", new LocalVariableReferenceElaborator());
        //map.put("xsl:namespace", new NamespaceConstructorCompiler());
        map.put("minus", new NegateElaborator());
        //map.put("xsl:next-iteration", new NextIterationCompiler());
        //map.put("xsl:next-match", new NextMatchCompiler());
        map.put("or-expression", new OrElaborator());
        //map.put("outerFor", new OuterForExpressionCompiler());
        //map.put("partialApply", new PartialApplyCompiler());
        map.put("xsl:processing-instruction", new ProcessingInstructionElaborator());
        map.put("some", new QuantifiedExprElaborator());
        map.put("every", new QuantifiedExprElaborator());
        //map.put("range", new RangeExpressionCompiler());
        //map.put("xsl:result-document", new ResultDocumentCompiler());
        map.put("root", new RootExprElaborator());
        map.put("simpleStep", new SimpleStepExprElaborator());
        //map.put("atomizeSingleton", new SingletonAtomizerCompiler());
        map.put("pathExpression", new SlashExprElaborator());
        //map.put("sort", new SortExpressionCompiler());
        //map.put("SquareArrayConstructor", new SquareArrayConstructorCompiler());
        //map.put("staticFunctionCall", new StaticFunctionCallCompiler());
        //map.put("subscript", new SubscriptExpressionCompiler());
        //map.put("supplied", new SuppliedParameterReferenceCompiler());
        //map.put("switch", new SwitchExpressionCompiler());
        map.put("sysFuncCall", null); // Handle this one specially
        //map.put("tailCallLoop", new TailCallLoopCompiler());
        //map.put("tail", new TailExpressionCompiler());
        //map.put("trace", new TraceExpressionCompiler());
        //map.put("useAS", new UseAttributeSetCompiler());
        //map.put("userFunctionCall", new UserFunctionCallCompiler());
        map.put("ValueComparison", new ValueComparisonElaborator());
        map.put("xsl:text", new ValueOfElaborator());
        map.put("xsl:value-of", new ValueOfElaborator());
        map.put("xsl:number", new ValueOfElaborator());
        //map.put("varRef", new VariableReferenceCompiler());
        map.put("union", new VennElaborator());
        map.put("intersect", new VennElaborator());
        //map.put("singleton-intersect", new SingletonIntersectExpressionCompiler());
        map.put("except", new VennElaborator());

        map.put("AccessorFnCompiler", new AccessorFnElaborator());
        map.put("BaseURICompiler", new NodePropertyElaborator());
        map.put("BooleanFnCompiler", new BooleanFnElaborator());
        //map.put("ConcatCompiler", new ConcatCompiler());
        map.put("ContainsCompiler", new ContainsFnElaborator());
        map.put("CountCompiler", new CountFnElaborator());
        map.put("EmptyCompiler", new EmptyFnElaborator());
        map.put("DateTimeConstructorCompiler", new DateTimeFnElaborator());
        map.put("ExistsCompiler", new ExistsFnElaborator());
        map.put("ForceCaseCompiler", new ForceCaseFnElaborator());
        map.put("GenerateIdCompiler", new NodePropertyElaborator());
        map.put("LastCompiler", new LastFnElaborator());
        map.put("LocalNameCompiler", new NodePropertyElaborator());
        map.put("MatchesCompiler", new MatchesFnElaborator());
        map.put("NameCompiler", new NodePropertyElaborator());
        map.put("NamespaceUriFnCompiler", new NodePropertyElaborator());
        map.put("NormalizeSpaceCompiler", new NormalizeSpaceFnElaborator());
        map.put("NodeNameFnCompiler", new NodePropertyElaborator());
        map.put("NotFnCompiler", new NotFnElaborator());
        map.put("NumberFnCompiler", new NumberFnElaborator());
        map.put("PositionCompiler", new PositionFnElaborator());
        map.put("QNameFnCompiler", new QNameFnElaborator());
        map.put("RootFunctionCompiler", new RootFnElaborator());
        map.put("RoundingCompiler", new RoundingFnElaborator());
        map.put("StartsWithCompiler", new ContainsFnElaborator());
        map.put("StringFnCompiler", new StringFnElaborator());
        //map.put("StringJoinCompiler", new StringJoinCompiler());
        map.put("StringLengthCompiler", new StringLengthFnElaborator());
        map.put("SubstringAfterCompiler", new SubstringAfterFnElaborator());
        map.put("SubstringCompiler", new SubstringFnElaborator());
        map.put("SubstringBeforeCompiler", new SubstringBeforeFnElaborator());
        map.put("SumCompiler", new SumFnElaborator());
        map.put("TranslateCompiler", new TranslateFnElaborator());


    }

    public static Elaborator getElaborator(String label) {
        return map.get(label);
    }

}

// Copyright (c) 2018-2022 Saxonica Limited

