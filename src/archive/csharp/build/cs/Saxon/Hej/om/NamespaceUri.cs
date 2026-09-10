using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class NamespaceUri {
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<string,Saxon.Hej.om.NamespaceUri> stringToNamespaceUri = new /* java.util.concurrent.ConcurrentHashMap */ System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.om.NamespaceUri>(16, 20);
        public static Saxon.Hej.om.NamespaceUri of(string content) {
            content ??= "";
 return Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(stringToNamespaceUri, Saxon.Hej.value.Whitespace.trim(content), str => new Saxon.Hej.om.NamespaceUri(str));

        }
        private readonly string stringContent;
        private readonly Saxon.Hej.str.UnicodeString unicodeStringContent;
        private NamespaceUri(string content)  {
            this.stringContent = content;
            this.unicodeStringContent = Saxon.Hej.str.StringTool.fromCharSequence(content);
        }
        public override string ToString() {
            return stringContent;
        }
        public Saxon.Hej.str.UnicodeString toUnicodeString() {
            return unicodeStringContent;
        }
        public bool isEmpty() {
            return (this == Saxon.Hej.om.NamespaceUri.NULL);
        }
        public static readonly Saxon.Hej.om.NamespaceUri NULL = Saxon.Hej.om.NamespaceUri.of("");
        public static readonly Saxon.Hej.om.NamespaceUri XML = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/XML/1998/namespace");
        public static readonly Saxon.Hej.om.NamespaceUri XSLT = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/1999/XSL/Transform");
        public static readonly Saxon.Hej.om.NamespaceUri SAXON = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/");
        public static readonly Saxon.Hej.om.NamespaceUri SAXON6 = Saxon.Hej.om.NamespaceUri.of("http://icl.com/saxon");
        public static readonly Saxon.Hej.om.NamespaceUri SAXON_XSLT_EXPORT = Saxon.Hej.om.NamespaceUri.of("http://ns.saxonica.com/xslt/export");
        public static readonly Saxon.Hej.om.NamespaceUri SCHEMA = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2001/XMLSchema");
        public static readonly Saxon.Hej.om.NamespaceUri SCHEMA_INSTANCE = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2001/XMLSchema-instance");
        public static readonly Saxon.Hej.om.NamespaceUri SCHEMA_VERSIONING = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2007/XMLSchema-versioning");
        public static readonly Saxon.Hej.om.NamespaceUri SQL = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/sql");
        public static readonly Saxon.Hej.om.NamespaceUri EXSLT_COMMON = Saxon.Hej.om.NamespaceUri.of("http://exslt.org/common");
        public static readonly Saxon.Hej.om.NamespaceUri EXSLT_MATH = Saxon.Hej.om.NamespaceUri.of("http://exslt.org/math");
        public static readonly Saxon.Hej.om.NamespaceUri EXSLT_SETS = Saxon.Hej.om.NamespaceUri.of("http://exslt.org/sets");
        public static readonly Saxon.Hej.om.NamespaceUri EXSLT_DATES_AND_TIMES = Saxon.Hej.om.NamespaceUri.of("http://exslt.org/dates-and-times");
        public static readonly Saxon.Hej.om.NamespaceUri EXSLT_RANDOM = Saxon.Hej.om.NamespaceUri.of("http://exslt.org/random");
        public static readonly Saxon.Hej.om.NamespaceUri FN = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xpath-functions");
        public static readonly Saxon.Hej.om.NamespaceUri OUTPUT = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2010/xslt-xquery-serialization");
        public static readonly Saxon.Hej.om.NamespaceUri ERR = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xqt-errors");
        public static readonly Saxon.Hej.om.NamespaceUri LOCAL = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xquery-local-functions");
        public static readonly Saxon.Hej.om.NamespaceUri MATH = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xpath-functions/math");
        public readonly static Saxon.Hej.om.NamespaceUri MAP_FUNCTIONS = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xpath-functions/map");
        public readonly static Saxon.Hej.om.NamespaceUri ARRAY_FUNCTIONS = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2005/xpath-functions/array");
        public readonly static Saxon.Hej.om.NamespaceUri EXPATH_BINARY = Saxon.Hej.om.NamespaceUri.of("http://expath.org/ns/binary");
        public readonly static Saxon.Hej.om.NamespaceUri EXPATH_FILE = Saxon.Hej.om.NamespaceUri.of("http://expath.org/ns/file");
        public static readonly Saxon.Hej.om.NamespaceUri XHTML = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/1999/xhtml");
        public static readonly Saxon.Hej.om.NamespaceUri SVG = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2000/svg");
        public static readonly Saxon.Hej.om.NamespaceUri MATHML = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/1998/Math/MathML");
        public static readonly Saxon.Hej.om.NamespaceUri XMLNS = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2000/xmlns/");
        public static readonly Saxon.Hej.om.NamespaceUri XLINK = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/1999/xlink");
        public static readonly Saxon.Hej.om.NamespaceUri XQUERY = Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2012/xquery");
        public static readonly Saxon.Hej.om.NamespaceUri JAVA_TYPE = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/java-type");
        public static readonly Saxon.Hej.om.NamespaceUri DOT_NET_TYPE = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/clitype");
        public static readonly Saxon.Hej.om.NamespaceUri ANONYMOUS = Saxon.Hej.om.NamespaceUri.of("http://ns.saxonica.com/anonymous-type");
        public static readonly Saxon.Hej.om.NamespaceUri SCM = Saxon.Hej.om.NamespaceUri.of("http://ns.saxonica.com/schema-component-model");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_SAXON = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/jaxp/xpath/om");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_XOM = Saxon.Hej.om.NamespaceUri.of("http://www.xom.nu/jaxp/xpath/xom");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_JDOM = Saxon.Hej.om.NamespaceUri.of("http://jdom.org/jaxp/xpath/jdom");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_AXIOM = Saxon.Hej.om.NamespaceUri.of("http://ws.apache.org/jaxp/xpath/axiom");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_DOM4J = Saxon.Hej.om.NamespaceUri.of("http://www.dom4j.org/jaxp/xpath/dom4j");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_DOT_NET_DOM = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/object-model/dotnet/dom");
        public static readonly Saxon.Hej.om.NamespaceUri OBJECT_MODEL_DOMINO = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/object-model/domino");
        public static readonly Saxon.Hej.om.NamespaceUri SAXON_GENERATED_VARIABLE = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/generated-variable");
        public static readonly Saxon.Hej.om.NamespaceUri SAXON_CONFIGURATION = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/ns/configuration");
        public static readonly Saxon.Hej.om.NamespaceUri EXPATH_ZIP = Saxon.Hej.om.NamespaceUri.of("http://expath.org/ns/zip");
        public static readonly Saxon.Hej.om.NamespaceUri GLOBAL_JS = Saxon.Hej.om.NamespaceUri.of("http://saxonica.com/ns/globalJS");
        public static readonly Saxon.Hej.om.NamespaceUri PHP = Saxon.Hej.om.NamespaceUri.of("http://php.net/xsl");
        public static readonly Saxon.Hej.om.NamespaceUri IXSL = Saxon.Hej.om.NamespaceUri.of("http://saxonica.com/ns/interactiveXSLT");
        public static Saxon.Hej.om.NamespaceUri getUriForConventionalPrefix(string prefix) {
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
        public static bool isReserved(Saxon.Hej.om.NamespaceUri uri) {
            return ((uri != null) && ((((((((((uri.Equals(XSLT) || uri.Equals(FN)) || uri.Equals(MATH)) || uri.Equals(MAP_FUNCTIONS)) || uri.Equals(ARRAY_FUNCTIONS)) || uri.Equals(XML)) || uri.Equals(SCHEMA)) || uri.Equals(SCHEMA_INSTANCE)) || uri.Equals(ERR)) || uri.Equals(XMLNS))));
        }
        public static bool isReservedInQuery31(Saxon.Hej.om.NamespaceUri uri) {
            return (((((((uri.Equals(FN) || uri.Equals(XML)) || uri.Equals(SCHEMA)) || uri.Equals(SCHEMA_INSTANCE)) || uri.Equals(MATH)) || uri.Equals(XQUERY)) || uri.Equals(MAP_FUNCTIONS)) || uri.Equals(ARRAY_FUNCTIONS));
        }
    
    }
}