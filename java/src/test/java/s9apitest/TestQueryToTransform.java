package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class TestQueryToTransform {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testQueryToTransform() throws Exception {
        Processor proc = new Processor(true);
        XQueryCompiler comp = proc.newXQueryCompiler();
        comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
        String qry_base = "<a>1234</a>";

        XQueryExecutable exp = comp.compile(qry_base);
        XQueryEvaluator qe = exp.load();
        XdmValue result = qe.evaluate();

        XsltCompiler comp_xslt = proc.newXsltCompiler();
        comp_xslt.setSchemaAware(false);
        XsltExecutable template = comp_xslt.compile(new File(configTest.getDataDir() + "identity.xsl"));
        XsltTransformer trans1 = template.load();

        for (XdmItem item : result) {
            //XdmNode source = proc.newDocumentBuilder().build(((XdmNode)item).asSource());
            trans1.setInitialContextNode((XdmNode)item);
            XdmDestination resultTree = new XdmDestination();
            trans1.setDestination(resultTree);
            trans1.transform();
            assertEquals("<a>1234</a>", resultTree.getXdmNode().toString().trim());
        }

    }

    @Test
    public void testQueryToTransformPiped() throws Exception {
        Processor proc = new Processor(true);
        XQueryCompiler comp = proc.newXQueryCompiler();
        comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
        String qry_base = "<a>1234</a>";

        XQueryExecutable exp = comp.compile(qry_base);
        XQueryEvaluator qe = exp.load();

        XsltCompiler comp_xslt = proc.newXsltCompiler();
        comp_xslt.setSchemaAware(false);
        XsltExecutable template = comp_xslt.compile(new File(configTest.getDataDir() + "identity.xsl"));
        XsltTransformer trans1 = template.load();

        XdmDestination resultTree = new XdmDestination();
        trans1.setDestination(resultTree);


        qe.setDestination(trans1);
        qe.run();

        assertEquals("<a>1234</a>", resultTree.getXdmNode().toString().trim());
    }

    @Test
    public void testFnTransformStaticErrorReporter() {
        List<XmlProcessingError> staticErrors = new ArrayList<>();
        List<XmlProcessingError> dynamicErrors = new ArrayList<>();
        try {
            Processor proc = new Processor(true);


            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setErrorReporter(staticErrors::add);
            comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
            String qry_base = "transform(map{'stylesheet-text':'<a/>'})";

            XQueryExecutable exp = comp.compile(qry_base);
            XQueryEvaluator qe = exp.load();
            qe.setErrorReporter(dynamicErrors::add);
            qe.run(new NullDestination());
            fail("Unexpected success");
        } catch (URISyntaxException e) {
            fail(e);
        } catch (SaxonApiException e) {
            // error expected;
            assertTrue(staticErrors.isEmpty());
            assertFalse(dynamicErrors.isEmpty());
        }
    }

    @Test
    public void testFnTransformDynamicErrorReporter() {
        List<XmlProcessingError> staticErrors = new ArrayList<>();
        List<XmlProcessingError> dynamicErrors = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            XdmNode source = proc.newDocumentBuilder().build(new StreamSource(new StringReader("<a/>")));
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setErrorReporter(staticErrors::add);
            comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
            String qry_base = "transform(map{'stylesheet-text':"
                    + "'<a xsl:version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" z=\"{year-from-date(current-date()) div 0}\"/>'"
                    + ", 'source-node':.})";

            XQueryExecutable exp = comp.compile(qry_base);
            XQueryEvaluator qe = exp.load();
            qe.setContextItem(source);
            qe.setErrorReporter(dynamicErrors::add);
            qe.run(new NullDestination());
            fail("Unexpected success");
        } catch (URISyntaxException e) {
            fail(e);
        } catch (SaxonApiException e) {
            // error expected;
            assertTrue(staticErrors.isEmpty());
            assertFalse(dynamicErrors.isEmpty());
            assertTrue(dynamicErrors.get(0).getMessage().contains("division by zero"));
        }
    }

    /**
     * Test that message reporter is passed through fn:transform, bug 6524
     */
    @Test
    public void testFnTransformMessageReporter() {
        List<Message> messages = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            String xsl1 = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                    "<xsl:param name='xsl2' as='node()' select='.'/>" +
                    "<xsl:template name='xsl:initial-template'><out><xsl:message>MESSAGE_1</xsl:message>" +
                    "<xsl:sequence select='transform(map{\"stylesheet-node\":$xsl2})?output'/></out>" +
                    "</xsl:template></xsl:transform>";

            String xsl2 = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                    "<xsl:template name='xsl:initial-template'><in><xsl:message>MESSAGE_2</xsl:message>" +
                    "<xsl:text>INNER_TEXT</xsl:text></in>" +
                    "</xsl:template></xsl:transform>";

            XdmNode xsl2Doc = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xsl2)));
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl1)));
            Xslt30Transformer trans = exec.load30();
            trans.setGlobalContextItem(xsl2Doc);
            trans.setMessageHandler(messages::add);
            trans.callTemplate(null);
            assertEquals(2, messages.size());
        } catch (SaxonApiException e) {
            fail(e);
        }
    }
}




