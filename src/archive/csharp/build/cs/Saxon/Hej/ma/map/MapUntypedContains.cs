using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class MapUntypedContains : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
            Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
            if (key.isUntypedAtomic()) {
                foreach (Saxon.Hej.type.PrimitiveUType prim in map.getKeyUType().decompose()) {
                    Saxon.Hej.type.BuiltInAtomicType t = (Saxon.Hej.type.BuiltInAtomicType)(prim.toItemType());
                    Saxon.Hej.type.StringConverter converter = t.getStringConverter(rules);
                    Saxon.Hej.type.ConversionResult av = converter.convert(key);
                    if ((av is Saxon.Hej.type.ValidationFailure)) {
                        if (prim.Equals(Saxon.Hej.type.PrimitiveUType.DECIMAL)) {
                            converter = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(rules);
                            av = converter.convert(key);
                            if ((av is Saxon.Hej.value.AtomicValue)) {
                                if ((map.get(av.asAtomic()) != null)) {
                                    return Saxon.Hej.value.BooleanValue.TRUE;
                                }
                            }
                        }
                    } else {
                        if ((map.get(av.asAtomic()) != null)) {
                            return Saxon.Hej.value.BooleanValue.TRUE;
                        }
                    }
                }
                return Saxon.Hej.value.BooleanValue.FALSE;
            } else {
                if (key.isNaN()) {
                    return Saxon.Hej.value.BooleanValue.FALSE;
                }
            }
            bool result = (map.get(key) != null);
            return Saxon.Hej.value.BooleanValue.get(result);
        }
    
    }
}