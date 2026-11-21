package resolvers;

import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.functions.DocumentFn;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class DocumentFnTest {
    @Test
    public void testDocumentFnFile() {
        Processor processor = new Processor();
        Controller controller = new Controller(processor.getUnderlyingConfiguration());
        XPathContext context = new XPathContextMajor(controller);
        try {
            Source source = DocumentFn.resolveURI("/path/to/sample.xml", "file:///original/path" , "file:///fail", context);
            Assertions.assertNotNull(source);
            Assertions.assertEquals("file:/path/to/sample.xml", source.getSystemId());
        } catch (XPathException ex) {
            fail(ex);
        }
    }

    @Test
    public void testDocumentFnJar() {
        Processor processor = new Processor();
        Controller controller = new Controller(processor.getUnderlyingConfiguration());
        XPathContext context = new XPathContextMajor(controller);
        try {
            String jar = "jar:" + new File("src/test/resources/classpath-testing.jar").toURI() + "!/";
            Source source = DocumentFn.resolveURI("/classpath-testing/doc.xml", jar + "classpath-testing/path/a.xsl", "file:///fail", context);
            Assertions.assertNotNull(source);
            Assertions.assertEquals(jar + "classpath-testing/doc.xml", source.getSystemId());
        } catch (XPathException ex) {
            fail(ex);
        }
    }
}
