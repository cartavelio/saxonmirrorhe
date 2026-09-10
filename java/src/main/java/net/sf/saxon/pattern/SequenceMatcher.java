package net.sf.saxon.pattern;

import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

public class SequenceMatcher {

    private final Pattern itemPattern;
    private final int cardinality;

    public final static SequenceMatcher UNIVERSAL_MATCHER =
            new SequenceMatcher(new UniversalPattern(), StaticProperty.ALLOWS_ZERO_OR_MORE);

    public static SequenceMatcher fromSequenceType(SequenceType sequenceType) {
        return new SequenceMatcher(new ItemTypePattern(sequenceType.getPrimaryType()), sequenceType.getCardinality());
    }

    public SequenceMatcher(Pattern itemPattern, int cardinality) {
        this.itemPattern = itemPattern;
        this.cardinality = cardinality;
    }

    public Pattern getItemPattern() {
        return itemPattern;
    }

    public boolean matches(GroundedValue value, XPathContext context) throws XPathException {
        int length = value.getLength();
        if (!Cardinality.allows(cardinality, length)) {
            return false;
        }
        for (Item item : value.asIterable()) {
            if (!itemPattern.matches(item, context)) {
                return false;
            }
        }
        return true;
    }

    public SequenceMatcher copy(RebindingMap rebindings) {
        return new SequenceMatcher(itemPattern.copy(rebindings), cardinality);
    }

}

