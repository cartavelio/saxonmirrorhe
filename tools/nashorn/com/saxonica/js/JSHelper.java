package com.saxonica.js;

import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.resource.ResourceLoader;
import net.sf.saxon.trans.XPathException;


import java.io.*;
import java.net.*;
import java.nio.charset.*;
import java.util.zip.GZIPInputStream;

public class JSHelper {

    public static String jsBase;

    /**
     * Fetch a resource (typically a JSON file) relative to the base URI of the Javascript code
     * @param href relative URI of the resource to be loaded
     * @return the text of the resource
     */

    public static String fetchResource(String href) {
        try {
            String uri = resolveUri(href, jsBase);
            InputStream stream = new URL(uri).openStream();
            Reader reader = new BufferedReader(new InputStreamReader(stream, StandardCharsets.UTF_8));
            return consume(reader);
        } catch (Exception err) {
            String message = err.getMessage();
            if (!message.contains(href)) {
                message = "Failed to read " + href + ": " + message;
            }
            return "\ufeff" + err.getClass().getName() + ": " + message;
        }
    }

    public static String consume(Reader reader) throws IOException {
        StringBuilder sb = new StringBuilder(2048);
        char[] buffer = new char[2048];
        boolean first = true;
        int actual;
        while (true) {
            actual = reader.read(buffer, 0, 2048);
            if (actual < 0) {
                break;
            }
            if (first) {
                first = false;
                if (buffer[0] == '\ufeff') {
                    // don't include the BOM in the result
                    sb.append(buffer, 1, actual - 1);
                } else {
                    sb.append(buffer, 0, actual);
                }
            } else {
                sb.append(buffer, 0, actual);
            }
        }
        reader.close();
        return sb.toString();
    }

    public static String fetchFile(String file, String enc) {
        try {
            //System.out.println("JSHelper file " + file);
            InputStream stream = new FileInputStream(new File(file));
//            String encoding = encodingForFile(file, enc);
//            Reader reader = new BufferedReader(new InputStreamReader(stream, Charset.forName(encoding).newDecoder()));
            Reader reader = getReader(new File(file).toURI(), enc);
            return consume(reader);
        } catch (MalformedInputException err) {
            return "\ufeffInput file " + file + " cannot be decoded as " + enc + ": " + err.getMessage();
        } catch (Exception err) {
            String message = err.getMessage();
            if (!message.contains(file)) {
                message = "Failed to read " + file + ": " + message;
            }
            return "\ufeff" + err.getClass().getName() + ": " + message;
        }
    }

    /*@NotNull*/
    public static String resolveUri(String relative, String base) throws URISyntaxException {
        return ResolveURI.makeAbsolute(relative, base).toASCIIString();
    }

    public static Reader getReader(URI absoluteURI, String encoding) throws XPathException {
        URL absoluteURL;
        try {
            absoluteURL = absoluteURI.toURL();
        } catch (MalformedURLException mue) {
            XPathException e = new XPathException("Cannot convert absolute URI to URL", mue);
            e.setErrorCode("FOUT1170");
            throw e;
        }
        try {
            URLConnection connection;
            try {
                connection = ResourceLoader.urlConnection(absoluteURL);
            } catch (IOException ioe) {
                XPathException xpe = new XPathException("Failed to read input file", ioe);
                xpe.setErrorCode("FOUT1170");
                throw xpe;
            }

            InputStream is = connection.getInputStream();
            String contentEncoding = connection.getContentEncoding();

            if ("gzip".equals(contentEncoding)) {
                is = new GZIPInputStream(is);
            }
            try {

                if (!is.markSupported()) {
                    is = new BufferedInputStream(is);
                }

                // Get any external (HTTP) encoding label.
                boolean isXmlMediaType = false;

                // The file:// URL scheme gives no useful information...
                if (!"file".equals(connection.getURL().getProtocol())) {

                    // Use the contentType from the HTTP header if available
                    String contentType = connection.getContentType();
                    if (contentType != null) {
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

                if (encoding == null || encoding.isEmpty() || isXmlMediaType) {
                    // Try to detect the encoding from the start of the content
                    is.mark(100);
                    byte[] start = new byte[100];
                    int read = is.read(start, 0, 100);
                    is.reset();
                    encoding = inferEncoding(start, read);
                }

            } catch (IOException e) {
                encoding = "UTF-8";
            }

            //}

            // The following is necessary to ensure that encoding errors are not recovered.
            Charset charset = Charset.forName(encoding);
            CharsetDecoder decoder = charset.newDecoder();
            decoder = decoder.onMalformedInput(CodingErrorAction.REPORT);
            decoder = decoder.onUnmappableCharacter(CodingErrorAction.REPORT);
            return new BufferedReader(new InputStreamReader(is, decoder));


        } catch (IOException ioe) {
            throw new XPathException(ioe.getMessage(), "FOUT1170");
        } catch (IllegalCharsetNameException icne) {
            throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
        } catch (UnsupportedCharsetException uce) {
            throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
        }

    }

    /**
     * Infer the encoding of a file by reading the first few bytes of the file
     *
     * @param start the first few bytes of the file
     * @param read  the number of bytes that have been read
     * @return the inferred encoding
     */

    private static String inferEncoding(byte[] start, int read) {
        if (read >= 2) {
            if (ch(start[0]) == 0xFE && ch(start[1]) == 0xFF) {
                return "UTF-16";
            } else if (ch(start[0]) == 0xFF && ch(start[1]) == 0xFE) {
                return "UTF-16LE";
            }
        }
        if (read >= 3) {
            if (ch(start[0]) == 0xEF && ch(start[1]) == 0xBB && ch(start[2]) == 0xBF) {
                return "UTF-8";
            }
        }
        if (read >= 4) {
            if (ch(start[0]) == '<' && ch(start[1]) == '?' &&
                    ch(start[2]) == 'x' && ch(start[3]) == 'm' && ch(start[4]) == 'l') {
                StringBuilder sb = new StringBuilder(read);
                for (int b = 0; b < read; b++) {
                    sb.append((char) start[b]);
                }
                String p = sb.toString();
                int v = p.indexOf("encoding");
                if (v >= 0) {
                    v += 8;
                    while (v < p.length() && " \n\r\t=\"'".indexOf(p.charAt(v)) >= 0) {
                        v++;
                    }
                    sb.setLength(0);
                    while (v < p.length() && p.charAt(v) != '"' && p.charAt(v) != '\'') {
                        sb.append(p.charAt(v++));
                    }
                    return sb.toString();
                }
            }
        } else if (read > 0 && start[0] == 0 && start[2] == 0 && start[4] == 0 && start[6] == 0) {
            return "UTF-16";
        } else if (read > 1 && start[1] == 0 && start[3] == 0 && start[5] == 0 && start[7] == 0) {
            return "UTF-16LE";
        }
        // If all else fails, assume UTF-8
        return "UTF-8";
    }

    private static int ch(byte b) {
        return ((int) b) & 0xff;
    }

//    public static String encodingForFile(String fileName, String enc) {
//        if (enc.isEmpty()) {
//
//        }
//        // A hack that knows about the encoding of some files in the test suite
//        if (fileName.endsWith("id/HighUnicode2.xml") ||
//                fileName.endsWith("unparsed-text/invalid-xml.xml")) {
//            return "utf-16";
//        } else {
//            return enc;
//        }
//    }

}


