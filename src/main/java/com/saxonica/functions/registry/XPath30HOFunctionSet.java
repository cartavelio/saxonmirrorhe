package com.saxonica.functions.registry;

import com.saxonica.functions.hof.*;
import net.sf.saxon.functions.ApplyFn;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath20FunctionSet;
import net.sf.saxon.functions.registry.XPath30FunctionSet;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XPath 3.0 in the
 * optional Higher-Order-Functions feature
 */

public class XPath30HOFunctionSet extends BuiltInFunctionSet {

    private static XPath30HOFunctionSet THE_INSTANCE = new XPath30HOFunctionSet();

    public static XPath30HOFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XPath30HOFunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath30FunctionSet.getInstance());
        importFunctionSet(XPath20FunctionSet.getInstance());

        // Functions requiring the Higher-Order Functions Feature

        SpecificFunctionType ft;

        // fn:apply()

        register("apply", 2, ApplyFn.class, AnyItemType.getInstance(),
                 STAR, HOF, LATE)
                .arg(0, AnyFunctionType.getInstance(), ONE, null)
                .arg(1, ArrayItemType.ANY_ARRAY_TYPE, ONE, null);


        // fn:filter()

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);
        register("filter", 2, FilterFn.class, AnyItemType.getInstance(),
                 STAR, HOF, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, ft, ONE, null);


        // fn:fold-left()

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_ITEM},
                SequenceType.ANY_SEQUENCE);
        register("fold-left", 3, FoldLeftFn.class, AnyItemType.getInstance(),
                 STAR, HOF, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, ft, ONE, null);

        // fn:fold-right()

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM, SequenceType.ANY_SEQUENCE},
                SequenceType.ANY_SEQUENCE);
        register("fold-right", 3, FoldRightFn.class, AnyItemType.getInstance(),
                 STAR, HOF, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, ft, ONE, null);

        // fn:for-each() (was fn:map)

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.ANY_SEQUENCE);
        register("for-each", 2, ForEachFn.class, AnyItemType.getInstance(),
                 STAR, HOF, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, ft, ONE, null);


        // fn:for-each-pair() (was fn:map-pairs)

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM, SequenceType.SINGLE_ITEM},
                SequenceType.ANY_SEQUENCE);
        register("for-each-pair", 3, ForEachPairFn.class, AnyItemType.getInstance(),
                 STAR, HOF, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(2, ft, ONE, null);

        // fn:function-arity

        register("function-arity", 1, FunctionArity.class, BuiltInAtomicType.INTEGER,
                 ONE, HOF, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);

        // fn:function-lookup

        register("function-lookup", 2, FunctionLookup.class, AnyFunctionType.getInstance(),
                 OPT, HOF,
                 FOCUS | DEPENDS_ON_STATIC_CONTEXT | LATE)
                .arg(0, BuiltInAtomicType.QNAME, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        // fn:function-name

        register("function-name", 1, FunctionName.class, BuiltInAtomicType.QNAME,
                 OPT, HOF, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null);


    }


}

// Copyright (c) 2018 Saxonica Limited
