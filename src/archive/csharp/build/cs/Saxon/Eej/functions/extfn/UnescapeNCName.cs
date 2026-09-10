using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class UnescapeNCName : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue arg = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString input = arg.getUnicodeStringValue();
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            if (!(((Saxon.Hej.type.BuiltInAtomicType.NCNAME.matches(arg, th) || Saxon.Hej.om.NameChecker.isValidNCName(input.codePoints()))))) {
                throw new Saxon.Hej.trans.XPathException((("Input to saxon:unescape-NCName must be a valid NCName: supplied value is '"+input)+"'"), "FORG0001");
            }
            if (((input.length() == 1) && (input.codePointAt(0) == '_'))) {
                return new Saxon.Hej.value.StringValue("");
            }
            Saxon.Hej.str.UnicodeBuilder @out = new Saxon.Hej.str.UnicodeBuilder((int)(input.estimatedLength()));
            Saxon.Hej.z.IntIterator iter = input.codePoints();
            while (iter.hasNext()) {
                int ch = iter.next();
                if ((ch == '_')) {
                    Saxon.Ejava.lang.StringBuilder hex = new Saxon.Ejava.lang.StringBuilder(6);
                    while (iter.hasNext()) {
                        int digit = iter.next();
                        if ((digit == '_')) {
                            if ((hex.length() == 0)) {
                                @out.append('_');
                            } else {
                                int code = Saxon.Impl.Helpers.Utils.ParseInt(hex.ToString(), 16);
                                if (!(Saxon.Hej.serialize.charcode.XMLCharacterData.isValid11(code))) {
                                    throw new Saxon.Hej.trans.XPathException(("Invalid escaped XML character: "+hex), "FORG0001");
                                }
                                @out.append(code);
                            }
                            break ;
                        } else {
                            if (((digit<128) && ("0123456789abcdefABCDEF".IndexOf((char)(digit))>=0))) {
                                hex.append((char)(digit));
                            } else {
                                throw new Saxon.Hej.trans.XPathException((("Invalid character '"+Saxon.Impl.Helpers.Utils.intToHexString(digit))+" in escaped NCName"));
                            }
                        }
                    }
                } else {
                    @out.append(ch);
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