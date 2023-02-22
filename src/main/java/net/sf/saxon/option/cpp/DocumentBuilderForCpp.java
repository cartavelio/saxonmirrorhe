package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import java.net.URI;
import java.net.URISyntaxException;

public class DocumentBuilderForCpp extends SaxonCAPI {

    @CEntryPoint(name = "createDocumentBuilder", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createDocumentBuilder(IsolateThread thread, ObjectHandle processorRef) throws SaxonApiException {
        if(processorRef != WordFactory.nullPointer()) {
            Processor processor = SaxonCAPI.objectHandles.get(processorRef);

            ObjectHandle handle = SaxonCAPI.objectHandles.create(processor.newDocumentBuilder());
            return handle.rawValue();
        }
        throw new SaxonApiException("Cannot create DocumentBuilder - Processor is null");
    }


    @CEntryPoint(name = "j_db_setBaseURI", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setBaseURI(IsolateThread thread, ObjectHandle documentBuilderRef, CCharPointer baseUri) throws SaxonApiException {

            DocumentBuilder builder = SaxonCAPI.objectHandles.get(documentBuilderRef);
            if(baseUri != WordFactory.nullPointer()) {
                try {
                    builder.setBaseURI(new URI(CTypeConversion.toJavaString(baseUri)));
                } catch (URISyntaxException e) {
                    throw new SaxonApiException(e);
                }
            }
            return SaxonCAPI.STATUS_OK;

    }



    @CEntryPoint(name = "j_db_setDTDValidation", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setDTDValidation(IsolateThread thread, ObjectHandle documentBuilderRef, int dtdVal) throws SaxonApiException {

            DocumentBuilder builder = SaxonCAPI.objectHandles.get(documentBuilderRef);
            builder.setDTDValidation(CTypeConversion.toBoolean(dtdVal));
            return SaxonCAPI.STATUS_OK;

    }



}
