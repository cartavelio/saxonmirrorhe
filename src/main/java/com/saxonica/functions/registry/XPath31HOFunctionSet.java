package com.saxonica.functions.registry;

import com.saxonica.functions.hof.RandomNumberGenerator;
import com.saxonica.functions.hof.Sort_3;
import com.saxonica.functions.hof.LoadXqueryModule;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XPath 3.1 in the
 * optional Higher-Order-Functions feature
 */

public class XPath31HOFunctionSet extends BuiltInFunctionSet {

    private static XPath31HOFunctionSet THE_INSTANCE = new XPath31HOFunctionSet();

    public static XPath31HOFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XPath31HOFunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath31FunctionSet.getInstance());
        importFunctionSet(XPath30HOFunctionSet.getInstance());

        SpecificFunctionType ft;

        // fn:load-xquery-module (XPath 3.1)

        register("load-xquery-module", 1, LoadXqueryModule.class, MapType.ANY_MAP_TYPE, ONE, HOF, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("load-xquery-module", 2, LoadXqueryModule.class, MapType.ANY_MAP_TYPE, ONE, HOF, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null) // null or EMPTY?
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .optionDetails(LoadXqueryModule.makeOptionsParameter());

        // fn:random-number-generator (XPath 3.1)

        register("random-number-generator", 0, RandomNumberGenerator.class, RandomNumberGenerator.RETURN_TYPE, ONE, HOF, LATE);

        register("random-number-generator", 1, RandomNumberGenerator.class, RandomNumberGenerator.RETURN_TYPE, ONE, HOF, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, OPT, null);

        // fn:sort (XPath 3.1). fn:sort#3 is higher-order, so it goes here; fn:sort#1 and fn:sort#2 are in Saxon-HE

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.ATOMIC_SEQUENCE);

        register("sort", 3, Sort_3.class, AnyItemType.getInstance(),
                 STAR, HOF, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null);


    }

}

// Copyright (c) 2018 Saxonica Limited
