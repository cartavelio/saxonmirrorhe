package com.saxonica.test.matchers;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.Matchers;
import org.hamcrest.TypeSafeDiagnosingMatcher;

import java.util.ArrayList;
import java.util.List;
import java.util.function.BiFunction;
import java.util.stream.Stream;

import static com.saxonica.test.matchers.MatchesCsvRecordGetFunction.callGetFunction;

public class MatchesCsvGetFunctionWithRow extends TypeSafeDiagnosingMatcher<ArrayItem> {
//    private List<Matcher<MapItem>> buildRowMatchers() {
//        return rows.stream().map(row -> matchesCsvRowRecord(row, columns)).collect(Collectors.toList());
//    };

    private ArrayItem expectedRow;
    private int rowIdx;
    private MapItem expectedColumns;
    private FunctionItem actualGetFunction;

    MatchesCsvGetFunctionWithRow(ArrayItem expectedRow, int rowIdx, MapItem expectedColumns, FunctionItem actualGetFunction) {
        this.expectedRow = expectedRow;
        this.rowIdx = rowIdx;
        this.expectedColumns = expectedColumns;
        this.actualGetFunction = actualGetFunction;
    }

    @Override
    protected boolean matchesSafely(ArrayItem item, Description mismatch) {
        boolean ok =  getFunctionAppliedToRowOk(item);
        if (ok) {
            mismatch.appendText("get function ok\n");
        } else {
            mismatch.appendText("\n");
            mismatchGetFunction(item, mismatch);
        }
        return ok;
    }

    private boolean getFunctionAppliedToRowOk(ArrayItem row) {
        boolean nameLookupOk;
        boolean indexLookupOk;

        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        try {
            nameLookupOk = nameLookupOk(ctxt, rowIdx, row);
            indexLookupOk = indexLookupOk(ctxt, rowIdx, row);
            return nameLookupOk && indexLookupOk;
        } catch (XPathException ex) {
            return false;
        }
    }

    private boolean nameLookupOk(XPathContext ctxt, int rowIdx, ArrayItem actualRow) throws XPathException {
        return nameLookupCheck(ctxt, rowIdx, actualRow, (expected, actual) -> {
            try {
                return Matchers.comparesEqualTo(expected).matches(actual.head().atomize().head().toString());
            } catch (XPathException ex) {
                return false;
            }
        });
    }

    private void mismatchNameLookupCheck(XPathContext ctxt, int rowIdx, ArrayItem actualRow, Description mismatchDescription) {
        try {
            boolean ok = nameLookupCheck(ctxt, rowIdx, actualRow, (expected, actual) -> {
                try {
                    String actualString = actual.head().atomize().head().toString();
                    Matcher<String> matcher = Matchers.comparesEqualTo(expected);
                    if (!matcher.matches(actualString)) {
                        matcher.describeMismatch(actualString, mismatchDescription);
                        return false;
                    }
                    return true;
                } catch (XPathException ex) {
                    mismatchDescription.appendText("      exception while looking up get: ").appendValue(ex).appendText("\n");
                    return false;
                }
            });
            if (ok) mismatchDescription.appendText("    + get: field lookup by name ok\n");
        } catch (XPathException ex) {
            mismatchDescription.appendText("    - get: field lookup by name failed with exception: ").appendValue(ex).appendText("\n");
        }
    }

    private boolean nameLookupCheck(XPathContext ctxt, int rowIdx, ArrayItem actualRow, BiFunction<String, Sequence, Boolean> checker) throws XPathException {
        boolean ok = true;
        if (expectedColumns != null) {
            for (KeyValuePair kv : ((MapItem) expectedColumns).keyValuePairs()) {
                Sequence actual = callGetFunction(ctxt, actualGetFunction, rowIdx, kv.key);
                int oneBasedIndex = (int) ((Int64Value) kv.value.head().atomize().head()).longValue();
                int zeroBasedIndex = oneBasedIndex - 1;
                String expected = zeroBasedIndex >= expectedRow.arrayLength() ? "" : expectedRow.get(zeroBasedIndex).toString();
                if (!checker.apply(expected, actual)) ok = false;
            }
        }
        return ok;
    }

    private boolean indexLookupOk(XPathContext ctxt, int rowIdx, ArrayItem actualRow) throws XPathException {
        return indexLookupCheck(ctxt, rowIdx, actualRow, (expected, actual) -> {
            try {
                return Matchers.comparesEqualTo(expected).matches(actual.head().atomize().head().toString());
            } catch (XPathException ex) {
                return false;
            }
        });
    }

    private boolean indexLookupCheck(XPathContext ctxt, int rowIdx, ArrayItem actualRow, BiFunction<String, Sequence, Boolean> checker) throws XPathException {
        boolean ok = true;
        for (int i = 0; i < expectedRow.arrayLength(); i++) {
            Int64Value oneIndexedKey = new Int64Value(i + 1);

            Sequence actual = callGetFunction(actualGetFunction, rowIdx, oneIndexedKey);
            String expected = expectedRow.get(i).toString();
            if (!checker.apply(expected, actual)) {
                ok = false;
            };
        }
        return ok;
    }

    private void mismatchIndexLookupCheck(XPathContext ctxt, int rowIdx, ArrayItem actualRow, Description mismatchDescription) {
        try {
            boolean ok = indexLookupCheck(ctxt, rowIdx, actualRow, (expected, actual) -> {
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

    private void mismatchGetFunction(ArrayItem actualRow, Description mismatchDescription) {
        if (getFunctionAppliedToRowOk(actualRow)) {
            mismatchDescription.appendText("  + field function ok\n");
        } else {
            mismatchDescription.appendText("  - field function not ok:\n");
            XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
            mismatchNameLookupCheck(ctxt, rowIdx, actualRow, mismatchDescription);
            mismatchIndexLookupCheck(ctxt, rowIdx, actualRow, mismatchDescription);
        }
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
        description.appendText("a csv row with fields ").appendValueList("", ", ", "", expectedRow);
    }

    public static Matcher<ArrayItem> matchesCsvGetFunctionWithRow(ArrayItem row, int rowIdx, MapItem columns, FunctionItem getFunction) {
        return new MatchesCsvGetFunctionWithRow(row, rowIdx, columns, getFunction);
    }
}
