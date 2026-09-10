package s9apitest;

import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.IntegerValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
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

    @Test
    public void testParse() {
        try {
            Processor p  = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType it = itf.parseItemType("xs:string");
            assertEquals(it, ItemType.STRING);
            it = itf.parseItemType("map(xs:string, xs:integer)");
            XdmMap map = new XdmMap();
            assertTrue(map.matches(it));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testParseRecordType() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType it1 = itf.parseItemType("record(x as xs:double, y as xs:double)");
            ItemType it2 = itf.parseItemType("record(x as xs:double, y as xs:double, *)");
            ItemType it3 = itf.parseItemType("record(x as xs:double, y as xs:double, z as xs:string)");
            XdmMap map = new XdmMap();
            map = map.put(new XdmAtomicValue("x"), new XdmAtomicValue(1.23));
            map = map.put(new XdmAtomicValue("y"), new XdmAtomicValue(3.45));
            assertTrue(it1.matches(map));
            assertTrue(map.matches(it1));
            assertTrue(it2.matches(map));
            assertTrue(map.matches(it2));
            assertFalse(it3.matches(map));
            assertFalse(map.matches(it3));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testParseUnionType() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType it1 = itf.parseItemType("(xs:double | xs:string)");
            assertTrue(it1.matches(new XdmAtomicValue(1.23)));
            assertTrue(it1.matches(new XdmAtomicValue("banana")));
            assertFalse(it1.matches(new XdmAtomicValue(true)));
            assertFalse(it1.matches(new XdmMap()));
            assertFalse(it1.matches(new XdmNode(new Orphan(p.getUnderlyingConfiguration()))));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testParseError() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType it1 = itf.parseItemType("onion(xs:double, xs:string)");
            fail("Error not detected");
        } catch (SaxonApiException e) {
            // OK
        }
    }


    @Test
    public void testSequenceType() {
        try {
            Processor p = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            SequenceType st = itf.parseItemType("xs:string").oneOrMore();
            XdmValue v1 = new XdmAtomicValue("banana");
            assertTrue(v1.matches(st));
            assertTrue(st.matches(v1));
            assertFalse(XdmEmptySequence.getInstance().matches(st));
            assertFalse(st.matches(XdmEmptySequence.getInstance()));
            st = itf.parseItemType("map(xs:string, xs:integer)").one();
            XdmMap map = new XdmMap();
            assertTrue(map.matches(st));
            assertTrue(st.matches(map));
            assertFalse(map.matches(st.getItemType().with(OccurrenceIndicator.ZERO)));
            assertFalse(st.getItemType().with(OccurrenceIndicator.ZERO).matches(map));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testFunctionType() {
        try {
            Processor p = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType constructedItemType = itf.getFunctionType(
                    ItemType.STRING.one(),
                    ItemType.ANY_ATOMIC_VALUE.zeroOrMore(),
                    ItemType.STRING.one());
            ItemType parsedItemType = itf.parseItemType("function(xs:anyAtomicType*, xs:string) as xs:string");
            assertEquals(constructedItemType, parsedItemType);
            XdmFunctionItem stringJoin = XdmFunctionItem.getSystemFunction(
                    p,new QName(NamespaceConstant.FN, "string-join"), 2);
            assertNotNull(stringJoin);
            assertEquals(constructedItemType, itf.getItemType(stringJoin));
            assertTrue(constructedItemType.matches(stringJoin));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testFunctionSubsumption() {
        try {
            Processor p = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType one = itf.parseItemType("function(xs:anyAtomicType*, xs:string) as xs:string+");
            ItemType two = itf.parseItemType("function(xs:string, xs:string) as xs:string+");
            ItemType three = itf.parseItemType("function(xs:anyAtomicType*, xs:string) as xs:string*");
            assertTrue(two.subsumes(one));
            assertTrue(three.subsumes(one));
            assertFalse(one.subsumes(two));
            assertFalse(one.subsumes(three));
            assertFalse(two.subsumes(three));
            assertFalse(three.subsumes(two));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testItemTypeToString() {
        try {
            Processor p = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            assertEquals("xs:boolean", ItemType.BOOLEAN.toString());
            assertEquals("xs:QName", ItemType.QNAME.toString());
            assertEquals("item()", ItemType.ANY_ITEM.toString());
            assertEquals("xs:anyAtomicType", ItemType.ANY_ATOMIC_VALUE.toString());
            assertEquals("node()", ItemType.ANY_NODE.toString());
            assertEquals("element()", ItemType.ELEMENT_NODE.toString());
            assertEquals("comment()", ItemType.COMMENT_NODE.toString());
            assertEquals("map(*)", ItemType.ANY_MAP.toString());
            assertEquals("array(*)", ItemType.ANY_ARRAY.toString());
            assertEquals("function(*)", ItemType.ANY_FUNCTION.toString());
            XdmFunctionItem nameFn = XdmFunctionItem.getSystemFunction(p, new QName(NamespaceConstant.FN, "name"), 1);
            assertNotNull(nameFn);
            ItemType nameFnType = itf.getItemType(nameFn);
            assertEquals("(function(node()?) as xs:string)", nameFnType.toString());
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testSequenceTypeToString() {
        try {
            Processor p = new Processor(true);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            assertEquals("xs:boolean", ItemType.BOOLEAN.toString());
            assertEquals("xs:QName", ItemType.QNAME.toString());
            assertEquals("item()", ItemType.ANY_ITEM.toString());
            assertEquals("xs:anyAtomicType", ItemType.ANY_ATOMIC_VALUE.toString());
            assertEquals("node()", ItemType.ANY_NODE.toString());
            assertEquals("element()", ItemType.ELEMENT_NODE.toString());
            assertEquals("comment()", ItemType.COMMENT_NODE.toString());
            assertEquals("map(*)", ItemType.ANY_MAP.toString());
            assertEquals("array(*)", ItemType.ANY_ARRAY.toString());
            assertEquals("function(*)", ItemType.ANY_FUNCTION.toString());
            XdmFunctionItem nameFn = XdmFunctionItem.getSystemFunction(p, new QName(NamespaceConstant.FN, "name"), 1);
            assertNotNull(nameFn);
            ItemType nameFnType = itf.getItemType(nameFn);
            assertEquals("(function(node()?) as xs:string)", nameFnType.toString());
        } catch (Exception e) {
            fail();
        }
    }
}

