using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class Operand : System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand>, Saxon.Hej.expr.ExpressionOwner {
        private readonly Saxon.Hej.expr.Expression parentExpression;
        private Saxon.Hej.expr.Expression childExpression;
        private Saxon.Hej.expr.OperandRole role;
        public Operand(Saxon.Hej.expr.Expression parentExpression, Saxon.Hej.expr.Expression childExpression, Saxon.Hej.expr.OperandRole role)  {
            this.parentExpression = parentExpression;
            this.role = role;
            setChildExpression(childExpression);
        }
        public Saxon.Hej.expr.Expression getParentExpression() {
            return parentExpression;
        }
        public Saxon.Hej.expr.Expression getChildExpression() {
            return childExpression;
        }
        public void setChildExpression(Saxon.Hej.expr.Expression childExpression) {
            if ((childExpression != this.childExpression)) {
                if (role.isConstrainedClass()) {
                    if ((role.getConstraint() != null)) {
                        if (!(role.getConstraint()(childExpression))) {
                            throw new Saxon.Ejava.lang.AssertionError();
                        }
                    } else {
                        if (((this.childExpression != null) && (childExpression.GetType() != this.childExpression.GetType()))) {
                            throw new Saxon.Ejava.lang.AssertionError();
                        }
                    }
                }
                this.childExpression = childExpression;
                parentExpression.adoptChildExpression(childExpression);
                parentExpression.resetLocalStaticProperties();
            }
        }
        private const bool DEBUG = false;
        public void detachChild() {
            
        }
        public Saxon.Hej.expr.OperandRole getOperandRole() {
            return role;
        }
        public void setOperandRole(Saxon.Hej.expr.OperandRole role) {
            this.role = role;
        }
        public bool setsNewFocus() {
            return role.setsNewFocus();
        }
        public bool hasSpecialFocusRules() {
            return role.hasSpecialFocusRules();
        }
        public bool hasSameFocus() {
            return role.hasSameFocus();
        }
        public bool isHigherOrder() {
            return role.isHigherOrder();
        }
        public bool isEvaluatedRepeatedly() {
            return role.isEvaluatedRepeatedly();
        }
        public Saxon.Hej.expr.OperandUsage getUsage() {
            return role.getUsage();
        }
        public void setUsage(Saxon.Hej.expr.OperandUsage usage) {
            role = new Saxon.Hej.expr.OperandRole(role.properties, usage, role.getRequiredType());
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return role.getRequiredType();
        }
        public bool isInChoiceGroup() {
            return role.isInChoiceGroup();
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.expr.Operand> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Hej.expr.Operand>(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            try {
                setChildExpression(getChildExpression().typeCheck(visitor, contextInfo));
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getChildExpression().getLocation());
                if (!(e.isReportableStatically())) {
                    visitor.getStaticContext().issueWarning(("Evaluation will always throw a dynamic error: "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXWN9027, getChildExpression().getLocation());
                    setChildExpression(new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e)));
                } else {
                    throw;
                }
            }
        }
        public void optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            try {
                setChildExpression(getChildExpression().optimize(visitor, contextInfo));
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getChildExpression().getLocation());
                if (!(e.isReportableStatically())) {
                    visitor.getStaticContext().issueWarning(("Evaluation will always throw a dynamic error: "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXWN9027, getChildExpression().getLocation());
                    setChildExpression(new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e)));
                } else {
                    throw;
                }
            }
        }
        public static Saxon.Hej.expr.OperandUsage typeDeterminedUsage(Saxon.Hej.type.ItemType type) {
            if (type.isPlainType()) {
                return Saxon.Hej.expr.OperandUsage.ABSORPTION;
            } else {
                if (((type is Saxon.Hej.pattern.NodeTest) || (type == Saxon.Hej.type.AnyItemType.getInstance()))) {
                    return Saxon.Hej.expr.OperandUsage.NAVIGATION;
                } else {
                    return Saxon.Hej.expr.OperandUsage.INSPECTION;
                }
            }
        }
    
    }
}