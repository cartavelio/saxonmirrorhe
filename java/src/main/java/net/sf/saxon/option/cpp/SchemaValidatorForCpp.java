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
     * This method calls the constructor class
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return - long value to the created SchemaValidatorForCpp object in the ObjectHandles pool
     */
    @CEntryPoint(name = "createSchemaValidator")
    public static long createSchemaValidator(IsolateThread thread, SaxonError saxonError){
        SchemaValidatorForCpp validatorForCpp = new SchemaValidatorForCpp();
        Processor processor = validatorForCpp.processor;
        if (debug && !processor.isSchemaAware()) {
            //throw new SaxonApiException("Processor is not licensed for schema processing!");
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is not licensed for schema processing!");
            return SaxonCAPI.STATUS_EXCEPTION;
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
    @CEntryPoint(name = "createSchemaValidatorWithProcessor")
    public static long createSchemaValidatorWithProcessor(IsolateThread thread, ObjectHandle procRef, SaxonError saxonError) {
        if (procRef == WordFactory.nullPointer()) {
            //throw new SaxonApiException("Processor object is null");
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }
        try {
            Processor processor = SaxonCAPI.objectHandles.get(procRef);

            SchemaValidatorForCpp validatorForCpp = new SchemaValidatorForCpp(processor);
            if (!processor.isSchemaAware()) {
                //throw new SaxonApiException("Processor is not licensed for schema processing!");
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is not licensed for schema processing!");
                return SaxonCAPI.STATUS_EXCEPTION;
            }

            ObjectHandle handle = SaxonCAPI.createHandle(validatorForCpp);
            return handle.rawValue();
        } catch (Exception ex) {
            SaxonCExceptionHandler.setErrorDetails(ex, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

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
    @CEntryPoint(name = "j_getValidationReport")
    public static long j_getValidationReport(IsolateThread thread, ObjectHandle schemaValidatorForCpp, SaxonError saxonError) {
        try {
            SchemaValidatorForCpp schemaValidatorForCpp1 = SaxonCAPI.objectHandles.get(schemaValidatorForCpp);
            XdmNode node = schemaValidatorForCpp1.getValidationReport();
            if (node == null) {
                return SaxonCAPI.STATUS_FAIL;
            }
            ObjectHandle handle = SaxonCAPI.createHandle(node);
            return handle.rawValue();
        } catch (Exception ex) {
            SaxonCExceptionHandler.setErrorDetails(ex, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
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
    @CEntryPoint(name = "j_registerSchema")
    public static long registerSchema(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {

        if (processorRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
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
        try {

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
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema document not found");
                return SaxonCAPI.STATUS_EXCEPTION;
            }
            Source source_xsd = resolveFileToSource(processor, cwd, xsd, ResourceRequest.XSD_NATURE);

            //validator.setErrorListener(errorListener);
            processor.getSchemaManager().load(source_xsd);
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
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
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_registerSchemaString")
    public static long registerSchemaString(IsolateThread thread, ObjectHandle processorRef, CCharPointer csystemId, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError)  {
        Processor processor = null;
        if (processorRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
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
        try {
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
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema document not found");
                return SaxonCAPI.STATUS_EXCEPTION;
            }

            StreamSource source = null;
            if (systemId != null) {
                source = new StreamSource(new StringReader(xsd), systemId);
            } else {
                source = new StreamSource(new StringReader(xsd));
            }

            processor.getSchemaManager().load(source);
            return SaxonCAPI.STATUS_OK;
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }

    /**
     * Register the Schema which is given as a Xdm Node Object.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                - processor reference
     * @param xsdRef                      - File name of the schema relative to the cwd
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_registerSchemaNode")
    public static long registerSchemaFromXdmNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdRef, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        Processor processor = null;
        if (processorRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        if (xsdRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XdmNode object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
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

        try {
            SchemaValidatorForCpp.setProperties(processor, params, values);
            XdmNode xsd = objectHandles.get(xsdRef);

            if (xsd == null) {
                SaxonApiException ex = new SaxonApiException("Schema document not found");
                throw ex;
            }

            processor.getSchemaManager().load(xsd.asSource());
            return SaxonCAPI.STATUS_OK;
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }


    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param thread       - Graalvm runtime data structure for the thread
     * @param processorRef - processor reference
     * @param ccwd         - The current working directory
     * @param coutputfile   - The file name to which the SCM will be saved
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return long value: Success= 0 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_exportSchema")
    public static long exportSchema(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer coutputfile, SaxonError saxonError){
        String cwd = null;
        try {
            if (ccwd != WordFactory.nullPointer()) {
                cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
            }
            String outputfile = SaxonCAPI.toJavaString(coutputfile, WordFactory.nullPointer());


            Processor processor = null;
            if (processorRef == WordFactory.nullPointer()) {
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
                return SaxonCAPI.STATUS_EXCEPTION;
            }

            processor = SaxonCAPI.objectHandles.get(processorRef);

            Serializer serializer = resolveOutputFile(processor, cwd, outputfile, ResourceRequest.XSD_NATURE);
            processor.getSchemaManager().exportComponents(serializer);
            return SaxonCAPI.STATUS_OK;
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

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
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return long value: Success= 0 or Exception thrown = -2
     **/
    @CEntryPoint(name = "j_validate")
    public static long validate(IsolateThread thread, ObjectHandle processorRef, ObjectHandle schemaValidatorForCppRef, CCharPointer ccwd, CCharPointer csourceFilename, CCharPointer c_outfilename, ObjectHandle processorDataRef, SaxonError saxonError)  {
        Source source = null; //This is required to make sure the source object created from a previous call is not used
        SchemaValidatorForCpp schemaValidatorForCpp = null;
        Processor processor = null;

        if (processorRef != WordFactory.nullPointer()) {
            processor = SaxonCAPI.objectHandles.get(processorRef);
        } else {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        if (schemaValidatorForCppRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema Manager is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        } else {
            schemaValidatorForCpp = SaxonCAPI.objectHandles.get(schemaValidatorForCppRef);
        }

        if (!processor.isSchemaAware()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is not licensed for schema processing");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        Map<QName, XdmValue> parameters = new HashMap<>();
        Properties props = new Properties();
        try {
            Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataRef, props, parameters, null, null, null, null, false);


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
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Source document not found");
                return SaxonCAPI.STATUS_EXCEPTION;
            }
            return SaxonCAPI.STATUS_OK;
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
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
     * @param saxonError   - if exception found then create SaxonError with exception details for callback
     * @return reference to the XdmNode object
     **/
    @CEntryPoint(name = "j_validateToNode")
    public static long validateToNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle schemaValidatorForCppRef, CCharPointer ccwd, CCharPointer csourceFilename, ObjectHandle processorDataRef, SaxonError saxonError) {


        Processor processor = null;
        SchemaValidatorForCpp schemaValidatorForCpp = null;
        if (processorRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        } else {
            processor = SaxonCAPI.objectHandles.get(processorRef);
        }

        if (schemaValidatorForCppRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema Manager is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        } else {
            schemaValidatorForCpp = SaxonCAPI.objectHandles.get(schemaValidatorForCppRef);
        }


        try {
            String cwd = null;
            if (ccwd != WordFactory.nullPointer()) {
                cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
            }

            String sourceFilename = null;
            SchemaValidator validator = null;
            validator = schemaValidatorForCpp.getSchemaManager().newSchemaValidator();

            Map<QName, XdmValue> parameters = new HashMap<>();
            Properties props = new Properties();
            Map<String, Object> optionsMap = setupConfigurationAndBuildMap(processorDataRef, props, parameters, null, null, null, null, false);

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
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Source document is null");
                return SaxonCAPI.STATUS_EXCEPTION;
            }

        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
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

}
