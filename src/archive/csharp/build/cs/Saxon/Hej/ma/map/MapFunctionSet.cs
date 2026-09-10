using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class MapFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        public static Saxon.Hej.ma.map.MapFunctionSet THE_INSTANCE = new Saxon.Hej.ma.map.MapFunctionSet();
        public MapFunctionSet()  {
            init();
        }
        public static Saxon.Hej.ma.map.MapFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private void init() {
            register("merge", 1, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapMerge), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (STAR|INS), null);
            Saxon.Hej.type.SpecificFunctionType ON_DUPLICATES_CALLBACK_TYPE = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE, Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            Saxon.Hej.value.SequenceType oneOnDuplicatesFunction = Saxon.Hej.value.SequenceType.makeSequenceType(ON_DUPLICATES_CALLBACK_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            Saxon.Hej.functions.OptionsParameter mergeOptionDetails = new Saxon.Hej.functions.OptionsParameter();
            mergeOptionDetails.addAllowedOption("duplicates", Saxon.Hej.value.SequenceType.SINGLE_STRING, Saxon.Hej.value.StringValue.bmp("use-first"));
            mergeOptionDetails.setAllowedValues("duplicates", "FOJS0005", "use-first", "use-last", "combine", "reject", "unspecified", "use-any", "use-callback");
            mergeOptionDetails.addAllowedOption(Saxon.Hej.ma.map.MapFunctionSet.MapMerge.errorCodeKey, Saxon.Hej.value.SequenceType.SINGLE_STRING, Saxon.Hej.value.StringValue.bmp("FOJS0003"));
            mergeOptionDetails.addAllowedOption(Saxon.Hej.ma.map.MapFunctionSet.MapMerge.keyTypeKey, Saxon.Hej.value.SequenceType.SINGLE_STRING, Saxon.Hej.value.StringValue.bmp("anyAtomicType"));
            mergeOptionDetails.addAllowedOption(Saxon.Hej.ma.map.MapFunctionSet.MapMerge.finalKey, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            mergeOptionDetails.addAllowedOption(Saxon.Hej.ma.map.MapFunctionSet.MapMerge.onDuplicatesKey, oneOnDuplicatesFunction, null);
            register("merge", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapMerge), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null).setOptionDetails(mergeOptionDetails);
            register("entry", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapEntry), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null);
            register("find", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapFind), Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null);
            register("get", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapGet), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null);
            Saxon.Hej.type.SpecificFunctionType indexArg = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
            register("index", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapIndex), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, indexArg, ONE, null);
            register("put", 3, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapPut), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null);
            register("contains", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapContains), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null);
            register("remove", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapRemove), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (STAR|ABS), null);
            register("keys", 1, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapKeys), Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null);
            register("size", 1, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapSize), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null);
            Saxon.Hej.type.ItemType actionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            register("for-each", 2, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapForEach), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, actionType, (ONE|INS), null);
            register("entries", 1, typeof(Saxon.Hej.ma.map.MapFunctionSet.MapEntries), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, STAR, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null);
            register("untyped-contains", 2, typeof(Saxon.Hej.ma.map.MapUntypedContains), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, 0).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, (ONE|ABS), null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.MAP_FUNCTIONS;
        }
        public override string getConventionalPrefix() {
            return "map";
        }
        public class MapContains : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                return Saxon.Hej.value.BooleanValue.get((map.get(key) != null));
            }
        
        }
        public class MapGet : Saxon.Hej.functions.SystemFunction {
            internal string pendingWarning = null;
            public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
                Saxon.Hej.type.ItemType it = arguments[0].getItemType();
                if ((it is Saxon.Hej.ma.map.TupleType)) {
                    if ((arguments[1] is Saxon.Hej.expr.StringLiteral)) {
                        string key = ((Saxon.Hej.expr.StringLiteral)(arguments[1])).stringify();
                        if ((((Saxon.Hej.ma.map.TupleType)(it)).getFieldType(key) == null)) {
                            Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(((("Field "+key)+" is not defined for tuple type ")+it), "SXTT0001");
                            xe.setIsTypeError(true);
                            throw xe;
                        }
                    }
                    Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                    Saxon.Hej.type.Affinity relation = th.relationship(arguments[1].getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING);
                    if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                        Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(("Key for tuple type must be a string (actual type is "+arguments[1].getItemType()), "XPTY0004");
                        xe.setIsTypeError(true);
                        throw xe;
                    }
                }
            }
            public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
                Saxon.Hej.type.ItemType mapType = args[0].getItemType();
                if (((mapType is Saxon.Hej.ma.map.RecordTest) && (args[1] is Saxon.Hej.expr.StringLiteral))) {
                    string key = ((Saxon.Hej.expr.StringLiteral)(args[1])).stringify();
                    Saxon.Hej.ma.map.RecordTest tit = (Saxon.Hej.ma.map.RecordTest)(mapType);
                    Saxon.Hej.value.SequenceType valueType = tit.getFieldType(key);
                    if ((valueType == null)) {
                        warning((("Field "+key)+" is not defined in tuple type"));
                        return Saxon.Hej.type.AnyItemType.getInstance();
                    } else {
                        return valueType.getPrimaryType();
                    }
                } else {
                    if ((mapType is Saxon.Hej.ma.map.MapType)) {
                        return ((Saxon.Hej.ma.map.MapType)(mapType)).getValueType().getPrimaryType();
                    } else {
                        return base.getResultItemType(args);
                    }
                }
            }
            public override int getCardinality(Saxon.Hej.expr.Expression[] args) {
                Saxon.Hej.type.ItemType mapType = args[0].getItemType();
                if (((mapType is Saxon.Hej.ma.map.RecordTest) && (args[1] is Saxon.Hej.expr.StringLiteral))) {
                    string key = ((Saxon.Hej.expr.StringLiteral)(args[1])).stringify();
                    Saxon.Hej.ma.map.RecordTest tit = (Saxon.Hej.ma.map.RecordTest)(mapType);
                    Saxon.Hej.value.SequenceType valueType = tit.getFieldType(key);
                    if ((valueType == null)) {
                        warning((("Field "+key)+" is not defined in tuple type"));
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_MANY;
                    } else {
                        return valueType.getCardinality();
                    }
                } else {
                    if ((mapType is Saxon.Hej.ma.map.MapType)) {
                        return Saxon.Hej.value.Cardinality.union(((Saxon.Hej.ma.map.MapType)(mapType)).getValueType().getCardinality(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
                    } else {
                        return base.getCardinality(args);
                    }
                }
            }
            public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
                if (((pendingWarning != null) && !(pendingWarning.Equals("DONE")))) {
                    visitor.issueWarning(pendingWarning, arguments[0].getLocation());
                    pendingWarning = "DONE";
                }
                return null;
            }
            private void warning(string message) {
                if (!("DONE".Equals(pendingWarning))) {
                    pendingWarning = message;
                }
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((map != null));
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                Saxon.Hej.om.Sequence value = map.get(key);
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return value;
                }
            }
        
        }
        public class MapFind : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> result = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                processSequence(arguments[0], key, result);
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(result);
            }
            private void processSequence(Saxon.Hej.om.Sequence @in, Saxon.Hej.value.AtomicValue key, System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> result) {
                Saxon.Hej.om.SequenceTool.supply(@in.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        foreach (Saxon.Hej.om.Sequence sequence in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                            processSequence(sequence, key, result);
                        }
                    } else {
                        if ((item is Saxon.Hej.ma.map.MapItem)) {
                            Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.ma.map.MapItem)(item)).get(key);
                            if ((value != null)) {
                                result.Add(value);
                            }
                            foreach (Saxon.Hej.ma.map.KeyValuePair entry in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                                processSequence(entry.value, key, result);
                            }
                        }
                    }
                } )));
            }
        
        }
        public class MapEntry : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
                System.Diagnostics.Debug.Assert((key != null));
                Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(arguments[1].iterate());
                return new Saxon.Hej.ma.map.SingleEntryMap(key, value);
            }
            public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
                Saxon.Hej.type.PlainType ku = args[0].getItemType().getAtomizedItemType();
                Saxon.Hej.type.AtomicType ka;
                if ((ku is Saxon.Hej.type.AtomicType)) {
                    ka = (Saxon.Hej.type.AtomicType)(ku);
                } else {
                    ka = ((/*1430*/Saxon.Hej.type.AtomicType)ku.getPrimitiveItemType());
                }
                return new Saxon.Hej.ma.map.MapType(ka, Saxon.Hej.value.SequenceType.makeSequenceType(args[1].getItemType(), args[1].getCardinality()));
            }
            public override string getStreamerName() {
                return "MapEntry";
            }
        
        }
        public class MapForEach : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[1].head());
                Saxon.Hej.ma.zeno.ZenoSequence results = new Saxon.Hej.ma.zeno.ZenoSequence();
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                    Saxon.Hej.om.Sequence seq = dynamicCall(fn, context, new Saxon.Hej.om.Sequence[]{pair.key, pair.value});
                    Saxon.Hej.om.GroundedValue val = seq.materialize();
                    if ((val.getLength()>0)) {
                        results.appendSequence(val);
                    }
                }
                return results;
            }
        
        }
        public class MapEntries : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                Saxon.Hej.ma.zeno.ZenoSequence results = new Saxon.Hej.ma.zeno.ZenoSequence();
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                    Saxon.Hej.ma.map.DictionaryMap kvp = new Saxon.Hej.ma.map.DictionaryMap(2);
                    kvp.initialPut("key", pair.key);
                    kvp.initialPut("value", pair.value);
                    results.appendSequence(kvp);
                }
                return results;
            }
        
        }
        public class MapIndex : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Impl.DataModel.ImmMap map = new Saxon.Impl.DataModel.ImmMap();
                Saxon.Hej.om.Function function = (Saxon.Hej.om.Function)(arguments[1]);
                Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
                Saxon.Hej.om.Item item;
                Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[1];
                while (((item = iter.next()) != null)) {
                    args[0] = item;
                    Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(dynamicCall(function, context, args));
                    map.initialPut(key, item);
                }
                return map;
            }
        
        }
        public class MapKeys : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((map != null));
                return Saxon.Hej.om.SequenceTool.toLazySequence(map.keys());
            }
        
        }
        public class MapMerge : Saxon.Hej.functions.SystemFunction {
            public readonly static string finalKey = (("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}final");
            public readonly static string keyTypeKey = (("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}key-type");
            public readonly static string onDuplicatesKey = (("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}on-duplicates");
            public readonly static string errorCodeKey = (("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}duplicates-error-code");
            private string duplicates = "use-first";
            private string duplicatesErrorCode = "FOJS0003";
            private Saxon.Hej.om.Function onDuplicates = null;
            private bool allStringKeys = false;
            private bool treatAsFinal = false;
            public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
                if (((arguments.Length == 2) && (arguments[1] is Saxon.Hej.expr.Literal))) {
                    Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(((Saxon.Hej.expr.Literal)(arguments[1])).getGroundedValue().head());
                    System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> values = getDetails().optionDetails.processSuppliedOptions(options, visitor.getStaticContext().makeEarlyEvaluationContext());
                    string duplicates = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, "duplicates"))).getStringValue();
                    string duplicatesErrorCode = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, errorCodeKey))).getStringValue();
                    Saxon.Hej.om.Function onDuplicates = (Saxon.Hej.om.Function)(Saxon.Impl.Helpers.MapUtils.Get(values, onDuplicatesKey));
                    if ((onDuplicates != null)) {
                        duplicates = "use-callback";
                    }
                    bool isFinal = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(values, finalKey))).getBooleanValue();
                    string keyType = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, keyTypeKey))).getStringValue();
                    Saxon.Hej.ma.map.MapFunctionSet.MapMerge mm2 = (Saxon.Hej.ma.map.MapFunctionSet.MapMerge)(Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("merge", 1));
                    mm2.duplicates = duplicates;
                    mm2.duplicatesErrorCode = duplicatesErrorCode;
                    mm2.onDuplicates = onDuplicates;
                    mm2.allStringKeys = keyType.Equals("string");
                    mm2.treatAsFinal = isFinal;
                    return mm2.makeFunctionCall(arguments[0]);
                }
                return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
            }
            public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
                Saxon.Hej.type.ItemType it = args[0].getItemType();
                if ((it == Saxon.Hej.type.ErrorType.getInstance())) {
                    return Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE;
                } else {
                    if ((it is Saxon.Hej.ma.map.MapType)) {
                        bool maybeCombined = true;
                        if ((args.Length == 1)) {
                            maybeCombined = false;
                        } else {
                            if ((args[1] is Saxon.Hej.expr.Literal)) {
                                Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(((Saxon.Hej.expr.Literal)(args[1])).getGroundedValue().head());
                                Saxon.Hej.om.GroundedValue dupes = options.get(Saxon.Hej.value.StringValue.bmp("duplicates"));
                                try {
                                    if (((dupes != null) && !("combine".Equals(dupes.getStringValue())))) {
                                        maybeCombined = false;
                                    }
                                } catch (Saxon.Hej.trans.XPathException e) {
                                }
                            }
                        }
                        if (maybeCombined) {
                            return new Saxon.Hej.ma.map.MapType(((Saxon.Hej.ma.map.MapType)(it)).getKeyType(), Saxon.Hej.value.SequenceType.makeSequenceType(((Saxon.Hej.ma.map.MapType)(it)).getValueType().getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE));
                        } else {
                            return it;
                        }
                    } else {
                        return base.getResultItemType(args);
                    }
                }
            }
            public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string duplicates = this.duplicates;
                string duplicatesErrorCode = this.duplicatesErrorCode;
                bool allStringKeys = this.allStringKeys;
                bool treatAsFinal = this.treatAsFinal;
                Saxon.Hej.om.Function onDuplicates = this.onDuplicates;
                if ((arguments.Length>1)) {
                    Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                    System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> values = getDetails().optionDetails.processSuppliedOptions(options, context);
                    duplicates = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, "duplicates"))).getStringValue();
                    duplicatesErrorCode = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, errorCodeKey))).getStringValue();
                    treatAsFinal = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(values, finalKey))).getBooleanValue();
                    allStringKeys = "string".Equals(((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(values, keyTypeKey))).getStringValue());
                    onDuplicates = (Saxon.Hej.om.Function)(Saxon.Impl.Helpers.MapUtils.Get(values, onDuplicatesKey));
                    if ((onDuplicates != null)) {
                        duplicates = "use-callback";
                    }
                }
                if ((treatAsFinal && allStringKeys)) {
                    Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
                    Saxon.Hej.ma.map.DictionaryMap baseMap = new Saxon.Hej.ma.map.DictionaryMap();
                    Saxon.Hej.ma.map.MapItem next;
                    switch (duplicates) {
                      case "unspecified":
                      case "use-any":
                      case "use-last":
                        while (((next = (Saxon.Hej.ma.map.MapItem)(iter.next())) != null)) {
                            foreach (Saxon.Hej.ma.map.KeyValuePair pair in next.keyValuePairs()) {
                                if (!(((pair.key is Saxon.Hej.value.StringValue)))) {
                                    throw new Saxon.Hej.trans.XPathException((("The keys in this map must all be strings (found "+pair.key.getItemType())+")"));
                                }
                                baseMap.initialPut(pair.key.getStringValue(), pair.value);
                            }
                        }
                        return baseMap;
                      default:
                        while (((next = (Saxon.Hej.ma.map.MapItem)(iter.next())) != null)) {
                            foreach (Saxon.Hej.ma.map.KeyValuePair pair in next.keyValuePairs()) {
                                if (!(((pair.key is Saxon.Hej.value.StringValue)))) {
                                    throw new Saxon.Hej.trans.XPathException((("The keys in this map must all be strings (found "+pair.key.getItemType())+")"));
                                }
                                Saxon.Hej.om.Sequence existing = baseMap.get(pair.key);
                                if ((existing != null)) {
                                    switch (duplicates) {
                                      case "use-first":
                                        break ;
                                      case "combine":
                                        Saxon.Hej.functions.InsertBefore.InsertIterator combinedIter = new Saxon.Hej.functions.InsertBefore.InsertIterator(pair.value.iterate(), existing.iterate(), 1);
                                        Saxon.Hej.om.GroundedValue combinedValue = Saxon.Hej.om.SequenceTool.toGroundedValue(combinedIter);
                                        baseMap.initialPut(pair.key.getStringValue(), combinedValue);
                                        break ;
                                      case "use-callback":
                                        Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[]{existing, pair.value};
                                        Saxon.Hej.om.Sequence combined = onDuplicates.call(context, args);
                                        baseMap.initialPut(pair.key.getStringValue(), combined.materialize());
                                        break ;
                                      default:
                                        throw new Saxon.Hej.trans.XPathException(("Duplicate key in constructed map: "+Saxon.Hej.trans.Err.wrap(pair.key.getStringValue())), duplicatesErrorCode);
                                    }
                                } else {
                                    baseMap.initialPut(pair.key.getStringValue(), pair.value);
                                }
                            }
                        }
                        return baseMap;
                    }
                } else {
                    Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
                    Saxon.Hej.ma.map.MapItem baseMap = (Saxon.Hej.ma.map.MapItem)(iter.next());
                    if ((baseMap == null)) {
                        return new Saxon.Impl.DataModel.ImmMap();
                    } else {
                        Saxon.Hej.ma.map.MapItem next;
                        while (((next = (Saxon.Hej.ma.map.MapItem)(iter.next())) != null)) {
                            bool inverse = (next.size()>baseMap.size());
                            Saxon.Hej.ma.map.MapItem larger = (inverse ? next : baseMap);
                            Saxon.Hej.ma.map.MapItem smaller = (inverse ? baseMap : next);
                            string dup = (inverse ? invertDuplicates(duplicates) : duplicates);
                            foreach (Saxon.Hej.ma.map.KeyValuePair pair in smaller.keyValuePairs()) {
                                Saxon.Hej.om.Sequence existing = larger.get(pair.key);
                                if ((existing != null)) {
                                    switch (dup) {
                                      case "use-first":
                                      case "unspecified":
                                      case "use-any":
                                        break ;
                                      case "use-last":
                                        larger = larger.addEntry(pair.key, pair.value);
                                        break ;
                                      case "combine":
                                        {
                                            Saxon.Hej.functions.InsertBefore.InsertIterator combinedIter = new Saxon.Hej.functions.InsertBefore.InsertIterator(pair.value.iterate(), existing.iterate(), 1);
                                            Saxon.Hej.om.GroundedValue combinedValue = Saxon.Hej.om.SequenceTool.toGroundedValue(combinedIter);
                                            larger = larger.addEntry(pair.key, combinedValue);
                                            break ;
                                        }
                                      case "combine-reverse":
                                        {
                                            Saxon.Hej.functions.InsertBefore.InsertIterator combinedIter = new Saxon.Hej.functions.InsertBefore.InsertIterator(existing.iterate(), pair.value.iterate(), 1);
                                            Saxon.Hej.om.GroundedValue combinedValue = Saxon.Hej.om.SequenceTool.toGroundedValue(combinedIter);
                                            larger = larger.addEntry(pair.key, combinedValue);
                                            break ;
                                        }
                                      case "use-callback":
                                        System.Diagnostics.Debug.Assert((onDuplicates != null));
                                        Saxon.Hej.om.Sequence[] args;
                                        if (inverse) {
                                            args = ((onDuplicates.getArity() == 2) ? new Saxon.Hej.om.Sequence[]{pair.value, existing} : new Saxon.Hej.om.Sequence[]{pair.value, existing, pair.key});
                                        } else {
                                            args = ((onDuplicates.getArity() == 2) ? new Saxon.Hej.om.Sequence[]{existing, pair.value} : new Saxon.Hej.om.Sequence[]{existing, pair.value, pair.key});
                                        }
                                        Saxon.Hej.om.Sequence combined = onDuplicates.call(context, args);
                                        larger = larger.addEntry(pair.key, combined.materialize());
                                        break ;
                                      default:
                                        throw new Saxon.Hej.trans.XPathException(("Duplicate key in constructed map: "+Saxon.Hej.trans.Err.wrap(pair.key.getStringValue())), duplicatesErrorCode);
                                    }
                                } else {
                                    larger = larger.addEntry(pair.key, pair.value);
                                }
                            }
                            baseMap = larger;
                        }
                        return baseMap;
                    }
                }
            }
            private string invertDuplicates(string duplicates) {
                switch (duplicates) {
                  case "use-first":
                  case "unspecified":
                  case "use-any":
                    return "use-last";
                  case "use-last":
                    return "use-first";
                  case "combine":
                    return "combine-reverse";
                  case "combine-reverse":
                    return "combine";
                  default:
                    return duplicates;
                }
            }
            public override string getStreamerName() {
                return "NewMap";
            }
            public override void exportAdditionalArguments(Saxon.Hej.expr.SystemFunctionCall call, Saxon.Hej.trace.ExpressionPresenter @out) {
                if ((call.getArity() == 1)) {
                    Saxon.Impl.DataModel.ImmMap options = new Saxon.Impl.DataModel.ImmMap();
                    options.initialPut(Saxon.Hej.value.StringValue.bmp("duplicates"), new Saxon.Hej.value.StringValue(duplicates));
                    options.initialPut(Saxon.Hej.value.StringValue.bmp("duplicates-error-code"), new Saxon.Hej.value.StringValue(duplicatesErrorCode));
                    Saxon.Hej.expr.Literal.exportValue(options, @out);
                }
            }
        
        }
        public class MapPut : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem baseMap = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                if (!(((baseMap is Saxon.Impl.DataModel.ImmMap)))) {
                    baseMap = Saxon.Impl.DataModel.ImmMap.copy(baseMap);
                }
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                Saxon.Hej.om.GroundedValue value = arguments[2].materialize();
                return baseMap.addEntry(key, value);
            }
        
        }
        public class MapRemove : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                Saxon.Hej.om.SequenceIterator iter = arguments[1].iterate();
                Saxon.Hej.value.AtomicValue key;
                while (((key = (Saxon.Hej.value.AtomicValue)(iter.next())) != null)) {
                    map = map.remove(key);
                }
                return map;
            }
        
        }
        public class MapSize : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
                return new Saxon.Hej.value.Int64Value(map.size());
            }
        
        }
    
    }
}