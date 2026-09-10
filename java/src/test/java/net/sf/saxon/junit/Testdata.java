package net.sf.saxon.junit;

import java.io.File;
import java.net.URI;
import java.nio.file.Path;
import java.nio.file.Paths;

public class Testdata {
    private static final String envVarName = "SAXON_TEST_DATA";
    private static Path dirPath;
    private static Path bugsPath;
    private static Path tempPath;

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

    private static Path bugsDir() {
        if (bugsPath == null) {
            String envar = "SAXON_BUGS_DATA";
            String env = System.getenv(envar);
            if (env == null) {
                // Assume the current directory is the root of the project
                bugsPath = Paths.get(System.getProperty("user.dir")).resolve("src/test/bugs");
                checkExists(bugsPath, envar);
            } else {
                bugsPath = Paths.get(env);
                checkExists(bugsPath, envar);
            }
        }
        return bugsPath;
    }

    private static Path tempDir() {
        if (tempPath == null) {
            try {
                String envar = "SAXON_TEMP_DATA";
                String env = System.getenv(envar);
                if (env == null) {
                    // Assume the current directory is the root of the project
                    Path buildPath = Paths.get(System.getProperty("user.dir")).resolve("build");
                    checkExists(buildPath, envar);

                    File dir = buildPath.resolve("temp").toFile();
                    if (!dir.exists()) {
                        if (!dir.mkdir()) {
                            throw new RuntimeException("Failed to create " + dir.getAbsolutePath() + "; set " + envar);
                        }
                    }

                    tempPath = buildPath.resolve("temp");
                    checkExists(tempPath, envar);
                } else {
                    tempPath = Paths.get(env);
                    checkExists(tempPath, envar);
                }
            } catch (RuntimeException e) {
                System.err.println(e.getMessage()); // otherwise we somehow lose the diagnostics
                throw e;
            }
        }
        return tempPath;
    }

    private static void checkExists(Path path, String envar) {
        if (!path.toFile().exists() || !path.toFile().isDirectory()) {
            throw new RuntimeException("Failed to find " + path + "; set " + envar);
        }
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

    public static Path bugsPath(String filePath) {
        return bugsDir().resolve(filePath);
    }

    public static File bugsFile(String filePath) {
        return bugsPath(filePath).toFile();
    }

    public static Path tempPath(String filePath) {
        return tempDir().resolve(filePath);
    }

    public static File tempFile(String filePath) {
        return tempPath(filePath).toFile();
    }

    public static String pathString(String filePath) {
        return file(filePath).toString();
    }
}
