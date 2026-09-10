package com.saxonica.functions.extfn;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.StandardErrorHandler;
import net.sf.saxon.lib.StandardErrorReporter;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.StringValue;
import nu.validator.htmlparser.common.XmlViolationPolicy;
import nu.validator.htmlparser.sax.HtmlParser;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;
import java.util.Stack;

/**
 * Supports the saxon:parse-html() which parses HTML using nu.validator
 */
public class ParseHtml extends Parse {

    @Override
    public NodeInfo call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringValue html = (StringValue)arguments[0].head();
        Builder builder = context.getController().makeBuilder();
        ProxyReceiver filter = new Html5Converter(builder);
        HtmlParser parser = new HtmlParser(XmlViolationPolicy.ALTER_INFOSET);
        parser.setReportingDoctype(false);
        parser.setErrorHandler(new HTML5ErrorHandler());
        InputSource is = new InputSource(new StringReader(html.getStringValue()));
        SAXSource ss = new SAXSource(parser, is);
        Sender.send(ss, filter, new ParseOptions());
        return builder.getCurrentRoot();
    }

    public static class HTML5ErrorHandler extends StandardErrorHandler {

        public HTML5ErrorHandler() {
            super(new StandardErrorReporter());
        }

        @Override
        public void error(SAXParseException e) throws SAXException {
            // Ignore non-fatal errors. These are generally "DTD-validation" style errors, and validator-nu
            // recovers from them if no action is taken.
        }
    }

    /**
     * This class "tweaks" the output of nu.validator to deliver what the parse-html() function expects.
     */

    public static class Html5Converter extends ProxyReceiver {

        private final Stack<NamespaceUri> nsStack = new Stack<>();

        public Html5Converter(Receiver next) {
            super(next);
            nsStack.push(NamespaceUri.XHTML);
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            NamespaceMap nsMap = namespaces;
            if (elemName.getLocalPart().equals("svg")) {
                nsStack.push(NamespaceUri.SVG);
            } else if (elemName.getLocalPart().equals("math")) {
                nsStack.push(NamespaceUri.MATHML);
            } else {
                nsStack.push(nsStack.peek());
            }
            nsMap = nsMap.put("", nsStack.peek());
            NodeName nn = new FingerprintedQName("", nsStack.peek(), elemName.getLocalPart().toLowerCase());
            if (attributes.size() != 0) {
                AttributeMap atts2 = EmptyAttributeMap.getInstance();
                for (AttributeInfo info : attributes) {
                    NodeName attName2 = new FingerprintedQName("",
                                                               NamespaceUri.NULL,
                                                               lowerCaseAttributeName(info.getNodeName().getLocalPart()));
                    AttributeInfo info2 = info.withNodeName(attName2);
                    atts2 = atts2.put(info2);
                }
                attributes = atts2;
            }
            super.startElement(nn, type, attributes, nsMap, location, properties);
        }

        private String lowerCaseAttributeName(String in) {
            // nu.validator doesn't convert SVG attribute names to lower-case, which it should. But we can't simply
            // call toLowerCase(), because the name may contains Uxxxxxx escapes that should remain upper-case.
            if (in.equals(in.toLowerCase())) {
                return in;
            } else {
                StringBuilder sb = new StringBuilder();
                for (int i=0; i<in.length(); i++) {
                    char ch = in.charAt(i);
                    if (Character.isLowerCase(ch)) {
                        sb.append(ch);
                    } else if (in.substring(i).matches("U[0-9a-fA-f]{6}")) {
                        sb.append(in.substring(i, i+7));
                        i+=6;
                    } else {
                        sb.append(Character.toLowerCase(ch));
                    }
                }
                return sb.toString();
            }

        }

        @Override
        public void endElement() throws XPathException {
            nsStack.pop();
            super.endElement();
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited
