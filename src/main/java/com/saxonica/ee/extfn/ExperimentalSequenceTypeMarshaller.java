package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.expr.SaxonSyntaxExtension;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.AlphaCode;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

public class ExperimentalSequenceTypeMarshaller {

    public static StringValue unmarshal(MapItem type) {
        return new StringValue(AlphaCode.fromXdmMap(type));
    }

    public static MapItem marshal(StringValue type) {
        String input = type.getStringValue();
        return AlphaCode.toXdmMap(input);
    }

    public static void main(String[] args) throws Exception {
        Configuration config = new EnterpriseConfiguration();
        IndependentContext ic = new IndependentContext(config);
        ic.declareNamespace("foo", "http://foo.com/");
        XPathParser parser = new XPathParser();
        parser.setParserExtension(new SaxonSyntaxExtension());
        parser.setLanguage(XPathParser.ParsedLanguage.XPATH, 31);
        String[] testCases = new String[] {
                "item()",
                "item()*",
                "xs:anyAtomicType?",
                "xs:string*",
                "xs:integer+",
                "node()",
                "element(foo)",
                "element(foo:bar)",
                "xs:numeric*",
                "xs:dayTimeDuration",
                "map(*)",
                "map(xs:string, node()*)",
                "array(*)",
                "array(array(xs:integer*))",
                "empty-sequence()",
                "function(*)",
                "function(xs:int, xs:int) as xs:date",
                "function(xs:int) as (function() as xs:date*)*",
                "document-node(element(foo, xs:integer))?"
        };
        for (String s : testCases) {
            SequenceType t = parser.parseSequenceType(s, ic);
            MapItem map = ExperimentalXPathCompiler.makeMap(t);
            String json = Err.depict(map).toString();
            String lex = unmarshal(map).getStringValue();
            MapItem map2 = marshal(new StringValue(lex));
            String json2 = Err.depict(map2).toString();
            String lex2 = unmarshal(map2).getStringValue();
            System.out.println(s + " => " + json + " => " + lex + " => " + json2 + " => " + lex2);
        }

    }
}

// Copyright (c) 2020-2022 Saxonica Limited

