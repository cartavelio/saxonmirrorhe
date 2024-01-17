package net.sf.saxon.option.cpp;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.DirectResourceResolver;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.SaxonApiException;
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

    public static URI normalisePathToURI(String path) throws URISyntaxException {
        if (isURIWithAllowedScheme(path)) return new URI(path);

        File file = new File(path);
        if(file.isAbsolute()) {
            return file.toURI();
        } else {
            return null;
        }
    }

    public static String normalisePathToRelativeURI(String path) {
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

    public static Source resolveToSource(Configuration config, String cwd, String path, String nature) throws SaxonApiException {
        Source source;
        Path cwdPath = null;
        URI cwdURI = null;
        try {
            if (cwd != null) {
                cwdPath = Paths.get(cwd);
                cwdURI = cwdPath.toUri();
                // should we throw if cwdPath is not absolute?
            }

            URI normalisedPathURI = normalisePathToURI(path);
            ResourceRequest request = new ResourceRequest();
            request.baseUri = (cwdURI != null ? cwdURI.toString() : null);
            if (normalisedPathURI == null) {
                request.uri = (cwdURI != null ? cwdPath.resolve(path).toUri().toASCIIString() : normalisePathToRelativeURI(path));
            }
            request.nature = nature;

            source = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));

            if (debug) {
                System.err.println("DEBUG source.getSystemID()=" + source.getSystemId());
            }
            if (source == null) {
                throw new SaxonApiException("Unable to resolve <" + path + "> into a Source");
            }

            return source;
        } catch (XPathException | NullPointerException | URISyntaxException exception) {
            if(debug) {
                System.err.println("DEBUG: Exception thrown: "+exception.getMessage());
                System.err.println("DEBUG: Using resolver fallback");
            }
            throw new SaxonApiException(exception);
        }
    }
}
