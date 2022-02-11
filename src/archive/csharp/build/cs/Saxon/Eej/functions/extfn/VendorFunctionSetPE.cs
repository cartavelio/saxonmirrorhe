using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class VendorFunctionSetPE : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        protected internal Saxon.Eej.config.ProfessionalConfiguration config;
        public VendorFunctionSetPE(Saxon.Eej.config.ProfessionalConfiguration config)  {
            this.config = config;
            init();
        }
        private readonly static System.Collections.Generic.IList<string> analyzeUriKeys = Saxon.Ejava.util.Arrays.asList("isValid", "isAbsolute", "isOpaque", "port", "queryParams", "authority", "rawAuthority", "userInfo", "rawUserInfo", "scheme", "host", "schemeSpecificPart", "rawSchemeSpecificPart", "path", "rawPath", "query", "rawQuery", "fragment", "rawFragment", "asciiString", "error");
        private readonly static System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> analyzeUriValues = Saxon.Ejava.util.Arrays.asList(Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.SequenceType.OPTIONAL_INTEGER, Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.SINGLE_STRING), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE), Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.SequenceType.OPTIONAL_STRING);
        private readonly static Saxon.Hej.ma.map.RecordTest analyzeUriResult = new Saxon.Hej.ma.map.RecordTest(analyzeUriKeys, analyzeUriValues, new System.Collections.Generic.List<string>(), false);
        private readonly static Saxon.Hej.type.SpecificFunctionType predicate = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
        private void init() {
            importFunctionSet(Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance());
            Saxon.Hej.type.SpecificFunctionType orderKey = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC);
            register("adjust-to-civil-time", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.AdjustToCivilTime), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, OPT, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, OPT, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("analyze-uri", 1, typeof(Saxon.Eej.functions.extfn.AnalyzeUri), analyzeUriResult, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("array-member", 1, typeof(Saxon.Hej.functions.ParcelFn), Saxon.Hej.ma.Parcel.TYPE, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("base64Binary-to-octets", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.Base64BinaryToOctets), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null);
            register("base64Binary-to-string", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.Base64BinaryToString), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("characters", 1, typeof(Saxon.Eej.functions.extfn.CharactersFn), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("column-number", 0, typeof(Saxon.Hej.functions.ContextItemAccessorFunction), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, (CITEM|LATE));
            register("column-number", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.ColumnNumber), Saxon.Hej.type.BuiltInAtomicType.INTEGER, OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY);
            register("compile-query", 1, typeof(Saxon.Eej.functions.extfn.CompileQuery), config.getJavaExternalObjectType(typeof(Saxon.Hej.query.XQueryExpression)), ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("compile-stylesheet", 1, typeof(Saxon.Eej.functions.extfn.CompileStylesheet), config.getJavaExternalObjectType(typeof(Saxon.Hej.PreparedStylesheet)), ONE, 0).arg(0, Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, null);
            register("configuration", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.ConfigurationFn), config.getJavaExternalObjectType(typeof(Saxon.Hej.Configuration)), ONE, LATE);
            register("context", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.ContextFn), config.getJavaExternalObjectType(typeof(Saxon.Hej.expr.XPathContext)), ONE, (CITEM|LATE));
            register("controller", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.ControllerFn), config.getJavaExternalObjectType(typeof(Saxon.Hej.Controller)), ONE, LATE);
            register("current-mode-name", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.CurrentModeName), Saxon.Hej.type.BuiltInAtomicType.QNAME, OPT, (CITEM|LATE));
            register("deep-equal", 4, typeof(Saxon.Eej.functions.extfn.SaxonDeepEqual), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("decimal-divide", 3, typeof(Saxon.Eej.functions.extfn.DecimalDivide), Saxon.Hej.type.BuiltInAtomicType.DECIMAL, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DECIMAL, OPT, EMPTY).arg(1, Saxon.Hej.type.BuiltInAtomicType.DECIMAL, OPT, EMPTY).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("discard-document", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.DiscardDocument), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, 0).arg(0, Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, null);
            register("EQName", 1, typeof(Saxon.Eej.functions.extfn.EQName), Saxon.Hej.type.BuiltInAtomicType.QNAME, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY);
            register("escape-NCName", 1, typeof(Saxon.Eej.functions.extfn.EscapeNCName), Saxon.Hej.type.BuiltInAtomicType.NCNAME, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY);
            register("eval", 10, typeof(Saxon.Eej.functions.extfn.Eval), Saxon.Hej.type.AnyItemType.getInstance(), STAR, CITEM).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Eej.functions.extfn.Evaluate.PreparedExpression)), ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(3, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(4, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(5, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(6, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(7, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(8, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(9, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            registerReducedArityVariants("eval#10", 1, 9);
            register("evaluate", 10, typeof(Saxon.Eej.functions.extfn.Evaluate), Saxon.Hej.type.AnyItemType.getInstance(), STAR, CITEM).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(3, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(4, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(5, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(6, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(7, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(8, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(9, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            registerReducedArityVariants("evaluate#10", 1, 9);
            register("expression", 1, typeof(Saxon.Eej.functions.extfn.ExpressionFn), config.getJavaExternalObjectType(typeof(Saxon.Eej.functions.extfn.Evaluate.PreparedExpression)), ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("expression", 2, typeof(Saxon.Eej.functions.extfn.ExpressionFn), config.getJavaExternalObjectType(typeof(Saxon.Eej.functions.extfn.Evaluate.PreparedExpression)), ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, null);
            register("function-annotations", 1, typeof(Saxon.Eej.functions.extfn.FunctionAnnotations), Saxon.Eej.functions.extfn.FunctionAnnotations.RETURN_TYPE, STAR, 0).arg(0, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null);
            register("get-context", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.GetContext), config.getJavaExternalObjectType(typeof(Saxon.Hej.expr.XPathContext)), ONE, (CITEM|LATE));
            register("get-pseudo-attribute", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.GetPseudoAttribute), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, (CITEM|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY);
            register("group-starting", 2, typeof(Saxon.Eej.functions.extfn.GroupStarting), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, predicate, ONE, EMPTY);
            register("has-same-nodes", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.HasSameNodes), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), STAR, null).arg(1, Saxon.Hej.pattern.AnyNodeTest.getInstance(), STAR, null);
            register("hexBinary-to-octets", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.HexBinaryToOctets), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY, ONE, null);
            register("hexBinary-to-string", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.HexBinaryToString), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("highest", 1, typeof(Saxon.Eej.functions.extfn.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY);
            register("highest", 2, typeof(Saxon.Eej.functions.extfn.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, orderKey, ONE, null);
            register("index", 2, typeof(Saxon.Eej.functions.extfn.Index), Saxon.Hej.ma.map.MapType.getInstance(), ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, EMPTY);
            register("index-where", 2, typeof(Saxon.Eej.functions.extfn.IndexWhere), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, predicate, ONE, EMPTY);
            register("in-scope-namespaces", 1, typeof(Saxon.Eej.functions.extfn.InScopeNamespacesFn), Saxon.Hej.ma.map.MapType.getInstance(), ONE, 0).arg(0, Saxon.Hej.pattern.NodeKindTest.ELEMENT, (ONE|INS), null);
            register("in-summer-time", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.InSummerTime), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, OPT, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("is-defaulted", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.IsDefaulted), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), ONE, null);
            register("is-NaN", 1, typeof(Saxon.Eej.functions.extfn.IsNaN), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, ONE, null);
            register("items-after", 2, typeof(Saxon.Eej.functions.extfn.Starting.ItemsAfter), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|TRA), EMPTY).arg(1, predicate, ONE, EMPTY);
            register("items-from", 2, typeof(Saxon.Eej.functions.extfn.Starting.ItemsFrom), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|TRA), EMPTY).arg(1, predicate, ONE, EMPTY);
            register("items-before", 2, typeof(Saxon.Eej.functions.extfn.Ending.ItemsBefore), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|TRA), EMPTY).arg(1, predicate, ONE, EMPTY);
            register("items-until", 2, typeof(Saxon.Eej.functions.extfn.Ending.ItemsUntil), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|TRA), EMPTY).arg(1, predicate, ONE, EMPTY);
            register("leading", 1, typeof(Saxon.Eej.functions.extfn.Leading), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY);
            register("leading", 2, typeof(Saxon.Eej.functions.extfn.Leading), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, predicate, ONE, null);
            register("line-number", 0, typeof(Saxon.Hej.functions.ContextItemAccessorFunction), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, (CITEM|LATE));
            register("line-number", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.LineNumber), Saxon.Hej.type.BuiltInAtomicType.INTEGER, OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY);
            register("lowest", 1, typeof(Saxon.Eej.functions.extfn.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY);
            register("lowest", 2, typeof(Saxon.Eej.functions.extfn.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, orderKey, ONE, null);
            Saxon.Hej.type.FunctionItemType tester = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            register("map-search", 2, typeof(Saxon.Eej.functions.extfn.MapSearch), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null);
            register("map-search", 3, typeof(Saxon.Eej.functions.extfn.MapSearch), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null).arg(2, tester, (ONE|INS), null);
            register("message-count", 1, typeof(Saxon.Eej.functions.extfn.MessageCount), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.QNAME, OPT, null);
            register("namespace-node", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.NamespaceNodeFn), Saxon.Hej.pattern.NodeKindTest.NAMESPACE, ONE, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            Saxon.Hej.type.LocalUnionType stringOrQName = new Saxon.Hej.type.LocalUnionType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.type.BuiltInAtomicType.QNAME);
            register("new-attribute", 2, typeof(Saxon.Eej.functions.extfn.NewAttribute), Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE, ONE, ((LATE|SIDE)|NEW)).arg(0, stringOrQName, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("new-comment", 1, typeof(Saxon.Eej.functions.extfn.NewComment), Saxon.Hej.pattern.NodeKindTest.COMMENT, ONE, ((LATE|SIDE)|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("new-document", 1, typeof(Saxon.Eej.functions.extfn.NewDocument), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, ((LATE|SIDE)|NEW)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("new-element", 2, typeof(Saxon.Eej.functions.extfn.NewElement), Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, ((LATE|SIDE)|NEW)).arg(0, stringOrQName, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("new-namespace", 2, typeof(Saxon.Eej.functions.extfn.NewNamespace), Saxon.Hej.pattern.NodeKindTest.NAMESPACE, ONE, ((LATE|SIDE)|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("new-processing-instruction", 2, typeof(Saxon.Eej.functions.extfn.NewProcessingInstruction), Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION, ONE, ((LATE|SIDE)|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null);
            register("new-text", 1, typeof(Saxon.Eej.functions.extfn.NewText), Saxon.Hej.pattern.NodeKindTest.TEXT, ONE, ((LATE|SIDE)|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("octets-to-base64Binary", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.OctetsToBase64Binary), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, null);
            register("octets-to-hexBinary", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.OctetsToHexBinary), Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, null);
            register("parse", 1, typeof(Saxon.Eej.functions.extfn.Parse), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY);
            register("parse", 2, typeof(Saxon.Eej.functions.extfn.Parse), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, EMPTY).setOptionDetails(Saxon.Hej.functions.Doc_2.makeOptionsParameter());
            register("parse-html", 1, typeof(Saxon.Impl.Helpers.ParseHtml), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY);
            register("parse-html", 2, typeof(Saxon.Impl.Helpers.ParseHtml), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, (LATE|NEW)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, EMPTY).setOptionDetails(Saxon.Hej.functions.Doc_2.makeOptionsParameter());
            register("parse-XPath", 3, typeof(Saxon.Eej.functions.extfn.ParseXPath), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null);
            register("pedigree", 1, typeof(Saxon.Eej.functions.extfn.PedigreeFn), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null);
            register("print-stack", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.PrintStack), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (CITEM|LATE));
            register("query", 1, typeof(Saxon.Eej.functions.extfn.QueryFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|CITEM)).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Hej.query.XQueryExpression)), OPT, EMPTY);
            register("query", 2, typeof(Saxon.Eej.functions.extfn.QueryFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Hej.query.XQueryExpression)), OPT, EMPTY).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("query", 3, typeof(Saxon.Eej.functions.extfn.QueryFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Hej.query.XQueryExpression)), OPT, EMPTY).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(2, Saxon.Hej.pattern.AnyNodeTest.getInstance(), STAR, null);
            register("read-binary-resource", 1, typeof(Saxon.Eej.functions.extfn.ReadBinaryResource), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            Saxon.Hej.type.FunctionItemType stringReplacer = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_STRING);
            register("replace-with", 3, typeof(Saxon.Eej.functions.extfn.ReplaceWith), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.StringValue.EMPTY_STRING).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, stringReplacer, ONE, null);
            register("replace-with", 4, typeof(Saxon.Eej.functions.extfn.ReplaceWith), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.StringValue.EMPTY_STRING).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, stringReplacer, ONE, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("serialize", 2, typeof(Saxon.Eej.functions.extfn.Serialize), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("slice", 2, typeof(Saxon.Eej.functions.extfn.Slice), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, null);
            register("string-to-hexBinary", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.StringToHexBinaryFn), Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("string-to-base64Binary", 2, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.StringToBase64BinaryFn), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("string-to-utf8", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.StringToUTF8), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("system-id", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.SystemId), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, (CITEM|LATE));
            register("system-id", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.SystemId), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY);
            register("timestamp", 0, typeof(Saxon.Eej.functions.extfn.TimestampFn), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME_STAMP, ONE, (SIDE|LATE));
            register("transform", 2, typeof(Saxon.Eej.functions.extfn.TransformFn), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, 0).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Hej.PreparedStylesheet)), ONE, null).arg(1, Saxon.Hej.pattern.AnyNodeTest.getInstance(), ONE, null);
            register("transform", 3, typeof(Saxon.Eej.functions.extfn.TransformFn), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, 0).arg(0, config.getJavaExternalObjectType(typeof(Saxon.Hej.PreparedStylesheet)), ONE, null).arg(1, Saxon.Hej.pattern.AnyNodeTest.getInstance(), ONE, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            Saxon.Hej.ma.map.MapType parameterMapType = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            register("tunnel-params", 0, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.TunnelParamsFn), parameterMapType, ONE, LATE);
            register("type-annotation", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.TypeAnnotationFn), Saxon.Hej.type.BuiltInAtomicType.QNAME, OPT, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null);
            register("unescape-NCName", 1, typeof(Saxon.Eej.functions.extfn.UnescapeNCName), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.NCNAME, ONE, EMPTY);
            register("unindexed", 1, typeof(Saxon.Eej.functions.extfn.Unindexed), Saxon.Hej.type.AnyItemType.getInstance(), STAR, CTRL).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, Saxon.Hej.value.EmptySequence.getInstance());
            register("valid-character", 1, typeof(Saxon.Eej.functions.extfn.VendorFunctionSetPE.ValidCharacter), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("with-pedigree", 1, typeof(Saxon.Eej.functions.extfn.WithPedigree), Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, 0).arg(0, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null);
            register("xquery", 1, typeof(Saxon.Eej.functions.extfn.XQueryFn), Saxon.Eej.functions.extfn.XQueryFn.functionType, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SAXON;
        }
        public override string getConventionalPrefix() {
            return "saxon";
        }
        public class AdjustToCivilTime : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(arguments[0].head());
                if ((dt == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                if (!(dt.hasTimezone())) {
                    dt = dt.adjustTimezone(context.getImplicitTimezone());
                }
                int seconds = Saxon.Hej.expr.number.NamedTimeZone.civilTimeOffsetInSeconds(dt, arguments[1].head().getStringValue());
                return dt.adjustTimezone((seconds/60));
            }
        
        }
        public class Base64BinaryToOctets : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue @in = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                byte[] bytes = @in.getBinaryValue();
                Saxon.Hej.om.Item[] array = new Saxon.Hej.om.Item[bytes.Length];
                for (int i = 0; (i<array.Length); i++) {
                    array[i] = Saxon.Hej.value.Int64Value.makeDerived((255&(int)(bytes[i])), Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE);
                }
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(array);
            }
        
        }
        public class Base64BinaryToString : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    Saxon.Hej.value.Base64BinaryValue @in = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                    byte[] bytes = @in.getBinaryValue();
                    string encoding = arguments[1].head().getStringValue();
                    return decodeString(context, bytes, normalizeEncodingName(encoding));
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        
        }
        public class ColumnNumber : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                return Saxon.Hej.value.Int64Value.makeIntegerValue(node.getColumnNumber());
            }
        
        }
        public class ContextFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.XPathContext>(context);
            }
        
        }
        public class ControllerFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.Controller>(context.getController());
            }
        
        }
        public class ConfigurationFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.Configuration>(context.getConfiguration());
            }
        
        }
        public class CurrentModeName : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.expr.Component.M mode = context.getCurrentMode();
                if ((mode == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Hej.om.StructuredQName name = mode.getActor().getModeName();
                if (((name == null) || name.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME))) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return new Saxon.Hej.value.QNameValue(name, Saxon.Hej.type.BuiltInAtomicType.QNAME);
            }
        
        }
        public class DiscardDocument : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) { lock(this) {
                Saxon.Hej.om.NodeInfo doc = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                if ((doc == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Hej.Controller c = context.getController();
                string uri = c.getDocumentPool().getDocumentURI(doc);
                if (((uri != null) && (c is Saxon.Hej.trans.XsltController))) {
                    ((Saxon.Hej.trans.XsltController)(c)).removeUnavailableOutputDestination(new Saxon.Hej.om.DocumentKey(uri));
                }
                c.getDocumentPool().discard(doc.getTreeInfo());
                return doc;
            }}
        
        }
        public class GetContext : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.XPathContext>(context);
            }
        
        }
        public class GetPseudoAttribute : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Item pi = context.getContextItem();
                if ((pi == null)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("The context item for saxon:get-pseudo-attribute() is not set");
                    e.setXPathContext(context);
                    throw e;
                }
                string value = Saxon.Impl.Helpers.ProcInstParser.getPseudoAttribute(pi.getStringValue(), arguments[0].head().getStringValue());
                return ((value == null) ? Saxon.Hej.value.EmptySequence.getInstance() : new Saxon.Hej.value.StringValue(value));
            }
        
        }
        public class HasSameNodes : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.SequenceIterator e1 = arguments[0].iterate();
                Saxon.Hej.om.SequenceIterator e2 = arguments[1].iterate();
                if ((e1 == null)) {
                    e1 = Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                }
                if ((e2 == null)) {
                    e2 = Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                }
                while (true) {
                    Saxon.Hej.om.NodeInfo n1 = (Saxon.Hej.om.NodeInfo)(e1.next());
                    Saxon.Hej.om.NodeInfo n2 = (Saxon.Hej.om.NodeInfo)(e2.next());
                    if (((n1 == null) || (n2 == null))) {
                        return Saxon.Hej.value.BooleanValue.get((n1 == n2));
                    }
                    if (!(n1.Equals(n2))) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                }
            }
        
        }
        public class HexBinaryToOctets : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.HexBinaryValue @in = (Saxon.Hej.value.HexBinaryValue)(arguments[0].head());
                byte[] bytes = @in.getBinaryValue();
                Saxon.Hej.om.Item[] array = new Saxon.Hej.om.Item[bytes.Length];
                for (int i = 0; (i<array.Length); i++) {
                    array[i] = Saxon.Hej.value.Int64Value.makeDerived((255&(int)(bytes[i])), Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE);
                }
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(array);
            }
        
        }
        public class HexBinaryToString : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    Saxon.Hej.value.HexBinaryValue @in = (Saxon.Hej.value.HexBinaryValue)(arguments[0].head());
                    byte[] bytes = @in.getBinaryValue();
                    string encoding = arguments[1].head().getStringValue();
                    return decodeString(context, bytes, normalizeEncodingName(encoding));
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        
        }
        private static string normalizeEncodingName(string encoding) {
            return encoding=="UTF8" ? "utf-8" : encoding;
        }
        private static Saxon.Hej.value.StringValue decodeString(Saxon.Hej.expr.XPathContext context, byte[] bytes, string encoding) {
            encoding = normalizeEncodingName(encoding);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding(encoding));
            char[] array = new char[bytes.Length];
            int used = reader.Read(array, 0, array.Length);
            checkBytes(array, 0, used, context.getConfiguration().getValidCharacterChecker());
            return new Saxon.Hej.value.StringValue(new string(array, 0, used));
        }
        public class InSummerTime : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(arguments[0].head());
                if (!(dt.hasTimezone())) {
                    dt = dt.adjustTimezone(context.getImplicitTimezone());
                }
                Saxon.Ejava.util.Optional<bool> b = Saxon.Hej.expr.number.NamedTimeZone.inSummerTime(dt, arguments[1].head().getStringValue());
                if (b.isPresent()) {
                    return Saxon.Hej.value.BooleanValue.get(b.get());
                } else {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
        
        }
        public class IsDefaulted : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                bool result = ((node is Saxon.Hej.tree.tiny.TinyAttributeImpl) && ((Saxon.Hej.tree.tiny.TinyAttributeImpl)(node)).isDefaultedAttribute());
                return Saxon.Hej.value.BooleanValue.get(result);
            }
        
        }
        public class LineNumber : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                return Saxon.Hej.value.Int64Value.makeIntegerValue(node.getLineNumber());
            }
        
        }
        public class NamespaceNodeFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string prefix0 = arguments[0].head().getStringValue();
                Saxon.Hej.str.UnicodeString uri0 = arguments[1].head().getUnicodeStringValue();
                if (!(((prefix0.Length==0 || Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(prefix0)))))) {
                    throw new Saxon.Hej.trans.XPathException((("Namespace prefix "+Saxon.Hej.trans.Err.wrap(prefix0))+" is not a valid NCName"));
                }
                if (uri0.isEmpty()) {
                    throw new Saxon.Hej.trans.XPathException("URI of namespace node must not be empty");
                }
                Saxon.Hej.tree.util.Orphan node = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
                node.setNodeKind(Saxon.Hej.type.Type.NAMESPACE);
                node.setNodeName(new Saxon.Hej.om.NoNamespaceName(prefix0));
                node.setStringValue(uri0);
                return node;
            }
        
        }
        public class OctetsToBase64Binary : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                byte[] bytes = toByteArray(arguments[0]);
                return new Saxon.Hej.value.Base64BinaryValue(bytes);
            }
        
        }
        public class OctetsToHexBinary : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                byte[] bytes = toByteArray(arguments[0]);
                return new Saxon.Hej.value.HexBinaryValue(bytes);
            }
        
        }
        private static byte[] toByteArray(Saxon.Hej.om.Sequence arg) {
            Saxon.Hej.om.GroundedValue input = arg.materialize();
            byte[] octets = new byte[input.getLength()];
            for (int i = 0; (i<octets.Length); i++) {
                long l = ((Saxon.Hej.value.IntegerValue)(input.itemAt(i))).longValue();
                if (((l<0L) || (l>255L))) {
                    throw new Saxon.Hej.trans.XPathException("Integers in input sequence must be in range 0-255", "SXJX0001");
                }
                octets[i] = (byte)(l);
            }
            return octets;
        }
        public class PrintStack : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.IO.MemoryStream baos = new System.IO.MemoryStream(1000);
                Saxon.Callbacks.StandardLogger logger = new Saxon.Callbacks.StandardLogger();
                logger.setPrintStream(new Saxon.Ejava.io.PrintStream(baos));
                new Saxon.Hej.lib.StandardDiagnostics().logStackTrace(context, logger, 2);
                return new Saxon.Hej.value.StringValue(baos.ToString().Replace("\r", ""));
            }
        
        }
        public class StringToBase64BinaryFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
                    if ((s == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                    Saxon.Hej.value.StringValue encoding = (Saxon.Hej.value.StringValue)(arguments[1].head());
                    byte[] bytes = Saxon.Impl.Helpers.StringUtils.EncodingForName(encoding.getStringValue()).GetBytes(s.getStringValue());
                    return new Saxon.Hej.value.Base64BinaryValue(bytes);
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        
        }
        public class StringToHexBinaryFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
                    if ((s == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                    Saxon.Hej.value.StringValue encoding = (Saxon.Hej.value.StringValue)(arguments[1].head());
                    byte[] bytes = Saxon.Impl.Helpers.StringUtils.EncodingForName(encoding.getStringValue()).GetBytes(s.getStringValue());
                    return new Saxon.Hej.value.HexBinaryValue(bytes);
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        
        }
        public class StringToUTF8 : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string s = arguments[0].head().getStringValue();
                System.Collections.Generic.List<Saxon.Hej.value.IntegerValue> list = new System.Collections.Generic.List<Saxon.Hej.value.IntegerValue>((s.Length*2));
                byte[] octets = new byte[4];
                for (int i = 0; (i<s.Length); i++) {
                    int used = Saxon.Hej.serialize.charcode.UTF8CharacterSet.getUTF8Encoding(s[i], (((i+1)<s.Length) ? s[(i+1)] : (char)(0)), octets);
                    for (int j = 0; (j<used); j++) {
                        list.Add(new Saxon.Hej.value.Int64Value((255&(int)(octets[j]))));
                    }
                }
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
            }
        
        }
        public class SystemId : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo node;
                if ((arguments.Length == 0)) {
                    Saxon.Hej.om.Item item = context.getContextItem();
                    if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                        throw new Saxon.Hej.trans.XPathException("Context item for saxon:system-id is not a node", "XPDY0002");
                    }
                    node = (Saxon.Hej.om.NodeInfo)(item);
                } else {
                    node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                }
                return new Saxon.Hej.value.StringValue(node.getSystemId());
            }
        
        }
        public class TunnelParamsFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.expr.instruct.ParameterSet @params = context.getTunnelParameters();
                @params.materializeValues();
                Saxon.Hej.om.StructuredQName[] names = @params.getParameterNames();
                Saxon.Impl.DataModel.ImmMap map = new Saxon.Impl.DataModel.ImmMap();
                for (int i = 0; (i<@params.size()); i++) {
                    map.initialPut(new Saxon.Hej.value.QNameValue(names[i], Saxon.Hej.type.BuiltInAtomicType.QNAME), @params.getValue(i).materialize());
                }
                return map;
            }
        
        }
        public class TypeAnnotationFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Item item = arguments[0].head();
                if ((item == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        Saxon.Hej.type.SchemaType type = node.getSchemaType();
                        if ((type == null)) {
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                        return new Saxon.Hej.value.QNameValue(type.getStructuredQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                    } else {
                        if ((item is Saxon.Hej.value.AtomicValue)) {
                            Saxon.Hej.type.AtomicType type = ((Saxon.Hej.value.AtomicValue)(item)).getItemType();
                            return new Saxon.Hej.value.QNameValue(type.getStructuredQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                        } else {
                            if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                                return new Saxon.Hej.value.QNameValue(Saxon.Hej.type.JavaExternalObjectType.classNameToQName(((Saxon.Hej.value.AnyExternalObject)(item)).getWrappedObject().GetType().Name), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                        }
                    }
                }
            }
        
        }
        public class UnparsedEntities : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.Collections.Generic.IEnumerator<string> names = ((Saxon.Hej.om.NodeInfo)(arguments[0].head())).getTreeInfo().getUnparsedEntityNames();
                System.Collections.Generic.IList<Saxon.Hej.value.StringValue> result = new System.Collections.Generic.List<Saxon.Hej.value.StringValue>();
                while (names.MoveNext() /*UNSAFE CONVERSION*/) {
                    result.Add(new Saxon.Hej.value.StringValue(names.Current));
                }
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
            }
        
        }
        public class ValidCharacter : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                long charCode = ((Saxon.Hej.value.IntegerValue)(arguments[0].head())).longValue();
                bool valid = (((charCode>0) && (charCode<Saxon.Hej.serialize.charcode.UTF16CharacterSet.NONBMP_MAX)) && context.getConfiguration().getValidCharacterChecker().test((int)(charCode)));
                return Saxon.Hej.value.BooleanValue.get(valid);
            }
        
        }
        private static void checkBytes(char[] array, int start, int end, Saxon.Hej.z.IntPredicateProxy checker) {
            int ch32 = Saxon.Hej.serialize.charcode.UTF16CharacterSet.firstInvalidChar(Saxon.Hej.str.StringTool.codePoints(new string(array, start, (end-start))), checker);
            if ((ch32 != -1)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The byte sequence contains a character not allowed by XML (hex "+Saxon.Impl.Helpers.Utils.intToHexString(ch32))+')'));
                err.setErrorCode("XTDE1180");
                throw err;
            }
        }
    
    }
}