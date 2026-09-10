using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public class ArrayFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        public static Saxon.Hej.ma.arrays.ArrayFunctionSet THE_INSTANCE = new Saxon.Hej.ma.arrays.ArrayFunctionSet();
        public ArrayFunctionSet()  {
            init();
        }
        public static Saxon.Hej.ma.arrays.ArrayFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private void init() {
            register("append", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayAppend), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null);
            Saxon.Hej.type.ItemType filterFunctionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            register("filter", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayFilter), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, filterFunctionType, (ONE|INS), null);
            register("flatten", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayFlatten), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|ABS), null);
            Saxon.Hej.type.ItemType foldFunctionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE, Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            register("fold-left", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayFoldLeft), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null).arg(2, foldFunctionType, (ONE|INS), null);
            register("fold-right", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayFoldRight), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null).arg(2, foldFunctionType, (ONE|INS), null);
            Saxon.Hej.type.ItemType forEachFunctionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            register("for-each", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayForEach), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, forEachFunctionType, (ONE|INS), null);
            register("for-each-pair", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayForEachPair), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(2, foldFunctionType, (ONE|INS), null);
            register("get", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayGet), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (ONE|ABS), null);
            register("head", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayHead), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            register("insert-before", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayInsertBefore), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (STAR|ABS), null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null);
            register("join", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayJoin), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (STAR|INS), null);
            register("put", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayPut), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (STAR|INS), null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|NAV), null);
            register("remove", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayRemove), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (STAR|ABS), null);
            register("reverse", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayReverse), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            register("size", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArraySize), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            Saxon.Hej.type.ItemType sortFunctionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
            register("sort", 1, typeof(Saxon.Hej.ma.arrays.ArraySort), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            register("sort", 2, typeof(Saxon.Hej.ma.arrays.ArraySort), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, (OPT|ABS), null);
            register("sort", 3, typeof(Saxon.Hej.ma.arrays.ArraySort), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, (OPT|ABS), null).arg(2, sortFunctionType, (ONE|INS), null);
            register("subarray", 2, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArraySubarray), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (ONE|ABS), null);
            register("subarray", 3, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArraySubarray), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (ONE|ABS), null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, (ONE|ABS), null);
            register("tail", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayTail), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            register("_to-sequence", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayToSequence), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, (ONE|INS), null);
            register("_from-sequence", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayFromSequence), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), (STAR|INS), null);
            register("of", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayOf), Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, SEQV).arg(0, Saxon.Hej.ma.Parcel.TYPE, STAR, null);
            register("members", 1, typeof(Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayMembers), Saxon.Hej.ma.Parcel.TYPE, STAR, SEQV).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, ONE, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.ARRAY_FUNCTIONS;
        }
        public override string getConventionalPrefix() {
            return "array";
        }
        public static int checkSubscript(Saxon.Hej.value.IntegerValue subscript, int limit) {
            int index = subscript.asSubscript();
            if ((index<=0)) {
                throw new Saxon.Hej.trans.XPathException((("Array subscript "+subscript.getUnicodeStringValue())+" is out of range"), "FOAY0001");
            }
            if ((index>limit)) {
                throw new Saxon.Hej.trans.XPathException((((("Array subscript "+subscript.getUnicodeStringValue())+" exceeds limit (")+limit)+")"), "FOAY0001");
            }
            return index;
        }
        public class ArrayAppend : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                return append(array, arguments[1]);
            }
            public static Saxon.Hej.ma.arrays.ArrayItem append(Saxon.Hej.ma.arrays.ArrayItem array, Saxon.Hej.om.Sequence member) {
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                list.Add(member.materialize());
                Saxon.Hej.ma.arrays.SimpleArrayItem otherArray = new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
                return array.concat(otherArray);
            }
        
        }
        public class ArrayFilter : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[1].head());
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                int i;
                for (i = 0; (i<array.arrayLength()); i++) {
                    if (((Saxon.Hej.value.BooleanValue)(dynamicCall(fn, context, new Saxon.Hej.om.Sequence[]{array.get(i)}).head())).getBooleanValue()) {
                        list.Add(array.get(i));
                    }
                }
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
            }
        
        }
        public class ArrayFlatten : Saxon.Hej.functions.SystemFunction {
            private void flatten(Saxon.Hej.om.Sequence arg, System.Collections.Generic.IList<Saxon.Hej.om.Item> @out) {
                Saxon.Hej.om.SequenceTool.supply(arg.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        foreach (Saxon.Hej.om.Sequence member in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                            flatten(member, @out);
                        }
                    } else {
                        @out.Add(item);
                    }
                } )));
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.Collections.Generic.IList<Saxon.Hej.om.Item> @out = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                flatten(arguments[0], @out);
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(@out);
            }
        
        }
        public class ArrayFoldLeft : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                int arraySize = array.arrayLength();
                Saxon.Hej.om.Sequence zero = arguments[1];
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[2].head());
                int i;
                for (i = 0; (i<arraySize); i++) {
                    zero = dynamicCall(fn, context, new Saxon.Hej.om.Sequence[]{zero, array.get(i)});
                }
                return zero;
            }
        
        }
        public class ArrayFoldRight : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                Saxon.Hej.om.Sequence zero = arguments[1];
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[2].head());
                int i;
                for (i = (array.arrayLength()-1); (i>=0); i--) {
                    zero = dynamicCall(fn, context, new Saxon.Hej.om.Sequence[]{array.get(i), zero});
                }
                return zero;
            }
        
        }
        public class ArrayForEach : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[1].head());
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                int i;
                for (i = 0; (i<array.arrayLength()); i++) {
                    list.Add(dynamicCall(fn, context, new Saxon.Hej.om.GroundedValue[]{array.get(i)}).materialize());
                }
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
            }
        
        }
        public class ArrayForEachPair : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array1 = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array1 != null));
                Saxon.Hej.ma.arrays.ArrayItem array2 = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[1].head());
                System.Diagnostics.Debug.Assert((array2 != null));
                Saxon.Hej.om.Function fn = (Saxon.Hej.om.Function)(arguments[2].head());
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                int i;
                for (i = 0; ((i<array1.arrayLength()) && (i<array2.arrayLength())); i++) {
                    list.Add(dynamicCall(fn, context, new Saxon.Hej.om.Sequence[]{array1.get(i), array2.get(i)}).materialize());
                }
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
            }
        
        }
        public class ArrayGet : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                Saxon.Hej.value.IntegerValue index = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
                return array.get((checkSubscript(index, array.arrayLength())-1));
            }
        
        }
        public class ArrayHead : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                if ((array.arrayLength() == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Argument to array:head is an empty array", "FOAY0001");
                }
                return array.get(0);
            }
        
        }
        public class ArrayInsertBefore : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                int index = (checkSubscript((Saxon.Hej.value.IntegerValue)(arguments[1].head()), (array.arrayLength()+1))-1);
                if (((index<0) || (index>array.arrayLength()))) {
                    throw new Saxon.Hej.trans.XPathException("Specified position is not in range", "FOAY0001");
                }
                Saxon.Hej.om.Sequence newMember = arguments[2];
                return array.insert(index, newMember.materialize());
            }
        
        }
        public class ArrayJoin : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.SequenceIterator iterator = arguments[0].iterate();
                Saxon.Hej.ma.arrays.ArrayItem array = Saxon.Hej.ma.arrays.SimpleArrayItem.EMPTY_ARRAY;
                Saxon.Hej.ma.arrays.ArrayItem nextArray;
                while (((nextArray = (Saxon.Hej.ma.arrays.ArrayItem)(iterator.next())) != null)) {
                    array = array.concat(nextArray);
                }
                return array;
            }
        
        }
        public class ArrayMembers : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem input = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Collections.Generic.IList<Saxon.Hej.ma.Parcel> parcels = new System.Collections.Generic.List<Saxon.Hej.ma.Parcel>(input.arrayLength());
                foreach (Saxon.Hej.om.GroundedValue member in input.members()) {
                    parcels.Add(new Saxon.Hej.ma.Parcel(member));
                }
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(parcels);
            }
        
        }
        public class ArrayOf : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                Saxon.Hej.om.SequenceIterator parcels = arguments[0].iterate();
                for (Saxon.Hej.om.Item parcel; ((parcel = parcels.next()) != null); ) {
                    members.Add(Saxon.Hej.functions.UnparcelFn.unparcel(parcel, context));
                }
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
            }
        
        }
        public class ArrayPut : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                int index = (checkSubscript((Saxon.Hej.value.IntegerValue)(arguments[1].head()), array.arrayLength())-1);
                Saxon.Hej.om.GroundedValue newVal = arguments[2].materialize();
                return array.put(index, newVal);
            }
        
        }
        public class ArrayRemove : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                Saxon.Hej.om.GroundedValue offsets = arguments[1].materialize();
                if ((offsets is Saxon.Hej.value.IntegerValue)) {
                    int index = (checkSubscript((Saxon.Hej.value.IntegerValue)(offsets), array.arrayLength())-1);
                    return array.remove(index);
                }
                Saxon.Hej.z.IntSet positions = new Saxon.Hej.z.IntHashSet();
                Saxon.Hej.om.SequenceIterator arg1 = offsets.iterate();
                Saxon.Hej.om.SequenceTool.supply(arg1, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((pos => {
                    int index = (checkSubscript((Saxon.Hej.value.IntegerValue)(pos), array.arrayLength())-1);
                    positions.add(index);
                } )));
                return array.removeSeveral(positions);
            }
        
        }
        public class ArrayReverse : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                int i;
                for (i = 0; (i<array.arrayLength()); i++) {
                    list.Add(array.get(((array.arrayLength()-i)-1)));
                }
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
            }
        
        }
        public class ArraySize : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                return new Saxon.Hej.value.Int64Value(array.arrayLength());
            }
        
        }
        public class ArraySubarray : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                int start = checkSubscript((Saxon.Hej.value.IntegerValue)(arguments[1].head()), (array.arrayLength()+1));
                int length;
                if ((arguments.Length == 3)) {
                    Saxon.Hej.value.IntegerValue len = (Saxon.Hej.value.IntegerValue)(arguments[2].head());
                    int signum = len.signum();
                    if ((signum<0)) {
                        throw new Saxon.Hej.trans.XPathException("Specified length of subarray is less than zero", "FOAY0002");
                    }
                    length = ((signum == 0) ? 0 : checkSubscript(len, array.arrayLength()));
                } else {
                    length = ((array.arrayLength()-start)+1);
                }
                if ((start<1)) {
                    throw new Saxon.Hej.trans.XPathException("Start position is less than one", "FOAY0001");
                }
                if ((start>(array.arrayLength()+1))) {
                    throw new Saxon.Hej.trans.XPathException("Start position is out of bounds", "FOAY0001");
                }
                if (((start+length)>(array.arrayLength()+1))) {
                    throw new Saxon.Hej.trans.XPathException("Specified length of subarray is too great for start position given", "FOAY0001");
                }
                return array.subArray((start-1), ((start+length)-1));
            }
        
        }
        public class ArrayTail : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                System.Diagnostics.Debug.Assert((array != null));
                if ((array.arrayLength()<1)) {
                    throw new Saxon.Hej.trans.XPathException("Argument to array:tail is an empty array", "FOAY0001");
                }
                return array.remove(0);
            }
        
        }
        public class ArrayToSequence : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
                return toSequence(array);
            }
            public static Saxon.Hej.om.Sequence toSequence(Saxon.Hej.ma.arrays.ArrayItem array) {
                Saxon.Hej.ma.zeno.ZenoSequence results = new Saxon.Hej.ma.zeno.ZenoSequence();
                foreach (Saxon.Hej.om.GroundedValue seq in array.members()) {
                    results.appendSequence(seq);
                }
                return results;
            }
        
        }
        public class ArrayFromSequence : Saxon.Hej.functions.FoldingFunction {
            public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return Saxon.Hej.ma.arrays.SimpleArrayItem.makeSimpleArrayItem(((Saxon.Hej.om.Sequence)(arguments[0])).iterate());
            }
            public override Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  additionalArguments) {
                return new Inner_Fold_d1253e3994(this);
            }
        
            private class Inner_Fold_d1253e3994 : Saxon.Hej.functions.Fold {
                private readonly ArrayFromSequence outerInstance;
                public Inner_Fold_d1253e3994(ArrayFromSequence outerInstance) {
                    this.outerInstance = outerInstance;
                    members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                }
                internal System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members;
                public void processItem(Saxon.Hej.om.Item item) {
                    /*field*/members.Add(/*localparam*/item);
                }
                public bool isFinished() {
                    return false;
                }
                public Saxon.Hej.om.Sequence result() {
                    return new Saxon.Hej.ma.arrays.SimpleArrayItem(/*field*/members);
                }
            }

        }
    
        private class Inner_Fold_d1253e3994 : Saxon.Hej.functions.Fold {
            private readonly ArrayFromSequence outerInstance;
            public Inner_Fold_d1253e3994(ArrayFromSequence outerInstance) {
                this.outerInstance = outerInstance;
                members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
            }
            internal System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members;
            public void processItem(Saxon.Hej.om.Item item) {
                /*field*/members.Add(/*localparam*/item);
            }
            public bool isFinished() {
                return false;
            }
            public Saxon.Hej.om.Sequence result() {
                return new Saxon.Hej.ma.arrays.SimpleArrayItem(/*field*/members);
            }
        }

    }
}