using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class IdentityField : Saxon.Eej.ee.schema.FieldOrSelector {
        private Saxon.Hej.type.BuiltInAtomicType builtInBaseType;
        private bool listValued;
        private bool ascending;
        public IdentityField(string xpath)  {
            setXPath(xpath);
        }
        public Saxon.Hej.type.BuiltInAtomicType getBuiltInBaseType() {
            return builtInBaseType;
        }
        public bool isListValued() {
            return listValued;
        }
        public void setBuiltInBaseType(Saxon.Hej.type.BuiltInAtomicType builtInBaseType, bool listValued) {
            this.builtInBaseType = builtInBaseType;
            this.listValued = listValued;
        }
        public void setAscending(bool upwards) {
            this.ascending = upwards;
        }
        public bool isAscending() {
            return ascending;
        }
        public int getCardinality(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            selectExpression = selectExpression.simplify().typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            return selectExpression.getCardinality();
        }
    
    }
}