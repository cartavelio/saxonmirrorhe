package com.saxonica.ee.config;

import com.saxonica.config.pe.StyleNodeFactoryPE;
import com.saxonica.ee.schema.XSLImportSchemaEE;
import com.saxonica.ee.stream.AccumulatorRegistryEE;
import com.saxonica.ee.trans.PrincipalStylesheetModuleEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.PrincipalStylesheetModule;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.style.XSLPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.linked.NodeImpl;

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

    /**
     * Make an XSL element node
     *
     * @param f      the fingerprint of the node name
     * @param parent the parent node
     * @return the constructed element node
     */
    @Override
    protected StyleElement makeXSLElement(int f, NodeImpl parent) {
        if (f == StandardNames.XSL_IMPORT_SCHEMA) {
            return new XSLImportSchemaEE();
        }
        return super.makeXSLElement(f, parent);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited