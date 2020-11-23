package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;


/**
 * JUnit tests for the s9api XsltTransformer running stylesheets using 1.0 compatibility mode
 */
public class TestXsltIn10Mode extends TestCase {

    /**
     * Simple test: 1.0 mode works in PE but not in HE
     */

    public void testEffectInHE() {
        effectIn("HE");
    }

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
                    new File(ConfigTest.DATA_DIR + "caffo/Input.xml"));
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

    public void testExecuting10code() {
        effectPartial(true);
    }

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
                    new File(ConfigTest.DATA_DIR + "caffo/Input.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.setInitialTemplate(new QName(execute10code ? "foo" : "bar"));
            transformer.transform();


        } catch (SaxonApiException e) {


            fail("Unexpected failure");
            e.printStackTrace();

        }

    }
}


//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//

