// Copyright (c) 2017. Saxonica Limited

package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;


/**
 * This class implements the saxon:parse-XPath() extension function,
 * which is specially-recognized by the system because it needs access
 * to parts of the static context
 */

public class ParseXPath extends SystemFunction {


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
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Controller controller = context.getController();
        Configuration configuration = controller.getConfiguration();
        AtomicValue content = (AtomicValue) arguments[0].head();
        throw new XPathException("saxon:parse-XPath() is only supported in SaxonJS", SaxonErrorCode.SXJS0001);
        // Just null in Java at present
        //return ZeroOrOne.empty();

        /*StringReader sr = new StringReader(content.getStringValue());
        InputSource is = new InputSource(sr);
        is.setSystemId(getStaticBaseUriString());
        Source source = new SAXSource(is);
        source.setSystemId(getStaticBaseUriString());
        Builder b = controller.makeBuilder();
        if (b instanceof TinyBuilder) {
            ((TinyBuilder) b).setStatistics(Statistics.FN_PARSE_STATISTICS);
        }
        Receiver s = b;
        ParseOptions options = new ParseOptions();
        options.setStripSpace(Whitespace.XSLT);
        options.setErrorReporter(context.getConfiguration().getErrorReporter());
        setXMLReader(configuration, options);
        if (controller.getExecutable().stripsInputTypeAnnotations()) {
            s = configuration.getAnnotationStripper(s);
        }
        s.setPipelineConfiguration(b.getPipelineConfiguration());
        Sender.send(source, s, options);
        NodeInfo node = b.getCurrentRoot();
        b.reset();
        return node;*/
    }



}

// Copyright (c) 2020-2023 Saxonica Limited
