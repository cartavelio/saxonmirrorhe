using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class PathMap {
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.parser.PathMap.PathMapRoot> pathMapRoots = new System.Collections.Generic.List<Saxon.Hej.expr.parser.PathMap.PathMapRoot>();
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.expr.Binding,Saxon.Hej.expr.parser.PathMap.PathMapNodeSet> pathsForVariables = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Binding, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet>();
        public class PathMapNode {
            internal System.Collections.Generic.IList<Saxon.Hej.expr.parser.PathMap.PathMapArc> arcs;
            private bool returnable;
            private bool atomized;
            private bool _hasUnknownDependencies;
            protected internal PathMapNode()  {
                arcs = new System.Collections.Generic.List<Saxon.Hej.expr.parser.PathMap.PathMapArc>();
            }
            public Saxon.Hej.expr.parser.PathMap.PathMapNode createArc(int axis, Saxon.Hej.pattern.NodeTest test) {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc a in arcs) {
                    if (((a.getAxis() == axis) && a.getNodeTest().Equals(test))) {
                        return a.getTarget();
                    }
                }
                Saxon.Hej.expr.parser.PathMap.PathMapNode target = new Saxon.Hej.expr.parser.PathMap.PathMapNode();
                Saxon.Hej.expr.parser.PathMap.PathMapArc arc = new Saxon.Hej.expr.parser.PathMap.PathMapArc(axis, test, target);
                arcs.Add(arc);
                return target;
            }
            public void createArc(int axis, Saxon.Hej.pattern.NodeTest test, Saxon.Hej.expr.parser.PathMap.PathMapNode target) {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc a in arcs) {
                    if ((((a.getAxis() == axis) && a.getNodeTest().Equals(test)) && (a.getTarget() == target))) {
                        a.getTarget().setReturnable((a.getTarget().isReturnable() || target.isReturnable()));
                        if (target.isAtomized()) {
                            a.getTarget().setAtomized();
                        }
                        return ;
                    }
                }
                Saxon.Hej.expr.parser.PathMap.PathMapArc arc = new Saxon.Hej.expr.parser.PathMap.PathMapArc(axis, test, target);
                arcs.Add(arc);
            }
            public Saxon.Hej.expr.parser.PathMap.PathMapArc[] getArcs() {
                return arcs.ToArray();
            }
            public void setReturnable(bool returnable) {
                this.returnable = returnable;
            }
            public bool isReturnable() {
                return returnable;
            }
            public bool hasReachableReturnables() {
                if (isReturnable()) {
                    return true;
                }
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in arcs) {
                    if (arc.getTarget().hasReachableReturnables()) {
                        return true;
                    }
                }
                return false;
            }
            public void setAtomized() {
                this.atomized = true;
            }
            public bool isAtomized() {
                return atomized;
            }
            public void setHasUnknownDependencies() {
                _hasUnknownDependencies = true;
            }
            public bool hasUnknownDependencies() {
                return _hasUnknownDependencies;
            }
            public bool allPathsAreWithinStreamableSnapshot() {
                if (((hasUnknownDependencies() || isReturnable()) || isAtomized())) {
                    return false;
                }
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in arcs) {
                    int axis = arc.getAxis();
                    if ((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                        Saxon.Hej.expr.parser.PathMap.PathMapNode next = arc.getTarget();
                        if (next.isReturnable()) {
                            return false;
                        }
                        if (((next.getArcs().Length != 0) && !(next.allPathsAreWithinStreamableSnapshot()))) {
                            return false;
                        }
                    } else {
                        if (((((axis == Saxon.Hej.om.AxisInfo.SELF) || (axis == Saxon.Hej.om.AxisInfo.ANCESTOR)) || (axis == Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF)) || (axis == Saxon.Hej.om.AxisInfo.PARENT))) {
                            Saxon.Hej.expr.parser.PathMap.PathMapNode next = arc.getTarget();
                            if (next.isAtomized()) {
                                return false;
                            }
                            if (!(next.allPathsAreWithinStreamableSnapshot())) {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    }
                }
                return true;
            }
        
        }
        public class PathMapRoot : Saxon.Hej.expr.parser.PathMap.PathMapNode {
            private readonly Saxon.Hej.expr.Expression rootExpression;
            public bool isDownwardsOnly;
            protected internal PathMapRoot(Saxon.Hej.expr.Expression root)  {
                this.rootExpression = root;
            }
            public Saxon.Hej.expr.Expression getRootExpression() {
                return rootExpression;
            }
        
        }
        public class PathMapArc {
            private readonly Saxon.Hej.expr.parser.PathMap.PathMapNode target;
            private readonly int axis;
            private readonly Saxon.Hej.pattern.NodeTest test;
            protected internal PathMapArc(int axis, Saxon.Hej.pattern.NodeTest test, Saxon.Hej.expr.parser.PathMap.PathMapNode target)  {
                this.axis = axis;
                this.test = test;
                this.target = target;
            }
            public int getAxis() {
                return axis;
            }
            public Saxon.Hej.pattern.NodeTest getNodeTest() {
                return test;
            }
            public Saxon.Hej.expr.parser.PathMap.PathMapNode getTarget() {
                return target;
            }
        
        }
        public class PathMapNodeSet : System.Collections.Generic.HashSet<Saxon.Hej.expr.parser.PathMap.PathMapNode> {
            public PathMapNodeSet()  {
            }
            public PathMapNodeSet(Saxon.Hej.expr.parser.PathMap.PathMapNode singleton)  {
                this.Add(singleton);
            }
            public Saxon.Hej.expr.parser.PathMap.PathMapNodeSet createArc(int axis, Saxon.Hej.pattern.NodeTest test) {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet targetSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    targetSet.Add(node.createArc(axis, test));
                }
                return targetSet;
            }
            public void addNodeSet(Saxon.Hej.expr.parser.PathMap.PathMapNodeSet nodes) {
                if ((nodes != null)) {
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in nodes) {
                        this.Add(node);
                    }
                }
            }
            public void setAtomized() {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    node.setAtomized();
                }
            }
            public void setReturnable(bool isReturned) {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    node.setReturnable(isReturned);
                }
            }
            public bool hasReachableReturnables() {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    if (node.hasReachableReturnables()) {
                        return true;
                    }
                }
                return false;
            }
            public bool allPathsAreWithinStreamableSnapshot() {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    if (!(node.allPathsAreWithinStreamableSnapshot())) {
                        return false;
                    }
                }
                return true;
            }
            public void addDescendants() {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    node.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.AnyNodeTest.getInstance());
                }
            }
            public void setHasUnknownDependencies() {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in this) {
                    node.setHasUnknownDependencies();
                }
            }
        
        }
        public PathMap(Saxon.Hej.expr.Expression exp)  {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet finalNodes = exp.addToPathMap(this, null);
            if ((finalNodes != null)) {
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode node in finalNodes) {
                    node.setReturnable(true);
                }
            }
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot makeNewRoot(Saxon.Hej.expr.Expression exp) {
            foreach (Saxon.Hej.expr.parser.PathMap.PathMapRoot r in pathMapRoots) {
                if (exp.isEqual(r.getRootExpression())) {
                    return r;
                }
            }
            Saxon.Hej.expr.parser.PathMap.PathMapRoot root = new Saxon.Hej.expr.parser.PathMap.PathMapRoot(exp);
            pathMapRoots.Add(root);
            return root;
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot[] getPathMapRoots() {
            return pathMapRoots.ToArray();
        }
        public void registerPathForVariable(Saxon.Hej.expr.Binding binding, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet nodeset) {
            Saxon.Impl.Helpers.MapUtils.Put(pathsForVariables, binding, nodeset);
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapNodeSet getPathForVariable(Saxon.Hej.expr.Binding binding) {
            return Saxon.Impl.Helpers.MapUtils.Get(pathsForVariables, binding);
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot getContextDocumentRoot() {
            Saxon.Hej.expr.parser.PathMap.PathMapRoot[] roots = getPathMapRoots();
            Saxon.Hej.expr.parser.PathMap.PathMapRoot contextRoot = null;
            foreach (Saxon.Hej.expr.parser.PathMap.PathMapRoot root in roots) {
                Saxon.Hej.expr.parser.PathMap.PathMapRoot newRoot = reduceToDownwardsAxes(root);
                if ((newRoot.getRootExpression() is Saxon.Hej.expr.RootExpression)) {
                    if ((contextRoot != null)) {
                        throw new Saxon.Ejava.lang.IllegalStateException("More than one context document root found in path map");
                    } else {
                        contextRoot = newRoot;
                    }
                }
            }
            return contextRoot;
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot getContextItemRoot() {
            Saxon.Hej.expr.parser.PathMap.PathMapRoot[] roots = getPathMapRoots();
            Saxon.Hej.expr.parser.PathMap.PathMapRoot contextRoot = null;
            foreach (Saxon.Hej.expr.parser.PathMap.PathMapRoot root in roots) {
                if ((root.getRootExpression() is Saxon.Hej.expr.ContextItemExpression)) {
                    if ((contextRoot != null)) {
                        throw new Saxon.Ejava.lang.IllegalStateException("More than one context document root found in path map");
                    } else {
                        contextRoot = root;
                    }
                }
            }
            return contextRoot;
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot getRootForDocument(string requiredUri) {
            Saxon.Hej.expr.parser.PathMap.PathMapRoot[] roots = getPathMapRoots();
            Saxon.Hej.expr.parser.PathMap.PathMapRoot requiredRoot = null;
            foreach (Saxon.Hej.expr.parser.PathMap.PathMapRoot root in roots) {
                Saxon.Hej.expr.parser.PathMap.PathMapRoot newRoot = reduceToDownwardsAxes(root);
                Saxon.Hej.expr.Expression exp = newRoot.getRootExpression();
                string baseUri;
                if (exp.isCallOn(typeof(Saxon.Hej.functions.Doc))) {
                    baseUri = exp.getStaticBaseURIString();
                } else {
                    if (exp.isCallOn(typeof(Saxon.Hej.functions.DocumentFn))) {
                        baseUri = exp.getStaticBaseURIString();
                    } else {
                        continue ;
                    }
                }
                Saxon.Hej.expr.Expression arg = ((Saxon.Hej.expr.SystemFunctionCall)(exp)).getArg(0);
                string suppliedUri = null;
                if ((arg is Saxon.Hej.expr.Literal)) {
                    try {
                        string argValue = ((Saxon.Hej.expr.Literal)(arg)).getGroundedValue().getUnicodeStringValue().ToString();
                        if ((baseUri == null)) {
                            if (new System.Uri(argValue, System.UriKind.RelativeOrAbsolute).IsAbsoluteUri) {
                                suppliedUri = argValue;
                            } else {
                                suppliedUri = null;
                            }
                        } else {
                            suppliedUri = Saxon.Hej.functions.ResolveURI.makeAbsolute(argValue, baseUri).ToString();
                        }
                    } catch (System.UriFormatException err) {
                        suppliedUri = null;
                    } catch (Saxon.Hej.trans.XPathException err) {
                        suppliedUri = null;
                    }
                }
                if (requiredUri.Equals(suppliedUri)) {
                    if ((requiredRoot != null)) {
                        throw new Saxon.Ejava.lang.IllegalStateException(("More than one document root found in path map for "+requiredUri));
                    } else {
                        requiredRoot = newRoot;
                    }
                }
            }
            return requiredRoot;
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapRoot reduceToDownwardsAxes(Saxon.Hej.expr.parser.PathMap.PathMapRoot root) {
            if (root.isDownwardsOnly) {
                return root;
            }
            Saxon.Hej.expr.parser.PathMap.PathMapRoot newRoot = root;
            if ((root.getRootExpression() is Saxon.Hej.expr.ContextItemExpression)) {
                Saxon.Hej.expr.RootExpression slash = new Saxon.Hej.expr.RootExpression();
                newRoot = makeNewRoot(slash);
                for (int i = (root.arcs.Count-1); (i>=0); i--) {
                    Saxon.Hej.expr.parser.PathMap.PathMapArc arc = root.arcs[i];
                    int axis = arc.getAxis();
                    switch (axis) {
                      case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                      case Saxon.Hej.om.AxisInfo.NAMESPACE:
                        {
                            Saxon.Hej.expr.parser.PathMap.PathMapNode newTarget = new Saxon.Hej.expr.parser.PathMap.PathMapNode();
                            newTarget.arcs.Add(arc);
                            newRoot.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT, newTarget);
                            break ;
                        }
                      default:
                        {
                            newRoot.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, arc.getNodeTest(), arc.getTarget());
                            break ;
                        }
                    }
                }
                for (int i = 0; (i<pathMapRoots.Count); i++) {
                    if ((pathMapRoots[i] == root)) {
                        Saxon.Impl.Helpers.ListUtils.Remove(pathMapRoots, i);
                        break ;
                    }
                }
            }
            Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.PathMap.PathMapNode> nodeStack = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.PathMap.PathMapNode>();
            nodeStack.push(newRoot);
            reduceToDownwardsAxes(newRoot, nodeStack);
            newRoot.isDownwardsOnly = true;
            return newRoot;
        }
        private void reduceToDownwardsAxes(Saxon.Hej.expr.parser.PathMap.PathMapRoot root, Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.PathMap.PathMapNode> nodeStack) {
            Saxon.Hej.expr.parser.PathMap.PathMapNode node = nodeStack.peek();
            if (node.hasUnknownDependencies()) {
                root.setHasUnknownDependencies();
            }
            for (int i = 0; (i<node.arcs.Count); i++) {
                nodeStack.push((node.arcs[i]).getTarget());
                reduceToDownwardsAxes(root, nodeStack);
                nodeStack.pop();
            }
            for (int i = (node.arcs.Count-1); (i>=0); i--) {
                Saxon.Hej.expr.parser.PathMap.PathMapArc thisArc = node.arcs[i];
                Saxon.Hej.expr.parser.PathMap.PathMapNode grandParent = (((nodeStack.size()<2) ? null : nodeStack.get((nodeStack.size()-2))));
                int lastAxis = -1;
                if ((grandParent != null)) {
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc1 in grandParent.arcs) {
                        Saxon.Hej.expr.parser.PathMap.PathMapArc arc = (arc1);
                        if ((arc.getTarget() == node)) {
                            lastAxis = arc.getAxis();
                        }
                    }
                }
                switch (thisArc.getAxis()) {
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    if ((thisArc.getNodeTest() == Saxon.Hej.pattern.NodeKindTest.DOCUMENT)) {
                        Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                        foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in thisArc.getTarget().arcs) {
                            root.arcs.Add(arc);
                        }
                        break ;
                    } else {
                        goto case Saxon.Hej.om.AxisInfo.ANCESTOR;
                    }
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                    {
                        if ((thisArc.getAxis() != Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF)) {
                            root.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, thisArc.getNodeTest(), thisArc.getTarget());
                            Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                        }
                        break ;
                    }
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                    break ;
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                    {
                        if ((grandParent != null)) {
                            grandParent.createArc(lastAxis, thisArc.getNodeTest(), thisArc.getTarget());
                            Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                            break ;
                        } else {
                            root.createArc(Saxon.Hej.om.AxisInfo.CHILD, thisArc.getNodeTest(), thisArc.getTarget());
                            Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                            break ;
                        }
                    }
                  case Saxon.Hej.om.AxisInfo.PARENT:
                    {
                        if ((((lastAxis == Saxon.Hej.om.AxisInfo.CHILD) || (lastAxis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) || (lastAxis == Saxon.Hej.om.AxisInfo.NAMESPACE))) {
                            if (node.isReturnable()) {
                                grandParent.setReturnable(true);
                            }
                            Saxon.Hej.expr.parser.PathMap.PathMapNode target = thisArc.getTarget();
                            for (int a = 0; (a<target.arcs.Count); a++) {
                                grandParent.arcs.Add(target.arcs[a]);
                            }
                            Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                        } else {
                            if ((lastAxis == Saxon.Hej.om.AxisInfo.DESCENDANT)) {
                                if (thisArc.getTarget().arcs.Count==0) {
                                    grandParent.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, thisArc.getNodeTest());
                                } else {
                                    grandParent.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, thisArc.getNodeTest(), thisArc.getTarget());
                                }
                                Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                            } else {
                                if (thisArc.getTarget().arcs.Count==0) {
                                    root.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, thisArc.getNodeTest());
                                } else {
                                    root.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, thisArc.getNodeTest(), thisArc.getTarget());
                                }
                                Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                            }
                        }
                        break ;
                    }
                  case Saxon.Hej.om.AxisInfo.SELF:
                    {
                        Saxon.Impl.Helpers.ListUtils.Remove(node.arcs, i);
                        break ;
                    }
                }
            }
        }
        private void showArcs(Saxon.Hej.lib.Logger @out, Saxon.Hej.expr.parser.PathMap.PathMapNode node, int indent) {
            string pad = "                                           ".Substring(0, indent - 0);
            System.Collections.Generic.IList<Saxon.Hej.expr.parser.PathMap.PathMapArc> arcs = node.arcs;
            foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in arcs) {
                @out.info(((((((pad+Saxon.Hej.om.AxisInfo.axisName[arc.getAxis()])+"::")+arc.getNodeTest().ToString())+((arc.getTarget().isAtomized() ? " @" : "")))+((arc.getTarget().isReturnable() ? " #" : "")))+((arc.getTarget().hasUnknownDependencies() ? " ...??" : ""))));
                showArcs(@out, arc.getTarget(), (indent+2));
            }
        }
    
    }
}