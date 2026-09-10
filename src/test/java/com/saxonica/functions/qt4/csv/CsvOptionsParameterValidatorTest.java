package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;

class CsvOptionsParameterValidatorTest {
    Map<String, GroundedValue> options;

    public Map<String, GroundedValue> makeOptions(DslLambda<MapBuilder> optionsBuilder) {
        MapItem options = FixtureBuilder.map(optionsBuilder);
        Map<String, GroundedValue> result = new HashMap<>();
        options.keyValuePairs().forEach(kvPair -> {
            result.put(((StringValue) kvPair.key).getStringValue(), kvPair.value);
        });
        return result;
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

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("column-names maps cannot use negative integers as a value")
            void columnNamesMapsNegativeErrors() {
                options = makeOptions(opts -> {
                    opts.put("column-names", FixtureBuilder.map(map -> {
                        map.put("First", -1);
                    }));
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("number-of-columns cannot be set to zero")
            void numberOfColumnsZeroErrors()  {
                options = makeOptions(opts -> {
                    opts.put("number-of-columns", 0);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("number-of-columns cannot be set to a negative integer")
            void negativeNumberOfColumnsErrors()  {
                options = makeOptions(opts -> {
                    opts.put("number-of-columns", -1);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("filter-columns cannot contain zero")
            void filterColumnsZero()  {
                options = makeOptions(opts -> {
                    opts.put("filter-columns", 0);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("filter-columns cannot contain a negative integer")
            void filterColumnsNegative()  {
                options = makeOptions(opts -> {
                    opts.put("filter-columns", -1);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }
        }
    }
}