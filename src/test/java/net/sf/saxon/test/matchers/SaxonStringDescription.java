package net.sf.saxon.test.matchers;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import org.hamcrest.Description;
import org.hamcrest.StringDescription;

public class SaxonStringDescription extends StringDescription {
    public Description appendValue(Object value) {
        if (value instanceof GroundedValue) {
            return super.appendValue(((GroundedValue) value).toShortString());
        } else if (value instanceof Sequence) {
            try {
                GroundedValue materialized = ((Sequence) value).materialize();
                return super.appendValue(materialized.toShortString());
            } catch (XPathException ex) {
                throw new AssertionError(ex);
            }
        } else {
            return super.appendValue(value);
        }
    }
}
