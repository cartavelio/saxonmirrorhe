using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class MapCreate : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
            Saxon.Hej.ma.map.MapItem baseMap = (Saxon.Hej.ma.map.MapItem)(iter.next());
            if ((baseMap == null)) {
                return new Saxon.Impl.DataModel.ImmMap();
            } else {
                if (!(((baseMap is Saxon.Impl.DataModel.ImmMap)))) {
                    baseMap = Saxon.Impl.DataModel.ImmMap.copy(baseMap);
                }
                Saxon.Hej.ma.map.MapItem next;
                while (((next = (Saxon.Hej.ma.map.MapItem)(iter.next())) != null)) {
                    foreach (Saxon.Hej.ma.map.KeyValuePair pair in next.keyValuePairs()) {
                        if ((baseMap.get(pair.key) != null)) {
                            throw new Saxon.Hej.trans.XPathException((("Duplicate key value ("+pair.key)+") in map"), "XQDY0137");
                        } else {
                            baseMap = baseMap.addEntry(pair.key, pair.value);
                        }
                    }
                }
                return baseMap;
            }
        }
        public override string getStreamerName() {
            return "NewMap";
        }
    
    }
}