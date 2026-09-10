package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StandardModuleURIResolver;
import net.sf.saxon.s9api.Processor;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;

import javax.xml.transform.stream.StreamSource;
import java.net.URI;

import static org.junit.Assert.fail;

public class StandardModuleURIResolverTest {
    static String cwd;
    static URI baseURI;

    @Before
    public void before() {
        cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        baseURI = URI.create("file://" + cwd);
    }

    @Test
    public void noConfigurationOk() {
        StandardModuleURIResolver resolver = new StandardModuleURIResolver();

        // Should work without a configuration
        try {
            String namespace = "http://irrelevant/";
            String[] loc = new String[] { "src/test/testdata/resolvers/module.xqy" };
            StreamSource[] source = resolver.resolve(namespace, baseURI.toString(), loc);
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withConfigurationOk() {
        Configuration config = new Configuration();
        StandardModuleURIResolver resolver = new StandardModuleURIResolver(config);

        // Should work with a configuration
        try {
            String namespace = "http://irrelevant/";
            String[] loc = new String[] { "src/test/testdata/resolvers/module.xqy" };
            StreamSource[] source = resolver.resolve(namespace, baseURI.toString(), loc);
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withCatalog() {
        Processor proc = new Processor(false);
        Configuration config = proc.getUnderlyingConfiguration();
        try {
            proc.setCatalogFile(cwd + "src/test/testdata/resolvers/catalog.xml");

            StandardModuleURIResolver resolver = new StandardModuleURIResolver(config);

            StreamSource[] source = resolver.resolve("http://example.com/testfunc", baseURI.toString(), new String[] {});
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withCatalogAndHints() {
        Processor proc = new Processor(false);
        Configuration config = proc.getUnderlyingConfiguration();
        try {
            proc.setCatalogFile(cwd + "src/test/testdata/resolvers/catalog.xml");
            
            StandardModuleURIResolver resolver = new StandardModuleURIResolver(config);

            String namespace = "http://irrelevant/";
            String[] loc = new String[] { "src/test/testdata/resolvers/module.xqy" };

            StreamSource[] source = resolver.resolve(namespace, baseURI.toString(), loc);
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }
}
