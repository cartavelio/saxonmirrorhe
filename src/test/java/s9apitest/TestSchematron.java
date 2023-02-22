package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.HashMap;

/**
 *
 */
public class TestSchematron extends TestCase {

    public void testAssertXMLDocument() {
        XmlAssertor xmlAssertor = new XmlAssertor(ConfigTest.DATA_DIR + "radu/iso_schematron_text.xsl");
        String sAssertionResult = xmlAssertor.assertXMLDocument(
                new File(ConfigTest.DATA_DIR + "radu/to_be_asserted.xml"),
                ConfigTest.DATA_DIR + "radu/legacygcib.sch");

        System.out.println("sAssertionResult = " + sAssertionResult);

        assertEquals("", sAssertionResult);
    }

    public class XmlAssertor {
        public static final String SUCCESS = "SUCCESS";
        private Processor proc = new Processor(true);
        private XsltCompiler xsltCompiler = proc.newXsltCompiler();
        private HashMap<String, XsltTransformer> mapTransformers = new HashMap<String, XsltTransformer>();

        private XsltExecutable xslSchematronISOTextXSLTExec = null;
        private XsltTransformer xslSchematronISOTextTransformer = null;

        public XmlAssertor(String sISOSchematronFilePath) {
            try {
                xslSchematronISOTextXSLTExec = xsltCompiler.compile(new StreamSource(new File(sISOSchematronFilePath)));
            } catch (SaxonApiException e) {
                e.printStackTrace();
            }
            xslSchematronISOTextTransformer = xslSchematronISOTextXSLTExec.load();
        }

        private XsltTransformer getAssertionTransformer(String sSchematronFilePath) {
            XsltTransformer assertionTransformer = (XsltTransformer)mapTransformers.get(sSchematronFilePath);

            // lazily generate the assertion xsl transformer for corresponding schematron file
            if (assertionTransformer == null) {
                try {
                    XdmNode nodeSchematronRules = proc.newDocumentBuilder().build(new StreamSource(new File(sSchematronFilePath)));

                    XdmDestination nodeStepOneResults = null;
                    xslSchematronISOTextTransformer.setInitialContextNode(nodeSchematronRules);
                    nodeStepOneResults = new XdmDestination();
                    xslSchematronISOTextTransformer.setDestination(nodeStepOneResults);
                    xslSchematronISOTextTransformer.transform();

//				System.out.println("nodeStepOneResults are: \n" + nodeStepOneResults.getXdmNode());

                    XsltExecutable xslCompiledSchematronXSLTExec = xsltCompiler.compile(nodeStepOneResults.getXdmNode().asSource());
                    assertionTransformer = xslCompiledSchematronXSLTExec.load();

                    // store the transformer for later use
                    mapTransformers.put(sSchematronFilePath, assertionTransformer);
                } catch (SaxonApiException e) {
                    e.printStackTrace();
                }
            }

            return assertionTransformer;
        }

        public String assertXMLDocument(File sXMLToBeAsserted, String sSchematronFilePath) {
            String sValidationResults = null;
            try {
                XsltTransformer assertionTransformer = getAssertionTransformer(sSchematronFilePath);

                XdmNode nodeXMLtoBeAsserted = proc.newDocumentBuilder().build(new StreamSource(sXMLToBeAsserted));
                assertionTransformer.setInitialContextNode(nodeXMLtoBeAsserted);
                XdmDestination nodeStepTwoResults = new XdmDestination();
                assertionTransformer.setDestination(nodeStepTwoResults);
                assertionTransformer.transform();

                if (nodeStepTwoResults.getXdmNode() != null && nodeStepTwoResults.getXdmNode().size() > 0)
                // schematron rule checks triggered errors - pass them to the caller.
                {
                    sValidationResults = nodeStepTwoResults.getXdmNode().itemAt(0).getStringValue();
                } else
                // all schematron rules passed.
                {
                    sValidationResults = SUCCESS;
                }

            } catch (SaxonApiException e) {
                e.printStackTrace();
            } catch (Exception e) {
                e.printStackTrace();
            }

            return sValidationResults;

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



