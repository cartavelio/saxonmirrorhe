using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class SaxonOutputKeys {
        private SaxonOutputKeys()  {
        }
        public const string SAXON_XQUERY_METHOD = "Q{http://saxon.sf.net/}xquery";
        public const string SAXON_BASE64_BINARY_METHOD = "Q{http://saxon.sf.net/}base64Binary";
        public const string SAXON_HEX_BINARY_METHOD = "Q{http://saxon.sf.net/}hexBinary";
        public const string SAXON_XML_TO_JSON_METHOD = "Q{http://saxon.sf.net/}xml-to-json";
        public const string ALLOW_DUPLICATE_NAMES = "allow-duplicate-names";
        public const string BUILD_TREE = "build-tree";
        public const string INDENT_SPACES = "{http://saxon.sf.net/}indent-spaces";
        public const string INTERNAL_DTD_SUBSET = "{http://saxon.sf.net/}internal-dtd-subset";
        public const string LINE_LENGTH = "{http://saxon.sf.net/}line-length";
        public const string SINGLE_QUOTES = "{http://saxon.sf.net/}single-quotes";
        public const string SUPPRESS_INDENTATION = "suppress-indentation";
        public const string HTML_VERSION = "html-version";
        public const string ITEM_SEPARATOR = "item-separator";
        public const string JSON_NODE_OUTPUT_METHOD = "json-node-output-method";
        public const string ATTRIBUTE_ORDER = "{http://saxon.sf.net/}attribute-order";
        public const string CANONICAL = "{http://saxon.sf.net/}canonical";
        public const string PROPERTY_ORDER = "{http://saxon.sf.net/}property-order";
        public const string DOUBLE_SPACE = "{http://saxon.sf.net/}double-space";
        public const string NEWLINE = "{http://saxon.sf.net/}newline";
        public const string STYLESHEET_VERSION = "{http://saxon.sf.net/}stylesheet-version";
        public const string USE_CHARACTER_MAPS = "use-character-maps";
        public const string INCLUDE_CONTENT_TYPE = "include-content-type";
        public const string UNDECLARE_PREFIXES = "undeclare-prefixes";
        public const string ESCAPE_URI_ATTRIBUTES = "escape-uri-attributes";
        public const string CHARACTER_REPRESENTATION = "{http://saxon.sf.net/}character-representation";
        public const string NEXT_IN_CHAIN = "{http://saxon.sf.net/}next-in-chain";
        public const string NEXT_IN_CHAIN_BASE_URI = "{http://saxon.sf.net/}next-in-chain-base-uri";
        public const string PARAMETER_DOCUMENT = "parameter-document";
        public const string PARAMETER_DOCUMENT_BASE_URI = "{http://saxon.sf.net/}parameter-document-base-uri";
        public const string BYTE_ORDER_MARK = "byte-order-mark";
        public const string NORMALIZATION_FORM = "normalization-form";
        public const string RECOGNIZE_BINARY = "{http://saxon.sf.net/}recognize-binary";
        public const string REQUIRE_WELL_FORMED = "{http://saxon.sf.net/}require-well-formed";
        public const string SUPPLY_SOURCE_LOCATOR = "{http://saxon.sf.net/}supply-source-locator";
        public const string WRAP = "{http://saxon.sf.net/}wrap-result-sequence";
        public const string UNFAILING = "{http://saxon.sf.net/}unfailing";
        public static string parseListOfNodeNames(string value, Saxon.Hej.om.NamespaceResolver nsResolver, bool useDefaultNS, bool prevalidated, bool allowStar, string errorCode) {
            Saxon.Ejava.lang.StringBuilder s = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(value, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string displayname = st.nextToken();
                if ((allowStar && "*".Equals(displayname))) {
                    s.append(' ').append(displayname);
                } else {
                    if ((prevalidated || ((nsResolver == null)))) {
                        s.append(' ').append(displayname);
                    } else {
                        if (displayname.StartsWith("Q{")) {
                            s.append(' ').append(displayname);
                        } else {
                            try {
                                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(displayname);
                                string muri = nsResolver.getURIForPrefix(parts[0], useDefaultNS);
                                if ((muri == null)) {
                                    throw new Saxon.Hej.trans.XPathException((("Namespace prefix '"+parts[0])+"' has not been declared"), errorCode);
                                }
                                s.append(" Q{").append(muri).append('}').append(parts[1]);
                            } catch (Saxon.Hej.om.QNameException err) {
                                throw new Saxon.Hej.trans.XPathException(("Invalid QName. "+err.Message), errorCode);
                            }
                        }
                    }
                }
            }
            return s.ToString();
        }
        public static bool isUnstrippedProperty(string key) {
            return (ITEM_SEPARATOR.Equals(key) || NEWLINE.Equals(key));
        }
        public static bool isXhtmlHtmlVersion5(Saxon.Ejava.util.Properties properties) {
            string htmlVersion = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION);
            try {
                return ((htmlVersion != null) && ((Saxon.Hej.value.BigDecimalValue)(Saxon.Hej.value.BigDecimalValue.makeDecimalValue(htmlVersion, false).asAtomic())).getDecimalValue().Equals(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(5)));
            } catch (Saxon.Hej.type.ValidationException e) {
                return false;
            }
        }
        public static bool isHtmlVersion5(Saxon.Ejava.util.Properties properties) {
            string htmlVersion = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION);
            if ((htmlVersion == null)) {
                htmlVersion = properties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION);
            }
            if ((htmlVersion != null)) {
                try {
                    return ((Saxon.Hej.value.BigDecimalValue)(Saxon.Hej.value.BigDecimalValue.makeDecimalValue(htmlVersion, false).asAtomic())).getDecimalValue().Equals(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(5));
                } catch (Saxon.Hej.type.ValidationException e) {
                    return false;
                }
            } else {
                return true;
            }
        }
        public static bool isBuildTree(Saxon.Ejava.util.Properties properties) {
            string buildTreeProperty = properties.getProperty("build-tree");
            if ((buildTreeProperty != null)) {
                return "yes".Equals(buildTreeProperty);
            }
            string method = properties.getProperty("method");
            return !((("json".Equals(method) || "adaptive".Equals(method))));
        }
    
    }
}