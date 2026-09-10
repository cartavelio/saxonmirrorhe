////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;


import com.saxonica.functions.extfn.cpp.NativeCall;
import net.sf.saxon.Configuration;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Validation;

import net.sf.saxon.lib.*;

import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.ObjectHandles;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.function.CFunctionPointer;
import org.graalvm.nativeimage.c.function.InvokeCFunctionPointer;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.util.*;

import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;



/**
 * This class holds common attributes and methods required in the XsltProcessor, XQueryEngine and XPathProcessor
 */

public class SaxonCAPI {

    //Properties props = new Properties();
    protected Processor processor = null;  //TODO remove this variable - class should be stateless

    public static boolean debug = false;
    protected boolean schemaAware = false;
    public static String RESOURCES_DIR = null;

    public static ObjectHandles objectHandles = ObjectHandles.create();



    /**
     * Default Constructor. Creates a processor that does not require a license edition
     */
    public SaxonCAPI() {
        processor = new Processor(false);

        if (debug) {
            System.err.println("New processor created in SaxonCAPI(), Processor: " + System.identityHashCode(processor));
        }

    }

    /**
     * Constructor with license edition flag
     *
     * @param license - specify license edition flag
     */
    public SaxonCAPI(boolean license) {
        processor = new Processor(license);

        if (debug) {
            System.err.println("New processor created in SaxonCAPI(l), Processor: " + System.identityHashCode(processor));
        }
    }

    public SaxonCAPI(Processor proc) {
        processor = proc;

        if (debug) {
            System.err.println("New processor created in SaxonCAPI(l), Processor: " + System.identityHashCode(processor));
        }
    }



    public static void setLibrary(String cwd, String libName) {
//#if EE==true || PE==true
        NativeCall.loadLibrary(cwd, libName);
//#endif
    }

    @CEntryPoint(name = "createSaxonProcessor")
    public static long createSaxonProcessor(IsolateThread thread, int license) throws SaxonApiException {
           /*Properties systemProperties = System.getProperties();
           systemProperties.put ("xml.catalog.defaultLoggerLogLevel", "debug");
           System.setProperties (systemProperties); */
        ObjectHandle handle = objectHandles.create(new Processor(CTypeConversion.toBoolean(license)));
        return handle.rawValue();
    }


    @CEntryPoint(name = "createDocumentBuilder")
    public static long createDocumentBuilder(IsolateThread thread, ObjectHandle processorRef) throws SaxonApiException {
        Processor processor = objectHandles.get(processorRef);

        if(processor != null) {
            ObjectHandle handle = objectHandles.create(processor.newDocumentBuilder());
            return handle.rawValue();
        }
        return -1;
    }

    public static class DataWrapper{
        long [] valueRefs;

        public DataWrapper(long [] vals){
            this.valueRefs = vals;
        }

        public long [] getDataRefs(){
            return valueRefs;
        }


    };


    interface AllocatorFn extends CFunctionPointer {
       @InvokeCFunctionPointer
       CLongPointer call(long size);
     }

     @CEntryPoint
     static CLongPointer getLongArray(IsolateThread thread, AllocatorFn alloc, ObjectHandle refsObj) {
        DataWrapper obj = objectHandles.get(refsObj);
       return longArrayToNative(alloc, obj.getDataRefs());
     }

    static CLongPointer longArrayToNative(AllocatorFn alloc, long [] refs) {

      CLongPointer a =  alloc.call(refs.length + 1);
      for (int i = 0; i < refs.length; i++) {
        a.write(i, refs[i]);
      }
      a.write(refs.length, (long) 0);
      return a;
    }

    


    /**
     * Static method to create Processor object given a configuration file
     *
     * @param cconfigFile - name of the configuration file
     */
    @CEntryPoint(name = "createSaxonProcessorWithConfigurationFile")
    public static long createSaxonProcessorWithConfigurationFile(IsolateThread thread, CCharPointer cconfigFile) throws SaxonApiException {
        Configuration config = null;
        String configFile = CTypeConversion.toJavaString(cconfigFile);
        try {
            config = Configuration.readConfiguration(new StreamSource(configFile));

        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }


        if (config == null) {
            config = Configuration.newConfiguration();
        }

        //config.setHostLanguage(Configuration.XQUERY);
        ObjectHandle handle = objectHandles.create(new Processor(config));
        return handle.rawValue();

    }
    

    /**
     * Error Listener to capture errors
     */
   /* protected ErrorListener errorListener = new StandardErrorListener() {

        @Override
        public void warning(TransformerException exception) {
            saxonWarnings.add(new SaxonCException(exception));

            try {
                super.error(exception);
            } catch (Exception ex) {
            }

        }

        @Override
        public void error(TransformerException exception) {
            if (Configuration.RECOVER_WITH_WARNINGS == Configuration.RECOVER_SILENTLY && !(exception instanceof ValidationException)) {
                // do nothing
                return;
            }
            saxonExceptions.add(new SaxonApiException(exception));
            try {
                super.error(exception);
            } catch (Exception ex) {
            }
        }

        @Override
        public void fatalError(TransformerException exception) {
            if (exception instanceof XPathException && ((XPathException) exception).hasBeenReported()) {
                // don't report the same error twice
                return;
            }
            saxonExceptions.add(new SaxonApiException(exception));
            try {
                super.fatalError(exception);
            } catch (Exception ex) {
            }
        }
    };   */
    @CEntryPoint(name = "getProductVersion")
    public static CCharPointer getProductVersion(IsolateThread thread, ObjectHandle processorRef) {
        Processor processor = objectHandles.get(processorRef);
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(processor.getUnderlyingConfiguration().getProductTitle());
        return holder.get();
    }

    @CEntryPoint(name = "setCatalogFile")
    public static void setCatalog(IsolateThread thread, ObjectHandle processorRef, CCharPointer catalogFile) {
        Processor processor = objectHandles.get(processorRef);

        processor.setCatalogFiles(CTypeConversion.toJavaString(catalogFile));
    }



    /**
     * Get the Processor object created
     *
     * @return Processor
     */
    public Processor getProcessor() {
        return this.processor;
    }


    @CEntryPoint(name = "j_applyToConfiguration")
    public static synchronized void applyToConfiguration(IsolateThread thread, ObjectHandle processorRef, ObjectHandle processorDataRef) throws SaxonApiException {

        Processor processor = objectHandles.get(processorRef);
        if (debug) {
            System.err.println("New processor created, Processor: " + System.identityHashCode(processor));
        }

        ProcessorDataAccumulator processorDataAccumulator = objectHandles.get(processorDataRef);
        Configuration config = processor.getUnderlyingConfiguration();
        String [] names = processorDataAccumulator.getParameterNames();
        Object [] values = processorDataAccumulator.getValues();
        int len = processorDataAccumulator.size();
        for (int i = 0; i < len; i++) {
            String name = names[i];
            String value = (String)values[i];
            if(debug) {
                System.err.println("SaxonC DEBUG - applyConfiguration, name=" + name);
                System.err.println("SaxonC DEBUG - applyConfiguration, value=" + value);
            }
            if(value == null || name == null) {
                if(debug) {
                    System.err.println("SaxonC DEBUG - applyConfiguration value or name is null");
                }
                continue;
            }
            if (name.equals("l")) {
                if (value != null) {
                    processor.setConfigurationProperty(Feature.LINE_NUMBERING,
                            "on".equals(value));
                }
            } else if (name.equals("dtd")) {
                if ("on".equals(value)) {
                    config.getParseOptions().setDTDValidationMode(Validation.STRICT);
                } else if ("off".equals(value)) {
                    config.getParseOptions().setDTDValidationMode(Validation.SKIP);
                } else if ("recover".equals(value)) {
                    config.getParseOptions().setDTDValidationMode(Validation.LAX);
                }

            } else if (name.equals("expand")) {
                config.getParseOptions().setExpandAttributeDefaults("on".equals(value));
            } else if (name.equals("opt")) {
                if (value != null) {
                    config.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, value);
                }

            } else if (name.equals("outval")) {

                Boolean isRecover = "recover".equals(value);
                config.setConfigurationProperty(Feature.VALIDATION_WARNINGS, isRecover);
                config.setConfigurationProperty(Feature.VALIDATION_COMMENTS, isRecover);


            } else if (name.equals("strip")) {

                config.setConfigurationProperty(Feature.STRIP_WHITESPACE, value);
            } else if (name.equals("val")) {
                if ("strict".equals(value)) {
                    processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.STRICT);
                } else if ("lax".equals(value)) {
                    processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.LAX);
                }

            } else if (name.equals("xsdversion")) {
                processor.setConfigurationProperty(Feature.XSD_VERSION, value);

            } else if (name.equals("xmlversion")) {

                processor.setConfigurationProperty(Feature.XML_VERSION, value);

            } else if (name.equals("xi")) {
                processor.setConfigurationProperty(Feature.XINCLUDE, "on".equals(value));

            } else if (name.equals("xsiloc")) {
                processor.setConfigurationProperty(Feature.USE_XSI_SCHEMA_LOCATION, "on".equals(value));
            } else if (name.startsWith("http://saxon.sf.net/feature/")) {
                try {
                    processor.setConfigurationProperty(name, value);
                } catch (IllegalArgumentException err) {
                    throw new SaxonApiException(err.getMessage());
                }
            }
        }


    }


    /**
     * set debug mode on or off
     *
     * @param d - flag for debug mode
     */
    @CEntryPoint(name = "setDebugMode")
    public static void setDebugMode(IsolateThread thread, boolean d) {
        debug = d;
    }
    


    /**
     * Method to convert arrays received from C++ to Java map object
     * this method also extracts serialization properties and parameters
     *
     *
     * @param processorDataAccumulatorRef
     * @param props   - Serialisaztion properties to be setup
     * @param parameters - Passed Processor parameters as map. Updated within method
     * @param staticParameters   - For XSLT use
     * @param globals    - Map to retrieve global variables to be used in XSLT transformer
     * @param initialTemplateParameters - Initial template parameter for use with XSLT
     * @param forXslt   - Indicate XSLT use
     * @return Map of String, Object pairs
     * @throws SaxonApiException
     */
    public static Map<String, Object> setupConfigurationAndBuildMap(ObjectHandle processorDataAccumulatorRef, Properties props, Map<QName, XdmValue> parameters, Map<QName, XdmValue> staticParameters, Map<QName, XdmValue> globals, Map<QName, XdmValue> initialTemplateParameters, boolean forXslt) throws SaxonApiException {
        ProcessorDataAccumulator processorDataAccumulator= objectHandles.get(processorDataAccumulatorRef);
        String[] params = null;
        Object[] values = null;
        if(processorDataAccumulator != null) {
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }

        Map<String, Object> map = new HashMap();
        if (params == null || values == null) {
            return map;
        }
        if (params.length != values.length) {
            throw new SaxonApiException("SaxonC internal Error: params array length does not match values length");
        }
        for (int i = 0; i < params.length; i++) {
            if (params[i].startsWith("!")) {
                String name = params[i].substring(1);
                Serializer.Property prop = Serializer.Property.get(name);
                if (prop == null) {
                    throw new SaxonApiException("Property name " + name + " not found");
                }
                if(props != null) {
                    props.put(prop.getQName().getClarkName(), values[i]);
                } else {
                    if(debug) {
                        System.err.println("Error Java property option found but Java Property object is null: Probably XPathProcessor in use");
                    }
                }
            }  else if (params[i].equals("resources")) {
                char separatorChar = '/';
                if (SaxonCAPI.RESOURCES_DIR == null && values[i] instanceof String) {
                    String dir1 = (String) values[i];
                    if (!dir1.endsWith("/")) {
                        dir1 = dir1.concat("/");
                    }
                    if (File.separatorChar != '/') {
                        dir1.replace(separatorChar, File.separatorChar);
                        separatorChar = '\\';
                        dir1.replace('/', '\\');
                    }
                    SaxonCAPI.RESOURCES_DIR = dir1;
                }

            }else if (initialTemplateParameters != null && params[i].startsWith("itparam:")) {
                //initial template parameters
                String paramName = params[i].substring(8);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                initialTemplateParameters.put(qname, valueForCpp);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI itparam: " + paramName);
                }

            } else if (staticParameters != null && params[i].startsWith("sparam:")) {
                //static parameters
                String paramName = params[i].substring(7);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                staticParameters.put(qname, valueForCpp);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI sparam: " + paramName);
                }

            } else if (params[i].startsWith("param:")) {
                String paramName = params[i].substring(6);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI param: " + paramName);
                }

                if (qname != null && valueForCpp != null) {
                    if(forXslt) {
                        globals.put(qname, valueForCpp);
                    } else {
                        if(parameters != null) {
                            parameters.put(qname, valueForCpp);
                        }
                    }
                }
            } /*else if (!forXslt && params[i].startsWith("dvar:")) {
                QName varQName = QName.fromClarkName((String)values[i]);
                variables.add(varQName);
            } */
            else {
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI: param-name: " +params[i] + ", type of Value= "+values[i].getClass().getName());
                }
                map.put(params[i], values[i]);
            }
        }
        return map;

    }


    /**
     * Utility method when XdmValues retrieved as objects from C++ to convert a object to an XmdValue.
     * @param value - object to be converted
     * @return XdmValue
     */
    public static XdmValue convertObjectToXdmValue(Object value) {

        XdmValue valueForCpp = null;
        if (value instanceof XdmValue) {
            valueForCpp = (XdmValue) value;
            if (debug) {

                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue().toString());
                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue());
            }

        } else if (value instanceof Object[]) {

            Object[] arr = (Object[]) value;
            if (debug) {
                System.err.println("DEBUG: Array of parameters found. arr len=" + arr.length);

            }
            if(arr.length== 0) {
                return null;
            }
            List<Item> valueList = new ArrayList<Item>();
            for (int j = 0; j < arr.length; j++) {
                Object itemi = arr[j];
                if (itemi == null) {
                    if (debug) {
                        System.err.println("Error: Null item at " + j + "th position in array of XdmValues");
                    }
                    break;
                }
                if (debug) {
                    System.err.println("Java object:" + itemi);
                }
                if (itemi instanceof XdmValue) {
                    valueList.add((Item) (((XdmValue) itemi).getUnderlyingValue()));
                } else {
                    XdmValue valuex = getXdmValue(itemi);
                    if (valuex == null) {
                        if (debug) {
                            System.err.println("Error: Null item at " + j + "th position in array of XdmValues when converting");
                        }
                        break;
                    }
                    valueList.add((Item) (getXdmValue(itemi)).getUnderlyingValue());
                }
            }
            GroundedValue gv = SequenceExtent.makeSequenceExtent(valueList);
            valueForCpp = XdmValue.wrap(gv);
        } else {
            //fast track for primitive values
            valueForCpp = getXdmValue(value);
            if (debug) {
                System.err.println("DEBUG: primitive value found");
            }
        }
        return valueForCpp;
    }


    /**
     * parse XML document supplied by file
     *
     * @param cwd      - Current Working directory
     * @param filename - File name of the XML document to parse
     * @return XdmNode
     */
    @CEntryPoint(name = "parseXmlFile")
    public static long parseXmlFile(IsolateThread thread, ObjectHandle processorRef, CCharPointer cwd, CCharPointer filename) throws SaxonApiException {
        Processor proc = objectHandles.get(processorRef);
            XdmNode doc = parseXmlFile(CTypeConversion.toJavaString(cwd), null, proc, null, CTypeConversion.toJavaString(filename));
            ObjectHandle handle = objectHandles.create(doc);
            return handle.rawValue();

    }

    @CEntryPoint(name = "parseXmlFileWithValidator")
    public static long parseXmlFileWithValidator(IsolateThread thread, CCharPointer cwd, ObjectHandle processorRef, ObjectHandle validatorRef, CCharPointer filename) throws SaxonApiException {
        Processor proc = objectHandles.get(processorRef);
        SchemaValidator validator= objectHandles.get(validatorRef);

        XdmNode node = parseXmlFile(CTypeConversion.toJavaString(cwd), null, proc, validator, CTypeConversion.toJavaString(filename));
        ObjectHandle handle = objectHandles.create(node);
        return handle.rawValue();

    }




    /**
     * parse XML document supplied string
     *
     * @param xml - string representation of XML document
     * @return XdmNode
     */
    @CEntryPoint(name = "parseXmlString")
    public static long parseXmlString(IsolateThread thread, ObjectHandle processorRef, CCharPointer xml) throws SaxonApiException {

        Processor proc = objectHandles.get(processorRef);

        XdmNode node = parseXmlString(null,proc, null, null, CTypeConversion.toJavaString(xml));
        ObjectHandle handle = objectHandles.create(node);
        return handle.rawValue();
    }


    @CEntryPoint(name = "parseXmlStringWithValidator")
    public static long parseXmlString(IsolateThread thread, ObjectHandle processorRef, ObjectHandle validatorRef, CCharPointer xml) throws SaxonApiException {
        Processor proc = objectHandles.get(processorRef);
        SchemaValidator validator = objectHandles.get(validatorRef);

        XdmNode node = parseXmlString(null,proc, null, validator, CTypeConversion.toJavaString(xml));
        ObjectHandle handle = objectHandles.create(node);
        return handle.rawValue();

    }

    @CEntryPoint(name = "createXdmAtomicItem")
    public static long createXdmAtomicItem(IsolateThread thread, CCharPointer ctypeStr, CCharPointer cvalueStr) throws SaxonApiException {
        String valueStr = CTypeConversion.toJavaString(cvalueStr);
        String typeStr = CTypeConversion.toJavaString(ctypeStr);

        ObjectHandle handle = null;

        handle = objectHandles.create(createXdmAtomicItem(valueStr, typeStr));
        return handle.rawValue();

    }

    /**
     * Create an Xdm atomic value from string representation
     *
     * @param typeStr  - Local name of a type in the XML Schema namespace.
     * @param valueStr - The value given in a string form.
     *                 In the case of a QName the value supplied must be in clark notation. {uri}local
     * @return XdmValue - value
     */

    public static XdmValue createXdmAtomicItem(String typeStr, String valueStr) throws SaxonApiException {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);


        BuiltInAtomicType type = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        if (type == null) {
            throw new SaxonApiException("Unknown built in type: " + typeStr + " not found");
        }

        if (type.isNamespaceSensitive()) {
            StructuredQName value = StructuredQName.fromClarkName(valueStr);
            return XdmValue.wrap(new QNameValue(value, type));
        }

        ConversionRules rules = new ConversionRules();
        Converter converter = rules.getConverter(BuiltInAtomicType.STRING, type);


        try {
            return XdmValue.wrap(converter.convert(new StringValue(valueStr)).asAtomic());
        } catch (ValidationException e) {
            throw new SaxonApiException(e);
        }


    }

    /**
     * Wrap a boxed primitive type as an XdmValue.
     *
     * @param value - boxed primitive type
     * @return XdmValue
     */
    public static XdmValue getXdmValue(Object value) {
        XdmValue valueForCpp = null;
        if (value instanceof Integer) {
            valueForCpp = XdmValue.wrap(new Int64Value(((Integer) value).intValue()));
        } else if (value instanceof Boolean) {
            valueForCpp = XdmValue.wrap(BooleanValue.get(((Boolean) value).booleanValue()));
        } else if (value instanceof Double) {
            valueForCpp = XdmValue.wrap(DoubleValue.makeDoubleValue(((Double) value).doubleValue()));
        } else if (value instanceof Float) {
            valueForCpp = XdmValue.wrap(FloatValue.makeFloatValue(((Float) value).floatValue()));
        } else if (value instanceof Long) {
            valueForCpp = XdmValue.wrap(Int64Value.makeIntegerValue((((Long) value).longValue())));
        } else if (value instanceof String) {
            valueForCpp = XdmValue.wrap(StringValue.makeStringValue(((String) value)));
        }
        return valueForCpp;
    }

    public static XdmNode parseXmlString(String cwd, Processor proci, DocumentBuilder builder, SchemaValidator validator, String xml) throws SaxonApiException {
        try {
            if(proci == null) {
                return null;
            }
            StringReader reader = new StringReader(xml);
            if(builder == null) {
                builder = proci.newDocumentBuilder();
                if (debug) {

                    System.err.println("New documentBuilder obj: " + System.identityHashCode(builder));
                }
            } else if (debug) {

                System.err.println("documentBuilder obj passed as argument: " + System.identityHashCode(builder));
            }
            if (validator != null) {
                builder.setSchemaValidator(validator);
            }
            Source source = new SAXSource(new InputSource(reader));
            if (cwd != null && cwd.length() > 0) {
                if (!cwd.endsWith("/")) {
                    cwd = cwd.concat("/");
                }
                source.setSystemId(cwd);
            }
            
            XdmNode doc = builder.build(source);
            if (debug) {
                System.err.println("xmlParseString, Processor: " + System.identityHashCode(proci));
                System.err.println("documentBuilder obj: " + System.identityHashCode(builder));
                if(doc != null) {
                    System.err.println("XdmNode: " + System.identityHashCode(doc));
                    net.sf.saxon.type.ItemType suppliedItemType = SequenceTool.getItemType(doc.getUnderlyingValue(), proci.getUnderlyingConfiguration().getTypeHierarchy());
                    System.err.println("xmlParserString, ItemType: " + suppliedItemType.toString());
                }
            }
            return doc;
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }

    /**
     * Utility method
     * @param ctypeStr
     * @return
     */
    @CEntryPoint(name = "getTypeName")
    public static CCharPointer getTypeName(IsolateThread thread, CCharPointer ctypeStr) {
        String typeStr = CTypeConversion.toJavaString(ctypeStr);
        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr); 
                                           
        BuiltInAtomicType typei = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        StructuredQName sname = typei.getTypeName();
        String typeName =new QName(sname.getPrefix(), sname.getNamespaceUri().toString(), sname.getLocalPart()).getClarkName();
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(typeName);
        return holder.get();

    }


    /**
     * Utility method to get the String value from the XdmValue representation
     * @param xdmvalueRef
     * @return  CCharPointer object
     */
    @CEntryPoint(name = "j_getStringValue")
    public static CCharPointer getStringValue(IsolateThread thread, ObjectHandle xdmvalueRef) {
        XdmValue value = objectHandles.get(xdmvalueRef);
        final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(value.toString());
        return holder.get();
    }


    /**
     * Receives file name of XML document and parse to XdmNode object. It is possible to validate with a SchemaValidator
     * @param cwd - The current working directory for parsing the XML file
     * @param proci   Processor object
     * @param validator - Schema Validator
     * @param filename
     * @return  XdmNode object for the XML file
     * @throws SaxonApiException
     */
    public static XdmNode parseXmlFile(String cwd, DocumentBuilder builder, Processor proci, SchemaValidator validator, String filename) throws SaxonApiException {
        try {
            if(builder == null) {
                builder = proci.newDocumentBuilder();
            }
            Source source = resolveFileToSource(proci, cwd,filename, ResourceRequest.XML_NATURE);
            if (validator != null) {
                builder.setSchemaValidator(validator);
            }
            return builder.build(source);
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }




    /**
     * Resolve the file with the cwd
     * deprecated method
     *
     *
     * @param processor
     * @param cwd      - Current working directory
     * @param filename -
     * @param nature
     * @return File object
     * @throws SaxonApiException
     */
    public static File resolveFile(Processor processor, String cwd, String filename, String nature) throws SaxonApiException {
        return new File(resolveFileToSource(processor, cwd, filename,nature).getSystemId());

    }


    /**
     * Resolve file name. Returns the file as a Source object
     *
     *
     * @param processor
     * @param cwd      - Current working directory
     * @param filename
     * @param nature
     * @return Source
     * @throws SaxonApiException
     */
    public static Source resolveFileToSource(Processor processor, String cwd, String filename, String nature) throws SaxonApiException {

        Source source = null;
        
        Configuration configuration = processor.getUnderlyingConfiguration();
        ResourceResolver resolver = configuration.getResourceResolver();
        ResourceRequest request = new ResourceRequest();
        request.baseUri = cwd;
        request.uri = filename;
        request.nature = nature;

        try {
            source = resolver.resolve(request);

            if(source == null) {
                source = resolveFileToSource_fallback(cwd, filename);

            }

            return source;
        } catch(XPathException exception) {
            throw new SaxonApiException(exception);
        }

    }

    public static Source resolveFileToSource_fallback(String cwd, String filename) throws SaxonApiException {

            Source source = null;
            File file = null;

            if (cwd != null && cwd.length() > 0) {
                if (!cwd.endsWith("/")) {
                    cwd = cwd.concat("/");
                }

                try {
                    if (filename.startsWith("http:")) {
                        URI fileURI = null;
                        fileURI = new URI(filename);
                        URL url = fileURI.toURL();
                        InputStream in = url.openStream();
                        source = new StreamSource(in);
                        source.setSystemId(filename);

                    } else {

                        file = absoluteFile(cwd, filename);
                        source = new StreamSource(file);
                        source.setSystemId(file.getAbsolutePath());
                    }
                } catch (Exception e) {
                    URL url = null;
                    try {
                        URI fileURI = null;
                        fileURI = new URI(filename);
                        url = fileURI.toURL();
                        InputStream in = url.openStream();
                        source = new StreamSource(in);
                        source.setSystemId(filename);
                    } catch (MalformedURLException e1) {
                        throw new SaxonApiException(e);
                    } catch (IOException e1) {
                        throw new SaxonApiException(e);
                    } catch (URISyntaxException e1) {
                        throw new SaxonApiException(e1);
                    }

                }


                return source;
            } else {
                if (filename == null) {
                    throw new SaxonApiException("Null string found for the filename");
                }
                file = new File(filename);
                StreamSource stream = new StreamSource(file);
                stream.setSystemId(filename);
                source = stream;
                return source;
            }
        }



    /**
      * Create a File object given the filename and the cwd used fix-up the location of the file.
      *
      * @param cwd      - Supply the current working directory which the filename will be resolved against
      * @param filename
      * @return file object
      */
     public static File absoluteFile(String cwd, String filename) throws SaxonApiException{
         if(filename == null) {
              throw new SaxonApiException("Failed to create file - File name is null");
         }
         if(cwd == null) {
             try {
                 return new File(filename);
             }catch(NullPointerException ex) {
                 throw new SaxonApiException("Failed to create file");
             }
         };
         char separatorChar = '/';
         if (File.separatorChar != '/') {
             filename.replace(separatorChar, File.separatorChar);
             cwd.replace(separatorChar, File.separatorChar);
             separatorChar = '\\';
         }
         if (cwd!= null && !cwd.endsWith(String.valueOf(separatorChar))) {
             cwd = cwd.concat(String.valueOf(separatorChar));
         }
         File absFile = null;
         try {
             absFile = new File(filename);
         }catch(NullPointerException ex) {
             throw new SaxonApiException("Failed to create file");
         }
         String fullpath;
         if (!absFile.isAbsolute()) {
             fullpath = cwd + filename;
         } else {
             fullpath = filename;
         }
         try {
             return new File(fullpath);
         }catch(NullPointerException ex) {
             throw new SaxonApiException("Failed to create file - File name is null");
         }
     }

    /**
     * Resolve the output file and wrap it into a Serializer for use in XQuery and XSLT processors
     *
     * @param processor - The same processor used in XQuery or XSLT engine must be used. Otherwise errors might occur
     * @param cwd       - Current working directory
     * @param outfile   - the output filename where result will be stored
     * @param nature
     * @return Serializer
     * @throws SaxonApiException
     */
    public static Serializer resolveOutputFile(Processor processor, String cwd, String outfile, String nature) throws SaxonApiException {
        Serializer serializer = null;
        File file = null;
        try {
            file = resolveFile(processor, cwd, outfile, nature);
        } catch(Exception ex){
              throw new SaxonApiException(ex.getMessage());
            
        }
        if(file == null) {
            throw new SaxonApiException("Output file is null");
        }
        serializer = processor.newSerializer(file);
        return serializer;
    }

}
