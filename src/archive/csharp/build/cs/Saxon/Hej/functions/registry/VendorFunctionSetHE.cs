using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class VendorFunctionSetHE : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.VendorFunctionSetHE THE_INSTANCE = new Saxon.Hej.functions.registry.VendorFunctionSetHE();
        public static Saxon.Hej.functions.registry.VendorFunctionSetHE getInstance() {
            return THE_INSTANCE;
        }
        private VendorFunctionSetHE()  {
            init();
        }
        private void init() {
            register("is-whole-number", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.IsWholeNumberFn), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.type.NumericType.getInstance(), OPT, EMPTY);
            register("dynamic-error-info", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.DynamicErrorInfoFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, ((FOCUS|LATE)|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("apply", 2, typeof(Saxon.Hej.functions.ApplyFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, null);
            register("create-map", 1, typeof(Saxon.Hej.ma.map.MapCreate), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, null);
            register("doc", 2, typeof(Saxon.Hej.functions.Doc_2), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, EMPTY).setOptionDetails(Saxon.Hej.functions.Doc_2.makeOptionsParameter());
            register("has-local-namespaces", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.HasLocalNamespaces), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, null);
            register("has-uniform-namespaces", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.HasUniformNamespaces), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, null);
            register("map-untyped-contains", 2, typeof(Saxon.Hej.ma.map.MapUntypedContains), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, ONE, null);
            Saxon.Hej.ma.map.RecordTest mapRepresentation = new Saxon.Hej.ma.map.RecordTest(Saxon.Ejava.util.Arrays.asList("key", "value"), Saxon.Ejava.util.Arrays.asList(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, Saxon.Hej.value.SequenceType.ANY_SEQUENCE}), new System.Collections.Generic.List<string>(), false);
            register("map-as-sequence-of-maps", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.MapAsSequenceOfMaps), mapRepresentation, STAR, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null);
            register("yes-no-boolean", 1, typeof(Saxon.Hej.functions.registry.VendorFunctionSetHE.YesNoBoolean), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SAXON;
        }
        public override string getConventionalPrefix() {
            return "saxon";
        }
        public class IsWholeNumberFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
                if ((arguments[0].getItemType().getPrimitiveItemType() == Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE);
                }
                return null;
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.NumericValue val = (Saxon.Hej.value.NumericValue)(arguments[0].head());
                return Saxon.Hej.value.BooleanValue.get(((val != null) && val.isWholeNumber()));
            }
        
        }
        public class HasLocalNamespaces : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo child = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                Saxon.Hej.om.NodeInfo parent = child.getParent();
                return Saxon.Hej.value.BooleanValue.get((((parent == null) || (parent.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) || (child.getAllNamespaces() != parent.getAllNamespaces())));
            }
        
        }
        public class HasUniformNamespaces : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo val = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                if ((val is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                    return Saxon.Hej.value.BooleanValue.get(((Saxon.Hej.tree.tiny.TinyElementImpl)(val)).hasUniformNamespaces());
                } else {
                    return Saxon.Hej.value.BooleanValue.FALSE;
                }
            }
        
        }
        public class DynamicErrorInfoFn : Saxon.Hej.functions.SystemFunction {
            public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
                return 0;
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string var = arguments[0].head().getStringValue();
                Saxon.Hej.trans.XPathException error = context.getCurrentException();
                if ((error == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Ejavax.xml.transform.SourceLocator locator = error.getLocator();
                switch (var) {
                  case "code":
                    Saxon.Hej.om.StructuredQName errorCodeQName = error.getErrorCodeQName();
                    if ((errorCodeQName == null)) {
                        errorCodeQName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "XXXX9999");
                    }
                    return new Saxon.Hej.value.QNameValue(errorCodeQName, Saxon.Hej.type.BuiltInAtomicType.QNAME);
                  case "description":
                    string s = error.Message;
                    if ((error.InnerException != null)) {
                        s += (("("+error.InnerException.Message)+")");
                    }
                    return new Saxon.Hej.value.StringValue(s);
                  case "value":
                    Saxon.Hej.om.Sequence value = error.getErrorObject();
                    if ((value == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return value;
                    }
                  case "module":
                    string module = ((locator == null) ? null : locator.getSystemId());
                    if ((module == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.StringValue(module);
                    }
                  case "line-number":
                    int line = ((locator == null) ? -1 : locator.getLineNumber());
                    if ((line == -1)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.Int64Value(line);
                    }
                  case "column-number":
                    int column = -1;
                    if ((locator == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        if ((locator is Saxon.Hej.expr.parser.XPathParser.NestedLocation)) {
                            column = ((Saxon.Hej.expr.parser.XPathParser.NestedLocation)(locator)).getContainingLocation().getColumnNumber();
                        } else {
                            column = locator.getColumnNumber();
                        }
                    }
                    if ((column == -1)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.Int64Value(column);
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
        
        }
        private static Saxon.Hej.value.StringValue valueKey = new Saxon.Hej.value.StringValue(new Saxon.Hej.str.Twine8(Saxon.Hej.str.StringConstants.bytes("value")));
        public class ArrayAsSequenceOfMaps : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Collections.Generic.IList<Saxon.Hej.om.Item> mapList = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                foreach (Saxon.Hej.om.GroundedValue value in array.members()) {
                    mapList.Add(new Saxon.Hej.ma.map.SingleEntryMap(valueKey, value));
                }
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(mapList);
            }
        
        }
        public class MapAsSequenceOfMaps : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                System.Collections.Generic.IList<Saxon.Hej.om.Item> mapList = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                    Saxon.Hej.ma.map.DictionaryMap dictionary = new Saxon.Hej.ma.map.DictionaryMap(2);
                    dictionary.initialPut("key", pair.key);
                    dictionary.initialPut("value", pair.value);
                    mapList.Add(dictionary);
                }
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(mapList);
            }
        
        }
        public class YesNoBoolean : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string supplied = arguments[0].head().getStringValue();
                switch (Saxon.Hej.value.Whitespace.trim(supplied)) {
                  case "0":
                  case "no":
                  case "false":
                    return Saxon.Hej.value.BooleanValue.FALSE;
                  case "1":
                  case "yes":
                  case "true":
                    return Saxon.Hej.value.BooleanValue.TRUE;
                  default:
                    throw new Saxon.Hej.trans.XPathException("Supplied value for boolean argument must be 0|false|no or 1|true|yes", "FORG0001");
                }
            }
        
        }
    
    }
}