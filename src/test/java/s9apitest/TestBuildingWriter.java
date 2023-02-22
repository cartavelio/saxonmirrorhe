package s9apitest;

import net.sf.saxon.expr.sort.CodepointCollatingComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.stream.XMLStreamException;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test BuildingStreamWriter in s9api
 */
public class TestBuildingWriter {

    @Test
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
            assertEquals(
                    "<out data=\"3\"><br/><br style=\"bold\"/></out>",
                    sr.serializeNodeToString(bsw.getDocumentNode()));
        } catch (SaxonApiException | XMLStreamException e) {
            fail(e.toString());
        }
    }

    @Test
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
                assertNull(DeepEqual.deepEquals(
                        actual.getUnderlyingNode(),
                        expected.getUnderlyingNode(),
                        CodepointCollatingComparer.getInstance(),
                        proc.getUnderlyingConfiguration().getConversionContext(),
                        0));
            } catch (SaxonApiException | XMLStreamException | XPathException e) {
                fail(e.toString());
            }
    }



}


