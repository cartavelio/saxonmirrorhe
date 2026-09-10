package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api XsltTransformer running stylesheets using 1.0 compatibility mode
 */
public class TestXsltIn10Mode {
    private ConfigTest configTest = new ConfigTest();

    /**
     * Simple test: 1.0 mode works in PE but not in HE
     */

    @Test
    public void testEffectInHE() {
        effectIn("HE");
    }

    @Test
    public void testEffectInPE() {
        effectIn("PE");
    }

    private void effectIn(String edition) {
        try {

            String xsl = "<xsl:transform version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><xsl:value-of select='42'/></xsl:template>" +
                    "</xsl:transform>";

            Processor processor = new Processor(!"HE".equals(edition));
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            transformer.setDestination(serializer);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.setInitialContextNode(input);
            transformer.transform();

        } catch (SaxonApiException e) {
            fail("Unexpected failure");
            e.printStackTrace();

        }

    }

    /**
     * HE: OK if 1.0 code not executed
     */

    @Test
    public void testExecuting10code() {
        effectPartial(true);
    }

    @Test
    public void testNotExecuting10code() {
        effectPartial(false);
    }

    private void effectPartial(boolean execute10code) {
        try {

            String xsl = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='foo' version='1.0'><xsl:value-of select='42'/></xsl:template>" +
                    "<xsl:template name='bar' version='2.0'><xsl:value-of select='43'/></xsl:template>" +
                    "</xsl:transform>";

            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            transformer.setDestination(serializer);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.setInitialTemplate(new QName(execute10code ? "foo" : "bar"));
            transformer.transform();


        } catch (SaxonApiException e) {


            fail("Unexpected failure");
            e.printStackTrace();

        }

    }
}


