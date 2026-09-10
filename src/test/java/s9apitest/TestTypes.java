package s9apitest;

import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.IntegerValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on s9api operations on XPath types
 */
public class TestTypes {

    /**
     * Test simple type match
     */

    @Test
    public void testSimpleTypeMatch() {
        try {
            Processor p = new Processor(false);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType xsInteger = itf.getAtomicType(new QName(NamespaceConstant.SCHEMA, "integer"));
            XdmAtomicValue a = new XdmAtomicValue(2);
            assertTrue(xsInteger.matches(a));
            XdmAtomicValue b = new XdmAtomicValue("london");
            assertFalse(xsInteger.matches(b));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test generic type match for an atomic value
     */

    @Test
    public void testAtomicTypeMatch() {
        XdmAtomicValue a = new XdmAtomicValue(2);
        assertTrue(ItemType.ANY_ATOMIC_VALUE.matches(a));
        assertTrue(ItemType.ANY_ITEM.matches(a));
        assertFalse(ItemType.ANY_NODE.matches(a));
    }

    /**
     * Test generic type match for a node
     */

    @Test
    public void testNodeMatch() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo><bar/></foo>")));
            assertFalse(ItemType.ANY_ATOMIC_VALUE.matches(foo));
            assertTrue(ItemType.ANY_ITEM.matches(foo));
            assertTrue(ItemType.ANY_NODE.matches(foo));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test item type subsumption
     */

    @Test
    public void testSubsumption() {
        try {
            Processor p = new Processor(false);

            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType xsInteger = itf.getAtomicType(new QName(NamespaceConstant.SCHEMA, "integer"));
            ItemType xsLong = itf.getAtomicType(new QName(NamespaceConstant.SCHEMA, "long"));
            ItemType anyElement = itf.getNodeKindTest(XdmNodeKind.ELEMENT);
            ItemType fooElement = itf.getElementTest(new QName("foo"), null, false);

            assertTrue(ItemType.ANY_ITEM.subsumes(ItemType.ANY_ITEM));
            assertTrue(ItemType.ANY_ITEM.subsumes(ItemType.ANY_NODE));
            assertTrue(ItemType.ANY_ITEM.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertTrue(ItemType.ANY_ITEM.subsumes(xsInteger));
            assertTrue(ItemType.ANY_ITEM.subsumes(anyElement));
            assertFalse(ItemType.ANY_NODE.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertFalse(ItemType.ANY_NODE.subsumes(ItemType.ANY_ITEM));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.ANY_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.ELEMENT_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.COMMENT_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.ATTRIBUTE_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.TEXT_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.DOCUMENT_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.PROCESSING_INSTRUCTION_NODE));
            assertTrue(ItemType.ANY_NODE.subsumes(ItemType.NAMESPACE_NODE));
            assertFalse(ItemType.ANY_NODE.subsumes(xsInteger));
            assertTrue(ItemType.ANY_NODE.subsumes(anyElement));
            assertFalse(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.ANY_ITEM));
            assertFalse(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.ANY_NODE));
            assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(xsInteger));
            assertFalse(ItemType.ANY_ATOMIC_VALUE.subsumes(anyElement));
            assertFalse(xsInteger.subsumes(ItemType.ANY_ITEM));
            assertFalse(xsInteger.subsumes(ItemType.ANY_NODE));
            assertFalse(xsInteger.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertTrue(xsInteger.subsumes(xsInteger));
            assertTrue(anyElement.subsumes(anyElement));
            assertFalse(anyElement.subsumes(ItemType.ANY_ITEM));
            assertFalse(anyElement.subsumes(ItemType.ANY_NODE));
            assertFalse(anyElement.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertFalse(anyElement.subsumes(xsInteger));
            assertTrue(ItemType.ELEMENT_NODE.subsumes(anyElement));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.ANY_ITEM));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.ANY_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.ANY_ATOMIC_VALUE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(xsInteger));
            assertTrue(ItemType.ELEMENT_NODE.subsumes(anyElement));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.COMMENT_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.ATTRIBUTE_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.TEXT_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.DOCUMENT_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.PROCESSING_INSTRUCTION_NODE));
            assertFalse(ItemType.ELEMENT_NODE.subsumes(ItemType.NAMESPACE_NODE));
            assertTrue(xsInteger.subsumes(xsLong));
            assertFalse(xsLong.subsumes(xsInteger));
            assertTrue(anyElement.subsumes(fooElement));
            assertFalse(fooElement.subsumes(anyElement));
            assertTrue(ItemType.ELEMENT_NODE.subsumes(fooElement));
            assertFalse(fooElement.subsumes(ItemType.ELEMENT_NODE));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testBuiltInSubsumption() {
        assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.INTEGER));
        assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.UNTYPED_ATOMIC));
        assertFalse(ItemType.NUMERIC.subsumes(ItemType.UNTYPED_ATOMIC));
        assertTrue(ItemType.NUMERIC.subsumes(ItemType.LONG));
        Processor proc = new Processor(false);
        assertTrue(ItemType.NUMERIC.subsumes(new ItemTypeFactory(proc).getItemType(new XdmAtomicValue(23))));
    }

    @Test
    public void testPrimitiveConstructors() {
        try {
            XdmAtomicValue v = new XdmAtomicValue("2009-10-08", ItemType.DATE);
            assertTrue(ItemType.DATE.matches(v), "date");
            v = new XdmAtomicValue("2009-10-08T12:00:00", ItemType.DATE_TIME);
            assertTrue(ItemType.DATE_TIME.matches(v), "dateTime");
            v = new XdmAtomicValue("12:00:00", ItemType.TIME);
            assertTrue(ItemType.TIME.matches(v), "time");
            v = new XdmAtomicValue("P12Y5M6DT12M", ItemType.DURATION);
            assertTrue(ItemType.DURATION.matches(v), "duration");
            v = new XdmAtomicValue("2009-10", ItemType.G_YEAR_MONTH);
            assertTrue(ItemType.G_YEAR_MONTH.matches(v), "gYM");
            v = new XdmAtomicValue("2009", ItemType.G_YEAR);
            assertTrue(ItemType.G_YEAR.matches(v), "gy");
            v = new XdmAtomicValue("--10-08", ItemType.G_MONTH_DAY);
            assertTrue(ItemType.G_MONTH_DAY.matches(v), "gmd");
            v = new XdmAtomicValue("--10", ItemType.G_MONTH);
            assertTrue(ItemType.G_MONTH.matches(v), "gm");
            v = new XdmAtomicValue("---08", ItemType.G_DAY);
            assertTrue(ItemType.G_DAY.matches(v), "gd");
            v = new XdmAtomicValue("1C1C00", ItemType.HEX_BINARY);
            assertTrue(ItemType.HEX_BINARY.matches(v), "hex");
            v = new XdmAtomicValue("-31000", ItemType.SHORT);
            assertTrue(ItemType.SHORT.matches(v), "short");
            assertTrue(ItemType.INTEGER.matches(v), "int");
            assertFalse(ItemType.BYTE.matches(v), "byte");
            v = new XdmAtomicValue("banana", ItemType.UNTYPED_ATOMIC);
            assertTrue(ItemType.UNTYPED_ATOMIC.matches(v), "untyped");
        } catch (SaxonApiException err) {
            fail("Valid value rejected");
        }
    }

    @Test
    public void testInvalidConstructor() {
        try {
            XdmAtomicValue v = new XdmAtomicValue("2009-02-29", ItemType.DATE);
            fail("Invalid value accepted");
        } catch (SaxonApiException err) {
            // success
        }
    }

    @Test
    public void testInvalidDoubleConstructor() {
        try {
            Double d = new XdmAtomicValue("clearly not a double").getDoubleValue();
            fail("Invalid value accepted");
        } catch (SaxonApiException err) {
            // success
        }
    }

    @Test
    public void testUnderlyingAtomicType() {
        try {
            ItemTypeFactory itf = new ItemTypeFactory(new Processor(false));
            BuiltInAtomicType bat = (BuiltInAtomicType)
                    itf.getAtomicType(new QName("", NamespaceConstant.SCHEMA, "unsignedShort")).getUnderlyingItemType();
            assertEquals(65535, IntegerValue.getMaxInclusive(bat).longValue(), "a1");
        } catch (SaxonApiException | XPathException e) {
            fail();
        }
    }

    @Test
    public void testUnderlyingNodeType() {
        ItemTypeFactory itf = new ItemTypeFactory(new Processor(false));
        ItemType t = itf.getNodeKindTest(XdmNodeKind.ELEMENT);
        NodeTest nt = ((NodeTest)t.getUnderlyingItemType());
        assertTrue(nt instanceof NodeKindTest, "a1");
    }

    @Test
    public void testUnderlyingSequenceType() {
        net.sf.saxon.value.SequenceType st =
                net.sf.saxon.s9api.SequenceType.makeSequenceType(ItemType.DATE, OccurrenceIndicator.ONE)
                .getUnderlyingSequenceType();
        assertEquals(StaticProperty.EXACTLY_ONE, st.getCardinality(), "a1");
        assertEquals(BuiltInAtomicType.DATE, st.getPrimaryType(), "a2");
        net.sf.saxon.s9api.SequenceType st2 =
                net.sf.saxon.s9api.SequenceType.fromUnderlyingSequenceType(new Processor(false), st);
        assertEquals(OccurrenceIndicator.ONE, st2.getOccurrenceIndicator(), "a3");
    }
}

