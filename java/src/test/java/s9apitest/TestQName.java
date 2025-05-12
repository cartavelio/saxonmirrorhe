package s9apitest;

import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.QName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;


/**
 * JUnit tests on s9api operations on QName class
 */
public class TestQName {

    /**
     * Test 3-arg constructor
     */

    @Test
    public void testQName3() {
        QName q = new QName("prefix", "http://some/uri", "local");
        assertEquals("prefix", q.getPrefix());
        assertEquals("http://some/uri", q.getNamespace());
        assertEquals("local", q.getLocalName());
    }

    /**
     * Test 3-arg constructor
     */

    @Test
    public void testQName3b() {
        QName q = new QName("", "", "local");
        assertEquals("", q.getPrefix());
        assertEquals("", q.getNamespace());
        assertEquals("local", q.getLocalName());
    }

    /**
     * Test 2-arg constructor
     */

    @Test
    public void testQName2() {
        QName q = new QName("http://some/uri", "prefix:local");
        assertEquals("prefix", q.getPrefix());
        assertEquals(NamespaceUri.of("http://some/uri"), q.getNamespaceUri());
        assertEquals("local", q.getLocalName());
    }

    @Test
    public void testQName2a() {
        QName q = new QName(NamespaceUri.of("http://some/uri"), "prefix:local");
        assertEquals("prefix", q.getPrefix());
        assertEquals(NamespaceUri.of("http://some/uri"), q.getNamespaceUri());
        assertEquals("local", q.getLocalName());
    }

    /**
     * Test 2-arg constructor
     */

    @Test
    public void testQName2b() {
        QName q = new QName("", "local");
        assertEquals("", q.getPrefix());
        assertEquals(NamespaceUri.NULL, q.getNamespaceUri());
        assertEquals("local", q.getLocalName());
    }

   /**
     * Test 1-arg constructor
     */

    @Test
    public void testQName1() {
        QName q = new QName("local");
        assertEquals("", q.getPrefix());
        assertEquals("", q.getNamespace());
        assertEquals("local", q.getLocalName());
    }

    /**
     * Test factory method from Clark name
     */

    @Test
    public void testQNameFromClarkName() {
        QName q = QName.fromClarkName("{http://some/uri}local");
        assertEquals("", q.getPrefix());
        assertEquals(NamespaceUri.of("http://some/uri"), q.getNamespaceUri());
        assertEquals("local", q.getLocalName());
    }

    /**
     * Test factory method from Clark name
     */

    @Test
    public void testQNameFromClarkNameB() {
        QName q = QName.fromClarkName("{}local");
        assertEquals("", q.getPrefix());
        assertEquals(NamespaceUri.NULL, q.getNamespaceUri());
        assertEquals("local", q.getLocalName());
    }


}
