////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.gizmo.JLine2Talker;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Test aspects of content completion in Gizmo
 */
public class GizmoCompleterTests {
    private final ConfigTest configTest = new ConfigTest();
    
    private static final String[] keywords = new String[]{
            "ancestor::", "ancestor-or-self::", "array", "attribute", "cast as", "castable as", "child::",
            "comment()", "descendant::", "descendant-or-self::", "document-node()", "element()", "else",
            "empty-sequence()", "every", "except", "following::", "following-sibling::", "function",
            "instance of", "intersect", "item()", "namespace::", "namespace-node()", "node()", "parent::",
            "preceding::", "preceding-sibling::", "processing-instruction()", "return", "satisfies",
            "schema-attribute", "schema-element", "self::", "some", "text()", "then", "treat as",
            "union"
    };

    private static final String[] names = new String[]{"BOOKLIST", "BOOKS", "ITEM", "TITLE", "AUTHOR", "PUBLISHER",
            "PRICE", "DIMENSIONS", "CATEGORY", "CATEGORIES", "@CAT", "@GENRE"};

    public static String cwd = null;

    // Filename completion tests are tricky to make independent of the actual filestore layout...
    // Make sure we're in the repo/tests/junit/testdata directory for these tests.
    @BeforeAll
    public static void setUp() {
        cwd = System.getProperty("user.dir");
        ConfigTest ctest = new ConfigTest();
        String root = ctest.getDataDir();
        // If you leave "/" on the end of "user.dir", bad happens
        if (root.endsWith("/") && root.length() > 1) {
            root = root.substring(0, root.length() - 1);
        }
        System.setProperty("user.dir", root);
    }

    @AfterAll
    public static void tearDown() {
        System.setProperty("user.dir", cwd);
    }

    private int go(String input, List<String> candidates) {
        List<String> tokens = new ArrayList<>();
        Collections.addAll(tokens, keywords);
        Collections.addAll(tokens, names);
        JLine2Talker.XPathCompleter completer = new JLine2Talker.XPathCompleter(tokens);
        List<CharSequence> result = new ArrayList<>();
        int x = completer.complete(input, input.length(), result);
        candidates.clear();
        result.forEach(cs -> candidates.add(cs.toString()));
        return x;
    }

    @Test
    public void testXPathCompletion001() {
        List<String> options = new ArrayList<>();
        int pos = go("list /BOOKLIST/BOOKS/IT", options);
        assertEquals(1, options.size());
        assertTrue(options.contains("ITEM"));
        assertEquals(pos, "list /BOOKLIST/BOOKS/".length());
        if (JLine2Talker.debugLog != null) {
            System.err.println(JLine2Talker.debugLog);
        }
    }

    @Test
    public void testXPathCompletion002() {
        List<String> options = new ArrayList<>();
        int pos = go("list /BOOKLIST/CAT", options);
        assertEquals(2, options.size());
        assertTrue(options.contains("CATEGORY"));
        assertTrue(options.contains("CATEGORIES"));
        assertEquals(pos, "list /BOOKLIST/".length());
        if (JLine2Talker.debugLog != null) {
            System.err.println(JLine2Talker.debugLog);
        }
    }

    @Test
    public void testXPathCompletion003() {
        List<String> options = new ArrayList<>();
        int pos = go("list ", options);
        assertTrue(options.size() > 30, "size");
        assertEquals(pos, "list ".length(), "length");
        if (JLine2Talker.debugLog != null) {
            System.err.println(JLine2Talker.debugLog);
        }
    }

    @Test
    public void testXPathCompletion004() {
        List<String> options = new ArrayList<>();
        int pos = go("list /", options);
        assertTrue(options.size() > 30);
        assertEquals(pos, "list /".length());
        if (JLine2Talker.debugLog != null) {
            System.err.println(JLine2Talker.debugLog);
        }
    }

    @Test
    public void testXPathCompletion005() {
        List<String> options = new ArrayList<>();
        int pos = go("list //ITEM/follo", options);
        assertEquals(2, options.size());
        assertTrue(options.contains("following::"));
        assertTrue(options.contains("following-sibling::"));
        assertEquals(pos, "list //ITEM/".length());
        if (JLine2Talker.debugLog != null) {
            System.err.println(JLine2Talker.debugLog);
        }
    }

    @Test
    // Assume the repo/tests/junit/testdata directory is the current directory

    public void testFileCompletion001() {
        List<String> options = new ArrayList<>();
        int pos = go("load trax/xml/cit", options);
        assertEquals(1, options.size(), "a");
        assertTrue(options.contains("cities.xml "), "b");
        assertEquals(pos, "load trax/xml/".length(), "c");
    }

    @Test
    public void testFileCompletion002() {
        List<String> options = new ArrayList<>();
        int pos = go("load trax/xsl/foo", options);
        assertEquals(3, options.size(), "a");
        assertTrue(options.contains("foo.xsl "), "b");
        assertTrue(options.contains("foo2.xsl "), "c");
        assertTrue(options.contains("foo3.xsl "), "d");
        assertEquals(pos, "load trax/xsl/".length(), "e");
    }

    @Test
    public void testFileCompletion003() {
        List<String> options = new ArrayList<>();
        int pos = go("load it", options);
        assertEquals(1, options.size(), "a");
        assertTrue(options.contains("items.xml "), "b");
        assertEquals(pos, "load ".length(), "e");
    }

    @Tag("mikeonly")
    @Test
    public void testFileCompletion004() {
        String home = System.getProperty("user.home");
        System.setProperty("user.home", configTest.getDataDir());
        List<String> options = new ArrayList<>();
        int pos = go("load ~/bible/com", options);    // Assumes MHK filestore layout
        assertEquals(1, options.size(), "a");
        assertTrue(options.contains("common/"), "b");
        assertEquals(pos, "load ~/bible/".length(), "e");
        System.setProperty("user.home", home);
    }

    @Test
    public void testFileCompletion005() {
        List<String> options = new ArrayList<>();
        int pos = go("load ./it", options);
        assertEquals(1, options.size(), "a");
        assertTrue(options.contains("items.xml "), "b");
        assertEquals(pos, "load ./".length(), "e");
    }

    @Test
    public void testFileCompletion006() {
        List<String> options = new ArrayList<>();
        int pos = go("load trax/xs", options);
        assertEquals(1, options.size(), "xsl/");

        String expected = "xsl" + File.separator;

        assertTrue(options.contains(expected), "b");
        assertEquals(pos, "load trax/".length(), "e");
    }


}

