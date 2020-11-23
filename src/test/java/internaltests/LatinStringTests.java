////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.regex.LatinString;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class LatinStringTests extends TestCase {

    public void testToString() {
        assertTrue(new LatinString("abc123").toString().equals("abc123"));
    }

    public void testEquals() {
        assertTrue(new LatinString("abc123").equals(new LatinString("abc123")));
    }

    public void testLength() {
        assertEquals(new LatinString("abc123").length(), 6);
    }

    public void testCharAt() {
        assertEquals(new LatinString("abc123").charAt(3), '1');
    }

    public void testIsEnd() {
        assertEquals(new LatinString("abc123").isEnd(3), false);
    }

    public void testSubsequence() {
        assertEquals(new LatinString("abc123").subSequence(1, 6).toString(), "bc123");
    }



}

