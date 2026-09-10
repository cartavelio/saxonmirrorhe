package resolvers;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.Processor;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.fail;

public class StandardURIResolverTest {

    private static String DATA_DIR = new ConfigTest().getDataDir();
    private static String REPO_ROOT = DATA_DIR.replace("src/test/testdata/", "");
    @Test
    public void noConfigurationOk() {
        StandardURIResolver resolver = new StandardURIResolver();
        URI baseURI = URI.create("file://" + REPO_ROOT);
        try {
            Source source = resolver.resolve("src/test/testdata/resolvers/ok.xml", baseURI.toString());
            new Processor(false).newDocumentBuilder().build(source);
            Assertions.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withConfigurationOk() {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);
        URI baseURI = URI.create("file://" + REPO_ROOT);
        try {
            Source source = resolver.resolve("src/test/testdata/resolvers/ok.xml", baseURI.toString());
            new Processor(false).newDocumentBuilder().build(source);
            Assertions.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withConfigurationWithSpaces() {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        URI baseURI = URI.create("file://" + REPO_ROOT);
        try {
            Source source = resolver.resolve("src/test/testdata/my dir/Transform3.xsl", baseURI.toString());
            new Processor(false).newDocumentBuilder().build(source);
            Assertions.assertNotNull(source);
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }
}
