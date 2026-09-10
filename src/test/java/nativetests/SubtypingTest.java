package nativetests;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.ItemType;
import net.sf.saxon.s9api.ItemTypeFactory;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test options on schema validation interfaces
 */
public class SubtypingTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testAtomicTypes() {
        Processor proc = new Processor(false);
        TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();
        assertEquals(Affinity.SAME_TYPE, th.relationship(BuiltInAtomicType.FLOAT, BuiltInAtomicType.FLOAT));
        assertEquals(Affinity.SUBSUMED_BY, th.relationship(BuiltInAtomicType.DATE, BuiltInAtomicType.ANY_ATOMIC));
        assertEquals(Affinity.SUBSUMED_BY, th.relationship(BuiltInAtomicType.INTEGER, BuiltInAtomicType.DECIMAL));
        assertEquals(Affinity.SUBSUMED_BY, th.relationship(BuiltInAtomicType.INT, BuiltInAtomicType.LONG));
        assertEquals(Affinity.DISJOINT, th.relationship(BuiltInAtomicType.DATE_TIME, BuiltInAtomicType.DURATION));
    }

    @Test
    public void testNodeTypes() {
        Processor proc = new Processor(false);
        TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();
        assertEquals(Affinity.SAME_TYPE, th.relationship(NodeKindTest.ELEMENT, NodeKindTest.ELEMENT));
        assertEquals(Affinity.SUBSUMED_BY, th.relationship(NodeKindTest.ELEMENT, AnyNodeTest.getInstance()));
        assertEquals(Affinity.DISJOINT, th.relationship(NodeKindTest.ELEMENT, NodeKindTest.ATTRIBUTE));
    }

    @Test
    public void testElementNameTests() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(proc);
            ItemType namedElement = itf.parseItemType("element(Q{uri}local)");
            ItemType prefix = itf.parseItemType("element(Q{uri}*)");
            ItemType prefix2 = itf.parseItemType("element(Q{uri2}*)");
            ItemType suffix = itf.parseItemType("element(*:local)");
            ItemType suffix2 = itf.parseItemType("element(*:local2)");
            ItemType wild = itf.parseItemType("element(*)");
            ItemType anyElement = itf.parseItemType("element()");
            TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();
            assertEquals(Affinity.SAME_TYPE, rel(th, wild, anyElement));
            assertEquals(Affinity.SUBSUMES, rel(th, prefix, namedElement));
            assertEquals(Affinity.SUBSUMES, rel(th, suffix, namedElement));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, namedElement, wild));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, prefix, wild));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, suffix, wild));
            assertEquals(Affinity.OVERLAPS, rel(th, prefix, suffix));
            assertEquals(Affinity.DISJOINT, rel(th, prefix, prefix2));
            assertEquals(Affinity.DISJOINT, rel(th, suffix, suffix2));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testAttributeNameTests() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(proc);
            ItemType namedElement = itf.parseItemType("attribute(Q{uri}local)");
            ItemType prefix = itf.parseItemType("attribute(Q{uri}*)");
            ItemType prefix2 = itf.parseItemType("attribute(Q{uri2}*)");
            ItemType suffix = itf.parseItemType("attribute(*:local)");
            ItemType suffix2 = itf.parseItemType("attribute(*:local2)");
            ItemType wild = itf.parseItemType("attribute(*)");
            ItemType anyElement = itf.parseItemType("attribute()");
            TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();
            assertEquals(Affinity.SAME_TYPE, rel(th, wild, anyElement));
            assertEquals(Affinity.SUBSUMES, rel(th, prefix, namedElement));
            assertEquals(Affinity.SUBSUMES, rel(th, suffix, namedElement));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, namedElement, wild));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, prefix, wild));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, suffix, wild));
            assertEquals(Affinity.OVERLAPS, rel(th, prefix, suffix));
            assertEquals(Affinity.DISJOINT, rel(th, prefix, prefix2));
            assertEquals(Affinity.DISJOINT, rel(th, suffix, suffix2));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testElementContentTests() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            ItemTypeFactory itf = new ItemTypeFactory(proc);
            ItemType namedElement = itf.parseItemType("element(Q{http://bookworm.ns/}schedule, Q{http://www.w3.org/2001/XMLSchema}untyped)");
            ItemType suffix = itf.parseItemType("element(Q{http://bookworm.ns/}*)");

            TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();
            assertEquals(Affinity.SUBSUMES, rel(th, suffix, namedElement));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testElementUnionNameTests() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XQUERY_VERSION, "4.0");
            TypeHierarchy th = proc.getUnderlyingConfiguration().getTypeHierarchy();

            ItemTypeFactory itf = new ItemTypeFactory(proc);
            ItemType t0 = itf.parseItemType("element(a|b)");
            ItemType t1 = itf.parseItemType("element(b|a)");
            ItemType t2 = itf.parseItemType("element(a)");
            ItemType t3 = itf.parseItemType("(element(a)|element(b))");
            ItemType t4 = itf.parseItemType("(element(b)|element(a))");
            assertEquals(Affinity.SAME_TYPE, rel(th, t0, t1));
            assertEquals(Affinity.SAME_TYPE, rel(th, t0, t3));
            assertEquals(Affinity.SAME_TYPE, rel(th, t3, t4));
            assertEquals(Affinity.SAME_TYPE, rel(th, t0, t4));
            assertEquals(Affinity.SUBSUMES, rel(th, t0, t2));
            assertEquals(Affinity.SUBSUMES, rel(th, t1, t2));
            assertEquals(Affinity.SUBSUMES, rel(th, t3, t2));
            assertEquals(Affinity.SUBSUMES, rel(th, t4, t2));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, t2, t0));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, t2, t1));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, t2, t3));
            assertEquals(Affinity.SUBSUMED_BY, rel(th, t2, t4));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    private static Affinity rel(TypeHierarchy th, ItemType s, ItemType t) {
        return th.relationship(u(s), u(t));
    }

    private static net.sf.saxon.type.ItemType u(ItemType t) {
        return t.getUnderlyingItemType();
    }

}