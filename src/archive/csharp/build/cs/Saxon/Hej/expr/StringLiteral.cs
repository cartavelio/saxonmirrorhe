using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class StringLiteral : Saxon.Hej.expr.Literal {
        public StringLiteral(Saxon.Hej.value.StringValue value) : base (value) {
        }
        public StringLiteral(Saxon.Hej.str.UnicodeString value) : this (new Saxon.Hej.value.StringValue(value)) {
        }
        public StringLiteral(string value) : this (new Saxon.Hej.value.StringValue(Saxon.Hej.str.StringTool.fromCharSequence(value))) {
        }
        public override Saxon.Hej.value.StringValue getGroundedValue() {
            return (Saxon.Hej.value.StringValue)(base.getGroundedValue());
        }
        public Saxon.Hej.str.UnicodeString getString() {
            return getGroundedValue().getUnicodeStringValue();
        }
        public string stringify() {
            return getGroundedValue().getStringValue();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.StringLiteral stringLiteral = new Saxon.Hej.expr.StringLiteral(getGroundedValue());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, stringLiteral);
            return stringLiteral;
        }
    
    }
}