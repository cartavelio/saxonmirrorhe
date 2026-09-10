package resolvers;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Transform;
import net.sf.saxon.lib.Logger;
import org.junit.jupiter.api.Test;

import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;

import static org.junit.Assert.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class CommandTests {
    private final ConfigTest configTest = new ConfigTest();
    private static class RetentiveLogger extends Logger {

        private final List<String> output = new ArrayList<>();

        @Override
        public void println(String message, int severity) {
            output.add(message);
        }

        private boolean outputMatches(Predicate<String> condition) {
            return output.stream().anyMatch(condition);
        }

    }

    private static class VerifyingTransform extends Transform {

        private final CommandTests.RetentiveLogger logger;

        public VerifyingTransform() {
            this.logger = new CommandTests.RetentiveLogger();
        }

        public String transform(String[] args) {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            // Capture System.out
            System.setOut(new PrintStream(baos));
            super.doTransform(args);
            // Revert change to System.out
            System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out)));
            try {
                return baos.toString("utf-8");
            } catch (UnsupportedEncodingException ex) {
                throw new RuntimeException(ex.getMessage());
            }
        }

        @Override
        protected void initializeConfiguration(Configuration config) {
            config.setLogger(logger);
        }

        private boolean outputMatches(Predicate<String> condition) {
            return logger.outputMatches(condition);
        }

    }

    @Test
    public void testWithoutCatalog() {
        String[] args = {
                "-xsl:" + configTest.getDataDir() + "resolvers/style.xsl",
                "-s:" + configTest.getDataDir() + "resolvers/input.xml",
                "-quit:off"
        };
        try {
            CommandTests.VerifyingTransform vt = new CommandTests.VerifyingTransform();
            vt.transform(args);
            fail();
        } catch (Exception e) {
            // expected
        }
    }

    @Test
    public void testWithCatalog() {
        String[] args = {
                "-xsl:" + configTest.getDataDir() + "resolvers/style.xsl",
                "-s:" + configTest.getDataDir() + "resolvers/input.xml",
                "-catalog:" + configTest.getDataDir() + "resolvers/catalog.xml",
                "-quit:off"
        };
        try {
            CommandTests.VerifyingTransform vt = new CommandTests.VerifyingTransform();
            String result = vt.transform(args);
            assertTrue(result.contains("Hello, world."));
        } catch (Exception e) {
            fail();
        }
    }

}
