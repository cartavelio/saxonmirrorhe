package com.saxonica.config;

import com.saxonica.ee.domino.DominoTreeModel;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Sender;
import net.sf.saxon.event.Valve;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.XMLReader;

import javax.xml.transform.Source;


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

    public void initialize(Configuration config) {
        super.initialize(config);
        config.registerExternalObjectModel(DominoTreeModel.getInstance());
    }




}

// Copyright (c) 2009-2017 Saxonica Limited.
