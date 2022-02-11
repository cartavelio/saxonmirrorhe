using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class TokenMatchOperand : Saxon.Eej.ee.trans.Characteristic {
        internal Saxon.Hej.expr.Expression exp;
        public TokenMatchOperand(Saxon.Hej.expr.Expression exp)  {
            this.exp = exp;
        }
        public override Saxon.Hej.z.IntSet evaluate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = exp.iterate(context);
            Saxon.Hej.z.IntHashSet keys = new Saxon.Hej.z.IntHashSet();
            Saxon.Hej.lib.StringCollator collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            Saxon.Hej.om.SequenceTool.supply(iter, item => {
                Saxon.Hej.value.Whitespace.Tokenizer seq = new Saxon.Hej.value.Whitespace.Tokenizer(item.getUnicodeStringValue());
                Saxon.Hej.om.Item token;
                while (((token = ((/*1430*/Saxon.Hej.value.StringValue)seq.next())) != null)) {
                    keys.add(collator.getCollationKey(token.getUnicodeStringValue()).GetHashCode());
                }
            } );
            return keys;
        }
        public override int GetHashCode() {
            return exp.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Eej.ee.trans.TokenMatchOperand) && exp.isEqual(((Saxon.Eej.ee.trans.TokenMatchOperand)(obj)).exp));
        }
        public override string ToString() {
            return exp.ToString();
        }
    
    }
}