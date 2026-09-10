package com.saxonica.functions.qt4.csv;

import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.test.OptionsParameterHelper;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.Map;

import static net.sf.saxon.test.TestXdmHelpers.stringValue;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;

class CsvOptionsParameterValidatorTest {
    Map<String, GroundedValue> options;

    public Map<String, GroundedValue> makeOptions(OptionsParameter optionsParameter, DslLambda<MapBuilder> optionsBuilder) throws XPathException {
        MapItem options = FixtureBuilder.map(optionsBuilder);
        return OptionsParameterHelper.parsedOptionParameters(optionsParameter, options);
    }

    final OptionsParameter defaultOpts = CsvFunction.extendedOptions();

    @Nested
    class OptionTypePreconditions {
        @Nested
        class PositiveIntegers {
            @Test
            @DisplayName("select-columns cannot be set to zero")
            void selectColumnsZeroErrors() throws XPathException {
                options = makeOptions(defaultOpts, opts -> {
                    opts.put("select-columns", 0);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }

            @Test
            @DisplayName("number-of-columns cannot be set to a negative integer")
            void negativeSelectColumnsErrors() throws XPathException {
                options = makeOptions(defaultOpts, opts -> {
                    opts.put("select-columns", -1);
                });

                assertThat(() -> CsvOptionsParameterValidator.validate(options), throwsXPathException("FOCV0005"));
            }
        }
    }
}