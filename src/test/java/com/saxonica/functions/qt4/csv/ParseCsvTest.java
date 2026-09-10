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
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static com.saxonica.test.matchers.MatchesCsvRecord.matchesCsvRecord;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.MatchesThrownXPathException.throwsXPathException;

class ParseCsvTest {
    MapItem options = MapBuilder.empty();
    Configuration configuration = new Configuration();

    public MapItem actual;
    public MapItem expected;

    public SystemFunction subject() throws XPathException {
        return XPath40FunctionSet.getInstance().makeFunction("parse-csv", 2);
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


    @Test
    @DisplayName("everything should be wired up correctly")
    void everythingIsWiredUpCorrectly() throws XPathException {
        options = makeOptions(opts -> {
            opts.put("header", BooleanValue.TRUE);
        });
        expected = FixtureBuilder.map(cols -> {
                cols.put("column-index", cols.map(names -> names.put("a", 1).put("b", 2)));
                cols.put("columns", cols.sequence("a", "b"));
                cols.put("rows", cols.array("f1", "f2"));
            });

        actual = (MapItem) subject().call(ctxt(), args("a,b\nf1,f2", options)).head();

        assertThat(actual, matchesCsvRecord(expected));
    }

    @Test
    @DisplayName("option parameter validation is validating things")
    void columnNamesMapsZeroErrors() {
        options = makeOptions(opts -> {
            opts.put("select-columns", opts.sequence(0));
        });

        assertThat(() -> subject().call(ctxt(), args("", options)), throwsXPathException("XPTY0004"));
    }
}
