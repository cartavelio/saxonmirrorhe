////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static net.sf.saxon.s9api.streams.Steps.path;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer with xsl:evaluate instruction
 */
public class TestXslEvaluate {

    public static final String xsl =
            "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
            +"   <xsl:param name='exp' select=\"'2+2'\"/>"
            +"   <xsl:template name='main'>"
            +"    <out prop=\"{system-property('xsl:supports-dynamic-evaluation')}\">"
            +"      <xsl:evaluate xpath='$exp'/>"
            +"    </out>"
            +"  </xsl:template>"
            +"</xsl:transform>";

    public static final String xslWithFallback =
            "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
                    + "   <xsl:param name='exp' select=\"'2+2'\"/>"
                    + "   <xsl:template name='main'>"
                    + "    <out prop=\"{system-property('xsl:supports-dynamic-evaluation')}\">"
                    + "      <xsl:evaluate xpath='$exp'><xsl:fallback>42</xsl:fallback></xsl:evaluate>"
                    + "    </out>"
                    + "  </xsl:template>"
                    + "</xsl:transform>";

    public static final String xslStaticProperty =
            "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
                    + "   <xsl:param name='exp' select=\"'2+2'\"/>"
                    + "   <xsl:template name='main'>"
                    + "    <out>"
                    + "      <xsl:attribute name='prop' use-when=\"'no'=system-property('xsl:supports-dynamic-evaluation')\">no</xsl:attribute>"
                    + "      <xsl:attribute name='prop' use-when=\"'yes'=system-property('xsl:supports-dynamic-evaluation')\">yes</xsl:attribute>"
                    + "    </out>"
                    + "  </xsl:template>"
                    + "</xsl:transform>";

    /**
     * Test that xsl:evaluate works with Saxon-HE configuration
     */

    @Test
    public void testXslEvaluateWithHE() {

        try {
            Processor processor = new Processor(false);

            // Create a transformer for the stylesheet.
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            Xslt30Transformer transformer = xsltCompiler.compile(
                    new StreamSource(new StringReader(xsl))).load30();

            // Create a destination
            XdmDestination destination = new XdmDestination();
            try {
                // Transform the source XML to System.out.

                transformer.callTemplate(new QName("main"), destination);
                // OK

            } catch (SaxonApiException e) {
                fail();
            }
        } catch (SaxonApiException e2) {
            fail();
        }

    }

    /**
     * Test that xsl:evaluate succeeds with Saxon-HE configuration with xsl:fallback
     */

    @Test
    public void testXslEvaluateWithHEWithFallback() {

        try {

            Processor processor = new Processor(false);
            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xslWithFallback))).load30();
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, true);

            // Create a destination
            XdmDestination destination = new XdmDestination();

            // Transform the source XML to System.out.

            transformer.callTemplate(new QName("main"), destination);
            assertEquals("no", destination.getXdmNode().select(path("out", "@prop")).asString());

        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }
    }


    /**
     * Test that xsl:evaluate can be statically disabled
     */

    @Test
    public void testDisableXslEvaluateStatically() {

        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, true);

            // Create a transformer for the stylesheet.
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            Xslt30Transformer transformer = xsltCompiler.compile(
                    new StreamSource(new StringReader(xsl))).load30();

            // Create a destination
            XdmDestination destination = new XdmDestination();
            try {
                // Transform the source XML to System.out.

                transformer.callTemplate(new QName("main"), destination);
                fail("Unexpected success");

            } catch (SaxonApiException e) {
                System.err.println(e.getMessage());
                assertEquals("XTDE3175", e.getErrorCode().getLocalName());
            }
        } catch (SaxonApiException e2) {
            fail();
        }

    }

    /**
     * Test that xsl:fallback takes effect when xsl:evaluate ine statically disabled
     */

    @Test
    public void testDisableXslEvaluateStaticallyWithFallback() {
        try {

            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, true);
            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xslWithFallback))).load30();

            // Create a destination
            XdmDestination destination = new XdmDestination();

            // Transform the source XML to System.out.

            transformer.callTemplate(new QName("main"), destination);
            assertEquals("42", destination.getXdmNode().getStringValue());
            assertEquals(destination.getXdmNode().select(path("out", "@prop")).asString(), "no");

        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }

    }




    /**
     * Test that xsl:evaluate can be dyamically disabled
     */

    @Test
    public void testDisableXslEvaluateDynamically() {
        try {
            Processor processor = new Processor(true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load30();

            // Create a destination
            XdmDestination serializer = new XdmDestination();

            // Transform the source XML to System.out.
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, true);
            transformer.callTemplate(new QName("main"), serializer);
            fail("Unexpected success");

        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            assertEquals("XTDE3175", e.getErrorCode().getLocalName());
        }

    }

    /**
     * Test that xsl:fallback takes effect when xsl:evaluate ine dyamically disabled
     */

    @Test
    public void testDisableXslEvaluateDynamicallyWithFallback() {
        try {

            Processor processor = new Processor(true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xslWithFallback))).load30();

            // Create a destination
            XdmDestination destination = new XdmDestination();

            // Transform the source XML to System.out.
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, true);
            transformer.callTemplate(new QName("main"), destination);
            assertEquals("42", destination.getXdmNode().getStringValue());
            assertEquals(destination.getXdmNode().select(path("out", "@prop")).asString(), "no");

        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }

    }

    /**
     * Test value of xsl:supports-dynamic-evaluation in a Saxon-HE static expression
     */

    @Test
    public void testStaticSystemPropertyHE() {
        systemProperty(false, "yes", false);
    }

    @Test
    public void testStaticSystemPropertyEE() {
        systemProperty(true, "yes", false);
    }

    @Test
    public void testStaticSystemPropertyEEdisabled() {
        systemProperty(true, "no", true);
    }

    public void systemProperty(boolean ee, String expected, boolean disable) {

        try {

            Processor processor = new Processor(ee);
            processor.setConfigurationProperty(Feature.DISABLE_XSL_EVALUATE, disable);
            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xslStaticProperty))).load30();

            // Create a destination
            XdmDestination destination = new XdmDestination();

            // Transform the source XML to System.out.

            transformer.callTemplate(new QName("main"), destination);
            assertEquals(destination.getXdmNode().select(path("out", "@prop")).asString(), expected);

        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }
    }


}
