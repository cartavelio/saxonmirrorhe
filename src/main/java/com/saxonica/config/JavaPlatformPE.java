package com.saxonica.config;

import com.saxonica.config.pe.DynamicLoaderPE;
import com.saxonica.functions.extfn.EXPathArchive.Archive;
import net.sf.saxon.Configuration;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.java.JavaPlatform;
import net.sf.saxon.lib.IDynamicLoader;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.option.exslt.Random;
import net.sf.saxon.option.exslt.Sets;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.trans.XPathException;

/**
 * Implementation of the Platform class containing methods specific to the Java platform
 * (as distinct from .NET) for Saxon-PE and Saxon-EE.
 * This is a singleton class with no instance data.
 */
public class JavaPlatformPE extends JavaPlatform {

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

    public JavaPlatformPE() {
    }

    /**
     * Perform platform-specific initialization of the configuration
     */

    @Override
    public void initialize(Configuration config) {
        super.initialize(config);
        if (config instanceof ProfessionalConfiguration) {
            JavaExtensionLibrary javaLib = new JavaExtensionLibrary((ProfessionalConfiguration) config);
            //javaLib.declareJavaClass(NamespaceConstant.SAXON, Extensions.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_SETS, Sets.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_MATH, net.sf.saxon.option.exslt.Math.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_DATES_AND_TIMES, net.sf.saxon.option.exslt.Date.class);
            javaLib.declareJavaClass(NamespaceConstant.EXSLT_RANDOM, Random.class);
            //javaLib.declareJavaClass(EXPathFile.NAMESPACE, EXPathFile.class);
            //javaLib.declareJavaClass(EXPathBinary.NAMESPACE, EXPathBinary.class);
            javaLib.declareJavaClass(Archive.NAMESPACE.toString(), Archive.class);

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

//#if EE==true
//            if (config instanceof EnterpriseConfiguration && config.getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
//                config.setCollectionURIResolver(new MultithreadedCollectionURIResolver());
//            }
//#endif
        }
    }

    /**
     * Get the default DynamicLoader for the platform
     *
     * @return the default DynamicLoader
     */

    public IDynamicLoader getDefaultDynamicLoader() {
        return new DynamicLoaderPE();
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
     * Register all the external object models that are provided as standard
     * with the relevant edition of Saxon for this Configuration
     *
     * @since 9.3
     */

    @Override
    public void registerAllBuiltInObjectModels(Configuration config) {
        try {
            config.registerExternalObjectModel(AxiomObjectModel.getInstance());
        } catch (NoClassDefFoundError err) {
            // This happens if Axiom is not on the classpath
        }

        try {
            config.registerExternalObjectModel(JDOM2ObjectModel.getInstance());
        } catch (NoClassDefFoundError err) {
            // This happens if JDOM2 is not on the classpath
        }

        try {
            config.registerExternalObjectModel(DOM4JObjectModel.getInstance());
        } catch (NoClassDefFoundError err) {
            // This happens if DOM4J is not on the classpath
        }

        try {
            config.registerExternalObjectModel(XOMObjectModel.getInstance());
        } catch (NoClassDefFoundError e) {
            // This happens if XOM is not on the classpath
        }
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
        StringCollator collator = ICULibrary.makeICUCollator(uri, config);
        if (collator == null) {
            // fallback
            return super.makeUcaCollator(uri, config);
        }
        return collator;
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

// Copyright (c) 2009-2022 Saxonica Limited