package com.saxonica.functions.qt4.csv;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xmlfixtures.DslLambda;
import net.sf.saxon.test.xmlfixtures.XmlFixtureBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static com.saxonica.test.matchers.MatchesCsvRecord.matchesCsvRecord;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;
import static org.hamcrest.core.Is.is;

class CsvXmlGeneratorTest {
    CsvXmlGenerator subject;
    Map<String, GroundedValue> options = new HashMap<>();
    Item expected;
    Item actual;

    Configuration configuration = new Configuration();
    XPathContext context = new Controller(configuration).newXPathContext();
    XmlFixtureBuilder xml = new XmlFixtureBuilder(configuration);

    CsvXmlGenerator subject() throws XPathException {
        if (subject == null) subject = new CsvXmlGenerator(options, context, "staticBaseUri");
        return subject;
    }

    Item csvDocFixture(DslLambda definition) throws XPathException {
        return xml.element(builder -> {
            builder.defaultNs("http://www.w3.org/2005/xpath-functions");
            builder.element("csv", definition);
        });
    }

    @Test
    @DisplayName("Empty sequence produces an appropriate XML document")
    void emptyCsv() throws XPathException {
        expected = csvDocFixture(csv -> {
            csv.element("rows");
        });

        actual = subject().fromRowSequence(EmptySequence.getInstance().iterate());

        assertThat(actual, deepEquals(expected));
    }

    @Nested
    class ExtractingColumns {
        SequenceIterator input = makeRowsIterator(r -> r.row("name", "city").row("Alice", "Aachen"));

        @Test
        @DisplayName("generates an Empty columns record by default")
        void defaultsToNoColumnExtraction() throws XPathException {
            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field", "name").element("field", "city");
                    }).element("row", row -> {
                        row.element("field", "Alice").element("field", "Aachen");
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Nested
        class ColumnNameExtractionOn {
            @BeforeEach
            void addColumnNamesOption() {
                options.put("header", BooleanValue.TRUE);
            }

            @Test
            @DisplayName("generates an empty <columns> record when asked to extract columns from an empty input")
            void emptyInputGeneratesEmptyColumnsRecord() throws XPathException {
                expected = csvDocFixture(csv -> {
                    csv.element("columns");
                    csv.element("rows");
                });

                actual = subject().fromRowSequence(EmptySequence.getInstance().iterate());

                assertThat(actual, deepEquals(expected));
            }

            @Test
            @DisplayName("generates the right Columns record when asked")
            void generatesColumnsRecord() throws XPathException {
                expected = csvDocFixture(csv -> {
                    csv.element("columns", cols -> {
                        cols.element("column", "name").element("column", "city");
                    });
                    csv.element("rows", rows -> {
                        rows.element("row", row -> {
                            row.element("field", f -> f.attr("column", "name").content("Alice"));
                            row.element("field", f -> f.attr("column", "city").content("Aachen"));
                        });
                    });
                });

                actual = subject().fromRowSequence(input);

                assertThat(actual, deepEquals(expected));
            }
        }

        @Nested
        class ColumnNamesSupplied {
            @Test
            @DisplayName("Supplying column names as a map produces the correct csv-record")
            void columnsNamesCanBeSupplied() throws XPathException {
                options.put("header", FixtureBuilder.sequence("Person", "Stadt"));
                expected = csvDocFixture(csv -> {
                    csv.element("columns", cols -> {
                        cols.element("column", "Person").element("column", "Stadt");
                    });
                    csv.element("rows", rows -> {
                        rows.element("row", row -> {
                            row.element("field",  f -> f.attr("column", "Person").content("name"));
                            row.element("field", f -> f.attr("column", "Stadt").content("city"));
                        }).element("row", row -> {
                            row.element("field",  f -> f.attr("column", "Person").content("Alice"));
                            row.element("field", f -> f.attr("column", "Stadt").content("Aachen"));
                        });
                    });
                });

                actual = subject().fromRowSequence(input);

                assertThat(actual, deepEquals(expected));
            }
        }
    }

    @Nested
    class SelectingColumns {
        SequenceIterator input = makeRowsIterator(r -> r.row("a", "b", "c", "d", "e", "f").row("1", "2", "3", "4"));

        @Test
        @DisplayName("columns can be selected in natural order (truncated)")
        void restrictColumnsInNaturalOrder() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1, 2, 3));

            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("a"));
                        row.element("field",  f -> f.content("b"));
                        row.element("field",  f -> f.content("c"));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("columns can be reordered")
        void reorderColumns() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(2, 3, 1));

            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("b"));
                        row.element("field",  f -> f.content("c"));
                        row.element("field",  f -> f.content("a"));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content("1"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("columns is properly tweaked when column extraction is on")
        void reorderedNamedColumns() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(2, 3, 1));
            options.put("header", BooleanValue.TRUE);

            expected = csvDocFixture(csv -> {
                csv.element("columns", cols -> {
                    cols.element("column", "b").element("column", "c").element("column", "a");
                });
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.attr("column", "b").content("2"));
                        row.element("field",  f -> f.attr("column", "c").content("3"));
                        row.element("field",  f -> f.attr("column", "a").content("1"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("select-columns does not reorder or filter column names if column-names were passed as an explicit sequence")
        void filteringDoesNotAffectSuppliedNames() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(s -> s.append(2, 3, 1)));
            options.put("header", FixtureBuilder.sequence("First", "Second", "Third"));

            expected = csvDocFixture(csv -> {
                csv.element("columns", cols -> {
                    cols.element("column", "First").element("column", "Second").element("column", "Third");
                });
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.attr("column", "First").content("b"));
                        row.element("field",  f -> f.attr("column", "Second").content("c"));
                        row.element("field",  f -> f.attr("column", "Third").content("a"));
                    });
                    rows.element("row", row -> {
                        row.element("field",  f -> f.attr("column", "First").content("2"));
                        row.element("field",  f -> f.attr("column", "Second").content("3"));
                        row.element("field",  f -> f.attr("column", "Third").content("1"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("the number of columns can be padded")
        void padding() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1,2,3,4,5,6,7));

            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("a"));
                        row.element("field",  f -> f.content("b"));
                        row.element("field",  f -> f.content("c"));
                        row.element("field",  f -> f.content("d"));
                        row.element("field",  f -> f.content("e"));
                        row.element("field",  f -> f.content("f"));
                        row.element("field",  f -> f.content(""));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content("4"));
                        row.element("field",  f -> f.content(""));
                        row.element("field",  f -> f.content(""));
                        row.element("field",  f -> f.content(""));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("some rows can be padded while other rows are truncated")
        void padAndTruncate() throws XPathException {
            options.put("select-columns", FixtureBuilder.sequence(1,2,3,4,5));

            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("a"));
                        row.element("field",  f -> f.content("b"));
                        row.element("field",  f -> f.content("c"));
                        row.element("field",  f -> f.content("d"));
                        row.element("field",  f -> f.content("e"));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content("4"));
                        row.element("field",  f -> f.content(""));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }
    }

    @Nested
    class TrimmingRows {
        SequenceIterator input = makeRowsIterator(r -> r.row("a", "b", "c", "d").row("1", "2", "3").row("1", "2", "3", "4", "5"));

        @Test
        @DisplayName("following rows size is conformed to the first")
        void followingRowsAreConformed() throws XPathException {
            options.put("trim-rows", BooleanValue.TRUE);

            expected = csvDocFixture(csv -> {
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("a"));
                        row.element("field",  f -> f.content("b"));
                        row.element("field",  f -> f.content("c"));
                        row.element("field",  f -> f.content("d"));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content(""));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content("4"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));
        }

        @Test
        @DisplayName("the first row, extracted by header: true(), is also used as the first row for trim-rows")
        void columnsNamesRowIsFirst() throws XPathException {
            options.put("trim-rows", BooleanValue.TRUE);
            options.put("column-names", BooleanValue.TRUE);

            expected = csvDocFixture(csv -> {
                csv.element("columns", cols -> {
                    cols.element("column", "a").element("column", "b").element("column", "c").element("column", "d");
                });
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content(""));
                    }).element("row", row -> {
                        row.element("field",  f -> f.content("1"));
                        row.element("field",  f -> f.content("2"));
                        row.element("field",  f -> f.content("3"));
                        row.element("field",  f -> f.content("4"));
                    });
                });
            });

            actual = subject().fromRowSequence(input);

            assertThat(actual, deepEquals(expected));        }
    }
}
