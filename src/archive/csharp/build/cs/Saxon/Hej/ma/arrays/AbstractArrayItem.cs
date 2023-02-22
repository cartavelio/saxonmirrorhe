using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public abstract class AbstractArrayItem : Saxon.Hej.ma.arrays.ArrayItem {
        private Saxon.Hej.value.SequenceType memberType = null;
        public override Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            return new Saxon.Hej.expr.OperandRole[]{Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC};
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(arrayLength());
            foreach (Saxon.Hej.om.GroundedValue seq in members()) {
                Saxon.Hej.om.SequenceTool.supply(seq.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                    Saxon.Hej.om.AtomicSequence atoms = item.atomize();
                    foreach (Saxon.Hej.value.AtomicValue atom in atoms) {
                        list.Add(atom);
                    }
                } )));
            }
            return new Saxon.Hej.om.AtomicArray(list);
        }
        public override Saxon.Hej.query.AnnotationList getAnnotations() {
            return Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return null;
        }
        public override string getDescription() {
            return "array";
        }
        public override int getArity() {
            return 1;
        }
        public override Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext callingContext, Saxon.Hej.expr.ContextOriginator originator) {
            return callingContext;
        }
        public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.value.IntegerValue subscript = (Saxon.Hej.value.IntegerValue)(args[0].head());
            return get((Saxon.Hej.ma.arrays.ArrayFunctionSet.checkSubscript(subscript, arrayLength())-1));
        }
        public override bool deepEquals(Saxon.Hej.om.FunctionItem other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags) {
            if ((other is Saxon.Hej.ma.arrays.ArrayItem)) {
                Saxon.Hej.ma.arrays.ArrayItem that = (Saxon.Hej.ma.arrays.ArrayItem)(other);
                if ((this.arrayLength() != that.arrayLength())) {
                    return false;
                }
                for (int i = 0; (i<this.arrayLength()); i++) {
                    if (!(Saxon.Hej.functions.DeepEqual.deepEqual(this.get(i).iterate(), that.get(i).iterate(), comparer, context, flags))) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }
        public override bool effectiveBooleanValue() {
            throw new Saxon.Hej.trans.XPathException("Effective boolean value is not defined for arrays", "FORG0006");
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new Saxon.Hej.trans.UncheckedXPathException(new Saxon.Hej.trans.XPathException("An array has no string value", "FOTY0014"));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("array");
            @out.emitAttribute("size", (arrayLength()+""));
            foreach (Saxon.Hej.om.GroundedValue mem in members()) {
                Saxon.Hej.expr.Literal.exportValue(mem, @out);
            }
            @out.endElement();
        }
        public override bool isTrustedResultType() {
            return false;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(256);
            buffer.append("[");
            foreach (Saxon.Hej.om.GroundedValue seq in members()) {
                if ((buffer.length()>1)) {
                    buffer.append(", ");
                }
                buffer.append(seq.ToString());
            }
            buffer.append("]");
            return buffer.ToString();
        }
        public override Saxon.Hej.value.SequenceType getMemberType(Saxon.Hej.type.TypeHierarchy th) {
            if ((memberType == null)) {
                if (isEmpty()) {
                    memberType = Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.ErrorType.getInstance(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                } else {
                    Saxon.Hej.type.ItemType contentType = null;
                    int contentCard = Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                    foreach (Saxon.Hej.om.GroundedValue s in members()) {
                        if ((contentType == null)) {
                            contentType = Saxon.Hej.om.SequenceTool.getItemType(s, th);
                            contentCard = Saxon.Hej.om.SequenceTool.getCardinality(s);
                        } else {
                            contentType = Saxon.Hej.type.Type.getCommonSuperType(contentType, Saxon.Hej.om.SequenceTool.getItemType(s, th));
                            contentCard = Saxon.Hej.value.Cardinality.union(contentCard, Saxon.Hej.om.SequenceTool.getCardinality(s));
                        }
                    }
                    memberType = Saxon.Hej.value.SequenceType.makeSequenceType(contentType, contentCard);
                }
            }
            return memberType;
        }
    
    }
}