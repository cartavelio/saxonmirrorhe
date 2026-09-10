using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class ExsltCommonFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.ExsltCommonFunctionSet THE_INSTANCE = new Saxon.Hej.functions.registry.ExsltCommonFunctionSet();
        public static Saxon.Hej.functions.registry.ExsltCommonFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private ExsltCommonFunctionSet()  {
            init();
        }
        private void init() {
            register("node-set", 1, e => e.populate(() => new Saxon.Hej.functions.registry.ExsltCommonFunctionSet.NodeSetFn(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), OPT, EMPTY));
            register("object-type", 1, e => e.populate(() => new Saxon.Hej.functions.registry.ExsltCommonFunctionSet.ObjectTypeFn(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null));
        }
        public override Saxon.Hej.om.NamespaceUri getNamespace() {
            return Saxon.Hej.om.NamespaceUri.EXSLT_COMMON;
        }
        public override string getConventionalPrefix() {
            return "exsltCommon";
        }
        public class NodeSetFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return arguments[0];
            }
        
        }
        public class ObjectTypeFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
                Saxon.Hej.om.Item value = arguments[0].head();
                Saxon.Hej.type.ItemType type = Saxon.Hej.om.SequenceTool.getItemType(value, th);
                if (th.isSubType(type, Saxon.Hej.pattern.AnyNodeTest.getInstance())) {
                    return Saxon.Hej.value.StringValue.bmp("node-set");
                } else {
                    if (th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                        return Saxon.Hej.value.StringValue.bmp("string");
                    } else {
                        if (Saxon.Hej.type.NumericType.isNumericType(type)) {
                            return Saxon.Hej.value.StringValue.bmp("number");
                        } else {
                            if (th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) {
                                return Saxon.Hej.value.StringValue.bmp("boolean");
                            } else {
                                return new Saxon.Hej.value.StringValue(type.ToString());
                            }
                        }
                    }
                }
            }
        
        }
    
    }
}