using System;
using System.Xml;
using System.Xml.Linq;
using Saxon.Hej.om;
using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// The <c>QName</c> class represents an instance of <c>xs:QName</c>, as defined in the XPath 2.0
    /// data model. Internally, it has three components, a namespace URI, a local name, and
    /// a prefix. The prefix is intended to be used only when converting the value back to 
    /// a string.
    /// </summary>
    /// <remarks>
    /// Note that a <c>QName</c> is not itself an <c>XdmItem</c> in this model; however it can
    /// be converted to an <c>XdmAtomicValue</c>.
    /// </remarks>    
    [Serializable]
    public sealed class QName {
        private StructuredQName sqName;

        private static string XS = NamespaceConstant.SCHEMA;

        /// <summary>QName constant for the name xs:string</summary>
        public static readonly QName XS_STRING = new QName(XS, "xs:string");

        /// <summary>QName constant for the name xs:integer</summary>
        public static readonly QName XS_INTEGER = new QName(XS, "xs:integer");

        /// <summary>QName constant for the name xs:double</summary>
        public static readonly QName XS_DOUBLE = new QName(XS, "xs:double");

        /// <summary>QName constant for the name xs:float</summary>
        public static readonly QName XS_FLOAT = new QName(XS, "xs:float");

        /// <summary>QName constant for the name xs:decimal</summary>
        public static readonly QName XS_DECIMAL = new QName(XS, "xs:decimal");

        /// <summary>QName constant for the name xs:boolean</summary>
        public static readonly QName XS_BOOLEAN = new QName(XS, "xs:boolean");

        /// <summary>QName constant for the name xs:anyURI</summary>
        public static readonly QName XS_ANYURI = new QName(XS, "xs:anyURI");

        /// <summary>QName constant for the name xs:QName</summary>
        public static readonly QName XS_QNAME = new QName(XS, "xs:QName");

        /// <summary>QName constant for the name xs:untypedAtomic</summary>
        public static readonly QName XS_UNTYPED_ATOMIC = new QName(XS, "xs:untypedAtomic");
        

        /// <summary>
        /// Construct a <c>QName</c> representing a name in no namespace
        /// </summary>
        /// <remarks>
        /// This constructor does not check that the components of the QName are
        /// lexically valid.
        /// </remarks>
        /// <param name="local">The local part of the name
        /// </param>
        public QName(string local) {
            // TODO: check for validity
            int colon = local.IndexOf(':');
            if (colon < 0) {
                sqName = new StructuredQName("", "", local);
            }
            else {
                throw new ArgumentException("Local name contains a colon");
            }
        }

        /// <summary>
        /// Construct a <c>QName</c> using a namespace URI and a lexical representation.
        /// The lexical representation may be a local name on its own, or it may 
        /// be in the form <c>prefix:local-name</c>
        /// </summary>
        /// <remarks>
        /// This constructor does not check that the components of the QName are
        /// lexically valid.
        /// </remarks>
        /// <param name="uri">The namespace URI. Use either the string "" or null
        /// for names that are not in any namespace.
        /// </param>
        /// <param name="lexical">Either the local part of the name, or the prefix
        /// and local part in the format <c>prefix:local</c>
        /// </param>
        public QName(string uri, string lexical) {
            // TODO: check for validity
            uri ??= "";
            int colon = lexical.IndexOf(':');
            if (colon < 0) {
                sqName = new StructuredQName("", uri, lexical);
            }
            else {
                string prefix = lexical[..colon];
                string local = lexical[(colon + 1)..];
                sqName = new StructuredQName(prefix, uri, local);
            }
        }

        /// <summary>
        /// Construct a <c>QName</c> using a namespace prefix, a namespace URI, and a local name
        /// (in that order).
        /// </summary>
        /// <remarks>
        /// This constructor does not check that the components of the QName are
        /// lexically valid.
        /// </remarks>
        /// <param name="prefix">The prefix of the name. Use either the string ""
        /// or null for names that have no prefix (that is, they are in the default
        /// namespace)</param>
        /// <param name="uri">The namespace URI. Use either the string "" or null
        /// for names that are not in any namespace.
        /// </param>
        /// <param name="local">The local part of the name</param>
        public QName(string prefix, string uri, string local) {
            sqName = new StructuredQName(prefix, uri, local);
        }

        /// <summary>
        /// Construct a <c>QName</c> from a lexical QName, supplying an element node whose
        /// in-scope namespaces are to be used to resolve any prefix contained in the QName.
        /// </summary>
        /// <remarks>
        /// <para>This constructor checks that the components of the QName are
        /// lexically valid.</para>
        /// <para>If the lexical QName has no prefix, the name is considered to be in the
        /// default namespace, as defined by <c>xmlns="..."</c>.</para>
        /// </remarks>
        /// <param name="lexicalQName">The lexical QName, in the form <code>prefix:local</code>
        /// or simply <c>local</c>.</param>
        /// <param name="element">The element node whose in-scope namespaces are to be used
        /// to resolve the prefix part of the lexical QName.</param>
        /// <exception cref="ArgumentException">If the prefix of the lexical QName is not in scope</exception>
        /// <exception cref="ArgumentException">If the lexical QName is invalid 
        /// (for example, if it contains invalid characters)</exception>
        /// 
        public QName(string lexicalQName, XdmNode element) {
            try {
                NodeInfo node = (NodeInfo) element.value;
                sqName = StructuredQName.fromLexicalQName(lexicalQName, true, true,
                    node.getAllNamespaces());
            }
            catch (Saxon.Hej.trans.XPathException err) {
                throw new ArgumentException(err.getMessage());
            }
        }

        /// <summary>
        /// Construct a <c>QName</c> from an <c>XmlQualifiedName</c> (as defined in the
        /// <c>System.Xml</c> package).
        /// </summary>
        /// <remarks>
        /// Note that an <c>XmlQualifiedName</c> does not contain any prefix, so the result
        /// will always have a prefix of ""
        /// </remarks>
        /// <param name="qualifiedName">The <c>XmlQualifiedName</c></param>
        public QName(XmlQualifiedName qualifiedName) {
            string uri = qualifiedName.Namespace;
            string local = qualifiedName.Name;
            string prefix = string.Empty;
            sqName = new StructuredQName(prefix, uri, local);
        }
        
        /// <summary>
        /// Construct a <c>QName</c> from an <c>XName</c> (as defined in the
        /// <c>System.Xml.Linq</c> package).
        /// </summary>
        /// <remarks>
        /// Note that an <c>XName</c> does not contain any prefix, so the result
        /// will always have a prefix of ""
        /// </remarks>
        /// <param name="name">The <c>XName</c></param>
        public QName(XName name) {
            string uri = name.Namespace == XNamespace.None ? "" : name.Namespace.NamespaceName;
            string local = name.LocalName;
            string prefix = string.Empty;
            sqName = new StructuredQName(prefix, uri, local);
        }

        //  internal constructor from a QNameValue

        internal QName(QNameValue q) {
            sqName = new StructuredQName(q.getPrefix(), q.getNamespaceURI(), q.getLocalName());
        }

        //  internal constructor with StructuredQName object as argument

        internal QName(StructuredQName q) {
            sqName = q;
        }

        /// <summary>
        /// Factory method to construct a <c>QName</c> from a string containing the expanded
        /// QName in Clark notation, that is, <c>{uri}local</c>
        /// </summary>
        /// <remarks>
        /// The prefix part of the <c>QName</c> will be set to an empty string.
        /// </remarks>
        /// <param name="expandedName">The URI in Clark notation: <c>{uri}local</c> if the
        /// name is in a namespace, or simply <c>local</c> if not.</param> 
        public static QName FromClarkName(string expandedName) {
            String namespaceURI;
            String localName;
            if (expandedName[0] == '{') {
                int closeBrace = expandedName.IndexOf('}');
                if (closeBrace < 0) {
                    throw new ArgumentException("No closing '}' in Clark name");
                }

                namespaceURI = expandedName.Substring(1, closeBrace - 1);
                if (closeBrace == expandedName.Length) {
                    throw new ArgumentException("Missing local part in Clark name");
                }

                localName = expandedName.Substring(closeBrace + 1);
            }
            else {
                namespaceURI = "";
                localName = expandedName;
            }

            return new QName("", namespaceURI, localName);
        }


        /// <summary>
        /// Factory method to construct a <c>QName</c> from a string containing the expanded
        /// QName in EQName notation, that is, <c>Q{uri}local</c>
        /// </summary>
        /// <remarks>
        /// The prefix part of the <c>QName</c> will be set to an empty string.
        /// </remarks>
        /// <param name="expandedName">The QName in EQName notation: <c>Q{uri}local</c>. 
        /// For a name in no namespace, either of the
        /// forms <c>Q{}local</c> or simply <c>local</c> are accepted.</param>
        /// <returns> the QName corresponding to the supplied name in EQName notation. This will always
        /// have an empty prefix.</returns>
        public static QName FromEQName(string expandedName) {
            string namespaceURI;
            string localName;
            if (expandedName.StartsWith("Q{")) {
                int closeBrace = expandedName.IndexOf('}');
                if (closeBrace < 0) {
                    throw new ArgumentException("No closing '}' in EQName");
                }

                namespaceURI = expandedName.Substring(2, closeBrace);
                if (closeBrace == expandedName.Length) {
                    throw new ArgumentException("Missing local part in EQName");
                }

                localName = expandedName.Substring(closeBrace + 1);
            }
            else {
                namespaceURI = "";
                localName = expandedName;
            }

            return new QName("", namespaceURI, localName);
        }

        // internal method: Factory method to construct a QName from Saxon's internal <c>StructuredQName</c>
        // representation.

        internal static QName FromStructuredQName(StructuredQName sqn) {
            return new QName(sqn);
        }

        /// <summary>
        /// Validate the <c>QName</c> against the XML rules for valid names.
        /// </summary>
        /// <returns>true if the name is valid, false if not</returns>
        public bool IsValid() {
            if (this.Prefix != string.Empty) {
                if (!NameChecker.isValidNCName(Prefix)) {
                    return false;
                }
            }

            if (!NameChecker.isValidNCName(this.LocalName)) {
                return false;
            }

            return true;
        }

        /// <summary>Get the prefix of the <c>QName</c>. This plays no role in operations such as comparison
        /// of QNames for equality, but is retained (as specified in XPath) so that a string representation
        /// can be reconstructed.
        /// </summary>
        /// <remarks>
        /// Returns the zero-length string in the case of a QName that has no prefix.
        /// </remarks>

        public string Prefix => sqName.getPrefix();

        /// <summary>Get the namespace URI of the <c>QName</c>. Returns "" (the zero-length string) if the
        /// QName is not in a namespace.
        /// </summary>

        public string Uri => sqName.getURI();

        internal NamespaceUri NamespaceUri => sqName.getNamespaceUri();

        /// <summary>Get the local part of the <c>QName</c></summary>

        public string LocalName => sqName.getLocalPart();

        /// <summary>Get the expanded name, as a string using the notation devised by James Clark.
        /// If the name is in a namespace, the resulting string takes the form <c>{uri}local</c>.
        /// Otherwise, the value is the local part of the name.
        /// </summary>

        public string ClarkName {
            get {
                if (Uri.Equals("")) {
                    return LocalName;
                }
                return "{" + Uri + "}" + LocalName;
            }
        }

        /// <summary>Get the expanded name in EQName format, that is <c>Q{uri}local</c>. A no-namespace name is returned as <c>Q{}local</c>.
        /// </summary>
        public string EQName => "Q{" + Uri + "}" + LocalName;

        /// <summary>
        /// Convert the value to a string. The resulting string is the lexical form of the QName,
        /// using the original prefix if there was one.
        /// </summary>
        public override string ToString() {
            if (Prefix.Equals("")) {
                return LocalName;
            }
            return Prefix + ":" + LocalName;
        }

        /// <summary>
        /// Get a hash code for the <c>QName</c>, to support equality matching. This supports the
        /// semantics of equality, which considers only the namespace URI and local name, and
        /// not the prefix.
        /// </summary>
        /// <remarks>
        /// The algorithm for allocating a hash code does not depend on registering the QName 
        /// with the <c>Processor</c>.
        /// </remarks>
        public override int GetHashCode() {
            return sqName.GetHashCode();
        }

        /// <summary>
        /// Test whether two QNames are equal. This supports the
        /// semantics of equality, which considers only the namespace URI and local name, and
        /// not the prefix.
        /// </summary>
        /// <param name="other">The value to be compared with this <c>QName</c>. If this value is not a <c>QName</c>, the
        /// result is always false. Otherwise, it is true if the namespace URI and local name both match.</param>
        public override bool Equals(object other) {
            return other is QName q && sqName.Equals(q.sqName);
        }

        /// <summary>
        /// Convert the value to an <c>XmlQualifiedName</c> (as defined in the
        /// <c>System.Xml</c> package)
        /// </summary>
        /// <remarks>
        /// Note that this loses the prefix.
        /// </remarks>
        public XmlQualifiedName ToXmlQualifiedName() {
            return new XmlQualifiedName(LocalName, Uri);
        }
        
        /// <summary>
        /// Convert the value to an <c>XName</c> (as defined in the
        /// <c>System.Xml.Linq</c> package)
        /// </summary>
        /// <remarks>
        /// Note that this loses the prefix.
        /// </remarks>
        public System.Xml.Linq.XName ToXName() {
            XNamespace ns = Uri == "" ? XNamespace.None : XNamespace.Get(Uri);
            return ns + LocalName;
        }

        // internal method: Convert to a Saxon.Hej.value.QNameValue

        internal QNameValue ToQNameValue() {
            return new QNameValue(sqName.getPrefix(), sqName.getNamespaceUri(), sqName.getLocalPart(), null);
        }

        /// <summary>
        /// Convert the value to an <c>StructuredQName</c>, which is the representation
        /// Saxon uses internally.
        /// </summary>

		public Saxon.Hej.om.StructuredQName GetStructuredQName() {
            return sqName;
        }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited