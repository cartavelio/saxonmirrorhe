package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.expr.sort.CodepointCollatingComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;

import javax.xml.stream.XMLStreamException;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * test BuildingStreamWriter in s9api
 */
public class TestBuildingWriter extends TestCase {

    public void testSimple() {
        try {
            Processor proc = new Processor(false);
            BuildingStreamWriter bsw = proc.newDocumentBuilder().newBuildingStreamWriter();
            bsw.writeStartDocument();
            bsw.writeStartElement("out");
            bsw.writeAttribute("data", "3");
            bsw.writeEmptyElement("br");
            bsw.writeEmptyElement("br");
            bsw.writeAttribute("style", "bold");
            bsw.writeEndElement();
            bsw.writeEndDocument();
            Serializer sr = proc.newSerializer();
            sr.setOutputProperty(Serializer.Property.INDENT, "no");
            sr.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            assertEquals(sr.serializeNodeToString(bsw.getDocumentNode()),
                    "<out data=\"3\"><br/><br style=\"bold\"/></out>");
        } catch (SaxonApiException e) {
            fail(e.toString());
        } catch (XMLStreamException e) {
            fail(e.toString());
        }
    }

    public void testAutoPrefixes() {
            try {
                Processor proc = new Processor(false);
                BuildingStreamWriter bsw = proc.newDocumentBuilder().newBuildingStreamWriter();
                bsw.writeStartDocument();
                bsw.writeStartElement("http://outer/", "out");
                bsw.writeAttribute("data", "3");
                bsw.writeEmptyElement("http://outer/", "br");
                bsw.writeEmptyElement("http://inner/", "br");
                bsw.writeAttribute("style", "bold");
                bsw.writeEndElement();
                bsw.writeEndDocument();
                XdmNode actual = bsw.getDocumentNode();
                String ref = "<ns0:out xmlns:ns0=\"http://outer/\" data=\"3\"><ns0:br/><ns1:br xmlns:ns1=\"http://inner/\" style=\"bold\"/></ns0:out>";
                DocumentBuilder db = proc.newDocumentBuilder();
                XdmNode expected = db.build(new StreamSource(new StringReader(ref)));
                assertTrue(DeepEqual.deepEquals(
                        actual.getUnderlyingNode(),
                        expected.getUnderlyingNode(),
                        CodepointCollatingComparer.getInstance(),
                        proc.getUnderlyingConfiguration(),
                        proc.getUnderlyingConfiguration().getErrorListener(),
                        0));
            } catch (SaxonApiException e) {
                fail(e.toString());
            } catch (XPathException e) {
                fail(e.toString());
            } catch (XMLStreamException e) {
                fail(e.toString());
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



