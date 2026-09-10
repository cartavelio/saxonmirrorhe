package com.saxonica.config;

import com.saxonica.ee.domino.DominoTreeModel;
import net.sf.saxon.Configuration;


/**
 * Implementation of the Platform class containing methods specific to the Java platform
 * (as distinct from .NET) for Saxon-EE.
 * This is a singleton class with no instance data.
 */
public class JavaPlatformEE extends JavaPlatformPE {



    /**
     * The constructor is called during the static initialization of the Configuration
     */

    public JavaPlatformEE() {
    }

    /**
     * Perform platform-specific initialization of the configuration
     */

    @Override
    public void initialize(Configuration config) {
        super.initialize(config);
        config.registerExternalObjectModel(DominoTreeModel.getInstance());
    }




}

// Copyright (c) 2009-2020 Saxonica Limited
