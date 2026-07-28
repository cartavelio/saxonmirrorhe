package net.sf.saxon.test.xmlfixtures;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;

public class XmlFixtureBuilder {
    private final Outputter out;
    private final Builder builder;
    private NamespaceUri defaultNsUri;

    public static Item document(DslLambda definition) throws XPathException {
        Configuration configuration = new Configuration();
        Controller controller = new Controller(configuration);
        String staticBaseUri = "http://www.example.org/";
        return document(definition, controller, staticBaseUri);
    }

    public static Item document(DslLambda definition, Controller controller, String staticBaseUri) throws XPathException {
        XmlFixtureBuilder builder = new XmlFixtureBuilder(controller, staticBaseUri);
        builder.doc(definition);
        return builder.getResult();
    }

    XmlFixtureBuilder(Controller controller, String staticBaseUri) throws XPathException {
        builder = controller.makeBuilder();
        builder.setSystemId(staticBaseUri);
        builder.setTiming(false);
        builder.setDurability(Durability.TEMPORARY);
        out = new ComplexContentOutputter(builder);
    }

    private XmlFixtureBuilder doc(DslLambda definition) throws XPathException {
        out.open();
        out.startDocument(ReceiverOption.NONE);
        definition.buildWith(this);
        return this;
    }

    public XmlFixtureBuilder defaultNs(String uri) {
        defaultNsUri = NamespaceUri.of(uri);
        return this;
    }

    private NodeName makeElementName(String name) {
        NodeName qn = FingerprintedQName.fromEQName(name);
        if (qn.getNamespaceUri() == NamespaceUri.NULL && defaultNsUri != null) {
            return new FingerprintedQName(null, defaultNsUri, name);
        }
        return qn;
    }

    public XmlFixtureBuilder element(String name) throws XPathException {
        element(name, el -> {});
        return this;
    }

    public XmlFixtureBuilder element(String name, DslLambda definition) throws XPathException {
        NodeName qn = makeElementName(name);
        out.startContent();
        out.startElement(qn, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
        definition.buildWith(this);
        out.endElement();
        return this;
    }

    public XmlFixtureBuilder element(String name, String content) throws XPathException {
        element(name, el -> el.content(content));
        return this;
    }

    public XmlFixtureBuilder attr(String name, String value) throws XPathException {
        NodeName qn = FingerprintedQName.fromEQName(name);
        out.attribute(qn, BuiltInAtomicType.STRING, value, Loc.NONE, ReceiverOption.NONE);
        return this;
    }

    public XmlFixtureBuilder content(String value) throws XPathException {
        out.startContent();
        out.characters(StringView.of(value), Loc.NONE, ReceiverOption.NONE);
        return this;
    }

    public Item getResult() throws XPathException {
        out.endDocument();
        out.close();
        return builder.getCurrentRoot();
    }
}
