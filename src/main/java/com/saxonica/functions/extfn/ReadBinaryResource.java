package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.resource.Loader;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.Base64BinaryValue;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Implement saxon:read-binary-resource extension function
 */
public class ReadBinaryResource extends SystemFunction {
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        try {
            String arg = arguments[0].head().getStringValue();
            InputStream stream;
            try {
                URL url = new URL(arg);
                stream = Loader.urlStream(url);
            } catch (RuntimeException e) {
                throw new XPathException("saxon:read-binary-resource - invalid or inaccessible URL " + arg, SaxonErrorCode.SXJE0053);
            }
            List<byte[]> buffers = new ArrayList<byte[]>();
            int len = 0;
            while (true) {
                byte[] buffer = new byte[4096];
                int b = stream.read(buffer);
                if (isFinished(b)) {
                    break;
                }
                if (b < 4096) {
                    buffer = Arrays.copyOf(buffer, b);
                }
                buffers.add(buffer);
                len += b;
            }
            byte[] result = new byte[len];
            int offset = 0;
            for (byte[] buffer : buffers) {
                System.arraycopy(buffer, 0, result, offset, buffer.length);
                offset += buffer.length;
            }
            return new Base64BinaryValue(result);
        } catch (IOException e) {
            throw new XPathException("saxon:read-binary-resource: I/O error: " + e.getMessage(), SaxonErrorCode.SXJE0053);
        }
    }

    @CSharpReplaceBody(code="return indicator == 0;")
    private boolean isFinished(int indicator) {
        return indicator < 0;
    }
}

// Copyright (c) 2020-2022 Saxonica Limited