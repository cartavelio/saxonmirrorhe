package net.sf.saxon.test;

import com.saxonica.functions.extfn.SaxonDeepEqual;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatcher;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.AssertionFailureBuilder;

import java.util.function.Supplier;

import static org.junit.jupiter.api.AssertionFailureBuilder.assertionFailure;

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
        performAssertion((GroundedValue) expected, (GroundedValue) actual, message);
    }

    static void assertXPathDeepEqual(Sequence expected, Sequence actual, Supplier<String> messageSupplier) {
        performAssertion((GroundedValue) expected, (GroundedValue) actual, messageSupplier);
    }

    static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual) {
        assertXPathDeepEqual(expected, actual, (String) null);
    }

    static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual, String message) {
        performAssertion(expected, actual, message);
    }

    static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual, Supplier<String> messageSupplier) {
        performAssertion(expected, actual, messageSupplier);
    }

    static void performAssertion(GroundedValue expected, GroundedValue actual, Object messageOrSupplier) {
        try {
            if (!deepEqual(expected, actual)) {
                buildFailure(XdmValue.wrap(expected).toString(), XdmValue.wrap(actual).toString(), messageOrSupplier).buildAndThrow();
            }
        } catch (XPathException ex) {
            buildFailure(expected.toString(), actual.toString(), messageOrSupplier) //
                    .reason("Unexpected exception") //
                    .cause(ex) //
                    .buildAndThrow();
        }
    }

    static boolean deepEqual(GroundedValue expected, GroundedValue actual) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        StringCollator collator = CodepointCollator.getInstance();
        AtomicMatcher atomicMatcher = GenericAtomicComparer.getTransitiveAtomicMatcher(collator, 0);
        return SaxonDeepEqual.deepEqual(expected.iterate(), actual.iterate(), atomicMatcher, ctxt, 0);
    }

    private static AssertionFailureBuilder buildFailure(String expected, String actual, Object messageOrSupplier) {
        return assertionFailure() //
                .message(messageOrSupplier) //
                .expected(expected) //
                .actual(actual); //
    }
}