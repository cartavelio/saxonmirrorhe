////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.lib;

import net.sf.saxon.Configuration;
import net.sf.saxon.resource.*;
import net.sf.saxon.trans.XPathException;


import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import java.io.*;
import java.net.*;
import java.nio.charset.*;
import java.util.zip.GZIPInputStream;

/**
 * Default implementation of the UnparsedTextURIResolver, used if no other implementation
 * is nominated to the Configuration. This implementation
 *  * handles anything that the java URL class will handle, plus the <code>classpath</code>
 *  * URI scheme defined in the Spring framework, and the <code>data</code> URI scheme defined in
 *  * RFC 2397.
 */

public class StandardUnparsedTextResolver
        //#if CSHARP==false
        implements UnparsedTextURIResolver
        //#endif
{

    private boolean debug = false;

    /**
     * Set debugging on or off. In debugging mode, information is written to System.err
     * to trace the process of deducing an encoding.
     *
     * @param debug set to true to enable debugging
     */

    public void setDebugging(boolean debug) {
        this.debug = debug;
    }

    /**
     * Resolve the URI passed to the XSLT unparsed-text() function, after resolving
     * against the base URI.
     *
     * @param absoluteURI the absolute URI obtained by resolving the supplied
     *                    URI against the base URI
     * @param encoding    the encoding requested in the call of unparsed-text(), if any. Otherwise null.
     * @param config      The configuration. Provided in case the URI resolver
     *                    needs it.
     * @return a Reader, which Saxon will use to read the unparsed text. After the text has been read,
     *         the close() method of the Reader will be called.
     * @throws net.sf.saxon.trans.XPathException
     *          if any failure occurs
     * @since 8.9
     */

    @Override
    public Reader resolve(URI absoluteURI, String encoding, Configuration config) throws XPathException {
        URL absoluteURL;

        Logger err = config.getLogger();
        if (debug) {
            err.info("unparsed-text(): processing " + absoluteURI);
            err.info("unparsed-text(): requested encoding = " + encoding);
        }
        if (!absoluteURI.isAbsolute()) {
            throw new XPathException("Resolved URI supplied to unparsed-text() is not absolute: " + absoluteURI.toString(),
                    "FOUT1170");
        }
        if (!config.getAllowedUriTest().test(absoluteURI)) {
            throw new XPathException("URI scheme '" + absoluteURI.getScheme() + "' has been disallowed");
        }

        // Do this early in case it returns a data: or classpath: URI.
        try {
            Source resolved = config.getSaxonResourceResolver().resolve(absoluteURI.toString(), null);
            if (resolved != null) {
                absoluteURI = new URI(resolved.getSystemId());
            }
        } catch (TransformerException te) {
            // nevermind, we'll just fall through and try the old way
        } catch (URISyntaxException mue) {
            // let this fall through too; it shouldn't be possible, but if it is,
            // try the old way.
        }

        InputStream inputStream = null;
        String contentEncoding;
        boolean isXmlMediaType = false;
        if (absoluteURI.getScheme().equals("data")) {
            Resource resource;
            try {
                resource = DataURIScheme.decode(absoluteURI);
            } catch (IllegalArgumentException e) {
                throw new XPathException("Invalid URI in 'data' scheme: " + e.getMessage(), "FOUT1170");
            }
            if (resource instanceof BinaryResource) {
                byte[] octets = ((BinaryResource) resource).getData();
                inputStream = new ByteArrayInputStream(octets);
                contentEncoding = "utf-8";

            } else {
                assert resource instanceof UnparsedTextResource;
                return new StringReader(((UnparsedTextResource) resource).getContent());
            }
            if (encoding == null) {
                encoding = contentEncoding;
            }
            String mediaType = resource.getContentType();
            isXmlMediaType = (mediaType.startsWith("application/") || mediaType.startsWith("text/")) &&
                    (mediaType.endsWith("/xml") || mediaType.endsWith("+xml"));
        } else if (absoluteURI.getScheme().equals("classpath")) {
            InputStream is = config.getDynamicLoader().getResourceAsStream(absoluteURI.toString().substring(10));
            if (is != null) {
                try {
                    if (encoding == null) {
                        encoding = "UTF-8";
                    }
                    return new InputStreamReader(is, encoding);
                } catch (UnsupportedEncodingException e) {
                    throw new AssertionError(e);
                }
            }
        } else {
            try {
                absoluteURL = absoluteURI.toURL();
            } catch (MalformedURLException mue) {
                XPathException e = new XPathException("Cannot convert absolute URI "
                                                              + absoluteURI + " to URL", mue);
                e.setErrorCode("FOUT1170");
                throw e;
            }
            try {
                URLConnection connection = null;
                try {
                    connection = Loader.urlConnection(absoluteURL);
                } catch (IOException ioe) {
                    if (debug) {
                        err.error("unparsed-text(): connection failure on " + absoluteURL
                                          + ". " + ioe.getMessage());
                    }
                    XPathException xpe = new XPathException("Failed to read input file " + absoluteURL, ioe);
                    xpe.setErrorCode("FOUT1170");
                    throw xpe;
                }

                inputStream = connection.getInputStream();
                contentEncoding = connection.getContentEncoding();

                if ("gzip".equals(contentEncoding)) {
                    inputStream = new GZIPInputStream(inputStream);
                }
                if (debug) {
                    err.info("unparsed-text(): established connection " +
                                     ("gzip".equals(contentEncoding) ? " (zipped)" : ""));
                }


                if (!inputStream.markSupported()) {
                    inputStream = new BufferedInputStream(inputStream);
                }

                // Get any external (HTTP) encoding label.
                isXmlMediaType = false;

                // The file:// URL scheme gives no useful information...
                if (!"file".equals(connection.getURL().getProtocol())) {

                    // Use the contentType from the HTTP header if available
                    String contentType = connection.getContentType();
                    if (debug) {
                        err.info("unparsed-text(): content type = " + contentType);
                    }
                    if (contentType != null) {
                        String mediaType;
                        int pos = contentType.indexOf(';');
                        if (pos >= 0) {
                            mediaType = contentType.substring(0, pos);
                        } else {
                            mediaType = contentType;
                        }
                        mediaType = mediaType.trim();
                        if (debug) {
                            err.info("unparsed-text(): media type = " + mediaType);
                        }
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
                            if (debug) {
                                err.info("unparsed-text(): charset = " + charset.trim());
                            }
                            encoding = charset.trim();
                        }
                    }
                }


                try {
                    if (encoding == null || isXmlMediaType) {
                        encoding = EncodingDetector.inferStreamEncoding(inputStream, debug ? err : null);
                        if (debug) {
                            err.info("unparsed-text(): inferred encoding = " + encoding);
                        }
                    }
                } catch (IOException e) {
                    encoding = "UTF-8";
                }

            } catch (IOException ioe) {
                throw new XPathException(ioe.getMessage(), "FOUT1170");
            } catch (IllegalCharsetNameException icne) {
                throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
            } catch (UnsupportedCharsetException uce) {
                throw new XPathException("Invalid encoding name: " + encoding, "FOUT1190");
            }

        }

        // The following is necessary to ensure that encoding errors are not recovered.
        Charset charset2 = Charset.forName(encoding);
        CharsetDecoder decoder = charset2.newDecoder()
                .onMalformedInput(CodingErrorAction.REPORT)
                .onUnmappableCharacter(CodingErrorAction.REPORT);
        return new BufferedReader(new InputStreamReader(inputStream, decoder));

    }


}

