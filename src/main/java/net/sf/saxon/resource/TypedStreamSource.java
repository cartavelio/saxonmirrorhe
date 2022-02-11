package net.sf.saxon.resource;

import javax.xml.transform.stream.StreamSource;

public class TypedStreamSource extends StreamSource {

    private String contentType;

    public void setContentType(String contentType) {
        this.contentType = contentType;
    }
    public String getContentType() {
        return contentType;
    }
}

