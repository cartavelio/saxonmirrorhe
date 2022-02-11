using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForMemberClause : Saxon.Hej.expr.flwor.ForClause {
        public ForMemberClause()  {
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.FOR_MEMBER;
        }
        public override Saxon.Hej.expr.flwor.ForMemberClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.ForMemberClause f2 = new Saxon.Hej.expr.flwor.ForMemberClause();
            f2.setLocation(getLocation());
            f2.setPackageData(getPackageData());
            f2.rangeVariable = rangeVariable.copy();
            if ((positionVariable != null)) {
                f2.positionVariable = positionVariable.copy();
            }
            f2.initSequence(flwor, getSequence().copy(rebindings));
            f2.allowsEmpty = allowsEmpty;
            return f2;
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.value.SequenceType decl = rangeVariable.getRequiredType();
            if ((allowsEmpty && !(Saxon.Hej.value.Cardinality.allowsZero(decl.getCardinality())))) {
                Saxon.Hej.expr.parser.RoleDiagnostic emptyRole = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, rangeVariable.getVariableQName().getDisplayName(), 0);
                Saxon.Hej.expr.Expression checker = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(getSequence(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE, emptyRole);
                setSequence(checker);
            }
            Saxon.Hej.value.SequenceType sequenceType = Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.ma.arrays.ArrayItemType(decl), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FOR_MEMBER, rangeVariable.getVariableQName().getDisplayName(), 0);
            setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), sequenceType, role, visitor.getStaticContext()));
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.ForMemberClausePull(@base, this);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.ForMemberClausePush(output, destination, this);
        }
        public override bool addPredicate(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression condition) {
            return false;
        }
        public override void refineVariableType(Saxon.Hej.expr.parser.ExpressionVisitor visitor, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, Saxon.Hej.expr.Expression returnExpr) {
            Saxon.Hej.type.ItemType actualItemType = getSequence().getItemType();
            if ((actualItemType is Saxon.Hej.ma.arrays.ArrayItemType)) {
                Saxon.Hej.value.SequenceType memberType = ((Saxon.Hej.ma.arrays.ArrayItemType)(actualItemType)).getMemberType();
                foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                    @ref.refineVariableType(memberType.getPrimaryType(), memberType.getCardinality(), null, getSequence().getSpecialProperties());
                }
            }
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet varPath = getSequence().addToPathMap(pathMap, pathMapNodeSet);
            pathMap.registerPathForVariable(rangeVariable, varPath);
        }
    
    }
}