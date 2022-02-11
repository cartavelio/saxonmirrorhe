using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public sealed class AxisInfo {

        static AxisInfo() {
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, ANCESTOR, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.DOCUMENT);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, CHILD, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, DESCENDANT, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.CHILD_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, FOLLOWING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, FOLLOWING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, PARENT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, PRECEDING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, PRECEDING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.DOCUMENT, SELF, Saxon.Hej.type.UType.DOCUMENT);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, ATTRIBUTE, Saxon.Hej.type.UType.ATTRIBUTE);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, CHILD, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, DESCENDANT, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, FOLLOWING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, NAMESPACE, Saxon.Hej.type.UType.NAMESPACE);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, PARENT, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, PRECEDING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ELEMENT, SELF, Saxon.Hej.type.UType.ELEMENT);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.ATTRIBUTE.union(Saxon.Hej.type.UType.PARENT_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, CHILD, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, DESCENDANT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.ATTRIBUTE);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, FOLLOWING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, PARENT, Saxon.Hej.type.UType.ELEMENT);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, PRECEDING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.ATTRIBUTE, SELF, Saxon.Hej.type.UType.ATTRIBUTE);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.TEXT.union(Saxon.Hej.type.UType.PARENT_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.TEXT, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, CHILD, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, DESCENDANT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.TEXT);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, FOLLOWING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, PARENT, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, PRECEDING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.TEXT, SELF, Saxon.Hej.type.UType.TEXT);
            e(Saxon.Hej.type.PrimitiveUType.PI, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.PI.union(Saxon.Hej.type.UType.PARENT_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.PI, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.PI, CHILD, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.PI, DESCENDANT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.PI, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.PI);
            e(Saxon.Hej.type.PrimitiveUType.PI, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, FOLLOWING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.PI, PARENT, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, PRECEDING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.PI, SELF, Saxon.Hej.type.UType.PI);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.COMMENT.union(Saxon.Hej.type.UType.PARENT_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, CHILD, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, DESCENDANT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.COMMENT);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, FOLLOWING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, PARENT, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, PRECEDING_SIBLING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.COMMENT, SELF, Saxon.Hej.type.UType.COMMENT);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, ANCESTOR, Saxon.Hej.type.UType.PARENT_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, ANCESTOR_OR_SELF, Saxon.Hej.type.UType.NAMESPACE.union(Saxon.Hej.type.UType.PARENT_NODE_KINDS));
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, ATTRIBUTE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, CHILD, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, DESCENDANT, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, DESCENDANT_OR_SELF, Saxon.Hej.type.UType.NAMESPACE);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, FOLLOWING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, FOLLOWING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, NAMESPACE, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, PARENT, Saxon.Hej.type.UType.ELEMENT);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, PRECEDING, Saxon.Hej.type.UType.CHILD_NODE_KINDS);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, PRECEDING_SIBLING, Saxon.Hej.type.UType.VOID);
            e(Saxon.Hej.type.PrimitiveUType.NAMESPACE, SELF, Saxon.Hej.type.UType.NAMESPACE);
        }
        public const int ANCESTOR = 0;
        public const int ANCESTOR_OR_SELF = 1;
        public const int ATTRIBUTE = 2;
        public const int CHILD = 3;
        public const int DESCENDANT = 4;
        public const int DESCENDANT_OR_SELF = 5;
        public const int FOLLOWING = 6;
        public const int FOLLOWING_SIBLING = 7;
        public const int NAMESPACE = 8;
        public const int PARENT = 9;
        public const int PRECEDING = 10;
        public const int PRECEDING_SIBLING = 11;
        public const int SELF = 12;
        public const int PRECEDING_OR_ANCESTOR = 13;
        public static readonly short[] principalNodeType = {Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.NAMESPACE, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Type.ELEMENT};
        public static readonly Saxon.Hej.type.UType[] principalNodeUType = {Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ATTRIBUTE, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.NAMESPACE, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT, Saxon.Hej.type.UType.ELEMENT};
        public static readonly bool[] isForwards = {false, false, true, true, true, true, true, true, true, true, false, false, true, false};
        public static readonly bool[] isPeerAxis = {false, false, true, true, false, false, false, true, true, true, false, true, true, false};
        public static readonly bool[] isSubtreeAxis = {false, false, true, true, true, true, false, false, true, false, false, false, true, false};
        public static readonly string[] axisName = {"ancestor", "ancestor-or-self", "attribute", "child", "descendant", "descendant-or-self", "following", "following-sibling", "namespace", "parent", "preceding", "preceding-sibling", "self", "preceding-or-ancestor"};
        private AxisInfo()  {
        }
        public static int getAxisNumber(string name) {
            switch (name) {
              case "ancestor":
                return ANCESTOR;
              case "ancestor-or-self":
                return ANCESTOR_OR_SELF;
              case "attribute":
                return ATTRIBUTE;
              case "child":
                return CHILD;
              case "descendant":
                return DESCENDANT;
              case "descendant-or-self":
                return DESCENDANT_OR_SELF;
              case "following":
                return FOLLOWING;
              case "following-sibling":
                return FOLLOWING_SIBLING;
              case "namespace":
                return NAMESPACE;
              case "parent":
                return PARENT;
              case "preceding":
                return PRECEDING;
              case "preceding-sibling":
                return PRECEDING_SIBLING;
              case "self":
                return SELF;
              case "preceding-or-ancestor":
                return PRECEDING_OR_ANCESTOR;
              default:
                throw new Saxon.Hej.trans.XPathException(("Unknown axis name: "+name));
            }
        }
        private static readonly int DOC = (1<<Saxon.Hej.type.Type.DOCUMENT);
        private static readonly int ELE = (1<<Saxon.Hej.type.Type.ELEMENT);
        private static readonly int ATT = (1<<Saxon.Hej.type.Type.ATTRIBUTE);
        private static readonly int TEX = (1<<Saxon.Hej.type.Type.TEXT);
        private static readonly int PIN = (1<<Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
        private static readonly int COM = (1<<Saxon.Hej.type.Type.COMMENT);
        private static readonly int NAM = (1<<Saxon.Hej.type.Type.NAMESPACE);
        private static readonly int[] voidAxisTable = {DOC, 0, (((((DOC|ATT)|TEX)|PIN)|COM)|NAM), ((((ATT|TEX)|PIN)|COM)|NAM), ((((ATT|TEX)|PIN)|COM)|NAM), 0, DOC, ((DOC|ATT)|NAM), (((((DOC|ATT)|TEX)|PIN)|COM)|NAM), DOC, DOC, ((DOC|ATT)|NAM), 0};
        public static bool isAlwaysEmpty(int axis, int nodeKind) {
            return (((voidAxisTable[axis]&((1<<nodeKind)))) != 0);
        }
        private static readonly int[] nodeKindTable = {(DOC|ELE), ((((((DOC|ELE)|ATT)|TEX)|PIN)|COM)|NAM), ATT, (((ELE|TEX)|PIN)|COM), (((ELE|TEX)|PIN)|COM), ((((((DOC|ELE)|ATT)|TEX)|PIN)|COM)|NAM), (((ELE|TEX)|PIN)|COM), (((ELE|TEX)|PIN)|COM), NAM, (DOC|ELE), (((ELE|TEX)|PIN)|COM), (((ELE|TEX)|PIN)|COM), ((((((DOC|ELE)|ATT)|TEX)|PIN)|COM)|NAM)};
        public static bool containsNodeKind(int axis, int nodeKind) {
            return ((nodeKind == Saxon.Hej.type.Type.NODE) || (((nodeKindTable[axis]&((1<<nodeKind)))) != 0));
        }
        public static int[] inverseAxis = {DESCENDANT, DESCENDANT_OR_SELF, PARENT, PARENT, ANCESTOR, ANCESTOR_OR_SELF, PRECEDING, PRECEDING_SIBLING, PARENT, CHILD, FOLLOWING, FOLLOWING_SIBLING, SELF};
        public static int[] excludeSelfAxis = {ANCESTOR, ANCESTOR, ATTRIBUTE, CHILD, DESCENDANT, DESCENDANT, FOLLOWING, FOLLOWING_SIBLING, NAMESPACE, PARENT, PRECEDING, PRECEDING_SIBLING, SELF};
        private static readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.type.UType> axisTransitions = new Saxon.Hej.z.IntHashMap<Saxon.Hej.type.UType>(50);
        private static void e(Saxon.Hej.type.PrimitiveUType origin, int axis, Saxon.Hej.type.UType target) {
            axisTransitions.put(makeKey(origin, axis), target);
        }
        private static int makeKey(Saxon.Hej.type.PrimitiveUType origin, int axis) {
            return ((origin.getBit()<<16)|axis);
        }
        public static Saxon.Hej.type.UType getTargetUType(Saxon.Hej.type.UType origin, int axis) {
            Saxon.Hej.type.UType resultType = Saxon.Hej.type.UType.VOID;
            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> origins = origin.intersection(Saxon.Hej.type.UType.ANY_NODE).decompose();
            foreach (Saxon.Hej.type.PrimitiveUType u in origins) {
                Saxon.Hej.type.UType r = axisTransitions.get(makeKey(u, axis));
                if ((r == null)) {
                    throw new Saxon.Ejava.lang.AssertionError(((("Unknown transitions for primitive type "+u.ToString())+"::")+axis));
                }
                resultType = resultType.union(r);
            }
            return resultType;
        }
    
    }
}