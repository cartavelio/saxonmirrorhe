package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;

/**
 * This class performs in-situ validation of a tree represented in memory as a NodeInfo
 * object. The standard copy() method is used to copy the tree to a validation pipeline
 * as a sequence of events. The locationId on these events is used to maintain a mapping
 * back to the original nodes in the tree, so that the original tree can be updated with
 * type annotations and expanded defaults reflecting the decisions of the validator.
 * <p>Note that if validation fails, the tree will not be in a usable state. For this reason
 * it may be desirable to do the validation twice, once with a Sink as the destination, and only
 * use the in-situ validation once it is known that it will succeed.</p>
 */
public class InSituValidator {

    // This cyclic buffer is designed to allow for some latency in the validation pipeline.
    // In fact it seems not to be needed: element N emerges from the pipeline before element N+1 enters
    private MutableNodeInfo[] nodeBuffer = new MutableNodeInfo[16];
    private MutableNodeInfo root;
    private int validationMode;

    /**
     * Create an InSituValidator
     *
     * @param root           the root node of the tree to be validated. This must be a document or element node,
     *                       and it must be an updateable node
     * @param validationMode the validation mode: {@link net.sf.saxon.lib.Validation#STRICT} or {@link net.sf.saxon.lib.Validation#LAX}
     */

    public InSituValidator(MutableNodeInfo root, int validationMode) {
        this.root = root;
        this.validationMode = validationMode;
    }

    /**
     * Perform the validation
     *
     * @throws XPathException if it is not possible to construct a validator, or if validation fails.
     */

    public void validate() throws XPathException {
        Configuration config = root.getConfiguration();
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setHostLanguage(Configuration.XML_SCHEMA);
        SequenceReceiver endPoint = new NodeUpdater(pipe);
        // Create the validation pipeline
        ParseOptions options = new ParseOptions();
        options.setSchemaValidationMode(validationMode);
        options.setSpaceStrippingRule(null);
        Receiver validator;
        if (root.getNodeKind() == Type.DOCUMENT) {
            validator = config.getDocumentValidator(endPoint, root.getSystemId(), options, null);
        } else {
            options.setTopLevelElement(NameOfNode.makeName(root).getStructuredQName());
            validator = config.getElementValidator(endPoint, options, ExplicitLocation.UNKNOWN_LOCATION);
        }
        pipe.setComponent(CopyInformee.class.getName(), new NodeCounter(validator));
        // Run the validation by invoking the copy() method to send events to the validation pipeline
        root.copy(validator, CopyOptions.ALL_NAMESPACES, ExplicitLocation.UNKNOWN_LOCATION);
    }

    /**
     * This class sits at the front of the validation pipeline. Because it is a CopyInformee, it receives
     * notification of nodes from the copy() method as events are fired into the pipeline, and it remembers
     * the mapping of nodes to locations so that events emerging at the far end of the pipeline can be
     * used to trigger updates to the originating nodes.
     */

    private class NodeCounter implements CopyInformee<Location> {

        int counter = 0;

        public NodeCounter(Receiver next) {
            //setUnderlyingReceiver(next);
        }

        /**
         * Provide information about the node being copied. This method is called immediately before
         * the startElement call for the element node in question.
         *
         * @param element the node being copied, which must be an element node
         */

        public Location notifyElementNode(NodeInfo element) {
            nodeBuffer[counter % 16] = (MutableNodeInfo) element;
            return new ExplicitLocation(element.getSystemId(), counter++, 998877);
        }

    }

    private class NodeUpdater extends ProxyReceiver {

        private MutableNodeInfo currentElement;

        public NodeUpdater(PipelineConfiguration pipe) {
            super(new Sink(pipe));
        }


        /**
         * Notify the start of an element
         * @param nameCode   the name of the element
         * @param type   the element's type annotation
         * @param location location of the originator of the element
         * @param properties properties of the element node
         */

        public void startElement(NodeName nameCode, SchemaType type, Location location, int properties) throws XPathException {
            MutableNodeInfo node = nodeBuffer[location.getLineNumber() % 16];
            node.setTypeAnnotation(type);
            currentElement = node;
        }

        /**
         * Notify an attribute. Attributes are notified after the startElement event, and before any
         * children. Namespaces and attributes may be intermingled.
         *
         * @param name       The name of the attribute
         * @param type   The type of the attribute
         * @param locationId the location of the node in the source, or of the instruction that created it
         * @param properties Bit significant value. The following bits are defined:
         *                   <dl>
         *                   <dd>DISABLE_ESCAPING</dd>    <dt>Disable escaping for this attribute</dt>
         *                   <dd>NO_SPECIAL_CHARACTERS</dd>      <dt>Attribute value contains no special characters</dt>
         *                   </dl>
         * @throws IllegalStateException: attempt to output an attribute when there is no open element
         *                                start tag
         */

        public void attribute(NodeName name, SimpleType type, CharSequence value, Location locationId, int properties) throws XPathException {
            if ((properties & ReceiverOptions.DEFAULTED_ATTRIBUTE) != 0) {
                currentElement.addAttribute(name, type, value, 0);
            } else {
                AxisIterator axis = currentElement.iterateAxis(AxisInfo.ATTRIBUTE);
                NodeInfo att;
                while ((att = axis.next()) != null) {
                    if (NameOfNode.makeName(att).equals(name)) {
                        ((MutableNodeInfo) att).setTypeAnnotation(type);
                        axis.close();
                        break;
                    }
                }
            }
        }

        @Override
        public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
            if ((properties & ReceiverOptions.DEFAULTED_ATTRIBUTE) != 0) {
                Orphan child = new Orphan(getConfiguration());
                child.setNodeKind(Type.TEXT);
                child.setStringValue(chars.toString());
                currentElement.insertChildren(new NodeInfo[]{child}, true, true);
            }
            super.characters(chars, locationId, properties);
        }

        /**
         * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
         * supplied on element and attribute events
         *
         * @return true if the Receiver makes any use of this information. If false, the caller
         *         may supply untyped nodes instead of supplying the type annotation
         */

        public boolean usesTypeAnnotations() {
            return true;
        }
    }

}

// Copyright (c) 2008-2018 Saxonica Limited.

