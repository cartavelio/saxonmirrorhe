package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.QName;


/**
 * JUnit tests on s9api operations on QName class
 */
public class TestQName extends TestCase {

    /**
     * Test 3-arg constructor
     */

    public void testQName3() {
        QName q = new QName("prefix", "http://some/uri", "local");
        assertEquals(q.getPrefix(), "prefix");
        assertEquals(q.getNamespaceURI(), "http://some/uri");
        assertEquals(q.getLocalName(), "local");
    }

    /**
     * Test 3-arg constructor
     */

    public void testQName3b() {
        QName q = new QName("", "", "local");
        assertEquals(q.getPrefix(), "");
        assertEquals(q.getNamespaceURI(), "");
        assertEquals(q.getLocalName(), "local");
    }

    /**
     * Test 2-arg constructor
     */

    public void testQName2() {
        QName q = new QName("http://some/uri", "prefix:local");
        assertEquals(q.getPrefix(), "prefix");
        assertEquals(q.getNamespaceURI(), "http://some/uri");
        assertEquals(q.getLocalName(), "local");
    }

    /**
     * Test 2-arg constructor
     */

    public void testQName2b() {
        QName q = new QName("", "local");
        assertEquals(q.getPrefix(), "");
        assertEquals(q.getNamespaceURI(), "");
        assertEquals(q.getLocalName(), "local");
    }

   /**
     * Test 1-arg constructor
     */

    public void testQName1() {
        QName q = new QName("local");
        assertEquals(q.getPrefix(), "");
        assertEquals(q.getNamespaceURI(), "");
        assertEquals(q.getLocalName(), "local");
    }

    /**
     * Test factory method from Clark name
     */

    public void testQNameFromClarkName() {
        QName q = QName.fromClarkName("{http://some/uri}local");
        assertEquals(q.getPrefix(), "");
        assertEquals(q.getNamespaceURI(), "http://some/uri");
        assertEquals(q.getLocalName(), "local");
    }

    /**
     * Test factory method from Clark name
     */

    public void testQNameFromClarkNameB() {
        QName q = QName.fromClarkName("{}local");
        assertEquals(q.getPrefix(), "");
        assertEquals(q.getNamespaceURI(), "");
        assertEquals(q.getLocalName(), "local");
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
