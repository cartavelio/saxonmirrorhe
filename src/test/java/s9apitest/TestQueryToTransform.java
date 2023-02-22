package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.assertEquals;

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
}




