package net.sf.saxon.test.matchers;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.XPathException;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;

public class DeepEquals extends TypeSafeDiagnosingMatcher<GroundedValue> {
    private GroundedValue expected;

    DeepEquals(GroundedValue expected) {
        this.expected = expected;
    }

    @Override
    public boolean matchesSafely(GroundedValue actual, Description mismatchDescription) {
        try {
            boolean ok = check(actual);
            if (!ok) {
                mismatchDescription.appendValue(actual);
            }
            return ok;
        } catch (XPathException ex) {
            mismatchDescription.appendValue(ex);
            return false;
        }
    };

    @Override
    public void describeTo(Description description) {
        description.appendText("fn:deep-equal() to return true()");
    }

    private boolean check(GroundedValue actual) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        StringCollator collator = CodepointCollator.getInstance();
        AtomicComparer comparer = new GenericAtomicComparer(collator, ctxt);
        return DeepEqual.deepEqual(expected.iterate(), actual.iterate(), comparer, ctxt, 0);
    }

    public static Matcher<GroundedValue> deepEquals(GroundedValue expected) {
        return new DeepEquals(expected);
    }
}
