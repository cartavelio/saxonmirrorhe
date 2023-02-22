namespace Saxon.Api {
    /// <summary>
    /// Identifies a host language in which XPath expressions appear. Generally used when different error codes
    /// need to be returned depending on the host language.
    /// </summary>
    public enum HostLanguage
    {
        UNKNOWN,
        XSLT,
        XQUERY,
        XML_SCHEMA,
        XPATH,
        XSLT_PATTERN
    }
}

// Copyright (c) 2021 Saxonica Limited