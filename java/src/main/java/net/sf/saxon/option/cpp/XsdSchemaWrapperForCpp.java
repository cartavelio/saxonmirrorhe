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
import java.io.File;

public class XsdSchemaWrapperForCpp  extends SaxonCAPI {


    /**
     * Create a {@link SchemaValidator} which can be used to validate instance documents against this schema
     *
     * @param thread       - Graalvm runtime data structure for the thread
     * @param xsdSchemaRef - XsdSchema reference
     * @return long value representing the {@code SchemaValidator}
     * @throws SaxonApiException if null found for the processor or schema document
     */
    @CEntryPoint(name = "j_newSchemaValidator")
    public static long j_newSchemaValidator(IsolateThread thread, ObjectHandle xsdSchemaRef) {

        XsdSchema schema = SaxonCAPI.objectHandles.get(xsdSchemaRef);

        SchemaValidator validator = schema.newValidator();
        ObjectHandle handle = SaxonCAPI.createHandle(validator);
        return handle.rawValue();

    }


    /**
     * Combine this schema with another, to form a new combined schema.
     * <p>Note: in relatively unusual circumstances, a document that is valid against one of the two input schemas
     * might not be valid against the combined schema. For example, if the first schema has a wildcard with
     * <code>processContents="lax"</code> then an element might be laxly validated when assessed using that schema,
     * but strictly validated when assessed against the combined schema; and strict validation may fail while
     * lax validation succeeded.</p>
     *
     * @param xsdSchemaRef         - XsdSchema reference
     * @param c_other_xsdSchemaRef - XsdSchema reference to the other schema to combine with this one.
     * @return long value representing the combined schema, assuming the two schemas are compatible
     * @throws SaxonApiException if the two schemas cannot be combined, for example because they
     *                           contain components with duplicated names
     * @since 13.0
     */
    @CEntryPoint(name = "j_xsd_schema_combine")
    public static long j_xsd_schema_combine(IsolateThread thread, ObjectHandle xsdSchemaRef, ObjectHandle c_other_xsdSchemaRef, SaxonError saxonError) {
        XsdSchema schema = null;
        XsdSchema other = null;
        if (xsdSchemaRef == WordFactory.nullPointer() || c_other_xsdSchemaRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XsdSchema is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }


        try {

            schema = SaxonCAPI.objectHandles.get(xsdSchemaRef);
            other = SaxonCAPI.objectHandles.get(c_other_xsdSchemaRef);

            XsdSchema result = schema.combine(other);
            ObjectHandle handle = SaxonCAPI.createHandle(result);
            return handle.rawValue();
        } catch (SaxonApiException e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }


    @CEntryPoint(name = "j_exportComponentsToFile")
    public static long exportComponentsToFile(IsolateThread thread, ObjectHandle xsdSchemaRef, /*CCharPointer csystemId,*/ CCharPointer c_filename, SaxonError saxonError) {
        XsdSchema schema = null;
        if (xsdSchemaRef == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "XsdSchema is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        schema = SaxonCAPI.objectHandles.get(xsdSchemaRef);
        Processor processor = (Processor) schema.getUnderlyingSchema().getConfiguration().getProcessor();

        String filename = null;
        try {
            if (c_filename != WordFactory.nullPointer()) {
                filename = SaxonCAPI.toJavaString(c_filename, WordFactory.nullPointer());
            }
            String systemId = null;
            /*if (csystemId != WordFactory.nullPointer()) {
                systemId = SaxonCAPI.toJavaString(csystemId, WordFactory.nullPointer());
            }*/

            Source source = resolveFileToSource(processor, systemId, filename, ResourceRequest.XSD_NATURE);
            Serializer dest = processor.newSerializer();

            File file = SaxonCAPI.resolveFile(processor, systemId, filename, ResourceRequest.XSD_NATURE);

            dest.setOutputFile(file);

            schema.exportComponents(dest);
            return SaxonCAPI.STATUS_OK;
        } catch (SaxonApiException e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }


}
