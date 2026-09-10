package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SimpleType;

import java.util.Set;

/**
 * A pending update action representing a replace node expression applied to an attribute. Note that
 * the replacement attribute may be absent; this is not quite the same as a delete operation, because
 * the consistency rules on the pending update list are different.
 */
public class ReplaceAttributeAction extends PendingUpdateAction {

    private final NodeInfo element;
    private final NodeInfo oldAttribute;
    private final AttributeMap newAttributes;
    private final boolean inherit;

    /**
     * Create a ReplaceAttributeAction
     *
     * @param element       the element into which it is to be inserted
     * @param oldAttribute  the name of the old attribute to be replaced
     * @param newAttributes the attributes that are to replace the target attribute
     * @param inherit       true if any namespaces added as a consequence of replacing the attribute
     *                      are to be inherited
     */

    ReplaceAttributeAction(NodeInfo element, NodeInfo oldAttribute, AttributeMap newAttributes, boolean inherit) {
        this.element = element;
        this.oldAttribute = oldAttribute;
        this.newAttributes = newAttributes;
        this.inherit = inherit;
    }

    public boolean isInheritNamespaces() {
        return inherit;
    }

    /**
     * Apply the pending update action to the affected nodes
     *
     * @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     *                        this action should add to as necessary
     * @throws net.sf.saxon.trans.XPathException
     *          if any error occurs applying the update
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) throws XPathException {
        if (!(element instanceof MutableNodeInfo)) {
            throw new XPathException("Attempting to update a non-updateable element");
        }
        affectedRootSet.add((MutableNodeInfo)element.getRoot());
//        FingerprintedQName name = new FingerprintedQName(
//                oldAttribute.getPrefix(), oldAttribute.getURI(), oldAttribute.getLocalPart());
        ((MutableNodeInfo) element).removeAttribute(oldAttribute);
        int len = newAttributes.size();
        for (AttributeInfo att : newAttributes) {
            NodeName name = att.getNodeName();
            SimpleType typeCode = att.getType();
            String value = att.getValue();
            int properties = att.getProperties();
            if (element.getAttributeValue(name.getURI(), name.getLocalPart()) != null) {
                throw new XPathException("Attempting to add an attribute that already exists");
            }
            ((MutableNodeInfo) element).addAttribute(name, typeCode, value, properties, inherit);
        }
        ((MutableNodeInfo) element).removeTypeAnnotation();
    }

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies. Returns null in the
     *         case of a delete action, which affects multiple nodes.
     */

    @Override
    public NodeInfo getTargetNode() {
        return element;
    }

    /**
     * Get the replaced attribute node
     *
     * @return the attribute being replaced
     */

    public NodeInfo getOldAttribute() {
        return oldAttribute;
    }

    /**
     * Get the attribute collection containing the new (replacement) attributes
     *
     * @return the new attributes to be added in place of the old attribute
     */

    public AttributeMap getNewAttributes() {
        return newAttributes;
    }

    /**
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification
     */

    @Override
    public int getApplyPhase() {
        return 3;
    }
}

// Copyright (c) 2008-2022 Saxonica Limited