package net.sf.saxon.test.matchers;

import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;
import org.junit.jupiter.api.function.Executable;

public class MatchesThrownXPathException extends TypeSafeDiagnosingMatcher<Executable> {
    private final String code;

    public MatchesThrownXPathException(String code) {
        this.code = code;
    }

    @Override
    protected boolean matchesSafely(Executable actual, Description mismatchDescription) {
        try {
            actual.execute();
        } catch (XPathException ex) {
            boolean ok = ex.hasErrorCode(code);
            if (!ok) {
                mismatchDescription.appendText("was an XPathException with error code ")
                        .appendValue(ex.getErrorCodeQName());
            }
            return ok;
        } catch (UncheckedXPathException uex) {
            XPathException ex = uex.getXPathException();
            boolean ok = ex.hasErrorCode(code);
            mismatchDescription.appendText("was an UncheckedXPathException");
            if (ok) {
                mismatchDescription.appendText(", but with the correct ");
            } else {
                mismatchDescription.appendText(" with ");
            }
            mismatchDescription.appendText("error code ").appendValue(ex.getErrorCodeQName());
            return false;

        } catch (Throwable t) {
            mismatchDescription.appendText("was not an XPathException, was ").appendValue(t);
            return false;
        }
        mismatchDescription.appendText("did not throw anything");
        return false;
    }

    @Override
    public void describeTo(Description description) {
        description.appendText("throw XPathException with code ").appendValue(code);
    }

    public static Matcher<Executable> throwsXPathException(String code) {
        return new MatchesThrownXPathException(code);
    }
}
