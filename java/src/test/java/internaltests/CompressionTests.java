package internaltests;

import net.sf.saxon.str.*;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Tests for whitespace compression. Introduced in response to bug #5838, which we weren't able to
 * reproduce with an actual parser and source document.
 */

public class CompressionTests {

    private void compressWhite(String supplied) {
        char[] buffer = new char[supplied.length() * 3];
        for (int i = 0; i < 3; i++) {
            int start = i * supplied.length();
            supplied.getChars(0, supplied.length(), buffer, start);
            UnicodeString result = StringTool.compress(buffer, start, supplied.length(), true);
            assertTrue(result instanceof CompressedWhitespace);
            assertEquals(result, BMPString.of(supplied));
        }
    }

    @Test
    public void test1() {
        compressWhite(" ");
        compressWhite("\t");
        compressWhite("\n");
        compressWhite("\r");
    }

    @Test
    public void test2() {
        String s32 = "                                ";
        compressWhite(s32);
        compressWhite(s32 + s32);
        compressWhite(s32 + s32 + s32);
        compressWhite(s32 + s32 + s32 + s32);
        compressWhite(s32 + s32 + s32 + s32 + s32);
    }

    @Test
    public void test3() {
        String s32 = "\n                                ";
        compressWhite(s32);
        compressWhite(s32 + s32);
        compressWhite(s32 + s32 + s32);
        compressWhite(s32 + s32 + s32 + s32);
        //compressWhite(s32 + s32 + s32 + s32 + s32);
    }

    @Test
    public void incompressible() {
        // This string can't be compressed because we only allow 8 run-lengths and this needs 10.
        String s32 = "\n                                ";
        String supplied = s32 + s32 + s32 + s32 + s32;
        char[] buffer = new char[supplied.length() * 3];
        for (int i = 0; i < 3; i++) {
            int start = i * supplied.length();
            supplied.getChars(0, supplied.length(), buffer, start);
            UnicodeString result = StringTool.compress(buffer, start, supplied.length(), true);
            assertTrue(result instanceof Twine8);
            assertEquals(result, BMPString.of(supplied));
        }
    }

}

