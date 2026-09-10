using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public sealed class MultipleNodeKindTest : Saxon.Hej.pattern.NodeTest {
        public readonly static Saxon.Hej.pattern.MultipleNodeKindTest PARENT_NODE = new Saxon.Hej.pattern.MultipleNodeKindTest(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.ELEMENT));
        public readonly static Saxon.Hej.pattern.MultipleNodeKindTest DOC_ELEM_ATTR = new Saxon.Hej.pattern.MultipleNodeKindTest(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.ELEMENT).union(Saxon.Hej.type.UType.ATTRIBUTE));
        public readonly static Saxon.Hej.pattern.MultipleNodeKindTest LEAF = new Saxon.Hej.pattern.MultipleNodeKindTest(Saxon.Hej.type.UType.TEXT.union(Saxon.Hej.type.UType.COMMENT).union(Saxon.Hej.type.UType.PI).union(Saxon.Hej.type.UType.NAMESPACE).union(Saxon.Hej.type.UType.ATTRIBUTE));
        public readonly static Saxon.Hej.pattern.MultipleNodeKindTest CHILD_NODE = new Saxon.Hej.pattern.MultipleNodeKindTest(Saxon.Hej.type.UType.ELEMENT.union(Saxon.Hej.type.UType.TEXT).union(Saxon.Hej.type.UType.COMMENT).union(Saxon.Hej.type.UType.PI));
        internal Saxon.Hej.type.UType uType;
        internal int nodeKindMask;
        public MultipleNodeKindTest(Saxon.Hej.type.UType u)  {
            this.uType = u;
            if (Saxon.Hej.type.UType.DOCUMENT.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.DOCUMENT);
            }
            if (Saxon.Hej.type.UType.ELEMENT.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.ELEMENT);
            }
            if (Saxon.Hej.type.UType.ATTRIBUTE.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.ATTRIBUTE);
            }
            if (Saxon.Hej.type.UType.TEXT.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.TEXT);
            }
            if (Saxon.Hej.type.UType.COMMENT.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.COMMENT);
            }
            if (Saxon.Hej.type.UType.PI.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
            }
            if (Saxon.Hej.type.UType.NAMESPACE.overlaps(u)) {
                nodeKindMask |= (1<<Saxon.Hej.type.Type.NAMESPACE);
            }
        }
        public override Saxon.Hej.type.UType getUType() {
            return uType;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return (((nodeKindMask&((1<<nodeKind)))) != 0);
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => {
                int nodeKind = (nodeKindArray[nodeNr]&0x0f);
                if ((nodeKind == Saxon.Hej.type.Type.WHITESPACE_TEXT)) {
                    nodeKind = Saxon.Hej.type.Type.TEXT;
                }
                return (((nodeKindMask&((1<<nodeKind)))) != 0);
            } );
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            int nodeKind = node.getNodeKind();
            return (((nodeKindMask&((1<<nodeKind)))) != 0);
        }
        public override double getDefaultPriority() {
            return -(0.5);
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType> types = new System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType>(uType.decompose());
            format(types, fsb, x => x.ToString() /* method reference */);
            return fsb.ToString();
        }
        public override string toShortString() {
            if ((nodeKindMask == CHILD_NODE.nodeKindMask)) {
                return "node()";
            }
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType> types = new System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType>(uType.decompose());
            format(types, fsb, it => ((Saxon.Hej.pattern.NodeKindTest)(it)).toShortString());
            return fsb.ToString();
        }
        public override string toExportString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType> types = new System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType>(uType.decompose());
            format(types, fsb, x => x.toExportString() /* method reference */);
            return fsb.ToString();
        }
        private void format(System.Collections.Generic.LinkedList<Saxon.Hej.type.PrimitiveUType> list, Saxon.Ejava.lang.StringBuilder fsb, System.Func<Saxon.Hej.type.ItemType,string> show) {
            if ((list.Count == 1)) {
                fsb.append(show(list.First.Value.toItemType()));
            } else {
                bool first = true;
                foreach (Saxon.Hej.type.PrimitiveUType pu in list) {
                    fsb.append((first ? '(' : '|'));
                    first = false;
                    fsb.append(show(pu.toItemType()));
                }
                fsb.append(')');
            }
        }
        public override int GetHashCode() {
            return uType.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.pattern.MultipleNodeKindTest) && uType.Equals(((Saxon.Hej.pattern.MultipleNodeKindTest)(obj)).uType));
        }
    
    }
}