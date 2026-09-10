package com.saxonica.functions.extfn;

import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.TreeReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.*;

import javax.xml.transform.TransformerException;


/**
 * This class implements the {@code saxon:transform()} extension function (not to be confused with
 * the standard {@code fn:transform()} function in XPath 3.1)
 */

public class TransformFn extends SystemFunction {


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
     * @return the results of the function.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        try {
            AnyExternalObject templatesObject = (AnyExternalObject) arguments[0].head();
            if (templatesObject == null) {
                return EmptySequence.getInstance(); // Should not happen
            }
            PreparedStylesheet pss = (PreparedStylesheet) templatesObject.getWrappedObject();

            NodeInfo source = (NodeInfo) arguments[1].head();
            if (source == null) {
                return EmptySequence.getInstance(); // Should not happen
            }
            XsltController transformer = pss.newController();
            GlobalParameterSet params = new GlobalParameterSet();

            if (arguments.length == 3) {
                SequenceIterator iter = arguments[2].iterate();
                Item param;
                while ((param = iter.next()) != null) {
                    if (param instanceof NodeInfo) {
                        switch (((NodeInfo) param).getNodeKind()) {
                            case Type.ELEMENT:
                            case Type.ATTRIBUTE:
                                setTransformerParameter(param, params);
                                break;
                            case Type.DOCUMENT:
                                for (NodeInfo kid : ((NodeInfo)param).children(NodeKindTest.ELEMENT)) {
                                    setTransformerParameter(kid, params);
                                }
                                break;
                            default:
                                throw new XPathException(
                                        "Parameters passed to saxon:transform() must be element, attribute, or document nodes");
                        }
                    } else if (param instanceof MapItem) {
                        for (KeyValuePair pair : ((MapItem) param).keyValuePairs()) {
                            AtomicValue key = pair.key;
                            GroundedValue val = ((Sequence) pair.value).materialize();
                            if (key instanceof StringValue) {
                                params.put(new StructuredQName("", NamespaceUri.NULL, key.getStringValue()), val);
                            } else if (key instanceof QNameValue) {
                                params.put(((QNameValue) key).getStructuredQName(), val);
                            }
                        }

                    } else {
                        throw new XPathException("Parameters passed to saxon:transform() must be nodes");
                    }
                }
            }
            Builder builder = context.getController().makeBuilder();
            if (builder instanceof TinyBuilder) {
                ((TinyBuilder) builder).setStatistics(context.getConfiguration().getTreeStatistics().RESULT_TREE_STATISTICS);
            }
            transformer.setApplyFunctionConversionRulesToExternalVariables(true);
            transformer.initializeController(params);
            transformer.applyTemplates(source, new TreeReceiver(builder));
            NodeInfo result = builder.getCurrentRoot();
            if (result == null) {
                return EmptySequence.getInstance();
            }
            return result;
        } catch (TransformerException e) {
            throw XPathException.makeXPathException(e);
        }
    }

    private static void setTransformerParameter(Item param, GlobalParameterSet params) throws XPathException {
        NodeName name = NameOfNode.makeName((NodeInfo) param);
        AtomicSequence val = param.atomize();
        params.put(name.getStructuredQName(), val);
    }


}

// Copyright (c) 2009-2022 Saxonica Limited
