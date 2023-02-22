using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class LookupExpression : Saxon.Hej.expr.BinaryExpression {
        private bool isClassified = false;
        protected internal bool isArrayLookup = false;
        protected internal bool isMapLookup = false;
        protected internal bool isSingleContainer = false;
        protected internal bool isSingleEntry = false;
        public LookupExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression step) : base (start, Saxon.Hej.expr.parser.Token.QMARK, step) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return ((arg == 0) ? Saxon.Hej.expr.OperandRole.INSPECT : Saxon.Hej.expr.OperandRole.ABSORB);
        }
        public override string getExpressionName() {
            return "lookupExp";
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if (isClassified) {
                if (isArrayLookup) {
                    Saxon.Hej.type.ItemType arrayType = getLhsExpression().getItemType();
                    if ((arrayType is Saxon.Hej.ma.arrays.ArrayItemType)) {
                        return ((Saxon.Hej.ma.arrays.ArrayItemType)(arrayType)).getMemberType().getPrimaryType();
                    }
                } else {
                    if (isMapLookup) {
                        Saxon.Hej.type.ItemType mapType = getLhsExpression().getItemType();
                        if (((mapType is Saxon.Hej.ma.map.RecordTest) && (getRhsExpression() is Saxon.Hej.expr.StringLiteral))) {
                            string fieldName = ((Saxon.Hej.expr.StringLiteral)(getRhsExpression())).stringify();
                            Saxon.Hej.value.SequenceType fieldType = ((Saxon.Hej.ma.map.RecordTest)(mapType)).getFieldType(fieldName);
                            if ((fieldType == null)) {
                                if (((Saxon.Hej.ma.map.RecordTest)(mapType)).isExtensible()) {
                                    return Saxon.Hej.type.AnyItemType.getInstance();
                                } else {
                                    return Saxon.Hej.type.ErrorType.getInstance();
                                }
                            } else {
                                return fieldType.getPrimaryType();
                            }
                        } else {
                            if ((mapType is Saxon.Hej.ma.map.MapType)) {
                                return ((Saxon.Hej.ma.map.MapType)(mapType)).getValueType().getPrimaryType();
                            }
                        }
                    }
                }
            }
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getItemType().getUType();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                return getLhsExpression();
            }
            Saxon.Hej.type.ItemType originalType = getLhsExpression().getItemType();
            getLhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType containerType = getLhsExpression().getItemType();
            isArrayLookup = (containerType is Saxon.Hej.ma.arrays.ArrayItemType);
            bool isTupleLookup = ((containerType is Saxon.Hej.ma.map.TupleType) || (originalType is Saxon.Hej.ma.map.TupleType));
            isMapLookup = ((containerType is Saxon.Hej.ma.map.MapType) || isTupleLookup);
            isSingleContainer = (getLhsExpression().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            if ((!(isArrayLookup) && !(isMapLookup))) {
                if ((((th.relationship(containerType, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE) == Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(containerType, Saxon.Hej.ma.arrays.ArrayItemType.getInstance()) == Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(containerType, Saxon.Hej.type.AnyExternalObjectType.THE_INSTANCE) == Saxon.Hej.type.Affinity.DISJOINT))) {
                    if (Saxon.Hej.value.Cardinality.allowsZero(getLhsExpression().getCardinality())) {
                        visitor.issueWarning(("The left-hand operand of '?' must be a map or an array; the expression can succeed only if the operand is an empty sequence "+containerType), getLocation());
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("The left-hand operand of '?' must be a map or an array; the supplied expression is of type "+containerType), "XPTY0004");
                        err.setLocation(getLocation());
                        err.setIsTypeError(true);
                        err.setFailingExpression(this);
                        throw err;
                    }
                }
            }
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "?", 1);
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.value.SequenceType req = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC.zeroOrMore();
            if (isArrayLookup) {
                req = Saxon.Hej.type.BuiltInAtomicType.INTEGER.zeroOrMore();
            }
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), req, role, visitor));
            isSingleEntry = (getRhsExpression().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            if ((isTupleLookup && (getRhsExpression() is Saxon.Hej.expr.StringLiteral))) {
                Saxon.Hej.ma.map.TupleType tt = (Saxon.Hej.ma.map.TupleType)((((containerType is Saxon.Hej.ma.map.TupleType) ? containerType : originalType)));
                if (!(tt.isExtensible())) {
                    string fieldName = ((Saxon.Hej.expr.StringLiteral)(getRhsExpression())).stringify();
                    if ((tt.getFieldType(fieldName) == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Field '"+fieldName)+"' is not defined in the record type"), "XPTY0004");
                        err.setIsTypeError(true);
                        err.setLocation(getLocation());
                        throw err;
                    }
                }
            }
            isClassified = true;
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().optimize(visitor, contextInfo);
            getRhs().optimize(visitor, contextInfo);
            return this;
        }
        public override double getCost() {
            return (getLhsExpression().getCost()*getRhsExpression().getCost());
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.LookupExpression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.LookupExpression exp = new Saxon.Hej.expr.LookupExpression(getLhsExpression().copy(rebindings), getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.isArrayLookup = isArrayLookup;
            exp.isMapLookup = isMapLookup;
            exp.isSingleEntry = isSingleEntry;
            exp.isSingleContainer = isSingleContainer;
            return exp;
        }
        protected internal override int computeCardinality() {
            if ((isSingleContainer && isSingleEntry)) {
                if (isArrayLookup) {
                    Saxon.Hej.type.ItemType arrayType = getLhsExpression().getItemType();
                    if ((arrayType is Saxon.Hej.ma.arrays.ArrayItemType)) {
                        return ((Saxon.Hej.ma.arrays.ArrayItemType)(arrayType)).getMemberType().getCardinality();
                    }
                } else {
                    if (isMapLookup) {
                        Saxon.Hej.type.ItemType mapType = getLhsExpression().getItemType();
                        if (((mapType is Saxon.Hej.ma.map.RecordTest) && (getRhsExpression() is Saxon.Hej.expr.StringLiteral))) {
                            string fieldName = ((Saxon.Hej.expr.StringLiteral)(getRhsExpression())).stringify();
                            Saxon.Hej.value.SequenceType fieldType = ((Saxon.Hej.ma.map.RecordTest)(mapType)).getFieldType(fieldName);
                            if ((fieldType == null)) {
                                return (((Saxon.Hej.ma.map.RecordTest)(mapType)).isExtensible() ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE : Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
                            } else {
                                return fieldType.getCardinality();
                            }
                        } else {
                            if ((mapType is Saxon.Hej.ma.map.MapType)) {
                                return (Saxon.Hej.value.Cardinality.union(((Saxon.Hej.ma.map.MapType)(mapType)).getValueType().getCardinality(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO));
                            }
                        }
                    }
                }
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.expr.LookupExpression)))) {
                return false;
            }
            Saxon.Hej.expr.LookupExpression p = (Saxon.Hej.expr.LookupExpression)(other);
            return (getLhsExpression().isEqual(p.getLhsExpression()) && getRhsExpression().isEqual(p.getRhsExpression()));
        }
        protected internal override int computeHashCode() {
            return (("LookupExpression".GetHashCode()^getLhsExpression().GetHashCode())^getRhsExpression().GetHashCode());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            if (isArrayLookup) {
                if ((isSingleContainer && isSingleEntry)) {
                    Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(getLhsExpression().evaluateItem(context));
                    Saxon.Hej.value.IntegerValue subscript = (Saxon.Hej.value.IntegerValue)(getRhsExpression().evaluateItem(context));
                    int index = Saxon.Hej.ma.arrays.ArrayFunctionSet.checkSubscript(subscript, array.arrayLength());
                    return array.get((index-1)).iterate();
                } else {
                    if (isSingleEntry) {
                        Saxon.Hej.om.SequenceIterator baseIterator = getLhsExpression().iterate(context);
                        Saxon.Hej.value.IntegerValue subscriptValue = (Saxon.Hej.value.IntegerValue)(getRhsExpression().evaluateItem(context));
                        int subscript = (subscriptValue.asSubscript()-1);
                        return Saxon.Hej.expr.MappingIterator.map(baseIterator, baseItem => {
                            Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(baseItem);
                            if (((subscript>=0) && (subscript<array.arrayLength()))) {
                                return array.get(subscript).iterate();
                            } else {
                                Saxon.Hej.ma.arrays.ArrayFunctionSet.checkSubscript(subscriptValue, array.arrayLength());
                                return null;
                            }
                        } );
                    } else {
                        Saxon.Hej.om.SequenceIterator baseIterator = getLhsExpression().iterate(context);
                        Saxon.Hej.om.GroundedValue rhs = Saxon.Hej.om.SequenceTool.toGroundedValue(getRhsExpression().iterate(context));
                        return Saxon.Hej.expr.MappingIterator.map(baseIterator, baseItem => Saxon.Hej.expr.MappingIterator.map(rhs.iterate(), index => {
                            Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(baseItem);
                            int subscript = (Saxon.Hej.ma.arrays.ArrayFunctionSet.checkSubscript((Saxon.Hej.value.IntegerValue)(index), array.arrayLength())-1);
                            return array.get(subscript).iterate();
                        } ));
                    }
                }
            } else {
                if (isMapLookup) {
                    if ((isSingleContainer && isSingleEntry)) {
                        Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(getLhsExpression().evaluateItem(context));
                        Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                        return optionalGroundedValueIterator(map.get(key));
                    } else {
                        if (isSingleEntry) {
                            Saxon.Hej.om.SequenceIterator baseIterator = getLhsExpression().iterate(context);
                            Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                            return Saxon.Hej.expr.MappingIterator.map(baseIterator, baseItem => optionalGroundedValueIterator(((Saxon.Hej.ma.map.MapItem)(baseItem)).get(key)));
                        } else {
                            Saxon.Hej.om.SequenceIterator baseIterator = getLhsExpression().iterate(context);
                            Saxon.Hej.om.GroundedValue rhs = Saxon.Hej.om.SequenceTool.toGroundedValue(getRhsExpression().iterate(context));
                            return Saxon.Hej.expr.MappingIterator.map(baseIterator, baseItem => Saxon.Hej.expr.MappingIterator.map(rhs.iterate(), index => optionalGroundedValueIterator(((Saxon.Hej.ma.map.MapItem)(baseItem)).get((Saxon.Hej.value.AtomicValue)(index)))));
                        }
                    }
                } else {
                    Saxon.Hej.om.SequenceIterator baseIterator = getLhsExpression().iterate(context);
                    Saxon.Hej.om.GroundedValue rhs = Saxon.Hej.om.SequenceTool.toGroundedValue(getRhsExpression().iterate(context));
                    Saxon.Hej.expr.MappingFunction mappingFunction = Saxon.Hej.expr.SequenceMapper.of(baseItem => {
                        switch (baseItem.getGenre()) {
                          case Saxon.Hej.om.Genre.ARRAY:
                            {
                                Saxon.Hej.expr.MappingFunction arrayAccess = Saxon.Hej.expr.SequenceMapper.of(index => {
                                    if ((index is Saxon.Hej.value.IntegerValue)) {
                                        Saxon.Hej.om.GroundedValue member = ((Saxon.Hej.ma.arrays.ArrayItem)(baseItem)).get(((int)(((Saxon.Hej.value.IntegerValue)(index)).longValue())-1));
                                        return member.iterate();
                                    } else {
                                        Saxon.Hej.trans.XPathException exception = new Saxon.Hej.trans.XPathException((("An item on the LHS of the '?' operator is an array, but a value on the RHS of the operator ("+baseItem.toShortString())+") is not an integer"), "XPTY0004");
                                        exception.setIsTypeError(true);
                                        exception.setLocation(getLocation());
                                        exception.setFailingExpression(this);
                                        throw exception;
                                    }
                                } );
                                Saxon.Hej.om.SequenceIterator rhsIter = rhs.iterate();
                                return new Saxon.Hej.expr.MappingIterator(rhsIter, arrayAccess);
                            }
                          case Saxon.Hej.om.Genre.MAP:
                            {
                                Saxon.Hej.om.SequenceIterator rhsIter = rhs.iterate();
                                return Saxon.Hej.expr.MappingIterator.map(rhsIter, key => optionalGroundedValueIterator(((Saxon.Hej.ma.map.MapItem)(baseItem)).get((Saxon.Hej.value.AtomicValue)(key))));
                            }
                          case Saxon.Hej.om.Genre.EXTERNAL:
                            {
                                if (!(((rhs is Saxon.Hej.value.StringValue)))) {
                                    Saxon.Hej.trans.XPathException exception = new Saxon.Hej.trans.XPathException((("An item on the LHS of the '?' operator is an external object, but a value on the RHS of the operator ("+baseItem.toShortString())+") is not a singleton string"), "XPTY0004");
                                    exception.setIsTypeError(true);
                                    exception.setLocation(getLocation());
                                    exception.setFailingExpression(this);
                                    throw exception;
                                }
                                string key = ((Saxon.Hej.value.StringValue)(rhs)).getStringValue();
                                return config.externalObjectAsMap((Saxon.Hej.value.ObjectValue<object>)(baseItem), key).get((Saxon.Hej.value.StringValue)(rhs)).iterate();
                            }
                          default:
                            {
                                mustBeArrayOrMap(this, baseItem);
                                return null;
                            }
                        }
                    } );
                    return new Saxon.Hej.expr.MappingIterator(baseIterator, mappingFunction);
                }
            }
        }
        private static Saxon.Hej.om.SequenceIterator optionalGroundedValueIterator(Saxon.Hej.om.GroundedValue value) {
            if ((value == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            } else {
                return value.iterate();
            }
        }
        protected internal static void mustBeArrayOrMap(Saxon.Hej.expr.Expression exp, Saxon.Hej.om.Item baseItem) {
            Saxon.Hej.trans.XPathException exception = new Saxon.Hej.trans.XPathException((("The items on the LHS of the '?' operator must be maps or arrays; but value ("+baseItem.toShortString())+") was supplied"), "XPTY0004");
            exception.setIsTypeError(true);
            exception.setLocation(exp.getLocation());
            exception.setFailingExpression(exp);
            throw exception;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("lookup", this);
            getLhsExpression().export(destination);
            getRhsExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            string rhs;
            if ((getRhsExpression() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.expr.Literal lit = (Saxon.Hej.expr.Literal)(getRhsExpression());
                if (((lit is Saxon.Hej.expr.StringLiteral) && Saxon.Hej.om.NameChecker.isValidNCName(((Saxon.Hej.expr.StringLiteral)(lit)).getGroundedValue().codePoints()))) {
                    rhs = ((Saxon.Hej.expr.StringLiteral)(lit)).stringify();
                } else {
                    if ((lit.getGroundedValue() is Saxon.Hej.value.Int64Value)) {
                        rhs = lit.getGroundedValue().ToString();
                    } else {
                        rhs = Saxon.Hej.expr.parser.ExpressionTool.parenthesize(lit);
                    }
                }
            } else {
                rhs = Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getRhsExpression());
            }
            return ((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getLhsExpression())+"?")+rhs);
        }
    
    }
}