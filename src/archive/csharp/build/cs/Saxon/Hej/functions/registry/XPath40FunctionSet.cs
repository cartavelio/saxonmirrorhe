using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class XPath40FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.XPath40FunctionSet THE_INSTANCE = new Saxon.Hej.functions.registry.XPath40FunctionSet();
        public static Saxon.Hej.functions.registry.XPath40FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private XPath40FunctionSet()  {
            init();
        }
        private void init() {
            importFunctionSet(Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance());
            Saxon.Hej.type.SpecificFunctionType predicate = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            Saxon.Hej.type.SpecificFunctionType ft = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ITEM}, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
            register("all", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (AS_ARG0|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, predicate, ONE, null);
            register("characters", 1, typeof(Saxon.Eej.functions.extfn.CharactersFn), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("codepoints-to-string", 1, typeof(Saxon.Hej.functions.CodepointsToString), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, SEQV).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, null);
            register("concat", 1, typeof(Saxon.Hej.functions.Concat), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, SEQV).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, null);
            register("highest", 1, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("highest", 2, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("highest", 3, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, ft, ONE, null);
            register("identity", 1, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (AS_ARG0|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("index-where", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, ONE, null);
            register("in-scope-namespaces", 1, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, LATE).arg(0, Saxon.Hej.pattern.NodeKindTest.ELEMENT, STAR, null);
            register("is-NaN", 1, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, ONE, null);
            register("items-before", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, ONE, null);
            register("items-until", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, ONE, null);
            register("items-from", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, ONE, null);
            register("items-after", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, ONE, null);
            register("lowest", 1, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("lowest", 2, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("lowest", 3, typeof(Saxon.Hej.functions.registry.XPath40FunctionSet.HighestOrLowest), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, ft, ONE, null);
            register("parcel", 1, typeof(Saxon.Hej.functions.ParcelFn), Saxon.Hej.ma.Parcel.TYPE, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("parts", 1, typeof(Saxon.Hej.functions.Parts), new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.ANY_SEQUENCE), ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, ONE, null);
            register("range", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, OPT, null);
            register("range", 3, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, OPT, null).arg(2, predicate, OPT, null);
            register("range", 4, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(1, predicate, OPT, null).arg(2, predicate, OPT, null).arg(3, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null);
            register("slice", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (AS_ARG0|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null);
            register("some", 2, typeof(Saxon.Hej.functions.registry.SefFunction), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (AS_ARG0|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY).arg(1, predicate, ONE, null);
            register("unparcel", 1, typeof(Saxon.Hej.functions.UnparcelFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.Parcel.TYPE, ONE, null);
        }
        public class HighestOrLowest : Saxon.Hej.functions.registry.SefFunction {
            public HighestOrLowest() : base () {
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Sequence input = arguments[0];
                string collationUri = null;
                if ((arguments.Length>=2)) {
                    Saxon.Hej.value.StringValue val = (Saxon.Hej.value.StringValue)(arguments[1].head());
                    if ((val != null)) {
                        try {
                            collationUri = Saxon.Hej.functions.ResolveURI.makeAbsolute(val.getStringValue(), getStaticBaseUriString()).ToString();
                        } catch (System.UriFormatException e) {
                            throw new Saxon.Hej.trans.XPathException(e);
                        }
                    }
                }
                if ((collationUri == null)) {
                    collationUri = getRetainedStaticContext().getDefaultCollationName();
                }
                Saxon.Hej.om.Function key;
                if ((arguments.Length>=3)) {
                    key = (Saxon.Hej.om.Function)(arguments[2].head());
                } else {
                    key = Saxon.Hej.functions.SystemFunction.makeFunction("data", getRetainedStaticContext(), 1);
                }
                Saxon.Hej.trans.SymbolicName.F @delegate = new Saxon.Hej.trans.SymbolicName.F(new Saxon.Hej.om.StructuredQName("", "http://ns.saxonica.com/xpath-functions", "mhk_highest"), 4);
                Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[4];
                args[0] = input.materialize();
                args[1] = new Saxon.Hej.value.StringValue(collationUri);
                args[2] = key;
                args[3] = Saxon.Hej.value.BooleanValue.get(getDetails().name.getLocalPart().Equals("highest"));
                return callFunction(context, @delegate, args);
            }
        
        }
    
    }
}