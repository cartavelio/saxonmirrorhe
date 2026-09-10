package com.saxonica.ee.config;

import com.saxonica.config.pe.StyleNodeFactoryPE;
import com.saxonica.ee.stream.AccumulatorRegistryEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.PrincipalStylesheetModule;
import net.sf.saxon.style.XSLPackage;
import net.sf.saxon.trans.XPathException;
import com.saxonica.ee.trans.PrincipalStylesheetModuleEE;

/**
 * Class StyleNodeFactoryEE: Subclass of StyleNodeFactory used for Saxon-EE. <br>
 * A Factory for nodes in the stylesheet tree. <br>
 * This version differs from the parent class in allowing extension elements to be constructed
 * if the URI corresponds to an extension element factory registered in the Configuration; it also
 * recognizes new XSLT elements defined in the (draft) specification of XSLT 3.0
 */
public class StyleNodeFactoryEE extends StyleNodeFactoryPE {
    /**
     * Create the node factory for representing an XSLT stylesheet as a tree structure
     *
     * @param config the Saxon configuration
     */
    public StyleNodeFactoryEE(Configuration config, Compilation compilation) {
        super(config, compilation);
    }

    @Override
    public AccumulatorRegistryEE makeAccumulatorManager() {
        return new AccumulatorRegistryEE();
    }

    @Override
    public PrincipalStylesheetModule newPrincipalModule(XSLPackage node) throws XPathException {
        return new PrincipalStylesheetModuleEE(node);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited