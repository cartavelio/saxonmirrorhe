using System;
using System.IO;
using System.Text;

namespace Saxon.Api {
    /// <summary>
    /// Dereferences a URI passed to calls on <c>unparsed-text()</c>, <c>unparsed-text-lines()</c>,
    /// <c>unparsed-text-available()</c>, and <c>json-doc()</c>.
    /// </summary>
    /// <param name="uri">The URI of the resource to be retrieved. If a relative URI is supplied
    /// in the XPath function call, this will be resolved against the static base URI before calling
    /// this resolver.</param>
    /// <param name="encoding">The encoding, if known. This will be supplied when an encoding
    /// name is given in the XPath function call; in the case of <c>json-doc</c> it defaults to utf-8.</param>
    /// <returns>A <c>TextReader</c> that delivers the content of the resource. Saxon will consume the
    /// content of the <c>TextReader</c> and close it after use.
    /// </returns>
    /// <remarks>
    /// <para>The XPath specifications imposes rules on how the encoding is inferred if not supplied.
    /// For the application to conform with the XPath specification, a user-written <c>TextResolver</c> must
    /// implement these rules. Saxon does not enforce this.</para>
    /// </remarks>
    /// 
    public delegate TextReader TextResolver(Uri uri, Encoding encoding);
}

// Copyright (c) 2021 Saxonica Limited