package net.sf.saxon.resource;

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

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
