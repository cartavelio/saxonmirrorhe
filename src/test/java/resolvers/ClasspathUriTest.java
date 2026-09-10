package resolvers;

import net.sf.saxon.s9api.*;
import org.junit.Assert;
import org.junit.Test;
import org.xml.sax.InputSource;
import org.xmlresolver.utils.URIUtils;

import javax.xml.transform.sax.SAXSource;
import java.io.InputStream;
import java.net.URI;
import java.net.URL;

import static junit.framework.TestCase.fail;

public class ClasspathUriTest {

    @Test
    public void testSaxonResolver() {
        try {
            String base = "classpath:my/class/path/";
            String relativeURI = "my-style.xslt";
            URI absoluteURI, resolvedURI;

            absoluteURI = new URI(base);
            resolvedURI = absoluteURI.resolve(relativeURI);

            Assert.assertEquals("my-style.xslt", resolvedURI.toString());

            resolvedURI = URIUtils.resolve(absoluteURI, relativeURI);

            Assert.assertEquals("classpath:my/class/path/my-style.xslt", resolvedURI.toString());
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void testClasspathReferences() {
        try {
            Processor processor = new Processor(false);
            InputStream stream = ClassLoader.getSystemResourceAsStream("classpath-testing/doc.xml");
            Assert.assertNotNull(stream);
            InputSource source = new InputSource(stream);
            source.setSystemId("classpath:classpath-testing/doc.xml");
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode document = builder.build(new SAXSource(source));
            Assert.assertNotNull(document);

            stream = ClassLoader.getSystemResourceAsStream("classpath-testing/style.xsl");
            Assert.assertNotNull(stream);
            source = new InputSource(stream);
            source.setSystemId("classpath:classpath-testing/doc.xml");

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(false);
            XsltExecutable exec = compiler.compile(new SAXSource(source));

            XsltTransformer transformer = exec.load();
            transformer.setInitialContextNode(document);
            XdmDestination destination = new XdmDestination();
            transformer.setDestination(destination);
            transformer.transform();

            XdmNode result = destination.getXdmNode();
            Assert.assertNotNull(result);
        } catch (Exception ex) {
            fail();
        }
    }
}
