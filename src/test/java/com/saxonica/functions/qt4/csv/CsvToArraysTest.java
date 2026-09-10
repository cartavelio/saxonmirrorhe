package com.saxonica.functions.qt4.csv;

import com.saxonica.functions.registry.XPath40FunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;

class CsvToArraysTest {
    Configuration configuration = new Configuration();
    RetainedStaticContext retainedStaticContext = new RetainedStaticContext(configuration);

    MapItem options;
    GroundedValue expected;
    GroundedValue actual;

    public SystemFunction subject() throws XPathException {
        return XPath40FunctionSet.getInstance().makeFunction("csv-to-arrays", 2);
    }

    public SystemFunctionCall expression(Sequence[] args) throws XPathException {
        SystemFunctionCall call = new SystemFunctionCall(subject(), new Expression[args.length]);
        call.setRetainedStaticContext(retainedStaticContext);
        return call;
    }

    public GroundedValue call_subject(Sequence[] args) throws XPathException {
        return expression(args).call(ctxt(), args).materialize();
    }

    public XPathContext ctxt() {
        return new Controller(configuration).newXPathContext();
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
        options = FixtureBuilder.map(b -> {});
        expected = FixtureBuilder.sequence(seq -> {
            seq.append(seq.array("a", "b"), seq.array("f1", "f2"));
        });

        actual = call_subject(args("a,b\nf1,f2", options));

        assertThat(actual, deepEquals(expected));
    }


}