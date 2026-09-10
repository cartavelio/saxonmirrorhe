package net.sf.saxon.junit;

import java.io.File;
import java.net.URI;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Properties;

public class Testdata {
    private static final String envVarName = "SAXON_TEST_DATA";
    private static TestdataDir dirPath;
    private static TestdataDir bugsDir;
    private static TestdataDir tempDatadir;
    private static TestdataDir xmarkDatadir;
    private static TestdataDir qt4testsDatadir;
    private static TestdataDir qt4SpecsDatadir;

    private static TestdataDir testdata() {
        if (dirPath == null) {
            String dirPathString = System.getenv(envVarName);
            if (dirPathString == null || dirPathString.isEmpty()) {
                throw new RuntimeException("Cannot get testdata path - environment var " + envVarName + " not set or empty.");
            }
            Path d = Paths.get(System.getenv(envVarName));
            if (!d.toFile().isDirectory()) {
                throw new RuntimeException("Cannot get testdata path - environment var " + envVarName + " value " + dirPathString + " is not a directory or does not exist.");
            }
            dirPath = new TestdataDir(d);
        }
        return dirPath;
    }

    public static Path dir() {
        return testdata().dir();
    }

    public static URI baseURI() {
        return testdata().baseURI();
    }

    public static Path path(String filePath) {
        return testdata().path(filePath);
    }

    public static File file(String filePath) {
        return testdata().file(filePath);
    }

    public static URI fileUri(String filePath) {
        return testdata().fileUri(filePath);
    }

    public static String fileUriString(String filePath) {
        return testdata().fileUriString(filePath);
    }

    public static String pathString(String filePath) {
        return testdata().pathString(filePath);
    }

    public static TestdataDir testDir(String dirPath) {
        return new TestdataDir(path(dirPath));
    }

    public static TestdataDir bugs() {
        if (bugsDir == null) {
            String envar = "SAXON_BUGS_DATA";
            String env = System.getenv(envar);
            Path d;
            if (env == null) {
                // Assume the current directory is the root of the project
                d = Paths.get(System.getProperty("user.dir")).resolve("src/test/bugs");
                checkExists(d, envar);
            } else {
                d = Paths.get(env);
                checkExists(d, envar);
            }
            bugsDir = new TestdataDir(d);
        }
        return bugsDir;
    }

    public static TestdataDir temp() {
        if (tempDatadir == null) {
            try {
                String envar = "SAXON_TEMP_DATA";
                String env = System.getenv(envar);
                if (env == null) {
                    // Assume the current directory is the root of the project
                    Path buildPath = Paths.get(System.getProperty("user.dir")).resolve("build");
                    checkExists(buildPath, envar);

                    Path dirPath = buildPath.resolve("temp");
                    File dir = dirPath.toFile();
                    if (!dir.exists()) {
                        if (!dir.mkdir()) {
                            throw new RuntimeException("Failed to create " + dir.getAbsolutePath() + "; set " + envar);
                        }
                    }

                    checkExists(dirPath, envar);
                    tempDatadir = new TestdataDir(dirPath);
                } else {
                    Path dirPath = Paths.get(env);
                    checkExists(dirPath, envar);
                    tempDatadir = new TestdataDir(dirPath);
                }
            } catch (RuntimeException e) {
                System.err.println(e.getMessage()); // otherwise we somehow lose the diagnostics
                throw e;
            }
        }
        return tempDatadir;
    }

    private static void checkExists(Path path, String envar) {
        if (!path.toFile().exists() || !path.toFile().isDirectory()) {
            throw new RuntimeException("Failed to find " + path + "; set " + envar);
        }
    }

    // TODO: These methods should reflect the move to using submodules
    public static TestdataDir qt4Tests() {
        if (qt4testsDatadir == null) {
            File dir = null;
            String envar = "W3C_QT4_TEST_SUITE_DIR";
            String env = System.getenv(envar);
            if (env == null) {
                // Assume the current directory is the root of the project
                throw new RuntimeException("Environment variable W3C_QT4_TEST_SUITE_DIR is not set");
            } else {
                dir = new File(env);
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
                }
            }

            qt4testsDatadir = new TestdataDir(dir.toPath());
        }
        return qt4testsDatadir;
    }

    public static TestdataDir qt4Specs() {
        if  (qt4SpecsDatadir == null) {
            File dir = null;
            String envar = "W3C_QT4_SPECS_DIR";
            String env = System.getenv(envar);
            if (env == null) {
                // Assume that the specs live as a sibling of the qt4 tests
                Path qt4TestsPath = qt4Tests().dir();
                Path assumedQt4SpecsPath = qt4TestsPath.resolve("../qtspecs");

                if (!assumedQt4SpecsPath.toFile().isDirectory()) {
                    // Assume the current directory is the root of the project
                    throw new RuntimeException("Environment variable " + envar + " is not set, and there is no 'qtspecs' dir sibling of the W3C_QT4_TEST_DIR dir");
                }
                dir = assumedQt4SpecsPath.toFile();
            } else {
                dir = new File(env);
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
                }
            }
            qt4SpecsDatadir = new TestdataDir(dir.toPath());
        }
        return qt4SpecsDatadir;
    }

    public static TestdataDir xmark() {
        if (xmarkDatadir == null) {
            File dir = null;
            String envar = "SAXON_XMARK_DATA";
            String env = System.getenv(envar);
            if (env == null) {
                // Assume the current directory is the root of the project
                dir = new File(System.getProperty("user.dir") + "/src/test/xmark");
                if (!dir.exists() || !dir.isDirectory()) {
                    Properties p = System.getProperties();
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; set " + envar);
                }
            } else {
                dir = new File(env);
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
                }
            }
            xmarkDatadir = new TestdataDir(dir.toPath());
        }
        return xmarkDatadir;
    }
}
