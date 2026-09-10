package resolvers;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.fail;

public class StandardURIResolverTest {

    private static String DATA_DIR = new ConfigTest().getDataDir();
    private static String REPO_ROOT = DATA_DIR.replace("src/test/testdata/", "");
    @Test
    public void noConfigurationOk() throws XPathException, SaxonApiException {
        StandardURIResolver resolver = new StandardURIResolver();
        Source source = resolver.resolve("resolvers/ok.xml", Testdata.baseURI().toString());
        new Processor(false).newDocumentBuilder().build(source);
    }

    @Test
    public void withConfigurationOk() throws XPathException, SaxonApiException {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);
        Source source = resolver.resolve("resolvers/ok.xml", Testdata.baseURI().toString());
        new Processor(false).newDocumentBuilder().build(source);
        Assertions.assertNotNull(source);
    }

    @Test
    public void withConfigurationWithSpaces() throws XPathException, SaxonApiException {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        Source source = resolver.resolve("my dir/Transform3.xslt", Testdata.baseURI().toString());
        new Processor(false).newDocumentBuilder().build(source);
        Assertions.assertNotNull(source);
    }
}
