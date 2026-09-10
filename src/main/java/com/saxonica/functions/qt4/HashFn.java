////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.transpile.CSharpReplaceException;
import net.sf.saxon.value.*;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Map;
import java.util.zip.CRC32;

/**
 * This class implements the function fn:hash(), introduced in XPath 4.0
 */

public class HashFn extends SystemFunction {

    public static OptionsParameter getOptionDetails() {
        OptionsParameter details = new OptionsParameter(40);
        //details.addAllowedOption("algorithm", SequenceType.SINGLE_STRING, StringValue.bmp("MD5"));
        return details;
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    @CSharpReplaceException(from="java.security.NoSuchAlgorithmException", to="Saxon.Hej.trans.UncheckedXPathException")
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        try {
            AtomicValue input = (AtomicValue)arguments[0].head();
            if (input == null) {
                return EmptySequence.getInstance();
            }
            byte[] binaryInput;
            if (input instanceof StringValue) {
                binaryInput = input.getStringValue().getBytes(StandardCharsets.UTF_8);
            } else {
                binaryInput = ((BinaryValue)input).getBinaryValue();
            }
            String algorithm = "MD5";
            if (arguments.length > 1) {
                StringValue algorithmArg = (StringValue)arguments[1].head();
                if (algorithmArg != null) {
                    algorithm = algorithmArg.getStringValue();
                }
            }
            MapItem options = null;
            if (arguments.length > 2) {
                options = (MapItem)arguments[2].head();
            }
            if (options == null) {
                options = new DictionaryMap();
            }
            Map<String, GroundedValue> checkedOptions =
                    getDetails().optionDetails.processSuppliedOptions(options, context);
            //String algorithm = checkedOptions.get("algorithm").getStringValue().trim().toUpperCase();
            if (algorithm.equals("CRC-32")) {
                return crc32(binaryInput);
            }
            byte[] hash = MessageDigest.getInstance(algorithm).digest(binaryInput);
            return new HexBinaryValue(hash);
        } catch (NoSuchAlgorithmException e) {
            throw new XPathException(e.getMessage(), "FOHA0001");
        }
    }

    @CSharpReplaceBody(code="return new Saxon.Hej.value.HexBinaryValue(reverseBytes(System.IO.Hashing.Crc32.Hash(binaryInput)));")
    private static HexBinaryValue crc32(byte[] binaryInput) {
        CRC32 hasher = new CRC32();
        hasher.update(binaryInput);
        long hash = hasher.getValue();
        byte[] result = new byte[4];
        result[0] = (byte)((hash >> 24) & 0xff);
        result[1] = (byte) ((hash >> 16) & 0xff);
        result[2] = (byte) ((hash >> 8) & 0xff);
        result[3] = (byte) (hash & 0xff);
        return new HexBinaryValue(result);
    }

    private static byte[] reverseBytes(byte[] input) { // Used in the C# implementation
        byte[] result = new byte[4];
        result[0] = input[3];
        result[1] = input[2];
        result[2] = input[1];
        result[3] = input[0];
        return result;
    }

}
