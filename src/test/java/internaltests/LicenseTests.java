package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.LicenseException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.StringReader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * A number of tests to ensure that certain constructs require a valid license,
 * especially after export/import
 */

public class LicenseTests {

    private void noLicenseNeeded(String xsl) {
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        config.disableLicensing();
        Processor proc = new Processor(config);
        XsltCompiler compiler = proc.newXsltCompiler();
        try {
            compiler.compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }


    private void requireProfessionalLicenseXslt(String xsl) {
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        config.disableLicensing();
        Processor proc = new Processor(config);
        XsltCompiler compiler = proc.newXsltCompiler();
        try {
            compiler.compile(new StreamSource(new StringReader(xsl)));
            fail("Absence of license not detected");
        } catch (SaxonApiException err) {
            System.err.println(err.getMessage());
            // OK - error expected
        } catch (LicenseException le) {
            System.err.println("LicenseException as expected: " + le.getMessage());
        }

        proc = new Processor(true);
        compiler = proc.newXsltCompiler();
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        try {
            XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
            exec.export(baos);
        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
        ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
        config = new ProfessionalConfiguration();
        config.disableLicensing();
        proc = new Processor(config);
        compiler = proc.newXsltCompiler();
        try {
            XsltExecutable exec = compiler.compile(new StreamSource(bais));
            fail("Absence of license not detected");
        } catch (SaxonApiException err) {
            System.err.println(err.getMessage());
            // OK - error expected
        } catch (LicenseException le) {
            System.err.println("LicenseException as expected: " + le.getMessage());
        }
    }

    private void requireEnterpriseLicenseXslt(String xsl) {
        String xsd = "<xs:schema targetNamespace='http://www.example.com/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                + "<xs:simpleType name='integers'><xs:list itemType='xs:integer'/></xs:simpleType>"
                + "</xs:schema>";
        EnterpriseConfiguration config = new EnterpriseConfiguration();
        config.disableLicensing();
        Processor proc = new Processor(config);
        XsltCompiler compiler = proc.newXsltCompiler();
        try {
            compiler.setSchemaAware(true);
            compiler.compile(new StreamSource(new StringReader(xsl)));
            fail("Absence of license not detected");
        } catch (SaxonApiException err) {
            System.err.println(err.getMessage());
            // OK - error expected
        } catch (LicenseException le) {
            System.err.println("LicenseException as expected: " + le.getMessage());
        }

        proc = new Processor(true);
        try {
            SchemaManager mgr = proc.getSchemaManager();
            mgr.load(new StreamSource(new StringReader(xsd)));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
        compiler = proc.newXsltCompiler();
        compiler.setSchemaAware(true);
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        try {
            XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
            exec.export(baos);
        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
        ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
        config = new EnterpriseConfiguration();
        config.disableLicensing();
        proc = new Processor(config);
        compiler = proc.newXsltCompiler();
        try {
            XsltExecutable exec = compiler.compile(new StreamSource(bais));
            fail("Absence of license not detected");
        } catch (SaxonApiException err) {
            System.err.println(err.getMessage());
            // OK - error expected
        } catch (LicenseException le) {
            System.err.println("LicenseException as expected: " + le.getMessage());
        }
    }

    @Test
    public void testSaxonExtensionFunction() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template name='main' xmlns:saxon='http://saxon.sf.net/'>"
                + "<xsl:value-of select=\"saxon:in-summer-time(current-dateTime(), 'Europe/London')\"/>"
                + "</xsl:template></xsl:stylesheet>";
        requireProfessionalLicenseXslt(xsl);
    }

//    @Test
//    public void testSaxonEvaluate() {
//        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
//                "<xsl:param name='in' select=\"'3'\"/>" +
//                "<xsl:template name='main' xmlns:saxon='http://saxon.sf.net/'>"
//                + "<xsl:value-of select=\"saxon:evaluate($in)\"/>"
//                + "</xsl:template></xsl:stylesheet>";
//        requireProfessionalLicenseXslt(xsl);
//    }

    @Test
    public void testHof() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:param name='in' select=\"head#1\"/>" +
                "<xsl:template name='main'>"
                + "<xsl:value-of select=\"function-name($in)\"/>"
                + "</xsl:template></xsl:stylesheet>";
        noLicenseNeeded(xsl);
    }

    @Test
    public void testHof2() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:param name='in' select=\"10\"/>" +
                "<xsl:template name='main'>"
                + "<xsl:value-of select=\"function($x){$x+1}($in)\"/>"
                + "</xsl:template></xsl:stylesheet>";
        noLicenseNeeded(xsl);
    }

    @Test
    public void testHof3() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:param name='in' select=\"function($x){$x+1}\"/>" +
                "<xsl:template name='main'>"
                + "<xsl:value-of select=\"function-name($in)\"/>"
                + "</xsl:template></xsl:stylesheet>";
        noLicenseNeeded(xsl);
    }

    @Test
    public void testSerializationExtensions() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:output saxon:line-length='120' xmlns:saxon='http://saxon.sf.net/'/>" +
                "<xsl:template name='main'>"
                + "<xsl:value-of select=\"120\"/>"
                + "</xsl:template></xsl:stylesheet>";
        requireProfessionalLicenseXslt(xsl);
    }

    @Test
    public void testStreaming() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template name='main'>"
                + "<xsl:source-document streamable='yes' href='in.xml'><xsl:value-of select='count(//*)'/></xsl:source-document>"
                + "</xsl:template></xsl:stylesheet>";
        requireEnterpriseLicenseXslt(xsl);
    }

    @Test
    public void testSchemaAwareness() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                "<xsl:import-schema namespace='http://example.com/'/>" +
                "<xsl:template name='main'>"
                + "<xsl:value-of select=\"Q{http://www.example.com/}integers('1 2 3')\"/>"
                + "</xsl:template></xsl:stylesheet>";
        requireEnterpriseLicenseXslt(xsl);
    }

    @Test
    public void testSchemaAwareness2() {
        String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                "<xsl:template name='main'>"
                + "<xsl:copy-of select='.' validation='strict'/>"
                + "</xsl:template></xsl:stylesheet>";
        requireEnterpriseLicenseXslt(xsl);
    }

    @Test
    public void testOEMActivation() {
        try {
            Processor processor = new Processor(true);
            ActivateSaxon.activate(processor);
            XPathCompiler xpc = processor.newXPathCompiler();
            xpc.declareNamespace("saxon", "http://saxon.sf.net/");
            XPathExecutable xpe = xpc.compile("saxon:decimal-divide(10.0, 3.0, 5)");
            XdmValue result = xpe.load().evaluate();
            assertEquals(3.33333, ((XdmAtomicValue) result).getDoubleValue());
            assertEquals("V009048", processor.getUnderlyingConfiguration().getLicenseFeature("Serial"));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testOEMActivationExpired() {
        try {
            Processor processor = new Processor(true);
            ActivateSaxon.activateExpired(processor);
            XPathCompiler xpc = processor.newXPathCompiler();
            xpc.declareNamespace("saxon", "http://saxon.sf.net/");
            XPathExecutable xpe = xpc.compile("saxon:decimal-divide(10.0, 3.0, 5)");
            XdmValue result = xpe.load().evaluate();
            fail("Unexpected success " + result);
        } catch (LicenseException e) {
            assertTrue(e.getMessage().contains("(serial number V010024)"), "serial");
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void TestReadConfigFileWithLinkToLicenseFile() {
        // Copy the license file from the standard location to a new location in the build directory,
        // and then pick it up by reference from a configuration file. This test doesn't rule out the
        // possibility that we're ignoring the configuration file and loading the license from the usual
        // location. To eliminate that possibility, we do the same again, corrupting the license and
        // checking that loading now fails.
        try {
            // First phase - should work
            String existingLicenseFileLocation = System.getenv("SAXON_LICENSE_DIR") + "/saxon-license.lic";
            String newLicenseLocation = new ConfigTest().getDataDir().replace("src/test/testdata", "build/license");
            Path newLicensePath = Paths.get(newLicenseLocation + "/copied-license.text");
            Files.createDirectories(Paths.get(newLicenseLocation));
            Files.copy(Paths.get(existingLicenseFileLocation), newLicensePath, StandardCopyOption.REPLACE_EXISTING);
            Processor processor = new Processor(new StreamSource(new File(
                    new ConfigTest().getDataDir() + "config-with-link-to-license-file.config")));
            assertNotNull(processor);
            assertTrue(processor.getUnderlyingConfiguration().isLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION));
            // Second phase - should fail
            List<String> content = Files.readAllLines(newLicensePath);
            for (int i=0; i<content.size(); i++) {
                if (content.get(i).startsWith("Signature=")) {
                    content.set(i, "Signature=FFFFFFFF");
                }
            }
            Files.write(newLicensePath, content);
            try {
                Processor proc2 = new Processor(new StreamSource(new File(
                        new ConfigTest().getDataDir() + "config-with-link-to-license-file.config")));
                assertNotNull(proc2);
                processor.getUnderlyingConfiguration().isLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION);
                fail("Invalid license not detected");
            } catch (SaxonApiException e) {
                assertTrue(e.getMessage().contains("Failed to process license"));
                assertTrue(e.getMessage().contains("build/license/copied-license.text"));
            }
        } catch (Exception ex) {
            ex.printStackTrace();
            fail(ex);
        }
    }
}

