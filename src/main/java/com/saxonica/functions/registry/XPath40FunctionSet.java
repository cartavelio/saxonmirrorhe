package com.saxonica.functions.registry;

import com.saxonica.functions.qt4.*;
import net.sf.saxon.functions.AtomicEqual;
import net.sf.saxon.functions.CodepointsToString;
import net.sf.saxon.functions.CollatingFunctionFree;
import net.sf.saxon.functions.Concat;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.ma.Parcel;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceType;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XPath 4.0
 */

public class XPath40FunctionSet extends BuiltInFunctionSet {

    private static final XPath40FunctionSet THE_INSTANCE = new XPath40FunctionSet();

    public static XPath40FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XPath40FunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath31FunctionSet.getInstance());

        SpecificFunctionType predicate = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);

        SpecificFunctionType compareFnType = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM, SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);

        SpecificFunctionType ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.ATOMIC_SEQUENCE);

        register("all", 1, e -> e.populate(AllFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE));

        register("all", 2, e -> e.populate(AllFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE)
                .arg(1, predicate, ONE, null));

        register("all-equal", 1, e -> e.populate(AllEqual::new, BuiltInAtomicType.BOOLEAN, ONE, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("all-equal", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("all-different", 1, e -> e.populate(AllDifferent::new, BuiltInAtomicType.BOOLEAN, ONE, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("all-different", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("atomic-equal", 2, e -> e.populate(AtomicEqual::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        //#if EE==true || PE==true
        register("build-uri", 1, 2, e -> e.populate(BuildURI::new, BuiltInAtomicType.STRING, ONE, LATE | NEW)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null));
        //#endif

        register("char", 1, e -> e.populate(CharFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("characters", 1, e -> e.populate(CharactersFn::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        // codepoints-to-string becomes variadic in 4.0:
        register("codepoints-to-string", 1, e -> e.populate(CodepointsToString::new, BuiltInAtomicType.STRING, ONE, SEQV)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null));

        // Concat changes in 4.0:
        register("concat", 1, e -> e.populate(Concat::new, BuiltInAtomicType.STRING, ONE, SEQV)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, null));

        register("contains-sequence", 2, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("contains-sequence", 3, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));


        register("ends-with-sequence", 2, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("ends-with-sequence", 3, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));


        register("expanded-QName", 1, e -> e.populate(ExpandedQNameFn::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null));

        register("foot", 1, e -> e.populate(FootFn::new, AnyItemType.getInstance(), OPT, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        register("highest", 1, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("highest", 2, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("highest", 3, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null));

        register("identity", 1, e -> e.populate(IdentityFn::new, AnyItemType.getInstance(), STAR, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("index-where", 2, e -> e.populate(IndexWhere::new, BuiltInAtomicType.INTEGER, STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null));

        register("in-scope-namespaces", 1, e -> e.populate(InScopeNamespacesFn::new, MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, NodeKindTest.ELEMENT, STAR, null));

        register("intersperse", 2, e -> e.populate(Intersperse::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null));

        register("is-NaN", 1, e -> e.populate(IsNaN::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        register("items-after", 2, e -> e.populate(ItemsStarting.ItemsAfter::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null));

        register("items-at", 2, e -> e.populate(ItemsAt::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, STAR, EmptySequence.getInstance()));

        register("items-before", 2, e -> e.populate(ItemsEnding.ItemsBefore::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null));

        register("items-ending-where", 2, e -> e.populate(ItemsEnding.ItemsUntil::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null));

        register("items-starting-where", 2, e -> e.populate(ItemsStarting.ItemsFrom::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null));

        SpecificFunctionType valuePredicate = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE},
                SequenceType.SINGLE_BOOLEAN);
        SpecificFunctionType valueAction = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE},
                SequenceType.ANY_SEQUENCE);

        register("iterate-while", 3, e -> e.populate(IterateWhileFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, valuePredicate, ONE, null)
                .arg(2, valueAction, ONE, null));

        register("lowest", 1, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("lowest", 2, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("lowest", 3, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null));


        SpecificFunctionType operatorType = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE, SequenceType.ANY_SEQUENCE},
                SequenceType.ANY_SEQUENCE);

        register("op", 1, e -> e.populate(OpFn::new, operatorType, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, STAR, null));


        register("parcel", 1, e -> e.populate(ParcelFn::new, Parcel.TYPE, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        //#if EE==true || PE==true
        register("parse-html", 1, e -> e.populate(ParseHtml::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        //#endif

        //#if EE==true || PE==true
        register("parse-uri", 1, 2, e -> e.populate(ParseURI::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null));
        //#endif

        register("parts", 1, e -> e.populate(Parts::new, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

//        register("range", 2, e -> e.populate( SefFunction::new, AnyItemType.getInstance(), STAR, LATE)
//                .arg(0, AnyItemType.getInstance(), STAR, null)
//                .arg(1, predicate, OPT, null));
//
//        register("range", 3, e -> e.populate( SefFunction::new, AnyItemType.getInstance(), STAR, LATE)
//                .arg(0, AnyItemType.getInstance(), STAR, null)
//                .arg(1, predicate, OPT, null)
//                .arg(2, predicate, OPT, null));
//
//        register("range", 4, e -> e.populate( SefFunction::new, AnyItemType.getInstance(), STAR, LATE)
//                .arg(0, AnyItemType.getInstance(), STAR, null)
//                .arg(1, predicate, OPT, null)
//                .arg(2, predicate, OPT, null)
//                .arg(3, MapType.ANY_MAP_TYPE,ONE, null));

        register("parse-QName", 1, e -> e.populate(ParseQNameFn::new, BuiltInAtomicType.QNAME, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("replicate", 2, e -> e.populate(Replicate::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null));

        register("some", 1, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("some", 2, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(1, predicate, ONE, null));

        register("starts-with-sequence", 2, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("starts-with-sequence", 3, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));

        register("trunk", 1, e -> e.populate(Trunk::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        register("unparcel", 1, e -> e.populate(UnparcelFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, Parcel.TYPE, ONE, null));

    }

}

// Copyright (c) 2020-2023 Saxonica Limited.
