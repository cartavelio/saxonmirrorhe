package com.saxonica.test.matchers;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.Matchers;
import org.hamcrest.TypeSafeDiagnosingMatcher;
import org.hamcrest.collection.IsIterableContainingInAnyOrder;
import org.hamcrest.collection.IsIterableContainingInOrder;

import java.util.ArrayList;
import java.util.List;
import java.util.function.BiFunction;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import static org.hamcrest.core.IsEqual.equalTo;


public class MatchesCsvRowRecord extends TypeSafeDiagnosingMatcher<MapItem> {
    private final List<String> fields;
    private final MapItem columns;
    private final List<String> expectedKeys = Stream.of("field", "fields").collect(Collectors.toList());
    private final List<Matcher<String>> keyMatchers = expectedKeys.stream().map(key -> equalTo(key)).collect(Collectors.toList());

    private final List<Matcher<String>> fieldMatchers;

    MatchesCsvRowRecord(List<String> fields, MapItem columns) {
        this.fields = fields;
        this.columns = columns;
        this.fieldMatchers = fields.stream().map(key -> equalTo(key)).collect(Collectors.toList());
    }

    @Override
    protected boolean matchesSafely(MapItem item, Description mismatch) {
        // right number of entries/right keys
        boolean keysOk = keysOk(item);
        // right fields
        List<String> actualFields = sequenceToStream(item.get(new StringValue("fields"))).map(Item::getStringValue).collect(Collectors.toList());
        boolean fieldsOk = fieldsOk(item);
        // right field function
        boolean fieldFunctionOk = fieldFunctionOk(item);

        boolean ok = keysOk && fieldsOk && fieldFunctionOk;
        if (ok) {
            mismatch.appendText("csv-row-record ok\n");
        } else {
            mismatch.appendText("\n");
            mismatchKeys(item, mismatch);
            mismatchFields(item, mismatch);
            mismatchFieldFunction(item, mismatch);
        }
        return ok;
    }

    private boolean keysOk(MapItem item) {
        return new IsIterableContainingInAnyOrder(keyMatchers).matches(actualKeys(item));
    }

    private List<String> actualKeys(MapItem item) {
        return sequenceIteratorToList(item.keys()).stream().map(Item::getStringValue).collect(Collectors.toList());
    }
    private void mismatchKeys(MapItem item, Description mismatchDescription) {
        boolean ok = new IsIterableContainingInAnyOrder(keyMatchers).matches(actualKeys(item));
        if (ok) {
            mismatchDescription.appendText("  + record keys ok\n");
        } else {
            mismatchDescription.appendText("  - record keys not ok (was ").appendValueList("[", ", ", "]", actualKeys(item)).appendText("): ");
            new IsIterableContainingInAnyOrder(keyMatchers).describeMismatch(actualKeys(item), mismatchDescription);
            mismatchDescription.appendText("\n");
        }
    }

    private List<String> actualFields(MapItem item) {
        return sequenceToStream(item.get(new StringValue("fields"))).map(Item::getStringValue).collect(Collectors.toList());
    }

    private boolean fieldsOk(MapItem item) {
        return new IsIterableContainingInOrder(fieldMatchers).matches(actualFields(item));
    }

    private void mismatchFields(MapItem item, Description mismatchDescription) {
        boolean ok = new IsIterableContainingInOrder(fieldMatchers).matches(actualFields(item));
        if (ok) {
            mismatchDescription.appendText("  + fields ok");
        } else {
            mismatchDescription.appendText("  - fields not ok: ");
            new IsIterableContainingInOrder(fieldMatchers).describeMismatch(item, mismatchDescription);
        }
        mismatchDescription.appendText("\n");
    }

    private boolean fieldFunctionOk(MapItem item) {
        boolean nameLookupOk;
        boolean indexLookupOk;

        if (!checkFieldFunctionExists(item)) return false;
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        try {
            FunctionItem actualFieldFunction = getRowFieldFunctionItem(item);
            nameLookupOk = nameLookupOk(ctxt, actualFieldFunction);
            indexLookupOk = indexLookupOk(ctxt, actualFieldFunction);
            return nameLookupOk && indexLookupOk;
        } catch (XPathException ex) {
            return false;
        }
    }

    private boolean checkFieldFunctionExists(MapItem item) {
        GroundedValue actualFieldValue = item.get(new StringValue("field"));
        if (actualFieldValue.getLength() > 1) return false;
        Item actualFieldFunctionItem = actualFieldValue.head();
        if (actualFieldFunctionItem.getGenre() != Genre.FUNCTION) return false;
        return true;
    }

    public static FunctionItem getRowFieldFunctionItem(MapItem item) throws XPathException {
        return (FunctionItem) item.get(new StringValue("field")).head().head();
    }

    private boolean nameLookupOk(XPathContext ctxt, FunctionItem actualFieldFunction) throws XPathException {
        return nameLookupCheck(ctxt, actualFieldFunction, (expected, actual) -> {
            try {
                return Matchers.comparesEqualTo(expected).matches(actual.head().atomize().head().toString());
            } catch (XPathException ex) {
                return false;
            }
        });
    }

    private void mismatchNameLookupCheck(XPathContext ctxt, FunctionItem actualFieldFunction, Description mismatchDescription) {
        try {
            boolean ok = nameLookupCheck(ctxt, actualFieldFunction, (expected, actual) -> {
                try {
                    String actualString = actual.head().atomize().head().toString();
                    Matcher<String> matcher = Matchers.comparesEqualTo(expected);
                    if (!matcher.matches(actualString)) {
                        matcher.describeMismatch(actualString, mismatchDescription);
                        return false;
                    }
                    return true;
                } catch (XPathException ex) {
                    mismatchDescription.appendText("      exception while looking up field: ").appendValue(ex).appendText("\n");
                    return false;
                }
            });
            if (ok) mismatchDescription.appendText("    + field lookup by name ok\n");
        } catch (XPathException ex) {
            mismatchDescription.appendText("    - field lookup by name failed with exception: ").appendValue(ex).appendText("\n");
        }
    }

    private boolean nameLookupCheck(XPathContext ctxt, FunctionItem actualFieldFunction, BiFunction<String, Sequence, Boolean> checker) throws XPathException {
        boolean ok = true;
        GroundedValue names = columns.get(new StringValue("names"));
        if (names != null) {
            for (KeyValuePair kv : ((MapItem) names).keyValuePairs()) {
                Sequence actual = callFieldFunction(ctxt, actualFieldFunction, kv.key);
                int oneBasedIndex = (int) ((Int64Value) kv.value.head().atomize().head()).longValue();
                int zeroBasedIndex = oneBasedIndex - 1;
                String expected = zeroBasedIndex >= fields.size() ? "" : fields.get(zeroBasedIndex);
                if (!checker.apply(expected, actual)) ok = false;
            }
        }
        return ok;
    }

    public static Sequence callFieldFunction(XPathContext ctxt, FunctionItem fieldFunction, AtomicValue key) throws XPathException{
        return fieldFunction.call(ctxt, SequenceTool.fromItems(key));
    }

    public static Sequence callFieldFunction(FunctionItem fieldFunction, AtomicValue key) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return callFieldFunction(ctxt, fieldFunction, key);
    }

    public static Sequence callFieldFunction(FunctionItem fieldFunction, String key) throws XPathException {
        return callFieldFunction(fieldFunction, new StringValue(key));
    }

    public static Sequence callFieldFunction(FunctionItem fieldFunction, int idx) throws XPathException {
        return callFieldFunction(fieldFunction, new Int64Value(idx));
    }

    private boolean indexLookupOk(XPathContext ctxt, FunctionItem actualFieldFunction) throws XPathException {
        return indexLookupCheck(ctxt, actualFieldFunction, (expected, actual) -> {
            try {
                return Matchers.comparesEqualTo(expected).matches(actual.head().atomize().head().toString());
            } catch (XPathException ex) {
                return false;
            }
        });
    }

    private boolean indexLookupCheck(XPathContext ctxt, FunctionItem actualFieldFunction, BiFunction<String, Sequence, Boolean> checker) throws XPathException {
        boolean ok = true;
        for (int i = 0; i < fields.size(); i++) {
            Int64Value oneIndexedKey = new Int64Value(i + 1);
            Sequence actual = callFieldFunction(ctxt, actualFieldFunction, oneIndexedKey);
            String expected = fields.get(i);
            if (!checker.apply(expected, actual)) {
                ok = false;
            };
        }
        return ok;
    }

    private void mismatchIndexLookupCheck(XPathContext ctxt, FunctionItem actualFieldFunction, Description mismatchDescription) {
        try {
            boolean ok = indexLookupCheck(ctxt, actualFieldFunction, (expected, actual) -> {
                try {
                    String actualString = actual.head().atomize().head().toString();
                    Matcher<String> matcher = Matchers.comparesEqualTo(expected);
                    if (!matcher.matches(actualString)) {
                        matcher.describeMismatch(actualString, mismatchDescription);
                        return false;
                    }
                    return true;
                } catch (XPathException ex) {
                    mismatchDescription.appendText("      exception while looking up field: ").appendValue(ex).appendText("\n");
                    return false;
                }
            });
            if (ok) mismatchDescription.appendText("    + field lookup by index ok\n");
        } catch (XPathException ex) {
            mismatchDescription.appendText("    - field lookup by index failed with exception: ").appendValue(ex).appendText("\n");
        }
    }

    private void mismatchFieldFunction(MapItem item, Description mismatchDescription) {
        if (fieldFunctionOk(item)) {
            mismatchDescription.appendText("  + field function ok\n");
        } else {
            mismatchDescription.appendText("  - field function not ok:\n");
            if (mismatchCheckFieldFunctionExists(item, mismatchDescription)) {
                XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
                try {
                    if (mismatchGetFieldFunctionItem(item, mismatchDescription)) {
                        FunctionItem actualFieldFunction = getRowFieldFunctionItem(item);
                        mismatchNameLookupCheck(ctxt, actualFieldFunction, mismatchDescription);
                        mismatchIndexLookupCheck(ctxt, actualFieldFunction, mismatchDescription);
                    }
                } catch (XPathException ex) {
                    mismatchDescription.appendText("    - field function checking failed with exception: ").appendValue(ex).appendText("\n");
                }
            }
        }
    }

    private boolean mismatchCheckFieldFunctionExists(MapItem item, Description mismatchDescription) {
        GroundedValue actualFieldValue = item.get(new StringValue("field"));
        if (actualFieldValue.getLength() > 1) {
            mismatchDescription.appendText("    - field entry was not a single item, but an ").appendValue(actualFieldValue.getLength()).appendText(" item sequence");
            return false;
        }
        Item actualFieldFunctionItem = actualFieldValue.head();
        if (actualFieldFunctionItem.getGenre() != Genre.FUNCTION) {
            mismatchDescription.appendText("    - field entry was not a FunctionItem");
            return false;
        }
        return true;
    }

    private boolean mismatchGetFieldFunctionItem(MapItem item, Description mismatchDescription) {
        try {
            FunctionItem func = getRowFieldFunctionItem(item);
        } catch (XPathException ex) {
            mismatchDescription.appendText("    - could not get field FunctionItem: ").appendValue(ex);
            return false;
        }
        return true;
    }

    public static List<Item> sequenceIteratorToList(SequenceIterator iter) {
        List<Item> list = new ArrayList<>(!SequenceTool.supportsGetLength(iter)
                ? 20
                : ((LastPositionFinder) iter).getLength());
        for (Item item; (item = iter.next()) != null; ) {
            list.add(item);
        }
        return list;
    }

    public static List<Item> sequenceToList(GroundedValue seq) {
        SequenceIterator iter = seq.iterate();
        return sequenceIteratorToList(iter);
    }

    public static Stream<Item> sequenceToStream(GroundedValue seq) {
        return sequenceToList(seq).stream();
    }

    @Override
    public void describeTo(Description description) {
        description.appendText("a csv-row-record with fields ").appendValueList("", ", ", "", fields);
    }

    public static Matcher<MapItem> matchesCsvRowRecord(List<String> fields, MapItem columns) {
        return new MatchesCsvRowRecord(fields, columns);
    }
}
