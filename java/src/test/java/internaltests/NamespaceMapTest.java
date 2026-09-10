package internaltests;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.type.BuiltInAtomicType;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Tests of the NamespaceMap class
 */
public class NamespaceMapTest {

    @Test
    public void testIncrementalAdd() {
        try {
            NamespaceMap map = NamespaceMap.emptyMap();
            map = map.put("xml", NamespaceUri.XML);
            map = map.put("xsl", NamespaceUri.XSLT);
            map = map.put("saxon", NamespaceUri.SAXON);
            assertEquals(NamespaceUri.SAXON, map.getNamespaceUri("saxon"));
            assertEquals(NamespaceUri.XSLT, map.getNamespaceUri("xsl"));
            map = map.put("xsl", NamespaceUri.MAP_FUNCTIONS);
            assertEquals(NamespaceUri.MAP_FUNCTIONS, map.getNamespaceUri("xsl"));
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testIncrementalRemove() {
        try {
            NamespaceMap map = NamespaceMap.emptyMap();
            map = map.put("xsl", NamespaceUri.XSLT);
            map = map.put("xml", NamespaceUri.XML);
            map = map.put("saxon", NamespaceUri.SAXON);
            assertEquals(NamespaceUri.SAXON, map.getNamespaceUri("saxon"));
            assertEquals(NamespaceUri.XSLT, map.getNamespaceUri("xsl"));
            map = map.put("xsl", NamespaceUri.NULL);
            assertNull(map.getNamespaceUri("xsl"));
            assertEquals(NamespaceUri.SAXON, map.getNamespaceUri("saxon"));
            map = map.put("saxon", null);
            assertNull(map.getNamespaceUri("saxon"));
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testMerge() {
        NamespaceMap map1 = NamespaceMap.emptyMap();
        map1 = map1.put("xml", NamespaceUri.XML);
        map1 = map1.put("xsl", NamespaceUri.XSLT);
        map1 = map1.put("saxon", NamespaceUri.SAXON);

        NamespaceMap map2 = NamespaceMap.emptyMap();
        map2 = map2.put("xsl", NamespaceUri.XSLT);
        map2 = map2.put("saxon", NamespaceUri.IXSL);
        map2 = map2.put("array", NamespaceUri.ARRAY_FUNCTIONS);

        NamespaceMap map3 = map1.putAll(map2);
        assertEquals(3, map3.size());
        assertEquals(NamespaceUri.XML, map3.getNamespaceUri("xml"));
        assertEquals(NamespaceUri.XSLT, map3.getNamespaceUri("xsl"));
        assertEquals(NamespaceUri.IXSL, map3.getNamespaceUri("saxon"));
        assertEquals(NamespaceUri.ARRAY_FUNCTIONS, map3.getNamespaceUri("array"));
        assertNull(map3.getNamespaceUri(""));
    }

    @Test
    public void testNormToveyWalsh() {
        try {
            Processor processor = new Processor(false);
            Configuration config = processor.getUnderlyingConfiguration();
            Controller controller = new Controller(config);

            XdmDestination destination = new XdmDestination();
            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            Receiver receiver = destination.getReceiver(pipe, new SerializationProperties());

            receiver.setPipelineConfiguration(pipe);
            receiver.setSystemId("http://example.com/");

            receiver = new ComplexContentOutputter(new NamespaceReducer(receiver));

            receiver.open();
            receiver.startDocument(ReceiverOption.NONE);

            Location location = Loc.NONE;

            FingerprintedQName attrName = new FingerprintedQName("ex", NamespaceUri.of("http://example.com/"), "foo");
            FingerprintedQName elemName = new FingerprintedQName("", NamespaceUri.NULL, "doc");

            Outputter out = (Outputter) receiver;
            out.startElement(elemName, BuiltInAtomicType.UNTYPED_ATOMIC, location, ReceiverOption.NONE);
            out.attribute(attrName, BuiltInAtomicType.UNTYPED_ATOMIC, "bar", location, ReceiverOption.NONE);
            out.startContent();
            out.endElement();
            out.endDocument();

            XdmNode result = destination.getXdmNode();
            assertEquals("<doc xmlns:ex=\"http://example.com/\" ex:foo=\"bar\"/>", result.toString());
            System.out.println(result);
        } catch (Exception e) {
            fail();
        }
    }
}
