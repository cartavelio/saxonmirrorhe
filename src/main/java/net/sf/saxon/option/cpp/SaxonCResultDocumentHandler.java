////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

public class SaxonCResultDocumentHandler implements java.util.function.Function<URI, Destination> {


    final Map<String, XdmDestination> results = new HashMap<>();

    final Map<String, RawDestination> rawResults = new HashMap<>();

    private boolean rawResultsFlag;

    public SaxonCResultDocumentHandler() {
        rawResultsFlag = false;
    }

    public SaxonCResultDocumentHandler(boolean r) {
        rawResultsFlag = r;
    }

    @CEntryPoint(name = "createSaxonCResultDocumentHandler")
    public static long createSaxonCResultDocumentHandler(IsolateThread thread, int flagInt) {
        SaxonCResultDocumentHandler resultDocumentHandler = new SaxonCResultDocumentHandler(CTypeConversion.toBoolean(flagInt));
        ObjectHandle handle = SaxonCAPI.createHandle(resultDocumentHandler);
        return handle.rawValue();
    }


    @Override
    public Destination apply(URI uri) {

        String href = uri.toASCIIString();

        if (rawResultsFlag) {
            RawDestination dest = null;
            dest = new RawDestination();
            dest.setDestinationBaseURI(uri);
            rawResults.put(href, dest);
            if (SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: href=" + href + "added");
                System.err.println("SaxonC Debug - ResultDocumentHandler: size=" + rawResults.size());
            }

            return dest;
        } else {
            XdmDestination dest = null;
            dest = new XdmDestination();
            dest.setDestinationBaseURI(uri);
            results.put(href, dest);
            if (SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: href=" + href + "added");
                System.err.println("SaxonC Debug - ResultDocumentHandler: size=" + results.size());
            }
            return dest;
        }


    }


    @CEntryPoint(name = "j_getResultDocuments", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static CLongPointer getResultDocuments(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle rdRef) {
        SaxonCResultDocumentHandler rdHandler = SaxonCAPI.objectHandles.get(rdRef);
        XdmValue[] results = null;
        if (rdHandler.rawResultsFlag) {
            results = rdHandler.getRawResults();
        } else {
            results = rdHandler.getXdmResultDocuments();
        }
        int len = results.length;
        long[] resultRefs = Arrays.stream(results).mapToLong(i -> {
            ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
            return objectHandle.rawValue();
        }).toArray();
        CLongPointer addr = SaxonCAPI.longArrayToNativeWithLength(alloc, resultRefs);
        return addr;
    }


    public XdmNode[] getXdmResultDocuments() {
        XdmNode[] resultDocArr = new XdmNode[results.size()];
        Collection<XdmDestination> resultDocCollection = results.values();
        int i = 0;
        if (resultDocCollection.size() == 0) {
            if (SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument is empty");
            }
            return null;
        }
        for (XdmDestination dest : resultDocCollection) {
            resultDocArr[i] = dest.getXdmNode();
            i++;
        }
        if (SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument.length" + resultDocArr.length);
        }
        return resultDocArr;
    }


    public XdmValue[] getRawResults() {
        XdmValue[] resultsArr = new XdmValue[rawResults.size()];
        Collection<RawDestination> resultDocCollection = rawResults.values();
        int i = 0;
        if (resultDocCollection.size() == 0) {
            if (SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument is empty");
            }
            return null;
        }
        for (RawDestination dest : resultDocCollection) {
            resultsArr[i] = dest.getXdmValue();
            i++;
        }
        if (SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument.length" + resultsArr.length);
        }
        return resultsArr;
    }

    @CEntryPoint(name = "j_getResultDocumentURIs", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static long getResultDocumentURIs(IsolateThread thread, ObjectHandle rdRef) {
        SaxonCResultDocumentHandler rdHandler = SaxonCAPI.objectHandles.get(rdRef);
        Collection<? extends AbstractDestination> resultDocCollection;
        if (rdHandler.rawResultsFlag) {
            resultDocCollection = rdHandler.rawResults.values();

        } else {
            resultDocCollection = rdHandler.results.values();

        }

        if (resultDocCollection.size() == 0) {
            if (SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocumentURIs is empty");
            }
            return SaxonCAPI.STATUS_FAIL;
        }
        ProcessorDataAccumulator processorData = new ProcessorDataAccumulator(resultDocCollection.size(), true);
        for (AbstractDestination dest : resultDocCollection) {
            processorData.addProperty(dest.getDestinationBaseURI().toASCIIString());
        }

        ObjectHandle handle = SaxonCAPI.createHandle(processorData);
        return handle.rawValue();
    }


    public XdmNode getResultDocument(String baseURI) throws SaxonApiException {
        try {
            XdmDestination dest = results.get(new URI(baseURI));
            return dest.getXdmNode();
        } catch (URISyntaxException e) {
            throw new SaxonApiException(e.getMessage());
        }

    }

}
