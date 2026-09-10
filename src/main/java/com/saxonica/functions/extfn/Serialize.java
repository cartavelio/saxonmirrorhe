package com.saxonica.functions.extfn;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ResultDocument;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.om.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.stream.StreamResult;
import java.io.StringWriter;
import java.util.Properties;


/**
 * This class implements the saxon:serialize() extension function,
 * which is specially-recognized by the system because it needs access
 * to parts of the static context
 */

public class Serialize extends SystemFunction {

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
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        NodeInfo node = (NodeInfo) arguments[0].head();
        if (node == null) {
            return StringValue.EMPTY_STRING;
        }
        Properties props = null;
        SerializationProperties outputProps;
        Item secondArg = arguments[1].head();
        if (secondArg instanceof StringValue) {
            String lexicalName = secondArg.getStringValue();
            PackageData pack = getRetainedStaticContext().getPackageData();
            if (pack instanceof StylesheetPackage) {
                StructuredQName qName;
                try {
                    qName = StructuredQName.fromLexicalQName(lexicalName, false,
                                                             true, getRetainedStaticContext());
                } catch (XPathException e) {
                    throw new XPathException("Invalid output format name " + lexicalName + ". " + e.getMessage());
                }
                props = ((StylesheetPackage) pack).getNamedOutputProperties(qName);
                if (props == null) {
                    throw new XPathException("Unknown output format " + lexicalName);
                }
                outputProps = new SerializationProperties(props);
            } else {
                outputProps = new SerializationProperties();
                outputProps.setProperty("method", lexicalName);
            }
        } else {
            if (secondArg instanceof NodeInfo &&
                    ((NodeInfo) secondArg).getNodeKind() == Type.ELEMENT &&
                    ((NodeInfo) secondArg).getNamespaceUri().equals(NamespaceUri.XSLT) &&
                    ((NodeInfo) secondArg).getLocalPart().equals("output")) {
                props = new Properties();
                ResultDocument.processXslOutputElement((NodeInfo) secondArg, props, context);
                outputProps = new SerializationProperties(props);
            } else {
                throw new XPathException("The second argument of saxon:serialize must either be " +
                                                 "a string literal, or an xsl:output element");
            }
        }

        StringWriter result = new StringWriter();
        SerializerFactory sf = context.getConfiguration().getSerializerFactory();
        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setXPathContext(context);
        Receiver out = sf.getReceiver(new StreamResult(result), outputProps, pipe);
        out.open();
        node.copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, Loc.NONE);
        out.close();
        return new StringValue(result.toString());
    }


}

// Copyright (c) 2009-2022 Saxonica Limited
