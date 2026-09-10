package com.saxonica.functions.qt4.csv;

import com.saxonica.test.matchers.MatchesCsvRecordGetFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.*;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static com.saxonica.test.matchers.MatchesCsvRecord.matchesCsvRecord;
import static com.saxonica.test.matchers.MatchesCsvRecordGetFunction.callGetFunction;
import static com.saxonica.test.matchers.MatchesCsvRecordGetFunction.getFunctionItem;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;
import static net.sf.saxon.test.xdmfixtures.AtomicValueBuilder.string;
import static org.hamcrest.core.Is.is;
import static org.hamcrest.core.IsEqual.equalTo;

class CsvRecordGeneratorTest {
    CsvRecordGenerator subject;
    Map<String, GroundedValue> options = new HashMap<>();
    MapItem expected;
    MapItem actual;

    CsvRecordGenerator subject() throws XPathException {
        if (subject == null) subject = new CsvRecordGenerator(options);
        return subject;
    }

    @Test
    @DisplayName("Empty sequence produces an appropriate Map")
    void emptyCsv() throws XPathException {
        expected = CsvRecordGenerator.EMPTY_CSV;

        actual = subject().fromRowSequence(EmptySequence.getInstance().iterate());

        assertThat(actual, deepEquals(expected));
    }

    @Nested
    class ExtractingColumns {
        SequenceIterator input = makeRowsIterator(r -> r.row("name", "city").row("Alice", "Aachen"));

        @Test
        @DisplayName("generates an Empty columns record by default")
        void defaultsToNoColumnExtraction() throws XPathException {
            actual = subject().fromRowSequence(input);
            expected = FixtureBuilder.map(e -> {
                e.put("columns", e.sequence());
                e.put("column-index", e.map());
                e.put("rows", e.sequence(rows -> {
                    rows.append(e.array("name", "city")).append(e.array("Alice", "Aachen"));
                }));
            });

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Nested
        class ColumnNameExtractionOn {
            @BeforeEach
            void addColumnNamesOption() {
                options.put("header", BooleanValue.TRUE);
            }

            @Test
            @DisplayName("generates the right Columns record when asked")
            void generatesColumnsRecord() throws XPathException {
                expected = FixtureBuilder.map(e -> {
                    e.put("columns", e.sequence("name", "city"));
                    e.put("column-index", e.map(cols -> {
                        cols.put("name", 1);
                        cols.put("city", 2);
                    }));
                    e.put("rows", e.sequence(rows -> {
                        rows.append(e.array("Alice", "Aachen"));
                    }));

                });
                actual = subject().fromRowSequence(input);

                assertThat(actual, matchesCsvRecord(expected));
            }
        }

        @Nested
        class ColumnNamesSupplied {
            @Test
            @DisplayName("Supplying column names as a sequence produces the correct csv-record")
            void columnsNamesCanBeSupplied() throws XPathException {
                options.put("header", FixtureBuilder.sequence(s -> s.append("Person", "Stadt")));
                expected = FixtureBuilder.map(cols -> {
                    cols.put("column-index", cols.map(names -> {
                        names.put("Person", 1);
                        names.put("Stadt", 2);
                    }));
                    cols.put("columns", cols.sequence("Person", "Stadt"));
                    cols.put("rows", cols.sequence(rows -> rows.append(cols.array("name", "city")).append(cols.array("Alice", "Aachen"))));
                });
                actual = subject().fromRowSequence(input);

                assertThat(actual, matchesCsvRecord(expected));
            }
        }
    }

    @Nested
    class FetchingNonExistentFields {
        SequenceIterator input = makeRowsIterator(r -> r.row("name", "city").row("Alice", "Aachen"));

        @Test
        @DisplayName("Attempting to fetch an unknown name raises the appropriate error")
        void fetchingUnknownColumnNameFails() throws XPathException{
            options.put("header", BooleanValue.TRUE);
            actual = subject().fromRowSequence(input);
            FunctionItem fieldFunction = getFunctionItem(actual);

            assertThat(() -> MatchesCsvRecordGetFunction.callGetFunction(fieldFunction, 1, "fnord"), throwsXPathException("FOCV0004"));
        }

        @Test
        @DisplayName("Attempting to fetch an non-existent field by integer returns the empty string")
        void fetchingNonexistentColumnByInteger() throws XPathException {
            options.put("header", BooleanValue.TRUE);
            actual = subject().fromRowSequence(input);
            FunctionItem fieldFunction = getFunctionItem(actual);

            assertThat(callGetFunction(fieldFunction, 1, 3), is(equalTo(string(""))));
        }
    }

    @Nested
    class SelectingColumns {
        SequenceIterator input = makeRowsIterator(r -> r.row("a", "b", "c", "d", "e", "f").row("1", "2", "3", "4"));

        @Test
        @DisplayName("columns can be selected in natural order (truncated)")
        void restrictColumnsInNaturalOrder() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1, 2, 3));

            expected = FixtureBuilder.map(cols -> {
                cols.put("column-index", cols.map());
                cols.put("columns", cols.sequence());
                cols.put("rows", cols.sequence(rows -> rows.append(cols.array("a", "b", "c")).append(cols.array("1", "2", "3"))));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("columns can be reordered")
        void reorderColumns() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(2, 3, 1));

            expected = FixtureBuilder.map(cols -> {
                cols.put("column-index", cols.map());
                cols.put("columns", cols.sequence());
                cols.put("rows", cols.sequence(rows -> rows.append(cols.array("b", "c", "a")).append(cols.array("2", "3", "1"))));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("columns and columns-index are properly tweaked when column extraction is on")
        void reorderedNamedColumns() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(2, 3, 1));
            options.put("header", BooleanValue.TRUE);

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map(names -> {
                    names.put("b", 1);
                    names.put("c", 2);
                    names.put("a", 3);
                }));
                e.put("columns", e.sequence("b", "c", "a"));
                e.put("rows", e.sequence(rows -> rows.append(rows.array("2", "3", "1"))));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("select-columns does not reorder or filter column names if column-names were passed as an explicit sequence")
        void filteringDoesNotAffectSuppliedNames() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(s -> s.append(2, 3, 1)));
            options.put("header", FixtureBuilder.sequence("First", "Second", "Third"));

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map(names -> {
                    names.put("First", 1)
                            .put("Second", 2)
                            .put("Third", 3);
                }));
                e.put("columns", e.sequence("First", "Second", "Third"));
                e.put("rows", e.sequence(rows -> rows.append(rows.array("b", "c", "a"), rows.array("2", "3", "1"))));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("the number of columns can be padded")
        void padding() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1,2,3,4,5,6,7));

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map());
                e.put("columns", e.sequence());
                e.put("rows", e.sequence(rows -> {
                    rows.append(
                            e.array("a", "b", "c", "d", "e", "f", ""),
                            e.array("1", "2", "3", "4", "", "", ""));
                }));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("some rows can be padded while other rows are truncated")
        void padAndTruncate() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1,2,3,4,5));

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map());
                e.put("columns", e.sequence());
                e.put("rows", e.sequence(rows -> {
                    rows.append(
                            e.array("a", "b", "c", "d", "e"),
                            e.array("1", "2", "3", "4", ""));
                }));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }
    }

    @Nested
    class TrimmingRows {
        SequenceIterator input = makeRowsIterator(r -> r.row("a", "b", "c", "d").row("1", "2", "3").row("1", "2", "3", "4", "5"));

        @Test
        @DisplayName("following rows size is conformed to the first")
        void followingRowsAreConformed() throws XPathException {
            options.put("trim-rows", BooleanValue.TRUE);

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map());
                e.put("columns", e.sequence());
                e.put("rows", e.sequence(rows -> {
                    rows.append(
                            e.array("a", "b", "c", "d"),
                            e.array("1", "2", "3", ""),
                            e.array("1", "2", "3", "4"));
                }));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }

        @Test
        @DisplayName("the first row, extracted by header: true(), is also used as the first row for trim-rows")
        void columnsNamesRowIsFirst() throws XPathException {
            options.put("trim-rows", BooleanValue.TRUE);
            options.put("column-names", BooleanValue.TRUE);

            expected = FixtureBuilder.map(e -> {
                e.put("column-index", e.map(names -> {
                    names.put("a", 1)
                            .put("b", 2)
                            .put("c", 3)
                            .put("d", 4);
                }));
                e.put("columns", e.sequence("a", "b", "c", "d"));
                e.put("rows", e.sequence(rows -> {
                    rows.append(e.array("1", "2", "3", ""), e.array("1", "2", "3", "4"));
                }));
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, matchesCsvRecord(expected));
        }
    }
}
