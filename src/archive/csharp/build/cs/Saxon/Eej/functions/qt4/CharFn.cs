using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class CharFn : Saxon.Hej.functions.SystemFunction {
        private static System.Collections.Generic.Dictionary<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> ENTITIES = null;
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue name = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString uName = name.getUnicodeStringValue();
            if (!(uName.isEmpty())) {
                int lead = uName.codePointAt(0);
                if (((lead == '\\') && (uName.length() == 2))) {
                    switch (uName.codePointAt(1)) {
                      case 'n':
                        return new Saxon.Hej.value.StringValue(new Saxon.Hej.str.UnicodeChar(10));
                      case 't':
                        return new Saxon.Hej.value.StringValue(new Saxon.Hej.str.UnicodeChar(9));
                      case 'r':
                        return new Saxon.Hej.value.StringValue(new Saxon.Hej.str.UnicodeChar(13));
                      default:
                        return invalid(uName);
                    }
                } else {
                    if (((lead == '#') && (uName.length()>=2))) {
                        try {
                            int cp;
                            if ((uName.codePointAt(1) == 'x')) {
                                cp = Saxon.Impl.Helpers.Utils.ParseInt(uName.substring(2).ToString(), 16);
                            } else {
                                cp = Saxon.Impl.Helpers.Utils.ParseInt(uName.substring(1).ToString(), 10);
                            }
                            if (!(context.getConfiguration().getValidCharacterChecker().test(cp))) {
                                throw new Saxon.Hej.trans.XPathException(("Character supplied to fn:char is not a valid XML character: "+uName), "FOCH0005");
                            }
                            return new Saxon.Hej.value.StringValue(new Saxon.Hej.str.UnicodeChar(cp));
                        } catch (Saxon.Ejava.lang.NumberFormatException e) {
                            invalid(uName);
                        }
                    } else {
                        return new Saxon.Hej.value.StringValue(readEntity(uName));
                    }
                }
            }
            return invalid(uName);
        }
        public static Saxon.Hej.str.UnicodeString NewLineName = Saxon.Hej.str.BMPString.of("NewLine");
        public Saxon.Hej.om.Sequence invalid(Saxon.Hej.str.UnicodeString name) {
            throw new Saxon.Hej.trans.XPathException(("Invalid argument to fn:char(): "+Saxon.Hej.trans.Err.wrap(name.ToString())), "FOCH0005");
        }
        public Saxon.Hej.str.UnicodeString readEntity(Saxon.Hej.str.UnicodeString name) {
            lock (typeof(Saxon.Eej.functions.qt4.CharFn)) {
                if ((ENTITIES == null)) {
                    build();
                }
            }
            Saxon.Hej.str.UnicodeString value = Saxon.Impl.Helpers.MapUtils.Get(ENTITIES, name);
            if ((value == null)) {
                invalid(name);
            }
            return value;
        }
        static void build() {
            ENTITIES = new System.Collections.Generic.Dictionary<Saxon.Hej.str.UnicodeString, Saxon.Hej.str.UnicodeString>(2500);
            Saxon.Impl.Helpers.MapUtils.Put(ENTITIES, NewLineName, new Saxon.Hej.str.UnicodeChar(10));
            System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("entities.txt", new System.Collections.Generic.List<string>());
            if ((@in == null)) {
                throw new System.Exception("Unable to read entities.txt file");
            }
            Saxon.Impl.Helpers.LineNumberReader reader = new Saxon.Impl.Helpers.LineNumberReader(new System.IO.StreamReader(@in, System.Text.Encoding.UTF8), 2048);
            string line;
            try {
                while (((line = reader.ReadLine()) != null)) {
                    int eq = line.IndexOf('=');
                    string key = line.Substring(0, eq - 0);
                    string value = line.Substring((eq+1));
                    Saxon.Hej.str.UnicodeString uValue = Saxon.Hej.str.StringTool.fromCharSequence(value);
                    if ((uValue.length() == 1)) {
                        uValue = new Saxon.Hej.str.UnicodeChar(uValue.codePointAt(0));
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(ENTITIES, new Saxon.Hej.str.Twine8(key), uValue);
                }
                reader.Close();
            } catch (System.IO.IOException e) {
                throw new System.Exception("Failed to read entities.txt file", e);
            }
        }
    
    }
}