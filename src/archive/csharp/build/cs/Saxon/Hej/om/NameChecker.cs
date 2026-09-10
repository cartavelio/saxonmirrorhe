using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public abstract class NameChecker {
        public static bool isQName(Saxon.Hej.z.IntIterator codePoints) {
            bool atStart = true;
            bool foundColon = false;
            while (codePoints.hasNext()) {
                int ch = codePoints.next();
                if ((ch == ':')) {
                    if ((atStart || foundColon)) {
                        return false;
                    }
                    atStart = true;
                    foundColon = true;
                } else {
                    if (atStart) {
                        if (!(isNCNameStartChar(ch))) {
                            return false;
                        }
                        atStart = false;
                    } else {
                        if (!(isNCNameChar(ch))) {
                            return false;
                        }
                    }
                }
            }
            return !(atStart);
        }
        public static string getPrefix(string qname) {
            int colon = qname.IndexOf(':');
            if ((colon<0)) {
                return "";
            }
            return qname.Substring(0, colon - 0);
        }
        public static string[] getQNameParts(string qname) {
            string[] parts = new string[2];
            int len = qname.Length;
            int colon = qname.IndexOf(':', 0);
            if ((colon<0)) {
                parts[0] = "";
                parts[1] = qname;
                if (!(isValidNCName(Saxon.Hej.str.StringTool.codePoints(qname)))) {
                    throw new Saxon.Hej.om.QNameException(("Invalid QName "+Saxon.Hej.trans.Err.wrap(qname)));
                }
            } else {
                if ((colon == 0)) {
                    throw new Saxon.Hej.om.QNameException(("QName cannot start with colon: "+Saxon.Hej.trans.Err.wrap(qname)));
                }
                if ((colon == (len-1))) {
                    throw new Saxon.Hej.om.QNameException(("QName cannot end with colon: "+Saxon.Hej.trans.Err.wrap(qname)));
                }
                parts[0] = qname.Substring(0, colon - 0);
                parts[1] = qname.Substring((colon+1));
                if (!(isValidNCName(parts[1]))) {
                    if (!(isValidNCName(parts[0]))) {
                        throw new Saxon.Hej.om.QNameException((((("Both the prefix "+Saxon.Hej.trans.Err.wrap(parts[0]))+" and the local part ")+Saxon.Hej.trans.Err.wrap(parts[1]))+" are invalid"));
                    }
                    throw new Saxon.Hej.om.QNameException(("Invalid QName local part "+Saxon.Hej.trans.Err.wrap(parts[1])));
                }
            }
            return parts;
        }
        public static string[] checkQNameParts(string qname) {
            try {
                string[] parts = getQNameParts(qname);
                if (((parts[0].Length>0) && !(isValidNCName(parts[0])))) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid QName prefix "+Saxon.Hej.trans.Err.wrap(parts[0])));
                }
                return parts;
            } catch (Saxon.Hej.om.QNameException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(e.Message);
                err.setErrorCode("FORG0001");
                throw err;
            }
        }
        public static bool isValidNCName(Saxon.Hej.z.IntIterator codePoints) {
            bool first = true;
            while (codePoints.hasNext()) {
                int ch = codePoints.next();
                if (first) {
                    if (!(isNCNameStartChar(ch))) {
                        return false;
                    }
                    first = false;
                } else {
                    if (!(isNCNameChar(ch))) {
                        return false;
                    }
                }
            }
            return !(first);
        }
        public static bool isValidNCName(string str) {
            return isValidNCName(Saxon.Hej.str.StringTool.codePoints(str));
        }
        public static bool isValidNmtoken(Saxon.Hej.str.UnicodeString @in) {
            Saxon.Hej.z.IntIterator codePoints = @in.codePoints();
            bool empty = true;
            while (codePoints.hasNext()) {
                int ch = codePoints.next();
                empty = false;
                if (((ch != ':') && !(isNCNameChar(ch)))) {
                    return false;
                }
            }
            return !(empty);
        }
        public static bool isNCNameChar(int ch) {
            return Saxon.Hej.serialize.charcode.XMLCharacterData.isNCName11(ch);
        }
        public static bool isNCNameStartChar(int ch) {
            return Saxon.Hej.serialize.charcode.XMLCharacterData.isNCNameStart11(ch);
        }
    
    }
}