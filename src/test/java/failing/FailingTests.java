package failing;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * This module is a place for parking tests that we are currently failing for a known reason. Failures in these
 * tests are known problems and should not inhibit progress.
 */
public class FailingTests {
    private final ConfigTest configTest = new ConfigTest();
    public final String DATA_DIR = configTest.getDataDir();

    /**
     * Test the effect of changing a schema to add members to a substitution group.
     * See bug 3531.
     */

    @Test
    @Disabled("Known to fail - needs design changes - see bug 3531")
    public void testMergeSubstitutionGroups() {
        try {

            String sch0 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <element name=\"FamilyMember\" type=\"string\" abstract='true'/>\n" +
                    "  <element name=\"Family\">\n" +
                    "    <complexType>\n" +
                    "      <sequence>\n" +
                    "        <element ref=\"fam:FamilyMember\" maxOccurs=\"unbounded\"/>\n" +
                    "      </sequence>\n" +
                    "    </complexType>\n" +
                    "  </element>  \n" +
                    "</schema>";

            String sch1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <include schemaLocation='sch0.xsd'/>" +
                    "  <element name=\"Parent\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <include schemaLocation='sch0.xsd'/>" +
                    "  <element name=\"Child\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";


            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, true);
            SchemaManager sm = proc.getSchemaManager();
            sm.setSchemaURIResolver(new SchemaURIResolver() {
                @Override
                public void setConfiguration(Configuration config) {

                }

                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) {
                    Source[] result = new Source[locations.length];
                    for (int i = 0; i < locations.length; i++) {
                        switch (locations[i]) {
                            case "sch0.xsd":
                                result[i] = new StreamSource(new StringReader(sch0), "http://example.com/sch0.xsd");
                                break;
                            case "sch1.xsd":
                                result[i] = new StreamSource(new StringReader(sch1), "http://example.com/sch1.xsd");
                                break;
                            case "sch2.xsd":
                                result[i] = new StreamSource(new StringReader(sch0), "http://example.com/sch2.xsd");
                                break;
                            default:
                                result[i] = null;
                                break;
                        }
                    }
                    return result;
                }
            });
            sm.load(new StreamSource(new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            sm.load(new StreamSource(new StringReader(sch2)));
            sm.newSchemaValidator().validate(new StreamSource(new StringReader(doc1)));
            System.err.println("Doc 1 OK");


        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());
            fail();
        }
    }


    @Test
    @Disabled("Known to fail. There are spec problems and implementation problems loading a package multiple times with different static parameter values")
    public void testPackageRenaming() {
        try {
            String configFile = DATA_DIR + "packages/config-120.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            Xslt30Transformer trans = top.load30();
            XdmNode result = (XdmNode) trans.callTemplate(null);
            XPathCompiler xp = processor.newXPathCompiler();
            String sum = xp.evaluateSingle("/out/add", result).getStringValue();
            String difference = xp.evaluateSingle("/out/subtract", result).getStringValue();
            String product = xp.evaluateSingle("/out/multiply", result).getStringValue();
            assertEquals("12", sum);
            assertEquals("8", difference);
            assertEquals("20", product);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    @Disabled("Known to fail: see bug 4220")
    public void testPackageExportWithParams() {
        // See bug 4220, currently still open (2020-01-23)
        try {
            String configFile = DATA_DIR + "packages/config-120.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            XsltPackage add = compiler.obtainPackage("http://package-add/", "1.0");
            add.save(new File(DATA_DIR + "packages/pack-120-add.xsl"));
            XsltPackage subtract = compiler.obtainPackage("http://package-subtract/", "1.0");
            subtract.save(new File(DATA_DIR + "packages/pack-120-subtract.xsl"));
            XsltPackage multiply = compiler.obtainPackage("http://package-multiply/", "1.0");
            multiply.save(new File(DATA_DIR + "packages/pack-120-multiply.xsl"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    @Disabled("Known to fail: see bug 4220")
    public void testPackageRenamingExported() {
        // This uses the sef packages created by test testPackageExportWithParams
        // See bug 4220, currently still open (2020-01-23)
        try {
            String configFile = DATA_DIR + "packages/config-121.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            Xslt30Transformer trans = top.load30();
            XdmNode result = (XdmNode) trans.callTemplate(null);
            XPathCompiler xp = processor.newXPathCompiler();
            String sum = xp.evaluateSingle("/out/add", result).getStringValue();
            String difference = xp.evaluateSingle("/out/subtract", result).getStringValue();
            String product = xp.evaluateSingle("/out/multiply", result).getStringValue();
            assertEquals("12", sum);
            assertEquals("8", difference);
            assertEquals("20", product);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


}

