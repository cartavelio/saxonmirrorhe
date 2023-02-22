package internaltests;

import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import nu.validator.htmlparser.common.XmlViolationPolicy;
import nu.validator.htmlparser.dom.HtmlDocumentBuilder;
import org.junit.Assert;
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
            String html = "<body><h1>Test<p>Hello.</body>";
            HtmlDocumentBuilder htmlBuilder = new HtmlDocumentBuilder(XmlViolationPolicy.ALTER_INFOSET);
            ByteArrayInputStream bais = new ByteArrayInputStream(html.getBytes(StandardCharsets.UTF_8));
            Document dom = htmlBuilder.parse(bais);
            DocumentBuilder documentBuilder = processor.newDocumentBuilder();
            XdmNode doc = documentBuilder.build(new DOMSource(dom));
            Assertions.assertTrue(doc.toString().contains("<body><h1>Test<p>Hello.</p></h1></body>"));
        } catch (SAXException | IOException | SaxonApiException ex) {
            throw new IllegalArgumentException("Parsing CommonMark output as HTML failed: " + ex.getMessage());
        }

    }

}
