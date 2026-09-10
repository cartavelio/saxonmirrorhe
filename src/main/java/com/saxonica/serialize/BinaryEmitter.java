package com.saxonica.serialize;

import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.str.CodePoints;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.serialize.Emitter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

import java.io.IOException;
import java.io.OutputStream;

/**
 * This is an abstract superclass for the two binary output methods saxon:base64Binary and saxon:hexBinary.
 */
public abstract class BinaryEmitter extends Emitter {

    /**
     * Determine whether the Emitter wants a Writer for character output or
     * an OutputStream for binary output. The standard Emitters all use a Writer, so
     * this returns true; but a subclass can override this if it wants to use an OutputStream
     *
     * @return false: this Emitter uses an OutputStream
     */

    @Override
    public boolean usesWriter() {
        return false;
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
    public void characters(CodePoints chars, Location locationId, int properties) throws XPathException {
        if (outputStream == null) {
            if (streamResult == null) {
                throw new IllegalStateException("BinaryEmitter must have a StreamResult to write to");
            }
            OutputStream os = streamResult.getOutputStream();
            if (os == null) {
                os = makeOutputStream();
            }
            if (os != null) {
                setOutputStream(os);
            } else {
                throw new IllegalStateException("BinaryEmitter's StreamResult must have an OutputStream to write to");
            }
        }
        byte[] bytes = textNodeToBinary(chars.tidy());
        try {
            outputStream.write(bytes);
        } catch (IOException err) {
            throw new XPathException(err);
        }
    }

    protected abstract byte[] textNodeToBinary(UnicodeString chars) throws XPathException;

    @Override
    public void processingInstruction(String name, CodePoints data, Location locationId, int properties) throws XPathException {

    }

    @Override
    public void comment(CodePoints content, Location locationId, int properties) throws XPathException {

    }

    @Override
    public boolean usesTypeAnnotations() {
        return false;
    }
}

// Copyright (c) 2010-2020 Saxonica Limited