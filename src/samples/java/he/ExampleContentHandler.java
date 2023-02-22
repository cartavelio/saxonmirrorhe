package he;
import org.xml.sax.Attributes;
import org.xml.sax.ContentHandler;
import org.xml.sax.Locator;
import org.xml.sax.SAXException;

import java.io.PrintStream;

/**
 * This class contains a specimen implementation of the SAX {@link ContentHandler}
 * interface, which simply outputs summary details of the calls that are made
 * to methods such as {@link #startElement(String, String, String, Attributes)} and
 * {@link #endElement(String, String, String)}.
 *
 * <p>The class includes a {@code} main method so it can be called directly from
 * the command line; it then takes a single argument supplying an XML file to be
 * parsed, and reports on the SAX events produced during parsing of this file.</p>
 *
 * <p>The class is also used by other sample applications, <code>JAXPExamples</code>
 * and <code>XQJExamples</code>, to show how the output of a query or transformation
 * can be sent to a SAX <code>ContentHandler</code> destination.</p>
 */
public class ExampleContentHandler implements ContentHandler {

    private PrintStream printStream = System.out;

    /**
     * Set the destination for trace output. Defaults to {@code System.out}
     * @param stream the output destination
     */
    public void setPrintStream(PrintStream stream) {
        printStream = stream;
    }

    /**
     * Get the destination for trace output. Defaults to {@code System.out}
     * @return the output destination
     */
    public PrintStream getPrintStream() {
        return printStream;
    }

    /**
     * Supply location information for SAX events. Note that when a <code>ContentHandler</code>
     * is used to receive query or transformation results, the location information will
     * generally relate to the position in the query or stylesheet source code that caused the
     * event to be generated.
     * @param locator an object that can return the location of
     *                any SAX document event
     */

    public void setDocumentLocator(Locator locator) {
        printStream.println("setDocumentLocator");
    }

    public void startDocument() throws SAXException {
        printStream.println("startDocument");
    }

    public void endDocument() throws SAXException {
        printStream.println("endDocument");
    }

    public void startPrefixMapping(String prefix, String uri)
            throws SAXException {
        printStream.println("startPrefixMapping: " + prefix + ", " + uri);
    }

    public void endPrefixMapping(String prefix) {
        printStream.println("endPrefixMapping: " + prefix);
    }

    public void startElement(
            String namespaceURI, String localName, String qName, Attributes atts) {

        printStream.print("startElement: " + namespaceURI + ", "
                         + localName + ", " + qName);

        int n = atts.getLength();

        for (int i = 0; i < n; i++) {
            printStream.print(", " + atts.getQName(i) + "='" + atts.getValue(i) + "'");
        }

        printStream.println("");
    }

    public void endElement(
            String namespaceURI, String localName, String qName) {
        printStream.println("endElement: " + namespaceURI + ", "
                           + localName + ", " + qName);
    }

    public void characters(char[] ch, int start, int length) {

        String s = new String(ch, start, Math.min(length, 30));

        if (length > 30) {
            printStream.println("characters: \"" + s + "\"...");
        } else {
            printStream.println("characters: \"" + s + "\"");
        }
    }

    public void ignorableWhitespace(char[] ch, int start, int length) {
        printStream.println("ignorableWhitespace");
    }

    public void processingInstruction(String target, String data) {
        printStream.println("processingInstruction: " + target + ", "
                           + data);
    }

    public void skippedEntity(String name) {
        printStream.println("skippedEntity: " + name);
    }

    /**
     * Main entry point. Expects a single argument containing the name of an XML file to be parsed.
     * @param args Command line arguments
     * @throws Exception if the file is not well-formed XML
     */
    public static void main(String[] args) throws Exception {
        org.xml.sax.XMLReader parser = 
            javax.xml.parsers.SAXParserFactory.newInstance().newSAXParser().getXMLReader();
        System.err.println("Parser: " + parser.getClass());
        parser.setContentHandler(new ExampleContentHandler());
        parser.parse(new java.io.File(args[0]).toURI().toString());
    }
}
