////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;


import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.util.*;

/**
 * A <code>SchemaValidator</code> is an object that is used for validating instance documents against a schema.
 * This class is a wrapper for the SchemaValidator designed for SaxonC.
 */
public class SchemaValidatorForCpp extends SaxonCAPI {

    private SchemaManager schemaManager = null;
    private Source source = null;
    private XdmDestination xdmDestination = null;


    /**
     * Default constructor to create a Schema validator based on the s9api. Assume license file is available.
     */
    public SchemaValidatorForCpp() {
        processor = new Processor(true);
        schemaManager = processor.getSchemaManager();
    }

    /**
     * Constructor to create a Schema validator based on the s9api. Assume license file is available.
     *
     * @param proc - the processor object
     */
    public SchemaValidatorForCpp(Processor proc) {
        processor = proc;
        schemaManager = processor.getSchemaManager();
    }

    /**
     * create SchemaValidatorForCpp with reference to object held in the Graalvm ObjectHandles pool.
     * This methods calls the constructor class
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @return - long value to the created SchemaValidatorForCpp object in the ObjectHandles pool
     * @throws SaxonApiException Processor is not licensed
     */
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

    /**
     * Default constructor to create a Schema validator based on the s9api. Assume license file is available.
     *
     * @param thread  - Graalvm runtime data structure for the thread
     * @param procRef - Supplied reference to the processor object in the ObjectHandles pool. Assume that license feature is enabled
     * @throws SaxonApiException if processor is not licensed
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
     * @param thread       - Graalvm runtime data structure for the thread
     * @param processorRef - Supplied reference to the processor object in the ObjectHandles pool. Assume that license feature is enabled
     * @return long value reference to the SchemaManager object
     */
    @CEntryPoint(name = "j_getSchemaManager")
    public static long j_getSchemaManager(IsolateThread thread, ObjectHandle processorRef) {
        Processor processor = objectHandles.get(processorRef);
        SchemaManager manager = processor.getSchemaManager();
        ObjectHandle handle = SaxonCAPI.createHandle(manager);
        return handle.rawValue();
    }

    /**
     * Set the source document to the Schema validator
     *
     * @param s - source
     */
    public void setSource(Source s) {
        source = s;
    }

    /**
     * get the source
     *
     * @return the source object
     */
    public Source getSource() {
        return source;
    }

    /**
     * Get the validation report of the schema validation
     *
     * @return XdmNode object
     */
    public XdmNode getValidationReport() {
        if (xdmDestination == null) {
            return null;
        }
        return xdmDestination.getXdmNode();
    }

    /**
     * get the validation report as a reference to the created XdmNode object
     *
     * @param thread                - Graalvm runtime data structure for the thread
     * @param schemaValidatorForCpp - Reference to the SchemaValidatorForCpp object
     * @return long value to the Validator report object in the ObjectHandles pool.
     */
    @CEntryPoint(name = "j_getValidationReport", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_getValidationReport(IsolateThread thread, ObjectHandle schemaValidatorForCpp) {
        SchemaValidatorForCpp schemaValidatorForCpp1 = SaxonCAPI.objectHandles.get(schemaValidatorForCpp);
        XdmNode node = schemaValidatorForCpp1.getValidationReport();
        if (node == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.createHandle(node);
        return handle.rawValue();
    }


    /**
     * Register the Schema by file name.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                - processor reference
     * @param ccwd                        - Current Working directory
     * @param cxsd                        - File name of the schema relative to the cwd
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     *                                    We handle processor properties here
     * @return long value: Success= 0, Failure = -1 and Exception thrown = -2
     * @throws SaxonApiException if null found for the processor or schema document
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
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        String xsd = null;
        if (cxsd != WordFactory.nullPointer()) {
            xsd = SaxonCAPI.toJavaString(cxsd, WordFactory.nullPointer());
        }

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
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                - processor reference
     * @param cxsd                        - File name of the schema relative to the cwd
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_registerSchemaString", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long registerSchemaString(IsolateThread thread, ObjectHandle processorRef, CCharPointer csystemId, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
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
        String xsd = null;
        if (cxsd != WordFactory.nullPointer()) {
            xsd = SaxonCAPI.toJavaString(cxsd, WordFactory.nullPointer());
        }
        String systemId = null;
        if (csystemId != WordFactory.nullPointer()) {
            systemId = SaxonCAPI.toJavaString(csystemId, WordFactory.nullPointer());
        }

        if (xsd == null) {
            SaxonApiException ex = new SaxonApiException("Schema document not found");
            throw ex;
        }

        StreamSource source = null;
        if (systemId != null) {
            source = new StreamSource(new StringReader(xsd), systemId);
        } else {
            source = new StreamSource(new StringReader(xsd));
        }

        processor.getSchemaManager().load(source);
        return SaxonCAPI.STATUS_OK;

    }

    /**
     * Register the Schema which is given as a Xdm Node Object.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                - processor reference
     * @param xsdRef                      - File name of the schema relative to the cwd
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_registerSchemaNode", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long registerSchemaFromXdmNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdRef, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        Processor processor = null;
        if (processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }

        if (xsdRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmNode object is null");
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
        XdmNode xsd = objectHandles.get(xsdRef);

        if (xsd == null) {
            SaxonApiException ex = new SaxonApiException("Schema document not found");
            throw ex;
        }

        processor.getSchemaManager().load(xsd.asSource());
        return SaxonCAPI.STATUS_OK;

    }


    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param thread       - Graalvm runtime data structure for the thread
     * @param processorRef - processor reference
     * @param ccwd         - The current working directory
     * @param coutputfile  The file name to which the SCM will be saved
     * @return long value: Success= 0 or Exception thrown = -2
     * @throws SaxonApiException if null found for the processor object
     */
    @CEntryPoint(name = "j_exportSchema", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long exportSchema(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer coutputfile) throws SaxonApiException {
        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String outputfile = SaxonCAPI.toJavaString(coutputfile, WordFactory.nullPointer());


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
     * @param thread           - Graalvm runtime data structure for the thread
     * @param processorRef     - processor reference
     * @param ccwd             - Current working directory
     * @param csourceFilename  - The name of the file to be validated
     * @param c_outfilename    - The name of the file where output from the validator will be sent. Can be null.
     * @param processorDataRef - parameters and property names given as reference to the ProcessorData object.
     *                         Parameters and properties names required by the Validator. This could contain the source as a node , source as string or file name, validator options, etc
     * @return long value: Success= 0 or Exception thrown = -2
     * @throws SaxonApiException if null found for the processor or schema document
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

        Map<QName, XdmValue> parameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataRef, props, parameters, null, null, null, false);


        SchemaValidator validator = null;
        validator = schemaValidatorForCpp.getSchemaManager().newSchemaValidator();
        String outfilename = null;
        String cwd = null;
        Serializer serializer = null;
        if (c_outfilename != WordFactory.nullPointer()) {
            outfilename = SaxonCAPI.toJavaString(c_outfilename, WordFactory.nullPointer());
        }

        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        if (outfilename != null) {
            serializer = resolveOutputFile(processor, cwd, outfilename, null);

            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            validator.setDestination(serializer);
        }

        applySchemaProperties(cwd, processor, schemaValidatorForCpp, validator, optionsMap, parameters, props);
        source = schemaValidatorForCpp.getSource();
        String sourceFilename = null;
        if (csourceFilename != WordFactory.nullPointer()) {
            sourceFilename = SaxonCAPI.toJavaString(csourceFilename, WordFactory.nullPointer());
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
     * @param thread           - Graalvm runtime data structure for the thread
     * @param processorRef     - processor reference
     * @param ccwd             - Current working directory
     * @param csourceFilename  - The name of the file to be validated
     * @param processorDataRef - parameters and property names given as reference to the ProcessorData object.
     *                         Parameters and properties names required by the Validator. This could contain the source as a node , source as string or file name, validator options, etc
     * @return reference to the XdmNode object
     * @throws SaxonApiException if null found for the processor or schema document
     **/
    @CEntryPoint(name = "j_validateToNode", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long validateToNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle schemaValidatorForCppRef, CCharPointer ccwd, CCharPointer csourceFilename, ObjectHandle processorDataRef) throws SaxonApiException {


        Processor processor = null;
        SchemaValidatorForCpp schemaValidatorForCpp = null;
        if (processorRef == WordFactory.nullPointer()) {
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
        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        String sourceFilename = null;
        SchemaValidator validator = null;
        validator = schemaValidatorForCpp.getSchemaManager().newSchemaValidator();

        Map<QName, XdmValue> parameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataRef, props, parameters, null, null, null, false);

        applySchemaProperties(cwd, processor, schemaValidatorForCpp, validator, optionsMap, parameters, props);

        Source source = schemaValidatorForCpp.getSource();

        if (csourceFilename != WordFactory.nullPointer()) {
            sourceFilename = SaxonCAPI.toJavaString(csourceFilename, WordFactory.nullPointer());
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
        } else {
            throw new SaxonApiException("Source document is null");
        }

    }


    /**
     * Applies the properties and parameters required in the transformation.
     * In addition we can supply the source, stylesheet and output file names.
     * We can also supply values to xsl:param and xsl:variables required in the stylesheet.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *
     * @param cwd        - current working directory
     * @param processor  - required to use the same processor as for the compiled stylesheet
     * @param thisClass  - pass the current object to set local variables supplied in the parameters
     * @param validator  - the schema validator object
     * @param optionsMap - reference to the ProcessorDataAccumulator object held in the ObjectHandles pool. USed to wrap data for the SchemaValidator
     * @param parameters - map of parameters to apply to the Schema Validator
     * @param props
     * @throws SaxonApiException if null found for the processor or failure to apply a property
     */
    public static void applySchemaProperties(String cwd, Processor processor, SchemaValidatorForCpp thisClass, SchemaValidator validator, Map<String, Object> optionsMap, Map<QName, XdmValue> parameters, Properties props) throws SaxonApiException {

        if (optionsMap.isEmpty()) {
            return;
        }
        String outfile = null;
        Object valuei = null;

        DocumentBuilder builder = processor.newDocumentBuilder();


        if (optionsMap.containsKey("lax")) {
            boolean lax = CTypeConversion.toBoolean(Integer.parseInt((String) optionsMap.get("lax")));
            validator.setLax(lax);
        }

        if (optionsMap.containsKey("element-name")) {
            String paramName = (String) optionsMap.get("element-name");
            QName qname = QName.fromClarkName(paramName);
            validator.setDocumentElementName(qname);
        }

        if (optionsMap.containsKey("element-type")) {
            valuei = optionsMap.get("element-type");
            String paramName = (String) valuei;
            QName qname = QName.fromClarkName(paramName);
            validator.setDocumentElementTypeName(qname);
        }

        if (optionsMap.containsKey("report-file")) {
            valuei = optionsMap.get("report-file");
            if (valuei instanceof String) {
                Serializer serializer = resolveOutputFile(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE);
                validator.setValidityReporting(serializer);
            }

        }

        if (optionsMap.containsKey("report-node")) {
            thisClass.xdmDestination = new XdmDestination();
            validator.setValidityReporting(thisClass.xdmDestination);
        }

        if (optionsMap.containsKey("o")) {
            valuei = optionsMap.get("o");
            if (valuei instanceof String) {
                outfile = (String) valuei;
                if (debug) {
                    System.err.println("DEBUG SchemaValidatorForCpp: value for property 'o': cwd=" + cwd + ", outfile=" + outfile);
                }
                Serializer serializer = thisClass.resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);

                //TODO the serializer properties need to be passed from the user
                //serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
                //serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
                serializer.setOutputProperties(props);
                validator.setDestination(serializer);
            }

        }

        if (optionsMap.containsKey("s")) {
            valuei = optionsMap.get("s");
            if (valuei instanceof String) {
                thisClass.setSource(thisClass.resolveFileToSource(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE));
            } else if (debug) {
                System.err.println("DEBUG: value error for property 's'");
            }

        }


        if (optionsMap.containsKey("item") || optionsMap.containsKey("node") || optionsMap.containsKey("param:node")) {
            if(optionsMap.containsKey("item")) {
                valuei = optionsMap.get("item");
            } else if (optionsMap.containsKey("node")) {
                valuei = optionsMap.get("node");
            } else if (optionsMap.containsKey("param:node")) {
                valuei = optionsMap.get("param:node");
            }

            if (debug) {
                System.err.println("DEBUG: is null value=" + (valuei == null));
                if (valuei != null) {
                    System.err.println("DEBUG: Type of value=" + (valuei).getClass().getName());

                }
                System.err.println("DEBUG: setting the source for node");
                System.err.println("DEBUG: is value a XdmNode=" + (valuei instanceof XdmNode));
                System.err.println("DEBUG: is value a XdmValue=" + (valuei instanceof XdmValue));

            }
            if (valuei instanceof XdmNode) {
                XdmNode node = (XdmNode) valuei;
                thisClass.setSource((node).asSource());
            } else if (debug) {
                System.err.println("Type of node Property error.");
            }
        }
        if (optionsMap.containsKey("resources")) {
            valuei = optionsMap.get("resources");
            char separatorChar = '/';
            if (SaxonCAPI.RESOURCES_DIR == null && valuei instanceof String) {
                String dir1 = (String) valuei;
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

        }
        if (optionsMap.containsKey("string")) {
            valuei = optionsMap.get("string");
            XdmNode sourceNode = SaxonCAPI.main_parseXmlString(cwd, processor, builder, null, (String) valuei);
            thisClass.setSource(sourceNode.asSource());


        }

        if (!parameters.isEmpty()) {
            Map<QName, XdmValue> variables = parameters;
            for (Map.Entry<QName, XdmValue> entry : variables.entrySet()) {
                if (SaxonCAPI.debug) {
                    System.err.println("applyParameter to validator: key=" + entry.getKey().getClarkName() + ", Value=" + entry.getValue().toString());
                }
                validator.setParameter(entry.getKey(), entry.getValue());


            }
        } else {
            if (SaxonCAPI.debug) {
                System.err.println("Java DEBUG  - Validator: parameters map is empty");
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
