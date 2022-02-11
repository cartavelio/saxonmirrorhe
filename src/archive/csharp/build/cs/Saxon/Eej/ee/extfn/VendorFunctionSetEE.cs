using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class VendorFunctionSetEE : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        protected internal Saxon.Eej.config.EnterpriseConfiguration config;
        public VendorFunctionSetEE(Saxon.Eej.config.EnterpriseConfiguration config)  {
            this.config = config;
            init();
        }
        private void init() {
            importFunctionSet(new Saxon.Eej.functions.extfn.VendorFunctionSetPE(config));
            register("schema", 0, typeof(Saxon.Eej.ee.extfn.SchemaFn), Saxon.Hej.type.AnyFunctionType.getInstance(), OPT, 0);
            register("schema", 2, typeof(Saxon.Eej.ee.extfn.SchemaFn), Saxon.Hej.type.AnyFunctionType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.QNAME, ONE, null);
            register("stream", 1, typeof(Saxon.Eej.ee.extfn.StreamFn), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY);
            register("validate", 1, typeof(Saxon.Eej.ee.extfn.ValidateFn), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, null);
            register("validate", 2, typeof(Saxon.Eej.ee.extfn.ValidateFn), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null);
            register("validate", 3, typeof(Saxon.Eej.ee.extfn.ValidateFn), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null).arg(2, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null);
            register("compile-XPath", 3, typeof(Saxon.Eej.ee.extfn.CompileXPath), Saxon.Hej.type.AnyItemType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null);
            register("marshal-type", 1, typeof(Saxon.Eej.ee.extfn.CompileXPath), Saxon.Hej.ma.map.MapType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY);
            register("parse-sequence-type", 3, typeof(Saxon.Eej.ee.extfn.CompileXPath), Saxon.Hej.ma.map.MapType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY).arg(1, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, EMPTY).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("type", 1, typeof(Saxon.Eej.ee.extfn.SchemaTypeFn), Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE, OPT, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.SAXON;
        }
        public override string getConventionalPrefix() {
            return "saxon";
        }
    
    }
}