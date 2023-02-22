using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public abstract class SteppingNavigator {
        static Saxon.Hej.tree.util.SteppingNode getFollowingNode(Saxon.Hej.tree.util.SteppingNode start, Saxon.Hej.tree.util.SteppingNode anchor) {
            Saxon.Hej.tree.util.SteppingNode nodei = start.getFirstChild();
            if ((nodei != null)) {
                return nodei;
            }
            if (start.isSameNodeInfo(anchor)) {
                return null;
            }
            nodei = start;
            Saxon.Hej.tree.util.SteppingNode parenti = (Saxon.Hej.tree.util.SteppingNode)(start.getParent());
            do {
                nodei = nodei.getNextSibling();
                if ((nodei != null)) {
                    return nodei;
                } else {
                    if (parenti.isSameNodeInfo(anchor)) {
                        return null;
                    }
                }
                nodei = parenti;
                parenti = (Saxon.Hej.tree.util.SteppingNode)(parenti.getParent());
            } while ((parenti != null));
            return null;
        }
        private interface Stepper {
            Saxon.Hej.tree.util.SteppingNode step(Saxon.Hej.tree.util.SteppingNode node);
        
        }
        private class FollowingNodeStepper : Saxon.Hej.tree.util.SteppingNavigator.Stepper {
            internal Saxon.Hej.tree.util.SteppingNode anchor;
            internal FollowingNodeStepper(Saxon.Hej.tree.util.SteppingNode anchor)  {
                this.anchor = anchor;
            }
            public Saxon.Hej.tree.util.SteppingNode step(Saxon.Hej.tree.util.SteppingNode node) {
                return getFollowingNode(node, anchor);
            }
        
        }
        private class FollowingFilteredNodeStepper : Saxon.Hej.tree.util.SteppingNavigator.Stepper {
            internal Saxon.Hej.tree.util.SteppingNode anchor;
            internal Saxon.Hej.pattern.NodeTest test;
            internal FollowingFilteredNodeStepper(Saxon.Hej.tree.util.SteppingNode anchor, Saxon.Hej.pattern.NodeTest test)  {
                this.anchor = anchor;
                this.test = test;
            }
            public Saxon.Hej.tree.util.SteppingNode step(Saxon.Hej.tree.util.SteppingNode node) {
                do {
                    node = getFollowingNode(node, anchor);
                } while (((node != null) && !(test.test(node))));
                return node;
            }
        
        }
        private class FollowingElementStepper : Saxon.Hej.tree.util.SteppingNavigator.Stepper {
            internal Saxon.Hej.tree.util.SteppingNode anchor;
            internal Saxon.Hej.om.NamespaceUri uri;
            internal string local;
            internal FollowingElementStepper(Saxon.Hej.tree.util.SteppingNode anchor, Saxon.Hej.om.NamespaceUri uri, string local)  {
                this.anchor = anchor;
                this.uri = uri;
                this.local = local;
            }
            public Saxon.Hej.tree.util.SteppingNode step(Saxon.Hej.tree.util.SteppingNode node) {
                return node.getSuccessorElement(anchor, uri, local);
            }
        
        }
        private class FollowingFingerprintedElementStepper : Saxon.Hej.tree.util.SteppingNavigator.Stepper {
            internal Saxon.Hej.tree.util.SteppingNode anchor;
            internal int fingerprint;
            internal FollowingFingerprintedElementStepper(Saxon.Hej.tree.util.SteppingNode anchor, int fingerprint)  {
                this.anchor = anchor;
                this.fingerprint = fingerprint;
            }
            public Saxon.Hej.tree.util.SteppingNode step(Saxon.Hej.tree.util.SteppingNode node) {
                do {
                    node = getFollowingNode(node, anchor);
                } while (((node != null) && (node.getFingerprint() != fingerprint)));
                return node;
            }
        
        }
        public class DescendantAxisIterator : Saxon.Hej.tree.iter.AxisIterator {
            private readonly Saxon.Hej.tree.util.SteppingNode start;
            private Saxon.Hej.tree.util.SteppingNode current;
            private bool done;
            private readonly Saxon.Hej.tree.util.SteppingNavigator.Stepper stepper;
            public DescendantAxisIterator(Saxon.Hej.tree.util.SteppingNode start, bool includeSelf, Saxon.Hej.pattern.NodeTest test)  {
                this.start = start;
                if (!(((includeSelf && test.test(start))))) {
                    current = start;
                }
                if (((test == null) || (test == Saxon.Hej.pattern.AnyNodeTest.getInstance()))) {
                    stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingNodeStepper(start);
                } else {
                    if ((test is Saxon.Hej.pattern.NameTest)) {
                        if ((((Saxon.Hej.pattern.NameTest)(test)).getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT)) {
                            Saxon.Hej.pattern.NameTest nt = (Saxon.Hej.pattern.NameTest)(test);
                            if (start.hasFingerprint()) {
                                stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFingerprintedElementStepper(start, nt.getFingerprint());
                            } else {
                                stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingElementStepper(start, nt.getNamespaceURI(), nt.getLocalPart());
                            }
                        } else {
                            stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFilteredNodeStepper(start, test);
                        }
                    } else {
                        if ((test is Saxon.Hej.pattern.NodeKindTest)) {
                            if ((((Saxon.Hej.pattern.NodeKindTest)(test)).getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT)) {
                                stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingElementStepper(start, null, null);
                            } else {
                                stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFilteredNodeStepper(start, test);
                            }
                        } else {
                            if ((test is Saxon.Hej.pattern.LocalNameTest)) {
                                if ((((Saxon.Hej.pattern.LocalNameTest)(test)).getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT)) {
                                    Saxon.Hej.pattern.LocalNameTest nt = (Saxon.Hej.pattern.LocalNameTest)(test);
                                    stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingElementStepper(start, null, nt.getLocalName());
                                } else {
                                    stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFilteredNodeStepper(start, test);
                                }
                            } else {
                                if ((test is Saxon.Hej.pattern.NamespaceTest)) {
                                    if ((((Saxon.Hej.pattern.NamespaceTest)(test)).getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT)) {
                                        Saxon.Hej.pattern.NamespaceTest nt = (Saxon.Hej.pattern.NamespaceTest)(test);
                                        stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingElementStepper(start, nt.getNamespaceURI(), null);
                                    } else {
                                        stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFilteredNodeStepper(start, test);
                                    }
                                } else {
                                    stepper = new Saxon.Hej.tree.util.SteppingNavigator.FollowingFilteredNodeStepper(start, test);
                                }
                            }
                        }
                    }
                }
            }
            public Saxon.Hej.om.Item next() {
                if (done) {
                    return null;
                }
                if ((current == null)) {
                    current = start;
                    return start;
                }
                Saxon.Hej.tree.util.SteppingNode curr = stepper.step(current);
                if ((curr == null)) {
                    done = true;
                }
                return current = curr;
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

        }
    
    }
}