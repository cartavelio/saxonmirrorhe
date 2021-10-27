////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import net.sf.saxon.regex.LatinString;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class LatinStringTests {

    @Test
    public void testToString() {
        assertTrue(new LatinString("abc123").toString().equals("abc123"));
    }

    @Test
    public void testEquals() {
        assertTrue(new LatinString("abc123").equals(new LatinString("abc123")));
    }

    @Test
    public void testLength() {
        assertEquals(6, new LatinString("abc123").length());
    }

    @Test
    public void testCharAt() {
        assertEquals(new LatinString("abc123").charAt(3), '1');
    }

    @Test
    public void testIsEnd() {
        assertEquals(false, new LatinString("abc123").isEnd(3));
    }

    @Test
    public void testSubsequence() {
        assertEquals(new LatinString("abc123").subSequence(1, 6).toString(), "bc123");
    }



}

