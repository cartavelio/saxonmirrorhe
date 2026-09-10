package net.sf.saxon.test.matchers;

import com.saxonica.functions.extfn.SaxonDeepEqual;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatcher;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;

public class SequenceIteratorIsDeepEqualTo extends TypeSafeDiagnosingMatcher<SequenceIterator> {
    private GroundedValue expected;

    SequenceIteratorIsDeepEqualTo(GroundedValue expected) {
        this.expected = expected;
    }

    @Override
    public boolean matchesSafely(SequenceIterator actual, Description mismatchDescription) {
        try {
            boolean ok = check(actual);
            if (!ok) {
                appendExpectedTypeAndValue(mismatchDescription.appendText("was "), SequenceTool.toGroundedValue(actual));
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

    private boolean check(SequenceIterator actual) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        StringCollator collator = CodepointCollator.getInstance();
        AtomicMatcher atomicMatcher = GenericAtomicComparer.getTransitiveAtomicMatcher(collator, 0);
        return SaxonDeepEqual.deepEqual(expected.iterate(), actual, atomicMatcher, ctxt, 0);
    }

    public static Matcher<SequenceIterator> deepEquals(GroundedValue expected) {
        return new SequenceIteratorIsDeepEqualTo(expected);
    }
}
