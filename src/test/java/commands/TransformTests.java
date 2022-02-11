package commands;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Transform;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Initializer;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.SystemLogger;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on Transform CommandLine options
 */
public class TransformTests {

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

        private final RetentiveLogger logger;

        public VerifyingTransform() {
            this.logger = new RetentiveLogger();
        }

        public String transform(String[] args) {
            try {
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                // Capture System.out
                System.setOut(new PrintStream(baos));
                super.doTransform(args);
                // Revert change to System.out
                System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out)));
                return baos.toString("utf-8");
            } catch (Exception e) {
                fail();
                return "failure";
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
    public void testTransformation() {

        String[] args = {"-t", "-xsl:" + ConfigTest.DATA_DIR + "play.xsl", "-s:" + ConfigTest.DATA_DIR + "othello.xml", "dir=" + ConfigTest.DATA_DIR + "samples/output"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertEquals(16, new File(ConfigTest.DATA_DIR + "samples/output").listFiles().length);
    }

    @Test
    public void testTransformationMessages() {

        String[] args = {
                "-t",
                "-xsl:" + ConfigTest.DATA_DIR + "play.xsl",
                "-s:" + ConfigTest.DATA_DIR + "othello.xml",
                "-quit:off",
                "dir=" + ConfigTest.DATA_DIR + "samples/output"};
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            assertTrue(vt.outputMatches(s -> s.startsWith("SaxonJ-EE 11")), "SaxonVersion");
            assertTrue(vt.outputMatches(s -> s.startsWith("Java version")), "JavaVersion");
            assertTrue(vt.outputMatches(s -> s.startsWith("Using license serial number")), "License");
            assertTrue(vt.outputMatches(s -> s.matches("Processing.*othello.xml")), "SourceFile");
            assertTrue(vt.outputMatches(s -> s.startsWith("Tree built in")), "BuildTime");
            assertTrue(vt.outputMatches(s -> s.startsWith("Execution time")), "ExecutionTime");
            assertTrue(result.startsWith("The Tragedy of Othello, the Moor of Venice"));
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertEquals(16, new File(ConfigTest.DATA_DIR + "samples/output").listFiles().length);
    }


    @Test
    public void testThreadTransformation() {

        String[] args = {"-xsl:" + ConfigTest.DATA_DIR + "threads/test.xsl", "-s:" + ConfigTest.DATA_DIR + "threads/input", "-o:" + ConfigTest.DATA_DIR + "threads/output", "-threads:2"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertEquals(5, new File(ConfigTest.DATA_DIR + "threads/output").listFiles().length);

    }

    @Test
    public void testDefaultSerialization() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            System.setOut(new PrintStream(baos));   // redirect System.out
            String strResult = vt.transform(args);
            assertTrue(strResult.toLowerCase().startsWith("<!doctype html><html>"), "html");
            assertTrue(strResult.contains("<head><meta"), "head-meta");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testOverriddenSerialization() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl",
                "!indent=yes"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            assertTrue(result.toLowerCase().startsWith("<!doctype html><html>"), "html");
            assertFalse(result.contains("<head><meta"), "head-meta");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSerializationOptions() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl",
                "!method=xml"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            assertTrue(result.startsWith("<?xml"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test saxon:line-number on different documents
     */

    @Test
    public void testLineNumberPrincipal() {
        String[] args = {
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "line-numbering.xsl",
                "-it:a",
                "-l",
                "!method=xml"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("<a>4</a>"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testLineNumberParam() {
        String[] args = {
                "-xsl:" + ConfigTest.DATA_DIR + "line-numbering.xsl",
                "-it:b",
                "-l:on",
                "+doc=" + ConfigTest.DATA_DIR + "books.xml",
                "!method=xml"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("<b>4</b>"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testLineNumberDoc() {
        String[] args = {
                "-xsl:" + ConfigTest.DATA_DIR + "line-numbering.xsl",
                "-it:c",
                "-l",
                "!method=xml"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("<c>4</c>"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that values for static stylesheet parameters can be supplied on the command line
     */

    @Test
    public void testStaticParams() {

        String[] args = {
                "-xsl:" + ConfigTest.DATA_DIR + "use-static.xsl",
                "-s:" + ConfigTest.DATA_DIR + "bills.xml",
                "date=2013-01-01",
                "?now=current-time()"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        // Expected output (but with @time being the current time):
//        <?xml version="1.0" encoding="UTF-8"?><Bills date="2013-01-01" time="22:02:52.954+01:00">
//        <Bill No="10030" Name="John Doe" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10009" Name="Jane Jones" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10100" Name="Bob Smith" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10030" Name="John Doe" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        </Bills>
    }

    /**
     * Test that the xml:stylesheet PI works
     */

    @Test
    public void testAssociated() {
        String[] args = {
                "-a:on",
                "--version-warning:off",
                "-s:" + ConfigTest.DATA_DIR + "bills.xml",
                "s=s-actual",
                "d=d-actual"};
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            assertTrue(result.contains("s=\"s-actual\""));
            assertTrue(result.contains("d=\"d-actual\""));
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        // Expected output:
//            <?xml version="1.0" encoding="UTF-8"?><Bills s="s-actual" d="d-actual"><Bill No="10009" Name="Jane Jones" NewBill="1">
//            </Bill><Bill No="10030" Name="John Doe" NewBill="1">
//            </Bill><Bill No="10030" Name="John Doe" NewBill="0">
//            </Bill><Bill No="10100" Name="Bob Smith" NewBill="1">
//            </Bill></Bills>
    }

    /**
     * Test suppression of extension functions
     */

    @Test
    public void testDisallowExtensions() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "samples/data/books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "samples/styles/books-pe.xsl"
        };
        try {
            Transform.main(args);
            fail();
        } catch (Exception e) {
            // OK
        }
    }

    @Test
    public void testTimingProfile() {
        String[] args = {
                "-ext:off",
                "-TP:" + ConfigTest.DATA_DIR + "temp/profile.html",
                "-o:" + ConfigTest.DATA_DIR + "temp/books.html",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testJsonMethod() {
        String[] args = {
                "-ext:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books-to-json.xsl"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("\"author\":\"Jane Austen\""));
            assertTrue(result.contains("\"dimensions\":[8.3,5.7,1.1]"));
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, two packages, library package first
     */
    @Test
    public void testPackages001() {
        // TODO: what is this test trying to achieve?
        String[] args = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-lib:" + ConfigTest.DATA_DIR + "packages/pack-002.xsl",
                "-xsl:" + ConfigTest.DATA_DIR + "packages/pack-001.xsl"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("Jasper Fforde"));
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, two packages, library package second
     */
    @Test
    public void testPackages002() {
        String[] args = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-lib:" + ConfigTest.DATA_DIR + "packages/pack-001.xsl",
                "-xsl:" + ConfigTest.DATA_DIR + "packages/pack-002.xsl"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("DANGGGG"));
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, first stylesheet uses saxon:next-in-chain
     */
    @Test
    public void testNextInChain() {
        String[] args = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "next-in-chain/test.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "next-in-chain/main.xsl"
        };
        try {
            VerifyingTransform vt = new VerifyingTransform();
            String result = vt.transform(args);
            System.err.println(result);
            assertTrue(result.contains("<second>principal</second>"));
        } catch (Exception e) {
            fail();
        }
    }

    @SuppressWarnings("ResultOfMethodCallIgnored")
    @Test
    public void testDirectory() {
        if (!new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out").exists()) {
            new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out").mkdir();
        }
        if (new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out/A.xsl.xml").exists()) {
            new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out/A.xsl.xml").delete();
        }
        if (new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out/B.xsl.xml").exists()) {
            new File(ConfigTest.DATA_DIR + "sandpit/welch-ser/out/B.xsl.xml").delete();
        }
        String[] args = {
                "-t",
                "-o:" + ConfigTest.DATA_DIR + "sandpit/welch-ser/out",
                "-s:" + ConfigTest.DATA_DIR + "welch-ser",
                "-xsl:" + ConfigTest.DATA_DIR + "identity.xsl"
        };
        try {
            Transform.main(args);
            assertTrue(new File(ConfigTest.DATA_DIR + "welch-ser/out/A.xsl.xml").exists(), "A exists");
            assertTrue(new File(ConfigTest.DATA_DIR + "welch-ser/out/B.xsl.xml").exists(), "B exists");
        } catch (Exception e) {
            fail();
        } finally {
            new File(ConfigTest.DATA_DIR + "welch-ser/out").delete();
        }
    }

    @SuppressWarnings("unused")
    public static class RequireStreaming implements Initializer {
        @Override
        public void initialize(Configuration config) {
            config.setSourceResolver((source, config1) -> {
                if (source instanceof NodeInfo && source.getSystemId().endsWith("caffo/Input.xml")) {
                    fail("Source tree for caffo/Input.xml has been built in memory");
                }
                return config1.resolveSource(source, config1);
            });
        }
    }


    @Test
    public void testStreaming() {

        String[] args = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "caffo/Input.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "caffo/Transform2Streamable.xslt",
                "-o:" + ConfigTest.DATA_DIR + "sandpit/caffoOut.xml",
                "-init:commands.TransformTests$RequireStreaming"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertTrue(new File(ConfigTest.DATA_DIR + "sandpit/caffoOut.xml").exists(), "Streaming output exists");
    }


    @Test
    public void testStreamingWithSEF() { // bug 4760

        String[] args = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "caffo/Input.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "caffo/Transform2Streamable.xslt",
                "-export:" + ConfigTest.DATA_DIR + "sandpit/caffo.xsl.sef",
                "-nogo"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertTrue(new File(ConfigTest.DATA_DIR + "sandpit/caffo.xsl.sef").exists(), "Streaming output exists");

        String[] args2 = {
                "-t",
                "-s:" + ConfigTest.DATA_DIR + "caffo/Input.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "sandpit/caffo.xsl.sef",
                "-o:" + ConfigTest.DATA_DIR + "sandpit/caffoOut.xml",
                "-init:commands.TransformTests$RequireStreaming"};
        try {
            Transform.main(args2);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertTrue(new File(ConfigTest.DATA_DIR + "sandpit/caffoOut.xml").exists(), "Streaming output exists");
    }

    @Test
    public void testMessageToLogger() {
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message><a nr='{.}'/></xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            p.getUnderlyingConfiguration().setLogger(new SystemLogger(java.util.logging.Logger.getAnonymousLogger()));
            p.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            t.setDestination(p.newSerializer(System.out));
            t.transform();
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }


}

