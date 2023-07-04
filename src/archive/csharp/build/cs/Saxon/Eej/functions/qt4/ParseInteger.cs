using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ParseInteger : Saxon.Hej.functions.SystemFunction {
        public static Saxon.Hej.value.StringValue ignorables = new Saxon.Hej.value.StringValue(new Saxon.Hej.str.Twine8(new byte[]{9, 10, 13, 32, 95/*_*/}));
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
            int radix = 10;
            if ((arguments.Length == 2)) {
                Saxon.Hej.value.IntegerValue r = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
                if (((r.compareTo(2)<0) || (r.compareTo(36)>0))) {
                    throw new Saxon.Hej.trans.XPathException("Radix in parse-integer must be in range 2..36", "FORG0011");
                }
                radix = (int)(r.longValue());
            }
            string val = Saxon.Hej.functions.Translate.translate(s, ignorables, Saxon.Hej.value.StringValue.EMPTY_STRING).getStringValue();
            try {
                System.Numerics.BigInteger x = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(val, radix);
                return Saxon.Hej.value.IntegerValue.makeIntegerValue(x);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                throw new Saxon.Hej.trans.XPathException((("Cannot parse integer '"+s.getStringValue())+"'"), "FORG0012");
            }
        }
    
    }
}