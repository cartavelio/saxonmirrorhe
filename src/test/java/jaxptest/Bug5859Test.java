package jaxptest;


import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertTrue;

public class Bug5859Test {
    public final static String dir = "/Users/mike/Bugs/2023/5859-Claeys/";


    @Test
    void testDomResult() throws TransformerFactoryConfigurationError, TransformerException {
        Source input = new StreamSource(new File(dir + "src/main/resources/xml/input.xml"));

        DOMResult tempResult = transform(input, dir + "src/main/resources/xslt/create-namespace-elements.xslt");
        // logNode(result.getNode());

        DOMResult domResult = new DOMResult();
        transform(new DOMSource(tempResult.getNode()), "pie", getConstantElement(),
                  dir + "src/main/resources/xslt/final.xslt", domResult);

        StringWriter writer = new StringWriter();
        Transformer transformer = TransformerFactory.newInstance().newTransformer();
        transformer.transform(new DOMSource(domResult.getNode()), new StreamResult(writer));
        String result = writer.toString();

        // check if both namespaces are declared somewhere in the result
        assertTrue(result
                .contains("http://kszbcss.fgov.be/intf/Abc/invented/v1"));
        assertTrue(result
                .contains("http://www.w3.org/2005/05/xmlmime"));
    }

    @Test
    void testStreamResult() throws TransformerFactoryConfigurationError, TransformerException {
        Source input = new StreamSource(new File(dir + "src/main/resources/xml/input.xml"));

        DOMResult tempResult = transform(input, dir + "src/main/resources/xslt/create-namespace-elements.xslt");

        StringWriter writer = new StringWriter();
        StreamResult streamResult = new StreamResult(writer);
        transform(new DOMSource(tempResult.getNode()), "pie", getConstantElement(),
                  dir + "src/main/resources/xslt/final.xslt", streamResult);

        String stringResult = writer.toString();
        // check if both namespaces are declared somewhere in the result
        assertTrue(stringResult
                           .contains("http://kszbcss.fgov.be/intf/Abc/invented/v1"));
        assertTrue(stringResult
                           .contains("http://www.w3.org/2005/05/xmlmime"));
       
    }

    private DOMResult transform(Source input, String xsltName)
            throws TransformerFactoryConfigurationError, TransformerException {
        Source xsltSource = new StreamSource(new File(xsltName));
        Templates transformerTemplate = TransformerFactory.newInstance().newTemplates(xsltSource);

        Transformer transformer = transformerTemplate.newTransformer();

        DOMResult xmlResult = new DOMResult();
        transformer.transform(input, xmlResult);

        return xmlResult;
    }

    private void transform(Source input, String paramName, Node param, String xsltName,
                           Result result) throws TransformerFactoryConfigurationError, TransformerException {
        Source xsltSource = new StreamSource(new File(xsltName));
        Templates transformerTemplate = TransformerFactory.newInstance().newTemplates(xsltSource);

        Transformer transformer = transformerTemplate.newTransformer();
        transformer.setParameter(paramName, param);

        transformer.transform(input, result);
    }

    private Element getConstantElement() {
        DocumentBuilderFactory builderFact = DocumentBuilderFactory.newDefaultInstance();
        DocumentBuilder builder;
        try {
            builder = builderFact.newDocumentBuilder();
        } catch (ParserConfigurationException e) {
            throw new RuntimeException(e);
        }
        Document doc = builder.newDocument();
        Element element = doc.createElement("test");
        element.setAttributeNS("http://kszbcss.fgov.be/intf/Abc/invented/v1", "ab:c", "someValue");
        return element;
    }

}
