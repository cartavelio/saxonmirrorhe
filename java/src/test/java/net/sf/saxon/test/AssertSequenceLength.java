package net.sf.saxon.test;

import net.sf.saxon.functions.Count;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.AssertionFailureBuilder;

import java.util.function.Supplier;

import static org.junit.jupiter.api.AssertionFailureBuilder.assertionFailure;

/**
 * {@code AssertSequenceLength} allows easy assertion about sequence length in tests.
 *
 */
public class AssertSequenceLength {
    static void assertSequenceLength(int expected, Sequence actual) {
        assertSequenceLength(expected, actual, (String) null);
    }

    static void assertSequenceLength(int expected, Sequence actual, String message) {
        check(expected, actual, message);
    }

    static void assertSequenceLength(int expected, Sequence actual, Supplier<String> messageSupplier) {
        check(expected, actual, messageSupplier);
    }

    static void check(int expected, Sequence actual, Object messageOrSupplier) {
        try {
            int actualLength = Count.count(actual.iterate());
            if (expected != actualLength) {
                buildFailure(expected, actualLength, messageOrSupplier).buildAndThrow();
            }
        } catch (XPathException ex) {
            buildFailure(expected, actual, messageOrSupplier)
                    .reason("Unexpected exception")
                    .cause(ex)
                    .buildAndThrow();
        }
    }

    private static AssertionFailureBuilder buildFailure(Object expected, Object actual, Object messageOrSupplier) {
        return assertionFailure()
                .message(messageOrSupplier)
                .expected(expected)
                .actual(actual);
    }
}
