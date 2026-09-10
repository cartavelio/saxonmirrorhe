package com.saxonica.functions.registry;

import com.saxonica.functions.qt4.pin.LabelFn;
import com.saxonica.functions.qt4.*;
import com.saxonica.functions.qt4.csv.CsvToArrays;
import com.saxonica.functions.qt4.csv.CsvToXml;
import com.saxonica.functions.qt4.csv.ParseCsv;
import com.saxonica.functions.qt4.ixml.InvisibleXmlFn;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.functions.Error;
import net.sf.saxon.functions.*;
import net.sf.saxon.functions.hof.*;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.ma.Parcel;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.json.JsonDoc;
import net.sf.saxon.ma.json.JsonToXMLFn;
import net.sf.saxon.ma.json.ParseJsonFn;
import net.sf.saxon.ma.json.XMLToJsonFn;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.DocumentNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.PortableNameTest;
import net.sf.saxon.transpile.CSharp;
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

        SpecificFunctionType forEachArg = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                SequenceType.ANY_SEQUENCE).withAcceptReducedArity();

        SpecificFunctionType predicate2 = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                SequenceType.OPTIONAL_BOOLEAN).withAcceptReducedArity();

        SpecificFunctionType compareFnType = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.SINGLE_ITEM,
                SequenceType.OPTIONAL_BOOLEAN);

        SpecificFunctionType ft = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM,
                SequenceType.ATOMIC_SEQUENCE);

        SpecificFunctionType navigator = new SpecificFunctionType(
                SequenceType.SINGLE_NODE,
                SequenceType.NODE_SEQUENCE);

        SpecificFunctionType replaceAction = new SpecificFunctionType(
                BuiltInAtomicType.UNTYPED_ATOMIC.one(), BuiltInAtomicType.UNTYPED_ATOMIC.zeroOrMore(),
                SequenceType.OPTIONAL_ITEM);

        SpecificFunctionType forEachPairArg = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                SequenceType.ANY_SEQUENCE); //.withAcceptReducedArity();

        ChoiceItemType chronoType = ChoiceItemType.of(
                BuiltInAtomicType.DATE_TIME,
                BuiltInAtomicType.DATE,
                BuiltInAtomicType.TIME,
                BuiltInAtomicType.G_YEAR,
                BuiltInAtomicType.G_YEAR_MONTH,
                BuiltInAtomicType.G_MONTH,
                BuiltInAtomicType.G_MONTH_DAY,
                BuiltInAtomicType.G_DAY);


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

        ItemType schemaTypeRecord = SchemaTypeFn.schemaTypeRecord;

        register("atomic-type-annotation", 1, e -> e.populate(AtomicTypeAnnotationFn::new, BuiltInAtomicType.QNAME, ONE, 0)
                         .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        //#if EE==true || PE==true
        register("build-uri", 1, 2, e -> e.populate(BuildURI::new, BuiltInAtomicType.STRING, ONE, LATE | NEW)
                .arg(0, BuildURI.uriStructureRecord, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null));
        //#endif

        register("char", 1, e -> e.populate(CharFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, LocalUnionType.of(BuiltInAtomicType.STRING, BuiltInAtomicType.INTEGER), ONE, null));
        // TODO: use POSITIVE_INTEGER (needs enhancement to implementation of coercion rules)

        register("characters", 1, e -> e.populate(CharactersFn::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("civil-timezone", 1, 2, e -> e.populate(CivilTimezoneFn::new, BuiltInAtomicType.DAY_TIME_DURATION, ONE, 0)
                .arg(0, BuiltInAtomicType.DATE_TIME, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("codepoints-to-string", 1, e -> e.populate(CodepointsToString::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null));

        register("collation", 1, e -> e.populate(CollationFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, null));

        register("collation-available", 1, 2, e -> e.populate(CollationAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, EnumerationUnionType.of("equality", "sort", "substring"), STAR, null));

        register("collation-key", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BASE64_BINARY,
                                                     ONE, DCOLL)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));


        register("compare", 2, e -> e.populate(Compare::new, BuiltInAtomicType.INTEGER, OPT, DCOLL)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, OPT, EMPTY));

        register("compare", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.INTEGER, OPT, BASE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, OPT, EMPTY)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        // Concat changes in 4.0:
        register("concat", 1, e -> e.populate(Concat::new, BuiltInAtomicType.STRING, ONE, SEQV)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, null));

        register("contains", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("contains-subsequence", 2, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("contains-subsequence", 3, e -> e.populate(SequenceMatching.ContainsSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, OPT, null));

        register("contains-token", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, STAR, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        //#if EE==true || PE==true
        register("csv-to-arrays", 1, e -> e.populate(CsvToArrays::new, new ArrayItemType(SequenceType.SINGLE_STRING), STAR, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .setOptionDetails(CsvToArrays.OPTION_DETAILS));

        register("csv-to-arrays", 2, e -> e.populate(CsvToArrays::new, new ArrayItemType(SequenceType.SINGLE_STRING), STAR, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(CsvToArrays.OPTION_DETAILS));
        
        ItemType csvToXmlResultType = new PortableNameTest(Type.ELEMENT, NamespaceUri.FN, "csv");
        register("csv-to-xml", 1, e -> e.populate(CsvToXml::new, csvToXmlResultType, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .setOptionDetails(CsvToXml.OPTION_DETAILS));

        register("csv-to-xml", 2, e -> e.populate(CsvToXml::new, csvToXmlResultType, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(CsvToXml.OPTION_DETAILS));

        register("day-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.DayFromDateTime(),
                                                           BuiltInAtomicType.INTEGER, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

        register("deep-equal", 2, 3, e -> e.populate(DeepEqual.makeDeepEqual(40), BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, Type.ITEM_TYPE, STAR | ABS, null)
                .arg(1, Type.ITEM_TYPE, STAR | ABS, null)
                .arg(2, ChoiceItemType.of(BuiltInAtomicType.STRING, MapType.ANY_MAP_TYPE), OPT | ABS, null));

        register("decode-from-uri", 1, 1, e -> e.populate(DecodeFromURI::new, BuiltInAtomicType.STRING, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));
        //#endif

        SpecificFunctionType valuePredicate = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_INTEGER,
                SequenceType.OPTIONAL_BOOLEAN).withAcceptReducedArity();
        SpecificFunctionType valueAction = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_INTEGER,
                SequenceType.ANY_SEQUENCE).withAcceptReducedArity();

        register("distinct-ordered-nodes", 0, 1, e -> e.populate(DistinctOrderedNodes::new, AnyNodeTest.getInstance(), STAR, UO | SEQV)
                .arg(0, AnyNodeTest.getInstance(), STAR, EMPTY));

        register("distinct-values", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, STAR, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("do-until", 3, e -> e.populate(DoUntilFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, valueAction, ONE, null)
                .arg(2, valuePredicate, ONE, null));

        register("duplicate-values", 1, e -> e.populate(DuplicateValues::new, BuiltInAtomicType.ANY_ATOMIC, STAR, DCOLL | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY));

        register("duplicate-values", 2, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.ANY_ATOMIC, STAR, BASE | UO)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("element-number", 0, 3, e -> e.populate(ElementNumber::new, BuiltInAtomicType.INTEGER, ONE, BASE)
                .arg(0, NodeKindTest.ELEMENT, OPT, null)
                .arg(1, ChoiceItemType.of(NodeKindTest.DOCUMENT, NodeKindTest.ELEMENT), OPT, null)
                .arg(2, predicate2, OPT, null));

        register("ends-with", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("ends-with-subsequence", 2, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("ends-with-subsequence", 3, e -> e.populate(SequenceMatching.EndsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, OPT, null));

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
                .arg(1, predicate2, OPT, null));

        register("expanded-QName", 1, e -> e.populate(ExpandedQNameFn::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null));

        register("for-each", 2, e -> e.populate(ForEachFn::new, AnyItemType.getInstance(),
                                                STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, forEachArg, ONE, null));

        register("for-each-pair", 3, e -> e.populate(ForEachPairFn::new, AnyItemType.getInstance(),
                                                     STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(2, forEachPairArg, ONE, null));


        register("filter", 2, e -> e.populate(FilterFn::new, AnyItemType.getInstance(),
                                              STAR, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate2, ONE, null));

        SpecificFunctionType foldLeftArg = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                SequenceType.ANY_SEQUENCE);

        register("fold-left", 3, e -> e.populate(FoldLeftFn::new, AnyItemType.getInstance(),
                                                 STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, foldLeftArg, ONE, null));

        SpecificFunctionType foldRightArg = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_INTEGER,
                SequenceType.ANY_SEQUENCE);

        register("fold-right", 3, e -> e.populate(FoldRightFn::new, AnyItemType.getInstance(),
                                                  STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, foldRightArg, ONE, null));


        register("foot", 1, e -> e.populate(FootFn::new, AnyItemType.getInstance(), OPT, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        register("format-date", 2, 5, e -> e.populate(FormatDate::new, BuiltInAtomicType.STRING,
                                                   OPT, CARD0)
                .arg(0, BuiltInAtomicType.DATE, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null)
                .arg(4, BuiltInAtomicType.STRING, OPT, null));

        register("format-dateTime", 2, 5, e -> e.populate(FormatDate::new, BuiltInAtomicType.STRING,
                                                       OPT, CARD0)
                .arg(0, BuiltInAtomicType.DATE_TIME, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null)
                .arg(4, BuiltInAtomicType.STRING, OPT, null));

        register("format-number", 2, e -> e.populate(FormatNumber::new, BuiltInAtomicType.STRING, ONE, NS | LATE)
                .arg(0, NumericType.getInstance(), OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("format-number", 3, e -> e.populate(FormatNumber::new, BuiltInAtomicType.STRING, ONE, NS | LATE)
                .arg(0, NumericType.getInstance(), OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), OPT, null));

        register("format-time", 2, 5, e -> e.populate(FormatDate::new, BuiltInAtomicType.STRING,
                                                   OPT, CARD0)
                .arg(0, BuiltInAtomicType.TIME, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null)
                .arg(4, BuiltInAtomicType.STRING, OPT, null));

        register("function-annotations", 1, e -> e.populate(FunctionAnnotations::new,
                                                            new MapType(BuiltInAtomicType.QNAME, SequenceType.ATOMIC_SEQUENCE),
                                                            ONE, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null));

        register("graphemes", 1, e -> e.populate(GraphemesFn::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        LocalUnionType hashInput = LocalUnionType.of(BuiltInAtomicType.STRING,
                                                      BuiltInAtomicType.HEX_BINARY,
                                                      BuiltInAtomicType.BASE64_BINARY);
        register("hash", 1, 3, e -> e.populate(HashFn::new, BuiltInAtomicType.HEX_BINARY, OPT, 0)
                .arg(0, hashInput, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(HashFn.getOptionDetails()));

        register("highest", 1, 3, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, OPT, null));

        register("hours-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.HoursFromDateTime(),
                                                           BuiltInAtomicType.INTEGER, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

        register("identity", 1, e -> e.populate(IdentityFn::new, AnyItemType.getInstance(), STAR, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("index-of", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.INTEGER, STAR, BASE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("index-where", 2, e -> e.populate(IndexWhere::new, BuiltInAtomicType.INTEGER, STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        // map((xs:NCName | enum('')), xs:anyURI)
        ItemType inScopeNamespacesResult = new MapType(
                LocalUnionType.of(BuiltInAtomicType.NCNAME, InScopeNamespacesFn.EMPTY_STRING_TYPE),
                                  BuiltInAtomicType.ANY_URI.one());
        register("in-scope-namespaces", 1, e -> e.populate(InScopeNamespacesFn::new, inScopeNamespacesResult, ONE, LATE)
                .arg(0, NodeKindTest.ELEMENT, ONE, null));

        register("intersperse", 2, e -> e.populate(Intersperse::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null));

        //#if EE==true || PE==true
        //#if CSHARP==false
        register("invisible-xml", 1, 2, e -> e.populate(InvisibleXmlFn::new, InvisibleXmlFn.RETURN_TYPE, OPT, 0)
                .arg(0, ChoiceItemType.of(BuiltInAtomicType.STRING, NodeKindTest.DOCUMENT, NodeKindTest.ELEMENT), OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(InvisibleXmlFn.getOptionDetails()));
        //#endif
        //#endif

        register("is-NaN", 1, e -> e.populate(IsNaN::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        register("items-after", 2, e -> e.populate(ItemsStarting.ItemsAfter::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        register("items-at", 2, e -> e.populate(ItemsAt::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, STAR, EmptySequence.getInstance()));

        register("items-before", 2, e -> e.populate(ItemsEnding.ItemsBefore::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        register("items-ending-where", 2, e -> e.populate(ItemsEnding.ItemsUntil::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        register("items-starting-where", 2, e -> e.populate(ItemsStarting.ItemsFrom::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        register("json-doc", 2, e -> e.populate(JsonDoc::new, AnyItemType.getInstance(), OPT, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(ParseJsonFn.makeOptionsParameter(40)));

        register("json-to-xml", 1, 2, e -> e.populate(JsonToXMLFn::new, NodeKindTest.DOCUMENT, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(JsonToXMLFn.makeOptionsParameter(40)));

        register("label", 1, e -> e.populate(LabelFn::new, MapType.getInstance(), ONE, 0)
                .arg(0, AnyItemType.getInstance(), ONE, null));

        register("load-xquery-module", 2, e -> e.populate(LoadXqueryModule::new, MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null) // null or EMPTY?
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(LoadXqueryModule.makeOptionsParameter(40)));
        
        register("lowest", 1, 3, e -> e.populate(HighestOrLowest::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, OPT, null));

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

        register("minutes-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.MinutesFromDateTime(),
                                                           BuiltInAtomicType.INTEGER, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

        register("month-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.MonthFromDateTime(),
                                                           BuiltInAtomicType.INTEGER, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

        register("node-kind", 0, e -> e.populate(ContextItemAccessorFunction::new, BuiltInAtomicType.STRING, ONE, CITEM | LATE));

        register("node-kind", 1, e -> e.populate(NodeKind_1::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, Type.NODE_TYPE, OPT | INS, EMPTY));

        register("node-type-annotation", 1, e -> e.populate(NodeTypeAnnotationFn::new, BuiltInAtomicType.QNAME, ONE, 0)
                .arg(0, AnyNodeTest.getInstance(), ONE, null));
        
        register("normalize-unicode", 2, e -> e.populate(NormalizeUnicode::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        SpecificFunctionType operatorType = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.ANY_SEQUENCE,
                SequenceType.ANY_SEQUENCE);

        register("op", 1, e -> e.populate(OpFn::new, operatorType, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));


        register("parcel", 1, e -> e.populate(ParcelFn::new, Parcel.TYPE, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        // record(
        //columns?	as xs:string*,
        //columns-index?	as map(xs:string,xs:integer)?,
        //rows?	as array(xs:string)*,
        //get?	as function(xs:positiveInteger, (xs:positiveInteger | xs:string)) as xs:string?
        //)
        ItemType parseCsvResultType = RecordTest.nonExtensible(
                new RecordTest.Field("columns", SequenceType.STRING_SEQUENCE, true),
                new RecordTest.Field("columns-index", SequenceType.optional(
                        new MapType(BuiltInAtomicType.STRING, SequenceType.SINGLE_INTEGER)), true),
                new RecordTest.Field("rows", SequenceType.zeroOrMore(new ArrayItemType(SequenceType.SINGLE_STRING)), true),
                new RecordTest.Field("get", SequenceType.one(
                        new SpecificFunctionType(
                                BuiltInAtomicType.POSITIVE_INTEGER.one(),
                                SequenceType.one(
                                        LocalUnionType.of(BuiltInAtomicType.POSITIVE_INTEGER, BuiltInAtomicType.STRING)),
                                SequenceType.OPTIONAL_STRING)), true));

        register("parse-csv", 1, e -> e.populate(com.saxonica.functions.qt4.csv.ParseCsv::new, parseCsvResultType, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .setOptionDetails(ParseCsv.OPTION_DETAILS));

        register("parse-csv", 2, e -> e.populate(com.saxonica.functions.qt4.csv.ParseCsv::new, MapType.ANY_MAP_TYPE, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(ParseCsv.OPTION_DETAILS));

        //#if EE==true || PE==true
        register("parse-html", 1, e -> e.populate(ParseHtml::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        //#endif


        register("parse-integer", 1, 2, e -> e.populate(ParseInteger::new, BuiltInAtomicType.INTEGER, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null));

        register("parse-json", 2, e -> e.populate(ParseJsonFn::new, AnyItemType.getInstance(), OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(ParseJsonFn.makeOptionsParameter(40)));

        //#if EE==true || PE==true
        register("parse-uri", 1, 2, e -> e.populate(ParseURI::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null));
        //#endif

        register("parse-xml", 1, 2, e -> e.populate(ParseXml::new, new DocumentNodeTest(NodeKindTest.ELEMENT), OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(ParseXml.makeOptionsParameter(40)));

        register("random-number-generator", 0, e -> e.populate(RandomNumberGenerator::new, RandomNumberGenerator.RETURN_TYPE, ONE, LATE));

        register("random-number-generator", 1, e -> e.populate(RandomNumberGenerator::new, RandomNumberGenerator.RETURN_TYPE, ONE, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, OPT, null));

        register("parse-xml-fragment", 1, 2, e -> e.populate(ParseXmlFragment::new, NodeKindTest.DOCUMENT, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .setOptionDetails(ParseXmlFragment.makeOptionsParameter(40)));

        ItemType partitionFunctionType = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                SequenceType.OPTIONAL_BOOLEAN);

        register("partition", 2, e -> e.populate(Partition::new, new ArrayItemType(SequenceType.SINGLE_ITEM), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, partitionFunctionType, ONE | INS, null));

        register("parts", 1, e -> e.populate(Parts::new, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        register("parse-QName", 1, e -> e.populate(ParseQNameFn::new, BuiltInAtomicType.QNAME, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("pin", 1, e -> e.populate(com.saxonica.functions.extfn.PinFn::new, AnyItemType.getInstance(), ONE, 0)
                .arg(0, AnyItemType.getInstance(), ONE, null));

        // remove() changes in 4.0 to allow multiple integers in the second argument
        register("remove", 2, e -> e.populate(Remove::new, Type.ITEM_TYPE, STAR, AS_ARG0 | FILTER)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, EMPTY)
                .arg(1, BuiltInAtomicType.INTEGER, STAR, null));


        register("replace", 2, 5, e -> e.populate(CSharp.staticRef(Replace::make40), BuiltInAtomicType.STRING, ONE, 0)
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

        register("round", 1, 3, e -> e.populate(Round::new, NumericType.getInstance(), OPT, AS_PRIM_ARG0)
                .arg(0, NumericType.getInstance(), OPT, EMPTY)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null)
                .arg(2, EnumerationUnionType.of("toward-zero", "away-from-zero", "floor", "ceiling",
                                                "half-toward-zero", "half-away-from-zero", "half-to-floor", "half-to-ceiling",
                                                "half-to-even"), OPT, null));

        register("round-half-to-even", 2, e -> e.populate(RoundHalfToEven::new, NumericType.getInstance(), OPT, AS_PRIM_ARG0)
                .arg(0, NumericType.getInstance(), OPT, EMPTY)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null));



        register("schema-type", 1, e -> e.populate(SchemaTypeFn::new, schemaTypeRecord, OPT, 0)
                .arg(0, BuiltInAtomicType.QNAME, ONE, null));

        register("seconds", 1, e -> e.populate(SecondsFn::new, BuiltInAtomicType.DAY_TIME_DURATION, OPT, 0)
                .arg(0, BuiltInAtomicType.DECIMAL, OPT, null));

        register("seconds-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.SecondsFromDateTime(),
                                                           BuiltInAtomicType.DECIMAL, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

        register("slice", 1, 4, e -> e.populate(Slice::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null)
                .arg(2, BuiltInAtomicType.INTEGER, OPT, null)
                .arg(3, BuiltInAtomicType.INTEGER, OPT, null));

        register("some", 1, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("some", 2, e -> e.populate(SomeFn::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(1, predicate2, OPT, null));

        SpecificFunctionType comparator = new SpecificFunctionType(
                SequenceType.SINGLE_ITEM, SequenceType.SINGLE_ITEM,
                SequenceType.SINGLE_INTEGER);

        register("sort", 4, e -> e.populate(Sort_3::new, AnyItemType.getInstance(),
                                            STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null)
                .arg(3, AnyItemType.getInstance(), STAR, null)); // TODO: not yet implemented

        register("sort-with", 2, e -> e.populate(SortWith::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, comparator, STAR, null));

        register("starts-with", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, BooleanValue.TRUE)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("starts-with-subsequence", 2, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE));

        register("starts-with-subsequence", 3, e -> e.populate(SequenceMatching.StartsWithSequence::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, BooleanValue.FALSE)
                .arg(2, compareFnType, OPT, null));

        register("string-join", 2, e -> e.populate(StringJoin::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("subsequence", 3, e -> e.populate(Subsequence_3::new, Type.ITEM_TYPE, STAR, AS_ARG0 | FILTER)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, EMPTY)
                .arg(1, NumericType.getInstance(), ONE, null)
                .arg(2, NumericType.getInstance(), OPT, null));

        register("subsequence-where", 1, 3, e -> e.populate(SubsequenceWhere::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, OPT, null)
                .arg(2, predicate2, OPT, null));

        register("substring-after", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.STRING, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("substring-before", 3, e -> e.populate(CollatingFunctionFree::new, BuiltInAtomicType.STRING, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("take-while", 1, 2, e -> e.populate(TakeWhile::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate2, ONE, null));

        register("timezone-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.TimezoneFromDateTime(),
                                                           BuiltInAtomicType.DAY_TIME_DURATION, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));

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

        register("transitive-closure", 2, e -> e.populate(TransitiveClosure::new, AnyNodeTest.getInstance(), STAR, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EmptySequence.getInstance())
                .arg(1, navigator, ONE, null));

        register("trunk", 1, e -> e.populate(Trunk::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        register("unix-dateTime", 0, 1, e -> e.populate(UnixDateTime::new, BuiltInAtomicType.DATE_TIME_STAMP, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, OPT, EmptySequence.getInstance()));

        register("unparcel", 1, e -> e.populate(UnparcelFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, Parcel.TYPE, ONE, null));

        ItemType mapOrString = ChoiceItemType.of(BuiltInAtomicType.STRING, MapType.ANY_MAP_TYPE);

        register("unparsed-text", 2, e -> e.populate(UnparsedText::new, BuiltInAtomicType.STRING, OPT, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, mapOrString, OPT, null));

        register("unparsed-text-available", 2, e -> e.populate(UnparsedTextAvailable::new,
                                                               BuiltInAtomicType.BOOLEAN, ONE, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, BooleanValue.FALSE)
                .arg(1, mapOrString, OPT, null));

        register("unparsed-text-lines", 2, e -> e.populate(UnparsedTextLines::new, BuiltInAtomicType.STRING, STAR, BASE | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, mapOrString, OPT, null));

        register("void", 0, 1, e -> e.populate(VoidFn::new, ErrorType.getInstance(), StaticProperty.EMPTY, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("while-do", 3, e -> e.populate(WhileDoFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, valuePredicate, ONE, null)
                .arg(2, valueAction, ONE, null));

        register("xdm-to-json", 1, e -> e.populate(XdmToJsonFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("xdm-to-json", 2, e -> e.populate(XdmToJsonFn::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null)
                .setOptionDetails(XdmToJsonFn.OPTION_DETAILS));

        register("xml-to-json", 2, e -> e.populate(XMLToJsonFn::new, BuiltInAtomicType.STRING,
                                                   OPT, LATE)
                .arg(0, AnyNodeTest.getInstance(), OPT | ABS, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, OPT | ABS, null)
                .setOptionDetails(XMLToJsonFn.makeOptionsParameter(40)));

        register("year-from-dateTime", 1, e -> e.populate(() -> new AccessorFn.YearFromDateTime(),
                                                           BuiltInAtomicType.INTEGER, OPT, CARD0)
                .arg(0, chronoType, OPT, EMPTY));


    }

}

// Copyright (c) 2020-2023 Saxonica Limited.
