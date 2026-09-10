package com.saxonica.functions.registry;

import com.saxonica.functions.qt4.*;
import net.sf.saxon.functions.*;
import net.sf.saxon.functions.Error;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.ma.Parcel;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.type.*;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

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

        SpecificFunctionType navigator = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_NODE},
                SequenceType.NODE_SEQUENCE);

        SpecificFunctionType replaceAction = new SpecificFunctionType(
                new SequenceType[]{BuiltInAtomicType.UNTYPED_ATOMIC.one(), BuiltInAtomicType.UNTYPED_ATOMIC.zeroOrMore()},
                SequenceType.OPTIONAL_ITEM);

        // TODO: superseded by "every"
        register("all", 1, e -> e.populate(EveryFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE));

        // TODO: superseded by "every"
        register("all", 2, e -> e.populate(EveryFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE)
                .arg(1, predicate, ONE, null));

        register("all-equal", 1, e -> e.populate(AllEqual::new, BuiltInAtomicType.BOOLEAN, ONE, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("all-equal", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("all-different", 1, e -> e.populate(AllDifferent::new, BuiltInAtomicType.BOOLEAN, ONE, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("all-different", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("atomic-equal", 2, e -> e.populate(AtomicEqual::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        //#if EE==true || PE==true
        register("build-uri", 1, 2, e -> e.populate(BuildURI::new, BuiltInAtomicType.STRING, ONE, LATE | NEW)
                .arg(0, BuildURI.uriStructureRecord, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null));
        //#endif

        register("char", 1, e -> e.populate(CharFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("characters", 1, e -> e.populate(CharactersFn::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        // codepoints-to-string becomes variadic in 4.0:
        register("codepoints-to-string", 1, e -> e.populate(CodepointsToString::new, BuiltInAtomicType.STRING, ONE, SEQV)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null));

        register("collation-key", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BASE64_BINARY,
                                                     ONE, DCOLL)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("compare", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.INTEGER, OPT, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        // Concat changes in 4.0:
        register("concat", 1, e -> e.populate(Concat::new, BuiltInAtomicType.STRING, ONE, SEQV)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, null));

        register("contains", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("contains-sequence", 2, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("contains-sequence", 3, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));

        register("contains-token", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, STAR, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        //#if EE==true || PE==true
        register("decode-from-uri", 1, 1, e -> e.populate(DecodeFromURI::new, BuiltInAtomicType.STRING, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));
        //#endif

        register("distinct-values", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, STAR, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("duplicate-values", 1, e -> e.populate(DuplicateValues::new, BuiltInAtomicType.ANY_ATOMIC, STAR, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("duplicate-values", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, STAR, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("ends-with", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("ends-with-sequence", 2, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("ends-with-sequence", 3, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));

        register("error", 2, e -> e.populate(Error::new, Type.ITEM_TYPE, OPT, LATE)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("error", 3, e -> e.populate(Error::new, Type.ITEM_TYPE, OPT, LATE)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, Type.ITEM_TYPE, STAR, null));

        register("every", 1, e -> e.populate(EveryFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE));

        register("every", 2, e -> e.populate(EveryFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.TRUE)
                .arg(1, predicate, ONE, null));

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

        register("index-of", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.INTEGER, STAR, BASE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

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

        register("matches", 3, e -> e.populate(Matches::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("max", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, OPT, BASE | UO | CARD0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("min", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, OPT, BASE | UO | CARD0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        SpecificFunctionType operatorType = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE, SequenceType.ANY_SEQUENCE},
                SequenceType.ANY_SEQUENCE);

        register("op", 1, e -> e.populate(OpFn::new, operatorType, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));


        register("parcel", 1, e -> e.populate(ParcelFn::new, Parcel.TYPE, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        //#if EE==true || PE==true
        register("parse-html", 1, e -> e.populate(ParseHtml::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        //#endif

        register("parse-integer", 1, e -> e.populate(ParseInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("parse-integer", 2, e -> e.populate(ParseInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null));

        //#if EE==true || PE==true
        register("parse-uri", 1, 2, e -> e.populate(ParseURI::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null));
        //#endif

        ItemType partitionFunctionType = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);

        register("partition", 2, e -> e.populate(Partition::new, new ArrayItemType(SequenceType.SINGLE_ITEM), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, partitionFunctionType, ONE | INS, null));

        register("parts", 1, e -> e.populate(Parts::new, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        register("parse-QName", 1, e -> e.populate(ParseQNameFn::new, BuiltInAtomicType.QNAME, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        // remove() changes in 4.0 to allow multiple integers in the second argument
        register("remove", 2, e -> e.populate(Remove::new, Type.ITEM_TYPE, STAR, AS_ARG0 | FILTER)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, EMPTY)
                .arg(1, BuiltInAtomicType.INTEGER, STAR, null));


        register("replace", 2, 5, e -> e.populate(Replace::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null)
                .arg(4, replaceAction, OPT, null));

        register("replicate", 2, e -> e.populate(Replicate::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.NON_NEGATIVE_INTEGER, ONE, null));

        register("resolve-uri", 2, e -> e.populate(ResolveURI::new, BuiltInAtomicType.ANY_URI, OPT, CARD0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("slice", 1, 4, e -> e.populate(Slice::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null)
                .arg(2, BuiltInAtomicType.INTEGER, OPT, null)
                .arg(3, BuiltInAtomicType.INTEGER, OPT, null));

        register("some", 1, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("some", 2, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(1, predicate, ONE, null));

        register("starts-with", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("starts-with-sequence", 2, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("starts-with-sequence", 3, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, ONE, null));

        register("subsequence", 3, e -> e.populate(Subsequence_3::new, Type.ITEM_TYPE, STAR, AS_ARG0 | FILTER)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, EMPTY)
                .arg(1, NumericType.getInstance(), ONE, null)
                .arg(2, NumericType.getInstance(), OPT, null));

        register("substring-after", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.STRING, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("substring-before", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.STRING, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("tokenize", 2, e -> e.populate(RegexFunctionSansFlags::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("tokenize", 3, e -> e.populate(Tokenize_3::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("trace", 2, e -> e.populate(Trace::new, Type.ITEM_TYPE, STAR, AS_ARG0 | LATE)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("transitive-closure", 1, 3, e -> e.populate(TransitiveClosure::new, TransitiveClosure.resultType, ONE, 0)
                .arg(0, navigator, ONE, null)
                .arg(1, BuiltInAtomicType.NON_NEGATIVE_INTEGER, OPT, null)
                .arg(2, BuiltInAtomicType.POSITIVE_INTEGER, OPT, null));

        register("trunk", 1, e -> e.populate(Trunk::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        register("unparcel", 1, e -> e.populate(UnparcelFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, Parcel.TYPE, ONE, null));

        register("unparsed-text", 2, e -> e.populate(UnparsedText::new, BuiltInAtomicType.STRING, OPT, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("unparsed-text-available", 2, e -> e.populate(UnparsedTextAvailable::new,
                                                               BuiltInAtomicType.BOOLEAN, ONE, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, BooleanValue.FALSE)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("unparsed-text-lines", 2, e -> e.populate(UnparsedTextLines::new, BuiltInAtomicType.STRING, STAR, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        // Result type of void() is deliberately mis-declared to prevent unwanted optimisations
        register("void", 1, e -> e.populate(VoidFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("xdm-to-json", 1, e -> e.populate(XdmToJsonFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("xdm-to-json", 2, e -> e.populate(XdmToJsonFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null)
                .setOptionDetails(XdmToJsonFn.OPTION_DETAILS));


    }

}

// Copyright (c) 2020-2023 Saxonica Limited.
