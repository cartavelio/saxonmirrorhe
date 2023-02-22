package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.XdmNode;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

public class SaxonCResultDocumentHandler implements java.util.function.Function<URI, Destination>{


    final Map<String, XdmDestination> results = new HashMap<>();

    public SaxonCResultDocumentHandler() { }



    @Override
    public Destination apply(URI uri) {

        XdmDestination dest = new XdmDestination();
        dest.setDestinationBaseURI(uri);
        String href = uri.toASCIIString();
        results.put(href, dest);
        if(SaxonCAPI.debug) {
            System.err.println("SaxonC Debug - ResultDocumentHandler: href="+ href + "added");
            System.err.println("SaxonC Debug - ResultDocumentHandler: size="+ results.size());
        }
        return dest;
    }


    public XdmNode [] getResultDocuments() {
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
