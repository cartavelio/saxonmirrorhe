using System;

namespace Saxon.Api {
 


    /// <summary>An <c>IResultDocumentHandler</c> can be nominated to handle output
    /// produced by the <c>xsl:result-document</c> instruction in an XSLT stylesheet.
    /// </summary>
    /// <remarks>
    /// <para>This interface affects any <c>xsl:result-document</c> instruction
    /// executed by the stylesheet, provided that it has an <c>href</c> attribute.</para> 
    /// <para>If no <c>IResultDocumentHandler</c> is nominated (in the
    /// <c>IResultDocumentHandler</c> property of the <c>XsltTransformer</c>), the output
    /// of <code>xsl:result-document</code> is serialized, and is written to the file
    /// or other resource identified by the URI in the <c>href</c> attribute, resolved
    /// (if it is relative) against the URI supplied in the <c>BaseOutputUri</c> property
    /// of the <c>XsltTransformer</c>.</para>
    /// <para>If an <c>IResultDocumentHandler</c> is nominated, however, its
    /// <c>HandleResultDocument</c> method will be called whenever an <c>xsl:result-document</c>
    /// instruction with an <c>href</c> attribute is evaluated, and the generated result tree
    /// will be passed to the <c>XmlDestination</c> returned by that method.</para> 
    /// </remarks>

    public interface IResultDocumentHandler
    {

        /// <summary> Handle output produced by the <c>xsl:result-document</c>
        /// instruction in an XSLT stylesheet. This method is called by the XSLT processor
        /// when an <c>xsl:result-document</c> with an <c>href</c> attribute is evaluated.
        /// </summary>
        /// <param name="href">An absolute or relative URI. This will be the effective value of the 
        /// <c>href</c> attribute of the <c>xsl:result-document</c> in the stylesheet.</param>
        /// <param name="baseUri">The base URI that should be used for resolving the value of
        /// <c>href</c> if it is relative. This will always be the value of the <c>BaseOutputUri</c>
        /// property of the <c>XsltTransformer</c>.</param>
        /// <returns>An <c>XmlDestination</c> to handle the result tree produced by the
        /// <c>xsl:result-document</c> instruction. The <c>Close</c> method of the returned
        /// <c>XmlDestination</c> will be called when the output is complete.</returns>
        /// <remarks>
        /// <para>The XSLT processor will ensure that the stylesheet cannot create
        /// two distinct result documents which are sent to the same URI. It is the responsibility
        /// of the <c>IResultDocumentHandler</c> to ensure that two distinct result documents are
        /// not sent to the same <c>XmlDestination</c>. Failure to observe this rule can result
        /// in output streams being incorrectly closed.
        /// </para>
        /// <para>Note that more than one result document can be open at the same time,
        /// and that the order of opening, writing, and closing result documents chosen
        /// by the processor does not necessarily bear any direct resemblance to the way
        /// that the XSLT source code is written.</para></remarks>
        /**public**/
        XmlDestination HandleResultDocument(string href, Uri baseUri);

    }
}