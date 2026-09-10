package com.saxonica.functions.extfn;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.functions.Doc_2;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.VendorFunctionSetHE;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.type.*;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringItem;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * The VendorFunctionLibraryPE represents additions to the set of Saxon vendor-supplied
 * extension functions that are available only with the Saxon-PE and Saxon-EE editions
 */

public class VendorFunctionSetPE extends BuiltInFunctionSet {

    protected ProfessionalConfiguration config;

    public VendorFunctionSetPE(ProfessionalConfiguration config) {
        this.config = config;
        init();
    }

    private final static List<String> analyzeUriKeys =
            Arrays.asList("isValid", "isAbsolute", "isOpaque", "port", "queryParams",
                          "authority", "rawAuthority",
                          "userInfo", "rawUserInfo", "scheme", "host", "schemeSpecificPart",
                          "rawSchemeSpecificPart", "path", "rawPath", "query", "rawQuery",
                          "fragment", "rawFragment", "asciiString", "error");
    private final static List<SequenceType> analyzeUriValues =
            Arrays.asList(SequenceType.SINGLE_BOOLEAN,
                          SequenceType.SINGLE_BOOLEAN,
                          SequenceType.SINGLE_BOOLEAN,
                          SequenceType.OPTIONAL_INTEGER,
                          SequenceType.makeSequenceType(
                                  new MapType(BuiltInAtomicType.STRING, SequenceType.SINGLE_STRING),
                                  StaticProperty.ALLOWS_ZERO_OR_ONE),
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING,
                          SequenceType.OPTIONAL_STRING
            );

    private final static RecordTest analyzeUriResult = new RecordTest(
            analyzeUriKeys, analyzeUriValues, new ArrayList<>(), false);

    private final static SpecificFunctionType predicate = new SpecificFunctionType(
            new SequenceType[]{SequenceType.SINGLE_ITEM},
            SequenceType.SINGLE_BOOLEAN);

    private void init() {

        importFunctionSet(VendorFunctionSetHE.getInstance());

        SpecificFunctionType orderKey = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.OPTIONAL_ATOMIC);




        register("analyze-uri", 1, AnalyzeUri.class, analyzeUriResult, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);


        register("characters", 1, CharactersFn.class, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("compile-query", 1, CompileQuery.class, config.getJavaExternalObjectType(XQueryExpression.class), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("compile-stylesheet", 1, CompileStylesheet.class, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, 0)
                .arg(0, NodeKindTest.DOCUMENT, ONE, null);

        register("deep-equal", 4, DeepEqual.class, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null);

        register("decimal-divide", 3, DecimalDivide.class, BuiltInAtomicType.DECIMAL, OPT, 0)
                .arg(0, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("EQName", 1, EQName.class, BuiltInAtomicType.QNAME, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("eval", 10, Eval.class, AnyItemType.getInstance(), STAR, CITEM)
                .arg(0, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, AnyItemType.getInstance(), STAR, null)
                .arg(3, AnyItemType.getInstance(), STAR, null)
                .arg(4, AnyItemType.getInstance(), STAR, null)
                .arg(5, AnyItemType.getInstance(), STAR, null)
                .arg(6, AnyItemType.getInstance(), STAR, null)
                .arg(7, AnyItemType.getInstance(), STAR, null)
                .arg(8, AnyItemType.getInstance(), STAR, null)
                .arg(9, AnyItemType.getInstance(), STAR, null);

        registerReducedArityVariants("eval#10", 1, 9);

        register("evaluate", 10, Evaluate.class, AnyItemType.getInstance(), STAR, CITEM)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, AnyItemType.getInstance(), STAR, null)
                .arg(3, AnyItemType.getInstance(), STAR, null)
                .arg(4, AnyItemType.getInstance(), STAR, null)
                .arg(5, AnyItemType.getInstance(), STAR, null)
                .arg(6, AnyItemType.getInstance(), STAR, null)
                .arg(7, AnyItemType.getInstance(), STAR, null)
                .arg(8, AnyItemType.getInstance(), STAR, null)
                .arg(9, AnyItemType.getInstance(), STAR, null);

        registerReducedArityVariants("evaluate#10", 1, 9);

        register("expression", 1, ExpressionFn.class, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("expression", 2, ExpressionFn.class, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, NodeKindTest.ELEMENT, ONE, null);

        register("function-annotations", 1, FunctionAnnotations.class, FunctionAnnotations.RETURN_TYPE, STAR, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);

        register("group-starting", 2, GroupStarting.class, ArrayItemType.ANY_ARRAY_TYPE , STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("highest", 1, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("highest", 2, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null);

        register("index", 2, Index.class, MapType.getInstance(), ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, EMPTY);

        register("index-where", 2, IndexWhere.class, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("in-scope-namespaces", 1, InScopeNamespacesFn.class, MapType.getInstance(), ONE, 0)
                .arg(0, NodeKindTest.ELEMENT, ONE | INS, null);

        register("is-NaN", 1, IsNaN.class, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null);

        register("items-after", 2, Starting.ItemsAfter.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("items-from", 2, Starting.ItemsFrom.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("items-before", 2, Ending.ItemsBefore.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("items-until", 2, Ending.ItemsUntil.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("key-map", 4, KeyMap.class, new MapType(BuiltInAtomicType.STRING, SequenceType.NODE_SEQUENCE), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null);

        register("last-modified", 1, LastModified.class, BuiltInAtomicType.DATE_TIME, OPT, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("leading", 1, Leading.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("leading", 2, Leading.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, null);

        register("lowest", 1, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("lowest", 2, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null);

        FunctionItemType tester = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE},
                SequenceType.SINGLE_BOOLEAN);

        register("map-search", 2, MapSearch.class, MapType.ANY_MAP_TYPE, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null);

        register("map-search", 3, MapSearch.class, MapType.ANY_MAP_TYPE, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                .arg(2, tester, ONE | INS, null);

        register("message-count", 1, MessageCount.class, BuiltInAtomicType.INTEGER, ONE, LATE|SIDE)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null);

        LocalUnionType stringOrQName = new LocalUnionType(
                Arrays.asList(BuiltInAtomicType.STRING, BuiltInAtomicType.QNAME));

        register("new-attribute", 2, NewAttribute.class, NodeKindTest.ATTRIBUTE, ONE, LATE | SIDE | NEW)
                .arg(0, stringOrQName, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null);

        register("new-comment",1, NewComment.class, NodeKindTest.COMMENT, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null);

        register("new-document", 1, NewDocument.class, NodeKindTest.DOCUMENT, ONE, LATE | SIDE | NEW)
                .arg(0, AnyItemType.getInstance(), STAR, null);

        register("new-element", 2, NewElement.class, NodeKindTest.ELEMENT, ONE, LATE | SIDE | NEW)
                .arg(0, stringOrQName, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null);

        register("new-namespace", 2, NewNamespace.class, NodeKindTest.NAMESPACE, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("new-processing-instruction", 2, NewProcessingInstruction.class, NodeKindTest.PROCESSING_INSTRUCTION, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null);

        register("new-text", 1, NewText.class, NodeKindTest.TEXT, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null);


        register("object-map", 1, ObjectMap.class, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0)
                .arg(0, AnyExternalObjectType.THE_INSTANCE, ONE, null);

        register("parse", 1, Parse.class, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse", 2, Parse.class, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .optionDetails(Doc_2.makeOptionsParameter());

        register("parse-html", 1, ParseHtml.class, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse-html", 2, ParseHtml.class, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .optionDetails(Doc_2.makeOptionsParameter());

        register("parse-XPath", 3, ParseXPath.class, NodeKindTest.DOCUMENT, ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null);

        register("pedigree", 1, PedigreeFn.class, MapType.getInstance(), OPT, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);

        register("query", 1, QueryFn.class, AnyItemType.getInstance(), STAR, LATE | CITEM)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY);

        register("query", 2, QueryFn.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null);

        register("query", 3, QueryFn.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null)
                .arg(2, AnyNodeTest.getInstance(), STAR, null);

        FunctionItemType stringReplacer = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_STRING},
                SequenceType.SINGLE_STRING);

        register("replace-with", 3, ReplaceWith.class, BuiltInAtomicType.STRING,
                 ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringItem.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, stringReplacer, ONE, null);

        register("replace-with", 4, ReplaceWith.class, BuiltInAtomicType.STRING,
                 ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringItem.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, stringReplacer, ONE, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null);
        
        register("send-mail", 3, SendMail.class, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null);

        register("send-mail", 4, SendMail.class, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null)
                .arg(3, AnyItemType.getInstance(), STAR, null);

        register("serialize", 2, Serialize.class, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyNodeTest.getInstance(), ONE, null)
                .arg(1, AnyItemType.getInstance(), ONE, null);

        register("slice", 2, Slice.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.INTEGER, STAR, null);

        register("timestamp", 0, TimestampFn.class, BuiltInAtomicType.DATE_TIME_STAMP, ONE, SIDE|LATE);

        register("transform", 2, TransformFn.class, NodeKindTest.DOCUMENT, OPT, 0)
                .arg(0, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null);

        register("transform", 3, TransformFn.class, NodeKindTest.DOCUMENT, OPT, 0)
                .arg(0, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, AnyItemType.getInstance(), STAR, null);

        register("unindexed", 1, Unindexed.class, AnyItemType.getInstance(), STAR, CTRL)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance());

        register("with-pedigree", 1, WithPedigree.class, AnyFunctionType.getInstance(), ONE, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);



    }

    /**
     * Return the namespace URI for the functions local to this function set.
     *
     * @return the namespace URI of the functions local to this function set.
     * Note that functions imported from another function set may have a different
     * namespace URI.
     */
    @Override
    public String getNamespace() {
        return NamespaceConstant.SAXON;
    }

    @Override
    public String getConventionalPrefix() {
        return "saxon";
    }
}

// Copyright (c) 2004-2020 Saxonica Limited