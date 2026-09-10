package com.saxonica.config.pe;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.Verifier;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.trans.DynamicLoader;

/**
 * Specialization of DynamicLoader for use in Saxon-PE
 */
public class DynamicLoaderPE extends DynamicLoader {

    /**
     * Register classes that might be dynamically loaded even though they are contained
     * within Saxon itself. This typically occurs for default implementations of classes that
     * can be substituted or subclassed by the user.
     */

    @Override
    protected void registerKnownClasses() {
        super.registerKnownClasses();
        knownClasses.put("com.saxonica.config.ProfessionalConfiguration", ProfessionalConfiguration.class);
        knownClasses.put("com.saxonica.config.Verifier", Verifier.class);
        knownClasses.put("org.w3c.dom.Document", org.w3c.dom.Document.class);
        knownClasses.put("net.sf.saxon.expr.number.Numberer_en", Numberer_en.class);

    }
}

//
// Copyright (c) 2010-2023 Saxonica Limited
//