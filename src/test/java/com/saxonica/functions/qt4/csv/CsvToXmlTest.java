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
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.TestMapBuilder;
import net.sf.saxon.test.xmlfixtures.XmlFixtureBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.GroundedValueIsDeepEqualTo.deepEquals;

class CsvToXmlTest {
    Configuration configuration = new Configuration();
    RetainedStaticContext retainedStaticContext = new RetainedStaticContext(configuration);
    XmlFixtureBuilder xml = new XmlFixtureBuilder(configuration);

    MapItem options;
    Item expected;
    Item actual;

    public SystemFunction subject() throws XPathException {
        return XPath40FunctionSet.getInstance().makeFunction("csv-to-xml", 2);
    }

    public SystemFunctionCall expression(Sequence[] args) throws XPathException {
        SystemFunctionCall call = new SystemFunctionCall(subject(), new Expression[args.length]);
        call.setRetainedStaticContext(retainedStaticContext);
        return call;
    }

    public Sequence call_subject(Sequence[] args) throws XPathException {
        return expression(args).call(ctxt(), args);
    }

    public XPathContext ctxt() {
        return new Controller(configuration).newXPathContext();
    }

    public Sequence[] args(String input, MapItem options) {
        return SequenceTool.fromItems(new StringValue(input), options);
    }

    public MapItem makeOptions(DslLambda<TestMapBuilder> optionsBuilder) {
        return FixtureBuilder.map(optionsBuilder);
    }


    @Test
    @DisplayName("everything should be wired up correctly")
    void everythingIsWiredUpCorrectly() throws XPathException {
        options = makeOptions(opts -> {
            opts.put("header", BooleanValue.TRUE);
        });
        expected = xml.element(builder -> {
            builder.defaultNs("http://www.w3.org/2005/xpath-functions");
            builder.element("csv", csv -> {
                csv.element("columns", cols -> {
                    cols.element("column", el -> el.content("a")).element("column", el -> el.content("b"));
                });
                csv.element("rows", rows -> {
                    rows.element("row", row -> {
                        row.element("field", el -> el.attr("column", "a").content("f1"));
                        row.element("field", el -> el.attr("column", "b").content("f2"));
                    });
                });
            });
        });

        actual = call_subject(args("a,b\nf1,f2", options)).head();

        assertThat(actual, deepEquals(expected));
    }


}