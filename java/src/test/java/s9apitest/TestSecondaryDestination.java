package s9apitest;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.OutputURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Result;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class TestSecondaryDestination {
    @Test
    public void testSecondaryDestination1() {
        // new processor, custom output resolver
        try {
            Processor proc = new Processor(false);
            Configuration config = proc.getUnderlyingConfiguration();
            config.setOutputURIResolver(new MyOutputResolver(config));

            // compile the stylesheet
            XsltTransformer trans = proc
                    .newXsltCompiler()
                    .compile(new StreamSource(new StringReader(STYLE)))
                    .load();

            // run template "main", ignore the main result
            trans.setInitialTemplate(new QName("main"));
            trans.setBaseOutputURI("http://www.example.com/");
            trans.setDestination(new XdmDestination());
            trans.transform();
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    private static class MyOutputResolver implements OutputURIResolver {
        public MyOutputResolver(Configuration config) {
            myConfig = config;
        }

        private final Configuration myConfig;
        private final Map<Result, String> expected = new HashMap<Result, String>();


        @Override
        public OutputURIResolver newInstance() {
            return new MyOutputResolver(myConfig);
        }

        private static String resolveUri(String href, String base)
                throws TransformerException {
            try {
                return new URI(base).resolve(href).toASCIIString();
            } catch (URISyntaxException ex) {
                throw new TransformerException(ex);
            }
        }


        @Override
        public Result resolve(String href, String base)
                throws TransformerException {
            String uri = resolveUri(href, base);
            XdmDestination dest = new XdmDestination();
//            try {
                Result res = dest.getReceiver(myConfig.makePipelineConfiguration(), myConfig.obtainDefaultSerializationProperties());
                System.err.println("New result: " + res);
                System.err.println("  sysid: " + uri);
                expected.put(res, uri);
                res.setSystemId(uri);
                return res;
//            } catch (SaxonApiException ex) {
//                throw new TransformerException(ex);
//            }
        }

        @Override
        public void close(Result result) {
            assertTrue(expected.get(result) != null);
        }


    }

    private static final String STYLE =
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                    "                version='2.0'>\n" +
                    "   <xsl:template name='main'>\n" +
                    "      <xsl:result-document href='foo.xml'>\n" +
                    "         <doc>Hello Foo!</doc>\n" +
                    "      </xsl:result-document>\n" +
                    "      <xsl:result-document href='bar.xml'>\n" +
                    "         <doc>Hello Bar!</doc>\n" +
                    "      </xsl:result-document>\n" +
                    "      <ignored/>\n" +
                    "   </xsl:template>\n" +
                    "</xsl:stylesheet>";

    @Test
    public void testNullPrimaryDestination() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(STYLE)));
            Xslt30Transformer t = exec.load30();
            Map<String, String> results = new ConcurrentHashMap<>();
            t.setResultDocumentHandler((uri) -> {
                StringWriter sw = new StringWriter();
                Serializer s = proc.newSerializer(sw);
                s.onClose(() -> results.put(uri.toASCIIString(), sw.toString()));
                return s;
            });
            Destination out = new NullDestination();
            out.setDestinationBaseURI(new URI("http://www.example.com/"));
            t.callTemplate(new QName("main"), out);
            assertTrue(results.size() == 2);
            assertTrue(results.get("http://www.example.com/foo.xml").contains("<doc>Hello Foo!</doc>"));
            assertTrue(results.get("http://www.example.com/bar.xml").contains("<doc>Hello Bar!</doc>"));

        } catch (SaxonApiException | URISyntaxException err) {
            fail();
        }
    }

    @Test
    public void testNullSecondaryDestination() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(STYLE)));
            Xslt30Transformer t = exec.load30();
            Map<String, String> results = new HashMap<>();
            t.setResultDocumentHandler(uri -> new NullDestination());
            Destination out = new NullDestination();
            out.setDestinationBaseURI(new URI("http://www.example.com/"));
            t.callTemplate(new QName("main"), out);

        } catch (SaxonApiException | URISyntaxException err) {
            fail();
        }
    }
}
