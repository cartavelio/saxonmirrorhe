using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnyFunctionType : Saxon.Hej.type.FunctionItemType {
        public readonly static Saxon.Hej.type.AnyFunctionType ANY_FUNCTION = new Saxon.Hej.type.AnyFunctionType();
        public static Saxon.Hej.type.AnyFunctionType getInstance() {
            return ANY_FUNCTION;
        }
        public Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.FUNCTION;
        }
        public bool isAtomicType() {
            return false;
        }
        public bool isPlainType() {
            return false;
        }
        public virtual bool isMapType() {
            return false;
        }
        public virtual bool isArrayType() {
            return false;
        }
        public virtual double getDefaultPriority() {
            return -(0.5);
        }
        public virtual string getBasicAlphaCode() {
            return "F";
        }
        public virtual Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return null;
        }
        public virtual Saxon.Hej.query.AnnotationList getAnnotationAssertions() {
            return Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public virtual bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return (item is Saxon.Hej.om.Function);
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return ANY_FUNCTION;
        }
        public int getPrimitiveType() {
            return Saxon.Hej.type.Type.FUNCTION;
        }
        public override string ToString() {
            return "function(*)";
        }
        public virtual Saxon.Hej.type.PlainType getAtomizedItemType() {
            return null;
        }
        public virtual bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public virtual Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            if ((other == this)) {
                return Saxon.Hej.type.Affinity.SAME_TYPE;
            } else {
                return Saxon.Hej.type.Affinity.SUBSUMES;
            }
        }
        public virtual Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RoleDiagnostic role) {
            return new Saxon.Hej.expr.ItemChecker(exp, this, role);
        }
        public virtual Saxon.Hej.value.SequenceType getResultType() {
            return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        }
    
        /* Expanding default method getGenre from interface FunctionItemType */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.FUNCTION;
        }

        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

        /* Expanding default method toExportString from interface ItemType */
        public virtual string toExportString() {
            return ToString();
        }

        /* Expanding default method explainMismatch from interface ItemType */
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Ejava.util.Optional<string>.empty();
        }

    }
}