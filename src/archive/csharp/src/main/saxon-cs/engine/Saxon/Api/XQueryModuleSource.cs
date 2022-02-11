using System;

namespace Saxon.Api {
    /// <summary>
    /// A representation of the source (un-compiled) form of an XQuery module.
    /// </summary>
    /// <remarks>
    /// <para>The content of the module is delivered as a string. This means that decoding of a binary stream
    /// is the responsibility of the creator of the <c>XQueryModuleSource</c> object.</para>
    /// </remarks>
    public class XQueryModuleSource 
    {
        /// <summary>
        /// The base URI of the XQuery module
        /// </summary>
        public Uri BaseUri { get; init; }
        
        /// <summary>
        /// The content of the XQuery module, as a string
        /// </summary>
        public string Content { get; init; }
    }
}

// Copyright (c) 2021 Saxonica Limited