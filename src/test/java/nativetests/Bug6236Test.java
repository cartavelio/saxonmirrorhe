package nativetests;

import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test various aspects of setting the Saxon Configuration from Java code
 */
public class Bug6236Test {

    @Test
    public void testBug6236() {
        String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                + "                xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                + "                version=\"3.0\">\n"
                + "  <xsl:output method=\"xml\" indent=\"yes\"/>\n"
                + "  <xsl:mode on-no-match=\"shallow-copy\"/>\n"
                + "  <xsl:param name=\"always-assume-indentation\" as=\"xs:boolean\" select=\"false()\"/> \n"
                + "  <xsl:param name=\"is-indented\" as=\"xs:boolean\"\n"
                + "    select=\"$always-assume-indentation or exists(/*[text()])\"/>\n"
                + "  <xsl:template match=\"/*\" mode=\"#all\">\n"
                + "    <xsl:copy>\n"
                + "      <xsl:attribute name=\"is-indented\" select=\"$is-indented\"/>\n"
                + "      <xsl:apply-templates select=\"@*, node()\" mode=\"#current\"/>\n"
                + "    </xsl:copy>\n"
                + "  </xsl:template>\n"
                + "</xsl:stylesheet>";

        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<doc>42</doc>")));
            System.out.println("saxon product version: " + proc.getSaxonProductVersion());
            XsltCompiler xsltCompiler = proc.newXsltCompiler();
            XsltExecutable compiledExecutable = xsltCompiler.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer loadedExecutable = compiledExecutable.load();
            loadedExecutable.setInitialContextNode(doc);
            XsltController controller = loadedExecutable.getUnderlyingController();
            controller.initializeController(new GlobalParameterSet());

            //XPathContextMajor context = controller.newXPathContext();
            //controller.preEvaluateGlobals(context);


            for (GlobalParam gp : controller.getExecutable().getGlobalParameters().values()) {
                String localName = gp.getVariableQName().getLocalPart();
                System.out.println("local param name: " + localName);
                GroundedValue val = gp.evaluateVariable(controller.newXPathContext(), gp.getDeclaringComponent());
                if (localName.equals("always-assume-indentation")) {
                    assertEquals(val.head().getStringValue(), "false");
                } else if (localName.equals("is-indented")) {
                    assertEquals(val.head().getStringValue(), "true");
                }
            }
        } catch (XPathException | SaxonApiException e) {
            e.printStackTrace();
            fail(e);
        }
    }

}



