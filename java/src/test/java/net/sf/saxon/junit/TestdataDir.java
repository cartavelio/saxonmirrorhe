package net.sf.saxon.junit;

import java.io.File;
import java.net.URI;
import java.nio.file.Path;

public class TestdataDir {
    private final Path baseDir;
    private final URI baseURI;

    TestdataDir(Path baseDir) {
        this.baseDir = baseDir;
        this.baseURI = baseDir.toUri();
    }

    public Path dir() {
        return baseDir;
    }

    public URI baseURI() {
        return this.baseURI;
    }

    public String baseURIString() {
        return this.baseURI.toASCIIString();
    }

    public Path path(String filePath) {
        return baseDir.resolve(filePath);
    }

    public String pathString(String filePath) {
        return path(filePath).toString();
    }

    public File file(String filePath) {
        return path(filePath).toFile();
    }

    public URI fileUri(String uriPath) {
        return baseURI().resolve(uriPath);
    }

    public String fileUriString(String uriPath) {
        return fileUri(uriPath).toASCIIString();
    }

    public URI dirUri(String uriPath) {
        String fixedUriPath;
        if (uriPath.endsWith("/") || uriPath.isEmpty()) {
            fixedUriPath = uriPath;
        } else {
            fixedUriPath = uriPath + "/";
        }
        return baseURI().resolve(fixedUriPath);
    }
}
