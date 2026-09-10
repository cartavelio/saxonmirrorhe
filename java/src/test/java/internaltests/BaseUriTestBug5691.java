////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.net.URI;
import java.util.ArrayList;
import java.util.Collections;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Test clearance of bug 5691 - processing instructions interfering with base URI
 */
public class BaseUriTestBug5691 {

    private final static String mainXml = "<root><include/></root>";
    private final static String moduleXml = "<module><foo/><?pi?><bar/></module>";

    @Test
    public void testBug5691() throws SaxonApiException {
        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        XdmNode main = builder.build(new StreamSource(new StringReader(mainXml), "http://example.com/main.xml"));
        XdmNode module = builder.build(new StreamSource(new StringReader(moduleXml), "http://example.com/module.xml"));

        TreeWriter tree = new TreeWriter(processor);
        tree.startDocument(main.getBaseURI());

        XdmSequenceIterator<XdmNode> iter = main.axisIterator(Axis.CHILD);
        XdmNode root = null;
        while (iter.hasNext()) {
            XdmNode node = iter.next();
            if (XdmNodeKind.ELEMENT.equals(node.getNodeKind())) {
                root = node;
                break;
            }
        }

        assert root != null;

        iter = root.axisIterator(Axis.CHILD);
        while (iter.hasNext()) {
            XdmNode node = iter.next();
            if ("include".equals(node.getNodeName().getLocalName())) {
                tree.addSubtree(module);
            } else {
                tree.addSubtree(node);
            }
        }
        tree.endDocument();

        showtree(tree.getResult(), "");
    }

    private void showtree(XdmNode node, String indent) {
        XdmSequenceIterator<XdmNode> iter;

        switch (node.getNodeKind()) {
            case DOCUMENT:
                iter = node.axisIterator(Axis.CHILD);
                while (iter.hasNext()) {
                    showtree(iter.next(), indent);
                }
                break;

            case PROCESSING_INSTRUCTION:
                assertEquals("http://example.com/main.xml", node.getBaseURI().toASCIIString());
                assertEquals("http://example.com/main.xml", node.getUnderlyingNode().getSystemId());
                break;

            case ELEMENT:
                assertEquals("http://example.com/main.xml", node.getBaseURI().toASCIIString());
                assertEquals("http://example.com/main.xml", node.getUnderlyingNode().getSystemId());
                iter = node.axisIterator(Axis.CHILD);
                while (iter.hasNext()) {
                    showtree(iter.next(), "  " + indent);
                }

            default:
                break;
        }
    }


    public static class TreeWriter {

        protected static final AttributeMap emptyAttributeMap = EmptyAttributeMap.getInstance();
        protected Controller controller = null;
        protected Executable exec = null;
        protected NamePool pool = null;
        protected XdmDestination destination = null;
        protected Receiver receiver = null;
        protected boolean seenRoot = false;
        protected boolean inDocument = false;

        /*
         * Creates a new instance of ProcessMatch
         */
        public TreeWriter(Processor processor) {
            pool = processor.getUnderlyingConfiguration().getNamePool();
            controller = new Controller(processor.getUnderlyingConfiguration());
        }

        public XdmNode getResult() {
            return destination.getXdmNode();
        }

        public void startDocument(URI baseURI) {
            inDocument = true;
            seenRoot = false;
            try {
                exec = new Executable(controller.getConfiguration());
                destination = new XdmDestination();
                PipelineConfiguration pipe = controller.makePipelineConfiguration();
                receiver = destination.getReceiver(pipe, new SerializationProperties());

                receiver.setPipelineConfiguration(pipe);

                if (baseURI != null) {
                    receiver.setSystemId(baseURI.toASCIIString());
                } else {
                    receiver.setSystemId("http://example.com/");
                }

                receiver = new ComplexContentOutputter(new NamespaceReducer(receiver));

                receiver.open();
                receiver.startDocument(0);
            } catch (Exception e) {
                throw new RuntimeException(e);
            }
        }

        public void endDocument() {
            try {
                receiver.setSystemId("http://example.com/you/should/never/see/this");
                receiver.endDocument();
                receiver.close();
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }

        public void addSubtree(XdmNode node) {
            // This is the bit that exercises https://saxonica.plan.io/issues/5691
            try {
                receiver.append(node.getUnderlyingNode());
            } catch (XPathException ex) {
                // What happened? Well, do it the hard way anyway.
                if (node.getNodeKind() == XdmNodeKind.DOCUMENT) {
                    writeChildren(node);
                } else if (node.getNodeKind() == XdmNodeKind.ELEMENT) {
                    addStartElement(node);
                    writeChildren(node);
                    addEndElement();
                } else if (node.getNodeKind() == XdmNodeKind.COMMENT) {
                    addComment(node.getStringValue());
                } else if (node.getNodeKind() == XdmNodeKind.TEXT) {
                    addText(node.getStringValue());
                } else if (node.getNodeKind() == XdmNodeKind.PROCESSING_INSTRUCTION) {
                    addPI(node.getNodeName().getLocalName(), node.getStringValue());
                } else {
                    throw new UnsupportedOperationException("Unexpected node type");
                }
            }
        }

        protected void writeChildren(XdmNode node) {
            XdmSequenceIterator<XdmNode> iter = node.axisIterator(Axis.CHILD);
            while (iter.hasNext()) {
                XdmNode child = (XdmNode) iter.next();
                addSubtree(child);
            }
        }

        public void addStartElement(QName nodeName) {
            addStartElement(nodeName, EmptyAttributeMap.getInstance());
        }

        public void addStartElement(XdmNode node) {
            addStartElement(node, node.getNodeName(), node.getBaseURI());
        }

        public void addStartElement(XdmNode node, URI overrideBaseURI) {
            addStartElement(node, node.getNodeName(), overrideBaseURI);
        }

        public void addStartElement(XdmNode node, QName newName) {
            addStartElement(node, newName, node.getBaseURI());
        }

        public void addStartElement(XdmNode node, QName newName, URI overrideBaseURI) {
            AttributeMap attrs = node.getUnderlyingNode().attributes();
            addStartElement(node, newName, overrideBaseURI, attrs);
        }

        public void addStartElement(XdmNode node, AttributeMap attrs) {
            NodeInfo inode = node.getUnderlyingNode();
            addStartElement(NameOfNode.makeName(inode), attrs, inode.getSchemaType(), inode.getAllNamespaces(), node.getBaseURI());
        }

        public void addStartElement(XdmNode node, QName newName, URI overrideBaseURI, AttributeMap attrs) {
            NodeInfo inode = node.getUnderlyingNode();

            NamespaceMap inscopeNS = null;
            if (seenRoot) {
                ArrayList<NamespaceBinding> nslist = new ArrayList<>();
                Collections.addAll(nslist, inode.getDeclaredNamespaces(null));
                inscopeNS = new NamespaceMap(nslist);
            } else {
                inscopeNS = inode.getAllNamespaces();
            }

            // If the newName has no prefix, then make sure we don't pass along some other
            // binding for the default namespace...
            if ("".equals(newName.getPrefix()) && !"".equals(inscopeNS.getDefaultNamespace())) {
                inscopeNS.remove("");
            }

            // Hack. See comment at top of file
            if (overrideBaseURI != null && !"".equals(overrideBaseURI.toASCIIString())) {
                receiver.setSystemId(overrideBaseURI.toASCIIString());
            }

            FingerprintedQName newNameOfNode = new FingerprintedQName(newName.getPrefix(), newName.getNamespaceUri(),
                                                                      newName.getLocalName());
            addStartElement(newNameOfNode, attrs, inode.getSchemaType(), inscopeNS);
        }

        public void addStartElement(QName newName, AttributeMap attrs) {
            addStartElement(newName, attrs, NamespaceMap.emptyMap());
        }

        public void addStartElement(QName newName, AttributeMap attrs, NamespaceMap nsmap) {
            NodeName elemName = new FingerprintedQName(newName.getPrefix(), newName.getNamespaceUri(), newName.getLocalName());
            addStartElement(elemName, attrs, Untyped.INSTANCE, nsmap);
        }

        public void addStartElement(NodeName elemName, SchemaType typeCode) {
            addStartElement(elemName, emptyAttributeMap, typeCode, NamespaceMap.emptyMap());
        }

        public void addStartElement(NodeName elemName, SchemaType typeCode, NamespaceMap nsmap) {
            addStartElement(elemName, emptyAttributeMap, typeCode, nsmap);
        }

        public void addStartElement(NodeName elemName, AttributeMap attrs, SchemaType typeCode, NamespaceMap nsmap, URI overrideBaseURI) {
            // Hack. See comment at top of file
            if (overrideBaseURI != null && !"".equals(overrideBaseURI.toASCIIString())) {
                receiver.setSystemId(overrideBaseURI.toASCIIString());
            }
            addStartElement(elemName, attrs, typeCode, nsmap);
        }

        public void addStartElement(NodeName elemName, AttributeMap attrs, SchemaType typeCode, NamespaceMap nsmap) {
            // Sort out the namespaces...
            nsmap = updateMap(nsmap, elemName.getPrefix(), elemName.getNamespaceUri());
            for (AttributeInfo attr : attrs) {
                if (attr.getNodeName().getURI() != null && !attr.getNodeName().getNamespaceUri().isEmpty()) {
                    nsmap = updateMap(nsmap, attr.getNodeName().getPrefix(), attr.getNodeName().getNamespaceUri());
                }
            }

            Location loc;
            String sysId = receiver.getSystemId();
            if (sysId == null) {
                loc = Loc.NONE;
            } else {
                loc = new SysIdLocation(sysId);
            }

            try {
                receiver.startElement(elemName, typeCode, attrs, nsmap, loc, 0);
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }

        private NamespaceMap updateMap(NamespaceMap nsmap, String prefix, NamespaceUri uri) {
            if (uri == null || uri.isEmpty()) {
                return nsmap;
            }

            if (prefix == null || "".equals(prefix)) {
                if (!uri.equals(nsmap.getDefaultNamespace())) {
                    return nsmap.put("", uri);
                }
            }

            NamespaceUri curNS = nsmap.getNamespaceUri(prefix);
            if (curNS == null) {
                return nsmap.put(prefix, uri);
            } else if (curNS.equals(uri)) {
                return nsmap;
            }

            throw new RuntimeException("Cannot add " + prefix + " to namespace map with URI " + uri);
        }

        public void addEndElement() {
            try {
                receiver.endElement();
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }


        public void addComment(String comment) {
            try {
                receiver.comment(StringView.of(comment), Loc.NONE, 0);
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }

        public void addText(String text) {
            try {
                receiver.characters(StringView.of(text), Loc.NONE, 0);
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }

        public void addPI(String target, String data) {
            addPI(target, data, Loc.NONE);
        }

        public void addPI(String target, String data, Location location) {
            try {
                receiver.processingInstruction(target, StringView.of(data), location, 0);
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }

        public void addPI(XdmNode node) {
            Location location = Loc.NONE;
            if (node.getBaseURI() != null) {
                location = new SysIdLocation(node.getBaseURI().toString());
                receiver.setSystemId(node.getBaseURI().toString());
            }
            addPI(node.getNodeName().getLocalName(), node.getStringValue(), location);
        }
    }

    public static class SysIdLocation
            implements Location {
        public SysIdLocation(String sysid) {
            this.sysid = sysid;
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
            return sysid;
        }

        @Override
        public Location saveLocation() {
            return this;
        }

        private final String sysid;
    }


}

