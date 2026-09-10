////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import com.saxonica.functions.extfn.cpp.CPPFunctionSet;
import com.saxonica.functions.extfn.cpp.PHPFunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import javax.xml.transform.Source;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.HashMap;
import java.util.Map;

/**
 * * This class is to use with SaxonC on C++ for XPath queries to be compiled and executed
 */
public class XPathProcessor extends SaxonCAPI {

    private XPathCompiler compiler = null;
    //private XPathSelector selector = null;
    //private XdmItem contextItem = null;

    /**
     * XPathProcessor default constructor
     */
    public XPathProcessor() {
        super();
        compiler = processor.newXPathCompiler();
        compiler.setAllowUndeclaredVariables(true);
    }

    /**
     * XPathProcessor constructor
     * @param l - license flag
     */
    public XPathProcessor(boolean l) {
        super(l);
        compiler = processor.newXPathCompiler();
        compiler.setAllowUndeclaredVariables(true);
        Configuration config = processor.getUnderlyingConfiguration();
//#if EE==true || PE==true
        if(config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif
    }

    /**
     * XPathProcessor constructor with the Processor object
     * @param proc   - the Processor object
     */
    public XPathProcessor(Processor proc) {
        super(proc);
        compiler = processor.newXPathCompiler();
        compiler.setAllowUndeclaredVariables(true);
        Configuration config = processor.getUnderlyingConfiguration();
//#if EE==true || PE==true
        if(config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif
    }

    /**
     * Create XPathProcessor and add to the ObjectHandle's pool. creates a HE XPath processor
     * @param thread  - Graalvm runtime data structure for a thread
     * @return long value referencing the XPathProcessor object in the ObjectHandles pool
     */
    @CEntryPoint(name = "createXPathProcessor")
    public static long createXPathProcessor(IsolateThread thread) {
            ObjectHandle handle = objectHandles.create(new net.sf.saxon.option.cpp.XPathProcessor(false));
            return handle.rawValue();
    }

    /**
     * Create XPathProcessor and add to the ObjectHandle's pool with reference to a Processor
     * @param thread  - Graalvm runtime data structure for a thread
     * @param processorRef  - long value referencing the Processor object in the ObjectHandles pool.
     * @return  long value referencing the XPathProcessor object in the ObjectHandles pool
     */
    @CEntryPoint(name = "createXPathProcessorWithProcessor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createXPathProcessor(IsolateThread thread, ObjectHandle processorRef) {
            Processor processor = objectHandles.get(processorRef);
            if(processor == null) {
                return 0;
            }
            ObjectHandle handle = objectHandles.create(XPathProcessor.newInstance(processor));
            return handle.rawValue();
    }

    /**
     * create new XPathProcessor object
     * @param proc - processor
     * @return XPathProcessor object
     */
    public static XPathProcessor newInstance(Processor proc) {
        return new XPathProcessor(proc);
    }

    /**
     * Get the XPath Compiler created for this XPath Processor
     * @return XPathCompiler
     */
    public XPathCompiler getCompiler(){
        return compiler;
    }

    /**
     * Declare a namespace binding as part of the static context for XPath expressions compiled using this
     * XPathCompiler
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_prefix The namespace prefix. If the value is a zero-length string, this method sets the default
     *               namespace for elements and types.
     * @param c_uri    The namespace URI. It is possible to specify a zero-length string to "undeclare" a namespace;
     *               in this case the prefix will not be available for use, except in the case where the prefix
     *               is also a zero length string, in which case the absence of a prefix implies that the name
     *               is in no namespace.
     * @throws NullPointerException if either the prefix or uri is null.
     */
    @CEntryPoint(name = "j_xp_declareNamespace", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long declareNamespace(IsolateThread thread, ObjectHandle xpathProcessorRef,  CCharPointer c_prefix, CCharPointer c_uri) throws SaxonApiException {
        String prefix = SaxonCAPI.toJavaString(c_prefix, WordFactory.nullPointer());
        String uri = SaxonCAPI.toJavaString(c_uri, WordFactory.nullPointer());
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        xPathProcessor.getCompiler().declareNamespace(prefix, uri);
        return SaxonCAPI.STATUS_OK;
    }

    /**
     * Declare a variable as part of the static context for XPath expressions compiled using this
     * {@code XPathCompiler}. It is an error for the XPath expression to refer to a variable unless it has been
     * declared, unless the method setAllowUndeclaredVariables has been called to permit
     * undeclared variables. This method declares the existence of the variable, but it does not
     * bind any value to the variable; that is done later, when the XPath expression is evaluated.
     * The variable is allowed to have any type (that is, the required type is <code>item()*</code>).
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_qname The name of the variable, expressions as a clark name string
     */
    @CEntryPoint(name = "j_xp_declareVariable", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long declareVariable(IsolateThread thread, ObjectHandle xpathProcessorRef,  CCharPointer c_qname) throws SaxonApiException {
        String qname = SaxonCAPI.toJavaString(c_qname, WordFactory.nullPointer());
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        if(compiler != null) {
            compiler.declareVariable(QName.fromClarkName(qname));
        }
        return SaxonCAPI.STATUS_OK;
    }


    /**
     * Import a schema namespace: that is, add the element and attribute declarations and type definitions
     * contained in a given namespace to the static context for the XPath expression.
     * <p>This method will not cause the schema to be loaded. That must be done separately, using the
     * {@link SchemaManager}. This method will not fail if the schema has not been loaded (but in that case
     * the set of declarations and definitions made available to the XPath expression is empty). The schema
     * document for the specified namespace may be loaded before or after this method is called.</p>
     * <p>This method does not bind a prefix to the namespace. That must be done separately, using the
     * declareNamespace method via the C++ API.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_uri The schema namespace to be imported. To import declarations in a no-namespace schema,
     *            supply a zero-length string.
     */
    @CEntryPoint(name = "j_xp_importSchemaNamespace", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long importSchemaNamespace(IsolateThread thread, ObjectHandle xpathProcessorRef,  CCharPointer c_uri) throws SaxonApiException{
        String uri = SaxonCAPI.toJavaString(c_uri, WordFactory.nullPointer());
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        if(compiler != null) {
            compiler.importSchemaNamespace(uri);
        }
        return SaxonCAPI.STATUS_OK;
    }

    /**
     * Say whether undeclared variables are allowed. By default, they are not allowed. When
     * undeclared variables are allowed, it is not necessary to predeclare the variables that
     * may be used in the XPath expression; instead, a variable is automatically declared when a reference
     * to the variable is encountered within the expression.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_allow true if undeclared variables are allowed, false if they are not allowed.
     */
    @CEntryPoint(name = "j_xp_setAllowUndeclaredVariables")
    public static void setAllowUndeclaredVariables(IsolateThread thread, ObjectHandle xpathProcessorRef, int c_allow) {
        boolean allow = CTypeConversion.toBoolean(c_allow);
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        if(compiler != null) {
            compiler.setAllowUndeclaredVariables(allow);
        }
    }

    /**
     * Set whether XPath 1.0 backwards compatibility mode is to be used. In backwards compatibility
     * mode, more implicit type conversions are allowed in XPath expressions, for example it
     * is possible to compare a number with a string. The default is false (backwards compatibility
     * mode is off).
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_option true if XPath 1.0 backwards compatibility is to be enabled, false if it is to
     *               be disabled.
     */
    @CEntryPoint(name = "j_xp_setBackwardsCompatible")
    public static void setBackwardsCompatible(IsolateThread thread, ObjectHandle xpathProcessorRef, int c_option) {
        boolean option = CTypeConversion.toBoolean(c_option);
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        compiler.setBackwardsCompatible(option);
    }

    /**
     * Set the static base URI for XPath expressions compiled using this XPathCompiler. The base URI
     * is part of the static context, and is used to resolve any relative URIs appearing within an XPath
     * expression, for example a relative URI passed as an argument to the doc() function. If no
     * static base URI is supplied, then the current working directory is used.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_uriStr - A string for the uri represented as a C pointer to a char array
     * @return long value for the status of setting the base URI
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xp_setBaseURI", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setBaseURI(IsolateThread thread, ObjectHandle xpathProcessorRef, CCharPointer c_uriStr) throws SaxonApiException {
        if(c_uriStr == WordFactory.nullPointer()) {
            throw new SaxonApiException("URI string is null");
        }
        String uriStr = SaxonCAPI.toJavaString(c_uriStr, WordFactory.nullPointer());
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        URI uri = null;
        try {
            uri = new URI(uriStr);
            compiler.setBaseURI(uri);
            return SaxonCAPI.STATUS_OK;
        } catch (URISyntaxException e) {
            SaxonApiException ex = new SaxonApiException(e);
            throw ex;
        }

    }

    @CEntryPoint(name = "j_xp_getBaseURI", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static CCharPointer getBaseURI(IsolateThread thread, ObjectHandle xpathProcessorRef, SaxonCAPI.AllocatorCCharFn alloc) {

        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        URI uri = null;
        String baseUri = compiler.getBaseURI().toString();

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, baseUri);
        return a;
    }


    /*public void setProperties(ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        if (selector != null) {
            try {
                Map<QName, XdmValue> parameters = new HashMap<>();
                Map<String, Object> map = setupConfigurationAndBuildMap(processorDataAccumulatorRef, null, parameters, null, null, null, false);
                applyXPathProperties(this, "", processor, selector, map, parameters);
            } catch (SaxonApiException e) {
                throw e;
            }
        } else {
            SaxonApiException ex = new SaxonApiException("XPathExecutable not created");
            throw ex;

        }

    }    */

    public void reset() {
        compiler = null;
       // selector = null;
    }


    /**
     * Compile and evaluate an XPath expression, supplied as a character string, with properties and parameters required
     * by the XPath expression
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param ccwd      - Current working directory
     * @param c_xpathStr - A string containing the source text of the XPath expression represented as a C pointer to a char array
     * @param processorDataAccumulatorRef   - Parameters and properties names required by the XPath expression along with their values in a ProcessorDataAccumulator.
     *                                      This could contain the context node , source as string or file name, etc
     *                                      The values for the parameters and properties required by the XPath expression
     * @return long value reference to the XdmItem in the ObjectHandle's pool
     **/
    @CEntryPoint(name = "j_evaluate", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long evaluate(IsolateThread thread, ObjectHandle xpathProcessorRef, CCharPointer ccwd, CCharPointer c_xpathStr, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XPathProcessor xPathProcessor = objectHandles.get(xpathProcessorRef);
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String xpathStr = SaxonCAPI.toJavaString(c_xpathStr, WordFactory.nullPointer());
        XPathCompiler compiler = xPathProcessor.getCompiler();
        if (SaxonCAPI.debug) {
            if (xpathStr != null) {
                System.err.println("xpathString: " + xpathStr);
            }
        }
        Map<QName, XdmValue> parameters = new HashMap<>();
        Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, null, parameters, null, null, null, false);
        setupXPathCompiler(compiler, optionsMap);

        XPathSelector selector = compiler.compile(xpathStr).load();
        applyXPathProperties(xPathProcessor, cwd, xPathProcessor.getProcessor(), selector, optionsMap, parameters);
        /*if (contextItem != null) {
            selector.setContextItem(contextItem);
        } */
        XdmValue value = selector.evaluate();
        ObjectHandle handle = SaxonCAPI.createHandle(value);
        return handle.rawValue();

    }

    private static void setupXPathCompiler(XPathCompiler compiler, Map<String, Object> parameterMap) {
        Object valuei = null;
        if(parameterMap.containsKey("caching:")){
            valuei = parameterMap.get("caching");
            if(valuei instanceof Boolean) {
                compiler.setCaching((Boolean)valuei);
            } if (valuei instanceof String && (valuei.equals("yes") || valuei.equals("true"))) {
                compiler.setCaching(true);
            }

        }

        /*if(parameterMap.containsKey("allowUVar:")){
            compiler.setAllowUndeclaredVariables(true);
        }  */

        if(parameterMap.containsKey("importSN:")){
            valuei = parameterMap.get("importSN:");
            if(valuei != null && valuei instanceof String) {
                compiler.importSchemaNamespace((String)valuei);

            }

        }

        if(parameterMap.containsKey("uemp:")){
            valuei = parameterMap.get("uemp:");
            if(valuei != null && valuei instanceof String) {
                int valueInt = Integer.parseInt((String)valuei);
                if (SaxonCAPI.debug) {
                    System.err.println("DEBUG setUnprefixedElementMatchingPolicy value=" + valueInt);
                    System.err.println("DEBUG setUnprefixedElementMatchingPolicy size=" + UnprefixedElementMatchingPolicy.values().length);
                }
                switch (valueInt) {

                    case 0:
                        compiler.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE);
                        break;
                    case 1:
                        compiler.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.ANY_NAMESPACE);
                        break;
                    case 2:
                        compiler.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE_OR_NONE);
                        break;
                }
            }

        }

        if(parameterMap.containsKey("backwardsCom:")) {
            compiler.setBackwardsCompatible(true);
        }

        if (parameterMap.containsKey("sa")) {
            String value = (String) parameterMap.get("sa");
            if (value.equals("true") || value.equals("on")) {
                compiler.setSchemaAware(true);
            } else {
                compiler.setSchemaAware(false);
            }
        }

        if (parameterMap.containsKey("lang:")) {
            valuei = (String) parameterMap.get("lang");
            if(valuei != null && valuei instanceof String) {
                compiler.setLanguageVersion((String)valuei);

            }
        }

        /*if(!variables.isEmpty()) {
            for(QName name : variables) {
                compiler.declareVariable(name);
            }
        } */
    }


    /**
     * Compile and evaluate an XPath expression whose result is expected to be
     * a single item, with a given context item. The expression is supplied as
     * a character string.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param ccwd      - Current working directory
     * @param c_xpathStr - A string containing the source text of the XPath expression represented as a C pointer to a char array
     * @param processorDataAccumulatorRef   - Parameters and properties names required by the XPath expression along with their values in a ProcessorDataAccumulator.
     *                                      This could contain the context node , source as string or file name, etc
     *                                      The values for the parameters and properties required by the XPath expression
     * @return long value reference to the XdmItem in the ObjectHandle's pool
     **/
    @CEntryPoint(name = "j_evaluateSingle", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long evaluateSingle(IsolateThread thread, ObjectHandle xpathProcessorRef, CCharPointer ccwd, CCharPointer c_xpathStr, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String xpathStr = SaxonCAPI.toJavaString(c_xpathStr, WordFactory.nullPointer());
        XPathProcessor xPathProcessor = SaxonCAPI.objectHandles.get(xpathProcessorRef);
        XPathCompiler compiler = xPathProcessor.getCompiler();
        if (debug) {
            if (xpathStr != null) {
                System.err.println("xpathString: " + xpathStr);
            }
        }
        Map<QName, XdmValue> parameters = new HashMap<>();
        Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, null, parameters, null, null, null, false);
        setupXPathCompiler(compiler, optionsMap);
        XPathSelector selector = compiler.compile(xpathStr).load();
        applyXPathProperties(xPathProcessor, cwd, xPathProcessor.getProcessor(), selector, optionsMap, parameters);


        XdmItem item = selector.evaluateSingle();
        ObjectHandle handle = SaxonCAPI.createHandle(item);
        return handle.rawValue();

    }

    /**
     * Evaluate the XPath expression, returning the effective boolean value of the result.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param xpathProcessorRef - reference to XPathProcessor object held in the ObjectHandle's pool
     * @param c_cwd      - Current working directory
     * @param c_xpathStr - A string containing the source text of the XPath expression represented as a C pointer to a char array
     * @param processorDataAccumulatorRef   - Parameters and properties names required by the XPath expression along with their values in a ProcessorDataAccumulator.
     *                                      This could contain the context node , source as string or file name, etc
     *                                      The values for the parameters and properties required by the XPath expression
     * @return a <code>boolean</code> as int value representing the effective boolean value of the result of evaluating
     *         the expression, as defined by the rules for the fn:boolean() function.
     **/
    @CEntryPoint(name = "j_effectiveBooleanValue", exceptionHandler = SaxonCExceptionIntHandler.class)
    public static int effectiveBooleanValue(IsolateThread thread, ObjectHandle xpathProcessorRef, CCharPointer c_cwd, CCharPointer c_xpathStr, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        Map<QName, XdmValue> parameters = new HashMap<>();
        Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, null, parameters, null, null, null, false);
        XPathProcessor xPathProcessor = objectHandles.get(xpathProcessorRef);

        XPathCompiler compiler = xPathProcessor.getCompiler();
        String cwd = null;

        if(c_cwd != WordFactory.nullPointer()) {
            SaxonCAPI.toJavaString(c_cwd, WordFactory.nullPointer());
        }
        String xpathStr = SaxonCAPI.toJavaString(c_xpathStr, WordFactory.nullPointer());
        setupXPathCompiler(compiler, optionsMap);
        XPathSelector selector = compiler.compile(xpathStr).load();

        applyXPathProperties(xPathProcessor, cwd, xPathProcessor.getProcessor(), selector, optionsMap, parameters);

        /*if (contextItem != null) {
            selector.setContextItem(contextItem);
        }     */
        boolean result = selector.effectiveBooleanValue();
        return (int)(result ? 1 : 0);


    }


    /**
     * Applies the properties and parameters required in the transformation.
     * In addition we can supply the source, stylesheet and output file names.
     * We can also supply values to xsl:param and xsl:variables required in the stylesheet.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     * @param api        - The SaxonCAPI object
     *  @param cwd       - current working directory
     * @param processor - required to use the same processor as for the compiled stylesheet
     * @param selector  - compiled and loaded XPath expression ready for execution.
     * @param map       - The map of properties to apply on the XPath processor
     * @param parameters  - map of parameters to apply on the XPath processor
     * @throws SaxonApiException if error in apply XPath property
     */
    public static void applyXPathProperties(SaxonCAPI api, String cwd, Processor processor, XPathSelector selector, Map<String, Object> map, Map<QName, XdmValue> parameters) throws SaxonApiException {
        if (!map.isEmpty()) {
            String outputFilename = null;
            String initialTemplate = null;
            String initialMode = null;
            XdmItem item = null;
            String outfile = null;
            Source source = null;
            DocumentBuilder builder = processor.newDocumentBuilder();
            Object valuei = null;

            if (cwd != null && cwd.length() > 0) {
                if (!cwd.endsWith("/")) {
                    cwd = cwd.concat("/");
                }
            }

            if (map.containsKey("s")) {
                valuei = map.get("s");
                if (!(valuei instanceof String)) {
                    throw new SaxonApiException("Source file has incorrect type");
                }
                source = api.resolveFileToSource(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE);
                selector.setContextItem(builder.build(source));
            }

            if (map.containsKey("item")) {
                valuei = map.get("item");
                if (valuei instanceof XdmItem) {
                    item = (XdmItem) valuei;
                    selector.setContextItem(item);
                }

            }

            if (map.containsKey("node")) {
                valuei = map.get("node");
                if (valuei instanceof XdmItem) {
                    if (debug && valuei != null) {
                        System.err.println("DEBUG: Type of value=" + (valuei).getClass().getName());

                    }
                }
                item = (XdmItem) valuei;
                selector.setContextItem(item);

            }


            

            if (map.containsKey("extc")) {
                //extension function library path
                String libName = (String) map.get("extc");
                SaxonCAPI.setLibrary("", libName);


            }
        }
         if (!parameters.isEmpty()){
             Map<QName, XdmValue> variables = parameters;
             for(Map.Entry<QName, XdmValue> entry: variables.entrySet()){
                 if (SaxonCAPI.debug) {
                         System.err.println("applyXPathProperties: key=" + entry.getKey().getClarkName() + ", Value="+entry.getValue().toString());
                 }
                 selector.setVariable(entry.getKey(), entry.getValue());

             }
         } else {
             if (SaxonCAPI.debug) {
                 System.err.println("applyXPathProperties: parameters map is empty");
             }

         }


    }


    public static void main(String[] arg) throws SaxonApiException {



        XPathProcessor xpath = new XPathProcessor(false);

        XdmItem item = xpath.getCompiler().evaluateSingle("matches('abcdef', '^\\p{IsBasicLatin}+$')", null);
        System.out.println(item.getStringValue());
        /*System.out.println("Version: " + SaxonCAPI.getProductVersion(xpath.processor));
        String sourcefile1 = "kamervragen.xml";
        String[] params1 = {"s"};
        Object[] values1 = {sourcefile1};
        Processor p = xpath.getProcessor();

        XdmNode node = xpath.parseXmlString("<out>\n" +
                "<person attr1='value1' attr2='value2' xmlns='http://example.com'>text1</person>\n" +
                "    <person>text2</person>\n" +
                "    <person1>text3</person1>\n" +
                "</out>");
        xpath.declareNamespace("p", "http://one.com/");
        XdmNode node2 = xpath.parseXmlFile("/Users/ond1/work/development/svn/test", null, "books.xml");
        XdmNode[] children1 = XdmUtils.getChildren(XdmUtils.getChildren(node)[0]);
        // xpath.setContextItem(node);
        Object[] values3 = {node2};
        String[] params2 = {"node",  };
        Object[] values2 = {node};

        String[] params4 = {"param:s1"};
        Object[] values4 = {"text in var"};

        String[] params5 = {"param:s1"};
        Object[] values5 = {"10"};
        XdmValue value = xpath.evaluateSingle("/Users/ond1/work/development/tests/jeroen/xml/", "//person[1]", params2, values2);
        if (value instanceof XdmNode) {
            String nodename = XdmUtils.getEQName(((XdmNode) value).getNodeName());
            System.out.println(nodename);
            //String [] values = XdmUtils.getAttributeValues((XdmNode)value);
            String valuex1 = XdmUtils.getAttributeValue((XdmNode) value, "attr1");
            XdmNode[] children = XdmUtils.getChildren((XdmNode) value);
            XdmNode parent = children[0].getParent();
            // System.out.println(values[0]);
            System.out.println(valuex1);
            System.out.println("Parent =" + parent.getParent().getNodeName());
        }
        boolean ebv = xpath.effectiveBooleanValue("/Users/ond1/work/development/tests/jeroen/xml/", "count(/out/person)>0", params2, values2);
        // System.out.println(value.toString());
        System.out.println(ebv);
        XdmValue valuex = xpath.evaluateSingle("/Users/ond1/work/development/tests/jeroen/xml/", "//person[1]", params2, values2);
        if (valuex != null) {
            System.out.println("evalSingle = " + XdmUtils.getStringValue(valuex));
        }
        XdmValue[] value2 = xpath.evaluate("/Users/ond1/work/development/svn/test", "/BOOKLIST/BOOKS/ITEM/TITLE", params2, values3);
        if (value2 != null) {
            for (int i = 0; i < value2.length; i++) {
                System.out.println("Book Title: " + XdmUtils.getStringValue(value2[i]));
            }
        } else {
            System.out.println("Book xpath expr returned null!!!");

        }

        XdmValue[] value3 = xpath.evaluate("/Users/ond1/work/development/svn/test", "$s1", params4, values4);
        XdmValue[] value4 = xpath.evaluate("/Users/ond1/work/development/svn/test", "$s1", params5, values5);
        if(value3.length>0 && value4.length>0) {
            System.out.println("Value of parameter = "+ XdmUtils.getStringValue(value3[0]));
            System.out.println("Value of parameter = "+ XdmUtils.getStringValue(value4[0]));
        } */
    }


}
