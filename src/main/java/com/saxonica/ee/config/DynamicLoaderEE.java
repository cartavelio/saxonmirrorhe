package com.saxonica.ee.config;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.pe.DynamicLoaderPE;
import com.saxonica.ee.domino.DominoNode;

/**
 * Specialization of DynamicLoader for use in Saxon-EE
 */
public class DynamicLoaderEE extends DynamicLoaderPE {

    /**
     * Register classes that might be dynamically loaded even though they are contained
     * within Saxon itself. This typically occurs for default implementations of classes that
     * can be substituted or subclassed by the user.
     */

    @Override
    protected void registerKnownClasses() {
        super.registerKnownClasses();
        knownClasses.put("com.saxonica.config.EnterpriseConfiguration", EnterpriseConfiguration.class);
        knownClasses.put("com.saxonica.ee.domino.DominoNode", DominoNode.class);
    }
}

//
// Copyright (c) 2010-2023 Saxonica Limited
//