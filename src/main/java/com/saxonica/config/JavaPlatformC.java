package com.saxonica.config;

import com.saxonica.ee.domino.DominoTreeModel;
import com.saxonica.functions.extfn.EXPathArchive.Archive;
import com.saxonica.functions.extfn.EXPathBinary;
import com.saxonica.functions.extfn.EXPathFile;
import com.saxonica.functions.extfn.Extensions;
import net.sf.saxon.Configuration;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.java.JavaPlatform;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.option.exslt.Random;
import net.sf.saxon.option.exslt.Sets;
import net.sf.saxon.trans.XPathException;


/**
 * Implementation of the Platform class containing methods specific to the Java platform
 * (as distinct from .NET) for Saxon-PE and Saxon-EE.
 * This is a singleton class with no instance data.
 * The key differences to the JavaPlatformPE is that we have no reference to registerAllBuiltInObjectModels and the
 * environment variable for Saxon/C we use SAXONC_HOME
 */
public class JavaPlatformC extends JavaPlatform {


     /**
     * Is an ICU collator loaded?
     * @return  true if ICU collations appear loaded
     */
    @Override
    public boolean hasICUCollator() {
        return ICULibrary.isICUCollatorAvailable();
    }
    /**
     * Is an ICU numberer loaded?
     * @return  true if ICU numberers appear loaded
     */
    @Override
    public boolean hasICUNumberer() {
        return ICULibrary.hasNumberer();
    }

    /**
     * The constructor is called during the static initialization of the Configuration
     */

    public JavaPlatformC() {
    }

    /**
     * Perform platform-specific initialization of the configuration
     */

    @Override
    public void initialize(Configuration config) {
        super.initialize(config);
        if (config instanceof ProfessionalConfiguration) {
                JavaExtensionLibrary javaLib = new JavaExtensionLibrary((ProfessionalConfiguration) config);
                javaLib.declareJavaClass(NamespaceConstant.SAXON, Extensions.class);
                javaLib.declareJavaClass(NamespaceConstant.EXSLT_SETS, Sets.class);
                javaLib.declareJavaClass(NamespaceConstant.EXSLT_MATH, net.sf.saxon.option.exslt.Math.class);
                javaLib.declareJavaClass(NamespaceConstant.EXSLT_DATES_AND_TIMES, net.sf.saxon.option.exslt.Date.class);
                javaLib.declareJavaClass(NamespaceConstant.EXSLT_RANDOM, Random.class);
                javaLib.declareJavaClass(EXPathFile.NAMESPACE, EXPathFile.class);
                javaLib.declareJavaClass(EXPathBinary.NAMESPACE, EXPathBinary.class);
                javaLib.declareJavaClass(Archive.NAMESPACE, Archive.class);

                // For sql:connect and sql:query, avoid creating a static dependency on the SQL module, which is in a separate JAR
                //Dropped from the 9.7 spec
                //javaLib.declareJavaClass("http://saxon.sf.net/sql", "net.sf.saxon.option.sql.SQLExtensionFunctions");

                ((ProfessionalConfiguration) config).setExtensionBinder("java", javaLib);
                JavaLocalizerFactory jlf = new JavaLocalizerFactory(config);
                jlf.setLocalizationClass("da", "net.sf.saxon.option.local.Numberer_da");
                jlf.setLocalizationClass("de", "net.sf.saxon.option.local.Numberer_de");
                jlf.setLocalizationClass("de", "net.sf.saxon.option.local.Numberer_de");
                jlf.setLocalizationClass("en", "net.sf.saxon.expr.number.Numberer_en");
                jlf.setLocalizationClass("fr", "net.sf.saxon.option.local.Numberer_fr");
                jlf.setLocalizationClass("fr-BE", "net.sf.saxon.option.local.Numberer_frBE");
                jlf.setLocalizationClass("it", "net.sf.saxon.option.local.Numberer_it");
                jlf.setLocalizationClass("nl", "net.sf.saxon.option.local.Numberer_nl");
                jlf.setLocalizationClass("nl-BE", "net.sf.saxon.option.local.Numberer_nlBE");
                jlf.setLocalizationClass("sv", "net.sf.saxon.option.local.Numberer_sv");
                config.setLocalizerFactory(jlf);
            //config.setCollectionURIResolver(new StandardCollectionURIResolver());

//#if EE==true
            if (config instanceof EnterpriseConfiguration) {
                config.setConfigurationProperty("http://saxon.sf.net/feature/generateByteCode", "false");
                config.registerExternalObjectModel(DominoTreeModel.getInstance());
            }
//#endif
        }
    }


    /**
     * Add the platform-specific function libraries to a function library list.
     *
     * @param list         the function library list that is to be extended
     * @param config       the Configuration
     * @param hostLanguage the host language, for example Configuration.XQUERY
     */

    @Override
    public void addFunctionLibraries(FunctionLibraryList list, Configuration config, int hostLanguage) {
        super.addFunctionLibraries(list, config, hostLanguage);
        if (config instanceof ProfessionalConfiguration) {
            FunctionLibrary extensionBinder = ((ProfessionalConfiguration) config).getExtensionBinder("java");
            list.addFunctionLibrary(extensionBinder);
        }
    }

    /**
     * Return the name of the directory in which the software is installed (if available)
     *
     * @param edition The edition of the software that is loaded ("HE", "PE", or "EE")
     * @param config  the Saxon configuration
     * @return the name of the directory in which Saxon/C is installed, if available, or null otherwise
     */

    @Override
    public String getInstallationDirectory(String edition, Configuration config) {
        return System.getenv("SAXONC_HOME");
    }


    /**
     * If available, make a collation using the ICU-J Library
     *
     * @param uri    the collation URI (which will always be a UCA collation URI as defined in XSLT 3.0)
     * @param config the Saxon configuration
     * @return the collation, or null if not available
     * @throws net.sf.saxon.trans.XPathException if the URI is malformed in some way
     */

    @Override
    public StringCollator makeUcaCollator(String uri, Configuration config) throws XPathException {
        return ICULibrary.makeICUCollator(uri, config);
    }

   /* public StringCollator makeICUCollator(String uri, Configuration config) throws XPathException {
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)
                && hasICUCollator()) {
            return new SimpleCollation(new UCACollator(uri));
        } else {
            return null;
        }
    }*/

}

// Copyright (c) 2009-2020 Saxonica Limited