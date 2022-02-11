namespace Saxon.Api {
    /// <summary>An <c>IMessageListener</c> can be nominated to handle output
    /// produced by the <c>xsl:message</c> instruction in an XSLT stylesheet.
    /// </summary>
    /// <remarks>
    /// <para>This interface affects any <c>xsl:message</c> instruction
    /// executed by the stylesheet.</para> 
    /// <para>If no <c>IMessageListener</c> is nominated (in the
    /// <c>MessageListener</c> property of the <c>XsltTransformer</c>), the output
    /// of <code>xsl:message</code> is serialized, and is written to standard error
    /// output stream.</para>
    /// <para>If an <c>IMessageListener</c> is nominated, however, its
    /// <c>Message</c> method will be called whenever an <c>xsl:message</c>
    /// instruction is evaluated.</para> 
    /// <para>The <c>MessageListener2</c> interface differs from <c>MessageListener</c>
    /// in allowing the error code supplied to <c>xsl:message</c> to be made available.</para>
    /// </remarks>

    public interface IMessageListener
    {

        /// <summary>Handle the output of an <c>xsl:message</c> instruction
        /// in the stylesheet
        /// </summary>
        /// <param name="content">a document node representing the message content. Note the putput of <c>xsl:message</c>
        /// is always an XML document node. It can be flattened to obtain the stringvalue if required by calling
        /// <c>GetStringValue()</c></param>
        /// <param name="errorCode">a QName containing the error supplied to the called on xsl:message, or its default of err:XTM9000.</param>
        /// <param name="terminate">Set to true if <c>terminate ='yes'</c> was specified or to false otherwise</param>
        /// <param name="location">an object that contains the location of the <c>xsl:message</c> This provides access to the URI of the stylesheet
        /// module and the line number of the <c>xsl:message</c></param>

        /**public**/
        void Message(XdmNode content, QName errorCode,  bool terminate, IXmlLocation location);

    }
}