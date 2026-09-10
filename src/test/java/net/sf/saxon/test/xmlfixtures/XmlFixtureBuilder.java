package net.sf.saxon.test.xmlfixtures;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;

public class XmlFixtureBuilder {
    public static Item document(Configuration configuration, DslLambda definition) throws XPathException {
        return new XmlFixtureBuilder(configuration).document(definition);
    }

    public static Item element(Configuration configuration, DslLambda definition) throws XPathException {
        return new XmlFixtureBuilder(configuration).element(definition);
    }

    public Item document(DslLambda definition) throws XPathException {
        return new BuilderDsl(controller, staticBaseUri).doc(definition);
    }

    public Item element(DslLambda definition) throws XPathException {
        return new BuilderDsl(controller, staticBaseUri).root(definition);
    }

    private final Configuration configuration;
    private final String staticBaseUri;
    private final Controller controller;

    public XmlFixtureBuilder(Configuration configuration) {
        this.configuration = configuration;
        this.staticBaseUri = "http://www.example.org/";
        this.controller = new Controller(configuration);
    }

    public class BuilderDsl {
        private final Outputter out;
        private final Builder builder;
        private NamespaceUri defaultNsUri;

        BuilderDsl(Controller controller, String staticBaseUri) throws XPathException {
            builder = controller.makeBuilder();
            builder.setSystemId(staticBaseUri);
            builder.setTiming(false);
            builder.setDurability(Durability.TEMPORARY);
            out = new ComplexContentOutputter(builder);
        }

        private Item doc(DslLambda definition) throws XPathException {
            out.open();
            out.startDocument(ReceiverOption.NONE);
            definition.buildWith(this);
            out.endDocument();
            out.close();
            return builder.getCurrentRoot();
        }

        private Item root(DslLambda definition) throws XPathException {
            out.open();
            definition.buildWith(this);
            out.close();
            return builder.getCurrentRoot();
        }

        public BuilderDsl defaultNs(String uri) {
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

        public BuilderDsl element(String name) throws XPathException {
            element(name, el -> {});
            return this;
        }

        public BuilderDsl element(String name, DslLambda definition) throws XPathException {
            NodeName qn = makeElementName(name);
            out.startContent();
            out.startElement(qn, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            definition.buildWith(this);
            out.endElement();
            return this;
        }

        public BuilderDsl element(String name, String content) throws XPathException {
            element(name, el -> el.content(content));
            return this;
        }

        public BuilderDsl attr(String name, String value) throws XPathException {
            NodeName qn = FingerprintedQName.fromEQName(name);
            out.attribute(qn, BuiltInAtomicType.STRING, value, Loc.NONE, ReceiverOption.NONE);
            return this;
        }

        public BuilderDsl content(String value) throws XPathException {
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
}
