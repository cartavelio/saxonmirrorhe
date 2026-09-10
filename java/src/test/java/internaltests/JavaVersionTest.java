package internaltests;


import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

public class JavaVersionTest {
    @Test
    public void testJavaVersion() {
        // This test just exists to assure that we can tell what version of Java was used for the tests
        System.err.println(System.getProperty("java.version"));
        Assertions.assertTrue(true);
    }
}
