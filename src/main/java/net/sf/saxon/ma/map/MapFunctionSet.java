////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;


import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.*;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SequenceIteratorOverJavaIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Function signatures (and pointers to implementations) of the functions defined in the map
 * namespace in XPath 3.1 and 4.0
 */

public class MapFunctionSet extends BuiltInFunctionSet {

    private final static MapFunctionSet instance31 = new MapFunctionSet(31);
    private final static MapFunctionSet instance40 = new MapFunctionSet(40);

    private MapFunctionSet(int version) {
        init(version);
    }

    /**
     * Get the set of functions defined in the F&amp;O spec in the "map" namespace
     * @param version the XPath version (eg 31, 40). Currently any version less than 40
     *                is treated as 31, and any version greater than 40 is treated as 40.
     * @return the function library
     */

    public static MapFunctionSet getInstance(int version) {
        return version >= 40 ? instance40 : instance31;
    }

    public static final RecordTest KVP_TYPE_INEXTENSIBLE = RecordTest.nonExtensible(
            field("key", SequenceType.SINGLE_ATOMIC, false),
            field("value", SequenceType.ANY_SEQUENCE, false));

    public static final RecordTest KVP_TYPE_EXTENSIBLE = RecordTest.extensible(
            field("key", SequenceType.SINGLE_ATOMIC, false),
            field("value", SequenceType.ANY_SEQUENCE, false));

    public static OnDuplicatesAction getDuplicatesCombiner (
            MapItem options, String defaultAction, String duplicatesErrorCode) throws XPathException {
        GroundedValue duplicatesOption = options.get(new StringValue("duplicates"));
        if (duplicatesOption == null) {
            duplicatesOption = new StringValue(defaultAction);
        } else {
            duplicatesOption = duplicatesOption.head();
            if (duplicatesOption == null) {
                duplicatesOption = new StringValue(defaultAction);
            }
        }
        if (duplicatesOption instanceof StringValue) {
            String action = duplicatesOption.getStringValue();
            switch (action) {
                case "use-first":
                case "use-any":
                case "unspecified": // used in XSLT 3.0
                    return (x, y, cxt) -> x;
                case "use-last":
                    return (x, y, cxt) -> y;
                case "combine":
                    return (x, y, cxt) -> x.concatenate(y);
                case "reject":
                    return (x, y, cxt) -> {
                        throw new UncheckedXPathException("Duplicate key in map", duplicatesErrorCode);
                    };
                default:
                    throw new AssertionError();

            }
        }
        FunctionItem combineOption = (FunctionItem)duplicatesOption;
        return (x, y, cxt) -> {
            try {
                return combineOption.call(cxt, new Sequence[]{x, y}).materialize();
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
        };

        

    }


    private void init(int version) {

        register("merge", 1, e -> e.populate( MapMerge::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, STAR | INS, null));

        SpecificFunctionType ON_DUPLICATES_CALLBACK_TYPE = new SpecificFunctionType(
                SequenceType.ANY_SEQUENCE, SequenceType.ANY_SEQUENCE,
                SequenceType.ANY_SEQUENCE
        );

        EnumerationUnionType duplicatesKeywords =
                EnumerationUnionType.of("reject", "use-first", "use-last", "use-any", "combine", "unspecified");
        // duplicates=unspecified is retained because that's what the XSLT 3.0 Rec incorrectly uses

        SequenceType duplicatesOptionType = SequenceType.one(
                ChoiceItemType.of(ON_DUPLICATES_CALLBACK_TYPE, duplicatesKeywords));



        OptionsParameter mergeOptionDetails = new OptionsParameter(version);
        mergeOptionDetails.addAllowedOption("duplicates", duplicatesOptionType, StringValue.bmp("use-first"));
        // duplicates=unspecified is retained because that's what the XSLT 3.0 Rec incorrectly uses
        mergeOptionDetails.addAllowedOption("retain-order", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        mergeOptionDetails.addAllowedOption(MapMerge.errorCodeKey, SequenceType.SINGLE_STRING, StringValue.bmp("FOJS0003"));


        register("merge", 2, e -> e.populate( MapMerge::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, STAR, null)
                .arg(1, MapType.ANY_MAP_TYPE, version>=40 ? OPT : ONE, null)
                .setOptionDetails(mergeOptionDetails));

        register("put", 3, e -> e.populate(MapPut::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                .arg(2, AnyItemType.getInstance(), STAR | NAV, null));

        register("contains", 2, e -> e.populate(MapContains::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null));

        register("remove", 2, e -> e.populate(MapRemove::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, STAR | ABS, null));

        register("keys", 1, e -> e.populate(MapKeys::new, BuiltInAtomicType.ANY_ATOMIC, STAR, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

        register("size", 1, e -> e.populate(MapSize::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

        register("entry", 2, e -> e.populate( MapEntry::new, MapType.ANY_MAP_TYPE, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                .arg(1, AnyItemType.getInstance(), STAR | NAV, null));

        register("find", 2, e -> e.populate( MapFind::new, ArrayItemType.ANY_ARRAY_TYPE, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null));

        ItemType actionType = new SpecificFunctionType(
                SequenceType.SINGLE_ATOMIC, SequenceType.ANY_SEQUENCE,
                SequenceType.ANY_SEQUENCE);

        register("for-each", 2, e -> e.populate(MapForEach::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                .arg(1, actionType, ONE | INS, null));

        register("get", 2, e -> e.populate( MapGet::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null));



        //#if EE==true || PE==true
        if (version >= 40) {

            ItemType fallbackType = new SpecificFunctionType(
                    SequenceType.SINGLE_ATOMIC,
                    SequenceType.ANY_SEQUENCE);

            register("get", 3, e -> e.populate(MapGet::new, AnyItemType.getInstance(), STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                    .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null)
                    .arg(2, fallbackType, ONE | INS, null));

            SpecificFunctionType mapBuildKeyArg = new SpecificFunctionType(
                    SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                    SequenceType.ATOMIC_SEQUENCE);

            SpecificFunctionType mapBuildValueArg = new SpecificFunctionType(
                    SequenceType.SINGLE_ITEM, SequenceType.SINGLE_INTEGER,
                    SequenceType.ANY_SEQUENCE);

            register("build", 1, 4, e -> e.populate(MapBuild::new, MapType.ANY_MAP_TYPE, ONE, 0)
                    .arg(0, AnyItemType.getInstance(), STAR, null)
                    .arg(1, mapBuildKeyArg, ONE, null)
                    .arg(2, mapBuildValueArg, ONE, null)
                    .arg(3, MapType.ANY_MAP_TYPE, OPT, null)
                    .setOptionDetails(mergeOptionDetails));

            ItemType filterFunctionType = new SpecificFunctionType(
                    SequenceType.SINGLE_ATOMIC, SequenceType.ANY_SEQUENCE,
                    SequenceType.OPTIONAL_BOOLEAN);

            register("items", 1, e -> e.populate(MapItems::new, AnyItemType.getInstance(), STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

            register("keys-where", 2, e -> e.populate(MapKeysWhere::new, BuiltInAtomicType.ANY_ATOMIC, STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                    .arg(1, filterFunctionType, ONE, null));

            register("pair", 2, e -> e.populate(MapPair::new, KVP_TYPE_INEXTENSIBLE, ONE, 0)
                    .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE | INS, null)
                    .arg(1, AnyItemType.getInstance(), STAR | INS, null));

            register("pairs", 1, e -> e.populate(MapPairs::new, KVP_TYPE_INEXTENSIBLE, STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

            register("empty", 1, e -> e.populate(MapEmpty::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

            register("entries", 1, e -> e.populate(MapEntries::new, MapType.ANY_MAP_TYPE, STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

            register("of-pairs", 1, e -> e.populate(MapOfPairs::new, MapType.ANY_MAP_TYPE, ONE, 0)
                    .arg(0, KVP_TYPE_EXTENSIBLE, STAR | INS, null));

            register("of-pairs", 2, e -> e.populate(MapOfPairs::new, MapType.ANY_MAP_TYPE, ONE, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, STAR | INS, null)
                    .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                    .setOptionDetails(mergeOptionDetails));

            register("untyped-contains", 2, e -> e.populate(MapUntypedContains::new, BuiltInAtomicType.BOOLEAN, ONE, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                    .arg(1, BuiltInAtomicType.ANY_ATOMIC, ONE | ABS, null));

            // TODO: drop this, renamed map:items
            register("values", 1, e -> e.populate(MapItems::new, AnyItemType.getInstance(), STAR, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null));

            SpecificFunctionType FILTER_TYPE = new SpecificFunctionType(
                    SequenceType.SINGLE_ATOMIC, SequenceType.ANY_SEQUENCE,
                    SequenceType.OPTIONAL_BOOLEAN
            );

            register("filter", 2, e -> e.populate(MapFilter::new, MapType.ANY_MAP_TYPE, ONE, 0)
                    .arg(0, MapType.ANY_MAP_TYPE, ONE | INS, null)
                    .arg(1, FILTER_TYPE, ONE | INS, null));

        }
        //#endif

    }

    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.MAP_FUNCTIONS;
    }

    @Override
    public String getConventionalPrefix() {
        return "map";
    }

    @FunctionalInterface
    public interface OnDuplicatesAction {
        GroundedValue combine(GroundedValue existing, GroundedValue newValue, XPathContext context);
    }


    /**
     * Implementation of the XPath 3.1 function map:contains(Map, key) =&gt; boolean
     */
    public static class MapContains extends SystemFunction {

        @Override
        public BooleanValue call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            AtomicValue key = (AtomicValue) arguments[1].head();
            return BooleanValue.get(map.get(key) != null);
        }

    }

    /**
     * Implementation of the proposed XPath 4.0 function map:filter(Map, function(*)) =&gt; Map
     */
    public static class MapFilter extends SystemFunction {

        @Override
        public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            FunctionItem fn = (FunctionItem) arguments[1].head();
            GeneralMapBuilder result = new GeneralMapBuilder();
            for (KeyValuePair pair : map.keyValuePairs()) {
                BooleanValue match = (BooleanValue)dynamicCall(fn, context, pair.key, pair.value).head();
                if (match != null && match.getBooleanValue()) {
                    result.put(pair.key, pair.value);
                }
            }
            return result.getCompletedMapConfidently();
        }

    }

    /**
     * Implementation of the XPath 3.1 function map:get(Map, key) =&gt; value
     */
    public static class MapGet extends SystemFunction {

        String pendingWarning = null;

        /**
         * Method called during static type checking. This method may be implemented in subclasses so that functions
         * can take advantage of knowledge of the types of the arguments that will be supplied.
         *
         * @param visitor         an expression visitor, providing access to the static context and configuration
         * @param contextItemType information about whether the context item is set, and what its type is
         * @param arguments       the expressions appearing as arguments in the function call
         */
        @Override
        public void supplyTypeInformation(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType, Expression[] arguments) throws XPathException {
            ItemType it = arguments[0].getItemType();
            if (it instanceof RecordType && arguments.length == 2) {
                if (arguments[1] instanceof StringLiteral) {
                    String key = ((StringLiteral)arguments[1]).stringify();
                    if (((RecordType)it).getFieldType(key) == null) {
                        XPathException xe = new XPathException("Field " + key + " is not defined for tuple type " + it, "SXTT0001");
                        xe.setIsTypeError(true);
                        throw xe;
                    }
                }
                TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                Affinity relation = th.relationship(arguments[1].getItemType(), BuiltInAtomicType.STRING);
                if (relation == Affinity.DISJOINT) {
                    XPathException xe = new XPathException("Key for tuple type must be a string (actual type is " + arguments[1].getItemType(), "XPTY0004");
                    xe.setIsTypeError(true);
                    throw xe;
                }
            }
        }

        /**
         * Get the return type, given knowledge of the actual arguments
         *
         * @param args the actual arguments supplied
         * @return the best available item type that the function will return
         */
        @Override
        public ItemType getResultItemType(Expression[] args) {
            if (args.length == 2) {
                ItemType mapType = args[0].getItemType();
                if (mapType instanceof RecordTest && args[1] instanceof StringLiteral) {
                    String key = ((StringLiteral) args[1]).stringify();
                    RecordTest tit = (RecordTest) mapType;
                    SequenceType valueType = tit.getFieldType(key);
                    if (valueType == null) {
                        warning("Field " + key + " is not defined in record type");
                        return AnyItemType.getInstance();
                    } else {
                        return valueType.getPrimaryType();
                    }
                } else if (mapType instanceof MapType) {
                    return ((MapType) mapType).getValueType().getPrimaryType();
                }
            }
            return super.getResultItemType(args);
        }

        /**
         * Get the cardinality, given knowledge of the actual arguments
         *
         * @param args the actual arguments supplied
         * @return the most precise available cardinality that the function will return
         */
        @Override
        public int getCardinality(Expression[] args) {
            ItemType mapType = args[0].getItemType();
            if (mapType instanceof RecordTest && args[1] instanceof StringLiteral) {
                String key = ((StringLiteral) args[1]).stringify();
                RecordTest tit = (RecordTest) mapType;
                SequenceType valueType = tit.getFieldType(key);
                if (valueType == null) {
                    warning("Field " + key + " is not defined in record type");
                    return StaticProperty.ALLOWS_MANY;
                } else {
                    return valueType.getCardinality();
                }
            } else if (mapType instanceof MapType) {
                return Cardinality.union(
                        ((MapType) mapType).getValueType().getCardinality(),
                        StaticProperty.ALLOWS_ZERO);
            } else {
                return super.getCardinality(args);
            }
        }

        /**
         * Allow the function to create an optimized call based on the values of the actual arguments
         *
         * @param visitor     the expression visitor
         * @param contextInfo information about the context item
         * @param arguments   the supplied arguments to the function call. Note: modifying the contents
         *                    of this array should not be attempted, it is likely to have no effect.
         * @return either a function call on this function, or an expression that delivers
         * the same result, or null indicating that no optimization has taken place
         * @throws XPathException if an error is detected
         */
        @Override
        public Expression makeOptimizedFunctionCall(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments) throws XPathException {
            if (pendingWarning != null && !pendingWarning.equals("DONE")) {
                visitor.issueWarning(pendingWarning, SaxonErrorCode.SXWN9038, arguments[0].getLocation());
                pendingWarning = "DONE";
            }
            return null;
        }

        private void warning(String message) {
            if (!"DONE".equals(pendingWarning)) {
                pendingWarning = message;
            }
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            assert map != null;
            AtomicValue key = (AtomicValue) arguments[1].head();
            Sequence value = map.get(key);
            if (value == null) {
                if (arguments.length > 2) {
                    FunctionItem fn = (FunctionItem)arguments[2].head();
                    return dynamicCall(fn, context, key);
                } else {
                    return EmptySequence.getInstance();
                }
            } else {
                return value;
            }
        }

    }

    /**
     * Implementation of the XPath 3.1 function map:find(item()*, key) =&gt; array
     */
    public static class MapFind extends SystemFunction {

        @Override
        public ArrayItem call(XPathContext context, Sequence[] arguments) throws XPathException {
            List<GroundedValue> result = new ArrayList<>();
            AtomicValue key = (AtomicValue) arguments[1].head();
            processSequence(arguments[0], key, result);
            return new SimpleArrayItem(result);
        }

        private void processSequence(Sequence in, AtomicValue key, List<GroundedValue> result) {
            SequenceTool.supply(in.iterate(), (ItemConsumer<? super Item>) item -> {
                if (item instanceof ArrayItem) {
                    for (Sequence sequence : ((ArrayItem) item).members()) {
                        processSequence(sequence, key, result);
                    }
                } else if (item instanceof MapItem) {
                    GroundedValue value = ((MapItem) item).get(key);
                    if (value != null) {
                        result.add(value);
                    }
                    for (KeyValuePair entry : ((MapItem) item).keyValuePairs()) {
                        processSequence(entry.value, key, result);
                    }
                }
            });
        }

    }

    /**
     * Implementation of the function map:entry(key, value) =&gt; Map
     */
    public static class MapEntry extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            AtomicValue key = (AtomicValue) arguments[0].head();
            assert key != null;
            GroundedValue value = arguments[1].materialize();
            return new SingleEntryMap(key, value);
        }

        /**
         * Get the return type, given knowledge of the actual arguments
         *
         * @param args the actual arguments supplied
         * @return the best available item type that the function will return
         */
        @Override
        public ItemType getResultItemType(Expression[] args) {
            PlainType ku = args[0].getItemType().getAtomizedItemType();
            AtomicType ka;
            if (ku instanceof AtomicType) {
                ka = (AtomicType)ku;
            } else {
                ka = ku.getPrimitiveItemType();
            }
            return new MapType(ka,
                               SequenceType.makeSequenceType(args[1].getItemType(), args[1].getCardinality()));
        }

        @Override
        public String getStreamerName() {
            return "MapEntry";
        }

    }

    /**
     * Implementation of the function map:for-each(Map, Function) =&gt; item()*
     */
    public static class MapForEach extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            FunctionItem fn = (FunctionItem) arguments[1].head();
            ZenoSequence results = new ZenoSequence();
            for (KeyValuePair pair : map.keyValuePairs()) {
                Sequence seq = dynamicCall(fn, context, pair.key, pair.value);
                GroundedValue val = seq.materialize();
                if (val.getLength() > 0) {
                    results = results.appendSequence(val);
                }
            }
            return results;
        }
    }

    /**
     * Implementation of the proposed 4.0 function map:entries(Map) =&gt; map(*)*
     */
    public static class MapEntries extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            return new LazySequence(map.entries());
        }
    }

    /**
     * Implementation of the proposed 4.0 function map:pair(key, value) =&gt; record(key, value)
     */
    public static class MapPair extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            AtomicValue key = (AtomicValue) arguments[0].head();
            GroundedValue value = arguments[1].materialize();
            return new ShapedMap(Shape.KEY_VALUE_PAIR, key, value);
        }
    }

    /**
     * Implementation of the proposed 4.0 function map:empty(Map) =&gt; boolean
     */
    public static class MapEmpty extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            return BooleanValue.get(map.isEmpty());
        }
    }

    /**
     * Implementation of the proposed 4.0 function map:pairs(Map) =&gt; record(key, value)*
     */
    public static class MapPairs extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            Iterable<KeyValuePair> pairIterable = map.keyValuePairs();
            java.util.function.Function<KeyValuePair, Item> mapper = KVP -> KVP.asKeyValueRecord();
            SequenceIterator iter = new SequenceIteratorOverJavaIterator<KeyValuePair>(
                    pairIterable.iterator(), mapper);
            return new LazySequence(iter);
        }
    }

    /**
     * Implementation of the proposed 4.0 function map:items(Map) =&gt; item()*
     */
    public static class MapItems extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            ZenoSequence results = new ZenoSequence();
            for (KeyValuePair pair : map.keyValuePairs()) {
                results = results.appendSequence(pair.value);
            }
            return results;
        }
    }


    /**
     * Implementation of the proposed XPath 4.0 function
     * map:build($sequence, $key, $value, $options) =&gt; map(*)
     */
    public static class MapBuild extends SystemFunction {

        @Override
        public Expression makeFunctionCall(Expression... arguments) {
            Expression[] newArgs = new Expression[4];
            newArgs[0] = arguments[0];
            if (arguments.length < 2 || arguments[1] instanceof DefaultedArgumentExpression) {
                newArgs[1] = FunctionLiteral.makeLiteral(SystemFunction.makeFunction40("identity", getRetainedStaticContext(), 1));
            } else {
                newArgs[1] = arguments[1];
            }
            if (arguments.length < 3 || arguments[2] instanceof DefaultedArgumentExpression) {
                newArgs[2] = FunctionLiteral.makeLiteral(SystemFunction.makeFunction40("identity", getRetainedStaticContext(), 1));
            } else {
                newArgs[2] = arguments[2];
            }
            if (arguments.length < 4 || arguments[3] instanceof DefaultedArgumentExpression) {
                newArgs[3] = Literal.makeLiteral(EmptyMap.getInstance());
            } else {
                newArgs[3] = arguments[3];
            }
            setArity(4);
            return super.makeFunctionCall(newArgs);
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            GeneralMapBuilder mapBuilder = new GeneralMapBuilder();
            FunctionItem keyFunction = (FunctionItem)arguments[1].head();
            FunctionItem valueFunction = (FunctionItem) arguments[2].head();
            MapItem options = (MapItem)arguments[3].head();
            if (options != null) {
                getDetails().optionDetails.processSuppliedOptions(options, context);
            } else {
                options = EmptyMap.getInstance();
            }
            OnDuplicatesAction action = getDuplicatesCombiner(options, "combine", "FOJS0003");
            mapBuilder.setDuplicatesAction(action);
            
            SequenceIterator iter = arguments[0].iterate();
            int position = 1;
            for (Item item; (item = iter.next()) != null; ) {
                IntegerValue posValue = Int64Value.makeIntegerValue(position++);
                SequenceIterator keys = dynamicCall(keyFunction, context, item, posValue).iterate();
                AtomicValue key;
                GroundedValue value = dynamicCall(valueFunction, context, item, posValue).materialize();
                while ((key = (AtomicValue)keys.next()) != null) {
                    mapBuilder.put(key, value);
                }
            }
            return mapBuilder.getCompletedMap(context);
        }
    }

    /**
     * Implementation of the XPath 3.1 function map:keys(Map) =&gt; atomicValue*
     */
    public static class MapKeys extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            assert map != null;
            return SequenceTool.toLazySequence(map.keys());
        }
    }

    /**
     * Implementation of the XPath 4.0 function map:keys-where(Map, predicate) =&gt; atomicValue*
     */
    public static class MapKeysWhere extends SystemFunction {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            assert map != null;

            FunctionItem fn = (FunctionItem) arguments[1].head();
            ZenoSequence results = new ZenoSequence();
            for (KeyValuePair pair : map.keyValuePairs()) {
                BooleanValue selected = (BooleanValue) fn.call(context, new Sequence[]{pair.key, pair.value}).head();
                if (selected != null && selected.getBooleanValue()) {
                    results = results.append(pair.key);
                }
            }
            return results;

        }
    }

    /**
     * Implementation of the function map:merge() =&gt; Map
     * From 9.8, map:merge is also used to implement map constructors in XPath and the xsl:map
     * instruction in XSLT. For this purpose it accepts an additional option to define the error
     * code to be used to signal duplicates.
     */
    public static class MapMerge extends SystemFunction {

        //public final static String finalKey = "Q{" + NamespaceConstant.SAXON + "}final";
        //public final static String keyTypeKey ="Q{"+NamespaceConstant.SAXON +"}key-type";
        public final static String errorCodeKey = "Q{" + NamespaceConstant.SAXON + "}duplicates-error-code";

        private final String duplicates = "use-first";
        private final String duplicatesErrorCode = "FOJS0003";


        /**
         * Get the return type, given knowledge of the actual arguments
         *
         * @param args the actual arguments supplied
         * @return the best available item type that the function will return
         */
        @Override
        public ItemType getResultItemType(Expression[] args) {
            ItemType it = args[0].getItemType();
            if (it == ErrorType.getInstance()) {
                return MapType.EMPTY_MAP_TYPE;
            } else if (it instanceof MapType) {
                boolean maybeCombined = true;  // see bug 3980
                if (args.length == 1) {
                    maybeCombined = false;
                } else if (args[1] instanceof Literal) {
                    MapItem options = (MapItem) ((Literal) args[1]).getGroundedValue().head();
                    if (options != null) {
                        GroundedValue dupes = options.get(StringValue.bmp("duplicates"));
                        try {
                            if (dupes != null && !"combine".equals(dupes.getStringValue())) {
                                maybeCombined = false;
                            }
                        } catch (XPathException e) {
                            //
                        }
                    }
                }
                if (maybeCombined) {
                    return new MapType(((MapType) it).getKeyType(),
                                       SequenceType.zeroOrMore(((MapType) it).getValueType().getPrimaryType()));
                } else {
                    return it;
                }
            } else {
                return super.getResultItemType(args);
            }
        }


        @Override
        public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                String duplicatesErrorCode = this.duplicatesErrorCode;
                MapFunctionSet.OnDuplicatesAction action = null;
                if (arguments.length > 1) {
                    MapItem options = (MapItem) arguments[1].head();
                    if (options != null && !options.isEmpty()) {
                        Map<String, GroundedValue> values = getDetails().optionDetails.processSuppliedOptions(options, context);
                        duplicatesErrorCode = ((StringValue) values.get(errorCodeKey)).getStringValue();
                        action = getDuplicatesCombiner(options, "use-first", duplicatesErrorCode);
                    }
                }
                if (action == null) {
                    action = getDuplicatesCombiner(EmptyMap.getInstance(), "use-first", "FOJS0003");
                }

                SequenceIterator iter = arguments[0].iterate();
                return mergeMaps(iter, context, action);

            } catch (UncheckedXPathException e) {
                throw e.getXPathException();
            }

        }

        /**
         * Merge a sequence of maps into a single map
         *
         * @param iter         iterator over the input maps
         * @param context      The XPath dynamic context
         * @param onDuplicates callback to be used when duplicates = "use-callback"
         * @return the merged map
         * @throws XPathException if any error occurs, including detection of disallowed duplicates
         */
        public static MapItem mergeMaps(SequenceIterator iter, XPathContext context,
                                        OnDuplicatesAction onDuplicates)
                throws XPathException {
            //MapBuilder mapBuilder = context.getConfiguration().makeMapBuilder(retainOrder, false);
            GeneralMapBuilder builder = new GeneralMapBuilder();
            builder.setDuplicatesAction(onDuplicates);
            MapItem next;
            while ((next = (MapItem) iter.next()) != null) {
                for (KeyValuePair kvp : next.keyValuePairs()) {
                    builder.put(kvp.key, kvp.value);
                }
            }
            return builder.getCompletedMap(context);
        }


        @Override
        public String getStreamerName() {
            return "NewMap";
        }

        /**
         * Export any implicit arguments held in optimized form within the SystemFunction call
         *
         * @param out the export destination
         */
        @Override
        public void exportAdditionalArguments(SystemFunctionCall call, ExpressionPresenter out) throws XPathException {
            if (call.getArity() == 1) {
                StringMapBuilder options = new StringMapBuilder(2).unordered();
                options.setOrdered(false);
                options.put(new Twine8("duplicates"), new StringValue(duplicates));
                options.put(new Twine8("Q{" + NamespaceUri.SAXON + "}duplicates-error-code"), new StringValue(duplicatesErrorCode));
                Literal.exportValue(options.getCompletedMap(), out);
            }
        }
    }

    /**
     * Implementation of the function map:of-pairs() =&gt; Map
     */
    public static class MapOfPairs extends SystemFunction {


        @Override
        public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
            GeneralMapBuilder builder = new GeneralMapBuilder();
            MapItem options = EmptyMap.getInstance();
            if (arguments.length > 1) {
                options = (MapItem) arguments[1].head();
                if (options != null) {
                    getDetails().optionDetails.processSuppliedOptions(options, context);
                }
            }
            OnDuplicatesAction action = getDuplicatesCombiner(options, "combine", "FOJS0003");
            builder.setDuplicatesAction(action);

            SequenceIterator iter = arguments[0].iterate();
            StringValue keyKey = new StringValue("key");
            StringValue valueKey = new StringValue("value");
            for (Item item; (item = iter.next()) != null; ) {
                AtomicValue key = (AtomicValue) ((MapItem) item).get(keyKey);
                GroundedValue suppliedValue = ((MapItem) item).get(valueKey);
                builder.put(key, suppliedValue);
            }
            return builder.getCompletedMap(context);
        }
    }

    /**
     * Implementation of the extension function map:put() =&gt; Map
     */

    public static class MapPut extends SystemFunction {

        @Override
        public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {

            MapItem baseMap = (MapItem) arguments[0].head();
            AtomicValue key = (AtomicValue) arguments[1].head();
            GroundedValue value = arguments[2].materialize();
            return baseMap.put(key, value);
        }
    }


    /**
     * Implementation of the XPath 3.1 function map:remove(Map, key) =&gt; value
     */
    public static class MapRemove extends SystemFunction {

        @Override
        public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            SequenceIterator iter = arguments[1].iterate();
            AtomicValue key;
            while ((key = (AtomicValue) iter.next()) != null) {
                map = map.remove(key);
            }
            return map;
        }
    }

    /**
     * Implementation of the extension function map:size(map) =&gt; integer
     */
    public static class MapSize extends SystemFunction {

        @Override
        public IntegerValue call(XPathContext context, Sequence[] arguments) throws XPathException {
            MapItem map = (MapItem) arguments[0].head();
            return new Int64Value(map.size());
        }
    }

}
