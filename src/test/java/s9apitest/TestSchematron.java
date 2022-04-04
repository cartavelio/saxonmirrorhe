package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.HashMap;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 *
 */
public class TestSchematron {
    ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testAssertXMLDocument() {
        XmlAssertor xmlAssertor = new XmlAssertor(configTest.getDataDir() + "radu/iso_schematron_text.xsl");
        String sAssertionResult = xmlAssertor.assertXMLDocument(
                new File(configTest.getDataDir() + "radu/to_be_asserted.xml"),
                configTest.getDataDir() + "radu/legacygcib.sch");

        System.out.println("sAssertionResult = " + sAssertionResult);

        assertEquals("", sAssertionResult);
    }

    public class XmlAssertor {
        public static final String SUCCESS = "SUCCESS";
        private final Processor proc = new Processor(true);
        private final XsltCompiler xsltCompiler = proc.newXsltCompiler();
        private final HashMap<String, XsltTransformer> mapTransformers = new HashMap<String, XsltTransformer>();

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
            XsltTransformer assertionTransformer = mapTransformers.get(sSchematronFilePath);

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

