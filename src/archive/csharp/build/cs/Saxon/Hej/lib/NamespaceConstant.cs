using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class NamespaceConstant {
        public const string NULL = "";
        public const string XML = "http://www.w3.org/XML/1998/namespace";
        public const string XSLT = "http://www.w3.org/1999/XSL/Transform";
        public const string SAXON = "http://saxon.sf.net/";
        public const string SAXON6 = "http://icl.com/saxon";
        public const string SAXON_XSLT_EXPORT = "http://ns.saxonica.com/xslt/export";
        public const string SCHEMA = "http://www.w3.org/2001/XMLSchema";
        public const string SCHEMA_INSTANCE = "http://www.w3.org/2001/XMLSchema-instance";
        public const string SCHEMA_VERSIONING = "http://www.w3.org/2007/XMLSchema-versioning";
        public const string SQL = "http://saxon.sf.net/sql";
        public const string EXSLT_COMMON = "http://exslt.org/common";
        public const string EXSLT_MATH = "http://exslt.org/math";
        public const string EXSLT_SETS = "http://exslt.org/sets";
        public const string EXSLT_DATES_AND_TIMES = "http://exslt.org/dates-and-times";
        public const string EXSLT_RANDOM = "http://exslt.org/random";
        public const string FN = "http://www.w3.org/2005/xpath-functions";
        public const string OUTPUT = "http://www.w3.org/2010/xslt-xquery-serialization";
        public const string ERR = "http://www.w3.org/2005/xqt-errors";
        public const string LOCAL = "http://www.w3.org/2005/xquery-local-functions";
        public const string MATH = "http://www.w3.org/2005/xpath-functions/math";
        public const string MAP_FUNCTIONS = "http://www.w3.org/2005/xpath-functions/map";
        public const string ARRAY_FUNCTIONS = "http://www.w3.org/2005/xpath-functions/array";
        public const string XHTML = "http://www.w3.org/1999/xhtml";
        public const string SVG = "http://www.w3.org/2000/svg";
        public const string MATHML = "http://www.w3.org/1998/Math/MathML";
        public const string XMLNS = "http://www.w3.org/2000/xmlns/";
        public const string XLINK = "http://www.w3.org/1999/xlink";
        public const string XQUERY = "http://www.w3.org/2012/xquery";
        public const string JAVA_TYPE = "http://saxon.sf.net/java-type";
        public const string DOT_NET_TYPE = "http://saxon.sf.net/clitype";
        public const string ANONYMOUS = "http://ns.saxonica.com/anonymous-type";
        public const string SCM = "http://ns.saxonica.com/schema-component-model";
        public const string OBJECT_MODEL_SAXON = "http://saxon.sf.net/jaxp/xpath/om";
        public const string OBJECT_MODEL_XOM = "http://www.xom.nu/jaxp/xpath/xom";
        public const string OBJECT_MODEL_JDOM = "http://jdom.org/jaxp/xpath/jdom";
        public const string OBJECT_MODEL_AXIOM = "http://ws.apache.org/jaxp/xpath/axiom";
        public const string OBJECT_MODEL_DOM4J = "http://www.dom4j.org/jaxp/xpath/dom4j";
        public const string OBJECT_MODEL_DOT_NET_DOM = "http://saxon.sf.net/object-model/dotnet/dom";
        public const string OBJECT_MODEL_DOMINO = "http://saxon.sf.net/object-model/domino";
        public const string CODEPOINT_COLLATION_URI = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
        public const string HTML5_CASE_BLIND_COLLATION_URI = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
        public const string SAXON_GENERATED_VARIABLE = (SAXON+"generated-variable");
        public const string SAXON_CONFIGURATION = "http://saxon.sf.net/ns/configuration";
        public const string EXPATH_ZIP = "http://expath.org/ns/zip";
        public const string GLOBAL_JS = "http://saxonica.com/ns/globalJS";
        public const string PHP = "http://php.net/xsl";
        public const string IXSL = "http://saxonica.com/ns/interactiveXSLT";
        private NamespaceConstant()  {
        }
        public static string getConventionalPrefix(string uri) {
            switch (uri) {
              case XSLT:
                return "xsl";
              case FN:
                return "fn";
              case XML:
                return "xml";
              case SCHEMA:
                return "xs";
              case SCHEMA_INSTANCE:
                return "xsi";
              case IXSL:
                return "ixsl";
              case GLOBAL_JS:
                return "js";
              case SAXON:
                return "saxon";
              case SAXON_GENERATED_VARIABLE:
                return "vv";
              case MATH:
                return "math";
              case MAP_FUNCTIONS:
                return "map";
              case ARRAY_FUNCTIONS:
                return "array";
              case ERR:
                return "err";
              default:
                return null;
            }
        }
        public static string getUriForConventionalPrefix(string prefix) {
            switch (prefix) {
              case "xsl":
                return XSLT;
              case "fn":
                return FN;
              case "xml":
                return XML;
              case "xs":
                return SCHEMA;
              case "xsi":
                return SCHEMA_INSTANCE;
              case "err":
                return ERR;
              case "ixsl":
                return IXSL;
              case "js":
                return GLOBAL_JS;
              case "saxon":
                return SAXON;
              case "vv":
                return SAXON_GENERATED_VARIABLE;
              case "math":
                return MATH;
              case "map":
                return MAP_FUNCTIONS;
              case "array":
                return ARRAY_FUNCTIONS;
              default:
                return null;
            }
        }
        public static bool isReserved(string uri) {
            return ((uri != null) && ((((((((((uri.Equals(XSLT) || uri.Equals(FN)) || uri.Equals(MATH)) || uri.Equals(MAP_FUNCTIONS)) || uri.Equals(ARRAY_FUNCTIONS)) || uri.Equals(XML)) || uri.Equals(SCHEMA)) || uri.Equals(SCHEMA_INSTANCE)) || uri.Equals(ERR)) || uri.Equals(XMLNS))));
        }
        public static bool isReservedInQuery31(string uri) {
            return (((((((uri.Equals(FN) || uri.Equals(XML)) || uri.Equals(SCHEMA)) || uri.Equals(SCHEMA_INSTANCE)) || uri.Equals(MATH)) || uri.Equals(XQUERY)) || uri.Equals(MAP_FUNCTIONS)) || uri.Equals(ARRAY_FUNCTIONS));
        }
        public static string findSimilarNamespace(string candidate) {
            if (isSimilar(candidate, XML)) {
                return XML;
            } else {
                if (isSimilar(candidate, SCHEMA)) {
                    return SCHEMA;
                } else {
                    if (isSimilar(candidate, XSLT)) {
                        return XSLT;
                    } else {
                        if (isSimilar(candidate, SCHEMA_INSTANCE)) {
                            return SCHEMA_INSTANCE;
                        } else {
                            if (isSimilar(candidate, FN)) {
                                return FN;
                            } else {
                                if (isSimilar(candidate, SAXON)) {
                                    return SAXON;
                                } else {
                                    if (isSimilar(candidate, EXSLT_COMMON)) {
                                        return EXSLT_COMMON;
                                    } else {
                                        if (isSimilar(candidate, EXSLT_MATH)) {
                                            return EXSLT_MATH;
                                        } else {
                                            if (isSimilar(candidate, EXSLT_DATES_AND_TIMES)) {
                                                return EXSLT_DATES_AND_TIMES;
                                            } else {
                                                if (isSimilar(candidate, EXSLT_RANDOM)) {
                                                    return EXSLT_RANDOM;
                                                } else {
                                                    if (isSimilar(candidate, XHTML)) {
                                                        return XHTML;
                                                    } else {
                                                        if (isSimilar(candidate, ERR)) {
                                                            return ERR;
                                                        } else {
                                                            if (isSimilar(candidate, JAVA_TYPE)) {
                                                                return JAVA_TYPE;
                                                            } else {
                                                                if (isSimilar(candidate, DOT_NET_TYPE)) {
                                                                    return DOT_NET_TYPE;
                                                                } else {
                                                                    return null;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static bool isSimilar(string s1, string s2) {
            s1 = Saxon.Hej.value.Whitespace.removeAllWhitespace(s1);
            s2 = Saxon.Hej.value.Whitespace.removeAllWhitespace(s2);
            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s1, s2)) {
                return true;
            } else {
                if ((s1.StartsWith(s2) && ((s1.Length-s2.Length)<3))) {
                    return true;
                } else {
                    if ((s2.StartsWith(s1) && ((s2.Length-s1.Length)<3))) {
                        return true;
                    } else {
                        if (((s1.Length>8) && (System.Math.Abs((s2.Length-s1.Length))<3))) {
                            int diff = 0;
                            for (int i = 0; (i<s1.Length); i++) {
                                char c1 = s1[i];
                                if (!(((((((i<s2.Length) && (c1 == s2[i]))) || ((((i>0) && (i<(s2.Length-1))) && (c1 == s2[(i-1)])))) || ((((i+1)<s2.Length) && (c1 == s2[(i+1)]))))))) {
                                    diff++;
                                }
                            }
                            return (diff<3);
                        } else {
                            return false;
                        }
                    }
                }
            }
        }
    
    }
}