using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class VendorFunctionSetEE : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private readonly static Saxon.Eej.ee.extfn.VendorFunctionSetEE INSTANCE = new Saxon.Eej.ee.extfn.VendorFunctionSetEE();
        public static Saxon.Eej.ee.extfn.VendorFunctionSetEE getInstance() {
            return INSTANCE;
        }
        private VendorFunctionSetEE()  {
            init();
        }
        private void init() {
            importFunctionSet(/*134*/Saxon.Eej.functions.extfn.VendorFunctionSetPE.getInstance());
            register("schema", 0, e => e.populate(() => new Saxon.Eej.ee.extfn.SchemaFn(), Saxon.Hej.type.AnyFunctionType.getInstance(), OPT, 0));
            register("schema", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.SchemaFn(), Saxon.Hej.type.AnyFunctionType.getInstance(), OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.QNAME, ONE, null));
            register("stream", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.StreamFn(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, EMPTY));
            register("validate", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.ValidateFn(), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, null));
            register("validate", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.ValidateFn(), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null));
            register("validate", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.ValidateFn(), Saxon.Hej.ma.map.MapType.getInstance(), OPT, 0).arg(0, Saxon.Hej.pattern.AnyNodeTest.getInstance(), OPT, EMPTY).arg(1, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null).arg(2, Saxon.Hej.ma.map.MapType.getInstance(), ONE, null));
            register("compile-XPath", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.CompileXPath(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
            register("marshal-type", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.CompileXPath(), Saxon.Hej.ma.map.MapType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY));
            register("parse-sequence-type", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.CompileXPath(), Saxon.Hej.ma.map.MapType.getInstance(), ONE, Saxon.Hej.functions.registry.BuiltInFunctionSet.LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, EMPTY).arg(1, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, EMPTY).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("type", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.SchemaTypeFn(), Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE, OPT, 0).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
        }
        public override Saxon.Hej.om.NamespaceUri getNamespace() {
            return Saxon.Hej.om.NamespaceUri.SAXON;
        }
        public override string getConventionalPrefix() {
            return "saxon";
        }
    
    }
}