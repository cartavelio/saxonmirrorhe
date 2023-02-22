////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.registry;

import net.sf.saxon.functions.*;
import net.sf.saxon.functions.hof.LoadXqueryModule;
import net.sf.saxon.functions.hof.RandomNumberGenerator;
import net.sf.saxon.functions.hof.Sort_3;
import net.sf.saxon.ma.json.JsonDoc;
import net.sf.saxon.ma.json.JsonToXMLFn;
import net.sf.saxon.ma.json.ParseJsonFn;
import net.sf.saxon.ma.json.XMLToJsonFn;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XPath 3.1 without the
 * Higher-Order-Functions feature
 */

public class XPath31FunctionSet extends BuiltInFunctionSet {

    private static final XPath31FunctionSet THE_INSTANCE = new XPath31FunctionSet();

    public static XPath31FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XPath31FunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath20FunctionSet.getInstance());
        importFunctionSet(XPath30FunctionSet.getInstance());

        SpecificFunctionType ft;

        register("collation-key", 1, () -> new CollationKeyFn(), BuiltInAtomicType.BASE64_BINARY,
                 OPT, DCOLL)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("collation-key", 2, () -> new CollatingFunctionFree(), BuiltInAtomicType.BASE64_BINARY,
                 OPT, DCOLL)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("contains-token", 2, () -> new ContainsToken(), BuiltInAtomicType.BOOLEAN, ONE, DCOLL)
                .arg(0, BuiltInAtomicType.STRING, STAR, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("contains-token", 3, () -> new CollatingFunctionFree(), BuiltInAtomicType.BOOLEAN, ONE, BASE)
                .arg(0, BuiltInAtomicType.STRING, STAR, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null);

        // The copy-of function is defined in XSLT 3.0, but we choose to make it available also in XPath/XQuery

        register("copy-of", 0, () -> new CopyOfFn(), AnyItemType.getInstance(),
                 STAR, NEW);

        register("copy-of", 1, () -> new CopyOfFn(), AnyItemType.getInstance(),
                 STAR, NEW)
                .arg(0, AnyItemType.getInstance(), STAR | ABS, EMPTY);

        register("default-language", 0, () -> new DynamicContextAccessor.DefaultLanguage(), BuiltInAtomicType.LANGUAGE, ONE, DLANG);

        register("generate-id", 0, () -> new ContextItemAccessorFunction(), BuiltInAtomicType.STRING, ONE, CITEM | LATE);

        register("generate-id", 1, () -> new GenerateId_1(), BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, Type.NODE_TYPE, OPT | INS, StringValue.EMPTY_STRING);

        register("has-children", 0, () -> new ContextItemAccessorFunction(), BuiltInAtomicType.BOOLEAN,
                 ONE, CITEM | LATE);

        register("has-children", 1, () -> new HasChildren_1(), BuiltInAtomicType.BOOLEAN,
                 OPT, 0)
                .arg(0, AnyNodeTest.getInstance(), OPT | INS, null);

        register("head", 1, () -> new HeadFn(), AnyItemType.getInstance(),
                 OPT, FILTER)
                .arg(0, AnyItemType.getInstance(), STAR | TRA, null);

        register("innermost", 1, () -> new Innermost(), AnyNodeTest.getInstance(),
                 STAR, 0)
                .arg(0, AnyNodeTest.getInstance(), STAR | NAV, null);

        register("json-doc", 1, () -> new JsonDoc(), AnyItemType.getInstance(),
                 OPT, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null);

        register("json-doc", 2, () -> new JsonDoc(), AnyItemType.getInstance(),
                 OPT, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null)
                .setOptionDetails(ParseJsonFn.OPTION_DETAILS);

        register("json-to-xml", 1, () -> new JsonToXMLFn(), AnyItemType.getInstance(),
                 OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null);

        register("json-to-xml", 2, () -> new JsonToXMLFn(), AnyItemType.getInstance(),
                 OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, null)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null)
                .setOptionDetails(JsonToXMLFn.OPTION_DETAILS);

        register("load-xquery-module", 1, () -> new LoadXqueryModule(), MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null);

        register("load-xquery-module", 2, () -> new LoadXqueryModule(), MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null) // null or EMPTY?
                .arg(1, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .setOptionDetails(LoadXqueryModule.makeOptionsParameter());


        register("parse-ietf-date", 1, () -> new ParseIetfDate(), BuiltInAtomicType.DATE_TIME, OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse-json", 1, () -> new ParseJsonFn(), AnyItemType.getInstance(), OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("parse-json", 2, () -> new ParseJsonFn(), AnyItemType.getInstance(), OPT, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE, null)
                .setOptionDetails(ParseJsonFn.OPTION_DETAILS);

        register("parse-xml", 1, () -> new ParseXml(), NodeKindTest.DOCUMENT, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("random-number-generator", 0, () -> new RandomNumberGenerator(), RandomNumberGenerator.RETURN_TYPE, ONE, LATE);

        register("random-number-generator", 1, () -> new RandomNumberGenerator(), RandomNumberGenerator.RETURN_TYPE, ONE, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, OPT, null);

        register("parse-xml-fragment", 1, () -> new ParseXmlFragment(), NodeKindTest.DOCUMENT, OPT, LATE | NEW)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("serialize", 2, () -> new Serialize(), BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, Type.ITEM_TYPE, OPT, null)
                .setOptionDetails(Serialize.makeOptionsParameter());

        // The snapshot function is defined in XSLT 3.0, but we choose to make it available also in XPath/XQuery

        register("snapshot", 0, () -> new ContextItemAccessorFunction(), AnyItemType.getInstance(), STAR, CITEM | LATE | NEW);

        register("snapshot", 1, () -> new SnapshotFn(), AnyNodeTest.getInstance(),
                 STAR, NEW)
                .arg(0, AnyItemType.getInstance(), STAR | ABS, EMPTY);

        register("sort", 1, () -> new Sort_1(), AnyItemType.getInstance(), STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null);

        register("sort", 2, () -> new Sort_2(), AnyItemType.getInstance(),
                 STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null);

        ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.ATOMIC_SEQUENCE);

        register("sort", 3, () -> new Sort_3(), AnyItemType.getInstance(),
                 STAR, 0)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null);

        register("string-join", 1, () -> new StringJoin(), BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, StringValue.EMPTY_STRING);

        register("string-join", 2, () -> new StringJoin(), BuiltInAtomicType.STRING, ONE, 0)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, STAR, StringValue.EMPTY_STRING)
                .arg(1, BuiltInAtomicType.STRING, ONE, null);

        register("tokenize", 1, () -> new Tokenize_1(), BuiltInAtomicType.STRING, STAR, 0)
                .arg(0, BuiltInAtomicType.STRING, OPT, EMPTY);

        register("trace", 1, () -> new Trace(), Type.ITEM_TYPE, STAR, AS_ARG0 | LATE)
                .arg(0, Type.ITEM_TYPE, STAR | TRA, null);

        register("transform", 1, () -> new TransformFn(), MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, EMPTY)
                .setOptionDetails(TransformFn.makeOptionsParameter());

        register("xml-to-json", 1, () -> new XMLToJsonFn(), BuiltInAtomicType.STRING,
                 OPT, LATE)
                .arg(0, AnyNodeTest.getInstance(), OPT | ABS, EMPTY);

        register("xml-to-json", 2, () -> new XMLToJsonFn(), BuiltInAtomicType.STRING,
                 OPT, LATE)
                .arg(0, AnyNodeTest.getInstance(), OPT | ABS, EMPTY)
                .arg(1, MapType.ANY_MAP_TYPE, ONE | ABS, null)
                .setOptionDetails(XMLToJsonFn.makeOptionsParameter());

    }

}
