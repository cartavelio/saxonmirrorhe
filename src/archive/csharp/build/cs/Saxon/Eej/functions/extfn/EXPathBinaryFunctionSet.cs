using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class EXPathBinaryFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet THE_INSTANCE = new Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet();
        public static Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private EXPathBinaryFunctionSet()  {
            init();
        }
        public const double VERSION = 1.0;
        public const string NAMESPACE = "http://expath.org/ns/binary";
        public const string PREFIX = "bin";
        public const string ERROR_NAMESPACE = NAMESPACE;
        public const string ERROR_PREFIX = PREFIX;
        private const string defaultEndianness = "BE";
        private const string ERROR_DIFFERENT_LENGTH_ARGUMENTS = "differing-length-arguments";
        private const string ERROR_INDEX_BEFORE_START = "index-out-of-range";
        private const string ERROR_INDEX_AFTER_END = "index-out-of-range";
        private const string ERROR_NEGATIVE_SIZE = "negative-size";
        private const string ERROR_EMPTY_SEARCH_ITEM = "empty-search-item";
        private const string ERROR_OCTET_RANGE = "octet-out-of-range";
        private const string ERROR_NON_NUMERIC_CHAR = "non-numeric-character";
        private const string ERROR_UNKNOWN_ENCODING = "unknown-encoding";
        private const string ERROR_CONVERSION = "conversion-error";
        private const string ERROR_SIGNIFICANCE_ORDER = "unknown-significance-order";
        private const long QUIET_DOUBLE_NAN = 0x7ff8_0000_0000_0000L;
        private const int QUIET_SINGLE_NAN = 0x7fc0_0000;
        private void init() {
            register("and", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryAnd), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("bin", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryBin), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("decode-string", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryDecodeString), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("decode-string", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryDecodeString), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("decode-string", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryDecodeString), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("decode-string", 4, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryDecodeString), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("encode-string", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryEncodeString), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("encode-string", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryEncodeString), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("find", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryFind), Saxon.Hej.type.BuiltInAtomicType.INTEGER, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null);
            register("from-octets", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryFromOctets), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, null);
            register("hex", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryHex), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("insert-before", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryInsertBefore), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, null);
            register("join", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryJoin), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, STAR, null);
            register("length", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryLength), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, Saxon.Hej.value.EmptySequence.getInstance());
            register("not", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryNot), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("octal", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryOctal), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("or", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryOr), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance());
            register("pack-double", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackDouble), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null);
            register("pack-double", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackDouble), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("pack-float", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackFloat), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.FLOAT, ONE, null);
            register("pack-float", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackFloat), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.FLOAT, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("pack-integer", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackInteger), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("pack-integer", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPackInteger), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("pad-left", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPadLeft), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("pad-left", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPadLeft), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("pad-right", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPadRight), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("pad-right", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPadRight), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("part", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPart), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("part", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryPart), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("shift", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryShift), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("to-octets", 1, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryToOctets), Saxon.Hej.type.BuiltInAtomicType.INTEGER, STAR, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null);
            register("unpack-double", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackDouble), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("unpack-double", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackDouble), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unpack-float", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackFloat), Saxon.Hej.type.BuiltInAtomicType.FLOAT, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("unpack-float", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackFloat), Saxon.Hej.type.BuiltInAtomicType.FLOAT, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unpack-integer", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackInteger), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("unpack-integer", 4, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackInteger), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unpack-unsigned-integer", 3, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackUnsignedInteger), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("unpack-unsigned-integer", 4, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryUnpackUnsignedInteger), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(3, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("version", 0, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryVersion), Saxon.Hej.type.BuiltInAtomicType.DECIMAL, ONE, 0);
            register("xor", 2, typeof(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.BinaryXor), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance()).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, OPT, Saxon.Hej.value.EmptySequence.getInstance());
        }
        public override string getNamespace() {
            return NAMESPACE;
        }
        public override string getConventionalPrefix() {
            return PREFIX;
        }
        public class BinaryAnd : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value1 = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                Saxon.Hej.value.Base64BinaryValue value2 = (Saxon.Hej.value.Base64BinaryValue)(arguments[1].head());
                if (((value1 == null) || (value2 == null))) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                checkSameLength(value1, value2);
                int len = value1.getLengthInOctets();
                byte[] in1 = value1.getBinaryValue();
                byte[] in2 = value2.getBinaryValue();
                byte[] result = new byte[value1.getLengthInOctets()];
                int i = 0;
                while ((i<len)) {
                    result[i] = (byte)(((in1[i]&in2[i])));
                    i++;
                }
                return one(result);
            }
        
        }
        public class BinaryBin : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
                if ((s == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return inputNumber(s.getStringValue(), 2, 1);
            }
        
        }
        public class BinaryDecodeString : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                string encoding;
                if ((arguments.Length<2)) {
                    encoding = "UTF-8";
                } else {
                    encoding = arguments[1].head().getStringValue();
                }
                int offset;
                if ((arguments.Length<3)) {
                    offset = 0;
                } else {
                    offset = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                }
                int len;
                if ((arguments.Length<4)) {
                    len = (value.getLengthInOctets()-offset);
                } else {
                    len = (int)(((Saxon.Hej.value.NumericValue)(arguments[3].head())).longValue());
                }
                checkIndexInclusive(value, offset, len);
                try {
                    string result = Saxon.Hej.resource.BinaryResource.decode(value.getBinaryValue(), offset, len, encoding);
                    return new Saxon.Hej.value.StringValue(result);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.setErrorCodeQName(new Saxon.Hej.om.StructuredQName("bin", ERROR_NAMESPACE, Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.ERROR_CONVERSION));
                    throw;
                }
            }
        
        }
        public class BinaryEncodeString : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.StringValue value = (Saxon.Hej.value.StringValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                string s = ((/*1430*/Saxon.Hej.value.AtomicValue)value.head()).getStringValue();
                string encoding;
                if ((arguments.Length<2)) {
                    encoding = "UTF-8";
                } else {
                    encoding = arguments[1].head().getStringValue();
                }
                byte[] data;
                try {
                    data = Saxon.Hej.resource.BinaryResource.encode(s, encoding);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.setErrorCodeQName(new Saxon.Hej.om.StructuredQName("bin", ERROR_NAMESPACE, Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.ERROR_CONVERSION));
                    throw;
                }
                return one(data);
            }
        
        }
        public class BinaryFind : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                Saxon.Hej.value.IntegerValue offsetValue = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
                int off = (int)(offsetValue.longValue());
                Saxon.Hej.value.Base64BinaryValue search = (Saxon.Hej.value.Base64BinaryValue)(arguments[2].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                byte[] @in = value.getBinaryValue();
                if ((search.getLengthInOctets() == 0)) {
                    return offsetValue;
                }
                checkIndex(value, off, 0);
                byte[] s = search.getBinaryValue();
                int i = find(@in, s, off);
                if ((i == -1)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return new Saxon.Hej.value.Int64Value(i);
            }
        
        }
        public class BinaryFromOctets : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.GroundedValue gv = arguments[0].materialize();
                byte[] bytes = new byte[gv.getLength()];
                Saxon.Hej.om.SequenceIterator iter = gv.iterate();
                Saxon.Hej.om.Item item;
                int i = 0;
                while (((item = iter.next()) != null)) {
                    long v = ((Saxon.Hej.value.IntegerValue)(item)).longValue();
                    if (((0<=v) && (v<=255))) {
                        bytes[i++] = (byte)(v);
                    } else {
                        error(("Integer outside octet range in binary constructor:"+v), ERROR_OCTET_RANGE);
                    }
                }
                return one(bytes);
            }
        
        }
        public static void error(string message, string code) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message);
            e.setErrorCodeQName(new Saxon.Hej.om.StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
            throw e;
        }
        public class BinaryHex : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
                if ((s == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return inputNumber(s.getStringValue(), 16, 4);
            }
        
        }
        public class BinaryInsertBefore : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int valLen = value.getLengthInOctets();
                Saxon.Hej.value.IntegerValue offsetValue = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
                int off = (int)(offsetValue.longValue());
                Saxon.Hej.value.Base64BinaryValue extra = (Saxon.Hej.value.Base64BinaryValue)(arguments[2].head());
                if ((extra == null)) {
                    return value;
                }
                checkIndexInclusive(value, off, 0);
                int extraLen = extra.getLengthInOctets();
                byte[] result = new byte[(valLen+extraLen)];
                Saxon.Ejava.lang.System.arraycopy(value.getBinaryValue(), 0, result, 0, off);
                Saxon.Ejava.lang.System.arraycopy(extra.getBinaryValue(), 0, result, off, extraLen);
                Saxon.Ejava.lang.System.arraycopy(value.getBinaryValue(), off, result, (off+extraLen), (valLen-off));
                return one(result);
            }
        
        }
        public class BinaryJoin : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                int len = 0;
                Saxon.Hej.om.GroundedValue input = arguments[0].materialize();
                Saxon.Hej.om.SequenceIterator iter = input.iterate();
                Saxon.Hej.value.Base64BinaryValue bv;
                while (((bv = (Saxon.Hej.value.Base64BinaryValue)(iter.next())) != null)) {
                    len += bv.getLengthInOctets();
                }
                byte[] result = new byte[len];
                int i = 0;
                iter = input.iterate();
                while (((bv = (Saxon.Hej.value.Base64BinaryValue)(iter.next())) != null)) {
                    int lb = bv.getLengthInOctets();
                    Saxon.Ejava.lang.System.arraycopy(bv.getBinaryValue(), 0, result, i, lb);
                    i += lb;
                }
                return one(result);
            }
        
        }
        public class BinaryLength : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                return new Saxon.Hej.value.Int64Value(value.getLengthInOctets());
            }
        
        }
        public class BinaryNot : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int len = value.getLengthInOctets();
                byte[] @in = value.getBinaryValue();
                byte[] result = new byte[len];
                int i = 0;
                while ((i<len)) {
                    result[i] = (byte)(~(@in[i++]));
                }
                return one(result);
            }
        
        }
        public class BinaryOctal : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
                if ((s == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Ejava.lang.StringBuilder binary = new Saxon.Ejava.lang.StringBuilder();
                Saxon.Hej.z.IntIterator iter = s.getUnicodeStringValue().codePoints();
                while (iter.hasNext()) {
                    int c = iter.next();
                    if (((c<'0') || (c>'7'))) {
                        error(("Wrong character in base 8 binary constructor string:"+c), ERROR_NON_NUMERIC_CHAR);
                    }
                    binary.append(((((c&4)) == 0) ? "0" : "1"));
                    binary.append(((((c&2)) == 0) ? "0" : "1"));
                    binary.append(((((c&1)) == 0) ? "0" : "1"));
                }
                return inputNumber(binary.ToString(), 2, 1);
            }
        
        }
        public class BinaryOr : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value1 = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                Saxon.Hej.value.Base64BinaryValue value2 = (Saxon.Hej.value.Base64BinaryValue)(arguments[1].head());
                if (((value1 == null) || (value2 == null))) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                checkSameLength(value1, value2);
                int len = value1.getLengthInOctets();
                byte[] in1 = value1.getBinaryValue();
                byte[] in2 = value2.getBinaryValue();
                byte[] result = new byte[value1.getLengthInOctets()];
                int i = 0;
                while ((i<len)) {
                    result[i] = (byte)(((in1[i]|in2[i])));
                    i++;
                }
                return one(result);
            }
        
        }
        public class BinaryPackDouble : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.DoubleValue value = (Saxon.Hej.value.DoubleValue)(arguments[0].head());
                long bits;
                if (value.isNaN()) {
                    bits = QUIET_DOUBLE_NAN;
                } else {
                    bits = System.BitConverter.DoubleToInt64Bits(value.getDoubleValue());
                }
                return pack(bits, 8, endianness(arguments, 1));
            }
        
        }
        public class BinaryPackFloat : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.FloatValue value = (Saxon.Hej.value.FloatValue)(arguments[0].head());
                int bits;
                if (value.isNaN()) {
                    bits = QUIET_SINGLE_NAN;
                } else {
                    bits = System.BitConverter.SingleToInt32Bits(value.getFloatValue());
                }
                return pack(bits, 4, endianness(arguments, 1));
            }
        
        }
        public class BinaryPackInteger : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.IntegerValue value = (Saxon.Hej.value.IntegerValue)(arguments[0].head());
                int length = (int)(((Saxon.Hej.value.IntegerValue)(arguments[1].head())).longValue());
                if ((length<0)) {
                    error("Requested length of integer packing is negative", ERROR_NEGATIVE_SIZE);
                }
                System.Numerics.BigInteger big = value.asBigInteger();
                return packBytes(big.ToByteArray(isUnsigned: false, isBigEndian: true), length, endianness(arguments, 2), (Saxon.Impl.Helpers.BigIntegerUtils.Signum(big) == -1));
            }
        
        }
        public class BinaryPadLeft : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int size = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int oct = 0;
                if ((arguments.Length>2)) {
                    oct = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                }
                if ((size<0)) {
                    error("Pad size is negative", ERROR_NEGATIVE_SIZE);
                    return null;
                } else {
                    if (((oct<0) || (255<oct))) {
                        error(("Integer outside octet range in padding:"+oct), ERROR_OCTET_RANGE);
                        return null;
                    } else {
                        if ((size == 0)) {
                            return value;
                        } else {
                            int len = value.getLengthInOctets();
                            byte[] @in = value.getBinaryValue();
                            byte[] result = new byte[(len+size)];
                            if ((oct != 0)) {
                                Saxon.Ejava.util.Arrays.fill(result, 0, size, (byte)(oct));
                            }
                            Saxon.Ejava.lang.System.arraycopy(@in, 0, result, size, len);
                            return one(result);
                        }
                    }
                }
            }
        
        }
        public class BinaryPadRight : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int size = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int oct = 0;
                if ((arguments.Length>2)) {
                    oct = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                }
                if ((size<0)) {
                    error("Pad size is negative", ERROR_NEGATIVE_SIZE);
                    return null;
                } else {
                    if (((oct<0) || (255<oct))) {
                        error(("Integer outside octet range in padding:"+oct), ERROR_OCTET_RANGE);
                        return null;
                    } else {
                        if ((size == 0)) {
                            return value;
                        } else {
                            int len = value.getLengthInOctets();
                            byte[] @in = value.getBinaryValue();
                            byte[] result = new byte[(len+size)];
                            Saxon.Ejava.lang.System.arraycopy(@in, 0, result, 0, len);
                            if ((oct != 0)) {
                                Saxon.Ejava.util.Arrays.fill(result, len, (len+size), (byte)(oct));
                            }
                            return one(result);
                        }
                    }
                }
            }
        
        }
        public class BinaryPart : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int start = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int len;
                if ((arguments.Length == 2)) {
                    len = System.Math.Max(0, (value.getLengthInOctets()-start));
                } else {
                    len = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                }
                checkIndexInclusive(value, start, len);
                if (((start == 0) && (len == value.getLengthInOctets()))) {
                    return value;
                } else {
                    byte[] result = new byte[len];
                    Saxon.Ejava.lang.System.arraycopy(value.getBinaryValue(), start, result, 0, len);
                    return one(result);
                }
            }
        
        }
        public class BinaryShift : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                if ((value == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int bits = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                if ((bits == 0)) {
                    return value;
                } else {
                    int len = value.getLengthInOctets();
                    byte[] @in = value.getBinaryValue();
                    byte[] result = new byte[len];
                    int shift = System.Math.Abs(bits);
                    int byteShift = (shift/8);
                    int bitShift = (shift%8);
                    int i = 0;
                    int offset = 0;
                    byte part = 0;
                    int partShift;
                    if ((bits>0)) {
                        offset = byteShift;
                        partShift = (8-bitShift);
                        if ((bitShift == 0)) {
                            while ((offset<len)) {
                                result[i++] = @in[offset++];
                            }
                        } else {
                            while ((offset<len)) {
                                part = (byte)(((((offset == (len-1))) ? 0 : (Saxon.Impl.Helpers.Utils.unsignedRightShift((@in[(offset+1)]), partShift)))));
                                result[i] = (byte)(((((@in[offset]<<bitShift))|part)));
                                i++;
                                offset++;
                            }
                        }
                    } else {
                        i = byteShift;
                        partShift = (8-bitShift);
                        while ((i<len)) {
                            result[i] = (byte)((((Saxon.Impl.Helpers.Utils.unsignedRightShift(((@in[offset]&unchecked((int)0xff))), bitShift))|part)));
                            part = (byte)(((((@in[offset]&unchecked((int)0xff)))<<partShift)));
                            i++;
                            offset++;
                        }
                    }
                    return one(result);
                }
            }
        
        }
        public class BinaryToOctets : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                System.Collections.Generic.IList<Saxon.Hej.value.IntegerValue> result = new System.Collections.Generic.List<Saxon.Hej.value.IntegerValue>();
                foreach (byte b in value.getBinaryValue()) {
                    result.Add(new Saxon.Hej.value.Int64Value((b&0xFF), Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE));
                }
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
            }
        
        }
        public class BinaryUnpackDouble : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                int offset = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int len = 8;
                checkIndex(value, offset, len);
                double d = toDouble(value.getBinaryValue(), offset, len, endianness(arguments, 2));
                return (System.Double.IsNaN(d) ? Saxon.Hej.value.DoubleValue.NaN : new Saxon.Hej.value.DoubleValue(d));
            }
            private static double toDouble(byte[] bytes, int offset, int len, bool bigEndian) {
                return Saxon.Impl.Helpers.Utils.bytesToDouble(bytes, offset, bigEndian);
            }
        
        }
        public class BinaryUnpackFloat : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                int offset = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int len = 4;
                checkIndex(value, offset, len);
                float f = toFloat(value.getBinaryValue(), offset, len, endianness(arguments, 2));
                return (System.Single.IsNaN(f) ? Saxon.Hej.value.FloatValue.NaN : new Saxon.Hej.value.FloatValue(f));
            }
            private float toFloat(byte[] bytes, int offset, int len, bool bigEndian) {
                return Saxon.Impl.Helpers.Utils.bytesToFloat(bytes, offset, bigEndian);
            }
        
        }
        public class BinaryUnpackInteger : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                int offset = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int len = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                checkIndex(value, offset, len);
                byte[] @in = value.getBinaryValue();
                bool negative = (((@in[offset]&unchecked((int)0x80))) != 0);
                bool bigEnd = endianness(arguments, 3);
                if ((len == 0)) {
                    return Saxon.Hej.value.Int64Value.ZERO;
                }
                if ((len>7)) {
                    System.Numerics.BigInteger big = bigIntegerFromBytes(offset, len, @in, bigEnd);
                    return new Saxon.Hej.value.BigIntegerValue(big);
                } else {
                    long signExt = (negative ? -(((1L<<((8*len))))) : 0);
                    long result = unpack(@in, offset, len, bigEnd);
                    return new Saxon.Hej.value.Int64Value((result|signExt));
                }
            }
            private System.Numerics.BigInteger bigIntegerFromBytes(int offset, int len, byte[] @in, bool bigEnd) {
                return Saxon.Impl.Helpers.BigIntegerUtils.FromBytes(@in, offset, len, false, bigEnd);
            }
        
        }
        public class BinaryUnpackUnsignedInteger : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                int offset = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
                int len = (int)(((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue());
                checkIndex(value, offset, len);
                byte[] @in = value.getBinaryValue();
                bool bigEnd = endianness(arguments, 3);
                checkIndex(value, offset, len);
                if ((len == 0)) {
                    return Saxon.Hej.value.Int64Value.ZERO;
                }
                if ((len>7)) {
                    System.Numerics.BigInteger big = bigIntegerFromBytes(@in, offset, len, bigEnd);
                    return new Saxon.Hej.value.BigIntegerValue(big);
                } else {
                    long result = unpack(@in, offset, len, bigEnd);
                    return new Saxon.Hej.value.Int64Value(result);
                }
            }
            private static System.Numerics.BigInteger bigIntegerFromBytes(byte[] @in, int offset, int len, bool bigEnd) {
                return Saxon.Impl.Helpers.BigIntegerUtils.FromBytes(@in, offset, len, true, bigEnd);
            }
        
        }
        public class BinaryVersion : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.BigDecimalValue(VERSION);
            }
        
        }
        public class BinaryXor : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.Base64BinaryValue value1 = (Saxon.Hej.value.Base64BinaryValue)(arguments[0].head());
                Saxon.Hej.value.Base64BinaryValue value2 = (Saxon.Hej.value.Base64BinaryValue)(arguments[1].head());
                if (((value1 == null) || (value2 == null))) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                checkSameLength(value1, value2);
                int len = value1.getLengthInOctets();
                byte[] in1 = value1.getBinaryValue();
                byte[] in2 = value2.getBinaryValue();
                byte[] result = new byte[value1.getLengthInOctets()];
                int i = 0;
                while ((i<len)) {
                    result[i] = (byte)(((in1[i]^in2[i])));
                    i++;
                }
                return one(result);
            }
        
        }
        private static Saxon.Hej.value.Base64BinaryValue one(byte[] result) {
            return new Saxon.Hej.value.Base64BinaryValue(result);
        }
        private static void checkIndex(Saxon.Hej.value.Base64BinaryValue value, int offset, int length) {
            if ((offset<0)) {
                error("Attempting to retrieve data before the start of a binary data type", ERROR_INDEX_BEFORE_START);
            }
            int len = value.getLengthInOctets();
            if ((length<0)) {
                error("Requested length of binary section is negative", ERROR_NEGATIVE_SIZE);
            }
            if ((offset>=len)) {
                error(((("Attempting to retrieve data beyond the end of a binary data type; index:"+offset)+" data:")+len), ERROR_INDEX_AFTER_END);
            }
            if (((offset+length)>len)) {
                error(((("Attempting to retrieve data beyond the end of a binary data type; index:"+((offset+length)))+" data:")+len), ERROR_INDEX_AFTER_END);
            }
        }
        private static void checkIndexInclusive(Saxon.Hej.value.Base64BinaryValue value, int offset, int length) {
            if ((offset<0)) {
                error("Attempting to retrieve data before the start of a binary data type", ERROR_INDEX_BEFORE_START);
            }
            int len = value.getLengthInOctets();
            if ((length<0)) {
                error("Requested length of binary section is negative", ERROR_NEGATIVE_SIZE);
            }
            if ((offset>len)) {
                error(((("Attempting to retrieve data beyond the end of a binary data type; index:"+offset)+" data:")+len), ERROR_INDEX_AFTER_END);
            }
            if (((offset+length)>len)) {
                error(((("Attempting to retrieve data beyond the end of a binary data type; index:"+((offset+length)))+" data:")+len), ERROR_INDEX_AFTER_END);
            }
        }
        private static void checkSameLength(Saxon.Hej.value.Base64BinaryValue value1, Saxon.Hej.value.Base64BinaryValue value2) {
            if ((value1.getLengthInOctets() != value2.getLengthInOctets())) {
                error("The arguments to a bitwise operation are differing lengths", ERROR_DIFFERENT_LENGTH_ARGUMENTS);
            }
        }
        private static bool endianness(Saxon.Hej.om.Sequence[] args, int position) {
            string s;
            if ((args.Length<=position)) {
                s = defaultEndianness;
            } else {
                s = args[position].head().getStringValue();
            }
            if (((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "BE") || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "most-significant-first")) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "big-endian"))) {
                return true;
            }
            if (((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "LE") || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "least-significant-first")) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, "little-endian"))) {
                return false;
            }
            error(("Unrecognized octet-order request:"+s), ERROR_SIGNIFICANCE_ORDER);
            return false;
        }
        private static Saxon.Hej.value.Base64BinaryValue inputNumber(string s, int @base, int bitLength) {
            int bits = (s.Length*bitLength);
            int charsPerOctet = (8/bitLength);
            int octetLength = (((bits+7))/8);
            int padLength = (bits%8);
            byte[] result = new byte[octetLength];
            int offset = 0;
            int end = ((padLength == 0) ? charsPerOctet : ((padLength/bitLength)));
            int b = 0;
            try {
                while ((offset<s.Length)) {
                    result[b++] = (byte)(((Saxon.Impl.Helpers.Utils.ParseInt(s.Substring(offset, end - offset), @base)&unchecked((int)0xff))));
                    offset = end;
                    end = (offset+charsPerOctet);
                }
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                error(((("Wrong character in base "+@base)+" binary constructor string:")+s), ERROR_NON_NUMERIC_CHAR);
            }
            return one(result);
        }
        private static int find(byte[] data, byte[] pattern, int offset) {
            int[] failure = computeFailure(pattern);
            int j = 0;
            if ((data.Length == 0)) {
                return -1;
            }
            for (int i = offset; (i<data.Length); i++) {
                while (((j>0) && (pattern[j] != data[i]))) {
                    j = failure[(j-1)];
                }
                if ((pattern[j] == data[i])) {
                    j++;
                }
                if ((j == pattern.Length)) {
                    return ((i-pattern.Length)+1);
                }
            }
            return -1;
        }
        private static int[] computeFailure(byte[] pattern) {
            int[] failure = new int[pattern.Length];
            int j = 0;
            for (int i = 1; (i<pattern.Length); i++) {
                while (((j>0) && (pattern[j] != pattern[i]))) {
                    j = failure[(j-1)];
                }
                if ((pattern[j] == pattern[i])) {
                    j++;
                }
                failure[i] = j;
            }
            return failure;
        }
        private static void checkEncoding(string encoding) {
            string se = encoding.ToUpper();
            if (((((!(se.Equals("UTF-8")) && !(se.Equals("UTF-16"))) && !(se.Equals("US-ASCII"))) && !(se.Equals("ASCII"))) && !(se.Equals("ISO-8859-1")))) {
                error(("Unsupported encoding in binary encode/decode:"+encoding), ERROR_UNKNOWN_ENCODING);
            }
        }
        private static Saxon.Hej.value.Base64BinaryValue pack(long @in, int length, bool big) {
            byte[] result = new byte[length];
            if (big) {
                while ((length>0)) {
                    result[--length] = (byte)(((@in&unchecked((int)0xff))));
                    @in >>= 8;
                }
            } else {
                int i = 0;
                while ((length-->0)) {
                    result[i++] = (byte)(((@in&unchecked((int)0xff))));
                    @in >>= 8;
                }
            }
            return one(result);
        }
        private static Saxon.Hej.value.Base64BinaryValue packBytes(byte[] @in, int length, bool big) {
            return packBytes(@in, length, big, false);
        }
        private static Saxon.Hej.value.Base64BinaryValue packBytes(byte[] @in, int length, bool big, bool negative) {
            byte[] result = new byte[length];
            int len = @in.Length;
            int i;
            if (big) {
                i = len;
                while (((length>0) && (i>0))) {
                    result[--length] = @in[--i];
                }
                if (negative) {
                    while ((length>0)) {
                        result[--length] = (byte)((255));
                    }
                }
            } else {
                i = 0;
                while (((length-->0) && (len>0))) {
                    result[i++] = @in[--len];
                }
                if (negative) {
                    while ((length>0)) {
                        result[i++] = (byte)((255));
                    }
                }
            }
            return one(result);
        }
        private static long unpack(byte[] @in, int offset, int length, bool big) {
            long result = 0;
            if (big) {
                while ((length-->0)) {
                    result = (((result<<8))+((@in[offset++]&0xFF)));
                }
            } else {
                int off = (offset+length);
                while ((length-->0)) {
                    result = (((result<<8))+((@in[--off]&0xFF)));
                }
            }
            return result;
        }
        private static byte[] unpackBytes(byte[] @in, int offset, int length, bool big, bool unsigned) {
            byte[] @out = new byte[(length+((unsigned ? 1 : 0)))];
            int i = (unsigned ? 1 : 0);
            if (big) {
                while ((length-->0)) {
                    @out[i++] = @in[offset++];
                }
            } else {
                int off = (offset+length);
                while ((length-->0)) {
                    @out[i++] = @in[--off];
                }
            }
            return @out;
        }
    
    }
}