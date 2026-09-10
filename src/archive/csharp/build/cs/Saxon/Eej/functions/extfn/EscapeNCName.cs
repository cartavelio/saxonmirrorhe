using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class EscapeNCName : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeString input = arguments[0].head().getUnicodeStringValue();
            if (input.isEmpty()) {
                return new Saxon.Hej.value.StringValue("_", Saxon.Hej.type.BuiltInAtomicType.NCNAME);
            }
            Saxon.Hej.str.UnicodeBuilder @out = new Saxon.Hej.str.UnicodeBuilder((int)(input.estimatedLength()));
            Saxon.Hej.z.IntIterator iter = input.codePoints();
            if (iter.hasNext()) {
                int ch = iter.next();
                if (Saxon.Hej.om.NameChecker.isNCNameStartChar(ch)) {
                    if ((ch == '_')) {
                        @out.append('_').append('_');
                    } else {
                        @out.append(ch);
                    }
                } else {
                    escape(ch, @out);
                }
            }
            while (iter.hasNext()) {
                int ch = iter.next();
                if (Saxon.Hej.om.NameChecker.isNCNameChar(ch)) {
                    if ((ch == '_')) {
                        @out.append('_').append('_');
                    } else {
                        @out.append(ch);
                    }
                } else {
                    escape(ch, @out);
                }
            }
            return new Saxon.Hej.value.StringValue(@out.toUnicodeString(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
        }
        private void escape(int codepoint, Saxon.Hej.str.UnicodeBuilder sb) {
            sb.append('_');
            sb.append(Saxon.Impl.Helpers.Utils.intToHexString(codepoint));
            sb.append('_');
        }
    
    }
}