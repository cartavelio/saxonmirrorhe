package jaxptest;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.ProfessionalTransformerFactory;
import com.saxonica.config.pe.ConfigurationReaderPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.ConfigurationReader;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.XMLConstants;
import javax.xml.transform.*;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TemplatesHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.*;


/**
 *  Test use of configuration files
 */
public class ConfigTest {
    private String DATA_DIR = null;
    private String TEMP_DIR = null;
    private String BUGS_DIR = null;
    private String XMARK_DIR = null;
    private String QT4_TESTS_DIR = null;

    public ConfigTest() {
        // The directories above used to be static final, but that had the nasty consequence
        // that a failure of those methods caused the ConfigTest constructor to fail which
        // in turn was quite tricky to debug, especially in a remote CI environment.
        // Norm reworked this class, and related classes, so that the values are constructed
        // once, but not during the constructor.
    }

    public String getDataDir() {
        if (DATA_DIR == null) {
            DATA_DIR = junitDataDirectory();
        }
        return DATA_DIR;
    }

    public String getTempDir() {
        if (TEMP_DIR == null) {
            TEMP_DIR = junitTempDirectory();
        }
        return TEMP_DIR;
    }

    public String getBugsDir() {
        if (BUGS_DIR == null) {
            BUGS_DIR = junitBugsDirectory();
        }
        return BUGS_DIR;
    }

    public String getXMarkDir() {
        if (XMARK_DIR == null) {
            XMARK_DIR = junitXMarkDirectory();
        }
        return XMARK_DIR;
    }

    public String getQt4TestsDir() {
        if (QT4_TESTS_DIR == null) {
            QT4_TESTS_DIR = qt4TestsDirectory();
        }
        return QT4_TESTS_DIR;
    }

    public String getQt4SpecsDir() {
        String testDir = getQt4TestsDir();
        return testDir.replace("qt4tests", "qtspecs");
    }

    private static String junitDataDirectory() {
        File dir = null;
        String envar = "SAXON_TEST_DATA";
        String env = System.getenv(envar);
        if (env == null) {
            // Assume the current directory is the root of the project
            dir = new File(System.getProperty("user.dir") + "/src/test/testdata");
            if (!dir.exists() || !dir.isDirectory()) {
                throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; set " + envar);
            }
        } else {
            dir = new File(env);
            if (!dir.exists() || !dir.isDirectory()) {
                throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
            }
        }
        String path = dir.getAbsolutePath().replace("\\", "/");
        if (!path.endsWith("/")) {
            path += "/";
        }
        return path;
    }

    private static String junitTempDirectory() {
        File dir = null;
        try {
            dir = null;
            String envar = "SAXON_TEMP_DATA";
            String env = System.getenv(envar);
            if (env == null) {
                // Assume the current directory is the root of the project
                dir = new File(System.getProperty("user.dir") + "/build");
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find build in " + dir.getAbsolutePath() + "; set " + envar);
                }
                dir = new File(System.getProperty("user.dir") + "/build/temp");
                if (!dir.exists()) {
                    if (!dir.mkdir()) {
                        throw new RuntimeException("Failed to create " + dir.getAbsolutePath() + "; set " + envar);
                    }
                }
                dir = new File(System.getProperty("user.dir") + "/build/temp");
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; set " + envar);
                }
            } else {
                dir = new File(env);
                if (!dir.exists() || !dir.isDirectory()) {
                    throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
                }
            }
        } catch (RuntimeException e) {
            System.err.println(e.getMessage()); // otherwise we somehow lose the diagnostics
            throw e;
        }
        String path = dir.getAbsolutePath().replace("\\", "/");
        if (!path.endsWith("/")) {
            path += "/";
        }
        return path;
    }

    private static String junitBugsDirectory() {
        File dir = null;
        String envar = "SAXON_BUGS_DATA";
        String env = System.getenv(envar);
        if (env == null) {
            // Assume the current directory is the root of the project
            dir = new File(System.getProperty("user.dir") + "/src/test/bugs");
            if (!dir.exists() || !dir.isDirectory()) {
                throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; set " + envar);
            }
        } else {
            dir = new File(env);
            if (!dir.exists() || !dir.isDirectory()) {
                throw new RuntimeException("Failed to find " + dir.getAbsolutePath() + "; bad " + envar);
            }
        }
        String path = dir.getAbsolutePath().replace("\\", "/");
        if (!path.endsWith("/")) {
            path += "/";
        }
        return path;
    }

    private static String junitXMarkDirectory() {
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
        String path = dir.getAbsolutePath().replace("\\", "/");
        if (!path.endsWith("/")) {
            path += "/";
        }
        return path;
    }

    private static String qt4TestsDirectory() {
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
        String path = dir.getAbsolutePath().replace("\\", "/");
        if (!path.endsWith("/")) {
            path += "/";
        }
        return path;
    }

    @Test
    public void testGeneralConfiguration() {
        try {
            Configuration config = new ConfigurationReader().makeConfiguration(
                    new StreamSource(getDataDir() + "config.xml"));
            assertTrue(config.isLineNumbering(), "lineNumbering?");
            assertEquals(Boolean.TRUE, config.getConfigurationProperty(Feature.USE_TYPED_VALUE_CACHE), "tvc?");
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testWhitespaceDefault() {
        Configuration config = new Configuration();
        assertEquals("ignorable", config.getConfigurationProperty(Feature.STRIP_WHITESPACE));
    }

    @Test
    public void testJaxpConfiguration() {
        try {
            TransformerFactoryImpl factory = new TransformerFactoryImpl();
            factory.setAttribute(FeatureKeys.CONFIGURATION_FILE, getDataDir() + "config.xml");
            Configuration config = factory.getConfiguration();
            assertTrue(config.isLineNumbering(), "lineNumbering?");
            assertEquals(Boolean.TRUE, config.getConfigurationProperty(Feature.USE_TYPED_VALUE_CACHE), "tvc?");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testCollationConfiguration() {
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration'>\n" +
                        "<global defaultCollation='http://uclc.com/'/>\n" +
                        "<collations>\n" +
                            "<collation uri='http://uclc.com/' case-order='upper-first'/>\n" +
                            "<collation uri='http://lcuc.com/' case-order='lower-first'/>\n" +
                        "</collations>\n" +
                    "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XPathCompiler compiler = proc.newXPathCompiler();
            XPathExecutable e1 = compiler.compile("compare('Alpha','alpha','http://uclc.com/')");
            XdmAtomicValue result1 = (XdmAtomicValue)e1.load().evaluateSingle();
            assertEquals(-1, result1.getLongValue(), "e1 result");
            XPathExecutable e2 = compiler.compile("compare('Alpha','alpha','http://lcuc.com/')");
            XdmAtomicValue result2 = (XdmAtomicValue)e2.load().evaluateSingle();
            assertEquals(+1, result2.getLongValue(), "e2 result");

            XQueryCompiler qcompiler = proc.newXQueryCompiler();
            XQueryExecutable e3 = qcompiler.compile("compare('Alpha','alpha','http://uclc.com/')");
            XdmAtomicValue result3 = (XdmAtomicValue)e3.load().evaluateSingle();
            assertEquals(-1, result3.getLongValue(), "e3 result");
            XQueryExecutable e4 = qcompiler.compile("compare('Alpha','alpha','http://lcuc.com/')");
            XdmAtomicValue result4 = (XdmAtomicValue)e4.load().evaluateSingle();
            assertEquals(+1, result4.getLongValue(), "e4 result");

            String stylesheet = "<a xmlns:x='http://www.w3.org/1999/XSL/Transform' x:version='2.0' \n" +
                    "default=\"{compare('Alpha','alpha')}\" " +
                    "uclc=\"{compare('Alpha','alpha','http://uclc.com/')}\" " +
                    "lcuc=\"{compare('Alpha','alpha','http://lcuc.com/')}\"/>";
            XsltCompiler xc = proc.newXsltCompiler();
            XsltTransformer t = xc.compile(new StreamSource(new StringReader(stylesheet))).load();
            StringWriter out = new StringWriter();
            t.setSource(new StreamSource(new StringReader(stylesheet)));
            Serializer ser = proc.newSerializer(out);
            t.setDestination(ser);
            t.transform();
            assertTrue(out.toString().contains("default=\"-1\""));
            assertTrue(out.toString().contains("uclc=\"-1\""));
            assertTrue(out.toString().contains("lcuc=\"1\""));
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testSerializationConfiguration() {
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='PE'>\n" +
                        "<serialization\n" +
                            "        method=\"xml\"\n" +
                            "        indent=\"yes\"\n" +
                            "        omit-xml-declaration='yes'\n" +
                            "        cdata-section-elements='ccc:c' xmlns:ccc='http://c.com'\n" +
                            "        saxon:indent-spaces=\"8\"\n" +
                            "        xmlns:saxon=\"http://saxon.sf.net/\"/>" +
                    "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XQueryCompiler qcompiler = proc.newXQueryCompiler();
            XQueryExecutable e3 = qcompiler.compile("<a><b/><c:c xmlns:c='http://c.com'>CCC</c:c></a>");
            StringWriter sw3 = new StringWriter();
            Serializer s3 = proc.newSerializer(sw3);
            XQueryEvaluator ev3 = e3.load();
            ev3.setDestination(s3);
            ev3.run();
            assertEquals("<a>\n        <b/>\n        <c:c xmlns:c=\"http://c.com\"><![CDATA[CCC]]></c:c>\n</a>\n", sw3.toString(), "e3 result");

            String stylesheet = "<a xmlns:x='http://www.w3.org/1999/XSL/Transform' x:version='2.0' \n" +
                    "><b/><c:c xmlns:c='http://c.com'>CCC</c:c></a>";
            XsltCompiler xc = proc.newXsltCompiler();
            XsltTransformer t = xc.compile(new StreamSource(new StringReader(stylesheet))).load();
            StringWriter out = new StringWriter();
            t.setSource(new StreamSource(new StringReader(stylesheet)));
            Serializer ser = proc.newSerializer(out);
            t.setDestination(ser);
            t.transform();
            System.err.println(out);
            System.err.println(sw3);
            assertEquals(out.toString(), sw3.toString(), "xslt result");
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }





    @Test
    public void testSchemaConfiguration() {
        try {
            String config =
                        "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='EE'>\n" +
                            "<global timing='true'/>\n" +
                            "<xsd useXsiSchemaLocation='false'/>\n" +
                            "<resources>\n" +
                                "<schemaDocument>file:///" + getDataDir() + "books.xsd</schemaDocument>\n" +
                            "</resources>\n" +
                        "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XsdCompiler sm = proc.newXsdCompiler();
            SchemaValidator v = sm.emptySchema().getValidator();
            v.validate(new StreamSource(new File(getDataDir() + "books.xml")));
            try {
                v.validate(new StreamSource(new File(getDataDir() + "config.xml")));
                fail("invalidity not detected");
            } catch (SaxonApiException verr) {
                System.err.println("Failed as expected: " + verr.getMessage());
                // OK, failure was expected
            }
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testSchemaConfigurationInHE() {
        // Bug 2731
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='true'/>\n" +
                            "<xsd useXsiSchemaLocation='false' occurrenceLimits='100,250'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Configuration property xsd/@occurrenceLimits"));
            assertTrue(err.getMessage().contains("Property occurrenceLimits is not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }

    @Test
    public void testInvalidPropertyValue() {
        // Bug 2731
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='maybe'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Invalid configuration property global/@timing"));
            //assertTrue(err.getMessage().contains("Property not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }

    @Test
    public void testWrongNamespace() {
        // Bug 4064
        try {
            String config =
                    "<configuration xmlns='http://not.saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='maybe'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Configuration"));
            assertTrue(err.getMessage().contains("namespace"));
            //assertTrue(err.getMessage().contains("Property not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }


    @Test
    public void testSqlExtensionPE() {
        try {
            javax.xml.transform.TransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory();
            tf.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");
            tf.setAttribute(net.sf.saxon.lib.FeatureKeys.CONFIGURATION_FILE, getDataDir() + "config-sql.xml");
            tf.newTemplates(new StreamSource(new File(getDataDir() + "books-sql.xsl")));
        } catch (TransformerException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testSqlExtensionPEWorkaround() {
        try {
            javax.xml.transform.TransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory();
            tf.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");

            ConfigurationReaderPE reader = new ConfigurationReaderPE();
            Configuration config = reader.makeConfiguration(new StreamSource(new File(getDataDir() + "config-sql.xml")));
            tf.setAttribute(FeatureKeys.CONFIGURATION, config);
            tf.newTemplates(new StreamSource(new File(getDataDir() + "books-sql.xsl")));
        } catch (TransformerException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testProfConfProblem() {

        // Bug 2609 - occurred using a TemplatesHandler with a ProfessionalConfiguration

        try {
            ProfessionalConfiguration pConfig = new ProfessionalConfiguration();
            SAXTransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory(pConfig);

            //This line is required as our product code uses it deal with multiple XSLT implementations.
            //Here in this program it may be redundant but cannot remove in product code.
            TemplatesHandler templatesHandler = tf.newTemplatesHandler();
            assertNotNull(templatesHandler);
        } catch (TransformerConfigurationException tce) {
            System.out.println("TFC...");
            tce.printStackTrace();
            fail();
        }
    }

    // Test  XMLConstants.FEATURE_SECURE_PROCESSING under XSLT

    private void testXsltSecureProcessing(Source xml, Source xsl, boolean expectFailure) {
        try {
            TransformerFactory factory = new ProfessionalTransformerFactory();
            factory.setFeature(XMLConstants.FEATURE_SECURE_PROCESSING, true);
            Transformer t = factory.newTransformer(xsl);
            StringWriter sw = new StringWriter();
            t.transform(xml, new StreamResult(sw));
            if (expectFailure) {
                fail("Unexpected success");
            }
        } catch (TransformerException err) {
            //assertTrue("message", err.getMessage().contains(failure));
            if (!expectFailure) {
                fail(err.getMessage());
            } else {
                System.err.println("Failed as expected: " + err.getMessage());
            }
            // OK, failure expected
        }
    }

    @Test
    public void testXsltSecureProcessing1() {
        // Calling extension functions is insecure
        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template match='/'><xsl:value-of select='Q{java:java.lang.System}getSystemProperty(\"user.dir\")'/>" +
                "</xsl:template></xsl:transform>";
        String xml = "<a/>";
        testXsltSecureProcessing(new StreamSource(new StringReader(xml)),
                                 new StreamSource(new StringReader(xsl)), true);
    }

    @Test
    public void testXsltSecureProcessing2() {
        // Calling doc() is NOT prevented
        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template match='/'><xsl:value-of select='doc(static-base-uri())'/>" +
                "</xsl:template></xsl:transform>";
        String xml = "<a/>";
        String base = new File(getDataDir() + "config.xml").toURI().toASCIIString();
        testXsltSecureProcessing(new StreamSource(new StringReader(xml), base),
                                 new StreamSource(new StringReader(xsl), base), false);
    }



}

