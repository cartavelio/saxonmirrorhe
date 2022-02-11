
namespace Saxon.Api {
    
    /// <summary>
    /// An <c>Message</c> represents the output of an <c>xsl:message</c> or <c>xsl:assert</c>
    /// instruction.
    /// </summary>
    public class Message {
        
        /// <summary>
        /// Constructor for use by the <c>xsl:message</c> and <c>xsl:assert</c>
        /// instructions: intended for system use.
        /// </summary>
        /// <param name="content">The content of the message as an XDM document node</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="terminate">True if the transformation will terminate after this message</param>
        /// <param name="location">The location in the stylesheet of the originating instruction</param>
        
        public Message(XdmNode content, QName errorCode, bool terminate, Location location) {
            Content = content;
            ErrorCode = errorCode;
            Terminate = terminate;
            Location = location;
        }

        /// <summary>
        /// The content of the message: always an XDM document node
        /// </summary>
        /// <remarks>
        /// If the message has simple textual content, then <c>ToString</c> will extract this.
        /// </remarks>
        public XdmNode Content { get; }

        /// <summary>
        /// The error code associated with the message. Defaults to <c>err:XTMM9000</c> for
        /// <c>xsl:message</c>, or <c>err:XTMM9001</c> for <c>xsl:assert</c>.
        /// </summary>
        public QName ErrorCode { get; }

        /// <summary>
        /// Indicates whether the transformation will terminate after issuing this message.
        /// True if <c>xsl:message</c> was called with <c>terminate="yes"</c>, or for any
        /// activated call on <c>xsl:assert</c>.
        /// </summary>
        public bool Terminate { get; }

        /// <summary>
        /// Information about the location of the originating <c>xsl:message</c> or
        /// <c>xsl:assert</c> instruction in the stylesheet.
        /// </summary>
        public Location Location { get; }
    
    }
}

// Copyright (c) 2021 Saxonica Limited