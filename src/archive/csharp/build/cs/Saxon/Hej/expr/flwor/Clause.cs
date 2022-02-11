using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public abstract class Clause {
        public enum ClauseName {
            FOR,
            LET,
            WINDOW,
            GROUP_BY,
            COUNT,
            ORDER_BY,
            WHERE,
            TRACE,
            FOR_MEMBER,
        }
        private Saxon.Hej.s9api.Location location;
        private Saxon.Hej.expr.PackageData packageData;
        private bool repeated;
        public Saxon.Hej.s9api.Location getLocation() {
            return location;
        }
        public void setLocation(Saxon.Hej.s9api.Location locationId) {
            this.location = locationId;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData pd) {
            this.packageData = pd;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return packageData.getConfiguration();
        }
        public virtual void setRepeated(bool repeated) {
            this.repeated = repeated;
        }
        public bool isRepeated() {
            return repeated;
        }
        public abstract Saxon.Hej.expr.flwor.Clause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings);
        public virtual void optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
        }
        public virtual void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
        }
        public abstract Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context);
        public abstract Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context);
        public abstract void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor);
        public abstract void explain(Saxon.Hej.trace.ExpressionPresenter @out);
        public virtual Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            return new Saxon.Hej.expr.flwor.LocalVariableBinding[0];
        }
        public virtual void gatherVariableReferences(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Binding binding, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> refs) {
        }
        public virtual bool containsNonInlineableVariableReference(Saxon.Hej.expr.Binding binding) {
            return false;
        }
        public virtual void refineVariableType(Saxon.Hej.expr.parser.ExpressionVisitor visitor, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, Saxon.Hej.expr.Expression returnExpr) {
        }
        public abstract void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet);
        public abstract Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey();
        public virtual string toShortString() {
            return ToString();
        }
    
    }
}