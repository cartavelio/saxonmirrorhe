namespace Saxon.Api {
    /// <summary>
    /// Identifies a host language in which XPath expressions appear. Generally used when different error codes
    /// need to be returned depending on the host language.
    /// </summary>
    public enum HostLanguage
    {
        /// <summary>
        /// Indicates that the host language in which the XPath expression appears is unknown
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// Indicates that the host language is XSLT
        /// </summary>
        XSLT,
        /// <summary>
        /// Indicates that the host language is XQuery
        /// </summary>
        XQUERY,
        /// <summary>
        /// Indicates that the host language is XSD (typically used for assertions in XSD 1.1 schema documents)
        /// </summary>
        XML_SCHEMA,
        /// <summary>
        /// Indicates a free-standing XPath expression, typically executed using an API from a language such as Java or C#
        /// </summary>
        XPATH,
        /// <summary>
        /// Indicates an XPath expression acting as an XSLT match pattern
        /// </summary>
        XSLT_PATTERN
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited