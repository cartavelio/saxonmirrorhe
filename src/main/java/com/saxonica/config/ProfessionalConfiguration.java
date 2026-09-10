package com.saxonica.config;


import com.saxonica.config.pe.ConfigurationReaderPE;
import com.saxonica.config.pe.StyleNodeFactoryPE;
import com.saxonica.expr.ObjectLookupExpression;
import com.saxonica.expr.SaxonSyntaxExtension;
import com.saxonica.expr.StaticQueryContextPE;
import com.saxonica.expr.XQueryParserExtensionPE;
import com.saxonica.functions.extfn.EXPathBinaryFunctionSet;
import com.saxonica.functions.extfn.EXPathFileFunctionSet;
import com.saxonica.functions.extfn.ObjectMap;
import com.saxonica.functions.extfn.VendorFunctionSetPE;
import com.saxonica.functions.registry.XQUpdate10FunctionSet;
import com.saxonica.serialize.SerializerFactoryPE;
import com.saxonica.trans.ModePE;
import com.saxonica.trans.PackageLoaderPE;
import com.saxonica.xsltextn.ExtensionElementCreator;
import com.saxonica.xsltextn.ExtensionElementFactory;
import com.saxonica.xsltextn.style.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.functions.MathFunctionSet;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.*;
import net.sf.saxon.lib.*;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.PatternParser;
import net.sf.saxon.pull.PullProvider;
import net.sf.saxon.pull.PullSource;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.style.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.*;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.JavaExternalObjectType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.ObjectValue;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import java.io.BufferedReader;
import java.io.Reader;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Map;


/**
 * This class holds details of user-selected configuration options for a set of transformations
 * and/or queries. When running XSLT, the preferred way of setting configuration options is via
 * the JAXP TransformerFactory interface, but the Configuration object provides a finer
 * level of control. As yet there is no standard API for XQuery, so the only way of setting
 * Configuration information is to use the methods on this class directly.
 * <p>As well as holding configuration settings, this class acts as a factory for classes
 * providing service in particular areas: error handling, URI resolution, and the like. Some
 * of these services are chosen on the basis of the current platform (Java or .NET), some vary
 * depending whether the environment is schema-aware or not.</p>
 * <p>The <code>Configuration</code> provides access to a {@link net.sf.saxon.om.NamePool} which is used to manage
 * all the names used in stylesheets, queries, schemas, and source and documents: the NamePool
 * allocates integer codes to these names allowing efficient storage and comparison. Normally
 * there will be a one-to-one relationship between a <code>NamePool</code> and a <code>Configuration</code>.
 * It is possible, however, for several <code>Configuration</code> objects to share the same
 * <code>NamePool</code>. Until Saxon 8.9, by default all <code>Configuration</code> objects
 * shared a single <code>NamePool</code> unless configured otherwise; this changed in 8.9 so that
 * the default is to allocate a new <code>NamePool</code> for each <code>Configuration</code>.</p>
 * <p>The <code>Configuration</code> establishes the scope within which node identity is managed.
 * Every document belongs to a <code>Configuration</code>, and every node has a distinct identity
 * within that <code>Configuration</code>. In consequence, it is not possible for any query or
 * transformation to manipulate multiple documents unless they all belong to the same
 * <code>Configuration</code>.</p>
 * <p>Saxon-EE has a subclass of the <code>Configuration</code> class which provides the additional
 * services needed for schema-aware processing. The {@link com.saxonica.config.EnterpriseConfiguration}
 * also holds a cache of loaded schema components used for compiling schema-aware transformations
 * and queries, and for validating instance documents.</p>
 * <p>Since Saxon 8.4, the JavaDoc documentation for Saxon attempts to identify interfaces
 * that are considered stable, and will only be changed in a backwards-incompatible way
 * if there is an overriding reason to do so. These interfaces and methods are labelled
 * with the JavaDoc "since" tag. The value 8.n indicates a method in this category that
 * was introduced in Saxon version 8.n: or in the case of 8.4, that was present in Saxon 8.4
 * and possibly in earlier releases. (In some cases, these methods have been unchanged for
 * a long time.) Methods without a "since" tag, although public, are provided for internal
 * use or for use by advanced users, and are subject to change from one release to the next.
 * The presence of a "since" tag on a class or interface indicates that there are one or more
 * methods in the class that are considered stable; it does not mean that all methods are
 * stable.</p>
 *
 * @since 9.2
 */


public class ProfessionalConfiguration extends Configuration {

    private transient HashMap<String, FunctionLibrary> extensionBinders;
    private boolean licenseFound = false;
    private boolean licenseNotFound = false;

    private transient Verifier verifier;
    private transient HashMap<NamespaceUri, ExtensionElementCreator> xsltExtensionElementNamespaceMap;
    protected BuiltInFunctionSet vendorFunctionSet;
    // Static cache of JavaExternalObjectType objects representing Java classes
    private final HashMap<Class, JavaExternalObjectType> externalObjectTypeCache = new HashMap<>();


    /**
     * Create a Saxon Professional Edition configuration object with default settings for all options.
     *
     * @since 9.2
     */

    public ProfessionalConfiguration() {
        super();
        //#if CSHARP==false
        if (!("com.saxonica.config.ProfessionalConfiguration".equals(getClass().getName()) ||
                "com.saxonica.config.EnterpriseConfiguration".equals(getClass().getName()))) {
            throw new LicenseException("Unauthorized subclass of Configuration", LicenseException.WRONG_CONFIGURATION);
        }
        //#endif
        verifier = makeVerifier();
        declareBuiltInExtensionElementNamespaces();
        declareBuiltInExtensionFunctions();
        Version.platform.registerAllBuiltInObjectModels(this);
        setSerializerFactory(new SerializerFactoryPE(this));
        setStaticQueryContextFactory(new StaticQueryContextFactory() {
            @Override
            public StaticQueryContext newStaticQueryContext(Configuration config, boolean copyFromDefault) {
                return new StaticQueryContextPE(ProfessionalConfiguration.this, copyFromDefault);
            }
        });
        internalSetBooleanProperty(FeatureCode.DISABLE_XSL_EVALUATE, FeatureKeys.DISABLE_XSL_EVALUATE, false);
    }

    @CSharpReplaceBody(code="return new Saxon.Impl.Overrides.Verifier();")
    private Verifier makeVerifier() {
        return new JavaVerifier();
    }


    @Override
    protected ConfigurationReader makeConfigurationReader() {
        return new ConfigurationReaderPE();
    }

    /**
     * Static method to instantiate a professional configuration.
     * <p>On the .NET platform, this method should not be called unless it is known that the assembly
     * saxon9sa.dll has already been loaded. This can be achieved by an appropriate call on Assembly.Load():
     * for an example, see the C# Configuration.cs class in the Saxon.Api namespace.</p>
     * <p>This method fails if Saxon-EE cannot be loaded, but it does not fail if there is no license
     * available. In that case it returns a schema-aware configuration object, but any attempt to use
     * schema-aware processing will fail.
     *
     * @param classLoader - the class loader to be used. If null, the context class loader for the current
     *                    thread is used.
     * @param className   - the name of the schema aware configuration class. Defaults to
     *                    "com.saxonica.config.ProfessionalConfiguration" if null is supplied. This allows an assembly
     *                    qualified name to be supplied under .NET. The class, once instantiated, must be an instance
     *                    of Configuration, but despite the name of this method there is nothing that requires it to
     *                    be schema-aware.
     * @return the new ProfessionalConfiguration
     * @throws RuntimeException if the Saxon-PE product cannot be loaded
     * @since 9.2 (renamed from makeSchemaAwareConfiguration)
     */

    public static Configuration makeProfessionalConfiguration(ClassLoader classLoader, /*@Nullable*/ String className)
            throws RuntimeException {
        if (className == null) {
            className = "com.saxonica.config.ProfessionalConfiguration";
        }
        try {
            return instantiateConfiguration(className, classLoader);
        } catch (Exception e) {
            throw new RuntimeException(e.getMessage(), e);
        }
    }

    /**
     * Get the edition code identifying this configuration: "HE", "PE" or "EE"
     */

    @Override
    public String getEditionCode() {
        return "PE";
    }

    /**
     * Get the TypeHierarchy: a cache holding type information
     *
     * @return the type hierarchy cache
     */

    @Override
    public TypeHierarchy getTypeHierarchy() {
        if (typeHierarchy == null) {
            typeHierarchy = new TypeHierarchy(this);
        }
        return typeHierarchy;
    }

    @Override
    public PackageLoaderPE makePackageLoader() {
        return new PackageLoaderPE(this);
    }

    /**
     * Obsolete interface for setting configuration properties: retained for backwards compatibility
     * @param name the name of the property (in the form of a URI)
     * @param value the value of the property
     */

    public void setConfigurationProperty(String name, Object value) {
        if (name.equals("http://saxonica.com/oem-data")) {
            supplyLicenseKey(new BufferedReader(new StringReader((String) value)));
        } else if (name.equals(Feature.LICENSE_FILE_LOCATION.name)) {
            synchronized(verifier) {
                verifier.loadLicense((String) value);
                licenseFound = true;
                licenseNotFound = false;
                setLicensedFeatures();
            }
        } else {
            super.setConfigurationProperty(name, value);
        }
    }


    /**
     * Set a property of the configuration. This method underpins the setAttribute() method of the
     * TransformerFactory implementation, and is provided
     * to enable setting of Configuration properties using URIs without instantiating a TransformerFactory:
     * specifically, this may be useful when running XQuery, and it is also used by the Validator API
     *
     * <p>From Saxon 9.9, an alternative interface is available: {@link #setConfigurationProperty(Feature, Object)}.
     * The new interface is more efficient because it avoids expensive string comparisons. The old interface is
     * retained mainly for compatibility, and also because there are a few cases where it cannot easily be replaced,
     * for example when using composite feature URIs to delegate configuration options to the XML parser.</p>
     *
     * @param feature identifies the property to be set. See the class {@link net.sf.saxon.lib.Feature} for
     *              constants representing the property names that can be set.
     * @param value the value of the property
     * @throws IllegalArgumentException if the property name is not recognized or if the value is not
     *                                  a valid value for the named property
     */

    @Override
    @CSharpModifiers(code = {"public", "override"})
    protected void setFeature(FeatureData feature, Object value) {
        String name = feature.uri;
        int code = feature.code;
        if (name.equals("http://saxonica.com/oem-data")) {
            supplyLicenseKey(new BufferedReader(new StringReader((String) value)));
        } else if (code == FeatureCode.LICENSE_FILE_LOCATION) {
            synchronized(verifier) {
                verifier.loadLicense((String) value);
                licenseFound = true;
                licenseNotFound = false;
                setLicensedFeatures();
            }
            //#if CSHARP==false
        } else if (code == FeatureCode.ALLOW_OLD_JAVA_URI_FORMAT) {
            internalSetBooleanProperty(FeatureCode.ALLOW_OLD_JAVA_URI_FORMAT, FeatureKeys.ALLOW_OLD_JAVA_URI_FORMAT, value);
            boolean b = requireBoolean(FeatureKeys.ALLOW_OLD_JAVA_URI_FORMAT, value);
            ((JavaExtensionLibrary) getExtensionBinder("java")).setStrictJavaUriFormat(!b);
            //#endif
        } else {
            super.setFeature(feature, value);
        }
    }

    /**
     * This method allows the contents of a license key to be supplied programmatically.
     * This must be a license file as issued to authorized OEM distributors of Saxon-EE.
     *
     * @param reader A reader that will be used to read the contents of the license file
     *               (for example, a StringReader)
     * @throws IllegalArgumentException if the key supplied is not a valid OEM license key
     */

    public void supplyLicenseKey(Reader reader) {
        verifier.supplyLicenseKey(reader);
        licenseFound = true;
        licenseNotFound = false;
        setLicensedFeatures();
    }

    protected final void forceLoadLicense() throws LicenseException {
        verifier.loadLicense(this);
    }

    /**
     * Share licensing data stored in one configuration with another configuration. Where many configurations
     * are created, this reduces the cost of repeatedly reading and analysing license key information.
     *
     * @param other another Configuration, whose license information will be imported into this Configuration.
     */

    @Override
    public void importLicenseDetails(Configuration other) {
        if (other instanceof ProfessionalConfiguration) {
            ProfessionalConfiguration pc = (ProfessionalConfiguration)other;
            this.verifier = pc.verifier;
            this.licenseFound = pc.licenseFound;
            this.licenseNotFound = pc.licenseNotFound;
            setLicensedFeatures();
        }
    }

    /**
     * This method is called to read a feature of the license. This method should be called
     * only if the caller requires access to the feature.
     *
     * @param name the name of the requested feature
     * @return the feature value of the license.
     * @throws RuntimeException if no license is available
     */

    protected String getFeature(String name) {
        String f = verifier.getFeature(name, this);
        if (!licenseFound) {
            licenseFound = true;
            licenseNotFound = false;
            setLicensedFeatures();
        }
        return f;
    }

    /**
     * Check whether a particular feature is licensed, with a fatal error if it is not
     *
     * @param feature the feature in question, identified by a constant in class {@link net.sf.saxon.Configuration.LicenseFeature}
     * @param name    the name of the feature for use in diagnostics
     * @param localLicenseId identifies a local license held within a SEF file, or -1 if not appicable
     * @throws net.sf.saxon.trans.LicenseException
     *          if the feature is not licensed
     */

    @Override
    public void checkLicensedFeature(int feature, String name, int localLicenseId) throws LicenseException {
        if (feature != LicenseFeature.PROFESSIONAL_EDITION) {
            throw new LicenseException("Requested feature (" + name + ") requires Saxon-EE, " +
                                               "but the current Configuration is an instance of com.saxonica.config.ProfessionalConfiguration",
                                       LicenseException.WRONG_CONFIGURATION);
        }
        if (localLicenseId >= 0 && isFeatureAllowedBySecondaryLicense(localLicenseId, feature)) {
            return;
        }
        if (licenseNotFound) {
            try {
                loadLicense(); // We know this will fail, but we want the capture the reason
            } catch (Exception err) {
                int reason = err instanceof LicenseException ? ((LicenseException) err).getReason() : LicenseException.CANNOT_READ;
                String msg = err.getMessage() + ". A license is needed to use " + name;
                throw new LicenseException(msg, reason);
            }
        }
        if (licenseFound) {
            if (!testFeature(feature)) {
                throw new LicenseException("Requested feature (" + name + ") requires Saxon-EE, " +
                        "but the current Configuration is an instance of com.saxonica.config.ProfessionalConfiguration",
                        LicenseException.WRONG_CONFIGURATION);
            }
        } else {
            synchronized (verifier) {
                loadLicense();
                licenseFound = true;
                reportIfMissingLicenseFeature(feature, name);
            }
        }
    }

    /**
     * Instruct Saxon to run as if no license is available. This method is provided for testing purposes,
     * so that tests with and without a license can be run without making changes to the classpath.
     */

    @Override
    public void disableLicensing() {
        verifier.disableLicensing();
        licenseNotFound = true;
        licenseFound = false;
    }

    @Override
    public boolean isFeatureAllowedBySecondaryLicense(int localLicenseId, int feature) {
        return verifier.isFeatureAllowedBySecondaryLicense(localLicenseId, feature);
    }

    protected void reportIfMissingLicenseFeature(int feature, String name) {
        if (!testFeature(feature)) {
            throw new LicenseException("A valid license was found (" + verifier.getSerialNumber() +
                    "), but does not support the requested feature (" + name + ")",
                    LicenseException.WRONG_FEATURES);
        }
    }


    /**
     * Determine if a particular feature is licensed. If this requires reading the license file to find out,
     * and the license file cannot be read, then a message is output to the standard error output if and only
     * if the configuration option {@link FeatureKeys#TIMING} is set (corresponding to -t on the command line).
     *
     * @param feature the feature in question, identified by a constant in class {@link net.sf.saxon.Configuration.LicenseFeature}
     * @return true if the feature is available for use.
     */

    @Override
    public final boolean isLicensedFeature(int feature) throws LicenseException {
        if (licenseNotFound) {
            return false;
        }
        //noinspection SynchronizeOnNonFinalField
        synchronized (verifier) {
            if (!licenseFound) {
                try {
                    loadLicense();
                } catch (LicenseException e) {
                    if (e.getReason() == LicenseException.NOT_FOUND) {
//                        if (isTiming()) {
//                            getStandardErrorOutput().println(e.getMessage() + ". Running with licensable features disabled");
//                        }
//                        new NullPointerException().printStackTrace();
                        licenseNotFound = true;
                        return false;
                    } else {
                        throw e;
                    }
                }
            }
            licenseFound = true;
            //setLicensedFeatures();
            return testFeature(feature);
        }
    }

    /**
     * Get the value of a named license feature
     *
     * @param name the name of the feature
     * @return the value of the feature if present, or null otherwise
     */

    @Override
    public final String getLicenseFeature(String name) {
        return verifier.getFeature(name, this);
    }

    /**
     * Attempt to load the license file. If it is an evaluation license, set the appropriate restrictions.
     *
     * @throws LicenseException if the license cannot be loaded.
     */

    protected final void loadLicense() throws LicenseException {
        verifier.loadPrimaryLicense(this);
        if (System.currentTimeMillis() % 100 == 42 && "yes".equals(verifier.getFeature("Evaluation", this))) {
            SerializerFactoryPE sf = new SerializerFactoryPE(this) {

                @Override
                @CSharpModifiers(code={"public", "override"})
                public Receiver getReceiver(Result result, SerializationProperties parameters, PipelineConfiguration pipe) throws XPathException {
                    Receiver r = super.getReceiver(result, parameters, pipe);
                    if (r instanceof EvaluationNuisance) {
                        return r;
                    } else {
                        return new EvaluationNuisance(r);
                    }
                }
            };
            setSerializerFactory(sf);
        }
        setLicensedFeatures();
    }

    protected void setLicensedFeatures() {
        // overridden in EnterpriseConfiguration
    }

    /**
     * Register a local license file (for use within a single transformation (etc))
     *
     * @param dmk the license in encoded form
     * @return an integer identifying this license uniquely within the configuration, or -1 if not accepted
     */

    @Override
    public int registerLocalLicense(String dmk) {
        return verifier.registerSecondaryLicense(dmk);
    }

    /**
     * Test whether a particular feature is available, given that a valid license has been successfully loaded.
     *
     * @param feature a constant in class {@link net.sf.saxon.Configuration.LicenseFeature}
     * @return true if the feature is available, otherwise false
     */

    protected boolean testFeature(int feature) {
        return feature == LicenseFeature.PROFESSIONAL_EDITION;
    }

    /**
     * Ask if the configuration is known to be unlicensed
     *
     * @return true if a previous attempt to load a license was made, and no license was found
     */

    public boolean isLicenseNotFound() {
        return licenseNotFound;
    }

    /**
     * Ask if the configuration is known to be licensed
     *
     * @return true if a previous attempt to load a license was made, and the license was found
     *         (and found to be valid)
     */

    public boolean isLicenseFound() {
        return licenseFound;
    }


    /**
     * Display a message about the license status. Used from command-line tools only.
     */

    @Override
    public void displayLicenseMessage() {
        if (licenseNotFound) {
            if (isTiming()) {
                getLogger().info(
                        "No license file found - running with licensable features disabled");
            }
        } else {
            try {
                verifier.displayLicenseMessage(this);
                licenseFound = true;
                setLicensedFeatures();
            } catch (LicenseException err) {
                licenseFound = false;
                licenseNotFound = true;
                if (err.getReason() == LicenseException.NOT_FOUND) {
                    getLogger().info(
                            "No license file found - running with licensable features disabled");
                    //new NullPointerException().printStackTrace();
                } else {
                    throw err;
                }

            }
        }
    }

    /**
     * Factory method to make a StyleNodeFactory, used for constructing elements
     * in a stylesheet document
     *
     * @return the StyleNodeFactory used in this Configuration
     * @param compilation the XSLT compilation object
     */

    @Override
    public StyleNodeFactory makeStyleNodeFactory(Compilation compilation) {
        PackageData pack = compilation.getPackageData();
        if (pack != null) {
            int locLic = pack.getLocalLicenseId();
            int feature = LicenseFeature.PROFESSIONAL_EDITION;
            if (!(isFeatureAllowedBySecondaryLicense(locLic, feature) || isLicensedFeature(feature))) {
                return super.makeStyleNodeFactory(compilation);
            }
        }
        return new StyleNodeFactoryPE(this, compilation);
    }

    /**
     * Make an expression Parser for a specified version of XPath or XQuery
     *
     * @param language        set to "XP" (XPath) or "XQ" (XQuery) or "PATTERN" (XSLT Patterns)
     * @param updating        indicates whether or not XQuery update syntax may be used. Note that XQuery Update
     *                        is supported only in Saxon-EE
     * @return the XPath or Query Parser
     * @throws net.sf.saxon.trans.XPathException if this version of Saxon does not support the
     *                                           requested options
     */

    @Override
    public XPathParser newExpressionParser(String language, boolean updating) throws XPathException {
        if (updating) {
            throw new IllegalArgumentException("XQuery Update requires Saxon-EE");
        }
        if ("XQ".equals(language)) {
            XQueryParser parser = new XQueryParser();
            if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
                parser.setParserExtension(new XQueryParserExtensionPE());
            }
            return parser;
        } else if ("XP".equals(language)) {
            XPathParser parser = super.newExpressionParser(language, updating);
            if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
                parser.setParserExtension(new SaxonSyntaxExtension());
            }
            return parser;
        } else if ("PATTERN".equals(language)) {
            PatternParser parser = new PatternParser();
            if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
                parser.setParserExtension(new SaxonSyntaxExtension());
            }
            return parser;
        } else {
            return super.newExpressionParser(language, updating);
        }
    }



    @Override
    public BuiltInFunctionSet getXQueryUpdateFunctionSet() {
        return XQUpdate10FunctionSet.getInstance();
    }

    /**
     * Set the FunctionLibrary used to bind calls on extension functions. This allows the
     * rules for identifying extension functions to be customized (in principle, it would
     * allow support for extension functions in other languages to be provided).
     * <p>When an application supplies its own FunctionLibrary for binding extension functions,
     * this replaces the default binding mechanism for Java extension functions, namely
     * {@link JavaExtensionLibrary}. It thus disables the function libraries
     * for built-in Saxon extensions and for EXSLT extensions. It is possible to create a
     * function library that adds to the existing mechanisms, rather than replacing them,
     * by supplying as the FunctionLibrary a {@link net.sf.saxon.functions.FunctionLibraryList}
     * that itself contains two FunctionLibrary objects: a JavaExtensionLibrary, and a user-written
     * FunctionLibrary.</p>
     *
     * @param scheme The scheme served by the extension binder. Typically this will be one
     *               of "java" or "clitype". However, user-defined extension binders for other
     *               schemes can be registered. Note that the scheme name does not have to be
     *               related to the actual namespace used in function calls; all registered
     *               function libraries have the opportunity to bind any extension function call.
     *               The significance is purely local to this class: setting an extension binder
     *               for a given scheme replaces the existing extension binder for that scheme.
     *               The two schemes used by Saxon itself are "java" for dynamically-bound
     *               Java extension functions, and "clitype" for dynamically-bound .NET extension
     *               functions
     * @param binder The FunctionLibrary object used to locate implementations of extension
     *               functions, based on their name and arity
     */

    public void setExtensionBinder(String scheme, FunctionLibrary binder) {
        if (extensionBinders == null) {
            extensionBinders = new HashMap<>(4);
        }
        extensionBinders.put(scheme, binder);
    }


    /**
     * Get the FunctionLibrary used to bind calls on extension functions on the specified
     * platform.
     * <p>This mechanism is for advanced users only, and the details are subject to change.</p>
     *
     * @param scheme The URI scheme served by the extension binder. This will typically be one
     *               of "java" or "clitype". On the Java platform, the only scheme currently supported is "java";
     *               on the .NET platform, the "java" and "clitype" schemes coexist.
     * @return the registered FunctionLibrary for extension functions if one has been
     *         registered; or null otherwise
     */

    public FunctionLibrary getExtensionBinder(String scheme) {
        if (extensionBinders == null) {
            return null;
        }
        return extensionBinders.get(scheme);
    }

    /**
     * Instantiate a Saxon extension function. This generally requires Saxon-PE or higher,
     * so it will fail with this Configuration
     *
     * @param localName the local name of the function in the Saxon namespace
     * @param arity     the function arity
     */

    @Override
    public SystemFunction bindSaxonExtensionFunction(String localName, int arity) throws XPathException {
        checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:" + localName + "#" + arity, -1);
        return getVendorFunctionSet().makeFunction(localName, arity);
    }

    /**
     * Add the registered extension binders to a function library.
     * This method is intended primarily for internal use
     */

    @Override
    public void addExtensionBinders(FunctionLibraryList list) {
        if (getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS) && extensionBinders != null) {
            for (FunctionLibrary functionLibrary : extensionBinders.values()) {
                list.addFunctionLibrary(functionLibrary);
            }
        }
    }

    /**
     * Get a system function. This can be any function defined in XPath 3.1 functions and operators,
     * including functions in the math, map, and array namespaces. It can also be a Saxon extension
     * function, provided a licensed Processor is used.
     *
     * @param name  the name of the required function
     * @param arity the arity of the required function
     * @return the requested function, or null if there is no such function. Note that some functions
     * (those with particular context dependencies) may be unsuitable for dynamic calling.
     */

    @Override
    public Function getSystemFunction(StructuredQName name, int arity) {
        try {
            if (staticContextForSystemFunctions == null) {
                staticContextForSystemFunctions = new IndependentContext(this);
            }
            FunctionLibraryList lib = new FunctionLibraryList();
            lib.addFunctionLibrary(XPath31FunctionSet.getInstance());
            lib.addFunctionLibrary(getBuiltInExtensionLibraryList());
            lib.addFunctionLibrary(new ConstructorFunctionLibrary(this));
            lib.addFunctionLibrary(getIntegratedFunctionLibrary());
            lib.addFunctionLibrary(getExtensionBinder("java"));
            SymbolicName.F symbolicName = new SymbolicName.F(name, arity);
            return lib.getFunctionItem(symbolicName, staticContextForSystemFunctions);
        } catch (XPathException e) {
            return null;
        }
    }

    public FunctionLibraryList getBuiltInExtensionLibraryList() {
        if (!isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
            return super.getBuiltInExtensionLibraryList();
        }
        if (builtInExtensionLibraryList == null) {
            builtInExtensionLibraryList = makeBuiltInExtensionLibraryList();
        }
        return builtInExtensionLibraryList;
    }

    private synchronized FunctionLibraryList makeBuiltInExtensionLibraryList() {
        vendorFunctionSet = makeVendorFunctionSet();
        FunctionLibraryList result = new FunctionLibraryList();
        result.addFunctionLibrary(vendorFunctionSet);
        result.addFunctionLibrary(MathFunctionSet.getInstance());
        result.addFunctionLibrary(MapFunctionSet.getInstance());
        result.addFunctionLibrary(ArrayFunctionSet.getInstance());
        result.addFunctionLibrary(EXPathBinaryFunctionSet.getInstance());
        result.addFunctionLibrary(ExsltCommonFunctionSet.getInstance());
        if (getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS)) {
            result.addFunctionLibrary(EXPathFileFunctionSet.getInstance());
            //#if CSHARP==false
            OnDemandFunctionSet sql = new OnDemandFunctionSet(
                    this, NamespaceUri.SQL, "com.saxonica.functions.sql.SQLFunctionSet");
            result.addFunctionLibrary(sql);
            //#endif
        }
        return result;
    }

    /**
     * Create a function library for Saxon-defined extension functions available in Professional Edition
     *
     * @return the new function library
     */


    protected BuiltInFunctionSet makeVendorFunctionSet() {
        if (isLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION)) {
            return new VendorFunctionSetPE(this);
        } else {
            return null;
        }
    }

    /**
     * Get the function library for Saxon-defined extension functions available in this Saxon Edition
     *
     * @return the new function library
     */


    public BuiltInFunctionSet getVendorFunctionSet() {
        return vendorFunctionSet;
    }

    /**
     * Declare built-in extension functions
     */

    public void declareBuiltInExtensionFunctions() {
        // These functions are available whether or not XQuery 3.0/XPath 3.0 are in use.
        // This is justified on the basis that they're in their own namespace, so in 1.0/2.0 they are
        // acceptable vendor extension functions

        //registerExtensionFunction(new DynamicFunctionCallDefinition());
        //registerExtensionFunction(new DynamicErrorInfo());


    }

    /**
     * Declare the built-in XSLT extension element namespaces
     */

    public void declareBuiltInExtensionElementNamespaces() {
        try {
            setExtensionElementCreator(NamespaceUri.SAXON, SaxonElementFactory.FACTORY);
            setExtensionElementCreator(NamespaceUri.SAXON6, localName -> {
                if (localName.equals("output")) {
                    return new XSLResultDocument();
                }
                return null;
            });
            //setExtensionElementNamespace(NamespaceConstant.SQL, "net.sf.saxon.option.sql.SQLElementFactory");
        } catch (XPathException e) {
            throw new IllegalStateException(e.getMessage());
        }
    }

    //#if CSHARP==false
    /**
     * Set a namespace used to refer to a library of XSLT extension elements
     *
     * @param namespace the namespace used for the extension elements
     * @param factory the implementation of the ExtensionElementFactory
     *                  that instantiates extension elements in this namespace
     * @throws net.sf.saxon.trans.XPathException
     *          if the namespace is reserved
     * @since 9.2
     */

    public void setExtensionElementNamespace(String namespace, ExtensionElementFactory factory) throws XPathException {
        setExtensionElementCreator(NamespaceUri.of(namespace),
                                     (localName) -> {
                                         try {
                                             return factory.getExtensionClass(localName).newInstance();
                                         } catch (InstantiationException | IllegalAccessException e) {
                                             throw new RuntimeException(e);
                                         }
                                     });

    }
    
    /**
     * Set a namespace used to refer to a library of XSLT extension elements
     *
     * @param namespace the namespace used for the extension elements
     * @param className   the name of the class implementing the ExtensionElementFactory
     *                  that instantiates extension elements in this namespace
     * @throws net.sf.saxon.trans.XPathException if (for example) the supplied class does not implement
     *                                           the ExtensionElementFactory interface
     * @since 9.2
     */

    public void setExtensionElementNamespace(String namespace, String className) throws XPathException {
        Object o = getDynamicLoader().getInstance(className, null);
        if (!(o instanceof ExtensionElementFactory)) {
            throw new XPathException("Class " + className + " does not implement ExtensionElementFactory");
        }
        setExtensionElementNamespace(namespace, (ExtensionElementFactory)o);
    }
    //#endif

    /**
     * Set a namespace used to refer to a library of XSLT extension elements
     *
     * @param namespace the namespace used for the extension elements
     * @param creator the an instance of {}@link ExtensionElementCreator}
     *                  that instantiates extension elements in this namespace
     * @throws net.sf.saxon.trans.XPathException if (for example) the namespace is reserved
     * @since 12.0
     */

    public void setExtensionElementCreator(NamespaceUri namespace, ExtensionElementCreator creator) throws XPathException {
        if (NamespaceUri.isReserved(namespace)) {
            throw new XPathException("Namespace " + namespace +
                                             " is reserved: it cannot be used for extension instructions");
        }
        if (xsltExtensionElementNamespaceMap == null) {
            xsltExtensionElementNamespaceMap = new HashMap<>(4);
        }
        xsltExtensionElementNamespaceMap.put(namespace, creator);
    }


    /**
     * Get the XSLT extension element factory for a given namespace
     *
     * @param namespace the namespace URI of the extension element
     * @return the corresponding extension element factory if one has been registered,
     *         or null otherwise
     * @since 11. Replaces {@code getExtensionElementNamespace}.
     */

    public ExtensionElementCreator getExtensionElementCreator(NamespaceUri namespace) {
        if (xsltExtensionElementNamespaceMap == null) {
            return null;
        }
        return xsltExtensionElementNamespaceMap.get(namespace);
    }

    /**
     * Ask whether an extension element with a particular name is available
     * @param qName the extension element name
     * @return true if the extension element is available
     */

    @Override
    public boolean isExtensionElementAvailable(StructuredQName qName) {
        ExtensionElementCreator eef = getExtensionElementCreator(qName.getURI());
        return eef != null && eef.createExtensionElement(qName.getLocalPart()) != null;
    }

    /**
     * Get the JavaExternalObjectType object representing a particular Java class
     *
     * @param theClass the class in question
     * @return the corresponding JavaExternalObjectType
     */

    @Override
    public synchronized JavaExternalObjectType getJavaExternalObjectType(Class<?> theClass) {
        JavaExternalObjectType jeot = externalObjectTypeCache.get(theClass);
        if (jeot == null) {
            jeot = new JavaExternalObjectType(theClass);
            externalObjectTypeCache.put(theClass, jeot);
        }
        return jeot;
    }

    //#if CSHARP==false
    /**
     * Make a map representing the methods defined in a class. This map is specific to the class, not to
     * a particular instance. The functions present in this map take an extra first argument representing
     * the target instance; the functions returned in the final instance-level map will be partial applications
     * of the functions in the class-level map.
     *
     * @param javaClass the Java class whose methods are required
     * @param required  if non-null, indicates the key of the entry that is required in the map. If
     *                  this parameter is supplied, then the map will be limited to a single entry
     *                  with this key, since it is known that the other entries would never be used
     * @return a map whose entries represent public instance-level methods in the supplied Java class,
     * to the extent that these methods have unique names.
     */

    @Override
    public Map<String, Function> makeMethodMap(Class javaClass, String required) {
        return ObjectMap.makeMethodMap(this, javaClass, required);
    }
    //#endif

    //#if CSHARP==false
    /**
     * Convert a Java object to a map
     *
     * @param value    the (wrapped) Java object to be converted
     * @param required if non-null, indicates the key of the entry that is required in the map. If
     *                 this parameter is supplied, then the map will be limited to a single entry
     *                 with this key, since it is known that the other entries would never be used.
     * @return an XDM map containing entries representing the public instance-level methods
     * available in the object, to the extent that they have unique names.
     */

    @Override
    public MapItem externalObjectAsMap(ObjectValue value, String required) {
        return ObjectMap.toMap(this, value, required);
    }
    //#endif

    //#if CSHARP==false
    /**
     * Make an object lookup expression: supports the construct X?Y where X is an external Java object.
     * Requires Saxon-PE or higher
     *
     * @param lhs the left-hand operand
     * @param rhs the right-hand operand
     * @return the constructed expression
     * @throws XPathException if anything goes wrong
     */

    @Override
    public Expression makeObjectLookupExpression(Expression lhs, Expression rhs) throws XPathException {
        return new ObjectLookupExpression(lhs, rhs);
    }
    //#endif

    /**
     * Resolve a Source.
     *
     * @param source A source object, typically the source supplied as the first
     *               argument to {@link javax.xml.transform.Transformer#transform(javax.xml.transform.Source, javax.xml.transform.Result)}
     *               or similar methods.
     * @param config The Configuration. This provides the SourceResolver with access to
     *               configuration information; it also allows the SourceResolver to invoke the
     *               resolveSource() method on the Configuration object as a fallback implementation.
     * @return a source object that Saxon knows how to process. This must be an instance of one
     *         of the classes  StreamSource, SAXSource, DOMSource, {@link net.sf.saxon.lib.AugmentedSource},
     *         {@link net.sf.saxon.om.NodeInfo},
     *         or {@link net.sf.saxon.pull.PullSource}. Return null if the Source object is not
     *         recognized
     * @throws XPathException if the Source object is recognized but cannot be processed
     */

    @Override
    public ActiveSource resolveSource(Source source, Configuration config) throws XPathException {
        ActiveSource s = super.resolveSource(source, config);
        if (s != null) {
            return s;
        }
        if (source instanceof PullProvider) {
            checkLicensedFeature(LicenseFeature.PROFESSIONAL_EDITION, "pull processing", -1);
            return new PullSource((PullProvider)source);
        }
        return null;
    }

    /**
     * Make a new Mode - this can be overridden in subclasses to produce optimized variants
     * @param modeName the name of the mode
     * @param compilerInfo  information on the compiler, that can alter rule optimization
     * @return  an instantiated Mode
     */
    @Override
    public SimpleMode makeMode(StructuredQName modeName, CompilerInfo compilerInfo) {
        return new ModePE(modeName);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited