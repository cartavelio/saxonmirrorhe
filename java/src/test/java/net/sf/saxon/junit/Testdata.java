package net.sf.saxon.junit;

import java.io.File;
import java.net.URI;
import java.nio.file.Path;
import java.nio.file.Paths;

public class Testdata {
    private static final String envVarName = "SAXON_TEST_DATA";
    private static Path dirPath;

    public static Path dir() {
        if (dirPath == null) {
            String dirPathString = System.getenv(envVarName);
            if (dirPathString == null || dirPathString.isEmpty()) {
                throw new RuntimeException("Cannot get testdata path - environment var " + envVarName + " not set or empty.");
            }
            dirPath = Paths.get(System.getenv(envVarName));
            if (!dirPath.toFile().isDirectory()) {
                throw new RuntimeException("Cannot get testdata path - environment var " + envVarName + " value " + dirPathString + " is not a directory or does not exist.");
            }
        }
        return dirPath;
    }

    public static URI baseURI() {
        return dir().toUri();
    }

    public static Path path(String filePath) {
        return dir().resolve(filePath);
    }

    public static File file(String filePath) {
        return path(filePath).toFile();
    }

    public static String pathString(String filePath) {
        return file(filePath).toString();
    }
}
