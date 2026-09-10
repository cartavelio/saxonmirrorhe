package com.saxonica.functions.extfn;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.VendorFunctionSetHE;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.type.*;
import net.sf.saxon.value.SequenceType;

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

    private void init() {

        importFunctionSet(VendorFunctionSetHE.getInstance());

        SpecificFunctionType orderKey = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.OPTIONAL_ATOMIC);

        SpecificFunctionType predicate = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);

        register("array-member", 1, ArrayMember.class, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null);

        register("compile-query", 1, CompileQuery.class, config.getJavaExternalObjectType(XQueryExpression.class), ONE, 0, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("compile-stylesheet", 1, CompileStylesheet.class, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, 0, 0)
                .arg(0, NodeKindTest.DOCUMENT, ONE, null);

        register("deep-equal", 4, DeepEqual.class, BuiltInAtomicType.BOOLEAN, ONE, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null);

        register("decimal-divide", 3, DecimalDivide.class, BuiltInAtomicType.DECIMAL, OPT, 0, 0)
                .arg(0, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

//        register("eval", 1, Eval.class, BuiltInAtomicType.BOOLEAN, ONE, 0, 0)
//                .arg(0, NumericType.getInstance(), OPT, EMPTY);


        register("eval", 10, Eval.class, AnyItemType.getInstance(), STAR, 0, CITEM)
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

        register("evaluate", 10, Evaluate.class, AnyItemType.getInstance(), STAR, 0, CITEM)
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

        register("evaluate-node", 1, EvaluateNode.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY);

        register("expression", 1, ExpressionFn.class, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, 0, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("expression", 2, ExpressionFn.class, config.getJavaExternalObjectType(Evaluate.PreparedExpression.class), ONE, 0, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, NodeKindTest.ELEMENT, ONE, null);

        register("function-annotations", 1, FunctionAnnotations.class, FunctionAnnotations.RETURN_TYPE, STAR, 0, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);

        register("group-starting", 2, GroupStarting.class, ArrayItemType.ANY_ARRAY_TYPE , STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, EMPTY);

        register("key-map", 4, KeyMap.class, new MapType(BuiltInAtomicType.STRING, SequenceType.NODE_SEQUENCE), ONE, 0, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null);

        register("highest", 1, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("highest", 2, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null);

        register("index", 2, Index.class, MapType.getInstance(), ONE, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, EMPTY);

        register("last-modified", 1, LastModified.class, BuiltInAtomicType.DATE_TIME, OPT, 0, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("leading", 1, Leading.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("leading", 2, Leading.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, null);

        register("lowest", 1, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("lowest", 2, HighestOrLowest.class, AnyItemType.getInstance(), STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null);

        FunctionItemType tester = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE},
                SequenceType.SINGLE_BOOLEAN);

        register("map-search", 2, MapSearch.class, MapType.ANY_MAP_TYPE, STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null);

        register("map-search", 3, MapSearch.class, MapType.ANY_MAP_TYPE, STAR, 0, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                .arg(2, tester, ONE | INS, null);

        register("message-count", 1, MessageCount.class, BuiltInAtomicType.INTEGER, ONE, 0, LATE|SIDE)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null);

        register("object-map", 1, ObjectMap.class, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0, 0)
                .arg(0, AnyExternalObjectType.THE_INSTANCE, ONE, null);

        register("parse", 1, Parse.class, NodeKindTest.DOCUMENT, ONE, 0, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse-html", 1, ParseHtml.class, NodeKindTest.DOCUMENT, ONE, 0, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse-XPath", 3, ParseXPath.class, NodeKindTest.DOCUMENT, ONE, 0, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null);

        register("pedigree", 1, PedigreeFn.class, MapType.getInstance(), OPT, 0, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);

        register("query", 1, QueryFn.class, AnyItemType.getInstance(), STAR, 0, LATE | CITEM)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY);

        register("query", 2, QueryFn.class, AnyItemType.getInstance(), STAR, 0, LATE)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null);

        register("query", 3, QueryFn.class, AnyItemType.getInstance(), STAR, 0, LATE)
                .arg(0, config.getJavaExternalObjectType(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null)
                .arg(2, AnyNodeTest.getInstance(), STAR, null);

        register("send-mail", 3, SendMail.class, BuiltInAtomicType.BOOLEAN, ONE, 0, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null);

        register("send-mail", 4, SendMail.class, BuiltInAtomicType.BOOLEAN, ONE, 0, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null)
                .arg(3, AnyItemType.getInstance(), STAR, null);

        register("serialize", 2, Serialize.class, BuiltInAtomicType.STRING, ONE, 0, 0)
                .arg(0, AnyNodeTest.getInstance(), ONE, null)
                .arg(1, AnyItemType.getInstance(), ONE, null);

        register("timestamp", 0, TimestampFn.class, BuiltInAtomicType.DATE_TIME_STAMP, ONE, 0, SIDE|LATE);

        register("transform", 2, TransformFn.class, NodeKindTest.DOCUMENT, OPT, 0, 0)
                .arg(0, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null);

        register("transform", 3, TransformFn.class, NodeKindTest.DOCUMENT, OPT, 0, 0)
                .arg(0, config.getJavaExternalObjectType(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, AnyItemType.getInstance(), STAR, null);

        register("with-pedigree", 1, WithPedigree.class, AnyFunctionType.getInstance(), ONE, 0, 0)
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

// Copyright (c) 2004-2016 Saxonica Limited.