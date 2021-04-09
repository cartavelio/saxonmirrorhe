package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;
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
            Source styleSource = null;
            styleSource = config.getURIResolver().resolve(styleFileName, null);
            if (styleSource == null) {
                styleSource = config.getSystemURIResolver().resolve(styleFileName, null);
            }
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
