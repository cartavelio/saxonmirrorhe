package com.saxonica.ee.validate;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

/**
 * This class is a filter that passes all Receiver events through unchanged,
 * except that it removes all type annotations from element and attribute nodes.
 * It implements validation="strip" or in schema terms, skip validation
 *
 */


public class SkipValidator extends ContentValidator {

    private boolean stripIdProperty = false;        // indicates whether the is-id and is-idref properties should be stripped

    public SkipValidator(Receiver next) {
        super(next);
        setPipelineConfiguration(next.getPipelineConfiguration()); // see bug #5153
    }

    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        stripIdProperty = pipe.getHostLanguage() == HostLanguage.XQUERY;
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    /*@Nullable*/
    @Override
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

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        int p2 = properties;
        if (!stripIdProperty && type != Untyped.getInstance() &&
                (!ReceiverOption.contains(properties, ReceiverOption.ID_IDREF_CHECKED))) {
            if (type.isIdType()) {
                p2 |= (ReceiverOption.IS_ID);
            } else if (type.isIdRefType()) {
                p2 |= (ReceiverOption.IS_IDREF);
            }
            p2 |= (ReceiverOption.ID_IDREF_CHECKED);
        }
        p2 &= ~ReceiverOption.NILLED_ELEMENT;

        AttributeMap atts2;
        try {
            atts2 = attributes.apply(att -> {
                //try {
                    int prop = att.getProperties();
                    if (!stripIdProperty && (!ReceiverOption.contains(properties, ReceiverOption.ID_IDREF_CHECKED))) {
                        SimpleType attType = att.getType();
                        if (attType.isIdType()) {
                            prop |= (ReceiverOption.IS_ID);
                        } else if (attType.isIdRefType()) {
                            prop |= (ReceiverOption.IS_IDREF);
                        }
                        prop |= (ReceiverOption.ID_IDREF_CHECKED);
                    }
                    if (stripIdProperty) {
                        prop &= ~(ReceiverOption.IS_ID | ReceiverOption.IS_IDREF);
                        prop |= ReceiverOption.ID_IDREF_CHECKED;
                    }
                    return new AttributeInfo(
                            att.getNodeName(), BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), prop);
//                } catch (MissingComponentException e) {
//                    throw new UncheckedXPathException(e);
//                }
            });
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
        //AttributeMap atts2 = attributes;
        //int p2 = properties;
        getNextReceiver().startElement(elemName, Untyped.getInstance(), atts2, namespaces, location, p2);
    }

    /**
     * Get a validator to handle the children of the current element. This
     * is called immediately on return from the startElement call that starts
     * this element.
     */

    @Override
    protected ContentValidator getChildValidator() {
        return this;
        // use the same validator at each level
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

// Copyright (c) 2004-2023 Saxonica Limited
