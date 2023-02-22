using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.compat {
    public class TypeChecker10 : Saxon.Hej.expr.parser.TypeChecker {
        public TypeChecker10()  {
        }
        public override Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.expr.Expression supplied, Saxon.Hej.value.SequenceType req, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if (supplied.implementsStaticTypeCheck()) {
                return supplied.staticTypeCheck(req, true, roleSupplier, visitor);
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if ((!(Saxon.Hej.value.Cardinality.allowsMany(req.getCardinality())) && Saxon.Hej.value.Cardinality.allowsMany(supplied.getCardinality()))) {
                Saxon.Hej.expr.Expression cexp = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(supplied);
                cexp.adoptChildExpression(supplied);
                supplied = cexp;
            }
            Saxon.Hej.type.ItemType reqItemType = req.getPrimaryType();
            if (((req.getPrimaryType().Equals(Saxon.Hej.type.BuiltInAtomicType.STRING) && !(Saxon.Hej.value.Cardinality.allowsMany(req.getCardinality()))) && !(th.isSubType(supplied.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING)))) {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(config);
                Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("string", rsc, supplied);
                try {
                    return fn.typeCheck(visitor, config.getDefaultContextItemStaticInfo());
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(supplied.getLocation());
                    err.setIsStaticError(true);
                    throw;
                }
            }
            if ((reqItemType.Equals(Saxon.Hej.type.NumericType.getInstance()) || ((reqItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE) && !(Saxon.Hej.value.Cardinality.allowsMany(req.getCardinality()))) && !(th.isSubType(supplied.getItemType(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE))))) {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(config);
                Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("number", rsc, supplied);
                try {
                    return fn.typeCheck(visitor, config.getDefaultContextItemStaticInfo());
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(supplied.getLocation());
                    err.setIsStaticError(true);
                    throw;
                }
            }
            return base.staticTypeCheck(supplied, req, roleSupplier, visitor);
        }
        public override Saxon.Hej.expr.Expression makeArithmeticExpression(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.expr.Expression rhs) {
            return new Saxon.Hej.expr.compat.ArithmeticExpression10(lhs, @operator, rhs);
        }
        public override Saxon.Hej.expr.Expression makeGeneralComparison(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.expr.Expression rhs) {
            return new Saxon.Hej.expr.compat.GeneralComparison10(lhs, @operator, rhs);
        }
        public override Saxon.Hej.expr.Expression processValueOf(Saxon.Hej.expr.Expression select, Saxon.Hej.Configuration config) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (!(select.getItemType().isPlainType())) {
                select = Saxon.Hej.expr.Atomizer.makeAtomizer(select, null);
            }
            if (Saxon.Hej.value.Cardinality.allowsMany(select.getCardinality())) {
                select = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(select);
            }
            if (!(th.isSubType(select.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                select = new Saxon.Hej.expr.AtomicSequenceConverter(select, Saxon.Hej.type.BuiltInAtomicType.STRING);
                ((Saxon.Hej.expr.AtomicSequenceConverter)(select)).allocateConverterStatically(config, false);
            }
            return select;
        }
    
    }
}