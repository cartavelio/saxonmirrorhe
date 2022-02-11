package net.sf.saxon.s9api;

/**
 * A {@code Message} represents the output of an {@code xsl:message} or
 * {@code xsl:assert} instruction.
 * The content of a message is always an XDM document node (in the common case
 * of purely textual messages, the document node will have a single text node
 * child).
 */

public class Message {

    private final XdmNode _content;
    private final QName _errorCode;
    private final boolean _terminate;
    private final Location _location;

    /**
     * Construct a Message. This constructor is used internally by the xsl:message
     * instruction.
     */

    public Message(XdmNode content, QName errorCode, boolean terminate, Location location) {
        this._content = content;
        this._errorCode = errorCode;
        this._terminate = terminate;
        this._location = location;
    }

    /**
     * Get the content of the message
     * @return the content of the message as an XDM document node
     */

    public XdmNode getContent() {
        return _content;
    }

    /**
     * Get the error code associated with the message. If no error
     * code was supplied in the call of {@code xsl:message} or {@code xsl:assert},
     * the default code {@code XTMM9000} or {@code XTMM9001} is used.
     * @return the error code
     */

    public QName getErrorCode() {
        return _errorCode;
    }

    /**
     * Ask whether <code>terminate="yes"</code> was specified on the
     * call of {@code xsl:message}. For an {@code xsl:assert} instruction
     * the value is always true.
     * @return true if the transformation will terminate after the message
     * has been output
     */

    public boolean isTerminate() {
        return _terminate;
    }

    /**
     * Get the location of the {@code xsl:message} or {@code xsl:assert}
     * instruction within the containing stylesheet
     * @return the location of the originating instruction
     */

    public Location getLocation() {
        return _location;
    }


}

