package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.CommonResourceResolver;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.Assert;
import org.junit.Test;
import org.xml.sax.InputSource;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.net.URI;
import java.util.Collections;
import java.util.Iterator;

import static org.junit.Assert.fail;

public class StandardURIResolverTest {
    @Test
    public void noConfigurationOk() {
        StandardURIResolver resolver = new StandardURIResolver();

        String cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
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

        String cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
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

        String cwd = System.getProperty("user.dir").replaceAll("\\\\", "/"); // Windows
        if (!cwd.endsWith("/")) {
            cwd += "/";
        }
        URI baseURI = URI.create("file://" + cwd);
        try {
            Source source = resolver.resolve("src/test/testdata/my dir/Transform3.xsl", baseURI.toString());
            Assert.assertNotNull(source);
        } catch (Exception ex) {
            fail();
        }
    }
}
