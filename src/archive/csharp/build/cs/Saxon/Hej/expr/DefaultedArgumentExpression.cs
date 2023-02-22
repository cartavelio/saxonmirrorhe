using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class DefaultedArgumentExpression : Saxon.Hej.expr.PseudoExpression {
        public DefaultedArgumentExpression()  {
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return this;
        }
        public override string getExpressionName() {
            return "defaultValue";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}