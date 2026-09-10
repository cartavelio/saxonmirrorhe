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
import net.sf.saxon.transpile.CSharpReplaceException;
import net.sf.saxon.value.*;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Arrays;
import java.util.Map;
import java.util.zip.CRC32;

/**
 * This class implements the function fn:sort-with#2, introduced in XPath 4.0
 */

public class HashFn extends SystemFunction {

    public static OptionsParameter getOptionDetails() {
        OptionsParameter details = new OptionsParameter(40);
        details.addAllowedOption("algorithm", SequenceType.SINGLE_STRING, StringValue.bmp("MD5"));
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
            MapItem options = null;
            if (arguments.length > 1) {
                options = (MapItem)arguments[1].head();
            }
            if (options == null) {
                options = new DictionaryMap();
            }
            Map<String, GroundedValue> checkedOptions =
                    getDetails().optionDetails.processSuppliedOptions(options, context);
            String algorithm = checkedOptions.get("algorithm").getStringValue().trim().toUpperCase();
            if (algorithm.equals("CRC-32")) {
                CRC32 hasher = new CRC32();
                hasher.update(binaryInput);
                long hash = hasher.getValue();
                return new HexBinaryValue(Arrays.copyOfRange(longToBytes(hash), 4, 8));
            }
            byte[] hash = MessageDigest.getInstance(algorithm).digest(binaryInput);
            return new HexBinaryValue(hash);
        } catch (NoSuchAlgorithmException e) {
            throw new XPathException(e.getMessage(), "FOHA0001");
        }
    }

    public static byte[] longToBytes(long l) {
        // https://stackoverflow.com/a/29132118/415448
        byte[] result = new byte[Long.BYTES];
        for (int i = Long.BYTES - 1; i >= 0; i--) {
            result[i] = (byte) (l & 0xFF);
            l >>= Byte.SIZE;
        }
        return result;
    }

}
