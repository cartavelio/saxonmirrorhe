using System;
using System.Xml;
using net.sf.saxon.trans;

namespace Saxon.Impl.Helpers {
    public class ProcInstParser {

        public static string getPseudoAttribute(string data, string attName) {
            try {
                string xml = "<a " + data + "/>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlElement top = doc.DocumentElement;
                return top.HasAttribute(attName) ? top.GetAttribute(attName) : null;
            }
            catch (Exception e) {
                throw new net.sf.saxon.trans.XPathException(
                    "Invalid syntax for pseudo-attributes: " + e.Message, 
                    SaxonErrorCode.SXCH0005);
            }
        }
        
    }
}