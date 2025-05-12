package internaltests;

import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import nu.validator.htmlparser.common.XmlViolationPolicy;
import nu.validator.htmlparser.dom.HtmlDocumentBuilder;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.xml.sax.SAXException;

import javax.xml.transform.dom.DOMSource;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;

public class ParseHtml {

    @Test
    public void parseHtmlSmokeTest() {
        try {
            Processor processor = new Processor(false);
            String html = "<body><h1>Test</h1><p>Hello.</body>";
            HtmlDocumentBuilder htmlBuilder = new HtmlDocumentBuilder(XmlViolationPolicy.ALTER_INFOSET);
            ByteArrayInputStream bais = new ByteArrayInputStream(html.getBytes(StandardCharsets.UTF_8));
            Document dom = htmlBuilder.parse(bais);
            DocumentBuilder documentBuilder = processor.newDocumentBuilder();
            XdmNode doc = documentBuilder.build(new DOMSource(dom));
            Assertions.assertTrue(doc.toString().contains("<html"));
            Assertions.assertTrue(doc.toString().contains("<h1>Test</h1>"));
            Assertions.assertTrue(doc.toString().contains("<p>Hello.</p>"));
        } catch (SAXException | IOException | SaxonApiException ex) {
            throw new IllegalArgumentException("Parsing HTML failed: " + ex.getMessage());
        }

    }

}
