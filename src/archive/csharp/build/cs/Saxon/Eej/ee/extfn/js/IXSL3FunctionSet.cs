using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSL3FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Eej.ee.extfn.js.IXSL3FunctionSet THE_INSTANCE = new Saxon.Eej.ee.extfn.js.IXSL3FunctionSet();
        public static Saxon.Eej.ee.extfn.js.IXSL3FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private IXSL3FunctionSet()  {
            init();
        }
        private void init() {
            importFunctionSet(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance());
            register("abort-controller", 0, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)));
            register("abort-controller", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, OPT, null));
            register("all", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null));
            register("all-settled", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null));
            register("any", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null));
            register("apply", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null).arg(2, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("call", 4, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Call(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null).arg(3, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("catch", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null));
            register("doc", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("doc", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
            register("doc-fetched", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("eval", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Eval(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("finally", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null));
            register("get", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("http-request", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null));
            register("http-request", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
            register("json-doc", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("json-doc", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("json-doc", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
            register("json-parse", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("json-parse", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("new", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null));
            register("new", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null).arg(2, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null));
            register("race", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null));
            register("resolve", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null));
            register("set-property", 4, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, null));
            register("sleep", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null));
            register("then", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.AnyFunctionType.getInstance(), ONE, null));
            register("unparsed-text", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("unparsed-text", 2, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
            register("unparsed-text", 3, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.AnyItemType.getInstance(), ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null));
            register("unparsed-text-fetched", 1, e => e.populate(() => new Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null));
        }
        public override Saxon.Hej.om.NamespaceUri getNamespace() {
            return Saxon.Hej.om.NamespaceUri.IXSL;
        }
        public override string getConventionalPrefix() {
            return "ixsl";
        }
    
    }
}