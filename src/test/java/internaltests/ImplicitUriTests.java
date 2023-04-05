package internaltests;

import net.sf.saxon.trans.CommandLineOptions;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class ImplicitUriTests {
    @Test
    public void testFile() {
        assertTrue(CommandLineOptions.isImplicitURI("file:///path/to/file"));
    }

    @Test
    public void testHttp() {
        assertTrue(CommandLineOptions.isImplicitURI("http://host/path/to/file"));
    }

    @Test
    public void testHttps() {
        assertTrue(CommandLineOptions.isImplicitURI("https://host/path/to/file"));
    }

    @Test
    public void testClasspath() {
        assertTrue(CommandLineOptions.isImplicitURI("classpath:/path/to/file"));
    }

    @Test
    public void testJar() {
        assertTrue(CommandLineOptions.isImplicitURI("jar:file:/path/to/file!sublocation"));
    }

    @Test
    public void testOther() {
        assertFalse(CommandLineOptions.isImplicitURI("/no/scheme"));
    }

    @Test
    public void testDrive() {
        // Windows
        assertFalse(CommandLineOptions.isImplicitURI("c:/tmp/file"));
    }
}
