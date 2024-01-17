package net.sf.saxon.test;

import net.sf.saxon.om.Sequence;

import java.util.function.Supplier;

public class Assertions {
    public static void assertXPathDeepEqual(Sequence expected, Sequence actual) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual);
    }

    public static void assertXPathDeepEqual(Sequence expected, Sequence actual, String message) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, message);
    }

    public static void assertXPathDeepEqual(Sequence expected, Sequence actual, Supplier<String> messageSupplier) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, messageSupplier);
    }
}
