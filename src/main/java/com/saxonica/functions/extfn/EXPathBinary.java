package com.saxonica.functions.extfn;

import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;
import net.sf.saxon.value.StringValue;

import java.io.UnsupportedEncodingException;
import java.math.BigInteger;
import java.nio.ByteBuffer;
import java.nio.CharBuffer;
import java.nio.charset.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Implementation of extension functions in the EXPath Binary package, see
 * http://expath.org/spec/binary
 */

public class EXPathBinary {
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

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws XPathException always
     */

    private static void error(String message, String code)
            throws XPathException {
        XPathException e = new XPathException(message);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        throw e;
    }

    /**
     * Generate an xs:base64Binary result
     *
     * @param result - byte sequence
     * @return suitably encapsulated Base64BinaryValue
     */
    private static One<Base64BinaryValue> one(byte[] result) {
        return new One<Base64BinaryValue>(new Base64BinaryValue(result));
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
     * Check that two binary values are the same 'length'
     *
     * @param value1
     * @param value2
     * @throws XPathException If both are xs:base64Binary and differ in length
     */
    private static void checkSameLength(ZeroOrOne<Base64BinaryValue> value1,
                                        ZeroOrOne<Base64BinaryValue> value2) throws XPathException {
        if (value1.head() == null || value2.head() == null)       // changes for 2013-11-13 spec - any empty - empty result
            return;
        if (value1.head().getLengthInOctets() != value2.head().getLengthInOctets()) {
            error("The arguments to a bitwise operation are differing lengths",
                    ERROR_DIFFERENT_LENGTH_ARGUMENTS);
        }
    }

    /**
     * Determine what the endianess is
     *
     * @param value string value to test
     * @return true for 'most-significant-first', false for 'least-significant-first'
     * @throws XPathException
     */
    private static boolean endianness(One<StringValue> value) throws XPathException {
        String s = value.getStringValue();
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

    private static One<Base64BinaryValue> inputNumber(String s, int base,
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

    // bin:version()
    // as xs:decimal
    public static One<BigDecimalValue> version() {
        try {
            return new One(new BigDecimalValue(VERSION));
        } catch (ValidationException e) {
            e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
        }
        return null;
    }


    // bin:hex($s as xs:string?)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> hex(ZeroOrOne<StringValue> s) throws XPathException {
        if (s.head() == null)
            return ZeroOrOne.empty();
        return inputNumber(s.getStringValue(), 16, 4);
    }

    // bin:bin($s as xs:string?)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> bin(ZeroOrOne<StringValue> s) throws XPathException {
        if (s.head() == null)
            return ZeroOrOne.empty();
        return inputNumber(s.getStringValue(), 2, 1);
    }

    // bin:octal($s as xs:string?)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> octal(ZeroOrOne<StringValue> s) throws XPathException {
        if (s.head() == null)
            return ZeroOrOne.empty();
        String binary = "";
        for (char c : s.getStringValue().toCharArray()) {
            if (c < '0' || c > '7')
                error("Wrong character in base 8 binary constructor string:"
                        + c, ERROR_NON_NUMERIC_CHAR);
            binary += (c & 4) == 0 ? "0" : "1";
            binary += (c & 2) == 0 ? "0" : "1";
            binary += (c & 1) == 0 ? "0" : "1";
        }
        // System.err.println("binary:" + binary);
        return inputNumber(binary, 2, 1);
    }

    // bin:length($contents as xs:base64Binary)
    // as xs:integer
    public static One<IntegerValue> length(One<Base64BinaryValue> value) {
        return One.integer(value.head().getLengthInOctets());
    }

    // bin:to-octets($contents as xs:base64Binary)
    // as xs:integer*
    public static ZeroOrMore<IntegerValue> toOctets(One<Base64BinaryValue> value) {
        List<IntegerValue> result = new ArrayList<IntegerValue>();
        for (byte b : value.head().getBinaryValue()) {
            result.add(new Int64Value(b & 0xFF));
        }
        return new ZeroOrMore(result);
    }

    // bin:from-octets($contents as xs:integer*)
    // as xs:base64Binary
    public static One<Base64BinaryValue> fromOctets(ZeroOrMore<IntegerValue> value)
            throws XPathException {
        if (value.head() == null) {
            return one(new byte[0]);
        }
        ArrayList<Byte> ba = new ArrayList<>();
        for (IntegerValue iv : value) {
            long v = iv.longValue();
            if (0 <= v && v <= 255) {
                ba.add((byte) v);
            } else {
                error("Integer outside octet range in binary constructor:" + v,
                        ERROR_OCTET_RANGE);
            }
        }
        byte[] br = new byte[ba.size()];
        int i = 0;
        for (Byte b : ba) {
            br[i++] = b;
        }
        return one(br);
    }

    // bin:join($contents as xs:base64Binary*)
    // as xs:base64Binary
    public static One<Base64BinaryValue> join(ZeroOrMore<Base64BinaryValue> value) throws XPathException {
        int len = 0;
        SequenceIterator si = value.iterate();
        Base64BinaryValue bv;
        while ((bv = (Base64BinaryValue) si.next()) != null) {
            len += bv.getLengthInOctets();
        }
        byte[] result = new byte[len];

        int i = 0;
        si = value.iterate();
        while ((bv = (Base64BinaryValue) si.next()) != null) {
            int lb = bv.getLengthInOctets();
            System.arraycopy(bv.getBinaryValue(), 0, result, i, lb);
            i += lb;
        }
        return one(result);
    }

    // bin:insert-before($contents as xs:base64Binary?, $offset as
    // xs:integer,$extra as xs:base64Binary? )
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> insertBefore(ZeroOrOne<Base64BinaryValue> value,
                                                            One<IntegerValue> offset, ZeroOrOne<Base64BinaryValue> extra) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int valLen = value.head().getLengthInOctets();
        int off = (int) offset.head().longValue();
        // This lets empty be inserted under any circumstances
        //int off = Math.max(0,Math.min(off,valLen));
        if (extra.head() == null)
            return value;
        checkIndexInclusive(value.head(), off, 0);

        int extraLen = extra.head().getLengthInOctets();

        byte[] result = new byte[valLen + extraLen];
        System.arraycopy(value.head().getBinaryValue(), 0, result, 0, off);
        System.arraycopy(extra.head().getBinaryValue(), 0, result, off, extraLen);
        System.arraycopy(value.head().getBinaryValue(), off, result, off + extraLen,
                valLen - off);
        return one(result);
    }

    // bin:part($contents as xs:base64Binary?, $offset as xs:integer)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> part(ZeroOrOne<Base64BinaryValue> value,
                                                    One<IntegerValue> offset) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int len = Math.max(0, (int) (value.head().getLengthInOctets() - offset.head().longValue()));
        return part(value, offset, One.integer(len));
    }

    // bin:part($contents as xs:base64Binary?, $offset as xs:integer,
    // $length as xs:integer)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> part(ZeroOrOne<Base64BinaryValue> value,
                                                    One<IntegerValue> offset, One<IntegerValue> length) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int off = (int) offset.head().longValue();
        int len = (int) length.head().longValue();
        checkIndexInclusive(value.head(), off, len);
        if (off == 0 && len == value.head().getLengthInOctets()) {
            return value;
        } else {
            byte[] result = new byte[len];
            System.arraycopy(value.head().getBinaryValue(), off, result, 0, len);
            return one(result);
        }
    }

    // bin:find($contents as xs:base64Binary?, $search as xs:base64Binary)
    // as xs:integer?
    public static ZeroOrOne<IntegerValue> find(ZeroOrOne<Base64BinaryValue> value,
                                               One<Base64BinaryValue> search) throws XPathException {
        return find(value, One.integer(0), search);
    }

    // bin:find($contents as xs:base64Binary?, $offset as xs:integer,
    // $search as xs:base64Binary)
    // as xs:integer?
    public static ZeroOrOne<IntegerValue> find(ZeroOrOne<Base64BinaryValue> value,
                                               One<IntegerValue> offset, One<Base64BinaryValue> search)
            throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        byte[] in = value.head().getBinaryValue();
        int off = (int) offset.head().longValue();
        // This lets empty match under any circumstances - swap these two following to change
        if (search.head().getLengthInOctets() == 0) {
            //error("Binary search item is empty", ERROR_EMPTY_SEARCH_ITEM);
            return offset;
        }
        checkIndex(value.head(), off, 0);
        byte[] s = search.head().getBinaryValue();
        int i = find(in, s, off);
        if (i == -1)
            return ZeroOrOne.empty();
        return One.integer(i);
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

    private static void checkEncoding(One<StringValue> encoding) throws XPathException {
        String se = encoding.getStringValue().toUpperCase();
        if (se.equals("UTF-8") || se.equals("UTF-16")
                || se.equals("US-ASCII") || se.equals("ASCII") || se.equals("ISO-8859-1")
                )
            return;
        else
            error("Unsupported encoding in binary encode/decode:" + encoding.getStringValue(),
                    ERROR_UNKNOWN_ENCODING);
    }

    // bin:decode-string($value as xs:base64Binary?)
    // as xs:string?
    public static ZeroOrOne<StringValue> decodeString(ZeroOrOne<Base64BinaryValue> value)
            throws XPathException, UnsupportedEncodingException {
        return decodeString(value, One.string("UTF-8"), One.integer(0));
    }

    // bin:decode-string($value as xs:base64Binary?, $encoding as xs:string)
    // as xs:string?
    public static ZeroOrOne<StringValue> decodeString(ZeroOrOne<Base64BinaryValue> value, One<StringValue> encoding)
            throws XPathException, UnsupportedEncodingException {
        return decodeString(value, encoding, One.integer(0));
    }

    // bin:decode-string($value as xs:base64Binary?, $encoding as xs:string,
    // $offset as xs:integer)
    // as xs:string?
    public static ZeroOrOne<StringValue> decodeString(ZeroOrOne<Base64BinaryValue> value, One<StringValue> encoding,
                                                      One<IntegerValue> offset) throws UnsupportedEncodingException,
            XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int len = (int) (value.head().getLengthInOctets() - offset.head().longValue());
        return decodeString(value, encoding, offset, One.integer(len));
    }

    // bin:decode-string($value as xs:base64Binary?, $encoding as xs:string,
    // $offset as xs:integer, $size as xs:size)
    // as xs:string?
    public static ZeroOrOne<StringValue> decodeString(ZeroOrOne<Base64BinaryValue> value, One<StringValue> encoding,
                                                      One<IntegerValue> offset, One<IntegerValue> size)
            throws UnsupportedEncodingException, XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        checkEncoding(encoding);
        int off = (int) offset.head().longValue();
        int len = (int) size.head().longValue();
        checkIndexInclusive(value.head(), off, len);
        if (len == 0) // This needs a bit of thought
            return One.string("");

        CharsetDecoder decoder = Charset.forName(encoding.getStringValue()).newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        ByteBuffer in = ByteBuffer.wrap(value.head().getBinaryValue(), off, len);
        char[] outChars = new char[len];
        CharBuffer out = CharBuffer.wrap(outChars);
        CoderResult res = decoder.decode(in, out, true);
        if (res.isError()) {
            if (res.isMalformed())
                error("Malformed input in decoding", ERROR_CONVERSION);
            if (res.isUnmappable())
                error("Unmappable input in decoding", ERROR_CONVERSION);
            error("Other error in decoding", ERROR_CONVERSION);
        }
        char[] resChars = new char[out.position()];
        System.arraycopy(outChars, 0, resChars, 0, out.position());
        return One.string(new String(resChars));
    }

    // bin:encode-string($value as xs:string?)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> encodeString(ZeroOrOne<StringValue> value)
            throws XPathException, UnsupportedEncodingException {
        return encodeString(value, One.string("UTF-8"));
    }

    // bin:encode-string($value as xs:string?, $encoding as xs:string)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> encodeString(ZeroOrOne<StringValue> value, One<StringValue> encoding)
            throws XPathException, UnsupportedEncodingException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        checkEncoding(encoding);
        // May need to use CharsetEncoder for better control
        CharsetEncoder encoder = Charset.forName(encoding.getStringValue()).newEncoder();
        encoder.onMalformedInput(CodingErrorAction.REPORT);
        String s = value.head().getStringValue();
        CharBuffer in = CharBuffer.wrap(s);
        ByteBuffer out = null;
        try {
            out = encoder.encode(in);
        } catch (MalformedInputException e) {
            error("Malformed input in encoding:" + e, ERROR_CONVERSION);
        } catch (UnmappableCharacterException e) {
            error("Unmappable input in encoding:" + e, ERROR_CONVERSION);
        } catch (CharacterCodingException e) {
            error("Character code problem in encoding:" + e, ERROR_CONVERSION);
        }
        byte data[] = new byte[out.limit()];
        System.arraycopy(out.array(), 0, data, 0, out.limit());
        return one(data);
        //return one(value.getStringValue().getBytes(encoding.getStringValue()));
    }

	/* Numeric */

    private static One<Base64BinaryValue> pack(long in, int length, boolean big) {
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

    private static One<Base64BinaryValue> packBytes(byte[] in, int length,
                                                    boolean big) {
        return packBytes(in, length, big, false);
    }

    private static One<Base64BinaryValue> packBytes(byte[] in, int length,
                                                    boolean big, boolean negative) {
        byte[] result = new byte[length];
        int len = in.length;
        int i;
        if (big) {
            i = len;
            while (length > 0 && i > 0) {
                result[--length] = in[--i];
            }
            if (negative)
                while (length > 0)
                    result[--length] = (byte) (255);
        } else {
            i = 0;
            while (length-- > 0 && len > 0) {
                result[i++] = in[--len];
            }
            if (negative)
                while (length > 0)
                    result[i++] = (byte) (255);
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

    // bin:unpack-double($contents as xs:base64Binary, $offset as
    // xs:integer)
    // as xs:double
    public static One<DoubleValue> unpackDouble(One<Base64BinaryValue> value,
                                                One<IntegerValue> offset) throws XPathException {
        return unpackDouble(value, offset, One.string(defaultEndianness));
    }

    // bin:unpack-double($contents as xs:base64Binary, $offset as
    // xs:integer, $endian as xs:string)
    // as xs:double
    public static One<DoubleValue> unpackDouble(One<Base64BinaryValue> value,
                                                One<IntegerValue> offset, One<StringValue> endian) throws XPathException {
        int off = (int) offset.head().longValue();
        byte[] in = value.head().getBinaryValue();
        int len = 8;
        checkIndex(value.head(), off, len);
        long bits = unpack(in, off, len, endianness(endian));
        Double d = Double.longBitsToDouble(bits);
        return new One(new DoubleValue(d.isNaN() ? Double.NaN : d));
    }

    // bin:unpack-float($contents as xs:base64Binary, $offset as xs:integer)
    // as xs:float
    public static One<FloatValue> unpackFloat(One<Base64BinaryValue> value,
                                              One<IntegerValue> offset)
            throws XPathException {
        return unpackFloat(value, offset, One.string(defaultEndianness));
    }

    // bin:unpack-float($contents as xs:base64Binary, $offset as xs:integer,
    // $endian as xs:string)
    // as xs:float
    public static One<FloatValue> unpackFloat(One<Base64BinaryValue> value,
                                              One<IntegerValue> offset, One<StringValue> endian) throws XPathException {
        int off = (int) offset.head().longValue();
        byte[] in = value.head().getBinaryValue();
        int len = 4;
        checkIndex(value.head(), off, len);
        long bits = unpack(in, off, len, endianness(endian));
        Float f = Float.intBitsToFloat((int) bits);
        return new One(new FloatValue(f.isNaN() ? Float.NaN : f));
    }

    // bin:unpack-unsigned-integer($contents as xs:base64Binary, $offset as
    // xs:integer, $length as xs:integer)
    // as xs:integer
    public static One<IntegerValue> unpackUnsignedInteger(One<Base64BinaryValue> value,
                                                          One<IntegerValue> offset, One<IntegerValue> length) throws XPathException {
        return unpackUnsignedInteger(value, offset, length, One.string(defaultEndianness));
    }

    // bin:unpack-unsigned-integer($contents as xs:base64Binary, $offset as
    // xs:integer, $length as xs:integer, $endian as xs:string)
    // as xs:integer
    public static One<IntegerValue> unpackUnsignedInteger(One<Base64BinaryValue> value,
                                                          One<IntegerValue> offset, One<IntegerValue> length, One<StringValue> endian)
            throws XPathException {
        boolean bigEnd = endianness(endian);
        int off = (int) offset.head().longValue();
        int len = (int) length.head().longValue();
        byte[] in = value.head().getBinaryValue();
        checkIndex(value.head(), off, len);
        if (len == 0)
            return One.integer(0);
        if (len > 7) {
            BigInteger big = new BigInteger(unpackBytes(in, off, len,
                    bigEnd, true));
            return new One(new BigIntegerValue(big));
        } else {
            long result = unpack(in, off, len, bigEnd);
            return One.integer(result);
        }
    }

    // bin:unpack-integer($contents as xs:base64Binary, $offset as
    // xs:integer, $length as xs:integer)
    // as xs:integer
    public static One<IntegerValue> unpackInteger(One<Base64BinaryValue> value,
                                                  One<IntegerValue> offset, One<IntegerValue> length) throws XPathException {
        return unpackInteger(value, offset, length, One.string(defaultEndianness));
    }

    // bin:unpack-integer($contents as xs:base64Binary, $offset as
    // xs:integer, $length as xs:integer)
    // as xs:integer
    public static One<IntegerValue> unpackInteger(One<Base64BinaryValue> value,
                                                  One<IntegerValue> offset, One<IntegerValue> length, One<StringValue> endian)
            throws XPathException {
        boolean bigEnd = endianness(endian);
        // Check here to use Big Integer
        int off = (int) offset.head().longValue();
        int len = (int) length.head().longValue();
        byte[] in = value.head().getBinaryValue();
        checkIndex(value.head(), off, len);
        if (len == 0)
            return One.integer(0);
        boolean negative = (in[off] & 0x80) != 0;
        if (len > 7) {
            BigInteger big = new BigInteger(unpackBytes(in, off, len,
                    bigEnd, false));
            return new One(new BigIntegerValue(big));
        } else {
            long signExt = negative ? ~((1l << (8 * len)) - 1) : 0;
            long result = unpack(in, off, len, bigEnd);
            return One.integer(result | signExt);
        }
    }

    // bin:pack-double($value as xs:double)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packDouble(One<DoubleValue> value)
            throws XPathException {
        return packDouble(value, One.string(defaultEndianness));
    }

    // bin:pack-double($value as xs:double, $endian as xs:string)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packDouble(One<DoubleValue> value, One<StringValue> endian)
            throws XPathException {
        return pack(Double.doubleToRawLongBits(value.head().getDoubleValue()), 8,
                endianness(endian));
    }

    // bin:pack-float($value as xs:float)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packFloat(One<FloatValue> value)
            throws XPathException {
        return packFloat(value, One.string(defaultEndianness));
    }

    // bin:pack-float($value as xs:float, $endian as xs:string)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packFloat(One<FloatValue> value, One<StringValue> endian)
            throws XPathException {
        return pack(Float.floatToRawIntBits(value.head().getFloatValue()), 4,
                endianness(endian));
    }

    // bin:pack-integer($value as xs:integer, $length as xs:integer)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packInteger(One<IntegerValue> value,
                                                     One<IntegerValue> length) throws XPathException {
        return packInteger(value, length, One.string(defaultEndianness));
    }

    // bin:pack-integer($value as xs:integer, $length as xs:integer, $endian
    // as xs:string)
    // as xs:base64Binary
    public static One<Base64BinaryValue> packInteger(One<IntegerValue> value,
                                                     One<IntegerValue> length, One<StringValue> endian) throws XPathException {
        if (length.head().signum() == -1)
            error("Requested length of integer packing is negative", ERROR_NEGATIVE_SIZE);

        return packBytes(value.head().asBigInteger().toByteArray(),
                (int) length.head().longValue(), endianness(endian), value.head().asBigInteger().signum() == -1);
    }

    /* Bitwise */
    // bin:pad-left($contents as xs:base64Binary?, $size as xs:integer)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> padLeft(ZeroOrOne<Base64BinaryValue> value,
                                                       IntegerValue size) throws XPathException {
        return padLeft(value, size, Int64Value.ZERO);
    }

    // bin:pad-left($contents as xs:base64Binary?, $size as xs:integer, $octet
    // as xs:integer)
    // as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> padLeft(ZeroOrOne<Base64BinaryValue> value,
                                                       IntegerValue sizeIV, IntegerValue octet) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int size = (int) sizeIV.longValue();
        int oct = (int) octet.longValue();
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
            int len = value.head().getLengthInOctets();
            byte[] in = value.head().getBinaryValue();
            byte[] result = new byte[len + size];
            if (oct != 0)
                Arrays.fill(result, 0, size, (byte) oct);
            // for (int i = 0; i < size; i++)
            //     result[i] = (byte) oct;
            System.arraycopy(in, 0, result, size, len);
            return one(result);
        }

    }

    // bin:pad-left($contents as xs:base64Binary?, $size as xs:integer)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> padRight(ZeroOrOne<Base64BinaryValue> value,
                                                        One<IntegerValue> size) throws XPathException {
        return padRight(value, size, One.integer(0));
    }

    // bin:pad-right($contents as xs:base64Binary?, $size as xs:integer)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> padRight(ZeroOrOne<Base64BinaryValue> value,
                                                        One<IntegerValue> sizeIV, One<IntegerValue> octet) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int size = (int) sizeIV.head().longValue();
        int oct = (int) octet.head().longValue();

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
            int len = value.head().getLengthInOctets();
            byte[] in = value.head().getBinaryValue();
            byte[] result = new byte[len + size];
            System.arraycopy(in, 0, result, 0, len);
            if (oct != 0)
                Arrays.fill(result, len, len + size, (byte) oct);
            //for (int i = len; i < len + size; i++)
            //    result[i] = (byte) oct;
            return one(result);
        }
    }


    // bin:shift($contents as xs:base64Binary?, $bits as xs:integer)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> shift(ZeroOrOne<Base64BinaryValue> value,
                                                     One<IntegerValue> bitsIV) throws XPathException {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int bits = (int) bitsIV.head().longValue();
        if (bits == 0)
            return value;
        else {
            int len = value.head().getLengthInOctets();
            byte[] in = value.head().getBinaryValue();
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

    // bin:not($contents as xs:base64Binary?)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> not(ZeroOrOne<Base64BinaryValue> value) {
        if (value.head() == null)
            return ZeroOrOne.empty();
        int len = value.head().getLengthInOctets();
        byte[] in = value.head().getBinaryValue();
        byte[] result = new byte[len];
        int i = 0;
        while (i < len)
            result[i] = (byte) ~in[i++];
        return one(result);
    }

    // bin:or($value1 as xs:base64Binary?, $value2 as xs:base64Binary?)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> or(ZeroOrOne<Base64BinaryValue> value1,
                                                  ZeroOrOne<Base64BinaryValue> value2) throws XPathException {
        checkSameLength(value1, value2);
        if (value1.head() == null || value2.head() == null)
            return ZeroOrOne.empty();
        int len = value1.head().getLengthInOctets();
        byte[] in1 = value1.head().getBinaryValue();
        byte[] in2 = value2.head().getBinaryValue();
        byte[] result = new byte[len];
        int i = 0;
        while (i < len) {
            result[i] = (byte) (in1[i] | in2[i]);
            i++;
        }
        return one(result);
    }

    // bin:and($value1 as xs:base64Binary?, $value2 as xs:base64Binary?)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> and(ZeroOrOne<Base64BinaryValue> value1,
                                                   ZeroOrOne<Base64BinaryValue> value2) throws XPathException {
        checkSameLength(value1, value2);
        if (value1.head() == null || value2.head() == null)
            return ZeroOrOne.empty();
        int len = value1.head().getLengthInOctets();
        byte[] in1 = value1.head().getBinaryValue();
        byte[] in2 = value2.head().getBinaryValue();
        byte[] result = new byte[len];
        int i = 0;
        while (i < len) {
            result[i] = (byte) (in1[i] & in2[i]);
            i++;
        }
        return one(result);
    }

    // bin:xor($value1 as xs:base64Binary?, $value2 as xs:base64Binary?)
// as xs:base64Binary?
    public static ZeroOrOne<Base64BinaryValue> xor(ZeroOrOne<Base64BinaryValue> value1,
                                                   ZeroOrOne<Base64BinaryValue> value2) throws XPathException {
        checkSameLength(value1, value2);
        if (value1.head() == null || value2.head() == null)
            return ZeroOrOne.empty();
        int len = value1.head().getLengthInOctets();
        byte[] in1 = value1.head().getBinaryValue();
        byte[] in2 = value2.head().getBinaryValue();
        byte[] result = new byte[len];
        int i = 0;
        while (i < len) {
            result[i] = (byte) (in1[i] ^ in2[i]);
            i++;
        }
        return one(result);
    }
}

// Copyright (c) 2018 Saxonica Limited
