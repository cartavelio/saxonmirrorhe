using System;
using System.Xml;
using Saxon.Hej.trans;

namespace Saxon.Impl.Helpers {
    public class ProcInstParser {

        private static readonly string substituteColon = "__C_O_L_O_N__";

        public static string getPseudoAttribute(string data, string attName) {
            try {
                string xml = "<a " + data + "/>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlElement top = doc.DocumentElement;
                return top != null && top.HasAttribute(attName) ? top.GetAttribute(attName) : null;
            }
            catch (Exception e) {
                if (data.Contains(":")) {
                    // It might have failed because of a colon in an attribute name
                    data = data.Replace(":", substituteColon);
                    attName = attName.Replace(":", substituteColon);
                    try {
                        string result = getPseudoAttribute(data, attName);
                        return result.Replace(substituteColon, ":");
                    }
                    catch (Exception) {
                        // if the recovery attempt fails, unwind the recursion
                        // and throw the original exception
                    }
                }
                throw new Saxon.Hej.trans.XPathException(
                    "Invalid syntax for pseudo-attributes: " + e.Message, 
                    SaxonErrorCode.SXCH0005);
            }
        }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited