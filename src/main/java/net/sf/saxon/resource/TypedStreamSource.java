package net.sf.saxon.resource;

import javax.xml.transform.stream.StreamSource;

/**
 * A <code>StreamSource</code> augmented with ContentType information,
 * which potentially provides the media type and encoding
 */

public class TypedStreamSource extends StreamSource {

    private String contentType;

    /**
     * Set the content type
     * @param contentType the content type, in the format of the HTTP content type header
     */
    public void setContentType(String contentType) {
        this.contentType = contentType;
    }

    /**
     * Get the content type
     *
     * @return contentType the content type, in the format of the HTTP content type header; or null if
     * not known
     */
    public String getContentType() {
        return contentType;
    }
}

