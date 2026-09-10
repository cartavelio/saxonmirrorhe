package bugs.bug6163;

import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;
import nu.validator.htmlparser.common.XmlViolationPolicy;
import nu.validator.htmlparser.dom.HtmlDocumentBuilder;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.transform.OutputKeys;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;

public class Bug6163 {
    private Processor processor;
    private Builder outerBuilder;
    private final NullLocation voidLocation = new NullLocation();

    @Test
    public void bugTest() throws XPathException, SaxonApiException, SAXException, IOException {
        processor = new Processor(false);
        outerBuilder = new Builder();
        outerBuilder.startDocument();

        outerBuilder.startElement("class", "nesting", "top_level", "type", "XmlFieldX", "fulltype", "com.saxonica.xmldoclet.XmlFieldX");
        outerBuilder.startElement("method", "public", "true", "name", "testing");

        description();

        outerBuilder.endElement();

        outerBuilder.endDocument();
        String xml = outerBuilder.getXml();
        System.err.println(xml);

        Assertions.assertTrue(xml.contains("<ref signature="));
    }

    private void parseMarkup(String wrapper, String markup) throws SaxonApiException, XPathException, IOException, SAXException {
        ByteArrayInputStream bais = new ByteArrayInputStream(markup.getBytes(StandardCharsets.UTF_8));
        HtmlDocumentBuilder htmlBuilder = new HtmlDocumentBuilder(XmlViolationPolicy.ALTER_INFOSET);
        Document html = htmlBuilder.parse(bais);
        Processor processor = new Processor(false);
        DocumentBuilder xbuilder = processor.newDocumentBuilder();
        XdmNode doc = xbuilder.build(new DOMSource(html));

        String xml = doc.toString();

        // Reparse to strip out the namespace. Hack.
        int pos = xml.indexOf("<body>");
        xml = xml.substring(pos);
        pos = xml.lastIndexOf(("</body>"));
        xml = xml.substring(0, pos+7);

        xbuilder = processor.newDocumentBuilder();
        bais = new ByteArrayInputStream(xml.getBytes(StandardCharsets.UTF_8));
        SAXSource source = new SAXSource(new InputSource(bais));
        XdmNode node = xbuilder.build(source);

        XPathCompiler compiler = processor.newXPathCompiler();
        XPathExecutable exec = compiler.compile("/body");
        XPathSelector selector = exec.load();
        selector.setContextItem(node);
        XdmValue selection = selector.evaluate();

        outerBuilder.startElement(wrapper);
        outerBuilder.addChildren((XdmNode) selection);
        outerBuilder.endElement();
    }

    private class Builder {
        private final Receiver receiver;
        private final XdmDestination destination;
        private final boolean forHTML;

        public Builder() {
            this(false);
        }

        public Builder(boolean forHTML) {
            this.forHTML = forHTML;
            Controller controller = new Controller(processor.getUnderlyingConfiguration());
            destination = new XdmDestination();
            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            Receiver receiver1 = destination.getReceiver(pipe, new SerializationProperties());
            receiver1.setPipelineConfiguration(pipe);
            receiver1.setSystemId("https://example.com/");
            receiver = new ComplexContentOutputter(new NamespaceReducer(receiver1));
        }

        public String getXml() throws SaxonApiException {
            SerializationProperties serprop = new SerializationProperties();
            serprop.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
            serprop.setProperty(OutputKeys.INDENT, "yes");
            if (forHTML) {
                serprop.setProperty(OutputKeys.METHOD, "html");
                serprop.setProperty(OutputKeys.VERSION, "5");
            }
            Serializer serializer = processor.newSerializer();
            serializer.setOutputProperties(serprop);
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            serializer.setOutputStream(baos);
            serializer.serializeNode(destination.getXdmNode());
            return baos.toString();
        }

        public void startDocument() throws XPathException {
            receiver.open();
            receiver.startDocument(0);
        }

        public void startElement(String name, String... pairs) throws XPathException {
            if (pairs.length %2 != 0) {
                throw new IllegalArgumentException("Odd number of name/value pairs for attributes");
            }

            Map<String,String> attr = new HashMap<>();
            for (int pos = 0; pos < pairs.length; pos += 2) {
                attr.put(pairs[pos], pairs[pos+1]);
            }

            startElement(name, attr);
        }

        public void startElement(String name, Map<String,String> attributes) throws XPathException {
            //System.err.printf("Start: %s%n", name);
            AttributeMap amap = EmptyAttributeMap.getInstance();
            if (attributes != null && !attributes.isEmpty()) {
                for (String aname : attributes.keySet()) {
                    //System.err.printf("  %s=%s%n", aname, attributes.get(aname));
                    FingerprintedQName qname = new FingerprintedQName("", NamespaceUri.of(""), aname);
                    AttributeInfo ainfo = new AttributeInfo(qname, BuiltInAtomicType.UNTYPED_ATOMIC, attributes.get(aname), voidLocation, 0);
                    amap = amap.put(ainfo);
                }
            }
            startElement(name, amap);
        }

        private void startElement(String name, AttributeMap attributes) {
            try {
                FingerprintedQName qname = new FingerprintedQName("", NamespaceUri.of(""), name);
                receiver.startElement(qname, Untyped.INSTANCE, attributes, NamespaceMap.emptyMap(), voidLocation, 0);
            } catch (XPathException ex) {
                throw new RuntimeException(ex);
            }
        }

        public void endElement() throws XPathException {
            //System.err.println("end element");
            receiver.endElement();
        }

        public void addSubtree(XdmNode node) throws XPathException {
            //System.err.println("Add subtree: " + node);
            if (node.getNodeKind() == XdmNodeKind.DOCUMENT) {
                addChildren(node);
            } else if (node.getNodeKind() == XdmNodeKind.ELEMENT) {
                startElement(node);
                addChildren(node);
                endElement();
            } else if (node.getNodeKind() == XdmNodeKind.COMMENT) {
                comment(node.getStringValue());
            } else if (node.getNodeKind() == XdmNodeKind.TEXT) {
                text(node.getStringValue());
            } else if (node.getNodeKind() == XdmNodeKind.PROCESSING_INSTRUCTION) {
                processingInstruction(node.getNodeName().getLocalName(), node.getStringValue());
            } else {
                throw new UnsupportedOperationException("Unexpected node type");
            }
        }

        public void addChildren(XdmNode node) throws XPathException {
            XdmSequenceIterator<XdmNode> iter = node.axisIterator(Axis.CHILD);
            while (iter.hasNext()) {
                XdmNode child = iter.next();
                addSubtree(child);
            }
        }

        private void startElement(XdmNode node) throws XPathException {
            System.err.println("startElement: " + node);
            AttributeMap amap = EmptyAttributeMap.getInstance();
            for (AttributeInfo info : node.getUnderlyingNode().attributes().asList()) {
                FingerprintedQName fqname = new FingerprintedQName("", NamespaceUri.of(""), info.getNodeName().getLocalPart());
                //AttributeInfo newInfo = new AttributeInfo(fqname, info.getType(), info.getValue(), info.getLocation(), info.getProperties());
                AttributeInfo newInfo = new AttributeInfo(info.getNodeName(), info.getType(), info.getValue(), info.getLocation(), info.getProperties());
                amap = amap.put(newInfo);
            }

            FingerprintedQName qname = new FingerprintedQName("", NamespaceUri.of(""), node.getNodeName().getLocalName());
            receiver.startElement(qname, Untyped.INSTANCE, amap, NamespaceMap.emptyMap(), voidLocation, 0);
        }

        public void comment(String text) throws XPathException {
            //System.err.println("Comment: " + text);
            receiver.comment(StringView.of(text), voidLocation, 0);
        }

        public void processingInstruction(String target, String data) throws XPathException {
            //System.err.println("PI: " + target + " " + data);
            receiver.processingInstruction(target, StringView.of(data), voidLocation, 0);
        }

        public void text(String text) throws XPathException {
            //System.err.printf("char: [[%s]]%n", text);
            receiver.characters(StringView.of(text), voidLocation, 0);
        }

        public void endDocument() throws XPathException {
            receiver.endDocument();
            receiver.close();
        }

    }

    private void description() throws XPathException, SaxonApiException, SAXException, IOException {
        Builder builder = new Builder(true);
        builder.startDocument();
        builder.startElement("body");
        builder.startElement("p");
        builder.startElement("ref", "signature", "XmlProcessor");
        builder.endElement();
        builder.endElement();
        builder.endElement();
        builder.endDocument();
        String xml = builder.getXml();

        DocumentBuilder xbuilder = processor.newDocumentBuilder();
        ByteArrayInputStream bais = new ByteArrayInputStream(xml.getBytes(StandardCharsets.UTF_8));
        SAXSource source = new SAXSource(new InputSource(bais));
        XdmNode node = xbuilder.build(source);

        parseMarkup("description", xml);
    }

    public class NullLocation implements Location {
        public NullLocation() {
            // nothing
        }

        @Override
        public int getColumnNumber() {
            return -1;
        }

        @Override
        public int getLineNumber() {
            return -1;
        }

        @Override
        public String getPublicId() {
            return null;
        }

        @Override
        public String getSystemId() {
            return null;
        }

        @Override
        public Location saveLocation() {
            return this;
        }
    }
}
