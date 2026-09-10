package net.sf.saxon.resource;

/**
 * Represents a content type as it appears in HTTP headers, parsed to extract
 * two properties: (a) a boolean indicating whether the media type is XML, and
 * (b) the character encoding.
 */


public class ParsedContentType {

    public boolean isXmlMediaType;
    public String encoding;

    public ParsedContentType(String contentType) {
        String mediaType;
        int pos = contentType.indexOf(';');
        if (pos >= 0) {
            mediaType = contentType.substring(0, pos);
        } else {
            mediaType = contentType;
        }
        mediaType = mediaType.trim();
        isXmlMediaType = (mediaType.startsWith("application/") || mediaType.startsWith("text/")) &&
                (mediaType.endsWith("/xml") || mediaType.endsWith("+xml"));

        String charset = "";
        pos = contentType.toLowerCase().indexOf("charset");
        if (pos >= 0) {
            pos = contentType.indexOf('=', pos + 7);
            if (pos >= 0) {
                charset = contentType.substring(pos + 1);
            }
            if ((pos = charset.indexOf(';')) > 0) {
                charset = charset.substring(0, pos);
            }

            // attributes can have comment fields (RFC 822)
            if ((pos = charset.indexOf('(')) > 0) {
                charset = charset.substring(0, pos);
            }
            // ... and values may be quoted
            if ((pos = charset.indexOf('"')) > 0) {
                charset = charset.substring(pos + 1,
                                            charset.indexOf('"', pos + 2));
            }
            encoding = charset.trim();
        }
    }
}

