package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.expr.HofParserExtension;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.Err;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;

public class ExperimentalSequenceTypeMarshaller {

    public static StringValue unmarshal(MapItem type) {
        StringBuilder out = new StringBuilder();

        StringValue indicator = (StringValue)type.get(new StringValue("o"));
        out.append(indicator == null ? "1" : indicator.getStringValue());

        StringValue alphaCode = (StringValue) type.get(new StringValue("p"));
        out.append(alphaCode == null ? "" : alphaCode.getStringValue());

        out.append(" ");

        for (KeyValuePair kvp : type.keyValuePairs()) {
            String key = kvp.key.getStringValue();
            switch (key) {
                case "o":
                case "p":
                    break;
                case "n":
                case "c":
                case "t":
                    out.append(key);
                    out.append(((StringValue)kvp.value).getStringValue());
                    out.append(" ");
                    break;
                case "k":
                case "r":
                case "v":
                case "e":
                    out.append(key);
                    out.append('[');
                    out.append(unmarshal((MapItem)kvp.value).getStringValue());
                    out.append(']');
                    out.append(" ");
                    break;
                case "a":
                case "u":
                    out.append(key);
                    out.append('[');
                    ArrayItem types = (ArrayItem)kvp.value;
                    boolean first = true;
                    for (GroundedValue<?> t : types.members()) {
                        if (first) {
                            first = false;
                        } else {
                            out.append(",");
                        }
                        out.append(unmarshal((MapItem)t).getStringValue());
                    }
                    out.append(']');
                    out.append(" ");
                    break;
                default:
                    throw new IllegalStateException("Unexpected key '" + key + "'");
            }
        }
        return new StringValue(out.toString());
    }

    public static MapItem marshal(StringValue type) {
        String input = type.getStringValue();
        return new ExperimentalSequenceTypeMarshaller(input).parseType();
    }


    private String input;
    private int position = 0;
    private ExperimentalSequenceTypeMarshaller(String input) {
        this.input = input;
    }

    private int next() {
        if (position >= input.length()) {
            return -1;
        }
        return input.charAt(position++);
    }

    private void expect(char c) {
        int d = next();
        if (d != c) {
            throw new IllegalStateException("Expected '" + c + "', found '" + d + "'");
        }
    }

    private int nextU() {
        if (position >= input.length()) {
            return -1;
        }
        char c = input.charAt(position++);
        if (UTF16CharacterSet.isHighSurrogate(c)) {
            char d = input.charAt(position++);
            return UTF16CharacterSet.combinePair(c, d);
        } else {
            return (int)c;
        }
    }

    private String readUntil(char delimiter) {
        StringBuilder sb = new StringBuilder();
        while (position < input.length()) {
            char c = input.charAt(position);
            if (c == delimiter) {
                return sb.toString();
            } else {
                sb.append(c);
                position++;
            }
        }
        return sb.toString();
    }

    private MapItem parseType() {
        DictionaryMap map = new DictionaryMap();
        int indicator = next();
        map.initialPut("o", new StringValue("" + (char)indicator));
        int sp = input.indexOf(' ', position);
        if (sp == -1) {
            sp = input.length();
        }
        map.initialPut("p", new StringValue(input.substring(position, sp)));
        position = sp+1;
        while (position < input.length()) {
            char c = input.charAt(position);
            switch (c) {
                case ']':
                case ',':
                    return map;
                case ' ':
                    position++;
                    break;
                case 'n':
                case 'c':
                case 't':
                    position++;
                    sp = input.indexOf(' ', position);
                    if (sp == -1) {
                        sp = input.length();
                    }
                    map.initialPut(""+c, new StringValue(input.substring(position, sp)));
                    position = sp+1;
                    break;
                case 'k':
                case 'r':
                case 'v':
                case 'e':
                    position++;
                    expect('[');
                    MapItem nestedType = parseType();
                    expect(']');
                    map.initialPut(""+c, nestedType);
                    position++;
                    break;
                case 'a':
                case 'u':
                    position++;
                    expect('[');
                    List<GroundedValue<?>> nestedTypes = new ArrayList<>();
                    if (input.charAt(position) == ']') {
                        position++;
                        map.initialPut("" + c, new SimpleArrayItem(nestedTypes));
                    } else {
                        while (true) {
                            nestedTypes.add(parseType());
                            if (input.charAt(position) == ',') {
                                position++;
                            } else {
                                expect(']');
                                map.initialPut("" + c, new SimpleArrayItem(nestedTypes));
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new IllegalStateException("Expected one of n|c|t|k|r|v|a|u, found '" + c + "'");
            }
        }
        return map;
    }

    public static void main(String[] args) throws Exception {
        Configuration config = new EnterpriseConfiguration();
        IndependentContext ic = new IndependentContext(config);
        ic.declareNamespace("foo", "http://foo.com/");
        XPathParser parser = new XPathParser();
        parser.setParserExtension(new HofParserExtension());
        parser.setLanguage(XPathParser.XPATH, 31);
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

