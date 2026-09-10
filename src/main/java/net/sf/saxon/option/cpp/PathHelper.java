package net.sf.saxon.option.cpp;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.DirectResourceResolver;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.trans.CommandLineOptions;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Path;
import java.nio.file.Paths;

public class PathHelper {
    private static URI rootURI = URI.create("file:///");
    public static boolean debug = false;

    /**
     * Normalise a path string. If an absolute path, a file URI is generated,
     * if an existing HTTP or file URI string, then this is converted directly
     * into a URI. Returns null if handed a relative path.
     * @param path
     * @return
     * @throws URISyntaxException
     */
    public static URI normalisePathToURI(String path) throws URISyntaxException {
        if (isURIWithAllowedScheme(path)) return new URI(path);

        File file = new File(path);
        if (file.isAbsolute()) {
            return file.toURI();
        } else {
            return null;
        }
    }


    /**
     * Resolve a supplied relative or absolute file path, or URI, to a Source.
     * Relative paths will be resolved against the supplied current working
     * directory, file and http URIs will be used unaltered.
     * The nature param should be one of the strings provided in the
     * ResourceRequest.*_NATURE constants - an IANA type registration URI, or a
     * namespace (for XSLT and XSD).
     *
     * @param config   - The current Configuration object
     * @param cwd      - Current working directory, which must be non-null and an absolute system path
     * @param path     - the file path or URI
     * @param nature   - one of the ResourceRequest.*_NATURE strings
     * @return
     * @throws SaxonApiException if error occurs when resolving the file path
     */
    public static Source resolveToSource(Configuration config, String cwd, String path, String nature) throws SaxonApiException {
        Source source;
        Path cwdPath = null;
        URI cwdURI = null;
        if (cwd == null || cwd == "") {
            throw new SaxonApiException("C API call error - cwd must be set to an absolute system path");
        }
        try {
            cwdPath = Paths.get(cwd);
            cwdURI = cwdPath.toUri();

            URI normalisedPathURI = normalisePathToURI(path);
            ResourceRequest request = new ResourceRequest();
            request.baseUri = cwdURI.toASCIIString();
            if (normalisedPathURI == null) {
                request.relativeUri = path;
                request.uri = cwdPath.resolve(path).toUri().toASCIIString();
            } else {
                request.uri = normalisedPathURI.toASCIIString();
            }
            request.nature = nature;
            request.purpose = ResourceRequest.ANY_PURPOSE;

            source = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));

            if (debug) {
                System.err.println("DEBUG source.getSystemID()=" + source.getSystemId());
            }
            if (source == null) {
                throw new SaxonApiException("Unable to resolve <" + path + "> into a Source");
            }

            return source;
        } catch (XPathException | NullPointerException | URISyntaxException exception) {
            if (debug) {
                System.err.println("DEBUG: Exception thrown: " + exception.getMessage());
            }
            throw new SaxonApiException(exception);
        }
    }

    /**
     * Resolve a supplied relative or absolute file path, or URI string, to a Java URI.
     * Relative paths will be resolved against the supplied current working
     * directory, file and http URIs will be used unaltered.
     *
     * @param config   - The current Configuration object
     * @param cwd      - Current working directory
     * @param path     - the file path or URI
     * @return
     * @throws SaxonApiException if error occurs when resolving the file path
     */
    public static URI resolveToURI(Configuration config, String cwd, String path) throws SaxonApiException {
        Path cwdPath = null;
        try {
            if (cwd != null) {
                cwdPath = Paths.get(cwd);
                // should we throw if cwdPath is not absolute?
            }
            if (debug) {
                System.err.println("cwd = " + cwd);
                System.err.println("path = " + path);
            }

            URI normalisedPathURI = normalisePathToURI(path);
            if (normalisedPathURI != null) return normalisedPathURI;
            if (cwdPath == null) throw new SaxonApiException("Supplied cwd value null, cannot resolve relative path");

            return cwdPath.resolve(path).toUri();
        } catch (URISyntaxException ex) {
            if(debug) {
                System.err.println("DEBUG: Exception thrown: " + ex.getMessage());
                System.err.println("DEBUG: Using resolver fallback");
                ex.printStackTrace();
            }
            throw new SaxonApiException(ex);
        }
    }

    public static URI pathToURI(Configuration config, String path) throws SaxonApiException {
        Path currentRelativePath = Paths.get("");
        String crp = currentRelativePath.toAbsolutePath().toString();
        URI uri = PathHelper.resolveToURI(config, crp , path);
        return uri;
    }

    private static String normalisePathToRelativeURI(String path) {
        return rootURI.relativize(new File("/", path).toURI()).toASCIIString();
    }

    private static boolean isURIWithAllowedScheme(String path) {
        try {
            URL maybeURL = new URL(path);
            switch (maybeURL.getProtocol()) {
                case "http":
                case "https":
                case "file":
                    return true;
                default:
                    return false;
            }
        } catch (MalformedURLException e) {
            return false;
        }
    }
}
