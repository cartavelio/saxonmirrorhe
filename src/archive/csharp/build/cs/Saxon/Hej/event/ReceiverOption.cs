using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class ReceiverOption {
        public const int NONE = 0;
        public const int DISABLE_ESCAPING = 0x01;
        public const int DISABLE_CHARACTER_MAPS = 0x02;
        public const int NO_SPECIAL_CHARS = 0x04;
        public const int DEFAULTED_VALUE = 0x08;
        public const int NILLED_ELEMENT = 0x10;
        public const int REJECT_DUPLICATES = 0x20;
        public const int NAMESPACE_OK = 0x40;
        public const int DISINHERIT_NAMESPACES = unchecked((int)0x80);
        public const int USE_NULL_MARKERS = 0x100;
        public const int NILLABLE_ELEMENT = 0x200;
        public const int WHOLE_TEXT_NODE = 0x400;
        public const int IS_ID = unchecked((int)0x800);
        public const int IS_IDREF = 0x1000;
        public const int ID_IDREF_CHECKED = 0x2000;
        public const int TERMINATE = 0x4000;
        public const int MUTABLE_TREE = unchecked((int)0x8000);
        public const int REFUSE_NAMESPACES = 0x10000;
        public const int BEQUEATH_INHERITED_NAMESPACES_ONLY = 0x20000;
        public const int HAS_CHILDREN = 0x40000;
        public const int ALL_NAMESPACES = unchecked((int)0x80000);
        public const int NOT_A_DUPLICATE = 0x100000;
        public const int SEPARATOR = 0x100000;
        public static bool contains(int options, int option) {
            return (((options&option)) != 0);
        }
    
    }
}