package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.EncodeForUri;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.resource.Loader;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.EmptySequence;

import java.io.IOException;
import java.net.*;

/**
 * Extension function to determined the last-modified date of a file
 */

public class LastModified extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return an iterator over the results of the function. If the result is a single item, it can be
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item item = arguments[0].head();
        if (item == null) {
            return EmptySequence.getInstance();
        }
        String suppliedURI = item.getStringValue();
        URI absoluteURI;
        try {
            absoluteURI = ResolveURI.makeAbsolute(suppliedURI, getStaticBaseUriString());
        } catch (URISyntaxException e) {
            XPathException err = new XPathException("Invalid URI passed to saxon:last-modified(). Base='"
                                                            + getStaticBaseUriString() + " Relative='" + suppliedURI, e);
            err.setErrorCode("FODC0005");
            throw err;
        }
        // The URL dereferencing classes throw all kinds of strange exceptions if given
        // ill-formed sequences of %hh escape characters. So we do a sanity check that the
        // escaping is well-formed according to UTF-8 rules

        EncodeForUri.checkPercentEncoding(absoluteURI.toString());

        URL absoluteURL;
        try {
            absoluteURL = absoluteURI.toURL();
        } catch (MalformedURLException e) {
            XPathException err = new XPathException("Invalid URL passed to saxon:last-modified(). Base='"
                                                            + getStaticBaseUriString() + " Relative='" + suppliedURI, e);
            err.setErrorCode("FODC0005");
            throw err;
        }
        long lastMod;
        try {
            URLConnection connection = Loader.urlConnection(absoluteURL);
            lastMod = connection.getLastModified();
        } catch (IOException e) {
            XPathException err = new XPathException("Cannot read from URI supplied to saxon:last-modified(). Base='"
                                                            + getStaticBaseUriString() + " Relative='" + suppliedURI, e);
            err.setErrorCode("FODC0002");
            throw err;
        }

        if (lastMod == 0) {
            return EmptySequence.getInstance();
        }
        return DateTimeValue.fromJavaTime(lastMod);
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
