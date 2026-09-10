////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.Controller;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.*;
import net.sf.saxon.om.ZeroOrOne;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringItem;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.net.URI;
import java.util.HashMap;
import java.util.Map;

/**
 * Implementation of fn:unparsed-text() - with one argument or two
 */
public class UnparsedText extends UnparsedTextFunction implements PushableFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public ZeroOrOne<StringItem> call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringItem hrefVal = (StringItem) arguments[0].head();
        String encoding = getArity() == 2 ? arguments[1].head().getStringValue() : null;
        try {
            return new ZeroOrOne<>(evalUnparsedText(hrefVal, getStaticBaseUriString(), encoding, context));
        } catch (XPathException e) {
            if (getArity() == 2 && e.getErrorCodeLocalPart().equals("FOUT1200")) {
                e.setErrorCode("FOUT1190");
            }
            throw e;
        }
    }

    @Override
    public void process(Outputter destination, XPathContext context, Sequence[] arguments) throws XPathException {
        boolean stable = context.getConfiguration().getBooleanProperty(Feature.STABLE_UNPARSED_TEXT);
        if (stable) {
            ZeroOrOne<StringItem> result = call(context, arguments);
            StringItem value = result.head();
            if (value != null) {
                destination.append(value, Loc.NONE, ReceiverOption.NONE);
            }
        } else {
            StringItem href = (StringItem) arguments[0].head();
            URI absoluteURI = getAbsoluteURI(href.getStringValue(), getStaticBaseUriString(), context);
            String encoding = getArity() == 2 ? arguments[1].head().getStringValue() : null;
            UniStringConsumer consumer = destination.getStringReceiver(false);
            consumer.open();
            try {
                readFile(absoluteURI, encoding, consumer, context);
                consumer.close();
            } catch (XPathException e) {
                if (getArity() == 2 && e.getErrorCodeLocalPart().equals("FOUT1200")) {
                    e.setErrorCode("FOUT1190");
                }
                throw e;
            }
        }
    }

    private static final int errorValue = 0;

    /**
     * Evaluation of the unparsed-text function
     *
     * @param hrefVal  the relative URI
     * @param base     the base URI
     * @param encoding the encoding to be used
     * @param context  dynamic evaluation context
     * @return the result of the function
     * @throws XPathException if evaluation fails
     */

    public static StringItem evalUnparsedText(StringItem hrefVal, String base, String encoding, XPathContext context) throws XPathException {
        UnicodeString content;
        StringItem result;
        boolean stable = context.getConfiguration().getBooleanProperty(Feature.STABLE_UNPARSED_TEXT);
        try {
            if (hrefVal == null) {
                return null;
            }
            String href = hrefVal.getStringValue();
            URI absoluteURI = getAbsoluteURI(href, base, context);
            if (stable) {
                final Controller controller = context.getController();
                //noinspection SynchronizationOnLocalVariableOrMethodParameter
                synchronized(controller) {
                    Map<URI, UnicodeString> cache = (Map<URI, UnicodeString>)controller.getUserData("unparsed-text-cache", "");
                    if (cache != null) {
                        UnicodeString existing = cache.get(absoluteURI);
                        if (existing != null) {
                            if (existing.codePointAt(0) == errorValue) {
                                throw new XPathException(existing.substring(1).toString(), "FOUT1170");
                            }
                            return new StringItem(existing);
                        }
                    }
                    XPathException error = null;
                    try {
                        UnicodeBuilder consumer = new UnicodeBuilder();
                        readFile(absoluteURI, encoding, consumer, context);
                        content = consumer.toUnicodeString();
                    } catch (XPathException e) {
                        error = e;
                        content = StringView.tidy(errorValue + e.getMessage());
                    }
                    if (cache == null) {
                        cache = new HashMap<>();
                        controller.setUserData("unparsed-text-cache", "", cache);
                        cache.put(absoluteURI, content);
                    }
                    if (error != null) {
                        throw error;
                    }
                }
            } else {
                UnicodeBuilder consumer = new UnicodeBuilder();
                readFile(absoluteURI, encoding, consumer, context);
                return new StringItem(consumer.toUnicodeString());
            }
            result = new StringItem(content);
        } catch (XPathException err) {
            err.maybeSetErrorCode("FOUT1170");
            throw err;
        }
        return result;
    }

    // diagnostic method to output the octets of a file

    public static void main(String[] args) throws Exception {
        StringBuilder sb1 = new StringBuilder(256);
        StringBuilder sb2 = new StringBuilder(256);
        File file = new File(args[0]);
        InputStream is = new FileInputStream(file);
        while (true) {
            int b = is.read();
            if (b < 0) {
                System.out.println(sb1);
                System.out.println(sb2);
                break;
            }
            sb1.append(Integer.toHexString(b) + " ");
            sb2.append((char) b + " ");
            if (sb1.length() > 80) {
                System.out.println(sb1);
                System.out.println(sb2);
                sb1 = new StringBuilder(256);
                sb2 = new StringBuilder(256);
            }
        }
        is.close();
    }

}
