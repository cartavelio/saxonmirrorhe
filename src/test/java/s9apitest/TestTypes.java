package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * JUnit tests on s9api operations on XPath types
 */
public class TestTypes extends TestCase {

    /**
     * Test simple type match
     */

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

    public void testAtomicTypeMatch() {
        XdmAtomicValue a = new XdmAtomicValue(2);
        assertTrue(ItemType.ANY_ATOMIC_VALUE.matches(a));
        assertTrue(ItemType.ANY_ITEM.matches(a));
        assertFalse(ItemType.ANY_NODE.matches(a));
    }

    /**
     * Test generic type match for a node
     */

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

    public void testBuiltInSubsumption() {
        assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.INTEGER));
        assertTrue(ItemType.ANY_ATOMIC_VALUE.subsumes(ItemType.UNTYPED_ATOMIC));
        assertFalse(ItemType.NUMERIC.subsumes(ItemType.UNTYPED_ATOMIC));
        assertTrue(ItemType.NUMERIC.subsumes(ItemType.LONG));
        Processor proc = new Processor(false);
        assertTrue(ItemType.NUMERIC.subsumes(new ItemTypeFactory(proc).getItemType(new XdmAtomicValue(23))));
    }

    public void testPrimitiveConstructors() {
        try {
            XdmAtomicValue v = new XdmAtomicValue("2009-10-08", ItemType.DATE);
            assertTrue("date", ItemType.DATE.matches(v));
            v = new XdmAtomicValue("2009-10-08T12:00:00", ItemType.DATE_TIME);
            assertTrue("dateTime", ItemType.DATE_TIME.matches(v));
            v = new XdmAtomicValue("12:00:00", ItemType.TIME);
            assertTrue("time", ItemType.TIME.matches(v));
            v = new XdmAtomicValue("P12Y5M6DT12M", ItemType.DURATION);
            assertTrue("duration", ItemType.DURATION.matches(v));
            v = new XdmAtomicValue("2009-10", ItemType.G_YEAR_MONTH);
            assertTrue("gYM", ItemType.G_YEAR_MONTH.matches(v));
            v = new XdmAtomicValue("2009", ItemType.G_YEAR);
            assertTrue("gy", ItemType.G_YEAR.matches(v));
            v = new XdmAtomicValue("--10-08", ItemType.G_MONTH_DAY);
            assertTrue("gmd", ItemType.G_MONTH_DAY.matches(v));
            v = new XdmAtomicValue("--10", ItemType.G_MONTH);
            assertTrue("gm", ItemType.G_MONTH.matches(v));
            v = new XdmAtomicValue("---08", ItemType.G_DAY);
            assertTrue("gd", ItemType.G_DAY.matches(v));
            v = new XdmAtomicValue("1C1C00", ItemType.HEX_BINARY);
            assertTrue("hex", ItemType.HEX_BINARY.matches(v));
            v = new XdmAtomicValue("-31000", ItemType.SHORT);
            assertTrue("short", ItemType.SHORT.matches(v));
            assertTrue("int", ItemType.INTEGER.matches(v));
            assertFalse("byte", ItemType.BYTE.matches(v));
            v = new XdmAtomicValue("banana", ItemType.UNTYPED_ATOMIC);
            assertTrue("untyped", ItemType.UNTYPED_ATOMIC.matches(v));
        } catch (SaxonApiException err) {
            fail("Valid value rejected");
        }
    }

    public void testInvalidConstructor() {
        try {
            XdmAtomicValue v = new XdmAtomicValue("2009-02-29", ItemType.DATE);
            fail("Invalid value accepted");
        } catch (SaxonApiException err) {
            // success
        }
    }

    public void testInvalidDoubleConstructor() {
        try {
            Double d = new XdmAtomicValue("clearly not a double").getDoubleValue();
            fail("Invalid value accepted");
        } catch (SaxonApiException err) {
            // success
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



