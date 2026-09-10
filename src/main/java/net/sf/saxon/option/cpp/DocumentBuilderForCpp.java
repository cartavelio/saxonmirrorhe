////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

import java.io.File;
import java.net.URI;
import java.net.URISyntaxException;

public class DocumentBuilderForCpp extends SaxonCAPI {

    @CEntryPoint(name = "createDocumentBuilder", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createDocumentBuilder(IsolateThread thread, ObjectHandle processorRef) throws SaxonApiException {
        if(processorRef != WordFactory.nullPointer()) {
            Processor processor = SaxonCAPI.objectHandles.get(processorRef);

            ObjectHandle handle = SaxonCAPI.createHandle(processor.newDocumentBuilder());
            return handle.rawValue();
        }
        throw new SaxonApiException("Cannot create DocumentBuilder - Processor is null");
    }


    @CEntryPoint(name = "j_db_setBaseURI", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setBaseURI(IsolateThread thread, ObjectHandle documentBuilderRef, CCharPointer baseUri) throws SaxonApiException {

            DocumentBuilder builder = SaxonCAPI.objectHandles.get(documentBuilderRef);
            if(baseUri != WordFactory.nullPointer()) {
                try {
                    String userDir = System.getProperty("user.dir");
                    char separatorChar = '/';
                    if (File.separatorChar != '/') {
                        userDir.replace(File.separatorChar, separatorChar);
                    }
                    URI cwdURI = new File(userDir).toURI();
                    cwdURI = cwdURI.resolve(CTypeConversion.toJavaString(baseUri));
                    builder.setBaseURI(cwdURI);
                } catch (Exception e) {
                    throw new SaxonApiException(e);
                }
            }
            return SaxonCAPI.STATUS_OK;

    }

    @CEntryPoint(name = "j_db_setLineNumbering", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setLineNumbering(IsolateThread thread, ObjectHandle documentBuilderRef, int option) throws SaxonApiException {

            DocumentBuilder builder = SaxonCAPI.objectHandles.get(documentBuilderRef);

            builder.setLineNumbering(CTypeConversion.toBoolean(option));
            return SaxonCAPI.STATUS_OK;
    }



    @CEntryPoint(name = "j_db_setDTDValidation", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setDTDValidation(IsolateThread thread, ObjectHandle documentBuilderRef, int dtdVal) throws SaxonApiException {

            DocumentBuilder builder = SaxonCAPI.objectHandles.get(documentBuilderRef);
            builder.setDTDValidation(CTypeConversion.toBoolean(dtdVal));
            return SaxonCAPI.STATUS_OK;

    }



}
