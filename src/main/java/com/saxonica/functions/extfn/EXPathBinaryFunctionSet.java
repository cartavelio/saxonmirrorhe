package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.om.*;
import net.sf.saxon.resource.BinaryResource;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntIterator;

import java.math.BigInteger;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Implementation of extension functions in the EXPath Binary package, see
 * http://expath.org/spec/binary
 */

public class
EXPathBinaryFunctionSet extends BuiltInFunctionSet {

    private static final EXPathBinaryFunctionSet THE_INSTANCE = new EXPathBinaryFunctionSet();

    public static EXPathBinaryFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private EXPathBinaryFunctionSet() {
        init();
    }


    public static final double VERSION = 1.0;

    public static final String NAMESPACE = "http://expath.org/ns/binary";
    public static final String PREFIX = "bin";
    //public static final String ERROR_NAMESPACE = "http://expath.org/ns/error";
    public static final String ERROR_NAMESPACE = NAMESPACE;
    public static final String ERROR_PREFIX = PREFIX;

    private static final String defaultEndianness = "BE";

    private static final String ERROR_DIFFERENT_LENGTH_ARGUMENTS = "differing-length-arguments";
    private static final String ERROR_INDEX_BEFORE_START = "index-out-of-range";
    private static final String ERROR_INDEX_AFTER_END = "index-out-of-range";
    private static final String ERROR_NEGATIVE_SIZE = "negative-size";
    private static final String ERROR_EMPTY_SEARCH_ITEM = "empty-search-item";
    private static final String ERROR_OCTET_RANGE = "octet-out-of-range";
    private static final String ERROR_NON_NUMERIC_CHAR = "non-numeric-character";
    private static final String ERROR_UNKNOWN_ENCODING = "unknown-encoding";
    private static final String ERROR_CONVERSION = "conversion-error";
    private static final String ERROR_SIGNIFICANCE_ORDER = "unknown-significance-order";

    private static final long QUIET_DOUBLE_NAN = 0x7ff8_0000_0000_0000L;
    private static final int QUIET_SINGLE_NAN = 0x7fc0_0000;


    private void init() {

        register("and", 2, BinaryAnd::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance());

        register("bin", 1, BinaryBin::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EmptySequence.getInstance());

        register("decode-string", 1, BinaryDecodeString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance());

        register("decode-string", 2, BinaryDecodeString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("decode-string", 3, BinaryDecodeString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("decode-string", 4, BinaryDecodeString::new, BuiltInAtomicType.STRING, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(3, BuiltInAtomicType.INTEGER, ONE, null);

        register("encode-string", 1, BinaryEncodeString::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EmptySequence.getInstance());

        register("encode-string", 2, BinaryEncodeString::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("find", 3, BinaryFind::new, BuiltInAtomicType.INTEGER, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.BASE64_BINARY, ONE, null);

        register("from-octets", 1, BinaryFromOctets::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, STAR, null);

        register("hex", 1, BinaryHex::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EmptySequence.getInstance());

        register("insert-before", 3, BinaryInsertBefore::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.BASE64_BINARY, OPT, null);

        register("join", 1, BinaryJoin::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, STAR, null);

        register("length", 1, BinaryLength::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, EmptySequence.getInstance());

        register("not", 1, BinaryNot::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance());

        register("octal", 1, BinaryOctal::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EmptySequence.getInstance());

        register("or", 2, BinaryOr::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance());

        register("pack-double", 1, BinaryPackDouble::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.DOUBLE, ONE, null);

        register("pack-double", 2, BinaryPackDouble::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.DOUBLE, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("pack-float", 1, BinaryPackFloat::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.FLOAT, ONE, null);

        register("pack-float", 2, BinaryPackFloat::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.FLOAT, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("pack-integer", 2, BinaryPackInteger::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("pack-integer", 3, BinaryPackInteger::new, BuiltInAtomicType.BASE64_BINARY, ONE, 0)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null);

        register("pad-left", 2, BinaryPadLeft::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("pad-left", 3, BinaryPadLeft::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("pad-right", 2, BinaryPadRight::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("pad-right", 3, BinaryPadRight::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);


        register("part", 2, BinaryPart::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("part", 3, BinaryPart::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("shift", 2, BinaryShift::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("to-octets", 1, BinaryToOctets::new, BuiltInAtomicType.INTEGER, STAR, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null);

        register("unpack-double", 2, BinaryUnpackDouble::new, BuiltInAtomicType.DOUBLE, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("unpack-double", 3, BinaryUnpackDouble::new, BuiltInAtomicType.DOUBLE, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null);

        register("unpack-float", 2, BinaryUnpackFloat::new, BuiltInAtomicType.FLOAT, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null);

        register("unpack-float", 3, BinaryUnpackFloat::new, BuiltInAtomicType.FLOAT, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null);

        register("unpack-integer", 3, BinaryUnpackInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("unpack-integer", 4, BinaryUnpackInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null);

        register("unpack-unsigned-integer", 3, BinaryUnpackUnsignedInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null);

        register("unpack-unsigned-integer", 4, BinaryUnpackUnsignedInteger::new, BuiltInAtomicType.INTEGER, ONE, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null);

        register("version", 0, BinaryVersion::new, BuiltInAtomicType.DECIMAL, ONE, 0); // not in spec

        register("xor", 2, BinaryXor::new, BuiltInAtomicType.BASE64_BINARY, OPT, 0)
                .arg(0, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance())
                .arg(1, BuiltInAtomicType.BASE64_BINARY, OPT, EmptySequence.getInstance());
    }

    @Override
    public String getNamespace() {
        return NAMESPACE;
    }

    @Override
    public String getConventionalPrefix() {
        return PREFIX;
    }


    public static class BinaryAnd extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value1 = (Base64BinaryValue) arguments[0].head();
            Base64BinaryValue value2 = (Base64BinaryValue) arguments[1].head();
            if (value1 == null || value2 == null) {
                return EmptySequence.getInstance();
            }
            checkSameLength(value1, value2);
            int len = value1.getLengthInOctets();
            byte[] in1 = value1.getBinaryValue();
            byte[] in2 = value2.getBinaryValue();
            byte[] result = new byte[value1.getLengthInOctets()];
            int i = 0;
            while (i < len) {
                result[i] = (byte) (in1[i] & in2[i]);
                i++;
            }
            return one(result);
        }
    }

    public static class BinaryBin extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            StringValue s = (StringValue)arguments[0].head();
            if (s == null) {
                return EmptySequence.getInstance();
            }
            return inputNumber(s.getStringValue(), 2, 1);
        }
    }

    public static class BinaryDecodeString extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            String encoding;
            if (arguments.length < 2) {
                encoding = "UTF-8";
            } else {
                encoding = arguments[1].head().getStringValue();
            }
            //checkEncoding(encoding);
            int offset;
            if (arguments.length < 3) {
                offset = 0;
            } else {
                offset = (int) ((NumericValue) arguments[2].head()).longValue();
            }
            int len;
            if (arguments.length < 4) {
                len = value.getLengthInOctets() - offset;
            } else {
                len = (int) ((NumericValue) arguments[3].head()).longValue();
            }
            checkIndexInclusive(value, offset, len);
//            if (len == 0) {
//                return StringValue.EMPTY_STRING;
//            }
            try {
                String result = BinaryResource.decode(value.getBinaryValue(), offset, len, encoding);
                return new StringValue(result);
            } catch (XPathException e) {
                e.setErrorCodeQName(new StructuredQName("bin", ERROR_NAMESPACE,
                                                        e.getMessage().startsWith("Unsupported encoding")
                                                                ? EXPathBinaryFunctionSet.ERROR_UNKNOWN_ENCODING
                                                                : EXPathBinaryFunctionSet.ERROR_CONVERSION));
                throw e;
            }
        }


    }

    public static class BinaryEncodeString extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            StringValue value = (StringValue)arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            String s = value.head().getStringValue();
            String encoding;
            if (arguments.length < 2) {
                encoding = "UTF-8";
            } else {
                encoding = arguments[1].head().getStringValue();
            }
            //checkEncoding(encoding);
            byte[] data;
            try {
                data = BinaryResource.encode(s, encoding);
            } catch (XPathException e) {
                e.setErrorCodeQName(new StructuredQName("bin", ERROR_NAMESPACE,
                                                        e.getMessage().startsWith("Unsupported encoding")
                                                            ? EXPathBinaryFunctionSet.ERROR_UNKNOWN_ENCODING
                                                            : EXPathBinaryFunctionSet.ERROR_CONVERSION));
                throw e;
            }
            return one(data);
        }


    }


    public static class BinaryFind extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            IntegerValue offsetValue = (IntegerValue) arguments[1].head();
            int off = (int)offsetValue.longValue();
            Base64BinaryValue search = (Base64BinaryValue) arguments[2].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            byte[] in = value.getBinaryValue();
            // This lets empty match under any circumstances - swap these two following to change
            if (search.getLengthInOctets() == 0) {
                //error("Binary search item is empty", ERROR_EMPTY_SEARCH_ITEM);
                return offsetValue;
            }
            checkIndex(value, off, 0);
            byte[] s = search.getBinaryValue();
            int i = find(in, s, off);
            if (i == -1) {
                return EmptySequence.getInstance();
            }
            return new Int64Value(i);
        }
    }

    public static class BinaryFromOctets extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            GroundedValue gv = arguments[0].materialize();
            byte[] bytes = new byte[gv.getLength()];
            SequenceIterator iter = gv.iterate();
            Item item;
            int i=0;
            while ((item = iter.next()) != null) {
                long v = ((IntegerValue)item).longValue();
                if (0 <= v && v <= 255) {
                    bytes[i++] = (byte) v;
                } else {
                    error("Integer outside octet range in binary constructor:" + v,
                                         ERROR_OCTET_RANGE);
                }
            }
            return one(bytes);
        }
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws XPathException always
     */

    public static void error(String message, String code) throws XPathException {
        XPathException e = new XPathException(message);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        throw e;
    }

    public static class BinaryHex extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            StringValue s = (StringValue) arguments[0].head();
            if (s == null) {
                return EmptySequence.getInstance();
            }
            return inputNumber(s.getStringValue(), 16, 4);
        }
    }

    public static class BinaryInsertBefore extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            int valLen = value.getLengthInOctets();
            IntegerValue offsetValue = (IntegerValue) arguments[1].head();
            int off = (int) offsetValue.longValue();
            Base64BinaryValue extra = (Base64BinaryValue) arguments[2].head();

            // This lets empty be inserted under any circumstances
            //int off = Math.max(0,Math.min(off,valLen));
            if (extra == null) {
                return value;
            }
            checkIndexInclusive(value, off, 0);

            int extraLen = extra.getLengthInOctets();

            byte[] result = new byte[valLen + extraLen];
            System.arraycopy(value.getBinaryValue(), 0, result, 0, off);
            System.arraycopy(extra.getBinaryValue(), 0, result, off, extraLen);
            System.arraycopy(value.getBinaryValue(), off, result, off + extraLen, valLen - off);
            return one(result);
        }
    }

    public static class BinaryJoin extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            int len = 0;
            GroundedValue input = arguments[0].materialize();
            SequenceIterator iter = input.iterate();
            Base64BinaryValue bv;
            while ((bv = (Base64BinaryValue) iter.next()) != null) {
                len += bv.getLengthInOctets();
            }
            byte[] result = new byte[len];

            int i = 0;
            iter = input.iterate();
            while ((bv = (Base64BinaryValue) iter.next()) != null) {
                int lb = bv.getLengthInOctets();
                System.arraycopy(bv.getBinaryValue(), 0, result, i, lb);
                i += lb;
            }
            return one(result);
        }
    }

    public static class BinaryLength extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            return new Int64Value(value.getLengthInOctets());
        }
    }

    public static class BinaryNot extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            int len = value.getLengthInOctets();
            byte[] in = value.getBinaryValue();
            byte[] result = new byte[len];
            int i = 0;
            while (i < len) {
                result[i] = (byte) ~in[i++];
            }
            return one(result);
        }
    }

    public static class BinaryOctal extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            StringValue s = (StringValue) arguments[0].head();
            if (s == null) {
                return EmptySequence.getInstance();
            }
            StringBuilder binary = new StringBuilder();
            IntIterator iter = s.getUnicodeStringValue().codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (c < '0' || c > '7')
                    error("Wrong character in base 8 binary constructor string:"
                                  + c, ERROR_NON_NUMERIC_CHAR);
                binary.append((c & 4) == 0 ? "0" : "1");
                binary.append((c & 2) == 0 ? "0" : "1");
                binary.append((c & 1) == 0 ? "0" : "1");
            }
            // System.err.println("binary:" + binary);
            return inputNumber(binary.toString(), 2, 1);
        }
    }

    public static class BinaryOr extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value1 = (Base64BinaryValue) arguments[0].head();
            Base64BinaryValue value2 = (Base64BinaryValue) arguments[1].head();
            if (value1 == null || value2 == null) {
                return EmptySequence.getInstance();
            }
            checkSameLength(value1, value2);
            int len = value1.getLengthInOctets();
            byte[] in1 = value1.getBinaryValue();
            byte[] in2 = value2.getBinaryValue();
            byte[] result = new byte[value1.getLengthInOctets()];
            int i = 0;
            while (i < len) {
                result[i] = (byte) (in1[i] | in2[i]);
                i++;
            }
            return one(result);
        }
    }

    public static class BinaryPackDouble extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            DoubleValue value = (DoubleValue)arguments[0].head();
            long bits;
            if (value.isNaN()) {
                // Normalise NaN representation
                bits = QUIET_DOUBLE_NAN;
            } else {
                bits = Double.doubleToRawLongBits(value.getDoubleValue());
            }
            return pack(bits, 8, endianness(arguments, 1));
        }
    }

    public static class BinaryPackFloat extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            FloatValue value = (FloatValue) arguments[0].head();
            int bits;
            if (value.isNaN()) {
                // Normalise NaN representation
                bits = QUIET_SINGLE_NAN;
            } else {
                bits = Float.floatToRawIntBits(value.getFloatValue());
            }
            return pack(bits, 4, endianness(arguments, 1));
        }
    }


    public static class BinaryPackInteger extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            IntegerValue value = (IntegerValue) arguments[0].head();
            int requestedLength = (int) ((IntegerValue) arguments[1].head()).longValue();
            if (requestedLength < 0) {
                error("Requested length of integer packing is negative", ERROR_NEGATIVE_SIZE);
            }
            BigInteger big = value.asBigInteger();
            byte[] in = big.toByteArray();
            byte[] result = new byte[requestedLength];
            if (big.signum() == -1) {
                // Negative number: unused bytes set to 0xFF
                Arrays.fill(result, (byte) 255);
            }
            int suppliedLength = in.length;
            if (endianness(arguments, 2)) {
                // big-endian (most significant byte first)
                int bytesToCopy = Math.min(suppliedLength, requestedLength);
                System.arraycopy(in, suppliedLength - bytesToCopy,
                                 result, requestedLength - bytesToCopy,
                                 bytesToCopy);
            } else {
                // little-endian (least significant byte first)
                int i = 0;
                while (i < requestedLength && suppliedLength > 0) {
                    result[i++] = in[--suppliedLength];
                }
            }
            return one(result);
        }
    }
    public static class BinaryPadLeft extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            int size = (int) ((NumericValue) arguments[1].head()).longValue();
            int oct = 0;
            if (arguments.length > 2) {
                oct = (int) ((NumericValue) arguments[2].head()).longValue();
            }
            if (size < 0) {
                error("Pad size is negative", ERROR_NEGATIVE_SIZE);
                return null;
            } else if (oct < 0 || 255 < oct) {
                error("Integer outside octet range in padding:" + oct,
                                     ERROR_OCTET_RANGE);
                return null;
            } else if (size == 0) {
                return value;
            } else {
                int len = value.getLengthInOctets();
                byte[] in = value.getBinaryValue();
                byte[] result = new byte[len + size];
                if (oct != 0) {
                    Arrays.fill(result, 0, size, (byte) oct);
                }
                System.arraycopy(in, 0, result, size, len);
                return one(result);
            }
        }
    }

    public static class BinaryPadRight extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            int size = (int) ((NumericValue) arguments[1].head()).longValue();
            int oct = 0;
            if (arguments.length > 2) {
                oct = (int) ((NumericValue) arguments[2].head()).longValue();
            }
            if (size < 0) {
                error("Pad size is negative", ERROR_NEGATIVE_SIZE);
                return null;
            } else if (oct < 0 || 255 < oct) {
                error("Integer outside octet range in padding:" + oct, ERROR_OCTET_RANGE);
                return null;
            } else if (size == 0) {
                return value;
            } else {
                int len = value.getLengthInOctets();
                byte[] in = value.getBinaryValue();
                byte[] result = new byte[len + size];
                System.arraycopy(in, 0, result, 0, len);
                if (oct != 0) {
                    Arrays.fill(result, len, len + size, (byte) oct);
                }
                return one(result);
            }
        }
    }

    public static class BinaryPart extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null) {
                return EmptySequence.getInstance();
            }
            int start = (int) ((NumericValue) arguments[1].head()).longValue();
            int len;
            if (arguments.length == 2) {
                len = Math.max(0, value.getLengthInOctets() - start);
            } else {
                len = (int) ((NumericValue) arguments[2].head()).longValue();
            }
            checkIndexInclusive(value, start, len);
            if (start == 0 && len == value.getLengthInOctets()) {
                return value;
            } else {
                byte[] result = new byte[len];
                System.arraycopy(value.getBinaryValue(), start, result, 0, len);
                return one(result);
            }
        }
    }

    public static class BinaryShift extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            if (value == null)
                return EmptySequence.getInstance();
            int bits = (int) ((NumericValue)arguments[1].head()).longValue();
            if (bits == 0)
                return value;
            else {
                int len = value.getLengthInOctets();
                byte[] in = value.getBinaryValue();
                byte[] result = new byte[len];
                int shift = Math.abs(bits);
                int byteShift = shift / 8;
                int bitShift = shift % 8;
                int i = 0;
                int offset = 0;
                byte part = 0;
                int partShift;
                if (bits > 0) {
                    offset = byteShift;
                    partShift = 8 - bitShift;
                    if (bitShift == 0) {
                        while (offset < len) {
                            // System.err.println("offset:"+offset);
                            result[i++] = in[offset++];
                        }
                    } else
                        while (offset < len) {
                            part = (byte) ((offset == len - 1) ? 0
                                                   : ((in[offset + 1]) >>> partShift));
                            /*
                             * System.err .println("next:" + ((offset == len - 1) ?
                             * 0 : ((in[offset + 1])))); System.err.println("part:"
                             * + part);
                             */
                            result[i] = (byte) ((in[offset] << bitShift) | part);
                            i++;
                            offset++;
                        }
                } else {
                    i = byteShift;
                    partShift = 8 - bitShift;
                    while (i < len) {
                        /*
                         * System.err.println("offset:" + offset);
                         * System.err.println("part:" + part);
                         * System.err.println("in:" + (in[offset] & 0xff));
                         * System.err.println("shifted:" + ((in[offset] & 0xff) >>>
                         * bitShift));
                         */
                        result[i] = (byte) (((in[offset] & 0xff) >>> bitShift) | part);
                        part = (byte) ((in[offset] & 0xff) << partShift);
                        i++;
                        offset++;
                    }
                }
                return one(result);
            }
        }
    }

    public static class BinaryToOctets extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            List<IntegerValue> result = new ArrayList<IntegerValue>();
            for (byte b : value.getBinaryValue()) {
                result.add(new Int64Value(b & 0xFF, BuiltInAtomicType.UNSIGNED_BYTE));
            }
            return SequenceExtent.makeSequenceExtent(result);
        }
    }

    public static class BinaryUnpackDouble extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            int offset = (int) ((NumericValue) arguments[1].head()).longValue();
            int len = 8;
            checkIndex(value, offset, len);
            double d = toDouble(value.getBinaryValue(), offset, len, endianness(arguments, 2));
            return Double.isNaN(d) ? DoubleValue.NaN : new DoubleValue(d);

        }
        @CSharpReplaceBody(code="return Saxon.Impl.Helpers.Utils.bytesToDouble(bytes, offset, bigEndian);")
        private static double toDouble(byte[] bytes, int offset, int len, boolean bigEndian) {
            long bits = unpack(bytes, offset, len, bigEndian);
            return Double.longBitsToDouble(bits);
        }
    }

    public static class BinaryUnpackFloat extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            int offset = (int)((NumericValue)arguments[1].head()).longValue();
            int len = 4;
            checkIndex(value, offset, len);
            float f = toFloat(value.getBinaryValue(), offset, len, endianness(arguments, 2));
            return Float.isNaN(f) ? FloatValue.NaN : new FloatValue(f);
        }

        @CSharpReplaceBody(code = "return Saxon.Impl.Helpers.Utils.bytesToFloat(bytes, offset, bigEndian);")
        private float toFloat(byte[] bytes, int offset, int len, boolean bigEndian) {
            int bits = (int)unpack(bytes, offset, len, bigEndian);
            return Float.intBitsToFloat(bits);
        }
    }

    public static class BinaryUnpackInteger extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            int offset = (int) ((NumericValue) arguments[1].head()).longValue();
            int len = (int) ((NumericValue) arguments[2].head()).longValue();
            checkIndex(value, offset, len);
            byte[] in = value.getBinaryValue();
            boolean negative = (in[offset] & 0x80) != 0;
            boolean bigEnd = endianness(arguments, 3);
            if (len == 0) {
                return Int64Value.ZERO;
            }
            if (len > 7) {
                BigInteger big = bigIntegerFromBytes(offset, len, in, bigEnd);
                return new BigIntegerValue(big);
            } else {
                long signExt = negative ? -(1L << (8 * len)) : 0;
                long result = unpack(in, offset, len, bigEnd);
                return new Int64Value(result | signExt);
            }
        }

        @CSharpReplaceBody(code = "return Saxon.Impl.Helpers.BigIntegerUtils.FromBytes(@in, offset, len, false, bigEnd);")
        private BigInteger bigIntegerFromBytes(int offset, int len, byte[] in, boolean bigEnd) {
            return new BigInteger(unpackBytes(in, offset, len, bigEnd, false));
        }
    }

    public static class BinaryUnpackUnsignedInteger extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value = (Base64BinaryValue) arguments[0].head();
            int offset = (int) ((NumericValue) arguments[1].head()).longValue();
            int len = (int) ((NumericValue) arguments[2].head()).longValue();
            checkIndex(value, offset, len);
            byte[] in = value.getBinaryValue();
            boolean bigEnd = endianness(arguments, 3);
            checkIndex(value, offset, len);
            if (len == 0) {
                return Int64Value.ZERO;
            }
            if (len > 7) {
                BigInteger big = bigIntegerFromBytes(in, offset, len, bigEnd);
                return new BigIntegerValue(big);
            } else {
                long result = unpack(in, offset, len, bigEnd);
                return new Int64Value(result);
            }
        }

        @CSharpReplaceBody(code="return Saxon.Impl.Helpers.BigIntegerUtils.FromBytes(@in, offset, len, true, bigEnd);")
        private static BigInteger bigIntegerFromBytes(byte[] in, int offset, int len, boolean bigEnd) {
            return new BigInteger(unpackBytes(in, offset, len, bigEnd, true));
        }
    }

    public static class BinaryVersion extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new BigDecimalValue(VERSION);
        }
    }

    public static class BinaryXor extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Base64BinaryValue value1 = (Base64BinaryValue) arguments[0].head();
            Base64BinaryValue value2 = (Base64BinaryValue) arguments[1].head();
            if (value1 == null || value2 == null) {
                return EmptySequence.getInstance();
            }
            checkSameLength(value1, value2);
            int len = value1.getLengthInOctets();
            byte[] in1 = value1.getBinaryValue();
            byte[] in2 = value2.getBinaryValue();
            byte[] result = new byte[value1.getLengthInOctets()];
            int i = 0;
            while (i < len) {
                result[i] = (byte) (in1[i] ^ in2[i]);
                i++;
            }
            return one(result);
        }
    }


    /**
     * Generate an xs:base64Binary result
     *
     * @param result - byte sequence
     * @return suitably encapsulated Base64BinaryValue
     */
    private static Base64BinaryValue one(byte[] result) {
        return new Base64BinaryValue(result);
    }

    private static void checkIndex(Base64BinaryValue value, int offset,
                                   int length) throws XPathException {
        if (offset < 0) {
            error("Attempting to retrieve data before the start of a binary data type",
                                 ERROR_INDEX_BEFORE_START);
        }
        int len = value.getLengthInOctets();
        if (length < 0) {
            error("Requested length of binary section is negative", ERROR_NEGATIVE_SIZE);
        }
        if (offset >= len) {
            error("Attempting to retrieve data beyond the end of a binary data type; index:"
                    + offset + " data:" + len, ERROR_INDEX_AFTER_END);
        }
        if (offset + length > len) {
            error("Attempting to retrieve data beyond the end of a binary data type; index:"
                    + (offset + length) + " data:" + len, ERROR_INDEX_AFTER_END);
        }
    }

    // This check permits access right at the end of a binary sequence - for things like part() and insert-before()
    private static void checkIndexInclusive(Base64BinaryValue value, int offset,
                                            int length) throws XPathException {
        if (offset < 0) {
            error("Attempting to retrieve data before the start of a binary data type",
                                 ERROR_INDEX_BEFORE_START);
        }
        int len = value.getLengthInOctets();
        if (length < 0) {
            error("Requested length of binary section is negative", ERROR_NEGATIVE_SIZE);
        }
        if (offset > len) {
            error("Attempting to retrieve data beyond the end of a binary data type; index:"
                    + offset + " data:" + len, ERROR_INDEX_AFTER_END);
        }
        if (offset + length > len) {
            error("Attempting to retrieve data beyond the end of a binary data type; index:"
                    + (offset + length) + " data:" + len, ERROR_INDEX_AFTER_END);
        }
    }

    /**
     * Check that two binary values are the same length (in octets)
     *
     * @param value1 the first value
     * @param value2 the second value
     * @throws XPathException If both are xs:base64Binary and differ in length
     */
    private static void checkSameLength(Base64BinaryValue value1,
                                        Base64BinaryValue value2) throws XPathException {
        if (value1.getLengthInOctets() != value2.getLengthInOctets()) {
            error("The arguments to a bitwise operation are differing lengths",
                                 ERROR_DIFFERENT_LENGTH_ARGUMENTS);
        }
    }

    

    /**
     * Determine what the endianess is
     *
     * @param args supplied arguments
     * @param position position of the relevant argument
     * @return true for 'most-significant-first', false for 'least-significant-first'
     * @throws XPathException if the supplied value is invalid
     */

    private static boolean endianness(Sequence[] args, int position) throws XPathException {
        String s;
        if (args.length <= position) {
            s = defaultEndianness;
        } else {
            s = args[position].head().getStringValue();
        }
        if (s.equalsIgnoreCase("BE")
                || s.equalsIgnoreCase("most-significant-first")
                || s.equalsIgnoreCase("big-endian"))
            return true;
        if (s.equalsIgnoreCase("LE")
                || s.equalsIgnoreCase("least-significant-first")
                || s.equalsIgnoreCase("little-endian"))
            return false;
        error("Unrecognized octet-order request:" + s, ERROR_SIGNIFICANCE_ORDER);
        return false;
    }

    private static Base64BinaryValue inputNumber(String s, int base,
                                                      int bitLength) throws XPathException {
        // Make these multiples of 8 bits
        int bits = s.length() * bitLength;
        int charsPerOctet = 8 / bitLength;
        int octetLength = (bits + 7) / 8;
        int padLength = bits % 8;
        byte[] result = new byte[octetLength];
        int offset = 0;
        int end = padLength == 0 ? charsPerOctet : (padLength / bitLength);
        int b = 0;
        try {
            while (offset < s.length()) {
                result[b++] = (byte) (Integer.parseInt(
                        s.substring(offset, end), base) & 0xff);
                offset = end;
                end = offset + charsPerOctet;
            }
        } catch (NumberFormatException e) {
            error("Wrong character in base " + base
                    + " binary constructor string:" + s, ERROR_NON_NUMERIC_CHAR);
        }
        return one(result);
    }
    


    private static int find(byte[] data, byte[] pattern, int offset) {
        int[] failure = computeFailure(pattern);
        int j = 0;
        if (data.length == 0)
            return -1;
        for (int i = offset; i < data.length; i++) {
            while (j > 0 && pattern[j] != data[i]) {
                j = failure[j - 1];
            }
            if (pattern[j] == data[i]) {
                j++;
            }
            if (j == pattern.length) {
                return i - pattern.length + 1;
            }
        }
        return -1;
    }

    /**
     * Computes the failure function using a boot-strapping process, where the
     * pattern is matched against itself.
     */
    private static int[] computeFailure(byte[] pattern) {
        int[] failure = new int[pattern.length];
        int j = 0;
        for (int i = 1; i < pattern.length; i++) {
            while (j > 0 && pattern[j] != pattern[i]) {
                j = failure[j - 1];
            }
            if (pattern[j] == pattern[i]) {
                j++;
            }
            failure[i] = j;
        }
        return failure;
    }

	/* String encoding and decoding */

    private static void checkEncoding(String encoding) throws XPathException {
        String se = encoding.toUpperCase();
        if (!se.equals("UTF-8") && !se.equals("UTF-16")
                && !se.equals("US-ASCII") && !se.equals("ASCII") && !se.equals("ISO-8859-1")) {
            error("Unsupported encoding in binary encode/decode:" + encoding,
                                 ERROR_UNKNOWN_ENCODING);
        }
    }


	/* Numeric */

    private static Base64BinaryValue pack(long in, int length, boolean big) {
        byte[] result = new byte[length];
        if (big) {
            while (length > 0) {
                result[--length] = (byte) (in & 0xff);
                in >>= 8;
            }
        } else {
            int i = 0;
            while (length-- > 0) {
                result[i++] = (byte) (in & 0xff);
                in >>= 8;
            }
        }
        return one(result);
    }

    private static long unpack(byte[] in, int offset, int length, boolean big) {
        // Something about sign extension
        long result = 0;
        if (big) {
            while (length-- > 0)
                result = (result << 8) + (in[offset++] & 0xFF);
        } else {
            int off = offset + length;
            while (length-- > 0)
                result = (result << 8) + (in[--off] & 0xFF);
        }
        return result;
    }

    private static byte[] unpackBytes(byte[] in, int offset, int length,
                                      boolean big, boolean unsigned) {
        // In unsigned situations add an extra zero byte at the start
        byte[] out = new byte[length + (unsigned ? 1 : 0)];
        int i = unsigned ? 1 : 0;
        if (big) {
            while (length-- > 0)
                out[i++] = in[offset++];
        } else {
            int off = offset + length;
            while (length-- > 0)
                out[i++] = in[--off];
        }
        return out;
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
