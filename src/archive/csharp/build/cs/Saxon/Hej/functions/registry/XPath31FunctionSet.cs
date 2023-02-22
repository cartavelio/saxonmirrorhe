using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class XPath31FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.XPath31FunctionSet THE_INSTANCE = new Saxon.Hej.functions.registry.XPath31FunctionSet();
        public static Saxon.Hej.functions.registry.XPath31FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private XPath31FunctionSet()  {
            init();
        }
        private void init() {
            importFunctionSet(Saxon.Hej.functions.registry.XPath20FunctionSet.getInstance());
            importFunctionSet(Saxon.Hej.functions.registry.XPath30FunctionSet.getInstance());
            Saxon.Hej.type.SpecificFunctionType ft;
            register("collation-key", 1, e => e.populate(() => new Saxon.Hej.functions.CollationKeyFn(), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, DCOLL).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("collation-key", 2, e => e.populate(() => new Saxon.Hej.functions.CollatingFunctionFree(), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, DCOLL).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("contains-token", 2, e => e.populate(() => new Saxon.Hej.functions.ContainsToken(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, DCOLL).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("contains-token", 3, e => e.populate(() => new Saxon.Hej.functions.CollatingFunctionFree(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, BASE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("copy-of", 0, e => e.populate(() => new Saxon.Hej.functions.CopyOfFn(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, NEW));
            register("copy-of", 1, e => e.populate(() => new Saxon.Hej.functions.CopyOfFn(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, NEW).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|ABS), EMPTY));
            register("default-language", 0, e => e.populate(() => new Saxon.Hej.functions.DynamicContextAccessor.DefaultLanguage(), Saxon.Hej.type.BuiltInAtomicType.LANGUAGE, ONE, DLANG));
            register("generate-id", 0, e => e.populate(() => new Saxon.Hej.functions.ContextItemAccessorFunction(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (CITEM|LATE)));
            register("generate-id", 1, e => e.populate(() => new Saxon.Hej.functions.GenerateId_1(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.Type.NODE_TYPE, (OPT|INS), Saxon.Hej.value.StringValue.EMPTY_STRING));
            register("has-children", 0, e => e.populate(() => new Saxon.Hej.functions.ContextItemAccessorFunction(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (CITEM|LATE)));
            register("has-children", 1, e => e.populate(() => new Saxon.Hej.functions.HasChildren_1(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), (OPT|INS), null));
            register("head", 1, e => e.populate(() => new Saxon.Hej.functions.HeadFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, FILTER).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|TRA), null));
            register("innermost", 1, e => e.populate(() => new Saxon.Hej.functions.Innermost(), Saxon.Hej.pattern.AnyNodeTest.getInstance(), STAR, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), (STAR|NAV), null));
            register("json-doc", 1, e => e.populate(() => new Saxon.Hej.ma.json.JsonDoc(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("json-doc", 2, e => e.populate(() => new Saxon.Hej.ma.json.JsonDoc(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null).setOptionDetails(Saxon.Hej.ma.json.ParseJsonFn.OPTION_DETAILS));
            register("json-to-xml", 1, e => e.populate(() => new Saxon.Hej.ma.json.JsonToXMLFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("json-to-xml", 2, e => e.populate(() => new Saxon.Hej.ma.json.JsonToXMLFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null).setOptionDetails(Saxon.Hej.ma.json.JsonToXMLFn.OPTION_DETAILS));
            register("load-xquery-module", 1, e => e.populate(() => new Saxon.Hej.functions.hof.LoadXqueryModule(), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("load-xquery-module", 2, e => e.populate(() => new Saxon.Hej.functions.hof.LoadXqueryModule(), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, EMPTY).setOptionDetails(Saxon.Hej.functions.hof.LoadXqueryModule.makeOptionsParameter()));
            register("parse-ietf-date", 1, e => e.populate(() => new Saxon.Hej.functions.ParseIetfDate(), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY));
            register("parse-json", 1, e => e.populate(() => new Saxon.Hej.ma.json.ParseJsonFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY));
            register("parse-json", 2, e => e.populate(() => new Saxon.Hej.ma.json.ParseJsonFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null).setOptionDetails(Saxon.Hej.ma.json.ParseJsonFn.OPTION_DETAILS));
            register("parse-xml", 1, e => e.populate(() => new Saxon.Hej.functions.ParseXml(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY));
            register("random-number-generator", 0, e => e.populate(() => new Saxon.Hej.functions.hof.RandomNumberGenerator(), Saxon.Hej.functions.hof.RandomNumberGenerator.RETURN_TYPE, ONE, LATE));
            register("random-number-generator", 1, e => e.populate(() => new Saxon.Hej.functions.hof.RandomNumberGenerator(), Saxon.Hej.functions.hof.RandomNumberGenerator.RETURN_TYPE, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, OPT, null));
            register("parse-xml-fragment", 1, e => e.populate(() => new Saxon.Impl.Helpers.ParseXmlFragment(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY));
            register("serialize", 2, e => e.populate(() => new Saxon.Hej.functions.Serialize(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.Type.ITEM_TYPE, OPT, null).setOptionDetails(Saxon.Hej.functions.Serialize.makeOptionsParameter()));
            register("snapshot", 0, e => e.populate(() => new Saxon.Hej.functions.ContextItemAccessorFunction(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, ((CITEM|LATE)|NEW)));
            register("snapshot", 1, e => e.populate(() => new Saxon.Hej.functions.SnapshotFn(), Saxon.Hej.pattern.AnyNodeTest.getInstance(), STAR, NEW).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|ABS), EMPTY));
            register("sort", 1, e => e.populate(() => new Saxon.Hej.functions.Sort_1(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null));
            register("sort", 2, e => e.populate(() => new Saxon.Hej.functions.Sort_2(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            ft = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
            register("sort", 3, e => e.populate(() => new Saxon.Hej.functions.hof.Sort_3(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, ft, ONE, null));
            register("string-join", 1, e => e.populate(() => new Saxon.Hej.functions.StringJoin(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, Saxon.Hej.value.StringValue.EMPTY_STRING));
            register("string-join", 2, e => e.populate(() => new Saxon.Hej.functions.StringJoin(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, Saxon.Hej.value.StringValue.EMPTY_STRING).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("tokenize", 1, e => e.populate(() => new Saxon.Hej.functions.Tokenize_1(), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY));
            register("trace", 1, e => e.populate(() => new Saxon.Hej.functions.Trace(), Saxon.Hej.type.Type.ITEM_TYPE, STAR, (AS_ARG0|LATE)).arg(0, Saxon.Hej.type.Type.ITEM_TYPE, (STAR|TRA), null));
            register("transform", 1, e => e.populate(() => new Saxon.Hej.functions.TransformFn(), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, LATE).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, EMPTY).setOptionDetails(Saxon.Hej.functions.TransformFn.makeOptionsParameter()));
            register("xml-to-json", 1, e => e.populate(() => new Saxon.Hej.ma.json.XMLToJsonFn(), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, LATE).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), (OPT|ABS), EMPTY));
            register("xml-to-json", 2, e => e.populate(() => new Saxon.Hej.ma.json.XMLToJsonFn(), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, LATE).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), (OPT|ABS), EMPTY).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|ABS), null).setOptionDetails(Saxon.Hej.ma.json.XMLToJsonFn.makeOptionsParameter()));
        }
    
    }
}