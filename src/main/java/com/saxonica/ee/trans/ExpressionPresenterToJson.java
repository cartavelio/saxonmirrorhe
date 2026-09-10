package com.saxonica.ee.trans;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Sink;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.StringValue;

/**
 * This class handles export of packages for Saxon-JS 2.0 by converting the XML representation
 * of SEF files to a JSON representation, at the level of emitting events.
 */

public class ExpressionPresenterToJson extends ProxyReceiver {

    private JSONEmitter out;
    private boolean pending = false;
    private int level = 0;

    public ExpressionPresenterToJson(JSONEmitter out, PipelineConfiguration pipe) {
        super(new Sink(pipe));
        this.out = out;
    }

    /**
     * Notify the start of an element
     *
     * @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location
     * @param properties for future use. Should be set to zero.
     */
    @Override
    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (pending) {
            out.writeKey("C");
            out.startArray(false);
        }
        out.startMap(false);
        out.writeKey("N");
        out.writeAtomicValue(new StringValue(nameCode.getLocalPart()));
        pending = true;
        level++;
    }

    /**
     * Notify an attribute. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param nameCode   The name of the attribute
     * @param typeCode   The type of the attribute
     * @param value
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dd>DISABLE_ESCAPING</dd>    <dt>Disable escaping for this attribute</dt>
     *                   <dd>NO_SPECIAL_CHARACTERS</dd>      <dt>Attribute value contains no special characters</dt>
     *                   </dl>
     * @throws IllegalStateException: attempt to output an attribute when there is no open element
     */
    @Override
    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
        out.writeKey(nameCode.getLocalPart());
        out.writeAtomicValue(new StringValue(value));
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */
    @Override
    public void startContent() throws XPathException {

    }

    /**
     * End of element
     */
    @Override
    public void endElement() throws XPathException {
        if (!pending) {
            out.endArray();
        }
        if (--level > 0) {
            // at outermost level, leave room for the checksum
            out.endMap();
        }
        pending = false;
    }

    @Override
    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) throws XPathException {
        // Used to output the checksum
        out.writeKey(target);
        out.writeAtomicValue(new StringValue(data));
    }

    /**
     * Notify the end of a document node
     */
    @Override
    public void endDocument() throws XPathException {
        out.endMap();
    }

    /**
     * End of output. Note that closing this receiver also closes the rest of the
     * pipeline.
     */
    @Override
    public void close() throws XPathException {
        out.close();
    }
}

// Copyright (c) 2018 Saxonica Limited.

