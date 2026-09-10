package com.saxonica.trans;

import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.SimpleMode;
import net.sf.saxon.trans.XPathException;

/**
 * The Mode class for use in Saxon-PE, which may contain additional XSLT3.0 features
 * This is subclassed in Saxon-EE to add optimisation features
 */
public class ModePE extends SimpleMode {

    public ModePE(StructuredQName modeName) {
        super(modeName);
    }

    /**
     * Prepare for possible streamability - probably eventually in EE
     *
     * @throws XPathException if not streamable
     */
    @Override
    public void prepareStreamability() throws XPathException {
        if (isDeclaredStreamable()) {
            computeStreamability();
            invertStreamableTemplates();
        }
    }

}

// Copyright (c) 2015-2023. Saxonica Limited