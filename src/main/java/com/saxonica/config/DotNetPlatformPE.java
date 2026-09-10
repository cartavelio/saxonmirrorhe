package com.saxonica.config;

import com.saxonica.functions.extfn.EXPathArchive.Archive;
import com.saxonica.functions.extfn.EXPathBinary;
import com.saxonica.functions.extfn.EXPathFile;
import com.saxonica.functions.extfn.Extensions;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.dotnet.DotNetPlatform;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.option.exslt.Random;
import net.sf.saxon.option.exslt.Sets;
import net.sf.saxon.trans.XPathException;


/**
 * Implementation of the Platform interface containing methods appropriate to the .NET platform
 */

public class DotNetPlatformPE extends DotNetPlatform {

    public DotNetPlatformPE(){}

    /**
     * Get the fully qualified class name of the Saxon-EE Configuration class
     * @return the fully-qualified class name
     */

    public static String getSaxonSaFullyQualifiedClassName() {
//        return "com.saxonica.ee.validate.EnterpriseConfiguration";
        return "com.saxonica.config.EnterpriseConfiguration, " +
                "saxon9sa, " +
                "Version=" + Version.getProductVersion() + ", " +
                "Culture=neutral, " +
                "PublicKeyToken=e1fdd002d5083fe6";
    }

    /**
     * Perform platform-specific initialization of the configuration
     */

    public void initialize(Configuration config) {
        super.initialize(config);
        if (config instanceof ProfessionalConfiguration) {
            JavaExtensionLibrary javaLib = new JavaExtensionLibrary((ProfessionalConfiguration)config);
            javaLib.declareJavaClass(NamespaceConstant.SAXON, Extensions.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_SETS, Sets.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_MATH, net.sf.saxon.option.exslt.Math.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_DATES_AND_TIMES, net.sf.saxon.option.exslt.Date.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_RANDOM, Random.class);
            javaLib.declareJavaClass(EXPathFile.NAMESPACE, EXPathFile.class);
            javaLib.declareJavaClass(EXPathBinary.NAMESPACE, EXPathBinary.class);
            javaLib.declareJavaClass(Archive.NAMESPACE, Archive.class);

            ((ProfessionalConfiguration) config).setExtensionBinder("java", javaLib);

            DotNetExtensionLibrary dotNetLib = new DotNetExtensionLibrary((ProfessionalConfiguration) config);
            ((ProfessionalConfiguration) config).setExtensionBinder("clitype", dotNetLib);
            config.setLocalizerFactory(new DotNetLocalizerFactory(config));
        }
    }

    /**
     * Is an ICU collator loaded?
     * @return  true if ICU collations appear loaded
     */
    public boolean hasICUCollator() {
        return ICULibrary.isICUCollatorAvailable();
    }
    /**
     * Is an ICU numberer loaded?
     * @return  true if ICU numberers appear loaded
     */
    public boolean hasICUNumberer() {
        return ICULibrary.hasNumberer();
    }

    /**
     * If available, make a collation using the ICU-J Library
     *
     * @param uri    the collation URI (which will always be a UCA collation URI as defined in XSLT 3.0)
     * @param config the Saxon configuration
     * @return the collation, or null if not available
     * @throws net.sf.saxon.trans.XPathException if the URI is malformed in some way
     */

    public StringCollator makeUcaCollator(String uri, Configuration config) throws XPathException {
        return ICULibrary.makeICUCollator(uri, config);
    }
}

// Copyright (c) 2009 Saxonica Limited.