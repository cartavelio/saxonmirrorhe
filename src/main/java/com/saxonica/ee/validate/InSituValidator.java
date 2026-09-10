package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaType;
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
    private final MutableNodeInfo[] nodeBuffer = new MutableNodeInfo[16];
    private final MutableNodeInfo root;
    private final int validationMode;
    private final Schema schema;

    /**
     * Create an InSituValidator
     *
     * @param root           the root node of the tree to be validated. This must be a document or element node,
     *                       and it must be an updatable node
     * @param schema         the schema to be used for validation
     * @param validationMode the validation mode: {@link net.sf.saxon.lib.Validation#STRICT} or {@link net.sf.saxon.lib.Validation#LAX}
     */

    public InSituValidator(MutableNodeInfo root, Schema schema, int validationMode) {
        this.root = root;
        this.schema = schema;
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
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        SequenceReceiver endPoint = new NodeUpdater(nodeBuffer, pipe);
        // Create the validation pipeline
        ParseOptions options = new ParseOptions()
                .withSchemaValidationMode(validationMode)
                .withSpaceStrippingRule(null);
        Receiver validator;
        if (root.getNodeKind() == Type.DOCUMENT) {
            validator = schema.getDocumentValidator(endPoint, root.getSystemId(), options, null);
        } else {
            options = options.withTopLevelElement(NameOfNode.makeName(root).getStructuredQName());
            validator = schema.getElementValidator(endPoint, options, Loc.NONE);
        }
        NodeCounter nodeCounter = new NodeCounter(nodeBuffer);
        pipe.setCopyInformee(CSharp.methodRef(nodeCounter::notifyElementNode));
        // Run the validation by invoking the copy() method to send events to the validation pipeline
        root.copy(validator, CopyOptions.ALL_NAMESPACES, Loc.NONE);
    }

    /**
     * This class sits at the front of the validation pipeline. Because it is a CopyInformee, it receives
     * notification of nodes from the copy() method as events are fired into the pipeline, and it remembers
     * the mapping of nodes to locations so that events emerging at the far end of the pipeline can be
     * used to trigger updates to the originating nodes.
     */

    private static class NodeCounter implements CopyInformee {

        int counter = 0;
        final MutableNodeInfo[] nodeBuffer;

        public NodeCounter(MutableNodeInfo[] nodeBuffer) {
            this.nodeBuffer = nodeBuffer;
        }

        /**
         * Provide information about the node being copied. This method is called immediately before
         * the startElement call for the element node in question.
         *
         * @param element the node being copied, which must be an element node
         */

        @Override
        public Location notifyElementNode(NodeInfo element) {
            nodeBuffer[counter % 16] = (MutableNodeInfo) element;
            return new Loc(element.getSystemId(), counter++, 998877);
        }

    }

    private static class NodeUpdater extends ProxyReceiver {

        private MutableNodeInfo currentElement;
        final MutableNodeInfo[] nodeBuffer;

        public NodeUpdater(MutableNodeInfo[] nodeBuffer, PipelineConfiguration pipe) {
            super(new Sink(pipe));
            this.nodeBuffer = nodeBuffer;
        }


        /**
         * Notify the start of an element
         *
         */

        @Override
        public void startElement(NodeName elemName, SchemaType type,
                                 AttributeMap attributes, NamespaceMap namespaces,
                                 Location location, int properties) throws XPathException {
            MutableNodeInfo node = nodeBuffer[location.getLineNumber() % 16];
            node.setTypeAnnotation(type);
            currentElement = node;
            node.setAttributes(attributes);
        }

        @Override
        public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
            if (ReceiverOption.contains(properties, ReceiverOption.DEFAULTED_VALUE)) {
                Orphan child = new Orphan(getConfiguration());
                child.setNodeKind(Type.TEXT);
                child.setStringValue(chars.tidy());
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

        @Override
        public boolean usesTypeAnnotations() {
            return true;
        }
    }

}

// Copyright (c) 2008-2023 Saxonica Limited

