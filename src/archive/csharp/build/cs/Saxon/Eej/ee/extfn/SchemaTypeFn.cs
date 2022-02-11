using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class SchemaTypeFn : Saxon.Hej.functions.SystemFunction {
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
                    return type.getComponentAsFunction();
                } else {
                    if ((item is Saxon.Hej.value.AtomicValue)) {
                        Saxon.Hej.type.AtomicType type = ((Saxon.Hej.value.AtomicValue)(item)).getItemType();
                        return type.getComponentAsFunction();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                }
            }
        }
    
    }
}