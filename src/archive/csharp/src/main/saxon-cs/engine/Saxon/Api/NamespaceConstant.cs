using System;
using System.Collections.Generic;
using System.Text;
using JNamespaceConstant = Saxon.Hej.lib.NamespaceConstant;

namespace Saxon.Api {
    /// <summary> 
    /// Holds a set of constants representing known namespaces.
    /// </summary>
    public static class NamespaceConstant {
        /// <summary>
        /// A URI representing the null namespace (actually, an empty string)
        /// </summary>
        public static readonly string NULL = "";

        /// <summary>
        /// Fixed namespace name for XML: "http://www.w3.org/XML/1998/namespace".
        /// </summary>
        public const string XML = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// Fixed namespace name for XSLT: "http://www.w3.org/1999/XSL/Transform"
        /// </summary>
        public const string XSLT = "http://www.w3.org/1999/XSL/Transform";

        /// <summary>
        /// Fixed namespace name for Saxon: "http://saxon.sf.net/"
        /// </summary>
        public const string SAXON = "http://saxon.sf.net/";

        /// <summary>
        /// Fixed namespace name for the export of a Saxon stylesheet package: "http://ns.saxonica.com/xslt/export"
        /// </summary>
        public const string SAXON_XSLT_EXPORT = "http://ns.saxonica.com/xslt/export";

        /// <summary>
        /// Namespace name for XML Schema: "http://www.w3.org/2001/XMLSchema"
        /// </summary>
        public const string SCHEMA = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// XML-schema-defined namespace for use in instance documents ("xsi"): "http://www.w3.org/2001/XMLSchema-instance"
        /// </summary>
        public const string SCHEMA_INSTANCE = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// Namespace defined in XSD 1.1 for schema versioning: "http://www.w3.org/2007/XMLSchema-versioning"
        /// </summary>
        public const string SCHEMA_VERSIONING = "http://www.w3.org/2007/XMLSchema-versioning";

        /// <summary>
        /// Fixed namespace name for Saxon SQL extension: "http://saxon.sf.net/sql"
        /// </summary>
        public const string SQL = "http://saxon.sf.net/sql";

        /// <summary>
        /// Fixed namespace name for EXSLT/Common: "http://exslt.org/common"
        /// </summary>
        public const string EXSLT_COMMON = "http://exslt.org/common";
        
        /// <summary>
        /// The standard namespace for functions and operators: "http://www.w3.org/2005/xpath-functions"
        /// </summary>
        public const string FN = "http://www.w3.org/2005/xpath-functions";

        /// <summary>
        /// The standard namespace for XQuery output declarations: "http://www.w3.org/2010/xslt-xquery-serialization"
        /// </summary>
        public const string OUTPUT = "http://www.w3.org/2010/xslt-xquery-serialization";


        /// <summary>
        /// The standard namespace for system error codes: "http://www.w3.org/2005/xqt-errors"
        /// </summary>
        public const string ERR = "http://www.w3.org/2005/xqt-errors";

        /// <summary>
        /// Predefined XQuery namespace for local functions: "http://www.w3.org/2005/xquery-local-functions"
        /// </summary>
        public const string LOCAL = "http://www.w3.org/2005/xquery-local-functions";

        /// <summary>
        /// Namespace name for the XPath 3.0 math functions: "http://www.w3.org/2005/xpath-functions/math"
        /// </summary>
        public const string MATH = "http://www.w3.org/2005/xpath-functions/math";

        /// <summary>
        /// Namespace name for XPath 3.0 functions associated with maps: "http://www.w3.org/2005/xpath-functions/map"
        /// </summary>
        public const string MAP = "http://www.w3.org/2005/xpath-functions/map";

        /// <summary>
        /// Namespace name for XPath 3.0 functions associated with arrays: "http://www.w3.org/2005/xpath-functions/array"
        /// </summary>
        public const string ARRAY = "http://www.w3.org/2005/xpath-functions/array";

        /// <summary>
        /// The XHTML namespace: "http://www.w3.org/1999/xhtml"
        /// </summary>
        public const string XHTML = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// The SVG namespace: "http://www.w3.org/2000/svg"
        /// </summary>
        public const string SVG = "http://www.w3.org/2000/svg";

        /// <summary>
        /// The MathML namespace: "http://www.w3.org/1998/Math/MathML"
        /// </summary>
        public const string MATHML = "http://www.w3.org/1998/Math/MathML";

        /// <summary>
        /// The XMLNS namespace (used in DOM): "http://www.w3.org/2000/xmlns/"
        /// </summary>
        public const string XMLNS = "http://www.w3.org/2000/xmlns/";

        /// <summary>
        /// The XLink namespace: "http://www.w3.org/1999/xlink"
        /// </summary>
        public const string XLINK = "http://www.w3.org/1999/xlink";

        /// <summary>
        /// The xquery-option namespace for the XQuery 3.0 feature names: "http://www.w3.org/2011/xquery-options"
        /// </summary>
        public const string XQUERY_OPTIONS = "http://www.w3.org/2011/xquery-options";

        /// <summary>
        /// The xquery namespace for the XQuery 3.0 declare option: "http://www.w3.org/2012/xquery"
        /// </summary>
        public const string XQUERY = "http://www.w3.org/2012/xquery";
        
        /// <summary>
        /// Namespace for names allocated to anonymous types: "http://ns.saxonica.com/anonymous-type". 
        /// This exists so that a name fingerprint can be allocated for use as a type annotation.
        /// </summary>
        public const string ANONYMOUS = "http://ns.saxonica.com/anonymous-type";

        /// <summary>
        /// Namespace for the Saxon serialization of the schema component model: "http://ns.saxonica.com/schema-component-model"
        /// </summary>
        public const string SCM = "http://ns.saxonica.com/schema-component-model";
        
        /// <summary>
        /// URI identifying the Unicode codepoint collation
        /// </summary>
        public const string CODEPOINT_COLLATION_URI =
            "http://www.w3.org/2005/xpath-functions/collation/codepoint";

        /// <summary>
        /// URI identifying the HML5 ascii-case-blind collation
        /// </summary>
        public const string HTML5_CASE_BLIND_COLLATION_URI =
            "http://www.w3.org/2005/xpath-functions/collation/html5-ascii-case-insensitive";

        /// <summary>
        /// Namespace for the names of generated global variables: "http://saxon.sf.net/generated-global-variable"
        /// </summary>
        public const string SAXON_GENERATED_GLOBAL = SAXON + "generated-global-variable";

        /// <summary>
        /// Namespace for the Saxon configuration file: "http://saxon.sf.net/ns/configuration"
        /// </summary>
        public const string SAXON_CONFIGURATION = "http://saxon.sf.net/ns/configuration";

        /// <summary>
        /// Namespace for the EXPath zip module: "http://expath.org/ns/zip" 
        /// </summary>
        public const string EXPATH_ZIP = "http://expath.org/ns/zip";


        /// <summary>
        /// Determine whether a namespace is a reserved namespace
        /// </summary>
        /// <returns><c>true</c>, if this namespace URI is a reserved namespace, <c>false</c> otherwise.</returns>
        /// <param name="uri">the namespace URI to be tested</param>
        public static bool IsReserved( /*@Nullable*/ string uri) {
            return JNamespaceConstant.isReserved(uri);
        }
        
        /// <summary>
        /// Determine whether a namespace is a reserved namespace in XQuery 3.1
        /// </summary>
        /// <returns><c>true</c>, if this namespace URI is reserved in XQuery 3.1, <c>false</c> otherwise.</returns>
        /// <param name="uri">the namespace URI to be tested</param>
        public static bool IsReservedInQuery31(string uri) {
            return JNamespaceConstant.isReservedInQuery31(uri);
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited
