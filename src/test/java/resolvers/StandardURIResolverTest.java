package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StandardURIResolver;
import org.junit.Assert;
import org.junit.Test;

import javax.xml.transform.Source;
import java.net.URI;

import static org.junit.Assert.fail;

public class StandardURIResolverTest {
    @Test
    public void noConfigurationOk() {
        StandardURIResolver resolver = new StandardURIResolver();

        String cwd = System.getProperty("user.dir").replace("\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            Source source = resolver.resolve("src/test/testdata/resolvers/input.xml", baseURI.toString());
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withConfigurationOk() {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        String cwd = System.getProperty("user.dir").replace("\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            Source source = resolver.resolve("src/test/testdata/resolvers/input.xml", baseURI.toString());
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void withConfigurationWithSpaces() {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        String cwd = System.getProperty("user.dir").replace("\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            Source source = resolver.resolve("src/test/testdata/my dir/Transform3.xsl", baseURI.toString());
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }
}
