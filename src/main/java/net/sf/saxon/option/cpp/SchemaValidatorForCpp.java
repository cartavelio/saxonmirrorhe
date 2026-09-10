////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;


import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.om.AtomicArray;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.s9api.*;
import net.sf.saxon.value.AtomicValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

/**
 * A <tt>SchemaValidator</tt> is an object that is used for validating instance documents against a schema.
 * This class is a wrapper for the SchemaValidator designed for SaxonC.
 */
public class SchemaValidatorForCpp extends SaxonCAPI {

    private SchemaManager schemaManager = null;
    private Source source = null;
    private XdmDestination xdmDestination = null;


    /*
     *   Default constructor to create a Schema valditor based on the s9api. Assume license file is available.
     *   throw SaxonApiException if processor not licensed
     **/

    public SchemaValidatorForCpp() throws SaxonApiException {
        processor = new Processor(true);
        schemaManager = processor.getSchemaManager();
    }

    public SchemaValidatorForCpp(Processor proc) throws SaxonApiException {
        processor = proc;
        schemaManager = processor.getSchemaManager();
    }

    @CEntryPoint(name = "createSchemaValidator", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createSchemaValidator(IsolateThread thread) throws SaxonApiException {
        SchemaValidatorForCpp validatorForCpp = new SchemaValidatorForCpp();
        Processor processor = validatorForCpp.processor;
        if (debug && !processor.isSchemaAware()) {
            throw new SaxonApiException("Processor is not licensed for schema processing!");
        }

        ObjectHandle handle = SaxonCAPI.createHandle(validatorForCpp);
        return handle.rawValue();


    }

    /*
     *   Default constructor to create a Schema valditor based on the s9api. Assume license file is available.
     *   @param proc - Supplied processor. Assume that license feature is enabled
     *   throw SaxonApiException if processor not licensed
     **/
    @CEntryPoint(name = "createSchemaValidatorWithProcessor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createSchemaValidatorWithProcessor(IsolateThread thread, ObjectHandle procRef) throws SaxonApiException {
        if (procRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        Processor processor = SaxonCAPI.objectHandles.get(procRef);

        SchemaValidatorForCpp validatorForCpp = new SchemaValidatorForCpp(processor);
        if (!processor.isSchemaAware()) {
            throw new SaxonApiException("Processor is not licensed for schema processing!");
        }

        ObjectHandle handle = SaxonCAPI.createHandle(validatorForCpp);
        return handle.rawValue();

    }

    /**
     * Get the Schema manager
     *
     * @return SchemaManager
     */
    public SchemaManager getSchemaManager() {
        return schemaManager;
    }


    /**
     * Get the Schema manager
     *
     * @return SchemaManager
     */
    @CEntryPoint(name = "j_getSchemaManager")
    public static long j_getSchemaManager(IsolateThread thread, ObjectHandle processorRef) {
        Processor processor = objectHandles.get(processorRef);
        SchemaManager manager = processor.getSchemaManager();
        ObjectHandle handle = SaxonCAPI.createHandle(manager);
        return handle.rawValue();
    }





    public  void setSource(Source s) {
        source = s;
    }


    public  Source getSource() {
        return source;
    }


    public XdmNode getValidationReport() throws SaxonApiException {
        if(xdmDestination == null) {
            return null;
        }
       return xdmDestination.getXdmNode();
    }

    @CEntryPoint(name = "j_getValidationReport", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_getValidationReport(IsolateThread thread, ObjectHandle schemaValidatorForCpp) throws SaxonApiException {
        SchemaValidatorForCpp schemaValidatorForCpp1 = SaxonCAPI.objectHandles.get(schemaValidatorForCpp);
        XdmNode node = schemaValidatorForCpp1.getValidationReport();
        if(node == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.createHandle(node);
        return handle.rawValue();
    }


    /**
     * Register the Schema by file name.
     *
     * @param cwd    - Current Working directory
     * @param xsd    - File name of the schema relative to the cwd
     * @param params - parameters and property names given as an array of stings.
     *               We handle processor properties here
     * @param values - the values of the parameters and properties. given as a array of Java objects
     */
    @CEntryPoint(name = "j_registerSchema", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long registerSchema(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if (processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        Processor processor = SaxonCAPI.objectHandles.get(processorRef);

        ProcessorDataAccumulator processorDataAccumulator = null;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }

        SchemaValidatorForCpp.setProperties(processor, params, values);
        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = CTypeConversion.toJavaString(ccwd);
        }

        String xsd = CTypeConversion.toJavaString(cxsd);

        if (xsd == null) {
            SaxonApiException ex = new SaxonApiException("Schema document not found");
            throw ex;
        }
        Source source_xsd = resolveFileToSource(processor, cwd, xsd, ResourceRequest.XSD_NATURE);

        //validator.setErrorListener(errorListener);
        processor.getSchemaManager().load(source_xsd);
        return SaxonCAPI.STATUS_OK;

    }

    /**
     * Register the Schema which is given as a string representation.
     *
     * @param cwd      - Current Working directory
     * @param xsd      - File name of the schema relative to the cwd
     * @param systemId - The system ID of the document
     * @param params   - parameters and property names given as an array of stings.
     *                 We handle processor properties here
     * @param values   - the values of the parameters and properties. given as a array of Java objects
     */
    @CEntryPoint(name = "j_registerSchemaString")
    public static long registerSchemaString(IsolateThread thread, ObjectHandle processorRef, CCharPointer cxsd, CCharPointer csystemId, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        Processor processor = null;
        if (processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        SchemaValidatorForCpp.setProperties(processor, params, values);
        String xsd = CTypeConversion.toJavaString(cxsd);
        String systemId = null;
        if (csystemId != WordFactory.nullPointer()) {
            systemId = CTypeConversion.toJavaString(csystemId);
        }

        if (xsd == null) {
            SaxonApiException ex = new SaxonApiException("Schema document not found");
            throw ex;
        }

        StreamSource source = null;
        if(systemId != null) {
            source = new StreamSource(new StringReader(xsd), systemId);
        } else {
            source = new StreamSource(new StringReader(xsd));
        }

        processor.getSchemaManager().load(source);
        return SaxonCAPI.STATUS_OK;

    }


    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param cwd        The current working directory
     * @param outputfile The file name to which the SCM will be saved
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_exportSchema", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long exportSchema(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer coutputfile) throws SaxonApiException {
        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = CTypeConversion.toJavaString(ccwd);
        }
        String outputfile = CTypeConversion.toJavaString(coutputfile);


        Processor processor = null;
        if (processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);

        Serializer serializer = resolveOutputFile(processor, cwd, outputfile, ResourceRequest.XSD_NATURE);
        processor.getSchemaManager().exportComponents(serializer);
        return SaxonCAPI.STATUS_OK;

    }

    private static void setProperties(Processor processor, String[] params, Object[] values) throws SaxonApiException {
        if (params != null && values != null) {
            for (int i = 0; i < params.length; i++) {
                if (params[i].startsWith("http://saxon.sf.net/feature")) {
                    if (debug) {
                        System.err.println("java: parameter name:" + params[i]);
                        System.err.println("java: parameter length:" + params[i].length());
                    }
                    String name = params[i];
                    String value = (String) values[i];
                    processor.setConfigurationProperty(name, value);
                } else if (params[i].equals("xsdversion")) {
                    if (values[i] instanceof String) {
                        String xsdversion = (String) values[i];

                        SchemaManager manager = processor.getSchemaManager();
                        manager.setXsdVersion(xsdversion);
                    } else {
                        SaxonApiException ex = new SaxonApiException("XSD version has not been correctly set");
                        throw ex;
                    }
                }
            }
        }
    }


    /**
     * Validate an instance document supplied as a Source object
     *
     * @param cwd            - Current working directory
     * @param sourceFilename - The name of the file to be validated
     * @param outfilename    - The name of the file where output from the validator will be sent. Can be null.
     * @param params         - Parameters and properties names required by the Validator. This could contain the source as a node , source as string or file name, validator options, etc
     * @param values         -  The values for the parameters and properties required by the Validator
     **/
    @CEntryPoint(name = "j_validate", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long validate(IsolateThread thread, ObjectHandle processorRef, ObjectHandle schemaValidatorForCppRef, CCharPointer ccwd, CCharPointer csourceFilename, CCharPointer c_outfilename, ObjectHandle processorDataRef) throws SaxonApiException {
        Source source = null; //This is required to make sure the source object created from a previous call is not used
        SchemaValidatorForCpp schemaValidatorForCpp = null;
        Processor processor = null;

        if (processorRef != WordFactory.nullPointer()) {
            processor = SaxonCAPI.objectHandles.get(processorRef);
        } else {
            throw new SaxonApiException("Processor object is null");
        }

        if (schemaValidatorForCppRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Schema Manager is null");
        } else {
            schemaValidatorForCpp = SaxonCAPI.objectHandles.get(schemaValidatorForCppRef);
        }

        if (!processor.isSchemaAware()) {
            SaxonApiException ex = new SaxonApiException("Processor is not licensed for schema processing!");
            throw ex;
        }


        SchemaValidator validator = null;
        validator = schemaValidatorForCpp.getSchemaManager().newSchemaValidator();
        String outfilename = null;
        String cwd = null;
        Serializer serializer = null;
        if (c_outfilename != WordFactory.nullPointer()) {
            outfilename = CTypeConversion.toJavaString(c_outfilename);
        }

        if (ccwd != WordFactory.nullPointer()) {
            cwd = CTypeConversion.toJavaString(ccwd);
        }

        if (outfilename != null) {
            serializer = resolveOutputFile(processor, cwd, outfilename, null);

            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            validator.setDestination(serializer);
        }

        applySchemaProperties(cwd, processor, schemaValidatorForCpp, validator, processorDataRef);
        source = schemaValidatorForCpp.getSource();
        String sourceFilename = null;
        if (csourceFilename != WordFactory.nullPointer()) {
            sourceFilename = CTypeConversion.toJavaString(csourceFilename);
        }

        if (source == null && sourceFilename != null && !sourceFilename.isEmpty()) {
            source = resolveFileToSource(processor, cwd, sourceFilename, ResourceRequest.XML_NATURE);
        }

        if (source != null) {
            validator.validate(source);
        } else {
            SaxonApiException ex = new SaxonApiException("Source document not found");
            throw ex;
        }
         return SaxonCAPI.STATUS_OK;
    }



    /**
     * Validate an instance document supplied as a Source object with the validated document returned to the calling program
     *
     * @param cwd            - Current working directory
     * @param sourceFilename - The name of the file to be validated
     * @param params         - Parameters and properties names required by the Validator. This could contain the source as a node , source as string or file name, validator options, etc
     * @param values         -  The values for the parameters and properties required by the Validator
     * @return XdmNode
     **/
    @CEntryPoint(name = "j_validateToNode", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long validateToNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle schemaValidatorForCppRef, CCharPointer ccwd, CCharPointer csourceFilename, ObjectHandle processorDataRef) throws SaxonApiException {


        Processor processor = null;
        SchemaValidatorForCpp schemaValidatorForCpp = null;
        if(processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        } else {
           processor = SaxonCAPI.objectHandles.get(processorRef);
        }

        if (schemaValidatorForCppRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Schema Manager is null");
        } else {
            schemaValidatorForCpp = SaxonCAPI.objectHandles.get(schemaValidatorForCppRef);
        }



        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = CTypeConversion.toJavaString(ccwd);
        }

        String sourceFilename = null;
        SchemaValidator validator = null;
        validator = schemaValidatorForCpp.getSchemaManager().newSchemaValidator();

        applySchemaProperties(cwd, processor, schemaValidatorForCpp, validator, processorDataRef);

        Source source = schemaValidatorForCpp.getSource();

        if (csourceFilename != WordFactory.nullPointer()) {
            sourceFilename = CTypeConversion.toJavaString(csourceFilename);
        }

        if (source == null && sourceFilename != null && !sourceFilename.isEmpty()) {
            source = resolveFileToSource(processor, cwd, sourceFilename, ResourceRequest.XML_NATURE);
        }


        if (source != null) {

            XdmDestination destination = new XdmDestination();
            validator.setDestination(destination);
            validator.validate(source);
            ObjectHandle handle = SaxonCAPI.createHandle(destination.getXdmNode());
            return handle.rawValue();
        }  else {
            throw new SaxonApiException("Source document is null");
        }

    }


    /**
     * Applies the properties and parameters required in the transformation.
     * In addition we can supply the source, stylesheet and output file names.
     * We can also supply values to xsl:param and xsl:variables required in the stylesheet.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *  @param cwd       - current working directory
     * @param processor - required to use the same processor as for the compiled stylesheet
     * @param thisClass - pass the current object to set local variables supplied in the parameters
     * @param validator
     * @param processorDataRef
     */
    public static void applySchemaProperties(String cwd, Processor processor, SchemaValidatorForCpp thisClass, SchemaValidator validator, ObjectHandle processorDataRef) throws SaxonApiException {

        String[] params = null;
        Object[] values = null;
        if(processorDataRef != WordFactory.nullPointer()) {
            ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }

        if (params != null) {
            String outfile = null;

            DocumentBuilder builder = processor.newDocumentBuilder();

            if (params.length != values.length) {
                throw new SaxonApiException("Length of params array not equal to the length of values array");
            }
            if (params.length != 0) {
                if (cwd != null && cwd.length() > 0) {
                    if (!cwd.endsWith("/")) {
                        cwd = cwd.concat("/");
                    }
                }
                for (int i = 0; i < params.length; i++) {
                    if (debug) {
                        System.err.println("parameter name:" + params[i]);
                        System.err.println("parameter length:" + params[i].length());
                    }
                    if (params[i].equals("lax")) {
                        boolean lax = CTypeConversion.toBoolean(Integer.parseInt((String)values[i]));
                        validator.setLax(lax);
                    } else if (params[i].equals("element-name")) {
                        String paramName = (String) values[i];
                        QName qname = QName.fromClarkName(paramName);
                        validator.setDocumentElementName(qname);
                    } else if (params[i].equals("element-type")) {
                        String paramName = (String) values[i];
                        QName qname = QName.fromClarkName(paramName);
                        validator.setDocumentElementTypeName(qname);
                    } else if (params[i].equals("report-file")) {
                        if (values[i] != null && values[i] instanceof String) {
                            Serializer serializer = resolveOutputFile(processor, cwd, (String) values[i], ResourceRequest.XML_NATURE);
                            validator.setValidityReporting(serializer);
                        }

                    } else if (params[i].equals("report-node")) {
                        thisClass.xdmDestination = new XdmDestination();
                        validator.setValidityReporting(thisClass.xdmDestination);

                    } else if (params[i].equals("o") && outfile == null) {
                        if (values[i] instanceof String) {
                            outfile = (String) values[i];
                            Serializer serializer = thisClass.resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);

                            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
                            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
                            validator.setDestination(serializer);
                        }
                    } else if (params[i].equals("s")) {
                        if (values[i] instanceof String) {
                            thisClass.setSource(thisClass.resolveFileToSource(processor, cwd, (String) values[i], ResourceRequest.XML_NATURE));
                        } else if (debug) {
                            System.err.println("DEBUG: value error for property 's'");
                        }
                    } else if (params[i].equals("item") || params[i].equals("node") || params[i].equals("param:node")) {
                        if (debug) {
                            System.err.println("DEBUG: is null value=" + (values[i] == null));
                            if (values[i] != null) {
                                System.err.println("DEBUG: Type of value=" + (values[i]).getClass().getName());

                            }
                            System.err.println("DEBUG: setting the source for node");
                            System.err.println("DEBUG: is value a XdmNode=" + (values[i] instanceof XdmNode));
                            System.err.println("DEBUG: is value a XdmValue=" + (values[i] instanceof XdmValue));

                        }
                        Object value = values[i];
                        if (value instanceof XdmNode) {
                            XdmNode node = (XdmNode) value;
                            thisClass.setSource((node).asSource());
                        } else if (debug) {
                            System.err.println("Type of node Property error.");
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
                    } else if (params[i].equals("string")) {
                        XdmNode sourceNode = SaxonCAPI.main_parseXmlString(cwd, processor, builder, null, (String) values[i]);
                        thisClass.setSource(sourceNode.asSource());
                    } else if (params[i].startsWith("param:")) {
                        String paramName = params[i].substring(6);
                        Object value = values[i];
                        XdmValue valueForCpp = null;
                        QName qname = QName.fromClarkName(paramName);
                        if (value instanceof XdmValue) {
                            valueForCpp = (XdmValue) value;
                            if (debug) {
                                System.err.println("DEBUG: XSLTTransformerForCpp: " + paramName);
                                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue().toString());
                                net.sf.saxon.type.ItemType suppliedItemType = SequenceTool.getItemType(valueForCpp.getUnderlyingValue(), processor.getUnderlyingConfiguration().getTypeHierarchy());
                                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue());
                                System.err.println("DEBUG: XSLTTransformerForCpp Type: " + suppliedItemType.toString());
                            }

                        } else if (value instanceof Object[]) {
                            Object[] arr = (Object[]) value;
                            if (debug) {
                                System.err.println("DEBUG: Array of parameters found. arr len=" + arr.length);

                            }
                            List<AtomicValue> valueList = new ArrayList<AtomicValue>();
                            for (int j = 0; j < arr.length; j++) {
                                Object itemi = arr[j];
                                if (itemi == null) {
                                    System.err.println("Error: Null item at " + i + "th position in array of XdmValues");
                                    break;
                                }
                                if (debug) {
                                    System.err.println("Java object:" + itemi);
                                }
                                if (itemi instanceof XdmValue) {
                                    valueList.add((AtomicValue) (((XdmValue) itemi).getUnderlyingValue()));
                                } else {
                                    XdmValue valuex = getXdmValue(itemi);
                                    if (valuex == null) {
                                        System.err.println("Error: Null item at " + i + "th position in array of XdmValues when converting");
                                        break;
                                    }
                                    valueList.add((AtomicValue) (getXdmValue(itemi)).getUnderlyingValue());
                                }
                            }
                            AtomicArray sequence = new AtomicArray(valueList);
                            valueForCpp = XdmValue.wrap(sequence);
                        } else {
                            //fast track for primitive values
                            valueForCpp = getXdmValue(value);
                            if (debug) {
                                System.err.println("DEBUG: primitive value found");
                                net.sf.saxon.type.ItemType suppliedItemType = SequenceTool.getItemType(valueForCpp.getUnderlyingValue(), processor.getUnderlyingConfiguration().getTypeHierarchy());
                                System.err.println("XSLTTransformerForCpp Type: " + suppliedItemType.toString());
                            }
                        }


                        if (qname != null && valueForCpp != null) {
                            validator.setParameter(qname, valueForCpp);
                        }
                    }

                }

            }
        }

    }


    


    public static void main(String[] args) throws SaxonApiException {
        SaxonCAPI.debug = true;
        Processor processor = new Processor(true);
        SchemaValidatorForCpp validatorForCpp = new SchemaValidatorForCpp(processor);
       /*
        XdmValue item = SaxonCAPI.createXdmAtomicItem("QName", "{http://myDomain.co.uk/namespaces/ns1}myElement");
        validatorForCpp.getProcessor().setConfigurationProperty("http://saxon.sf.net/feature/validation-warnings", "true");
        validatorForCpp.getProcessor().setConfigurationProperty("http://saxon.sf.net/feature/xsd-version", "1.1");
        String cwd = "/Users/ond1/work/development/svn/saxon-dev/tests/junit/testdata/";
        Serializer serializer = processor.newSerializer();
        XdmNode resultNode = null;
        String resultStr = null;
        String[] paramsx = {"report-node", "verbose"};
        Object[] valuesx = {"true", "false"};
        validatorForCpp.registerSchema("/Users/ond1/work/development/files/millicom/LineNumber", "schema.xsd", null, null);
        validatorForCpp.validate("/Users/ond1/work/development/files/millicom/LineNumber", "example1.xml", null, paramsx, valuesx);
        resultNode = validatorForCpp.getValidationReport();

        if (resultNode != null) {
            resultStr = serializer.serializeNodeToString(resultNode);
            System.err.println("Validation Report-Millicom:" + resultStr);
        }


        System.out.println("\n\n Testing family.xml\n");
        validatorForCpp.registerSchema("/Users/ond1", "family-ext.xsd", null, null);
        validatorForCpp.registerSchema("/Users/ond1", "family.xsd", null, null);
        System.err.println("=============");
        validatorForCpp.validate("/Users/ond1/", "family.xml", null, paramsx, valuesx);
        XdmNode resultNode2 = validatorForCpp.getValidationReport();

        if (resultNode2 != null) {
            String resultStr2 = serializer.serializeNodeToString(resultNode2);
            System.err.println("Validation Report2:" + resultStr2);
        }
        System.err.println("=============");
        String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
        String sch1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\"" +
                " attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";


        String doc1 = "<request xmlns=\"http://myexample/family\">\n" +
                "  <Parent>John</Parent>\n" +
                "  <Child>Alice</Child>\n" +
                "</request>";
        String[] paramsSch = {"xsdversion"};
        Object[] valuesSch = {"1.1"};
        XdmNode sourceNode = validatorForCpp.parseXmlString(invalid_xml);
        String[] params = {"node", "xsdversion", "report-file"};
        Object[] values = {sourceNode, "1.1", "validationReport3.xml"};
        validatorForCpp.registerSchemaString(cwd, sch1, "file///o", paramsSch, valuesSch);

        validatorForCpp.validate(cwd, null, null, params, values);

        XdmNode sourceNode2 = validatorForCpp.parseXmlString(doc1);
        String[] params1 = {"string", "xsd-string", "xsdversion"};
        Object[] values1 = {doc1, sch1, "1.1"};

        System.err.println("Test 3: ");
        testValidator3(validatorForCpp);

        System.err.println("Test 5: ");
        validatorForCpp.registerSchemaString(cwd, sch1, "file///o1", paramsSch, valuesSch);
        //XdmNode resultDoc = validatorForCpp.validateToNode(cwd, null, null, null, params1, values1);

        validatorForCpp.validate(cwd, null, null, params1, values1);
        XdmNode outputNode2 = validatorForCpp.getValidationReport();

        XdmNode resultNode3 = validatorForCpp.getValidationReport();

        if (resultNode3 != null) {
            resultStr = serializer.serializeNodeToString(resultNode);
            System.err.println("Validation Report3:" + resultStr);

        }
            */

    }

}
