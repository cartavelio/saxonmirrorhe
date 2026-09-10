package com.saxonica.serialize;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.serialize.Emitter;
import net.sf.saxon.serialize.ExpandedStreamResult;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

import javax.xml.transform.stream.StreamResult;
import java.io.*;
import java.net.URISyntaxException;

/**
 * This is an abstract superclass for the two binary output methods saxon:base64Binary and saxon:hexBinary.
 */
public abstract class BinaryEmitter extends Emitter {

    protected OutputStream outputStream;
    private boolean mustClose;

    public BinaryEmitter(StreamResult streamResult, PipelineConfiguration pipe) throws XPathException {
        setPipelineConfiguration(pipe);
        this.outputStream = makeOutputStream(streamResult);
    }

    @Override
    public void open() throws XPathException {

    }

    @Override
    public void startDocument(int properties) throws XPathException {

    }

    @Override
    public void endDocument() throws XPathException {

    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
    }

    @Override
    public void endElement() throws XPathException {

    }

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        assert outputStream != null;
        byte[] bytes = textNodeToBinary(chars.tidy());
        try {
            outputStream.write(bytes);
        } catch (IOException err) {
            throw new XPathException(err);
        }
    }

    protected abstract byte[] textNodeToBinary(UnicodeString chars) throws XPathException;

    @Override
    public void processingInstruction(String name, UnicodeString data, Location locationId, int properties) throws XPathException {

    }

    @Override
    public void comment(UnicodeString content, Location locationId, int properties) throws XPathException {

    }

    /**
     * Notify the end of the event stream
     *
     * @throws XPathException if an error occurs
     */
    @Override
    public void close() throws XPathException {
        try {
            if (mustClose && outputStream != null) {
                outputStream.close();
            }
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }

    @Override
    public boolean usesTypeAnnotations() {
        return false;
    }

    protected OutputStream makeOutputStream(StreamResult streamResult) throws XPathException {
        if (streamResult.getOutputStream() != null) {
            return streamResult.getOutputStream();
        }
        String uriString = streamResult.getSystemId();
        if (uriString == null) {
            throw new XPathException("BinaryEmitter result has no system ID or output stream defined", SaxonErrorCode.SXRD0004);
        }

        try {
            File file = ExpandedStreamResult.makeWritableOutputFile(uriString);
            outputStream = new FileOutputStream(file);
            // Set the outputstream in the StreamResult object so that the
            // call on OutputURIResolver.close() can close it
            streamResult.setOutputStream(outputStream);
            mustClose = true;
        } catch (FileNotFoundException | URISyntaxException | IllegalArgumentException fnf) {
            // for example, the system ID doesn't use the file: scheme
            XPathException err = new XPathException("Unable to write to output destination", fnf);
            err.setErrorCode(SaxonErrorCode.SXRD0004);
            throw err;
        }
        return outputStream;
    }
}

// Copyright (c) 2010-2023 Saxonica Limited