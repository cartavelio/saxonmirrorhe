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
import java.util.function.Function;

public class PathHelper {
    private static URI rootURI = URI.create("file:///");
    public static boolean debug = false;

    public static Function<Path, Boolean> pathExistsOnFs = (path) -> path.toFile().exists();

    private final Function<Path, Boolean> fsPathVerifier;

    /**
     * Create a new PathHelper, which uses the fsPathVerifier function to verify
     * a path to be resolved.
     *
     * @param fsPathVerifier
     */
    PathHelper(Function<Path, Boolean> fsPathVerifier) {
        this.fsPathVerifier = fsPathVerifier;
    }

    /**
     * Create a new PathHelper, using the default path verifier which requires
     * paths to be resolved to exist on a local filesysteœm.
     */
    PathHelper() {
        this(pathExistsOnFs);
    }
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
    public Source resolveToSource(Configuration config, String cwd, String path, String nature) throws SaxonApiException {
        Source source;
        Path cwdPath = null;
        URI cwdURI = null;
        if (cwd == null || cwd == "") {
            throw new SaxonApiException("C API call error - cwd must be set to an absolute system path");
        }
        try {
            if (isURIWithAllowedScheme(path)) {
                source = resolveURIToSource(config, cwd, path, nature);
            } else {
                source = resolvePathToSource(config, cwd, path, nature);
            }
            if (debug) {
                System.err.println("DEBUG source.getSystemID()=" + source.getSystemId());
            }
            return source;
        } catch (XPathException | NullPointerException ex) {
            if (debug) {
                System.err.println("DEBUG: Exception thrown: " + ex.getMessage());
            }

            throw new SaxonApiException(ex);
        }
    }

    private Source resolvePathToSource(Configuration config, String cwd, String input, String nature) throws SaxonApiException {
        Path cwdPath = Paths.get(cwd);
        Path inputPath = Paths.get(input);

        Path outputPath = cwdPath.resolve(inputPath);
        if (!fsPathVerifier.apply(outputPath)) {
            throw new SaxonApiException("Unable to resolve <" + input + "> into a Source");
        }

        return new StreamSource(outputPath.toUri().toString());
    }

    private Source resolveURIToSource(Configuration config, String cwd, String input, String nature) throws SaxonApiException, XPathException {
        Path cwdPath = Paths.get(cwd);
        URI cwdURI = cwdPath.toUri();
        Source source;

        ResourceRequest request = new ResourceRequest();
        request.baseUri = cwdURI.toString();
        request.relativeUri = input;
        request.uri = URI.create(input).toString();
        request.nature = nature;
        request.purpose = ResourceRequest.ANY_PURPOSE;

        source = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));

        if (source == null) {
            throw new SaxonApiException("Unable to resolve <" + input + "> into a Source");
        }

        return source;
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

    private static String normalisePathToRelativeURI(String path) {
        return rootURI.relativize(new File("/", path).toURI()).toASCIIString();
    }

    private static boolean isURIWithAllowedScheme(String name) {
        return name.startsWith("http:") ||
                name.startsWith("https:") ||
                name.startsWith("file:");
    }
}
