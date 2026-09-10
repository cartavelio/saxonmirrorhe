package com.saxonica.test.matchers;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;
import org.hamcrest.collection.IsIterableContainingInOrder;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.IntStream;
import java.util.stream.Stream;

public class MatchesCsvRecordGetFunction extends TypeSafeDiagnosingMatcher<MapItem> {
    private MapItem expected;
    private MapItem expectedColumns;
    private List<ArrayItem> expectedRows;

    MatchesCsvRecordGetFunction(MapItem expected) {
        this.expected = expected;
        this.expectedColumns = (MapItem) expected.get(new StringValue("column-index"));
        this.expectedRows = sequenceToStream(expected.get(new StringValue("rows"))).map(row -> (ArrayItem) row).collect(Collectors.toList());
    }

    private List<MatchesCsvGetFunctionWithRow> buildRowMatchers(MapItem expected, FunctionItem actualGetFunction) {
        return IntStream.range(0, expectedRows.size())
                .mapToObj(rowIdx -> new MatchesCsvGetFunctionWithRow(expectedRows.get(rowIdx), rowIdx + 1, expectedColumns, actualGetFunction))
                .collect(Collectors.toList());
    }

    @Override
    protected boolean matchesSafely(MapItem item, Description mismatch) {
        boolean ok = getFunctionOk(item);
        if (ok) {
            mismatch.appendText("get function ok\n");
        } else {
            mismatch.appendText("\n");
            mismatchGetFunction(item, mismatch);
        }
        return ok;
    }

    private void mismatchGetFunction(MapItem item, Description mismatchDescription) {
        boolean ok = getFunctionOk(item);
        if (ok) {
            mismatchDescription.appendText("  + get function ok\n");
        } else {
            mismatchDescription.appendText("  - get function not ok: ");
            try {
                FunctionItem actualGetFunction = getFunctionItem(item);
                new IsIterableContainingInOrder(buildRowMatchers(expected, actualGetFunction)).describeMismatch(actualRows(item), mismatchDescription);
                mismatchDescription.appendText("\n");
            } catch (XPathException ex) {
                mismatchDescription.appendText("\n");
            }
        }
    }

    private List<ArrayItem> actualRows(MapItem item) {
        return sequenceToStream(item.get(new StringValue("rows"))).map(row -> (ArrayItem) row).collect(Collectors.toList());
    }

    private boolean getFunctionOk(MapItem item) {
        List<ArrayItem> actualRows = sequenceToStream(item.get(new StringValue("rows"))).map(row -> (ArrayItem) row).collect(Collectors.toList());

        if (!checkGetFunctionExists(item)) return false;
        try {
            FunctionItem actualGetFunction = getFunctionItem(item);
            return new IsIterableContainingInOrder(buildRowMatchers(expected, actualGetFunction)).matches(actualRows);
        } catch (XPathException ex) {
            return false;
        }
    }

    private boolean checkGetFunctionExists(MapItem item) {
        GroundedValue actualGetValue = item.get(new StringValue("get"));
        if (actualGetValue.getLength() > 1) return false;
        Item actualGetFunctionItem = actualGetValue.head();
        if (actualGetFunctionItem.getGenre() != Genre.FUNCTION) return false;
        return true;
    }

    public static FunctionItem getFunctionItem(MapItem item) throws XPathException {
        return (FunctionItem) item.get(new StringValue("get")).head().head();
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
        description.appendText("a csv-record `get` function");
    }

    public static Matcher<MapItem> matchesCsvRecordGetFunction(MapItem expected) {
        return new MatchesCsvRecordGetFunction(expected);
    }

    public static Sequence callGetFunction(XPathContext ctxt, FunctionItem fieldFunction, int rowIdx, AtomicValue key) throws XPathException{
        return fieldFunction.call(ctxt, SequenceTool.fromItems(new Int64Value(rowIdx), key));
    }

    public static Sequence callGetFunction(FunctionItem fieldFunction, int rowIdx, AtomicValue key) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return callGetFunction(ctxt, fieldFunction, rowIdx, key);
    }

    public static Sequence callGetFunction(FunctionItem fieldFunction, int rowIdx, String key) throws XPathException {
        return callGetFunction(fieldFunction, rowIdx, new StringValue(key));
    }

    public static Sequence callGetFunction(FunctionItem fieldFunction, int rowIdx, int idx) throws XPathException {
        return callGetFunction(fieldFunction, rowIdx, new Int64Value(idx));
    }
}
