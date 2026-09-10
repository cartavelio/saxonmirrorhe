using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class PendingUpdateListImpl : Saxon.Hej.expr.PendingUpdateList {
        private readonly Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>> lists = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>>((Saxon.Eej.ee.update.PutAction.PHASE+1));
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedTrees = new System.Collections.Generic.HashSet<Saxon.Hej.om.MutableNodeInfo>();
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> renamedNodes = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> replacedNodes = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> replacedValueNodes = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeInfo,Saxon.Hej.om.NamespaceMap> newNamespaceBindings = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeInfo, Saxon.Hej.om.NamespaceMap>();
        private readonly System.Collections.Generic.ISet<string> putURIs = new System.Collections.Generic.HashSet<string>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeInfo,System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>> attributeActions = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeInfo, System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>>();
        private readonly System.Collections.Generic.ISet<Saxon.Hej.expr.PackageData> packages = new System.Collections.Generic.HashSet<Saxon.Hej.expr.PackageData>();
        public PendingUpdateListImpl()  {
        }
        public void add(Saxon.Eej.ee.update.PendingUpdateAction action) {
            int phase = action.getApplyPhase();
            if ((lists.get(phase) == null)) {
                lists.put(phase, new System.Collections.Generic.List<Saxon.Eej.ee.update.PendingUpdateAction>(4));
            }
            if ((action is Saxon.Eej.ee.update.RenameAction)) {
                bool added = renamedNodes.Add(action.getTargetNode());
                if (!(added)) {
                    throw conflict("rename", "XUDY0015", action.getTargetNode());
                }
                Saxon.Hej.om.NodeName newName = ((Saxon.Eej.ee.update.RenameAction)(action)).getNewName();
                Saxon.Hej.om.NamespaceBinding nsCode = new Saxon.Hej.om.NamespaceBinding(newName.getPrefix(), newName.getURI());
                Saxon.Hej.om.NodeInfo targetNode = action.getTargetNode();
                int nodeKind = targetNode.getNodeKind();
                if ((nodeKind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    Saxon.Hej.om.NodeInfo parent = targetNode.getParent();
                    addAttributeAction(parent, action);
                    targetNode = parent;
                } else {
                    if ((nodeKind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                        targetNode = null;
                    }
                }
                checkForNamespaceConflict(targetNode, nodeKind, nsCode, action);
                if ((nodeKind != Saxon.Hej.type.Type.ATTRIBUTE)) {
                    lists.get(phase).Add(action);
                }
            } else {
                if ((action is Saxon.Eej.ee.update.ReplaceNodeAction)) {
                    bool added = replacedNodes.Add(action.getTargetNode());
                    if (!(added)) {
                        throw conflict("replace", "XUDY0016", action.getTargetNode());
                    }
                    lists.get(phase).Add(action);
                } else {
                    if ((action is Saxon.Eej.ee.update.ReplaceAttributeAction)) {
                        Saxon.Hej.om.NodeInfo element = action.getTargetNode();
                        Saxon.Hej.om.NodeInfo oldAtt = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getOldAttribute();
                        bool added = replacedNodes.Add(oldAtt);
                        if (!(added)) {
                            throw conflict("replace", "XUDY0016", oldAtt);
                        }
                        addAttributeAction(element, action);
                        Saxon.Hej.pattern.SameNameTest test = new Saxon.Hej.pattern.SameNameTest(oldAtt);
                        Saxon.Hej.om.AttributeMap newAtts = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getNewAttributes();
                        foreach (Saxon.Hej.om.AttributeInfo att in newAtts) {
                            if (!(test.matches(att.getNodeName().getStructuredQName()))) {
                                Saxon.Hej.om.NamespaceBinding nsBinding = new Saxon.Hej.om.NamespaceBinding(att.getNodeName().getPrefix(), att.getNodeName().getURI());
                                checkForNamespaceConflict(element, Saxon.Hej.type.Type.ATTRIBUTE, nsBinding, action);
                            }
                        }
                    } else {
                        if ((action is Saxon.Eej.ee.update.ReplaceValueAction)) {
                            bool added = replacedValueNodes.Add(action.getTargetNode());
                            if (!(added)) {
                                throw conflict("replace the value of", "XUDY0017", action.getTargetNode());
                            }
                            lists.get(phase).Add(action);
                        } else {
                            if ((action is Saxon.Eej.ee.update.InsertAttributeAction)) {
                                Saxon.Hej.om.NodeName name = ((Saxon.Eej.ee.update.InsertAttributeAction)(action)).getAttributeName();
                                Saxon.Hej.om.NamespaceBinding nsCode = new Saxon.Hej.om.NamespaceBinding(name.getPrefix(), name.getURI());
                                Saxon.Hej.om.NodeInfo targetNode = action.getTargetNode();
                                addAttributeAction(targetNode, action);
                                checkForNamespaceConflict(targetNode, Saxon.Hej.type.Type.ATTRIBUTE, nsCode, action);
                            } else {
                                if ((action is Saxon.Eej.ee.update.InsertAction)) {
                                    lists.get(phase).Add(action);
                                } else {
                                    if ((action is Saxon.Eej.ee.update.DeleteAction)) {
                                        Saxon.Hej.om.NodeInfo targetNode = action.getTargetNode();
                                        if ((targetNode.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                                            Saxon.Hej.om.NodeInfo parent = targetNode.getParent();
                                            if ((parent != null)) {
                                                addAttributeAction(parent, action);
                                            }
                                        } else {
                                            lists.get(phase).Add(action);
                                        }
                                    } else {
                                        if ((action is Saxon.Eej.ee.update.PutAction)) {
                                            lists.get(phase).Add(action);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            packages.Add(action.getOriginator().getPackageData());
        }
        private void addAttributeAction(Saxon.Hej.om.NodeInfo element, Saxon.Eej.ee.update.PendingUpdateAction action) {
            System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction> actions = Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(attributeActions, element, k => new System.Collections.Generic.List<Saxon.Eej.ee.update.PendingUpdateAction>(2));
            actions.Add(action);
            packages.Add(action.getOriginator().getPackageData());
        }
        public void addPutAction(Saxon.Hej.om.NodeInfo node, string uri, Saxon.Hej.expr.Expression originator) {
            if (!(putURIs.Add(uri))) {
                Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException((("Cannot put() two documents to the same URI ("+uri)+")"));
                err1.setErrorCode("XUDY0031");
                err1.setIsTypeError(false);
                err1.setLocation(originator.getLocation());
                throw err1;
            }
            Saxon.Eej.ee.update.PutAction action = new Saxon.Eej.ee.update.PutAction(node, uri);
            action.setOriginator(originator);
            add(action);
        }
        private void checkForNamespaceConflict(Saxon.Hej.om.NodeInfo targetNode, int nodeKind, Saxon.Hej.om.NamespaceBinding nsCode, Saxon.Eej.ee.update.PendingUpdateAction action) {
            if (((targetNode != null) && (((nodeKind == Saxon.Hej.type.Type.ELEMENT) || !(nsCode.getPrefix().Length==0))))) {
                string prefix = nsCode.getPrefix();
                string uri = nsCode.getURI();
                Saxon.Hej.om.NamespaceMap map = targetNode.getAllNamespaces();
                string existingURI = map.getURI(prefix);
                if ((((existingURI != null) && !(existingURI.Length==0)) && !(existingURI.Equals(uri)))) {
                    Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException(("An update action creates a namespace binding that conflicts with an existing "+"namespace binding on the same target element"));
                    err1.setErrorCode("XUDY0023");
                    err1.setIsTypeError(false);
                    err1.setLocation(action.getOriginator().getLocation());
                    throw err1;
                }
                Saxon.Hej.om.NamespaceMap nsmap = Saxon.Impl.Helpers.MapUtils.Get(newNamespaceBindings, targetNode);
                if ((nsmap == null)) {
                    nsmap = Saxon.Hej.om.NamespaceMap.of(prefix, uri);
                    Saxon.Impl.Helpers.MapUtils.Put(newNamespaceBindings, targetNode, nsmap);
                } else {
                    string existingUri = nsmap.getURI(prefix);
                    if ((existingUri == null)) {
                        nsmap = nsmap.put(prefix, uri);
                        Saxon.Impl.Helpers.MapUtils.Put(newNamespaceBindings, targetNode, nsmap);
                    } else {
                        if (!(existingUri.Equals(uri))) {
                            Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException("Two updates create conflicting namespace bindings on the same target node");
                            err1.setErrorCode("XUDY0024");
                            err1.setIsTypeError(false);
                            throw err1;
                        }
                    }
                }
            }
        }
        private Saxon.Hej.trans.XPathException conflict(string action, string errorCode, Saxon.Hej.om.NodeInfo node) {
            int kind = node.getNodeKind();
            string kindStr = Saxon.Hej.pattern.NodeKindTest.describe(kind);
            if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                kindStr = ((Saxon.Hej.trans.Err.wrap(node.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)+" ")+kindStr);
            } else {
                if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    kindStr = ((Saxon.Hej.trans.Err.wrap(node.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)+" ")+kindStr);
                }
            }
            string msg = (((("Update conflict: two attempts to "+action)+" the same ")+kindStr)+" node");
            if ((node.getLineNumber() != -1)) {
                msg += (((". Node at line "+node.getLineNumber())+" of ")+node.getSystemId());
            }
            return new Saxon.Hej.trans.XPathException(msg, errorCode);
        }
        private void finalCheck() {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.NodeInfo,System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>> e in attributeActions) {
                Saxon.Hej.om.MutableNodeInfo element = (Saxon.Hej.om.MutableNodeInfo)(e.Key);
                checkAttributeActions(element, e.Value);
                affectedTrees.Add((Saxon.Hej.om.MutableNodeInfo)(element.getRoot()));
            }
        }
        public void apply(Saxon.Hej.expr.XPathContext context, int validationMode) { lock(this) {
            finalCheck();
            for (int phase = 0; (phase<Saxon.Eej.ee.update.PutAction.PHASE); phase++) {
                System.Collections.Generic.ICollection<Saxon.Eej.ee.update.PendingUpdateAction> actionList = lists.get(phase);
                if ((actionList != null)) {
                    foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actionList) {
                        action.apply(context, affectedTrees);
                    }
                }
            }
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.NodeInfo,System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction>> e in attributeActions) {
                Saxon.Hej.om.MutableNodeInfo element = (Saxon.Hej.om.MutableNodeInfo)(e.Key);
                if (!(element.isDeleted())) {
                    applyAttributeActions(element, e.Value);
                    affectedTrees.Add((Saxon.Hej.om.MutableNodeInfo)(element.getRoot()));
                }
            }
            System.Collections.Generic.ICollection<Saxon.Eej.ee.update.PendingUpdateAction> list = lists.get(Saxon.Eej.ee.update.PutAction.PHASE);
            if ((list != null)) {
                foreach (Saxon.Eej.ee.update.PendingUpdateAction action in list) {
                    action.apply(context, affectedTrees);
                }
            }
            foreach (Saxon.Hej.om.MutableNodeInfo root in affectedTrees) {
                if ((root is Saxon.Hej.om.MutableDocumentInfo)) {
                    foreach (Saxon.Hej.expr.PackageData packageData in packages) {
                        packageData.getKeyManager().clearDocumentIndexes(root.getTreeInfo());
                    }
                    ((Saxon.Hej.om.MutableDocumentInfo)(root)).resetIndexes();
                }
            }
            if (((validationMode == Saxon.Hej.lib.Validation.STRICT) || (validationMode == Saxon.Hej.lib.Validation.LAX))) {
                foreach (Saxon.Hej.om.MutableNodeInfo root in affectedTrees) {
                    Saxon.Eej.ee.validate.InSituValidator validator = new Saxon.Eej.ee.validate.InSituValidator(root, validationMode);
                    validator.validate();
                }
            }
        }}
        private void checkAttributeActions(Saxon.Hej.om.MutableNodeInfo element, System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction> actions) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeName,int> names = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, int>();
            Saxon.Hej.tree.iter.AxisIterator attributes = element.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
            Saxon.Hej.om.NodeInfo att;
            while (((att = ((/*1430*/Saxon.Hej.om.NodeInfo)attributes.next())) != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(names, Saxon.Hej.om.NameOfNode.makeName(att), 1);
            }
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.DeleteAction)) {
                    Saxon.Hej.om.NodeName nameOfNode = Saxon.Hej.om.NameOfNode.makeName(action.getTargetNode());
                    int count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                    if ((count>0)) {
                        Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count-1));
                    }
                } else {
                    if ((action is Saxon.Eej.ee.update.RenameAction)) {
                        Saxon.Hej.om.NodeName nameOfNode = Saxon.Hej.om.NameOfNode.makeName(action.getTargetNode());
                        int count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                        if ((count>0)) {
                            Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count-1));
                        }
                        nameOfNode = ((Saxon.Eej.ee.update.RenameAction)(action)).getNewName();
                        count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                        Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count+1));
                    } else {
                        if ((action is Saxon.Eej.ee.update.ReplaceAttributeAction)) {
                            Saxon.Hej.om.NodeInfo target = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getOldAttribute();
                            Saxon.Hej.om.NodeName nameOfNode = Saxon.Hej.om.NameOfNode.makeName(target);
                            int count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                            if ((count>0)) {
                                Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count-1));
                            }
                            Saxon.Hej.om.AttributeMap newNodes = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getNewAttributes();
                            foreach (Saxon.Hej.om.AttributeInfo a in newNodes) {
                                nameOfNode = a.getNodeName();
                                count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                                Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count+1));
                            }
                        } else {
                            if ((action is Saxon.Eej.ee.update.InsertAttributeAction)) {
                                Saxon.Hej.om.NodeName nameOfNode = ((Saxon.Eej.ee.update.InsertAttributeAction)(action)).getAttributeName();
                                int count = Saxon.Impl.Helpers.MapUtils.GetOrDefault(names, nameOfNode, 0);
                                Saxon.Impl.Helpers.MapUtils.Put(names, nameOfNode, (count+1));
                            }
                        }
                    }
                }
            }
            foreach (Saxon.Hej.om.NodeName nn in Saxon.Impl.Helpers.MapUtils.KeySet(names)) {
                int count = Saxon.Impl.Helpers.MapUtils.Get(names, nn);
                if ((count>1)) {
                    throw new Saxon.Hej.trans.XPathException(((((("After applying all updates, the element at "+Saxon.Hej.tree.util.Navigator.getPath(element))+" would have ")+(((count == 2) ? "two" : count)))+" attributes named ")+nn.getStructuredQName().getEQName()), "XUDY0021");
                }
            }
        }
        private void applyAttributeActions(Saxon.Hej.om.MutableNodeInfo element, System.Collections.Generic.IList<Saxon.Eej.ee.update.PendingUpdateAction> actions) {
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.DeleteAction)) {
                    ((Saxon.Hej.om.MutableNodeInfo)(action.getTargetNode())).delete();
                }
            }
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.ReplaceAttributeAction)) {
                    Saxon.Hej.om.NodeInfo target = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getOldAttribute();
                    ((Saxon.Hej.om.MutableNodeInfo)(target)).delete();
                }
            }
            System.Collections.Generic.IDictionary<Saxon.Hej.om.MutableNodeInfo,Saxon.Hej.om.NodeName> pendingRenames = null;
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.RenameAction)) {
                    Saxon.Hej.om.NodeName nc = ((Saxon.Eej.ee.update.RenameAction)(action)).getNewName();
                    Saxon.Hej.om.NodeName newName = null;
                    if ((element.getAttributeValue(nc.getURI(), nc.getLocalPart()) != null)) {
                        for (int n = 1; ; n++) {
                            string candidate = (nc.getLocalPart()+n);
                            if ((element.getAttributeValue(nc.getURI(), candidate) == null)) {
                                newName = new Saxon.Hej.om.FingerprintedQName(nc.getPrefix(), nc.getURI(), candidate);
                                break ;
                            }
                        }
                        if ((pendingRenames == null)) {
                            pendingRenames = new System.Collections.Generic.Dictionary<Saxon.Hej.om.MutableNodeInfo, Saxon.Hej.om.NodeName>();
                        }
                        Saxon.Impl.Helpers.MapUtils.Put(pendingRenames, (Saxon.Hej.om.MutableNodeInfo)(action.getTargetNode()), nc);
                        nc = newName;
                    }
                    ((Saxon.Hej.om.MutableNodeInfo)(action.getTargetNode())).rename(nc, ((Saxon.Eej.ee.update.RenameAction)(action)).isInheritNamespaces());
                }
            }
            if ((pendingRenames != null)) {
                foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.MutableNodeInfo,Saxon.Hej.om.NodeName> e in pendingRenames) {
                    e.Key.rename(e.Value, true);
                }
            }
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.ReplaceAttributeAction)) {
                    Saxon.Hej.om.AttributeMap content = ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).getNewAttributes();
                    foreach (Saxon.Hej.om.AttributeInfo a in content) {
                        element.addAttribute(a.getNodeName(), a.getType(), a.getValue(), Saxon.Hej.@event.ReceiverOption.NONE, ((Saxon.Eej.ee.update.ReplaceAttributeAction)(action)).isInheritNamespaces());
                    }
                }
            }
            foreach (Saxon.Eej.ee.update.PendingUpdateAction action in actions) {
                if ((action is Saxon.Eej.ee.update.InsertAttributeAction)) {
                    Saxon.Eej.ee.update.InsertAttributeAction a = (Saxon.Eej.ee.update.InsertAttributeAction)(action);
                    element.addAttribute(a.getAttributeName(), a.getNewTypeCode(), a.getNewStringValue(), Saxon.Hej.@event.ReceiverOption.NONE, ((Saxon.Eej.ee.update.InsertAttributeAction)(action)).isInheritNamespaces());
                }
            }
            element.removeTypeAnnotation();
        }
        public System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> getAffectedTrees() {
            return affectedTrees;
        }
    
    }
}