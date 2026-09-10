package com.saxonica.ee.extfn;

import com.saxonica.functions.extfn.VendorFunctionSetPE;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.NamespaceUri;
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


    private final static VendorFunctionSetEE INSTANCE = new VendorFunctionSetEE();

    public static VendorFunctionSetEE getInstance() {
        return INSTANCE;
    }

    private  VendorFunctionSetEE() {
        init();
    }

    private void init() {

        importFunctionSet(VendorFunctionSetPE.getInstance());

        register("schema", 0, e -> e.populate(SchemaFn::new, AnyFunctionType.getInstance(), OPT, 0));

        register("schema", 2, e -> e.populate(SchemaFn::new, AnyFunctionType.getInstance(), OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.QNAME, ONE, null));

        register("stream", 1, e -> e.populate(StreamFn::new, AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY));

        register("validate", 1, e -> e.populate(ValidateFn::new, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, null));

        register("validate", 2, e -> e.populate(ValidateFn::new, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY)
                .arg(1, MapType.getInstance(), ONE, null));

        register("validate", 3, e -> e.populate(ValidateFn::new, MapType.getInstance(), OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT, EMPTY)
                .arg(1, MapType.getInstance(), ONE, null)
                .arg(2, MapType.getInstance(), ONE, null));

        register("compile-XPath", 3, e -> e.populate(CompileXPath::new, AnyItemType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, AnyItemType.getInstance(), OPT, null)
                .arg(2, AnyItemType.getInstance(), OPT, null));

        register("marshal-type", 1, e -> e.populate(CompileXPath::new, MapType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY));

        register("parse-sequence-type", 3, e -> e.populate(CompileXPath::new, MapType.getInstance(), ONE, BuiltInFunctionSet.LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, EMPTY)
                .arg(1, NodeKindTest.ELEMENT, ONE, EMPTY)
                .arg(2, BuiltInAtomicType.STRING, OPT, null));

        register("type", 1, e -> e.populate(SchemaTypeFn::new, SpecificFunctionType.COMPONENT_FUNCTION_TYPE, OPT, 0)
                .arg(0, AnyItemType.getInstance(), OPT, null));

    }

    /**
     * Return the namespace URI for the functions local to this function set.
     *
     * @return the namespace URI of the functions local to this function set.
     * Note that functions imported from another function set may have a different
     * namespace URI.
     */
    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.SAXON;
    }

    @Override
    public String getConventionalPrefix() {
        return "saxon";
    }
}

// Copyright (c) 2004-2022 Saxonica Limited