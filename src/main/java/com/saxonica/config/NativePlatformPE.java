package com.saxonica.config;

import com.saxonica.ee.domino.DominoTreeModel;
import com.saxonica.functions.extfn.EXPathArchive.Archive;
import net.sf.saxon.Configuration;
import net.sf.saxon.Query;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.option.cpp.NativePlatform;
import net.sf.saxon.option.cpp.ProcessorDataAccumulator;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.SaxonCExceptionLongHandler;
import net.sf.saxon.option.exslt.Random;
import net.sf.saxon.option.exslt.Sets;
import net.sf.saxon.trans.XPathException;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.word.WordFactory;


/**
 * Implementation of the Platform class containing methods specific to the C/C++ platform
 * (as distinct from .NET and Java) for Saxon-PE and Saxon-EE.
 * This is a singleton class with no instance data.
 * The key differences to the JavaPlatformPE is that we have no reference to registerAllBuiltInObjectModels and the
 * environment variable for SaxonC we use SAXONC_HOME
 */
public class NativePlatformPE extends NativePlatform {


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
     * The constructor is called during the static initialization of the Configuration
     */

    public NativePlatformPE() {
    }


    /**
     * Perform platform-specific initialization of the configuration
     */

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
            //config.setCollectionURIResolver(new StandardCollectionURIResolver());

//#if EE==true
            if (config instanceof EnterpriseConfiguration) {
                config.setConfigurationProperty("http://saxon.sf.net/feature/generateByteCode", "false");
                config.registerExternalObjectModel(DominoTreeModel.getInstance());
            }
//#endif
        }
    }


    @CEntryPoint(name = "j_run_validate", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long run_query(IsolateThread thread, ObjectHandle processorDataRef){

        String[] args;


        if(processorDataRef == WordFactory.nullPointer()) {
            return SaxonCAPI.STATUS_FAIL;
        }   else {
           ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
           args = processorDataAccumulator.getParameterNames();
        }

        // This attempted to do validation?

        return -1;
    }


    /**
     * Add the platform-specific function libraries to a function library list.
     *
     * @param list         the function library list that is to be extended
     * @param config       the Configuration
     * @param hostLanguage the host language, for example Configuration.XQUERY
     */

    public void addFunctionLibraries(FunctionLibraryList list, Configuration config, int hostLanguage) {
        super.addFunctionLibraries(list, config, hostLanguage);
        if (config instanceof ProfessionalConfiguration) {
            FunctionLibrary extensionBinder = ((ProfessionalConfiguration) config).getExtensionBinder("java");
            list.addFunctionLibrary(extensionBinder);
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

// Copyright (c) 2009-2023 Saxonica Limited.
