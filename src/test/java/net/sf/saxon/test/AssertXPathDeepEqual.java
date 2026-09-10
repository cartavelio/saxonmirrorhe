package net.sf.saxon.test;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.AssertionFailureBuilder;

import static org.junit.jupiter.api.AssertionFailureBuilder.assertionFailure;

import java.util.function.Supplier;

/**
 * {@code AssertXPathDeepEqual} allows easier equality assertions on XDM values in tests.
 *
 */
class AssertXPathDeepEqual {
    private AssertXPathDeepEqual() {
        /* no-op */
    }

    static void assertXPathDeepEqual(Sequence expected, Sequence actual) {
        assertXPathDeepEqual(expected, actual, (String) null);
    }

    static void assertXPathDeepEqual(Sequence expected, Sequence actual, String message) {
        deepEqual(expected, actual, message);
    }

    static void assertXPathDeepEqual(Sequence expected, Sequence actual, Supplier<String> messageSupplier) {
        deepEqual(expected, actual, messageSupplier);
    }

    static void deepEqual(Sequence expected, Sequence actual, Object messageOrSupplier) {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        StringCollator collator = CodepointCollator.getInstance();
        AtomicComparer comparer = new GenericAtomicComparer(collator, ctxt);
        try {
            if (!DeepEqual.deepEqual(expected.iterate(), actual.iterate(), comparer, ctxt, 0)) {
                buildFailure(expected, actual, messageOrSupplier).buildAndThrow();
            }
        } catch (XPathException ex) {
            buildFailure(expected, actual, messageOrSupplier) //
                    .reason("Unexpected exception") //
                    .cause(ex) //
                    .buildAndThrow();
        }
    }

    private static AssertionFailureBuilder buildFailure(Object expected, Object actual, Object messageOrSupplier) {
        return assertionFailure() //
                .message(messageOrSupplier) //
                .expected(expected) //
                .actual(actual); //
    }
}