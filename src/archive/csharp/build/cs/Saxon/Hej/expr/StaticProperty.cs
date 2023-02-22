using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class StaticProperty {
        public const int DEPENDS_ON_CURRENT_ITEM = 1;
        public const int DEPENDS_ON_CONTEXT_ITEM = (1<<1);
        public const int DEPENDS_ON_POSITION = (1<<2);
        public const int DEPENDS_ON_LAST = (1<<3);
        public const int DEPENDS_ON_CONTEXT_DOCUMENT = (1<<4);
        public const int DEPENDS_ON_CURRENT_GROUP = (1<<5);
        public const int DEPENDS_ON_REGEX_GROUP = (1<<6);
        public const int DEPENDS_ON_LOCAL_VARIABLES = (1<<7);
        public const int DEPENDS_ON_USER_FUNCTIONS = (1<<8);
        public const int DEPENDS_ON_ASSIGNABLE_GLOBALS = (1<<9);
        public const int DEPENDS_ON_RUNTIME_ENVIRONMENT = (1<<10);
        public const int DEPENDS_ON_STATIC_CONTEXT = (1<<11);
        public const int DEPENDS_ON_XSLT_CONTEXT = (((DEPENDS_ON_CURRENT_ITEM|DEPENDS_ON_CURRENT_GROUP)|DEPENDS_ON_REGEX_GROUP)|DEPENDS_ON_ASSIGNABLE_GLOBALS);
        public const int DEPENDS_ON_FOCUS = (((DEPENDS_ON_CONTEXT_ITEM|DEPENDS_ON_POSITION)|DEPENDS_ON_LAST)|DEPENDS_ON_CONTEXT_DOCUMENT);
        public const int DEPENDS_ON_NON_DOCUMENT_FOCUS = ((DEPENDS_ON_CONTEXT_ITEM|DEPENDS_ON_POSITION)|DEPENDS_ON_LAST);
        public const int ALLOWS_ZERO = (1<<13);
        public const int ALLOWS_ONE = (1<<14);
        public const int ALLOWS_MANY = (1<<15);
        public const int CARDINALITY_MASK = ((ALLOWS_ZERO|ALLOWS_ONE)|ALLOWS_MANY);
        public const int ALLOWS_ONE_OR_MORE = (ALLOWS_ONE|ALLOWS_MANY);
        public const int ALLOWS_ZERO_OR_MORE = ((ALLOWS_ZERO|ALLOWS_ONE)|ALLOWS_MANY);
        public const int ALLOWS_ZERO_OR_ONE = (ALLOWS_ZERO|ALLOWS_ONE);
        public const int EXACTLY_ONE = ALLOWS_ONE;
        public const int EMPTY = ALLOWS_ZERO;
        public static int getCardinalityCode(int cardinality) {
            return (((cardinality&CARDINALITY_MASK))>>13);
        }
        public const int CONTEXT_DOCUMENT_NODESET = (1<<16);
        public const int ORDERED_NODESET = (1<<17);
        public const int REVERSE_DOCUMENT_ORDER = (1<<18);
        public const int PEER_NODESET = (1<<19);
        public const int SUBTREE_NODESET = (1<<20);
        public const int ATTRIBUTE_NS_NODESET = (1<<21);
        public const int ALL_NODES_NEWLY_CREATED = (1<<22);
        public const int NO_NODES_NEWLY_CREATED = (1<<23);
        public const int SINGLE_DOCUMENT_NODESET = (1<<24);
        public const int HAS_SIDE_EFFECTS = (1<<25);
        public const int NOT_UNTYPED_ATOMIC = (1<<26);
        public const int ALL_NODES_UNTYPED = (1<<27);
        public const int DEPENDENCY_MASK = (((((((((((DEPENDS_ON_CONTEXT_DOCUMENT|DEPENDS_ON_CONTEXT_ITEM)|DEPENDS_ON_CURRENT_GROUP)|DEPENDS_ON_REGEX_GROUP)|DEPENDS_ON_CURRENT_ITEM)|DEPENDS_ON_FOCUS)|DEPENDS_ON_LOCAL_VARIABLES)|DEPENDS_ON_USER_FUNCTIONS)|DEPENDS_ON_ASSIGNABLE_GLOBALS)|DEPENDS_ON_RUNTIME_ENVIRONMENT)|DEPENDS_ON_STATIC_CONTEXT)|HAS_SIDE_EFFECTS);
        public const int SPECIAL_PROPERTY_MASK = (((((((((((CONTEXT_DOCUMENT_NODESET|ORDERED_NODESET)|REVERSE_DOCUMENT_ORDER)|PEER_NODESET)|SUBTREE_NODESET)|ATTRIBUTE_NS_NODESET)|SINGLE_DOCUMENT_NODESET)|NO_NODES_NEWLY_CREATED)|HAS_SIDE_EFFECTS)|NOT_UNTYPED_ATOMIC)|ALL_NODES_UNTYPED)|ALL_NODES_NEWLY_CREATED);
        public const int NODESET_PROPERTIES = (((((((CONTEXT_DOCUMENT_NODESET|ORDERED_NODESET)|REVERSE_DOCUMENT_ORDER)|PEER_NODESET)|SUBTREE_NODESET)|ATTRIBUTE_NS_NODESET)|SINGLE_DOCUMENT_NODESET)|ALL_NODES_UNTYPED);
        private StaticProperty()  {
        }
        public static string display(int props) {
            Saxon.Ejava.lang.StringBuilder s = new Saxon.Ejava.lang.StringBuilder(128);
            s.append("D(");
            if ((((props&DEPENDS_ON_CURRENT_ITEM)) != 0)) {
                s.append("U");
            }
            if ((((props&DEPENDS_ON_CONTEXT_ITEM)) != 0)) {
                s.append("C");
            }
            if ((((props&DEPENDS_ON_POSITION)) != 0)) {
                s.append("P");
            }
            if ((((props&DEPENDS_ON_LAST)) != 0)) {
                s.append("L");
            }
            if ((((props&DEPENDS_ON_CONTEXT_DOCUMENT)) != 0)) {
                s.append("D");
            }
            if ((((props&DEPENDS_ON_LOCAL_VARIABLES)) != 0)) {
                s.append("V");
            }
            if ((((props&DEPENDS_ON_ASSIGNABLE_GLOBALS)) != 0)) {
                s.append("A");
            }
            if ((((props&DEPENDS_ON_REGEX_GROUP)) != 0)) {
                s.append("R");
            }
            if ((((props&DEPENDS_ON_RUNTIME_ENVIRONMENT)) != 0)) {
                s.append("E");
            }
            if ((((props&DEPENDS_ON_STATIC_CONTEXT)) != 0)) {
                s.append("S");
            }
            s.append(") C(");
            bool m = Saxon.Hej.value.Cardinality.allowsMany(props);
            bool z = Saxon.Hej.value.Cardinality.allowsZero(props);
            if ((m && z)) {
                s.append("*");
            } else {
                if (m) {
                    s.append("+");
                } else {
                    if (z) {
                        s.append("?");
                    } else {
                        s.append("1");
                    }
                }
            }
            s.append(") S(");
            if ((((props&HAS_SIDE_EFFECTS)) != 0)) {
                s.append("E");
            }
            if ((((props&NO_NODES_NEWLY_CREATED)) != 0)) {
                s.append("N");
            }
            if ((((props&NOT_UNTYPED_ATOMIC)) != 0)) {
                s.append("T");
            }
            if ((((props&ORDERED_NODESET)) != 0)) {
                s.append("O");
            }
            if ((((props&PEER_NODESET)) != 0)) {
                s.append("P");
            }
            if ((((props&REVERSE_DOCUMENT_ORDER)) != 0)) {
                s.append("R");
            }
            if ((((props&SINGLE_DOCUMENT_NODESET)) != 0)) {
                s.append("S");
            }
            if ((((props&SUBTREE_NODESET)) != 0)) {
                s.append("D");
            }
            s.append(")");
            return s.ToString();
        }
    
    }
}