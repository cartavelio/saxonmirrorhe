package com.saxonica.ee.update;

import com.saxonica.ee.validate.InSituValidator;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.PendingUpdateList;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.SameNameTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;
import net.sf.saxon.z.IntHashMap;

import java.util.*;

/**
 * Concrete implementation of the Pending Update List
 */
public class PendingUpdateListImpl implements PendingUpdateList {

    private final IntHashMap<List<PendingUpdateAction>> lists =
            new IntHashMap<>(PutAction.PHASE + 1);
            // one list per phase of the apply process

    private final Set<MutableNodeInfo> affectedTrees = new HashSet<>();

    private final Set<NodeInfo> renamedNodes = new HashSet<>();
    private final Set<NodeInfo> replacedNodes = new HashSet<>();
    private final Set<NodeInfo> replacedValueNodes = new HashSet<>();

    private final Map<NodeInfo, NamespaceMap> newNamespaceBindings = new HashMap<>();

    private final Set<String> putURIs = new HashSet<>();

    // Actions to insert, delete, rename, or replace attributes are sorted according to the element that they affect.
    // These actions are processed after all other actions.
    private final Map<NodeInfo, List<PendingUpdateAction>> attributeActions = new HashMap<>();

    private final Set<PackageData> packages = new HashSet<>();

    /**
     * Create a Pending Update List
     */

    public PendingUpdateListImpl() {

    }

    /**
     * Add an action to the pending update list
     *
     * @param action the Pending Update Action to be added to the list
     * @throws XPathException if the pending update action conflicts with an action that is already on the list
     */

    public void add(PendingUpdateAction action) throws XPathException {
        int phase = action.getApplyPhase();
        if (lists.get(phase) == null) {
            //noinspection Convert2Diamond
            lists.put(phase, new ArrayList<PendingUpdateAction>(4));
        }

        if (action instanceof RenameAction) {
            boolean added = renamedNodes.add(action.getTargetNode());
            if (!added) {
                throw conflict("rename", "XUDY0015", action.getTargetNode());
            }
            NodeName newName = ((RenameAction) action).getNewName();
            NamespaceBinding nsCode = new NamespaceBinding(newName.getPrefix(), newName.getNamespaceUri());
            NodeInfo targetNode = action.getTargetNode();
            int nodeKind = targetNode.getNodeKind();
            if (nodeKind == Type.ATTRIBUTE) {
                NodeInfo parent = targetNode.getParent();
                addAttributeAction(parent, action);
                targetNode = parent;
            } else if (nodeKind == Type.PROCESSING_INSTRUCTION) {
                targetNode = null;
            }
            checkForNamespaceConflict(targetNode, nodeKind, nsCode, action);
            if (nodeKind != Type.ATTRIBUTE) {
                lists.get(phase).add(action);
            }
        } else if (action instanceof ReplaceNodeAction) {
            boolean added = replacedNodes.add(action.getTargetNode());
            if (!added) {
                throw conflict("replace", "XUDY0016", action.getTargetNode());
            }
            lists.get(phase).add(action);
        } else if (action instanceof ReplaceAttributeAction) {
            NodeInfo element = action.getTargetNode();
            NodeInfo oldAtt = ((ReplaceAttributeAction) action).getOldAttribute();
            boolean added = replacedNodes.add(oldAtt);
            if (!added) {
                throw conflict("replace", "XUDY0016", oldAtt);
            }
            addAttributeAction(element, action);
            SameNameTest test = new SameNameTest(oldAtt);
            AttributeMap newAtts = ((ReplaceAttributeAction) action).getNewAttributes();
            for (AttributeInfo att : newAtts) {
                if (!test.matches(att.getNodeName().getStructuredQName())) {
                    NamespaceBinding nsBinding = new NamespaceBinding(att.getNodeName().getPrefix(), att.getNodeName().getNamespaceUri());
                    checkForNamespaceConflict(element, Type.ATTRIBUTE, nsBinding, action);
                }
            }
        } else if (action instanceof ReplaceValueAction) {
            boolean added = replacedValueNodes.add(action.getTargetNode());
            if (!added) {
                throw conflict("replace the value of", "XUDY0017", action.getTargetNode());
            }
            lists.get(phase).add(action);
        } else if (action instanceof InsertAttributeAction) {
            NodeName name = ((InsertAttributeAction) action).getAttributeName();
            NamespaceBinding nsCode = new NamespaceBinding(name.getPrefix(), name.getNamespaceUri());
            NodeInfo targetNode = action.getTargetNode();
            addAttributeAction(targetNode, action);
            checkForNamespaceConflict(targetNode, Type.ATTRIBUTE, nsCode, action);
        } else if (action instanceof InsertAction) {
            lists.get(phase).add(action);
        } else if (action instanceof DeleteAction) {
            NodeInfo targetNode = action.getTargetNode();
            //deletedNodes.add(targetNode);
            if (targetNode.getNodeKind() == Type.ATTRIBUTE) {
                NodeInfo parent = targetNode.getParent();
                if (parent != null) {
                    addAttributeAction(parent, action);
                }
            } else {
                lists.get(phase).add(action);
            }
        } else if (action instanceof PutAction) {
            lists.get(phase).add(action);
        }
        packages.add(action.getOriginator().getPackageData());
    }

    private void addAttributeAction(NodeInfo element, PendingUpdateAction action) {
        List<PendingUpdateAction> actions =
                attributeActions.computeIfAbsent(element, k -> new ArrayList<PendingUpdateAction>(2));
        actions.add(action);
        packages.add(action.getOriginator().getPackageData());
    }

    /**
     * Add a put() action to the pending update list
     *
     * @param node (the first argument of put())
     * @param uri  (the second argument of put())
     */

    @Override
    public void addPutAction(NodeInfo node, String uri, Expression originator) throws XPathException {
        if (!putURIs.add(uri)) {
            XPathException err1 = new XPathException(
                    "Cannot put() two documents to the same URI (" + uri + ")");
            err1.setErrorCode("XUDY0031");
            err1.setIsTypeError(false);
            err1.setLocation(originator.getLocation());
            throw err1;
        }
        PutAction action = new PutAction(node, uri);
        action.setOriginator(originator);
        add(action);
    }

    /**
     * Check whether an action will cause a namespace conflicy
     *
     * @param targetNode the node being checked
     * @param nodeKind   the kind of node
     * @param nsCode     the namespace code
     * @param action     the action being performed
     * @throws XPathException if a namespace conflict is detected
     */

    private void checkForNamespaceConflict(/*@Nullable*/ NodeInfo targetNode, int nodeKind, NamespaceBinding nsCode, PendingUpdateAction action)
            throws XPathException {
        if (targetNode != null && (nodeKind == Type.ELEMENT || !nsCode.getPrefix().isEmpty())) {
            String prefix = nsCode.getPrefix();
            NamespaceUri uri = nsCode.getNamespaceUri();

            // Check that the namespace prefix is not already declared on the element
            NamespaceMap map = targetNode.getAllNamespaces();
            NamespaceUri existingURI = map.getNamespaceUri(prefix);

            if (existingURI != null && !existingURI.isEmpty() && !existingURI.equals(uri)) {
                XPathException err1 = new XPathException(
                        "An update action creates a namespace binding that conflicts with an existing " +
                                "namespace binding on the same target element");
                err1.setErrorCode("XUDY0023");
                err1.setIsTypeError(false);
                err1.setLocation(action.getOriginator().getLocation());
                throw err1;
            }


            NamespaceMap nsmap = newNamespaceBindings.get(targetNode);
            if (nsmap == null) {
                nsmap = NamespaceMap.of(prefix, uri);
                newNamespaceBindings.put(targetNode, nsmap);
            } else {
                NamespaceUri existingUri = nsmap.getNamespaceUri(prefix);
                if (existingUri == null) {
                    nsmap = nsmap.put(prefix, uri);
                    newNamespaceBindings.put(targetNode, nsmap);
                } else if (!existingUri.equals(uri)) {
                    XPathException err1 = new XPathException(
                            "Two updates create conflicting namespace bindings on the same target node");
                    err1.setErrorCode("XUDY0024");
                    err1.setIsTypeError(false);
                    throw err1;
                }
            }
        }
    }

    /**
     * Construct an XPathException object that reports a namespace conflict
     *
     * @param action    the action being performed, for example "insert" or "delete"
     * @param errorCode the error code to be reported
     * @param node      the element or attribute node triggering the conflict
     * @return the constructed XPathException object
     */

    private XPathException conflict(String action, String errorCode, NodeInfo node) {
        int kind = node.getNodeKind();
        String kindStr = NodeKindTest.describe(kind);
        if (kind == Type.ELEMENT) {
            kindStr = Err.wrap(node.getDisplayName(), Err.ELEMENT) + " " + kindStr;
        } else if (kind == Type.ATTRIBUTE) {
            kindStr = Err.wrap(node.getDisplayName(), Err.ATTRIBUTE) + " " + kindStr;
        }
        String msg = "Update conflict: two attempts to " + action + " the same " + kindStr + " node";

        if (node.getLineNumber() != -1) {
            msg += ". Node at line " + node.getLineNumber() + " of " + node.getSystemId();
        }
        return new XPathException(msg, errorCode);
    }

    /**
     * Make a final check on the total update list. This method performs checks that cannot be made
     * immediately when actions are added to the list, because they might be cancelled by other actions;
     * for example, inserting an attribute that already exists on an element is not an error if the
     * existing attribute (or the element) is subsequently deleted.
     *
     * @throws net.sf.saxon.trans.XPathException
     *          if the final check fings inconsistencies
     */

    private void finalCheck() throws XPathException {
        // Check the attribute actions for each affected element
        for (Map.Entry<NodeInfo, List<PendingUpdateAction>> e : attributeActions.entrySet()) {
            MutableNodeInfo element = (MutableNodeInfo) e.getKey();
            checkAttributeActions(element, e.getValue());
            affectedTrees.add((MutableNodeInfo) element.getRoot());
        }
    }

    /**
     * Apply the pending updates
     *
     * @param context        the XPath dynamic evaluation context
     * @param validationMode the revalidation mode from the static context
     */

    @Override
    public synchronized void apply(XPathContext context, int validationMode) throws XPathException {
        finalCheck();
        // Do everything except the put() actions
        for (int phase = 0; phase < PutAction.PHASE; phase++) {
            Collection<PendingUpdateAction> actionList = lists.get(phase);
            if (actionList != null) {
                for (PendingUpdateAction action : actionList) {
                    action.apply(context, affectedTrees);
                }
            }
        }

        // Now process the attribute actions for each affected element
        for (Map.Entry<NodeInfo, List<PendingUpdateAction>> e : attributeActions.entrySet()) {
            MutableNodeInfo element = (MutableNodeInfo) e.getKey();
            if (!element.isDeleted()) {
                applyAttributeActions(element, e.getValue());
                affectedTrees.add((MutableNodeInfo) element.getRoot());
            }
        }

        // Do the put() actions
        Collection<PendingUpdateAction> list = lists.get(PutAction.PHASE);
        if (list != null) {
            for (PendingUpdateAction action : list) {
                action.apply(context, affectedTrees);
            }
        }

        // invalidate all document-level indexes
        for (MutableNodeInfo root : affectedTrees) {
            if (root instanceof MutableDocumentInfo) {
                for (PackageData packageData : packages) {
                    packageData.getKeyManager().clearDocumentIndexes(root.getTreeInfo());
                }
                ((MutableDocumentInfo) root).resetIndexes();
            }
        }

        // perform revalidation
        if (validationMode == Validation.STRICT || validationMode == Validation.LAX) {
            for (MutableNodeInfo root : affectedTrees) {
                InSituValidator validator = new InSituValidator(root, validationMode);
                validator.validate();
            }
        }
    }

    /**
     * Check consistency of the attribute actions for an element.
     * <p>We have already checked namespace constraints; the check here is that after applying all
     * the deletes, renames, replaces, and inserts, there will not be two attributes with the
     * same name.</p>
     * <p>Everything is checked before any updates are applied, to ensure atomicity: if there are
     * any errors, the data should be unchanged.</p>
     *
     * @param element the element to which the actions apply
     * @param actions the list of actions to be performed
     * @throws net.sf.saxon.trans.XPathException
     *          if there are inconsistencies among the list
     *          of actions
     */

    private void checkAttributeActions(MutableNodeInfo element, List<PendingUpdateAction> actions)
            throws XPathException {

        // Note: the rules have been changed so that nodes in deleted or detached tree fragments
        // are checked. This means we could do most of the checks much earlier.

        Map<NodeName, Integer> names = new HashMap<>(); // map from node names to the number of times
                                                        // they will appear in the result
        // Get the existing attribute names
        AxisIterator attributes = element.iterateAxis(AxisInfo.ATTRIBUTE);
        NodeInfo att;
        while ((att = attributes.next()) != null) {
            names.put(NameOfNode.makeName(att), 1);
        }
        // Adjust for the names that will be created or removed using insert, replace, or rename
        for (PendingUpdateAction action : actions) {
            if (action instanceof DeleteAction) {
                NodeName nameOfNode = NameOfNode.makeName(action.getTargetNode());
                int count = names.getOrDefault(nameOfNode, 0);
                if (count > 0) {
                    names.put(nameOfNode, count - 1);
                }
            } else if (action instanceof RenameAction) {
                NodeName nameOfNode = NameOfNode.makeName(action.getTargetNode());
                int count = names.getOrDefault(nameOfNode, 0);
                if (count > 0) {
                    names.put(nameOfNode, count - 1);
                }

                nameOfNode = ((RenameAction) action).getNewName();
                count = names.getOrDefault(nameOfNode, 0);
                names.put(nameOfNode, count + 1);
            } else if (action instanceof ReplaceAttributeAction) {
                NodeInfo target = ((ReplaceAttributeAction) action).getOldAttribute();
                NodeName nameOfNode = NameOfNode.makeName(target);
                int count = names.getOrDefault(nameOfNode, 0);
                if (count > 0) {
                    names.put(nameOfNode, count - 1);
                }
                AttributeMap newNodes = ((ReplaceAttributeAction) action).getNewAttributes();
                for (AttributeInfo a : newNodes) {
                    nameOfNode = a.getNodeName();
                    count = names.getOrDefault(nameOfNode, 0);
                    names.put(nameOfNode, count + 1);
                }
            } else if (action instanceof InsertAttributeAction) {
                NodeName nameOfNode = ((InsertAttributeAction) action).getAttributeName();
                int count = names.getOrDefault(nameOfNode, 0);
                names.put(nameOfNode, count + 1);
            }
        }
        // Finally, see whether any name will appear more than once
        for (NodeName nn : names.keySet()) {
            int count = names.get(nn);
            if (count > 1) {
                throw new XPathException("After applying all updates, the element at " +
                        Navigator.getPath(element) + " would have " +
                        (count == 2 ? "two" : count) + " attributes named " +
                        nn.getStructuredQName().getEQName(), "XUDY0021");
            }
        }

    }

    /**
     * Apply attribute actions to an element. At this stage sufficient checking has been done
     * to ensure that the updates must succeed.
     *
     * @param element the element affected by the update actions
     * @param actions the actions to be performed
     */

    private void applyAttributeActions(MutableNodeInfo element, List<PendingUpdateAction> actions) {
        // First do any delete actions
        for (PendingUpdateAction action : actions) {
            if (action instanceof DeleteAction) {
                ((MutableNodeInfo) action.getTargetNode()).delete();
            }
        }
        // Delete any attributes that are being replaced
        for (PendingUpdateAction action : actions) {
            if (action instanceof ReplaceAttributeAction) {
                NodeInfo target = ((ReplaceAttributeAction) action).getOldAttribute();
                ((MutableNodeInfo) target).delete();
            }
        }
        // Rename any attributes that are being renamed, using a temporary name in the event of a failure
        Map<MutableNodeInfo, NodeName> pendingRenames = null;
        for (PendingUpdateAction action : actions) {
            if (action instanceof RenameAction) {
                NodeName nc = ((RenameAction) action).getNewName();
                NodeName newName = null;
                if (element.getAttributeValue(nc.getNamespaceUri(), nc.getLocalPart()) != null) {
                    // Name already exists. Need to allocate a temporary name.
                    for (int n = 1; ; n++) {
                        String candidate = nc.getLocalPart() + n;
                        if (element.getAttributeValue(nc.getNamespaceUri(), candidate) == null) {
                            newName = new FingerprintedQName(nc.getPrefix(), nc.getNamespaceUri(), candidate);
                            break;
                        }
                    }
                    if (pendingRenames == null) {
                        pendingRenames = new HashMap<>();
                    }
                    pendingRenames.put((MutableNodeInfo) action.getTargetNode(), nc);
                    nc = newName;
                }
                ((MutableNodeInfo) action.getTargetNode()).rename(nc, ((RenameAction)action).isInheritNamespaces());
            }
        }
        // Apply any pending renames
        if (pendingRenames != null) {
            for (Map.Entry<MutableNodeInfo, NodeName> e : pendingRenames.entrySet()) {
                e.getKey().rename(e.getValue(), true);
            }
        }
        // Create any attributes that result from replacements
        for (PendingUpdateAction action : actions) {
            if (action instanceof ReplaceAttributeAction) {
                AttributeMap content = ((ReplaceAttributeAction) action).getNewAttributes();
                for (AttributeInfo a : content) {
                    element.addAttribute(
                            a.getNodeName(),
                            a.getType(),
                            a.getValue(),
                            ReceiverOption.NONE,
                            ((ReplaceAttributeAction)action).isInheritNamespaces());
                }
            }
        }
        // Create any inserted attributes
        // Create any attributes that result from replacements
        for (PendingUpdateAction action : actions) {
            if (action instanceof InsertAttributeAction) {
                InsertAttributeAction a = (InsertAttributeAction) action;
                element.addAttribute(
                        a.getAttributeName(),
                        a.getNewTypeCode(),
                        a.getNewStringValue(),
                        ReceiverOption.NONE,
                        ((InsertAttributeAction)action).isInheritNamespaces());
            }
        }
        element.removeTypeAnnotation();

    }

    /**
     * Get the root nodes of the trees that are affected by updates in the pending update list
     *
     * @return the root nodes of affected trees, as a Set. Note that this includes all trees that have
     *         been updated; this is a superset of the trees that need to be revalidated, but updates that don't
     *         require revalidation are sufficiently unusual that we don't trouble with the distinction.
     */


    @Override
    public Set<MutableNodeInfo> getAffectedTrees() {
        return affectedTrees;
    }
}

// Copyright (c) 2008-2023 Saxonica Limited
