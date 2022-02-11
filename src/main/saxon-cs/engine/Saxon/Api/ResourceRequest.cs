namespace Saxon.Api {
    
    /// <summary>
    /// A bundle of information made available when requesting resource resolution using
    /// a <c>ResourceResolver</c>. Any of these properties may be absent.
    /// </summary>
    public class ResourceRequest {
        
        public const string TEXT_NATURE = "https://www.iana.org/assignments/media-types/text/plain";
        public const string XQUERY_NATURE = "https://www.iana.org/assignments/media-types/application/xquery";
        public const string XSLT_NATURE = NamespaceConstant.XSLT;
        public const string XSD_NATURE = NamespaceConstant.SCHEMA;
        public const string XML_NATURE = "https://www.iana.org/assignments/media-types/application/xml";
        
        /// <summary>
        /// The URI to be dereferenced. If the request was for a relative URI reference, this will
        /// be the absolute URI after resolving against the base URI if known; if no base URI is known,
        /// it will be the URI as requested. 
        /// </summary>
        public System.Uri Uri { get; set; }
        
        /// <summary>
        /// The base URI that was used to resolve any relative URI, if known.
        /// </summary>
        public System.Uri BaseUri { get; set; }
        
        /// <summary>
        /// The relative URI that was actually requested, where applicable.
        /// </summary>
        public string RelativeUri { get; set; }
        
        /// <summary>
        /// The encoding specified in the request, explicitly or implicitly. For example, this will
        /// be supplied for a call on <c>fn:unparsed-text</c> if an encoding argument was supplied;
        /// for <c>fn:parse-json</c> it will be set to UTF-8 unconditionally.
        /// </summary>
        /// <remarks>
        /// <para>If a specific encoding is requested, the preferred format for the response is a
        /// <c>TextResource</c> containing a <c>Reader</c> or explicit string-valued <c>Content</c>.
        /// If no encoding is requested for a text resource, the preferred format is a <c>TextResource</c>
        /// containing a (binary) <c>Stream</c>: but a <c>TextResource</c> containing a <c>Reader</c>
        /// or <c>Content</c> is acceptable if the data is already in character form so that no decoding
        /// is needed.
        /// </para>
        /// </remarks>
        public System.Text.Encoding Encoding { get; set; }
        
        /// <summary>
        /// The <c>Nature</c> of the resource, as defined in the RDDL specifications (based on the <c>role</c>
        /// attribute in XLink). Some of the allowed values are provided as constants, for example
        /// <c>TEXT_NATURE</c>, <c>XSLT_NATURE</c>, <c>XQUERY_NATURE</c>.
        /// </summary>
        public string Nature { get; set; }
        
        /// <summary>
        /// The <c>Purpose</c> of the request, as defined in the RDDL specifications (based on the <c>arcrole</c>
        /// attribute in XLink).
        /// </summary>
        public string Purpose { get; set; }
        
        /// <summary>
        /// This boolean flag is set to true when the URI takes the form of a namespace URI (rather than a location
        /// hint). Specifically, it is set when resolving an <c>import module</c> declaration in XQuery when only
        /// the module namespace is known, and when resolving an <c>xsl:import</c>, or an XML Schema import in XSLT
        /// or XQuery, if only the target namespace of the required schema document is known.
        /// </summary>
        
        public bool UriIsNamespace { get; set; }


    }
}