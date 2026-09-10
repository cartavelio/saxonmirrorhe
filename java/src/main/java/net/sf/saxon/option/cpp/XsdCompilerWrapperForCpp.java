// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.WordFactory;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

public class XsdCompilerWrapperForCpp extends SaxonCAPI  {

    /**
     * create XsdCompiler with reference to object held in the Graalvm ObjectHandles pool.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @return XsdCompiler - ObjectHandle value of the XsdCompiler object held in the ObjectHandles pool
     */
    @CEntryPoint(name = "j_createXsdCompiler")
    public static long createXsdCompiler(IsolateThread thread, ObjectHandle processorRef, SaxonError saxonError) {

        Processor processor = null;
        if(processorRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);

        if (!processor.isSchemaAware()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is not licensed for schema processing!");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        XsdCompiler compiler = processor.newXsdCompiler();

        ObjectHandle handle = SaxonCAPI.createHandle(compiler);
        return handle.rawValue();


    }


    /**
     * Compile a Schema from string content to a XdmSchema.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param cxsd                        - File name of the schema relative to the cwd
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: ObjectHandle raw value representing the XsdSchema.
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_xsdCompileFromString")
    public static long j_xsdCompileFromString(IsolateThread thread, ObjectHandle xsdCompilerRef, CCharPointer csystemId, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Processor is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        SaxonCErrorReporter reporter = null;
        try {
            XsdCompilerWrapperForCpp.setProperties(compiler, params, values);
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
            reporter = new SaxonCErrorReporter();
            compiler.setErrorReporter(reporter);
            XsdSchema schema = compiler.compile(source);
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, reporter);
            saxonError.setStatus((int)SaxonCAPI.STATUS_EXCEPTION);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }

    /**
     * Compile a Schema from string content to a XdmSchema.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param cxsd                        - File name of the schema relative to the cwd
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: ObjectHandle raw value representing the XsdSchema.
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_xsdCompileFromNode")
    public static long j_xsdCompileFromNode(IsolateThread thread, ObjectHandle xsdCompilerRef, ObjectHandle cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "SchemaCompiler is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        SaxonCErrorReporter reporter = null;
        try {
            XsdCompilerWrapperForCpp.setProperties(compiler, params, values);


            XdmNode xsd = objectHandles.get(cxsd);

            if (xsd == null) {
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema document not found");
                return SaxonCAPI.STATUS_EXCEPTION;
            }

            reporter = new SaxonCErrorReporter();
            compiler.setErrorReporter(reporter);
            XsdSchema schema = compiler.compile(xsd.asSource());
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, reporter);
            saxonError.setStatus((int)SaxonCAPI.STATUS_EXCEPTION);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }

    /**
     * Compile a Schema from string content to a XdmSchema.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param xsdCompilerRef                - XsdCompiler reference
     * @return long value: ObjectHandle raw value representing the XsdSchema. Or -2 if exception is thrown
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_xsdempty")
    public static long j_xsdempty(IsolateThread thread, ObjectHandle xsdCompilerRef) {
        XsdCompiler compiler = null;

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);


        XsdSchema schema = compiler.emptySchema();
        ObjectHandle handle = SaxonCAPI.createHandle(schema);
        return handle.rawValue();

    }


    /**
     * Combine a set of schemas into one
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param c_xsdschemas                 - File name of the schema relative to the cwd
     * @param saxonError                  -  Struct to handle errors.
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_xsdcombine")
    public static long j_xsdcombine(IsolateThread thread, ObjectHandle xsdCompilerRef, ObjectHandle c_xsdschemas, SaxonError saxonError) {
        XsdCompiler compiler = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema not found");
            return SaxonCAPI.STATUS_EXCEPTION;
        }
        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);

        ProcessorDataAccumulator schemasPDA = objectHandles.get(c_xsdschemas);

        XsdSchema [] schemas = (XsdSchema[]) schemasPDA.getValues();

        try {
            XsdSchema schema = compiler.combine(schemas);
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }


    /**
     * Register the Schema which is given as a string representation.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                 - Processor reference
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param cxsd                        - File name of the schema relative to the cwd
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_xsdCompileFromFile")
    public static long j_xsdCompileFromFile(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdCompilerRef, CCharPointer csystemId, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        Processor processor = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "SchemaCompiler object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        SaxonCErrorReporter reporter = null;
        try {
            XsdCompilerWrapperForCpp.setProperties(compiler, params, values);
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

            Source source = resolveFileToSource(processor, systemId, xsd, ResourceRequest.XSD_NATURE);

            reporter = new SaxonCErrorReporter();
            compiler.setErrorReporter(reporter);
            XsdSchema schema = compiler.compile(source);
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, reporter);
            saxonError.setStatus((int) SaxonCAPI.STATUS_EXCEPTION);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }

    /**
     * Register the Schema which is given as a string representation.
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                 - Processor reference
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param xsdCompilerRef                       - File name of the schema relative to the cwd
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_xsdCompileFromProcessorData")
    public static long j_xsdCompileFromProcessorData(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdCompilerRef, CCharPointer csystemId, ObjectHandle xsdFilenamesRef, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        Processor processor = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "SchemaCompiler object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        SaxonCErrorReporter reporter = null;
        try {
            XsdCompilerWrapperForCpp.setProperties(compiler, params, values);
            ProcessorDataAccumulator pd_filenames = null;

            if (xsdFilenamesRef!= WordFactory.nullPointer()) {
                pd_filenames = SaxonCAPI.objectHandles.get(xsdFilenamesRef);
            } else {
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Schema documents not found");
                return SaxonCAPI.STATUS_EXCEPTION;
            }
            String systemId = null;
            if (csystemId != WordFactory.nullPointer()) {
                systemId = SaxonCAPI.toJavaString(csystemId, WordFactory.nullPointer());
            }
            if(pd_filenames == null && pd_filenames.size() == 0) {
                SaxonCExceptionHandler.createStandardErrorResult(saxonError, "No filenames specified");
                return SaxonCAPI.STATUS_EXCEPTION;
            }
            Source [] sources = new Source[pd_filenames.size()];
            String [] filenames = pd_filenames.getParameterNames();

            for (int i = 0; i < pd_filenames.size(); i++) {
                sources[i] = resolveFileToSource(processor, systemId, filenames[i], ResourceRequest.XSD_NATURE);
            }

            reporter = new SaxonCErrorReporter();
            compiler.setErrorReporter(reporter);
            XsdSchema schema = compiler.compile(sources);
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, reporter);
            saxonError.setStatus((int)SaxonCAPI.STATUS_EXCEPTION);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }


    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                 - Processor reference
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param cxsd                        - the XML file containing the schema component model, as generated by a previous call on
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @param saxonError
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_xsdImportComponentsFromNode")
    public static long j_xsdImportComponentsFromNode(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdCompilerRef, ObjectHandle nodeRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        Processor processor = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XsdSchema object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        XdmNode node = null;

        if (nodeRef == WordFactory.nullPointer()) {
            node = SaxonCAPI.objectHandles.get(nodeRef);
        } else {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XsdSchema object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        try {

            XsdSchema schema = compiler.importComponents(node.asSource());
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        }  catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }



    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param thread                      - Graalvm runtime data structure for the thread
     * @param processorRef                 - Processor reference
     * @param xsdCompilerRef                - XsdCompiler reference
     * @param cxsd                        - the XML file containing the schema component model, as generated by a previous call on
     * @param csystemId                   - The system ID of the document
     * @param processorDataAccumulatorRef - parameters and property names given as reference to the ProcessorData object.
     * @param saxonError
     * @return long value: Success= 0, Failure = -1 or Exception thrown = -2
     */
    @CEntryPoint(name = "j_xsdImportComponents")
    public static long j_xsdImportComponents(IsolateThread thread, ObjectHandle processorRef, ObjectHandle xsdCompilerRef, CCharPointer csystemId, CCharPointer cxsd, ObjectHandle processorDataAccumulatorRef, SaxonError saxonError) {
        XsdCompiler compiler = null;
        Processor processor = null;
        if (xsdCompilerRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XsdSchema object is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        processor = SaxonCAPI.objectHandles.get(processorRef);

        compiler = SaxonCAPI.objectHandles.get(xsdCompilerRef);
        ProcessorDataAccumulator processorDataAccumulator;
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        try {
            XsdCompilerWrapperForCpp.setProperties(compiler, params, values);
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

            Source source = resolveFileToSource(processor, systemId, xsd, ResourceRequest.XSD_NATURE);

            XsdSchema schema = compiler.importComponents(source);
            ObjectHandle handle = SaxonCAPI.createHandle(schema);
            return handle.rawValue();
        }  catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }

    }


    private static void setProperties(XsdCompiler compiler, String[] params, Object[] values) throws SaxonApiException {
        if (params != null && values != null) {
            for (int i = 0; i < params.length; i++) {
                 if (params[i].equals("xsdversion")) {
                    if (values[i] instanceof String) {
                        String xsdversion = (String) values[i];

                        compiler.setXsdVersion(xsdversion);
                    } else {
                        SaxonApiException ex = new SaxonApiException("XSD version has not been correctly set");
                        throw ex;
                    }
                }
            }
        }
    }

}


