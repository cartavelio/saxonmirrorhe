package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import java.io.StringReader;
import java.io.StringWriter;

/**
 *
 */
public class ReceiverTests extends TestCase {

    public void testStreamWriterToReceiver() {
        // Simple test to fix a bug in 9.3
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            Receiver receiver = config.getSerializerFactory().getReceiver(new StreamResult(out));
            XMLStreamWriter sw = new StreamWriterToReceiver(receiver);
            sw.writeStartDocument();
            sw.writeEmptyElement("doc");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }

    }


    public void testNamespaceUndeclaration() {
        try {
            Processor processor = null;
            NamePool pool = null;
            Controller controller = null;
            Receiver receiver = null;


            processor = new Processor(false);
            pool = processor.getUnderlyingConfiguration().getNamePool();
            controller = new Controller(processor.getUnderlyingConfiguration());

            String xml = "<document xmlns:ex='http://example.com/ns/p'><p>A paragraph</p></document>";

            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(new SAXSource(new InputSource(new StringReader(xml))));

            Executable exec = new Executable(controller.getConfiguration());
            XdmDestination destination = new XdmDestination();
            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            receiver = destination.getReceiver(pipe, new SerializationProperties());
            receiver.setPipelineConfiguration(pipe);
            receiver.setSystemId("http://example.com/");
            //receiver = new NamespaceReducer(receiver);
            receiver = new ComplexContentOutputter(receiver);

            receiver.open();
            receiver.startDocument(0);

            NamespaceBinding onlyNewNS[] = new NamespaceBinding[0];

            XdmSequenceIterator iter = doc.axisIterator(Axis.CHILD);
            XdmNode root = (XdmNode) iter.next();

            NodeInfo info = root.getUnderlyingNode();
            NodeName name = NameOfNode.makeName(info);
            receiver.startElement(name, info.getSchemaType(), info.saveLocation(), 0);
            receiver.startContent();

            iter = root.axisIterator(Axis.CHILD);
            XdmNode p = (XdmNode) iter.next();

            info = p.getUnderlyingNode();
            name = NameOfNode.makeName(info);
            receiver.startElement(name, info.getSchemaType(), info.saveLocation(), 0);
            receiver.startContent();

            receiver.endElement();
            receiver.endDocument();

            receiver.endElement();
            receiver.endDocument();

            XdmNode result = destination.getXdmNode();

            System.out.println(result);
        } catch (SaxonApiException e) {
            e.printStackTrace();
        } catch (XPathException e) {
            e.printStackTrace();
        }


    }

}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



