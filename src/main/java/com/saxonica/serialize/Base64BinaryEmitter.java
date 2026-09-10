package com.saxonica.serialize;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.Whitespace;

/**
 * This emitter implements output method saxon:base64Binary. All nodes in the result tree other than text
 * nodes are ignored. Text nodes are required to be in the lexical space of base64binary, and are serialized
 * by writing the corresponding octets to the output file. All other serialization parameters are ignored
 */
public class Base64BinaryEmitter extends BinaryEmitter {

    protected byte[] textNodeToBinary(CharSequence chars) throws XPathException {
        return new Base64BinaryValue(Whitespace.normalizeWhitespace(chars)).getBinaryValue();
    }
}

// Copyright (c) 2010 Saxonica Limited.


