package com.saxonica.functions.extfn;

import com.saxonica.functions.qt4.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.number.NamedTimeZone;
import net.sf.saxon.functions.ContextItemAccessorFunction;
import net.sf.saxon.functions.Doc_2;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.VendorFunctionSetHE;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.StandardDiagnostics;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.ma.Parcel;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.serialize.charcode.UTF8CharacterSet;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.tiny.TinyAttributeImpl;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.tree.util.ProcInstParser;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntPredicateProxy;

import java.io.*;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeFormatterBuilder;
import java.time.format.DateTimeParseException;
import java.time.format.ResolverStyle;
import java.time.temporal.ChronoField;
import java.time.temporal.TemporalAccessor;
import java.util.*;

/**
 * The VendorFunctionLibraryPE represents additions to the set of Saxon vendor-supplied
 * extension functions that are available only with the Saxon-PE and Saxon-EE editions
 */

public class VendorFunctionSetPE extends BuiltInFunctionSet {

    private final static VendorFunctionSetPE INSTANCE = new VendorFunctionSetPE();

    public static VendorFunctionSetPE getInstance() {
        return INSTANCE;
    }


    public VendorFunctionSetPE() {
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


        register("adjust-to-civil-time", 2, e -> e.populate( AdjustToCivilTime::new, BuiltInAtomicType.DATE_TIME, OPT, LATE)
                .arg(0, BuiltInAtomicType.DATE_TIME, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("analyze-uri", 1, e -> e.populate( AnalyzeUri::new, analyzeUriResult, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("array-member", 1, e -> e.populate(ParcelFn::new, Parcel.TYPE, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("base64Binary-to-octets", 1, e -> e.populate( Base64BinaryToOctets::new, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null));

        register("base64Binary-to-string", 2, e -> e.populate( Base64BinaryToString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("characters", 1, e -> e.populate(CharactersFn::new, BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("column-number", 0, e -> e.populate(ContextItemAccessorFunction::new, BuiltInAtomicType.INTEGER, ONE, CITEM | LATE));

        register("column-number", 1, e -> e.populate( ColumnNumber::new, BuiltInAtomicType.INTEGER, OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY));

        register("compile-query", 1, e -> e.populate( CompileQuery::new, JavaExternalObjectType.of(XQueryExpression.class), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("compile-stylesheet", 1, e -> e.populate( CompileStylesheet::new, JavaExternalObjectType.of(PreparedStylesheet.class), ONE, 0)
                .arg(0, NodeKindTest.DOCUMENT, ONE, null));

        register("configuration", 0, e -> e.populate(ConfigurationFn::new, JavaExternalObjectType.of(Configuration.class), ONE, LATE));

        register("context", 0, e -> e.populate(ContextFn::new, JavaExternalObjectType.of(XPathContext.class), ONE, CITEM | LATE));

        register("controller", 0, e -> e.populate(ControllerFn::new, JavaExternalObjectType.of(Controller.class), ONE, LATE));

        register("current-mode-name", 0, e -> e.populate(CurrentModeName::new, BuiltInAtomicType.QNAME, OPT, CITEM | LATE));

        register("deep-equal", 4, e -> e.populate( SaxonDeepEqual::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null));

        register("decimal-divide", 3, e -> e.populate( DecimalDivide::new, BuiltInAtomicType.DECIMAL, OPT, 0)
                .arg(0, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.DECIMAL, OPT, EMPTY)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null));

        register("discard-document", 1, e -> e.populate( DiscardDocument::new, NodeKindTest.DOCUMENT, OPT, 0)
                .arg(0, NodeKindTest.DOCUMENT, OPT, null));

        register("EQName", 1, e -> e.populate( EQName::new, BuiltInAtomicType.QNAME, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY));

        register("escape-NCName", 1, e -> e.populate( EscapeNCName::new, BuiltInAtomicType.NCNAME, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY));

//        registerVariadic("eval", 2, e -> e.populate(Eval::new, AnyItemType.getInstance(), STAR, SEQV | CITEM)
//                .arg(0, JavaExternalObjectType.of(Evaluate.PreparedExpression.class), ONE, null)
//                .arg(1, AnyItemType.getInstance(), STAR, null));
//
//        registerVariadic("evaluate", 2, e -> e.populate(Evaluate::new, AnyItemType.getInstance(), STAR, SEQV | CITEM)
//                .arg(0, BuiltInAtomicType.STRING, ONE, null)
//                .arg(1, AnyItemType.getInstance(), STAR, null));
//
//        register("expression", 1, e -> e.populate( ExpressionFn::new, JavaExternalObjectType.of(Evaluate.PreparedExpression.class), ONE, 0)
//                .arg(0, BuiltInAtomicType.STRING, ONE, null));
//
//        register("expression", 2, e -> e.populate( ExpressionFn::new, JavaExternalObjectType.of(Evaluate.PreparedExpression.class), ONE, 0)
//                .arg(0, BuiltInAtomicType.STRING, ONE, null)
//                .arg(1, NodeKindTest.ELEMENT, ONE, null));

        register("function-annotations", 1, e -> e.populate( FunctionAnnotations::new, FunctionAnnotations.RETURN_TYPE, STAR, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null));

        register("get-context", 0, e -> e.populate(GetContext::new, JavaExternalObjectType.of(XPathContext.class), ONE, CITEM | LATE));

        register("get-pseudo-attribute", 1, e -> e.populate( GetPseudoAttribute::new, BuiltInAtomicType.STRING, OPT, CITEM | LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY));


        register("group-starting", 2, e -> e.populate( GroupStarting::new, ArrayItemType.ANY_ARRAY_TYPE , STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        register("has-same-nodes", 2, e -> e.populate( HasSameNodes::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyNodeTest.getInstance(), STAR, null)
                .arg(1, AnyNodeTest.getInstance(), STAR, null));

        register("hexBinary-to-octets", 1, e -> e.populate( HexBinaryToOctets::new, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, BuiltInAtomicType.HEX_BINARY, ONE, null));

        register("hexBinary-to-string", 2, e -> e.populate( HexBinaryToString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.HEX_BINARY, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("highest", 1, e -> e.populate( HighestOrLowest::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY));

        register("highest", 2, e -> e.populate( HighestOrLowest::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null));

        register("index", 2, e -> e.populate( Index::new, MapType.getInstance(), ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, EMPTY));

        register("index-where", 2, e -> e.populate(IndexWhere::new, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        register("in-scope-namespaces", 1, e -> e.populate(InScopeNamespacesFn::new, MapType.getInstance(), ONE, 0)
                .arg(0, NodeKindTest.ELEMENT, ONE | INS, null));

        register("in-summer-time", 2, e -> e.populate( InSummerTime::new, BuiltInAtomicType.DATE_TIME, OPT, LATE)
                .arg(0, BuiltInAtomicType.DATE_TIME, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("is-defaulted", 1, e -> e.populate( IsDefaulted::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, AnyNodeTest.getInstance(), ONE, null));

        register("is-NaN", 1, e -> e.populate(IsNaN::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null));

        register("items-after", 2, e -> e.populate(ItemsStarting.ItemsAfter::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        register("items-from", 2, e -> e.populate(ItemsStarting.ItemsFrom::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        register("items-before", 2, e -> e.populate(ItemsEnding.ItemsBefore::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        register("items-until", 2, e -> e.populate(ItemsEnding.ItemsUntil::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, EMPTY)
                .arg(1, predicate, ONE, EMPTY));

        //#if CSHARP==false
        register("key-map", 4, e -> e.populate( KeyMap::new, new MapType(BuiltInAtomicType.STRING, SequenceType.NODE_SEQUENCE), ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, BuiltInAtomicType.STRING, OPT, null)
                .arg(3, BuiltInAtomicType.STRING, OPT, null));
        //#endif
        //#if CSHARP==false
        register("last-modified", 1, e -> e.populate( LastModified::new, BuiltInAtomicType.DATE_TIME, OPT, BASE)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY));
        //#endif

        register("leading", 1, e -> e.populate( Leading::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY));

        register("leading", 2, e -> e.populate( Leading::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, null));

        register("line-number", 0, e -> e.populate(ContextItemAccessorFunction::new, BuiltInAtomicType.INTEGER, ONE, CITEM | LATE));

        register("line-number", 1, e -> e.populate( LineNumber::new, BuiltInAtomicType.INTEGER, OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY));

        register("lowest", 1, e -> e.populate( HighestOrLowest::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY));

        register("lowest", 2, e -> e.populate( HighestOrLowest::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, orderKey, ONE, null));

        FunctionItemType tester = new SpecificFunctionType(
                new SequenceType[]{SequenceType.ANY_SEQUENCE},
                SequenceType.SINGLE_BOOLEAN);

        register("map-search", 2, e -> e.populate( MapSearch::new, MapType.ANY_MAP_TYPE, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null));

        register("map-search", 3, e -> e.populate( MapSearch::new, MapType.ANY_MAP_TYPE, STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                .arg(2, tester, ONE | INS, null));

        register("message-count", 1, e -> e.populate( MessageCount::new, BuiltInAtomicType.INTEGER, ONE, LATE|SIDE)
                .arg(0, BuiltInAtomicType.QNAME, OPT, null));

        register("namespace-node", 2, e -> e.populate( NamespaceNodeFn::new, NodeKindTest.NAMESPACE, ONE, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        LocalUnionType stringOrQName = new LocalUnionType(BuiltInAtomicType.STRING, BuiltInAtomicType.QNAME);

        register("new-attribute", 2, e -> e.populate( NewAttribute::new, NodeKindTest.ATTRIBUTE, ONE, LATE | SIDE | NEW)
                .arg(0, stringOrQName, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("new-comment", 1, e -> e.populate( NewComment::new, NodeKindTest.COMMENT, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("new-document", 1, e -> e.populate( NewDocument::new, NodeKindTest.DOCUMENT, ONE, LATE | SIDE | NEW)
                .arg(0, AnyItemType.getInstance(), STAR, null));

        register("new-element", 2, e -> e.populate( NewElement::new, NodeKindTest.ELEMENT, ONE, LATE | SIDE | NEW)
                .arg(0, stringOrQName, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null));

        register("new-namespace", 2, e -> e.populate( NewNamespace::new, NodeKindTest.NAMESPACE, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("new-processing-instruction", 2, e -> e.populate( NewProcessingInstruction::new, NodeKindTest.PROCESSING_INSTRUCTION, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null));

        register("new-text", 1, e -> e.populate( NewText::new, NodeKindTest.TEXT, ONE, LATE | SIDE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        //#if CSHARP==false
        register("object-map", 1, e -> e.populate( ObjectMap::new, new MapType(BuiltInAtomicType.STRING, SequenceType.ANY_SEQUENCE), ONE, 0)
                .arg(0, AnyExternalObjectType.THE_INSTANCE, ONE, null));
        //#endif

        register("octets-to-base64Binary", 1, e -> e.populate( OctetsToBase64Binary::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null));

        register("octets-to-hexBinary", 1, e -> e.populate( OctetsToHexBinary::new, BuiltInAtomicType.HEX_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null));

        register("parse", 1, e -> e.populate( Parse::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY));

        register("parse", 2, e -> e.populate( Parse::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .setOptionDetails(Doc_2.makeOptionsParameter()));

        //#if CSHARP==false
        register("parse-dateTime", 2, e -> e.populate( ParseDateTime::new, BuiltInAtomicType.ANY_ATOMIC, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, EMPTY));
        //#endif
        register("parse-html", 1, e -> e.populate( com.saxonica.functions.qt4.ParseHtml::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY));

        register("parse-html", 2, e -> e.populate( com.saxonica.functions.qt4.ParseHtml::new, NodeKindTest.DOCUMENT, ONE, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .setOptionDetails(Doc_2.makeOptionsParameter()));

        register("parse-XPath", 3, e -> e.populate( ParseXPath::new, NodeKindTest.DOCUMENT, ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null));

        register("pedigree", 1, e -> e.populate( PedigreeFn::new, MapType.getInstance(), OPT, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null));

        register("print-stack", 0, e -> e.populate(PrintStack::new, BuiltInAtomicType.STRING, ONE, CITEM | LATE));

        register("query", 1, e -> e.populate( QueryFn::new, AnyItemType.getInstance(), STAR, LATE | CITEM)
                .arg(0, JavaExternalObjectType.of(XQueryExpression.class), OPT, EMPTY));

        register("query", 2, e -> e.populate( QueryFn::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, JavaExternalObjectType.of(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null));

        register("query", 3, e -> e.populate( QueryFn::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, JavaExternalObjectType.of(XQueryExpression.class), OPT, EMPTY)
                .arg(1, AnyItemType.getInstance(), ONE, null)
                .arg(2, AnyNodeTest.getInstance(), STAR, null));
        
        register("read-binary-resource", 1, e -> e.populate( ReadBinaryResource::new, BuiltInAtomicType.BASE64_BINARY, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        
        FunctionItemType stringReplacer = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_STRING},
                SequenceType.SINGLE_STRING);

        register("replace-with", 3, e -> e.populate( ReplaceWith::new, BuiltInAtomicType.STRING,
                 ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, stringReplacer, ONE, null));

        register("replace-with", 4, e -> e.populate( ReplaceWith::new, BuiltInAtomicType.STRING,
                 ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, stringReplacer, ONE, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null));

        //#if CSHARP==false
        register("send-mail", 3, e -> e.populate( SendMail::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null));

        register("send-mail", 4, e -> e.populate( SendMail::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, MapType.getInstance(), ONE, EMPTY)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null)
                .arg(3, AnyItemType.getInstance(), STAR, null));
        //#endif

        register("serialize", 2, e -> e.populate( Serialize::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyNodeTest.getInstance(), ONE, null)
                .arg(1, AnyItemType.getInstance(), ONE, null));

        register("slice", 2, e -> e.populate(SaxonSlice::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.INTEGER, STAR, null));

        register("string-to-hexBinary", 2, e -> e.populate( StringToHexBinaryFn::new, BuiltInAtomicType.HEX_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("string-to-base64Binary", 2, e -> e.populate( StringToBase64BinaryFn::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("string-to-utf8", 1, e -> e.populate( StringToUTF8::new, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("system-id", 0, e -> e.populate(SystemId::new, BuiltInAtomicType.STRING, OPT, CITEM | LATE));

        register("system-id", 1, e -> e.populate( SystemId::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY));


        register("timestamp", 0, e -> e.populate(TimestampFn::new, BuiltInAtomicType.DATE_TIME_STAMP, ONE, SIDE|LATE));

        register("transform", 2, e -> e.populate( TransformFn::new, NodeKindTest.DOCUMENT, OPT, 0)
                .arg(0, JavaExternalObjectType.of(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null));

        register("transform", 3, e -> e.populate( TransformFn::new, NodeKindTest.DOCUMENT, OPT, 0)
                .arg(0, JavaExternalObjectType.of(PreparedStylesheet.class), ONE, null)
                .arg(1, AnyNodeTest.getInstance(), ONE, null)
                .arg(2, AnyItemType.getInstance(), STAR, null));

        MapType parameterMapType = new MapType(BuiltInAtomicType.QNAME, SequenceType.ANY_SEQUENCE);

        register("tunnel-params", 0, e -> e.populate(TunnelParamsFn::new, parameterMapType, ONE, LATE));

        register("type-annotation", 1, e -> e.populate( TypeAnnotationFn::new, BuiltInAtomicType.QNAME, OPT, 0)
                .arg(0, AnyItemType.getInstance(), OPT, null));

        register("unescape-NCName", 1, e -> e.populate( UnescapeNCName::new, BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.NCNAME, ONE, EMPTY));

        register("unindexed", 1, e -> e.populate( Unindexed::new, AnyItemType.getInstance(), STAR, CTRL)
                .arg(0, AnyItemType.getInstance(), STAR, EmptySequence.getInstance()));

        //#if CSHARP==false
        register("unparsed-entities", 1, e -> e.populate( UnparsedEntities::new, BuiltInAtomicType.STRING, STAR, CTRL)
                .arg(0, NodeKindTest.DOCUMENT, ONE, null));
        //#endif

        register("valid-character", 1, e -> e.populate( ValidCharacter::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null));

        register("with-pedigree", 1, e -> e.populate( WithPedigree::new, AnyFunctionType.getInstance(), ONE, 0)
                .arg(0, AnyFunctionType.getInstance(), ONE, null));

        register("xquery", 1, e -> e.populate( XQueryFn::new, XQueryFn.functionType, ONE, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

    }

    /**
     * Return the namespace URI for the functions local to this function set.
     *
     * @return the namespace URI of the functions local to this function set.
     * Note that functions imported from another function set may have a different
     * namespace URI.
     */
    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.SAXON;
    }

    @Override
    public String getConventionalPrefix() {
        return "saxon";
    }

    public static class AdjustToCivilTime extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            DateTimeValue dt = (DateTimeValue)arguments[0].head();
            if (dt == null) {
                return EmptySequence.getInstance();
            }
            if (!dt.hasTimezone()) {
                dt = dt.adjustTimezone(context.getImplicitTimezone());
            }
            int seconds = NamedTimeZone.civilTimeOffsetInSeconds(dt, arguments[1].head().getStringValue());
            return dt.adjustTimezone(seconds / 60);
        }
    }

    public static class Base64BinaryToOctets extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue in = (Base64BinaryValue) arguments[0].head();
            byte[] bytes = in.getBinaryValue();
            Item[] array = new Item[bytes.length];
            for (int i = 0; i < array.length; i++) {
                array[i] = Int64Value.makeDerived(255 & (int) bytes[i], BuiltInAtomicType.UNSIGNED_BYTE);
            }
            return new SequenceExtent.Of<>(array);
        }
    }

    public static class Base64BinaryToString extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                Base64BinaryValue in = (Base64BinaryValue) arguments[0].head();
                if (in == null) {
                    return EmptySequence.getInstance();
                }
                byte[] bytes = in.getBinaryValue();
                String encoding = arguments[1].head().getStringValue();
                return decodeString(context, bytes, normalizeEncodingName(encoding));
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
    }




    public static class ColumnNumber extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo node = (NodeInfo) arguments[0].head();
            return Int64Value.makeIntegerValue(node.getColumnNumber());
        }
    }



    public static class ContextFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new ObjectValue<>(context);
        }
    }

    public static class ControllerFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new ObjectValue<Controller>(context.getController());
        }
    }

    public static class ConfigurationFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new ObjectValue<Configuration>(context.getConfiguration());
        }
    }

    public static class CurrentModeName extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Component.M mode = context.getCurrentMode();
            if (mode == null) {
                return EmptySequence.getInstance();
            }
            StructuredQName name = mode.getActor().getModeName();
            if (name == null || name.equals(Mode.UNNAMED_MODE_NAME)) {
                return EmptySequence.getInstance();
            }
            return new QNameValue(name, BuiltInAtomicType.QNAME);
        }
    }

    /**
     * Implement the saxon:discard-document() function, which returns its argument
     * unchanged, but with the side-effect of removing it from the document
     * pool, making it eligible for garbage collection.
     */

    public static class DiscardDocument extends SystemFunction {
        @Override
        public synchronized Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo doc = (NodeInfo)arguments[0].head();
            if (doc == null) {
                return EmptySequence.getInstance();
            }
            Controller c = context.getController();
            String uri = c.getDocumentPool().getDocumentURI(doc);
            if (uri != null && c instanceof XsltController) {
                ((XsltController) c).removeUnavailableOutputDestination(new DocumentKey(uri));
            }
            c.getDocumentPool().discard(doc.getTreeInfo());
            return doc;
        }
    }

    public static class GetContext extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new ObjectValue<>(context);
        }
    }

    public static class GetPseudoAttribute extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Item pi = context.getContextItem();
            if (pi == null) {
                throw new XPathException("The context item for saxon:get-pseudo-attribute() is not set")
                    .withXPathContext(context);
            }
            // we'll assume it's a PI, it doesn't matter if it isn't...
            final String value = ProcInstParser.getPseudoAttribute(pi.getStringValue(), arguments[0].head().getStringValue());
            return value == null ? EmptySequence.getInstance() : new StringValue(value);
        }
    }

    public static class HasSameNodes extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            SequenceIterator e1 = arguments[0].iterate();
            SequenceIterator e2 = arguments[1].iterate();

            if (e1 == null) {
                e1 = EmptyIterator.getInstance();
            }

            if (e2 == null) {
                e2 = EmptyIterator.getInstance();
            }

            while (true) {
                NodeInfo n1 = (NodeInfo) e1.next();
                NodeInfo n2 = (NodeInfo) e2.next();
                if (n1 == null || n2 == null) {
                    return BooleanValue.get(n1 == n2);
                }
                if (!n1.equals(n2)) {
                    return BooleanValue.FALSE;
                }
            }
        }
    }

    public static class HexBinaryToOctets extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            HexBinaryValue in = (HexBinaryValue) arguments[0].head();
            byte[] bytes = in.getBinaryValue();
            Item[] array = new Item[bytes.length];
            for (int i = 0; i < array.length; i++) {
                array[i] = Int64Value.makeDerived(255 & (int) bytes[i], BuiltInAtomicType.UNSIGNED_BYTE);
            }
            return new SequenceExtent.Of<>(array);
        }
    }

    public static class HexBinaryToString extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                HexBinaryValue in = (HexBinaryValue) arguments[0].head();
                if (in == null) {
                    return EmptySequence.getInstance();
                }
                byte[] bytes = in.getBinaryValue();
                String encoding = arguments[1].head().getStringValue();
                return decodeString(context, bytes, normalizeEncodingName(encoding));
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
    }

    @CSharpReplaceBody(code="return encoding==\"UTF8\" ? \"utf-8\" : encoding;")
    private static String normalizeEncodingName(String encoding) {
        return encoding;
    }

    private static StringValue decodeString(XPathContext context, byte[] bytes, String encoding) throws IOException, XPathException {
        encoding = normalizeEncodingName(encoding);
        ByteArrayInputStream stream = new ByteArrayInputStream(bytes);
        InputStreamReader reader = new InputStreamReader(stream, encoding);
        char[] array = new char[bytes.length];
        int used = reader.read(array, 0, array.length);
        checkBytes(array, 0, used, context.getConfiguration().getValidCharacterChecker());
        return new StringValue(new String(array, 0, used));
    }

    public static class InSummerTime extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            DateTimeValue dt = (DateTimeValue) arguments[0].head();
            if (!dt.hasTimezone()) {
                dt = dt.adjustTimezone(context.getImplicitTimezone());
            }

            Optional<Boolean> b = NamedTimeZone.inSummerTime(dt, arguments[1].head().getStringValue());
            if (b.isPresent()) {
                return BooleanValue.get(b.get());
            } else {
                return EmptySequence.getInstance();
            }
        }
    }

    /**
     * Ask whether an attribute node exists as a result of the expansion of
     * default and fixed values defined in a schema. Note that this property will only be set if both the configuration properties
     * {@link FeatureKeys#EXPAND_ATTRIBUTE_DEFAULTS} and {@link FeatureKeys#MARK_DEFAULTED_ATTRIBUTES}
     * are set. The property is currently maintained only for attribute nodes in a TinyTree that results from schema
     * validation, or where DTD-defined defaults are notified by the XML parser.
     */

    public static class IsDefaulted extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo node = (NodeInfo)arguments[0].head();
            boolean result = node instanceof TinyAttributeImpl && ((TinyAttributeImpl) node).isDefaultedAttribute();
            return BooleanValue.get(result);
        }
    }

    public static class LineNumber extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo node = (NodeInfo) arguments[0].head();
            return Int64Value.makeIntegerValue(node.getLineNumber());
        }
    }

    /**
     * Implement saxon:namespace-node extension function
     */
    public static class NamespaceNodeFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String prefix0 = arguments[0].head().getStringValue();
            UnicodeString uri0 = arguments[1].head().getUnicodeStringValue();
            if (!(prefix0.isEmpty() || NameChecker.isValidNCName(StringTool.codePoints(prefix0)))) {
                throw new XPathException("Namespace prefix " + Err.wrap(prefix0) + " is not a valid NCName");
            }
            if (uri0.isEmpty()) {
                throw new XPathException("URI of namespace node must not be empty");
            }
            Orphan node = new Orphan(context.getConfiguration());
            node.setNodeKind(Type.NAMESPACE);
            node.setNodeName(new NoNamespaceName(prefix0));
            node.setStringValue(uri0);
            return node;
        }
    }

    public static class OctetsToBase64Binary extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            byte[] bytes = toByteArray(arguments[0]);
            return new Base64BinaryValue(bytes);
        }
    }

    public static class OctetsToHexBinary extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            byte[] bytes = toByteArray(arguments[0]);
            return new HexBinaryValue(bytes);
        }
    }

    private static byte[] toByteArray(Sequence arg) throws XPathException {
        GroundedValue input = arg.materialize();
        byte[] octets = new byte[input.getLength()];
        for (int i = 0; i < octets.length; i++) {
            long l = ((IntegerValue)input.itemAt(i)).longValue();
            if (l < 0L || l > 255L) {
                throw new XPathException("Integers in input sequence must be in range 0-255", "SXJX0001");
            }
            octets[i] = (byte) l;
        }
        return octets;
    }

    //#if CSHARP==false
    public static class ParseDateTime extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            StringValue input = (StringValue) arguments[0].head();
            if (input == null) {
                return EmptySequence.getInstance();
            }
            StringValue format = (StringValue) arguments[1].head();
            DateTimeFormatter formatter;
            try {
                formatter = new DateTimeFormatterBuilder()
                        .parseCaseInsensitive()
                        .appendPattern(format.getStringValue())
                        .toFormatter()
                        .withResolverStyle(ResolverStyle.SMART);
            } catch (IllegalArgumentException e) {
                throw new XPathException("Invalid date/time pattern " + format.getUnicodeStringValue() + ": " + e.getMessage());
            }

            TemporalAccessor ta;
            try {
                ta = formatter.parse(input.getStringValue());
            } catch (DateTimeParseException e) {
                throw new XPathException("Supplied date/time value " + input.getUnicodeStringValue() + " cannot be parsed using pattern " + format.getUnicodeStringValue() + ": " + e.getMessage());

            }
            boolean hasYear = ta.isSupported(ChronoField.YEAR);
            boolean hasMonth = ta.isSupported(ChronoField.MONTH_OF_YEAR);
            boolean hasDay = ta.isSupported(ChronoField.DAY_OF_MONTH);
            boolean hasHour = ta.isSupported(ChronoField.HOUR_OF_DAY);
            boolean hasMinute = ta.isSupported(ChronoField.MINUTE_OF_HOUR);
            boolean hasSecond = ta.isSupported(ChronoField.SECOND_OF_MINUTE);
            boolean hasNano = ta.isSupported(ChronoField.NANO_OF_SECOND);
            boolean hasTZ = ta.isSupported(ChronoField.OFFSET_SECONDS);
            int tz = hasTZ ? ta.get(ChronoField.OFFSET_SECONDS) / 60 : CalendarValue.NO_TIMEZONE;
            int year = hasYear ? ta.get(ChronoField.YEAR) : 0;
            byte month = hasMonth ? (byte) ta.get(ChronoField.MONTH_OF_YEAR) : 0;
            byte day = hasDay ? (byte) ta.get(ChronoField.DAY_OF_MONTH) : 0;
            byte hour = hasHour ? (byte) ta.get(ChronoField.HOUR_OF_DAY) : 0;
            byte minute = hasMinute ? (byte) ta.get(ChronoField.MINUTE_OF_HOUR) : 0;
            byte second = hasSecond ? (byte) ta.get(ChronoField.SECOND_OF_MINUTE) : 0;
            int nano = hasNano ? ta.get(ChronoField.NANO_OF_SECOND) : 0;
            if (hasYear && hasMonth && hasDay) {
                if (hasHour && hasMinute) {
                    return new DateTimeValue(year, month, day, hour, minute, second, nano, tz);
                } else {
                    return new DateValue(year, month, day, tz, false);
                }
            } else if (hasHour && hasMinute) {
                return new TimeValue(hour, minute, second, nano, tz, BuiltInAtomicType.TIME);
            } else if (hasYear && hasMonth) {
                return new GYearMonthValue(year, month, tz, false);
            } else if (hasYear) {
                return new GYearValue(year, tz, false);
            } else if (hasMonth && hasDay) {
                return new GMonthDayValue(month, day, tz);
            } else if (hasMonth) {
                return new GMonthValue(month, tz);
            } else if (hasDay) {
                return new GDayValue(day, tz);
            } else {
                return EmptySequence.getInstance(); // should not reach here
            }
        }
    }
    //#endif

    public static class PrintStack extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            ByteArrayOutputStream baos = new ByteArrayOutputStream(1000);
            StandardLogger logger = new StandardLogger();
            logger.setPrintStream(new PrintStream(baos));
            new StandardDiagnostics().logStackTrace(context, logger, 2);
            return new StringValue(baos.toString().replace("\r", ""));
        }
    }

    public static class StringToBase64BinaryFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                StringValue s = (StringValue) arguments[0].head();
                if (s == null) {
                    return EmptySequence.getInstance();
                }
                StringValue encoding = (StringValue) arguments[1].head();
                byte[] bytes = s.getStringValue().getBytes(encoding.getStringValue());
                return new Base64BinaryValue(bytes);
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
    }

    public static class StringToHexBinaryFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                StringValue s = (StringValue) arguments[0].head();
                if (s == null) {
                    return EmptySequence.getInstance();
                }
                StringValue encoding = (StringValue) arguments[1].head();
                byte[] bytes = s.getStringValue().getBytes(encoding.getStringValue());
                return new HexBinaryValue(bytes);
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
    }

    /**
     * Implement saxon:string-to-utf8 extension function
     */
    public static class StringToUTF8 extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String s = arguments[0].head().getStringValue();
            ArrayList<IntegerValue> list = new ArrayList<>(s.length() * 2);
            byte[] octets = new byte[4];
            for (int i = 0; i < s.length(); i++) {
                int used = UTF8CharacterSet.getUTF8Encoding(
                        s.charAt(i), i + 1 < s.length() ? s.charAt(i + 1) : (char) 0, octets);
                for (int j = 0; j < used; j++) {
                    list.add(new Int64Value(255 & (int) octets[j]));
                }
            }
            return SequenceExtent.makeSequenceExtent(list);
        }
    }

    public static class SystemId extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo node;
            if (arguments.length == 0) {
                Item item = context.getContextItem();
                if (!(item instanceof NodeInfo)) {
                    throw new XPathException("Context item for saxon:system-id is not a node", "XPDY0002");
                }
                node = (NodeInfo)item;
            } else {
                node = (NodeInfo) arguments[0].head();
            }
            return new StringValue(node.getSystemId());
        }
    }

    /**
     * Implement saxon:tunnel-params extension function. Returns a map containing all the tunnel parameters
     * currently available, whether declared in the current template or not.
     */
    public static class TunnelParamsFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            ParameterSet params = context.getTunnelParameters();
            params.materializeValues();
            StructuredQName[] names = params.getParameterNames();
            HashTrieMap map = new HashTrieMap();
            for (int i = 0; i < params.size(); i++) {
                map.initialPut(new QNameValue(names[i], BuiltInAtomicType.QNAME), params.getValue(i).materialize());
            }
            return map;
        }
    }

    /**
     * Extension function saxon:type-annotation, which returns the type annotation of
     * a node or atomic value as an xs:QName value.
     */

    public static class TypeAnnotationFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Item item = arguments[0].head();
            if (item == null) {
                return EmptySequence.getInstance();
            } else if (item instanceof NodeInfo) {
                NodeInfo node = (NodeInfo) item;
                SchemaType type = node.getSchemaType();
                if (type == null) {
                    return EmptySequence.getInstance();
                }
                return new QNameValue(type.getStructuredQName(), BuiltInAtomicType.QNAME);
            } else if (item instanceof AtomicValue) {
                AtomicType type = ((AtomicValue) item).getItemType();
                return new QNameValue(type.getStructuredQName(), BuiltInAtomicType.QNAME);
            } else if (item.getGenre() == Genre.EXTERNAL) {
                return new QNameValue(JavaExternalObjectType.classNameToQName(
                        ((AnyExternalObject) item).getWrappedObject().getClass().getName()), BuiltInAtomicType.QNAME);
            } else {
                return EmptySequence.getInstance();
            }
        }
    }


    /**
     * Implement saxon:unparsed-entities extension function
     */
    public static class UnparsedEntities extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Iterator<String> names = ((NodeInfo)arguments[0].head()).getTreeInfo().getUnparsedEntityNames();
            List<StringValue> result = new ArrayList<>();
            while (names.hasNext()) {
                result.add(new StringValue(names.next()));
            }
            return SequenceExtent.makeSequenceExtent(result);
        }
    }

    /**
     * Implement saxon:valid-character extension function
     */
    public static class ValidCharacter extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            long charCode = ((IntegerValue)arguments[0].head()).longValue();
            boolean valid = charCode > 0
                    && charCode < UTF16CharacterSet.NONBMP_MAX
                    && context.getConfiguration().getValidCharacterChecker().test((int)charCode);
            return BooleanValue.get(valid);
        }
    }

    /**
     * Check that characters are valid XML characters (UTF-16 encoded)
     *
     * @param array   the array of characters
     * @param start   the position of the first significant character in the array
     * @param end     the position after the last significant character in the array
     * @param checker the NameChecker to be used (for XML 1.0 or XML 1.1 rules)
     * @throws XPathException if the string contains characters that are invalid in XML
     */

    private static void checkBytes(char[] array, int start, int end, IntPredicateProxy checker) throws XPathException {
        int ch32 = UTF16CharacterSet.firstInvalidChar(
                StringTool.codePoints(new String(array, start, end - start)), checker);
        if (ch32 != -1) {
            throw new XPathException("The byte sequence contains a character not allowed by XML (hex " +
                                                            Integer.toHexString(ch32) + ')')
                    .withErrorCode("XTDE1180");
        }
    }


}

// Copyright (c) 2004-2023 Saxonica Limited