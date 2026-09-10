////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.*;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

public class SaxonCResultDocumentHandler implements java.util.function.Function<URI, Destination>{


    final Map<String, XdmDestination> results = new HashMap<>();

    final Map<String, RawDestination> rawResults = new HashMap<>();

    private boolean rawResultsFlag;

    public SaxonCResultDocumentHandler() {
        rawResultsFlag = false;
    }

    public SaxonCResultDocumentHandler(boolean r) {
        rawResultsFlag = r;
    }



    @Override
    public Destination apply(URI uri) {

        String href = uri.toASCIIString();

        if(rawResultsFlag) {
            RawDestination dest = null;
            dest = new RawDestination();
            dest.setDestinationBaseURI(uri);
            rawResults.put(href, dest);
            if(SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: href="+ href + "added");
                System.err.println("SaxonC Debug - ResultDocumentHandler: size="+ rawResults.size());
            }

            return dest;
        } else {
            XdmDestination dest = null;
            dest = new XdmDestination();
            dest.setDestinationBaseURI(uri);
            results.put(href, dest);
            if(SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: href="+ href + "added");
                System.err.println("SaxonC Debug - ResultDocumentHandler: size="+ results.size());
            }
            return dest;
        }


    }


    public XdmValue [] getResultDocuments() {
        if(rawResultsFlag) {
              return getResultDocuments();
        }   else {
            return getXdmResultDocuments();
        }
    }


    public XdmNode [] getXdmResultDocuments() {
        XdmNode [] resultDocArr = new XdmNode[results.size()];
        Collection<XdmDestination> resultDocCollection = results.values();
        int i=0;
        if(resultDocCollection.size() == 0) {
            if(SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument is empty");
            }
            return null;
        }
        for(XdmDestination dest : resultDocCollection) {
            resultDocArr[i] = dest.getXdmNode();
            i++;
        }
        if(SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument.length"+ resultDocArr.length);
        }
        return resultDocArr;
    }



    public XdmValue [] getRawResults() {
        XdmValue [] resultsArr = new XdmValue[rawResults.size()];
        Collection<RawDestination> resultDocCollection = rawResults.values();
        int i=0;
        if(resultDocCollection.size() == 0) {
            if(SaxonCAPI.debug) {
                System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument is empty");
            }
            return null;
        }
        for(RawDestination dest : resultDocCollection) {
            resultsArr[i] = dest.getXdmValue();
            i++;
        }
        if(SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument.length"+ resultsArr.length);
        }
        return resultsArr;
    }

    public String [] getResultDocumentURIs() {
        String [] resultDocUris = new String[results.size()];
        Collection<XdmDestination> resultDocCollection = results.values();
        int i=0;
        if(resultDocCollection.size() == 0) {
            return null;
        }
        for(XdmDestination dest : resultDocCollection) {
            resultDocUris[i] = dest.getBaseURI().toASCIIString();
            i++;
        }
        if(SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: getResultDocument-uris.length"+ resultDocUris.length);
        }
        return resultDocUris;
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
