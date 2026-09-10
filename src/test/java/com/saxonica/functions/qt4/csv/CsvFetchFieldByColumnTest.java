package com.saxonica.functions.qt4.csv;

import com.saxonica.functions.registry.XPath40FunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.IsDeepEqualTo.deepEquals;

class CsvFetchFieldByColumnTest {
    MapItem columns = MapBuilder.empty();
    Sequence fields = EmptySequence.getInstance();
    Configuration configuration = new Configuration();

    public SystemFunction subject() throws XPathException {
        return XPath40FunctionSet.getInstance().makeFunction("csv-fetch-field-by-column", 3);
    }

    public XPathContext ctxt() {
        return new EarlyEvaluationContext(configuration);
    }

    public Sequence[] args(MapItem columns, Sequence fields, String key) {
        return args(columns, fields, FixtureBuilder.sequence(b -> b.append(key)));
    }

    public Sequence[] args(MapItem columns, Sequence fields, int key) {
        return args(columns, fields, FixtureBuilder.sequence(b -> b.append(key)));
    }

    public Sequence[] args(MapItem columns, Sequence fields, Sequence key) {
        Sequence[] args = SequenceTool.makeSequenceArray(3);
        args[0] = columns;
        args[1] = fields;
        args[2] = key;
        return args;
    }

    @Nested
    class GoodColumnsRecord {
        MapItem columns = FixtureBuilder.map(m -> {
            m.put("names", m.map(names -> {
                names.put("a", 1).put("c", 3);
            }));
            m.put("fields", m.sequence("f1", "f2", "f3"));
        });

        Sequence fields = FixtureBuilder.sequence(seq -> seq.append("f1", "f2", "f3"));

        @Test
        @DisplayName("can fetch field by name")
        void fetchFieldByName() throws XPathException {
            assertThat(subject().call(ctxt(), args(columns, fields, "a")).materialize(), deepEquals(new StringValue("f1")));
        }

        @Test
        @DisplayName("can fetch field by index")
        void fetchFieldByIndex() throws XPathException {
            assertThat(subject().call(ctxt(), args(columns, fields, 3)).materialize(), deepEquals(new StringValue("f3")));
        }
    }
}