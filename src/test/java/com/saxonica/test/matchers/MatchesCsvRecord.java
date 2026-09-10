package com.saxonica.test.matchers;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.StringValue;
import org.hamcrest.Description;
import org.hamcrest.Matcher;
import org.hamcrest.TypeSafeDiagnosingMatcher;
import org.hamcrest.collection.IsIterableContainingInOrder;

import java.util.List;
import java.util.stream.Collectors;

import static com.saxonica.test.matchers.MatchesCsvRowRecord.*;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;
import static org.hamcrest.collection.IsIterableContainingInAnyOrder.containsInAnyOrder;

public class MatchesCsvRecord extends TypeSafeDiagnosingMatcher<MapItem> {
    private MapItem columns;
    private List<List<String>> rows;
    private List<Matcher<MapItem>> rowMatchers;
    MatchesCsvRecord(MapItem columns, List<List<String>> rows) {
        this.columns = columns;
        this.rows = rows;
        this.rowMatchers = buildRowMatchers();
    }

    @Override
    protected boolean matchesSafely(MapItem item, Description mismatchDescription) {
        mismatchDescription.appendText("was ");

        if (recordShapeBad(item, mismatchDescription)) return false;
        boolean columns = columnsOk(item.get(new StringValue("columns")), mismatchDescription);
        boolean rows = rowsOk(item.get(new StringValue("rows")), mismatchDescription);
        return columns && rows;
    }

    private boolean recordShapeOk(MapItem item, Description mismatchDescription) {
        StringValue[] expectedKeys = new StringValue[]{ new StringValue("columns"), new StringValue("rows")};
        boolean ok = containsInAnyOrder(expectedKeys).matches(sequenceIteratorToList(item.keys()));
        if (ok) {
            mismatchDescription.appendText("a csv-record: \n");
        } else {
            mismatchDescription.appendText("not a csv-record: wrong shape, contained keys: ").appendValue(sequenceIteratorToList(item.keys())).appendText("\n");
        }
        return ok;
    }

    private boolean recordShapeBad(MapItem item, Description mismatchDescription) {
        return !recordShapeOk(item, mismatchDescription);
    }
    private List<Matcher<MapItem>> buildRowMatchers() {
        return rows.stream().map(row -> matchesCsvRowRecord(row, columns)).collect(Collectors.toList());
    };

    private boolean rowsOk(GroundedValue actualRows, Description mismatchDescription) {
        List<MapItem> actualRowsList = sequenceToStream(actualRows).map(item -> (MapItem) item).collect(Collectors.toList());

//        Matcher<Iterable<? extends List<Matcher<MapItem>>>> matcher = new IsIterableContainingInOrder(rowMatchers);
        boolean ok = new IsIterableContainingInOrder(rowMatchers).matches(actualRowsList);
        if (ok) {
            mismatchDescription.appendText("* rows ok\n");
        } else {
            mismatchDescription.appendText("* rows not ok:\n");
            new IsIterableContainingInOrder(rowMatchers).describeMismatch(actualRowsList, mismatchDescription);
        }
        return ok;
    }

    private boolean columnsOk(GroundedValue actualColumns, Description mismatchDescription) {
        Matcher<GroundedValue> columnsMatcher = deepEquals(columns);
        boolean ok = columnsMatcher.matches(actualColumns);
        if (ok) {
            mismatchDescription.appendText("* csv-columns-record ok\n");
        } else {
            columnsMatcher.describeMismatch(actualColumns, mismatchDescription);
        }
        return ok;
    }

    @Override
    public void describeTo(Description description) {
        description.appendText("a csv-record with csv-columns-record matching ").appendValue(columns).appendText(" and rows matching");
        description.appendValueList("[\n  ", ",\n  ", "\n]", rowMatchers);
    }

    public static Matcher<MapItem> matchesCsvRecord(MapItem columns, List<List<String>> rows) {
        return new MatchesCsvRecord(columns, rows);
    }
}
