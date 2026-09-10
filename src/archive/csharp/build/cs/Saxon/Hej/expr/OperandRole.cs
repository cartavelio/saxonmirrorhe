using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class OperandRole {
        public const int SETS_NEW_FOCUS = 1;
        public const int USES_NEW_FOCUS = 2;
        public const int HIGHER_ORDER = 4;
        public const int IN_CHOICE_GROUP = 8;
        public const int CONSTRAINED_CLASS = 16;
        public const int SINGLETON = 32;
        public const int HAS_SPECIAL_FOCUS_RULES = 64;
        public readonly static Saxon.Hej.expr.OperandRole SAME_FOCUS_ACTION = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole FOCUS_CONTROLLING_SELECT = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.SETS_NEW_FOCUS, Saxon.Hej.expr.OperandUsage.INSPECTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole FOCUS_CONTROLLED_ACTION = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole INSPECT = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.INSPECTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole ABSORB = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole REPEAT_INSPECT = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.HIGHER_ORDER, Saxon.Hej.expr.OperandUsage.INSPECTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole NAVIGATE = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole REPEAT_NAVIGATE = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.HIGHER_ORDER, Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole FLWOR_TUPLE_CONSTRAINED = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.HIGHER_ORDER|Saxon.Hej.expr.OperandRole.CONSTRAINED_CLASS), Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE, expr => (expr is Saxon.Hej.expr.flwor.TupleExpression));
        public readonly static Saxon.Hej.expr.OperandRole SINGLE_ATOMIC = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
        public readonly static Saxon.Hej.expr.OperandRole ATOMIC_SEQUENCE = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole NEW_FOCUS_ATOMIC = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
        public readonly static Saxon.Hej.expr.OperandRole PATTERN = new Saxon.Hej.expr.OperandRole(((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER)|Saxon.Hej.expr.OperandRole.CONSTRAINED_CLASS), Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, expr => (expr is Saxon.Hej.pattern.Pattern));
        internal int properties;
        private readonly Saxon.Hej.expr.OperandUsage usage;
        private Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        private System.Predicate<Saxon.Hej.expr.Expression> constraint;
        public OperandRole(int properties, Saxon.Hej.expr.OperandUsage usage)  {
            this.properties = properties;
            this.usage = usage;
        }
        public OperandRole(int properties, Saxon.Hej.expr.OperandUsage usage, Saxon.Hej.value.SequenceType requiredType)  {
            this.properties = properties;
            this.usage = usage;
            this.requiredType = requiredType;
        }
        public OperandRole(int properties, Saxon.Hej.expr.OperandUsage usage, Saxon.Hej.value.SequenceType requiredType, System.Predicate<Saxon.Hej.expr.Expression> constraint)  {
            this.properties = properties;
            this.usage = usage;
            this.requiredType = requiredType;
            this.constraint = constraint;
        }
        public Saxon.Hej.expr.OperandRole withConstraint(System.Predicate<Saxon.Hej.expr.Expression> constraint) {
            return new Saxon.Hej.expr.OperandRole(properties, usage, requiredType, constraint);
        }
        public Saxon.Hej.expr.OperandRole withConstrainedClass() {
            return new Saxon.Hej.expr.OperandRole((properties|CONSTRAINED_CLASS), usage, requiredType, constraint);
        }
        public bool setsNewFocus() {
            return (((properties&SETS_NEW_FOCUS)) != 0);
        }
        public bool hasSameFocus() {
            return (((properties&((USES_NEW_FOCUS|HAS_SPECIAL_FOCUS_RULES)))) == 0);
        }
        public bool hasSpecialFocusRules() {
            return (((properties&HAS_SPECIAL_FOCUS_RULES)) != 0);
        }
        public bool isHigherOrder() {
            return (((properties&HIGHER_ORDER)) != 0);
        }
        public bool isEvaluatedRepeatedly() {
            return (((((properties&HIGHER_ORDER)) != 0)) && ((((properties&SINGLETON)) == 0)));
        }
        public bool isConstrainedClass() {
            return (((properties&CONSTRAINED_CLASS)) != 0);
        }
        public void setConstraint(System.Predicate<Saxon.Hej.expr.Expression> constraint) {
            this.constraint = constraint;
        }
        public System.Predicate<Saxon.Hej.expr.Expression> getConstraint() {
            return constraint;
        }
        public Saxon.Hej.expr.OperandUsage getUsage() {
            return usage;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public bool isInChoiceGroup() {
            return (((properties&IN_CHOICE_GROUP)) != 0);
        }
        public static Saxon.Hej.expr.OperandUsage getTypeDeterminedUsage(Saxon.Hej.type.ItemType type) {
            if ((type is Saxon.Hej.type.FunctionItemType)) {
                return Saxon.Hej.expr.OperandUsage.INSPECTION;
            } else {
                if ((type is Saxon.Hej.type.PlainType)) {
                    return Saxon.Hej.expr.OperandUsage.ABSORPTION;
                } else {
                    return Saxon.Hej.expr.OperandUsage.NAVIGATION;
                }
            }
        }
        public Saxon.Hej.expr.OperandRole modifyProperty(int property, bool on) {
            int newProp = (on ? ((properties|property)) : ((properties&~(property))));
            return new Saxon.Hej.expr.OperandRole(newProp, usage, requiredType);
        }
        public int getProperties() {
            return properties;
        }
    
    }
}