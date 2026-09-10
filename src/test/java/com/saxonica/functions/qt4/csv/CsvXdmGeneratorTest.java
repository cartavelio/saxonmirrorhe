package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.SequenceBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.*;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static com.saxonica.test.matchers.MatchesCsvRecord.matchesCsvRecord;
import static com.saxonica.test.matchers.MatchesCsvRowRecord.callFieldFunction;
import static com.saxonica.test.matchers.MatchesCsvRowRecord.getRowFieldFunctionItem;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.IsDeepEqualTo.deepEquals;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;
import static net.sf.saxon.test.xdmfixtures.AtomicValueBuilder.string;
import static org.hamcrest.core.Is.is;
import static org.hamcrest.core.IsEqual.equalTo;

class CsvXdmGeneratorTest {
    CsvXdmGenerator subject;
    Map<String, GroundedValue> options = new HashMap<>();
    MapItem expected;
    MapItem actual;

    CsvXdmGenerator subject() throws XPathException {
        if (subject == null) subject = new CsvXdmGenerator(options);
        return subject;
    }

    public MapItem getFirstRow(MapItem csvRecord) {
        return (MapItem) csvRecord.get(new StringValue("rows")).head();
    }

    public MapItem getRow(MapItem csvRecord, int rowNum) {
        return (MapItem) csvRecord.get(new StringValue("rows")).itemAt(rowNum - 1);
    }

    @Test
    @DisplayName("Empty sequence produces an appropriate Map")
    void emptyCsv() throws XPathException {
        expected = CsvXdmGenerator.EMPTY_CSV;

        actual = subject().fromRowSequence(EmptySequence.getInstance());

        assertThat(actual, deepEquals(expected));
    }

    @Test
    @DisplayName("The empty columns record is structured per the spec")
    void correctlyStructuredEmptyColumnsRecord() {
        expected = FixtureBuilder.map(m -> {
            m.put("names", m.map()).put("fields", m.sequence());
        });

        assertThat(CsvXdmGenerator.EMPTY_COLUMNS_RECORD, deepEquals(expected));
    }

    @Nested
    class ExtractingColumns {
        Sequence input = makeRows(r -> r.row("name", "city").row("Alice", "Aachen"));

        @Test
        @DisplayName("generates an Empty columns record by default")
        void defaultsToNoColumnExtraction() throws XPathException {
            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(CsvXdmGenerator.EMPTY_COLUMNS_RECORD,
                                                expectedRows(r -> r.row("name", "city").row("Alice", "Aachen"))));
        }

        @Nested
        class ColumnNameExtractionOn {
            @BeforeEach
            void addColumnNamesOption() {
                options.put("column-names", BooleanValue.TRUE);
            }

            @Test
            @DisplayName("generates the right Columns record when asked")
            void generatesColumnsRecord() throws XPathException {
                expected = FixtureBuilder.map(cols -> {
                    cols.put("names", cols.map(names -> {
                        names.put("name", 1);
                        names.put("city", 2);
                    }));
                    cols.put("fields", cols.sequence("name", "city"));
                });
                actual = subject().fromRowSequence(input);

                assertThat(actual, matchesCsvRecord(expected,
                        expectedRows(r -> r.row("Alice", "Aachen"))));
            }

        }

        @Nested
        class ColumnNamesSupplied {
            @Test
            @DisplayName("Supplying column names as a map produces the correct csv-record")
            void columnsNamesCanBeSupplied() throws XPathException {
                options.put("column-names", FixtureBuilder.map(m -> m.put("Person", 1).put("Stadt", 2)));
                expected = FixtureBuilder.map(cols -> {
                    cols.put("names", cols.map(names -> {
                        names.put("Person", 1);
                        names.put("Stadt", 2);
                    }));
                    cols.put("fields", cols.sequence("Person", "Stadt"));
                });
                actual = subject().fromRowSequence(input);

                assertThat(actual, matchesCsvRecord(expected,
                        expectedRows(r -> r.row("name", "city").row("Alice", "Aachen"))));
            }
        }
    }

    @Nested
    class FetchingNonExistentFields {
        Sequence input = makeRows(r -> r.row("name", "city").row("Alice", "Aachen"));

        @Test
        @DisplayName("Attempting to fetch an unknown name raises the appropriate error")
        void fetchingUnknownColumnNameFails() throws XPathException{
            options.put("column-names", BooleanValue.TRUE);
            actual = subject().fromRowSequence(input);
            FunctionItem fieldFunction = getRowFieldFunctionItem(getFirstRow(actual));

            assertThat(() -> callFieldFunction(fieldFunction, "fnord"), throwsXPathException("FOCV0004"));
        }

        @Test
        @DisplayName("Attempting to fetch an non-existent field by integer returns the empty string")
        void fetchingNonexistentColumnByInteger() throws XPathException {
            options.put("column-names", BooleanValue.TRUE);
            actual = subject().fromRowSequence(input);
            FunctionItem fieldFunction = getRowFieldFunctionItem(getFirstRow(actual));

            assertThat(callFieldFunction(fieldFunction, 3), is(equalTo(string(""))));
        }
    }

    @Nested
    class FilteringColumns {
        Sequence input = makeRows(r -> r.row("a", "b", "c", "d", "e").row("1", "2", "3", "4", "5"));

        @Test
        @DisplayName("columns can be filtered in natural order (truncated)")
        void restrictColumnsInNaturalOrder() throws XPathException {
            options.put("filter-columns", SequenceBuilder.build(s -> s.append(1, 2, 3)));

            actual = subject().fromRowSequence(input);

            assertThat(getFirstRow(actual).get(string("fields")), deepEquals(simpleValueSequence("a", "b", "c")));
            assertThat(getRow(actual, 2).get(string("fields")), deepEquals(simpleValueSequence("1", "2", "3")));
        }

        @Test
        @DisplayName("columns can be reordered")
        void reorderColumns() throws XPathException {
            options.put("filter-columns", SequenceBuilder.build(s -> s.append(2, 3, 1)));

            actual = subject().fromRowSequence(input);

            assertThat(getFirstRow(actual).get(string("fields")), deepEquals(simpleValueSequence("b", "c", "a")));
            assertThat(getRow(actual, 2).get(string("fields")), deepEquals(simpleValueSequence("2", "3", "1")));
        }

        @Test
        @DisplayName("csv-columns-record is properly tweaked when column extraction is on")
        void reorderedNamedColumns() throws XPathException {
            options.put("filter-columns", SequenceBuilder.build(s -> s.append(2, 3, 1)));
            options.put("column-names", BooleanValue.TRUE);

            expected = FixtureBuilder.map(cols -> {
                cols.put("names", cols.map(names -> {
                    names.put("b", 1);
                    names.put("c", 2);
                    names.put("a", 3);
                }));
                cols.put("fields", cols.sequence("b", "c", "a"));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual.get(string("columns")), deepEquals(expected));
        }

        @Test
        @DisplayName("filter-columns does not reorder or filter column names if column-names was passed an explicit map")
        void filteringDoesNotAffectSuppliedNames() throws XPathException {
            options.put("filter-columns", FixtureBuilder.sequence(s -> s.append(2, 3, 1)));
            options.put("column-names", FixtureBuilder.map(m -> m.put("First", 1).put("Second", 2).put("Third", 3)));

            expected = FixtureBuilder.map(cols -> {
                cols.put("names", cols.map(names -> {
                    names.put("First", 1)
                            .put("Second", 2)
                            .put("Third", 3);
                }));
                cols.put("fields", cols.sequence("First", "Second", "Third"));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected,
                    expectedRows(r -> r.row("b", "c", "a").row("2", "3", "1"))));
        }
    }
}
