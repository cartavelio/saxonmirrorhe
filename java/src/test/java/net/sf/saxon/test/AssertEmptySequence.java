package net.sf.saxon.test;

import net.sf.saxon.functions.Count;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import org.junit.jupiter.api.AssertionFailureBuilder;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.AssertionFailureBuilder.assertionFailure;

import java.util.function.Supplier;

/**
 * {@code AssertSequenceLength} allows easy assertion that a value is the Empty Sequence.
 *
 */
public class AssertEmptySequence {
    private static final Sequence emptySequence = EmptySequence.getInstance();
    static void assertEmptySequence(Sequence actual) {
        assertEmptySequence(actual, (String) null);
    }

    static void assertEmptySequence(Sequence actual, String message) {
        check(actual, message);
    }

    static void assertEmptySequence(Sequence actual, Supplier<String> messageSupplier) {
        check(actual, messageSupplier);
    }

    static void check(Sequence actual, Object messageOrSupplier) {
        if (actual.equals(emptySequence)) return;
        try {
            int actualLength = Count.count(actual.iterate());
            if (actualLength == 0) return;
            buildFailure(emptySequence, actual, messageOrSupplier).buildAndThrow();
        } catch (XPathException ex) {
            buildFailure(emptySequence, actual, messageOrSupplier)
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
