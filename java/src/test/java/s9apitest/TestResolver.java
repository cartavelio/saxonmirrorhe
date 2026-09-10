package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.fail;

public class TestResolver {
    @Test
    public void transformPassthrough() {
        Map<String,String> documents = new HashMap<>();

        documents.put("sample1.xml", "<root>Sample 1</root>");
        documents.put("sample2.xml", "<root>Sample 2</root>");
        documents.put("sheet1.xsl", "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:mode on-no-match='shallow-copy'/>" +
                "<xsl:template match='/'><xsl:next-match/><xsl:copy-of select='doc(\"sample2.xml\")'/></xsl:template>" +
                "</xsl:stylesheet>");

        Processor processor = new Processor(false);

        XPathCompiler xpathCompiler = processor.newXPathCompiler();

        try {
            XPathExecutable xpathExecutable = xpathCompiler.compile("doc('sample1.xml'), doc('sample2.xml'), doc('sheet1.xsl')");

            XPathSelector xpathSelector = xpathExecutable.load();

            xpathSelector.setResourceResolver(resourceRequest -> {
                if (documents.containsKey(resourceRequest.relativeUri)) {
                    return new StreamSource(new StringReader(documents.get(resourceRequest.relativeUri)));
                }
                return null;
            });

            XdmValue result = xpathSelector.evaluate();

            Assertions.assertTrue(result.toString().contains("<root>Sample 1<"));
            Assertions.assertTrue(result.toString().contains("<root>Sample 2<"));

            xpathExecutable = xpathCompiler.compile("transform(map { 'source-node' : doc('sample1.xml'), 'stylesheet-node' : doc('sheet1.xsl'), 'delivery-format' : 'serialized' })");

            xpathSelector = xpathExecutable.load();

            xpathSelector.setResourceResolver(resourceRequest -> {
                if (documents.containsKey(resourceRequest.relativeUri)) {
                    return new StreamSource(new StringReader(documents.get(resourceRequest.relativeUri)));
                }
                return null;
            });

            result = xpathSelector.evaluate();

            Assertions.assertTrue(result.toString().contains("<root>Sample 1<"));
            Assertions.assertTrue(result.toString().contains("<root>Sample 2<"));
        } catch (SaxonApiException e) {
            System.out.println(e.getMessage());
            e.printStackTrace();
            fail();
        }

    }

}
