package net.sf.saxon.test.matchers;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatcher;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.SaxonDeepEqual;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.XmlProcessingError;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;

public class GroundedValueIsDeepEqualTo extends TypeSafeDiagnosingMatcher<GroundedValue> {
    private GroundedValue expected;
    private XPathContext context;

    private static class LocalErrorReporter implements ErrorReporter {
        private final StringBuilder errors;

        LocalErrorReporter() {
            errors = new StringBuilder();
        }

        @Override
        public void report(XmlProcessingError error) {
            errors.append(error.getMessage());
        }

        public String toString() {
            return errors.toString();
        }
    }


    GroundedValueIsDeepEqualTo(GroundedValue expected, XPathContext context) {
        this.expected = expected;
        this.context = context;
    }

    @Override
    public boolean matchesSafely(GroundedValue actual, Description mismatchDescription) {
        try {
            LocalErrorReporter err = new LocalErrorReporter();
            boolean ok = check(err, actual, context);
            if (!ok) {
                appendExpectedTypeAndValue(mismatchDescription.appendText("was "), actual)
                        .appendText("\n")
                        .appendValue(err);
            }
            return ok;
        } catch (XPathException ex) {
            mismatchDescription.appendValue(ex);
            return false;
        }
    };

    @Override
    public void describeTo(Description description) {
        appendExpectedTypeAndValue(description, expected);
    }

    private Description appendExpectedTypeAndValue(Description description, GroundedValue value) {
        return description.appendText(getExpectedType(value)).appendText(" ").appendValue(value);
    }

    private String getExpectedType(GroundedValue value) {
        if (value instanceof NodeInfo) {
            switch (((NodeInfo) value).getNodeKind()) {
                case Type.DOCUMENT:
                    return "XML Document node";
                case Type.ELEMENT:
                    return "XML Element node";
            }
        }
        return "XDM value";
    }

    private boolean check(ErrorReporter err, GroundedValue actual, XPathContext ctxt) throws XPathException {
        StringCollator collator = CodepointCollator.getInstance();
        AtomicMatcher atomicMatcher = GenericAtomicComparer.getTransitiveAtomicMatcher(collator, 0);
        return SaxonDeepEqual.deepEqual(expected.iterate(), actual.iterate(), atomicMatcher, ctxt, SaxonDeepEqual.WARNING_IF_FALSE);
    }

    public static Matcher<GroundedValue> deepEquals(GroundedValue expected, XPathContext context) {
        return new GroundedValueIsDeepEqualTo(expected, context);
    }
}
