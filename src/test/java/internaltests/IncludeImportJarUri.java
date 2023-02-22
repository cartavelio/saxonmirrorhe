package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.DirectResourceResolver;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class IncludeImportJarUri {
    public static final String TESTROOT = ConfigTest.DATA_DIR + "jaruri/";

    @Test
    public void testJarImport() {
        // See also https://saxonica.plan.io/issues/4706
        String styleFileName= "jar:file://" + TESTROOT + "test.jar!/io/xspec/xspec/impl/src/compiler/compile-xslt-tests.xsl";

        try {
            Processor processor = new Processor(false);
            Configuration config = processor.getUnderlyingConfiguration();
            ResourceRequest request = new ResourceRequest();
            request.uri = styleFileName;
            request.nature = ResourceRequest.XSLT_NATURE;
            Source styleSource = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));

            QName initialTemplateName = new QName("xsl", NamespaceConstant.XSLT, "initial-template");

            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable exec = compiler.compile(styleSource);
            XsltTransformer transformer = exec.load();
            transformer.setInitialTemplate(initialTemplateName);

            XdmDestination destination = new XdmDestination();
            transformer.setDestination(destination);
            transformer.transform();
            String result = destination.getXdmNode().getStringValue();
            assertEquals("good", result);
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

}
