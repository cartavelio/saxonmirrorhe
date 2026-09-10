package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SimpleType;

import java.util.Set;

/**
 * A pending update action representing the insert attribute action.
 */
public class InsertAttributeAction extends PendingUpdateAction {

    private final MutableNodeInfo element;
    private final NodeName name;
    private final SimpleType typeCode;
    private final String value;
    private final int properties;
    private final boolean inherit;

    /**
     * Create a InsertAttributeAction
     *
     * @param element    the element into which it is to be inserted
     * @param name       the name of the new attribute
     * @param typeCode   the type annotation of the new attribute
     * @param value      the string value of the new attribute
     * @param properties properties including IS_ID and IS_IDREF properties
     * @param inherit    true if any new namespace bindings are to be inherited by descendant elements
     */

    InsertAttributeAction(MutableNodeInfo element, NodeName name,
                          SimpleType typeCode, String value, int properties, boolean inherit) {
        this.element = element;
        this.name = fixNodeName(name);
        this.typeCode = typeCode;
        this.value = value;
        this.properties = properties;
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
        if (!element.getTreeInfo().isMutable()) {
            throw new XPathException("Attempting to update a non-updatable element", SaxonErrorCode.SXUP0081);
        }
        if (element.getAttributeValue(name.getNamespaceUri(), name.getLocalPart()) != null) {
            throw new XPathException("Attempt to insert an attribute " +
                    name.getDisplayName() +
                    " that is already present on the target element", "XUDY0021");
        }
        affectedRootSet.add((MutableNodeInfo)element.getRoot());
        element.addAttribute(name, typeCode, value, properties, inherit);
        element.removeTypeAnnotation();
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
     * Get the name of the attribute to be inserted
     *
     * @return the attribute name
     */

    public NodeName getAttributeName() {
        return name;
    }

    /**
     * Get the type code of the inserted attribute
     *
     * @return the type annotation as an integer namecode of a type
     */

    public SimpleType getNewTypeCode() {
        return typeCode;
    }

    /**
     * Get the string value of the inserted attribute
     *
     * @return the string value of the new attribute
     */

    public String getNewStringValue() {
        return value;
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
        return 2;
    }
}

// Copyright (c) 2008-2023 Saxonica Limited
