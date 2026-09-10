package com.saxonica.functions.qt4.csv;

import com.saxonica.functions.registry.XPath40FunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;

class CsvToXdmTest {
    MapItem options = MapBuilder.empty();
    Configuration configuration = new Configuration();

    public SystemFunction subject() throws XPathException {
        return XPath40FunctionSet.getInstance().makeFunction("csv-to-xdm", 2);
    }

    public XPathContext ctxt() {
        return new EarlyEvaluationContext(configuration);
    }

    public Sequence[] args(String input, MapItem options) {
        return SequenceTool.fromItems(new StringValue(input), options);
    }

    public MapItem makeOptions(DslLambda<MapBuilder> optionsBuilder) {
        return FixtureBuilder.map(optionsBuilder);
    }

    @Nested
    class OptionTypePreconditions {
        @Nested
        class PositiveIntegers {
            @Test
            @DisplayName("column-names maps cannot use zero as a value")
            void columnNamesMapsZeroErrors() {
                options = makeOptions(opts -> {
                    opts.put("column-names", FixtureBuilder.map(map -> {
                        map.put("First", 0);
                    }));
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("column-names maps cannot use negative integers as a value")
            void columnNamesMapsNegativeErrors() {
                options = makeOptions(opts -> {
                    opts.put("column-names", FixtureBuilder.map(map -> {
                        map.put("First", -1);
                    }));
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("number-of-columns cannot be set to zero")
            void numberOfColumnsZeroErrors()  {
                options = makeOptions(opts -> {
                    opts.put("number-of-columns", 0);
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("number-of-columns cannot be set to a negative integer")
            void negativeNumberOfColumnsErrors()  {
                options = makeOptions(opts -> {
                    opts.put("number-of-columns", -1);
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("filter-columns cannot contain zero")
            void filterColumnsZero()  {
                options = makeOptions(opts -> {
                    opts.put("filter-columns", 0);
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("filter-columns cannot contain a negative integer")
            void filterColumnsNegative()  {
                options = makeOptions(opts -> {
                    opts.put("filter-columns", -1);
                });

                assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("FOCV0005"));
            }
        }
    }
}