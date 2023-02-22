using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class SelfReferenceRecordTest : Saxon.Hej.type.AnyFunctionType, Saxon.Hej.ma.map.TupleType {
        private readonly Saxon.Hej.ma.map.RecordTest containingType;
        public SelfReferenceRecordTest(Saxon.Hej.ma.map.RecordTest containingType)  {
            this.containingType = containingType;
        }
        public override  /* REPLACING public */ Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.MAP;
        }
        public override bool isMapType() {
            return true;
        }
        public override bool isArrayType() {
            return false;
        }
        public System.Collections.Generic.IEnumerable<string> getFieldNames() {
            return containingType.getFieldNames();
        }
        public Saxon.Hej.value.SequenceType getFieldType(string field) {
            return containingType.getFieldType(field);
        }
        public bool isOptionalField(string field) {
            return containingType.isOptionalField(field);
        }
        public bool isExtensible() {
            return containingType.isExtensible();
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return containingType.matches(item, th);
        }
        public int getArity() {
            return 1;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ATOMIC};
        }
        public override Saxon.Hej.value.SequenceType getResultType() {
            return containingType.getResultType();
        }
        public override double getDefaultPriority() {
            return 0.5;
        }
        public override string ToString() {
            return "..";
        }
        public override  /* REPLACING public */ string toExportString() {
            return "..";
        }
        public override string getBasicAlphaCode() {
            return "..";
        }
        public override bool Equals(System.Object other) {
            return (((this == other) || containingType.Equals(other)) || ((other is Saxon.Hej.ma.map.SelfReferenceRecordTest) && containingType.Equals(((Saxon.Hej.ma.map.SelfReferenceRecordTest)(other)).containingType)));
        }
        public override int GetHashCode() {
            return 0x27ca481f;
        }
        public override Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            return containingType.relationship(other, th);
        }
        public override Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) {
            return new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).makeFunctionSequenceCoercer(exp, role);
        }
    
    }
}