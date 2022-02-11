using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CodepointsToString : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public static Saxon.Hej.value.StringValue unicodeToString(Saxon.Hej.om.SequenceIterator chars, Saxon.Hej.z.IntPredicateProxy checker) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            while (true) {
                Saxon.Hej.value.NumericValue nextInt = (Saxon.Hej.value.NumericValue)(chars.next());
                if ((nextInt == null)) {
                    return new Saxon.Hej.value.StringValue(sb.toUnicodeString());
                }
                long next = nextInt.longValue();
                if ((((next<0) || (next>System.Int32.MaxValue)) || !(checker.test((int)(next))))) {
                    throw new Saxon.Hej.trans.XPathException((("codepoints-to-string(): invalid XML character [x"+Saxon.Impl.Helpers.Utils.intToHexString((int)(next)))+']'), "FOCH0001");
                }
                sb.append((int)(next));
            }
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator chars = arguments[0].iterate();
            return unicodeToString(chars, context.getConfiguration().getValidCharacterChecker());
        }
        public override string getStreamerName() {
            return "CodepointsToString";
        }
    
    }
}