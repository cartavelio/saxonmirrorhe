package com.saxonica.functions.extfn;

import jaxptest.ConfigTest;
import net.sf.saxon.junit.TestHttpServer;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import spark.Route;

import javax.xml.transform.sax.SAXSource;
import java.io.ByteArrayInputStream;
import java.nio.file.Path;
import java.nio.file.Paths;

import static org.junit.jupiter.api.Assertions.fail;

public class LastModifiedTest {
    private final Path testDataDirPath = Paths.get(new ConfigTest().getDataDir()).resolve("collectionTests");
    private TestHttpServer testHttp = new TestHttpServer(testDataDirPath);

    @Test
    public void lastModifiedXPath() {
        try {
            testHttp.route(http -> {
                http.get("/index.html", lastModifiedRoute());
            });

            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("saxon:last-modified('" + testHttp.url("/index.html") + "')");
            XPathSelector eval = exec.load();
            XdmValue val = eval.evaluate();
            Assertions.assertEquals("2022-06-06T12:34:56Z", val.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        } finally {
            testHttp.stop();
        }
    }

    @Test
    public void lastModifiedXSLT() {
        try {
            testHttp.route(http -> {
                http.get("/index.html", lastModifiedRoute());
            });

            String xslt = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                version=\"3.0\"\n" +
                    "                xmlns:saxon=\"http://saxon.sf.net/\"\n" +
                    "                exclude-result-prefixes=\"#all\">\n" +
                    "  <xsl:output indent=\"no\"/>\n" +
                    "  <xsl:template name=\"main\">\n" +
                    "    <out>\n" +
                    "      <xsl:value-of select=\"saxon:last-modified('http://www.saxonica.com/index.html')\"/>\n" +
                    "    </out>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:stylesheet>";
            ByteArrayInputStream stream = new ByteArrayInputStream(xslt.getBytes());
            SAXSource source = new SAXSource(new InputSource(stream));

            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable ex = c.compile(source);
            XsltTransformer t = ex.load();
            t.setInitialTemplate(new QName("main"));
            XdmDestination destination = new XdmDestination();
            t.setDestination(destination);
            t.transform();
            XdmNode out = destination.getXdmNode();
            Assertions.assertEquals("<out>2023-12-06T13:25:00Z</out>", out.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        } finally {
            testHttp.stop();
        }
    }

    private Route lastModifiedRoute() {
        String fileBody = "TEST";
        return (request, response) -> {
            response.status(200);
            response.header("Last-Modified", "Mon, 6 June 2022 12:34:56 GMT");
            return fileBody;
        };
    }
}
