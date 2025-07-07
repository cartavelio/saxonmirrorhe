package resolvers;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xmlresolver.utils.URIUtils;

import javax.xml.transform.sax.SAXSource;
import java.io.InputStream;
import java.net.URI;
import java.net.URL;
import java.net.URLClassLoader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.fail;

public class ClasspathUriTest {

    @Test
    public void testSaxonResolver() {
        try {
            String base = "classpath:my/class/path/";
            String relativeURI = "my-style.xslt";
            URI absoluteURI, resolvedURI;

            absoluteURI = new URI(base);
            resolvedURI = absoluteURI.resolve(relativeURI);

            assertEquals("my-style.xslt", resolvedURI.toString());

            resolvedURI = URIUtils.resolve(absoluteURI, relativeURI);

            assertEquals("classpath:my/class/path/my-style.xslt", resolvedURI.toString());
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void testClasspathReferences() throws SaxonApiException {
        Processor processor = new Processor(false);

        System.err.println(ClassLoader.class.getName());
        ClassLoader cl = ClassLoader.getSystemClassLoader();
        System.err.println(cl.getClass().getName());
//        cl.getParent().
//        URL[] urls = ((URLClassLoader)cl).getURLs();
//
//        for(URL url: urls){
//            System.err.println(url.getFile());
//        }

        InputStream stream = ClassLoader.getSystemResourceAsStream("classpath-testing/doc.xml");
        assertNotNull(stream, "ClassLoader.getSystemResourceAsStream returned null");
        InputSource source = new InputSource(stream);
        source.setSystemId("classpath:classpath-testing/doc.xml");
        DocumentBuilder builder = processor.newDocumentBuilder();
        XdmNode document = builder.build(new SAXSource(source));
        assertNotNull(document);

        stream = ClassLoader.getSystemResourceAsStream("classpath-testing/style.xsl");
        assertNotNull(stream);
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
        assertNotNull(result);
    }
}
