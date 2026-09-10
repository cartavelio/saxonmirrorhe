package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

/**
 * This class is a filter that passes all Receiver events through unchanged,
 * except that it removes all type annotations from element and attribute nodes.
 * It implements validation="strip" or in schema terms, skip validation
 *
 * @author Michael Kay
 */


public class SkipValidator extends ContentValidator {

    boolean stripIdProperty = false;        // indicates whether the is-id and is-idref properties should be stripped
    TypeHierarchy typeHierarchy;

    public SkipValidator(Receiver next) {
        super(next);
    }

    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        stripIdProperty = pipe.getHostLanguage() == Configuration.XQUERY;
        typeHierarchy = pipe.getConfiguration().getTypeHierarchy();
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    /*@Nullable*/
    public SchemaType getSchemaType() {
        return null;
    }

    @Override
    protected ConstraintChecker getConstraintChecker() {
        return null;
    }

    /**
     * Output element start tag
     */

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (!stripIdProperty && typeCode != Untyped.getInstance() &&
                ((properties & ReceiverOptions.ID_IDREF_CHECKED) == 0)) {
            if (typeCode.isIdType()) {
                properties |= ReceiverOptions.IS_ID;
            } else if (typeCode.isIdRefType()) {
                properties |= ReceiverOptions.IS_IDREF;
            }
            properties |= ReceiverOptions.ID_IDREF_CHECKED;
        }
        properties &= ~ReceiverOptions.NILLED_ELEMENT;
        super.startElement(nameCode, Untyped.getInstance(), location, properties);
    }

    /**
     * Get a validator to handle the children of the current element. This
     * is called immediately on return from the startElement call that starts
     * this element.
     */

    protected ContentValidator getChildValidator() {
        return this;
        // use the same validator at each level
    }

    /**
     * Handle an attribute
     *
     * @param nameCode   integer identifying the name of the attribute
     * @param type   integer identifying the type annotation of the
     *                   attribute
     * @param value      the value of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     *@param properties bit-significant flags indicating special action to
     *                   be taken  @throws XPathException On any failure to write the attribute
     */

    public void attribute(NodeName nameCode, SimpleType type, CharSequence value, Location locationId, int properties)
            throws XPathException {

        if (!stripIdProperty && ((properties & ReceiverOptions.ID_IDREF_CHECKED) == 0)) {
            if (type.isIdType()) {
                properties |= ReceiverOptions.IS_ID;
            } else if (type.isIdRefType()) {
                properties |= ReceiverOptions.IS_IDREF;
            }
            properties |= ReceiverOptions.ID_IDREF_CHECKED;
        }
        if (stripIdProperty) {
            properties = properties & ~(ReceiverOptions.IS_ID | ReceiverOptions.IS_IDREF);
            properties |= ReceiverOptions.ID_IDREF_CHECKED;
        }
        nextReceiver.attribute(nameCode, BuiltInAtomicType.UNTYPED_ATOMIC, value, locationId, properties);
    }

    @Override
    public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
        if (item instanceof NodeInfo) {
            decompose(item, locationId, copyNamespaces);
        } else {
            nextReceiver.append(item, locationId, copyNamespaces);
        }
    }
}

// Copyright � 2004-2006 Saxonica Limited. No warranty.
