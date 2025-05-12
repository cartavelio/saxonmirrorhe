using System;

namespace Saxon.Api {

    /// <summary>A <c>ResultDocumentHandler</c> can be nominated to handle output
    /// produced by the <c>xsl:result-document</c> instruction in an XSLT stylesheet.
    /// The function is called by the XSLT processor
    /// when an <c>xsl:result-document</c> with an <c>href</c> attribute is evaluated.
    /// </summary>
    /// <param name="href">An absolute or relative URI. This will be the effective value of the 
    /// <c>href</c> attribute of the <c>xsl:result-document</c> in the stylesheet.</param>
    /// <param name="baseUri">The base URI that should be used for resolving the value of
    /// <c>href</c> if it is relative. This will always be the value of the <c>BaseOutputUri</c>
    /// property of the <c>XsltTransformer</c>.</param>
    /// <returns>An <c>IDestination</c> object representing the destination to which
    /// the result document is sent.</returns>
    /// <remarks>
    /// <para>This callback affects any <c>xsl:result-document</c> instruction
    /// executed by the stylesheet, provided that it has an <c>href</c> attribute.</para> 
    /// <para>If no <c>ResultDocumentHandler</c> is nominated (in the
    /// <c>ResultDocumentHandler</c> property of the <c>XsltTransformer</c> or <c>Xslt30Transformer</c>), the output
    /// of <code>xsl:result-document</code> is serialized, and is written to the file
    /// or other resource identified by the URI in the <c>href</c> attribute, resolved
    /// (if it is relative) against the URI supplied in the <c>BaseOutputUri</c> property
    /// of the <c>XsltTransformer</c>.</para>
    /// <para>If a <c>ResultDocumentHandler</c> is nominated, however, it
    /// will be called whenever an <c>xsl:result-document</c>
    /// instruction with an <c>href</c> attribute is evaluated, and the generated result tree
    /// will be passed to the <c>IDestination</c> that it returns.</para>
    /// <para>The XSLT processor will ensure that the stylesheet cannot create
    /// two distinct result documents which are sent to the same URI. It is the responsibility
    /// of the <c>ResultDocumentHandler</c> to ensure that two distinct result documents are
    /// not sent to the same <c>IDestination</c>. Failure to observe this rule can result
    /// in output streams being incorrectly closed.
    /// </para>
    /// <para>Note that more than one result document can be open at the same time,
    /// and that the order of opening, writing, and closing result documents chosen
    /// by the processor does not necessarily bear any direct resemblance to the way
    /// that the XSLT source code is written. SaxonCS processes <c>xsl:result-document</c>
    /// instructions asynchronously, in multiple threads, unless this is suppressed using
    /// the configuration property <c>Feature.ALLOW_MULTITHREADING</c>.</para>
    /// <para>Note also that in the event of a dynamic error occurring in the transformation,
    /// it is unpredictable which result documents have been successfully written before
    /// the failure occurs.</para> 
    /// </remarks>

    public delegate IDestination ResultDocumentHandler(string href, Uri baseUri);
    
}

// Copyright (c) 2021 - 2023 Saxonica Limited