package com.saxonica.serialize;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.Whitespace;

import javax.xml.transform.stream.StreamResult;

/**
 * This emitter implements output method saxon:base64Binary. All nodes in the result tree other than text
 * nodes are ignored. Text nodes are required to be in the lexical space of base64binary, and are serialized
 * by writing the corresponding octets to the output file. All other serialization parameters are ignored
 */
public class Base64BinaryEmitter extends BinaryEmitter {

    public Base64BinaryEmitter(StreamResult stream, PipelineConfiguration pipe) throws XPathException {
        super(stream, pipe);
    }

    @Override
    protected byte[] textNodeToBinary(UnicodeString chars) throws XPathException {
        return new Base64BinaryValue(Whitespace.normalizeWhitespace(chars)).getBinaryValue();
    }
}

// Copyright (c) 2010-2023 Saxonica Limited


