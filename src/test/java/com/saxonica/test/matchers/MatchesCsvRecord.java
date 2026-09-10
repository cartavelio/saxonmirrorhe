package com.saxonica.test.matchers;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.StringValue;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;

import java.util.stream.Collectors;
import java.util.stream.Stream;

import static com.saxonica.test.matchers.MatchesCsvGetFunctionWithRow.sequenceIteratorToList;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;
import static org.hamcrest.collection.IsIterableContainingInAnyOrder.containsInAnyOrder;

public class MatchesCsvRecord extends TypeSafeDiagnosingMatcher<MapItem> {
    private MapItem expected;
    MatchesCsvRecord(MapItem expected) {
        this.expected = expected;
    }

    @Override
    protected boolean matchesSafely(MapItem item, Description mismatchDescription) {
        mismatchDescription.appendText("was ");

        if (recordShapeBad(item, mismatchDescription)) return false;
        boolean columns = columnsOk(item.get(new StringValue("columns")), mismatchDescription);
        boolean columnIndex = columnIndexOk(item.get(new StringValue("column-index")), mismatchDescription);
        boolean rows = rowsOk(item.get(new StringValue("rows")), mismatchDescription);
        boolean get = getOk(item, mismatchDescription);
        return columns && columnIndex && rows && get;
    }

    private boolean recordShapeOk(MapItem actualCsvRecord, Description mismatchDescription) {
        String[] expectedKeys = Stream.of("columns", "column-index", "rows", "get").toArray(String[]::new);
//        boolean ok = containsInAnyOrder(expectedKeys).matches(sequenceIteratorToList(item.keys()));

        boolean ok = containsInAnyOrder(expectedKeys).matches(sequenceIteratorToList(
                actualCsvRecord.keys()).stream().map(k -> k.toString()).collect(Collectors.toList()));
        if (ok) {
            mismatchDescription.appendText("a csv-record: \n");
        } else {
            mismatchDescription.appendText("not a csv-record: wrong shape, contained keys: ").appendValue(sequenceIteratorToList(actualCsvRecord.keys())).appendText("\n");
        }
        return ok;
    }

    private boolean recordShapeBad(MapItem item, Description mismatchDescription) {
        return !recordShapeOk(item, mismatchDescription);
    }

    private boolean rowsOk(GroundedValue actualRows, Description mismatchDescription) {
        GroundedValue rows = expected.get(new StringValue("rows"));
        Matcher<GroundedValue> rowsMatcher = deepEquals(rows);

        boolean ok = rowsMatcher.matches(actualRows);

        if (ok) {
            mismatchDescription.appendText("* rows ok\n");
        } else {
            mismatchDescription.appendText("* rows not ok:\n");
            rowsMatcher.describeMismatch(actualRows, mismatchDescription);
        }
        return ok;
    }

    private boolean columnsOk(GroundedValue actualColumns, Description mismatchDescription) {
        GroundedValue columns = expected.get(new StringValue("columns"));
        Matcher<GroundedValue> columnsMatcher = deepEquals(columns);
        boolean ok = columnsMatcher.matches(actualColumns);
        if (ok) {
            mismatchDescription.appendText("* columns map ok\n");
        } else {
            columnsMatcher.describeMismatch(actualColumns, mismatchDescription);
        }
        return ok;
    }

    private boolean columnIndexOk(GroundedValue actualColumnIndex, Description mismatchDescription) {
        GroundedValue columnIndex = expected.get(new StringValue("column-index"));
        Matcher<GroundedValue> columnsMatcher = deepEquals(columnIndex);
        boolean ok = columnsMatcher.matches(actualColumnIndex);
        if (ok) {
            mismatchDescription.appendText("* column-index ok\n");
        } else {
            columnsMatcher.describeMismatch(actualColumnIndex, mismatchDescription);
        }
        return ok;
    }

    private boolean getOk(MapItem actualCsvRecord, Description mismatchDescription) {
        boolean ok = new MatchesCsvRecordGetFunction(expected).matches(actualCsvRecord);
        if (ok) {
            mismatchDescription.appendText("* get() ok\n");
        } else {
            mismatchDescription.appendText("* get() not ok:\n");
            new MatchesCsvRecordGetFunction(expected).describeMismatch(actualCsvRecord, mismatchDescription);
        }
        return ok;
    }

    @Override
    public void describeTo(Description description) {
        description.appendText("a csv-record with csv-columns-record matching ").appendValue(expected);
    }

//    @Override
//    public void describeTo(Description description) {
//        description.appendText("a csv-record with csv-columns-record matching ").appendValue(columns).appendText(" and rows matching");
//        description.appendValueList("[\n  ", ",\n  ", "\n]", rowMatchers);
//    }

    public static Matcher<MapItem> matchesCsvRecord(MapItem expected) {
        return new MatchesCsvRecord(expected);
    }
}
