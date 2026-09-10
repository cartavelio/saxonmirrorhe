using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class RandomNumberGenerator : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public static readonly Saxon.Hej.ma.map.MapType RETURN_TYPE = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.SINGLE_ITEM);
        private readonly static Saxon.Hej.type.FunctionItemType NEXT_FN_TYPE = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{}, Saxon.Hej.value.SequenceType.makeSequenceType(RETURN_TYPE, Saxon.Hej.expr.StaticProperty.ALLOWS_ONE));
        private readonly static Saxon.Hej.type.FunctionItemType PERMUTE_FN_TYPE = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        private static Saxon.Hej.ma.map.MapItem generator(long seed, Saxon.Hej.expr.XPathContext context) {
            Saxon.Ejava.util.Random random = new Saxon.Ejava.util.Random(seed);
            double number = random.nextDouble();
            long nextSeed = random.nextLong();
            Saxon.Hej.ma.map.DictionaryMap map = new Saxon.Hej.ma.map.DictionaryMap();
            map.initialPut("number", new Saxon.Hej.value.DoubleValue(number));
            map.initialPut("next", new Saxon.Hej.functions.CallableFunction(0, new Saxon.Hej.functions.hof.RandomNumberGenerator.NextGenerator(nextSeed), NEXT_FN_TYPE));
            map.initialPut("permute", new Saxon.Hej.functions.CallableFunction(1, new Saxon.Hej.functions.hof.RandomNumberGenerator.Permutation(nextSeed), PERMUTE_FN_TYPE));
            return map;
        }
        private class Permutation : Saxon.Hej.expr.Callable {
            internal System.Int64 nextSeed;
            public Permutation(System.Int64 nextSeed)  {
                this.nextSeed = nextSeed;
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Sequence input = arguments[0];
                Saxon.Hej.om.SequenceIterator iterator = input.iterate();
                Saxon.Hej.om.Item item;
                System.Collections.Generic.IList<Saxon.Hej.om.Item> output = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                Saxon.Ejava.util.Random random = new Saxon.Ejava.util.Random(nextSeed);
                while (((item = iterator.next()) != null)) {
                    int p = random.nextInt((output.Count+1));
                    output.Insert(p, item);
                }
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(output);
            }
            public override string ToString() {
                return "random-number-generator.permute";
            }
        
        }
        private class NextGenerator : Saxon.Hej.expr.Callable {
            internal long nextSeed;
            public NextGenerator(long nextSeed)  {
                this.nextSeed = nextSeed;
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return generator(nextSeed, context);
            }
            public override string ToString() {
                return "random-number-generator.next";
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            long seed;
            if ((arguments.Length == 0)) {
                seed = context.getCurrentDateTime().randomSeed();
            } else {
                Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
                seed = ((val == null) ? context.getCurrentDateTime().randomSeed() : val.GetHashCode());
            }
            return generator(seed, context);
        }
    
    }
}