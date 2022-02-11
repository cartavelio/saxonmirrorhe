namespace Saxon.Api {
    /// <summary>
    /// Identifies a host language in which XPath expressions appear. Generally used when different error codes
    /// need to be returned depending on the host language.
    /// </summary>
    public enum HostLanguage
    {
        XSLT,
        XQUERY,
        XML_SCHEMA,
        XPATH,
        XSLT_PATTERN
    }
}