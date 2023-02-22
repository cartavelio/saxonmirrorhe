package ee;

import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.*;
import nu.xom.*;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;

import static net.sf.saxon.s9api.streams.Steps.child;

/**
 * A simple example to show how Saxon can be used with a XOM tree.
 * It is designed to be used with the source document books.xml and the
 * stylesheet total.xsl
 */
public class XOMExample {

    private XOMExample() {}

    /**
     * Method main
     */
    public static void main(String[] argv)
            throws SaxonApiException, IOException, ParsingException {

        if (argv.length == 2) {
            transform(argv[0], argv[1]);
        } else {
            System.err.println("Usage: XOMExample source.xml style.xsl >out.xml");
        }

    }

    /**
     * Show (a) use of freestanding XPath expressions against the JDOM document, and
     * (b) the simplest possible transformation from system id to output stream.
     */

    public static void transform(String sourceID, String xslID)
            throws SaxonApiException, IOException, ParsingException {



        // Get a Saxon processor and document builder
        Processor processor = new Processor(true);
        processor.getUnderlyingConfiguration().registerExternalObjectModel(XOMObjectModel.getInstance());
        DocumentBuilder documentBuilder = processor.newDocumentBuilder();

        // Build the XOM document
        Builder builder = new Builder();
        Document doc = builder.build(new File(sourceID));

        // Give it a Saxon wrapper
        XdmNode wrappedNode = documentBuilder.wrap(doc);

        // Retrieve all the ITEM elements
        XPathCompiler xpathCompiler = processor.newXPathCompiler();
        XPathSelector exp = xpathCompiler.compile("//ITEM").load();
        exp.setContextItem(wrappedNode);
        XdmValue result = exp.evaluate();

        // For element returned, compute an additional attribute

        for (XdmItem item : result) {
            Element xomElement = (Element) ((XdmNode) item).getExternalNode();
            String price = item.select(child("PRICE")).firstItem().getStringValue();
            String quantity = item.select(child("QUANTITY")).firstItem().getStringValue();
            try {
                double priceval = Double.parseDouble(price);
                double quantityval = Double.parseDouble(quantity);
                double value = priceval * quantityval;
                xomElement.addAttribute(new Attribute("VALUE", "" + value));
            } catch (NumberFormatException err) {
                xomElement.addAttribute(new Attribute("VALUE", "?"));
            }
        }

        // Compile the stylesheet

        XsltCompiler xsltCompiler = processor.newXsltCompiler();
        Xslt30Transformer transformer = xsltCompiler.compile(new StreamSource(xslID)).load30();

        // Now do a transformation
        
        transformer.transform(wrappedNode.asSource(), processor.newSerializer(System.out));

    }
    


}
