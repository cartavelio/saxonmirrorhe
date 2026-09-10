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
        XsltExecutable template = comp_xslt.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "identity.xsl")));
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
        XsltExecutable template = comp_xslt.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "identity.xsl")));
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
}




