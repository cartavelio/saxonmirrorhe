using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public class CharacterSetFactory {
        private readonly System.Collections.Generic.Dictionary<string,Saxon.Hej.serialize.charcode.CharacterSet> characterSets = new System.Collections.Generic.Dictionary<string, Saxon.Hej.serialize.charcode.CharacterSet>(10);
        public CharacterSetFactory()  {
            System.Collections.Generic.Dictionary<string,Saxon.Hej.serialize.charcode.CharacterSet> c = characterSets;
            Saxon.Hej.serialize.charcode.UTF8CharacterSet utf8 = Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            Saxon.Impl.Helpers.MapUtils.Put(c, "utf8", utf8);
            Saxon.Hej.serialize.charcode.UTF16CharacterSet utf16 = Saxon.Hej.serialize.charcode.UTF16CharacterSet.getInstance();
            Saxon.Impl.Helpers.MapUtils.Put(c, "utf16", utf16);
            Saxon.Hej.serialize.charcode.ASCIICharacterSet acs = Saxon.Hej.serialize.charcode.ASCIICharacterSet.getInstance();
            Saxon.Impl.Helpers.MapUtils.Put(c, "ascii", acs);
            Saxon.Impl.Helpers.MapUtils.Put(c, "iso646", acs);
            Saxon.Impl.Helpers.MapUtils.Put(c, "usascii", acs);
            Saxon.Hej.serialize.charcode.ISO88591CharacterSet lcs = Saxon.Hej.serialize.charcode.ISO88591CharacterSet.getInstance();
            Saxon.Impl.Helpers.MapUtils.Put(c, "iso88591", lcs);
        }
        public void setCharacterSetImplementation(string encoding, Saxon.Hej.serialize.charcode.CharacterSet charSet) {
            Saxon.Impl.Helpers.MapUtils.Put(characterSets, normalizeCharsetName(encoding), charSet);
        }
        private static string normalizeCharsetName(string name) {
            return name.Replace("-", "").Replace("_", "").ToLower();
        }
        public Saxon.Hej.serialize.charcode.CharacterSet getCharacterSet(Saxon.Ejava.util.Properties details) {
            string encoding = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            if ((encoding == null)) {
                return Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            }
            return getCharacterSet(encoding);
        }
        public Saxon.Hej.serialize.charcode.CharacterSet getCharacterSet(string encoding) {
            if ((encoding == null)) {
                return Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            } else {
                string encodingKey = normalizeCharsetName(encoding);
                Saxon.Hej.serialize.charcode.CharacterSet cs = Saxon.Impl.Helpers.MapUtils.Get(characterSets, encodingKey);
                if ((cs != null)) {
                    return cs;
                }
                return Saxon.Helpers.DotNetCharacterSet.GetCharacterSet(encoding);
            }
        }
        public Saxon.Hej.serialize.charcode.CharacterSet unknownEncoding(string encoding) {
            throw new Saxon.Hej.trans.XPathException(("Unknown encoding requested: "+encoding), "SESU0007");
        }
    
    }
}