package resolvers;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.config.StandardSchemaResolver;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.StandardURIResolver;
import org.junit.Assert;
import org.junit.Test;

import javax.xml.transform.Source;
import java.net.URI;

import static org.junit.Assert.fail;

public class SchemaURIResolverTest {
    @Test
    public void resolveXLink() {
        Configuration config = new EnterpriseConfiguration();
        SchemaURIResolver resolver = new StandardSchemaResolver();
        resolver.setConfiguration(config);

        String cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            Source[] sources = resolver.resolve(NamespaceConstant.XLINK, null, new String[]{"a lie"});
            Assert.assertNotNull(sources);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void resolveXLinkNamespace() {
        Configuration config = new EnterpriseConfiguration();
        SchemaURIResolver resolver = new StandardSchemaResolver();
        resolver.setConfiguration(config);

        String cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            // I hoppen to know that the XML Resolver data jar contains a catalog that can resolve
            // the XML Catalogs schema from its namespace, let's see if it works!
            Source[] sources = resolver.resolve("urn:oasis:names:tc:entity:xmlns:xml:catalog", null, null);
            Assert.assertNotNull(sources);
        } catch (Exception ex) {
            fail();
        }
    }

}
