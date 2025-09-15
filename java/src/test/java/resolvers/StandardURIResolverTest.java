package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.fail;

public class StandardURIResolverTest {
    @Test
    public void noConfigurationOk() throws XPathException {
        StandardURIResolver resolver = new StandardURIResolver();

        Source source = resolver.resolve("src/test/testdata/resolvers/input.xml", Testdata.baseURI().toString());

        Assertions.assertNotNull(source);
    }

    @Test
    public void withConfigurationOk() throws XPathException {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        Source source = resolver.resolve("asdasdsrc/test/testdata/resolvers/input.xml", Testdata.baseURI().toString());

        Assertions.assertNotNull(source);
    }

    @Test
    public void withConfigurationWithSpaces() throws XPathException {
        Configuration config = new Configuration();
        StandardURIResolver resolver = new StandardURIResolver(config);

        Source source = resolver.resolve("src/test/testdata/my dir/Transform3.xsl", Testdata.baseURI().toString());

        Assertions.assertNotNull(source);
    }
}
