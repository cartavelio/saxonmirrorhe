package com.saxonica.ee.extfn.js;

import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;

/**
 * The IXSL3FunctionSet represents additions to the set of Saxon vendor-supplied
 * extension functions that are available from SaxonJS 3.
 */

public class IXSL3FunctionSet extends BuiltInFunctionSet {

    private static final IXSL3FunctionSet THE_INSTANCE = new IXSL3FunctionSet();

    public static IXSL3FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private IXSL3FunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(IXSLFunctionSet.getInstance());

        register("abort-controller", 0, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE));

        register("abort-controller", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.INTEGER, OPT, null));

        register("all", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, ArrayItemType.getInstance(), ONE, null));

        register("all-settled", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, ArrayItemType.getInstance(), ONE, null));

        register("any", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, ArrayItemType.getInstance(), ONE, null));

        register("doc", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("doc", 2, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, AnyItemType.getInstance(), OPT, null));

        register("doc-fetched", 1, e -> e.populate(IXSLFunctionSet.Stub::new, BuiltInAtomicType.BOOLEAN, ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("http-request", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, null));

        register("http-request", 2, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, null)
                .arg(1, AnyItemType.getInstance(), OPT, null));

        register("json-doc", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("json-doc", 2, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null));

        register("json-doc", 3, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, MapType.ANY_MAP_TYPE, OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null));

        register("race", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, ArrayItemType.getInstance(), ONE, null));

        register("sleep", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null));

        register("then", 2, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), ONE, null)
                .arg(1, AnyFunctionType.getInstance(), ONE, null));

        register("unparsed-text", 1, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("unparsed-text", 2, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null));

        register("unparsed-text", 3, e -> e.populate(IXSLFunctionSet.Stub::new, AnyItemType.getInstance(), ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null));

        register("unparsed-text-fetched", 1, e -> e.populate(IXSLFunctionSet.Stub::new, BuiltInAtomicType.BOOLEAN, ONE, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));
    }

    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.IXSL;
    }

    @Override
    public String getConventionalPrefix() {
        return "ixsl";
    }
}
