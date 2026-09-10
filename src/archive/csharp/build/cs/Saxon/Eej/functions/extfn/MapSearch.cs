using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class MapSearch : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> result = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
            Saxon.Hej.om.Function tester = ((arguments.Length>2) ? (Saxon.Hej.om.Function)(arguments[2].head()) : null);
            processSequence(arguments[0].materialize(), key, tester, context, result);
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
        }
        private void processSequence(Saxon.Hej.om.GroundedValue seq, Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.Function tester, Saxon.Hej.expr.XPathContext context, System.Collections.Generic.IList<Saxon.Hej.om.Item> result) {
            Saxon.Hej.om.GroundedValue @in = ((Saxon.Hej.om.Sequence)(seq)).materialize();
            Saxon.Hej.om.SequenceIterator iter = @in.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                    Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(item);
                    if (!(((array is Saxon.Eej.xsltextn.pedigree.PedigreeValue)))) {
                        array = new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem(array);
                    }
                    foreach (Saxon.Hej.om.GroundedValue member in array.members()) {
                        processSequence(member, key, tester, context, result);
                    }
                } else {
                    if ((item is Saxon.Hej.ma.map.MapItem)) {
                        Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(item);
                        if (!(((map is Saxon.Eej.xsltextn.pedigree.PedigreeValue)))) {
                            map = new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem(map);
                        }
                        Saxon.Hej.om.GroundedValue value = map.get(key);
                        if ((value != null)) {
                            Saxon.Hej.om.Sequence test = ((tester == null) ? Saxon.Hej.value.BooleanValue.TRUE : tester.call(context, new Saxon.Hej.om.Sequence[]{value}));
                            if (((Saxon.Hej.value.BooleanValue)(test.head())).getBooleanValue()) {
                                Saxon.Hej.ma.map.DictionaryMap hit = new Saxon.Hej.ma.map.DictionaryMap();
                                hit.initialPut("key", key);
                                hit.initialPut("value", value);
                                hit.initialPut("map", map);
                                result.Add(hit);
                            }
                        }
                        foreach (Saxon.Hej.ma.map.KeyValuePair entry in map.keyValuePairs()) {
                            processSequence(entry.value, key, tester, context, result);
                        }
                    }
                }
            }
        }
    
    }
}