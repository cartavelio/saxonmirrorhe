////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import net.sf.saxon.str.BMPString;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class BMPStringTests {

    @Test
    public void testToString() {
        assertTrue(BMPString.of("abc123").toString().equals("abc123"));
    }

    @Test
    public void testEquals() {
        assertTrue(BMPString.of("abc123").equals(BMPString.of("abc123")));
    }

    @Test
    public void testLength() {
        assertEquals(BMPString.of("abc123").length(), 6);
    }

    @Test
    public void testCharAt() {
        assertEquals(BMPString.of("abc123").codePointAt(3), '1');
    }

    @Test
    public void testSubsequence() {
        assertEquals(BMPString.of("abc123").substring(1, 6).toString(), "bc123");
    }

}

