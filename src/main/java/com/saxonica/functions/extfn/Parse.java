package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.Doc_2;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.IgnorableSpaceStrippingRule;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.value.AtomicValue;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Map;


/**
 * This class implements the saxon:parse() extension function,
 * which is specially-recognized by the system because it needs access
 * to parts of the static context
 */

public class Parse extends SystemFunction {


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
    public NodeInfo call(XPathContext context, Sequence[] arguments) throws XPathException {
        Controller controller = context.getController();
        Configuration config = controller.getConfiguration();
        AtomicValue content = (AtomicValue) arguments[0].head();
        StringReader sr = new StringReader(content.getStringValue());
        Source source = new StreamSource(sr, getStaticBaseUriString());
        Map<String, Sequence> checkedOptions;
        ParseOptions parseOptions;
        if (arguments.length == 1) {
            checkedOptions = new HashMap<>();
            parseOptions = new ParseOptions();
        } else {
            checkedOptions =
                    getDetails().optionDetails.processSuppliedOptions((MapItem) arguments[1].head(), context);
            parseOptions = Doc_2.setParseOptions(getRetainedStaticContext(), checkedOptions, context);

        }
        Sequence suppliedBaseUriVal = checkedOptions.get("base-uri");
        if (suppliedBaseUriVal == null || suppliedBaseUriVal.head() == null) {
            source.setSystemId(getStaticBaseUriString());
        } else {
            source.setSystemId(suppliedBaseUriVal.head().getStringValue());
        }
        Builder b = controller.makeBuilder();
        if (b instanceof TinyBuilder) {
            ((TinyBuilder) b).setStatistics(config.getTreeStatistics().FN_PARSE_STATISTICS);
        }
        Receiver s = b;
        PackageData pd = getRetainedStaticContext().getPackageData();
        if (checkedOptions.get("strip-space") == null) {
            if (pd instanceof StylesheetPackage) {
                parseOptions.setSpaceStrippingRule(((StylesheetPackage) pd).getSpaceStrippingRule());
                if (((StylesheetPackage) pd).isStripsTypeAnnotations()) {
                    s = config.getAnnotationStripper(s);
                }
            } else {
                parseOptions.setSpaceStrippingRule(IgnorableSpaceStrippingRule.getInstance());
            }
        }
        parseOptions.setErrorReporter(context.getController().getErrorReporter());
        setXMLReader(config, parseOptions);
        s.setPipelineConfiguration(b.getPipelineConfiguration());
        Sender.send(source, s, parseOptions);
        NodeInfo node = b.getCurrentRoot();
        b.reset();
        return node;
    }

    /**
     * Set the XML parser to be used, if different from the default XML parser
     *
     * @param config the Saxon configuration
     * @param source the ParseOptions object to be updated with the selected parser
     * @throws XPathException if the selected parser cannot be loaded
     */

    protected void setXMLReader(Configuration config, ParseOptions source) throws XPathException {
        // no-op; overridden in subclass
    }


}

// Copyright (c) 2009-2022 Saxonica Limited
