package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.functions.extfn.VendorFunctionSetPE;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;

/**
 * The VendorFunctionLibraryEE represents additions to the set of Saxon vendor-supplied
 * extension functions that are available only with the Saxon-EE edition
 */

public class VendorFunctionSetEE extends BuiltInFunctionSet {

    protected EnterpriseConfiguration config;

    public VendorFunctionSetEE(EnterpriseConfiguration config) {
        this.config = config;
        init();
    }

    private void init() {

        importFunctionSet(new VendorFunctionSetPE(config));

        register("schema", 0, SchemaFn.class, AnyFunctionType.getInstance(), OPT, 0);

        register("schema", 2, SchemaFn.class, AnyFunctionType.getInstance(), OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.QNAME, ONE, null);

        register("stream", 1, StreamFn.class, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY);

        register("validate", 1, ValidateFn.class, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, null);

        register("validate", 2, ValidateFn.class, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY)
                .arg(1, MapType.getInstance(), ONE, null);

        register("validate", 3, ValidateFn.class, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY)
                .arg(1, MapType.getInstance(), ONE, null)
                .arg(2, MapType.getInstance(), ONE, null);

        register("compile-XPath", 3, CompileXPath.class, AnyItemType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, AnyItemType.getInstance(), OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null);

        register("marshal-type", 1, CompileXPath.class, MapType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY);

        register("parse-sequence-type", 3, CompileXPath.class, MapType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, NodeKindTest.ELEMENT, ONE, EMPTY)
                .arg(2, BuiltInAtomicType.STRING, OPT, null);

        register("type", 1, SchemaTypeFn.class, SpecificFunctionType.COMPONENT_FUNCTION_TYPE, OPT, 0)
                .arg(0, AnyItemType.getInstance(), OPT, null);

    }

    /**
     * Return the namespace URI for the functions local to this function set.
     *
     * @return the namespace URI of the functions local to this function set.
     * Note that functions imported from another function set may have a different
     * namespace URI.
     */
    @Override
    public String getNamespace() {
        return NamespaceConstant.SAXON;
    }

    @Override
    public String getConventionalPrefix() {
        return "saxon";
    }
}

// Copyright (c) 2004-2022 Saxonica Limited