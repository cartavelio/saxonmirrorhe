using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class TypeChecker {
        public TypeChecker()  {
        }
        public virtual Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.expr.Expression supplied, Saxon.Hej.value.SequenceType req, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if (supplied.implementsStaticTypeCheck()) {
                return supplied.staticTypeCheck(req, false, roleSupplier, visitor);
            }
            Saxon.Hej.expr.Expression exp = supplied;
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            Saxon.Hej.Configuration config = env.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.ContextItemStaticInfo defaultContextInfo = config.getDefaultContextItemStaticInfo();
            Saxon.Hej.type.ItemType reqItemType = req.getPrimaryType();
            int reqCard = req.getCardinality();
            bool allowsMany = Saxon.Hej.value.Cardinality.allowsMany(reqCard);
            Saxon.Hej.type.ItemType suppliedItemType = null;
            int suppliedCard = -1;
            bool cardOK = (reqCard == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            if (!(cardOK)) {
                suppliedCard = exp.getCardinality();
                cardOK = Saxon.Hej.value.Cardinality.subsumes(reqCard, suppliedCard);
            }
            bool itemTypeOK = (reqItemType is Saxon.Hej.type.AnyItemType);
            if ((reqCard == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                itemTypeOK = true;
            }
            if (!(itemTypeOK)) {
                suppliedItemType = exp.getItemType();
                if (((reqItemType == null) || (suppliedItemType == null))) {
                    throw new Saxon.Ejava.lang.NullPointerException();
                }
                Saxon.Hej.type.Affinity affinity = th.relationship(reqItemType, suppliedItemType);
                itemTypeOK = ((affinity == Saxon.Hej.type.Affinity.SAME_TYPE) || (affinity == Saxon.Hej.type.Affinity.SUBSUMES));
            }
            if (!(itemTypeOK)) {
                if (reqItemType.isPlainType()) {
                    if ((!(suppliedItemType.isPlainType()) && !(((suppliedCard == Saxon.Hej.expr.StaticProperty.EMPTY))))) {
                        if (!(suppliedItemType.isAtomizable(th))) {
                            string shortItemType;
                            if ((suppliedItemType is Saxon.Hej.ma.map.TupleType)) {
                                shortItemType = "a tuple type";
                            } else {
                                if ((suppliedItemType is Saxon.Hej.ma.map.MapType)) {
                                    shortItemType = "a map type";
                                } else {
                                    if ((suppliedItemType is Saxon.Hej.type.FunctionItemType)) {
                                        shortItemType = "a function type";
                                    } else {
                                        if ((suppliedItemType is Saxon.Hej.pattern.NodeTest)) {
                                            shortItemType = "an element type with element-only content";
                                        } else {
                                            shortItemType = suppliedItemType.ToString();
                                        }
                                    }
                                }
                            }
                            Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("An atomic value is required for the "+role.getMessage())+", but the supplied type is ")+shortItemType)+", which cannot be atomized"), "FOTY0013", supplied.getLocation());
                            err.setIsTypeError(true);
                            err.setFailingExpression(supplied);
                            throw err;
                        }
                        if ((exp.getRetainedStaticContext() == null)) {
                            exp.setRetainedStaticContextLocally(env.makeRetainedStaticContext());
                        }
                        Saxon.Hej.expr.Expression cexp = Saxon.Hej.expr.Atomizer.makeAtomizer(exp, roleSupplier);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                        exp = cexp;
                        cexp = exp.simplify();
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                        exp = cexp;
                        suppliedItemType = exp.getItemType();
                        suppliedCard = exp.getCardinality();
                        cardOK = Saxon.Hej.value.Cardinality.subsumes(reqCard, suppliedCard);
                    }
                    if ((suppliedItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && !(((reqItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || reqItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)))))) {
                        if (((Saxon.Hej.type.PlainType)(reqItemType)).isNamespaceSensitive()) {
                            Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("An untyped atomic value cannot be converted to a QName or NOTATION as required for the "+role.getMessage()), "XPTY0117", supplied.getLocation());
                            err.setIsTypeError(true);
                            throw err;
                        }
                        Saxon.Hej.expr.UntypedSequenceConverter cexp = Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, exp, (Saxon.Hej.type.PlainType)(reqItemType));
                        cexp.setRoleDiagnostic(roleSupplier);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                        try {
                            if ((exp is Saxon.Hej.expr.Literal)) {
                                exp = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.om.SequenceTool.toGroundedValue(cexp.iterate(visitor.makeDynamicContext())), exp);
                                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(cexp, exp);
                            } else {
                                exp = cexp;
                            }
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetLocation(exp.getLocation());
                            err.setFailingExpression(supplied);
                            err.setErrorCode(roleSupplier().getErrorCode());
                            err.setIsStaticError(true);
                            throw;
                        }
                        itemTypeOK = true;
                        suppliedItemType = reqItemType;
                    }
                    if (((suppliedItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) && !(((reqItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || reqItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))))) && !(exp.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)))) {
                        Saxon.Hej.expr.Expression conversion;
                        if (((Saxon.Hej.type.PlainType)(reqItemType)).isNamespaceSensitive()) {
                            conversion = Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceRejector(config, exp, (Saxon.Hej.type.PlainType)(reqItemType));
                        } else {
                            Saxon.Hej.expr.UntypedSequenceConverter usc = Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, exp, (Saxon.Hej.type.PlainType)(reqItemType));
                            usc.setRoleDiagnostic(roleSupplier);
                            conversion = usc;
                        }
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, conversion);
                        try {
                            if ((exp is Saxon.Hej.expr.Literal)) {
                                exp = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.om.SequenceTool.toGroundedValue(conversion.iterate(visitor.makeDynamicContext())), exp);
                                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(supplied, exp);
                            } else {
                                exp = conversion;
                            }
                            suppliedItemType = exp.getItemType();
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetLocation(exp.getLocation());
                            err.setIsStaticError(true);
                            throw;
                        }
                    }
                    if ((reqItemType is Saxon.Hej.type.AtomicType)) {
                        int rt = ((Saxon.Hej.type.AtomicType)(reqItemType)).getFingerprint();
                        if (((rt == Saxon.Hej.om.StandardNames.XS_DOUBLE) && (th.relationship(suppliedItemType, Saxon.Hej.type.NumericType.getInstance()) != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT))) {
                            Saxon.Hej.expr.Expression cexp = makePromoterToDouble(exp);
                            if ((cexp is Saxon.Hej.expr.AtomicSequenceConverter)) {
                                ((Saxon.Hej.expr.AtomicSequenceConverter)(cexp)).setRoleDiagnostic(roleSupplier);
                            }
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                            exp = cexp;
                            try {
                                exp = exp.simplify().typeCheck(visitor, defaultContextInfo);
                            } catch (Saxon.Hej.trans.XPathException err) {
                                err.maybeSetLocation(exp.getLocation());
                                err.setIsStaticError(true);
                                throw;
                            }
                            suppliedItemType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                            suppliedCard = -1;
                        } else {
                            if ((((rt == Saxon.Hej.om.StandardNames.XS_FLOAT) && (th.relationship(suppliedItemType, Saxon.Hej.type.NumericType.getInstance()) != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && !(th.isSubType(suppliedItemType, Saxon.Hej.type.BuiltInAtomicType.DOUBLE)))) {
                                Saxon.Hej.expr.Expression cexp = makePromoterToFloat(exp);
                                if ((cexp is Saxon.Hej.expr.AtomicSequenceConverter)) {
                                    ((Saxon.Hej.expr.AtomicSequenceConverter)(cexp)).setRoleDiagnostic(roleSupplier);
                                }
                                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                                exp = cexp;
                                try {
                                    exp = exp.simplify().typeCheck(visitor, defaultContextInfo);
                                } catch (Saxon.Hej.trans.XPathException err) {
                                    err.maybeSetLocation(exp.getLocation());
                                    err.setFailingExpression(supplied);
                                    err.setIsStaticError(true);
                                    throw;
                                }
                                suppliedItemType = Saxon.Hej.type.BuiltInAtomicType.FLOAT;
                                suppliedCard = -1;
                            }
                        }
                        if (((rt == Saxon.Hej.om.StandardNames.XS_STRING) && th.isSubType(suppliedItemType, Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                            itemTypeOK = true;
                            Saxon.Hej.expr.Expression cexp = makePromoterToString(exp);
                            if ((cexp is Saxon.Hej.expr.AtomicSequenceConverter)) {
                                ((Saxon.Hej.expr.AtomicSequenceConverter)(cexp)).setRoleDiagnostic(roleSupplier);
                            }
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                            exp = cexp;
                            try {
                                exp = exp.simplify().typeCheck(visitor, defaultContextInfo);
                            } catch (Saxon.Hej.trans.XPathException err) {
                                err.maybeSetLocation(exp.getLocation());
                                err.setFailingExpression(supplied);
                                err.setIsStaticError(true);
                                throw;
                            }
                            suppliedItemType = Saxon.Hej.type.BuiltInAtomicType.STRING;
                            suppliedCard = -1;
                        }
                        if (((!(itemTypeOK) && (reqItemType.getBasicAlphaCode().Length>2)) && (visitor.getStaticContext().getXPathVersion()>=40))) {
                            if (reqItemType.getUType().overlaps(suppliedItemType.getUType())) {
                                itemTypeOK = true;
                                Saxon.Hej.expr.Expression cexp = makeDownCaster(exp, (Saxon.Hej.type.AtomicType)(reqItemType), config);
                                if ((cexp is Saxon.Hej.expr.AtomicSequenceConverter)) {
                                    ((Saxon.Hej.expr.AtomicSequenceConverter)(cexp)).setRoleDiagnostic(roleSupplier);
                                }
                                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                                exp = cexp;
                                try {
                                    exp = exp.simplify().typeCheck(visitor, defaultContextInfo);
                                } catch (Saxon.Hej.trans.XPathException err) {
                                    err.maybeSetLocation(exp.getLocation());
                                    err.setFailingExpression(supplied);
                                    err.setIsStaticError(true);
                                    throw;
                                }
                                suppliedItemType = reqItemType;
                            }
                        }
                    }
                } else {
                    if ((((reqItemType is Saxon.Hej.type.FunctionItemType) && !(((Saxon.Hej.type.FunctionItemType)(reqItemType)).isMapType())) && !(((Saxon.Hej.type.FunctionItemType)(reqItemType)).isArrayType()))) {
                        Saxon.Hej.type.Affinity r = th.relationship(suppliedItemType, th.getGenericFunctionItemType());
                        if ((r != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                            if (!(((suppliedItemType is Saxon.Hej.type.FunctionItemType)))) {
                                exp = new Saxon.Hej.expr.ItemChecker(exp, th.getGenericFunctionItemType(), roleSupplier);
                                suppliedItemType = th.getGenericFunctionItemType();
                            }
                            exp = makeFunctionSequenceCoercer(exp, (Saxon.Hej.type.FunctionItemType)(reqItemType), roleSupplier);
                            itemTypeOK = true;
                        }
                    } else {
                        if (((reqItemType is Saxon.Hej.type.JavaExternalObjectType) && (reqCard == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                            if (typeof(Saxon.Hej.om.Sequence).IsAssignableFrom(((Saxon.Hej.type.JavaExternalObjectType)(reqItemType)).getJavaClass())) {
                                itemTypeOK = true;
                            } else {
                                if ((supplied is Saxon.Hej.expr.FunctionCall)) {
                                    if (((Saxon.Hej.expr.FunctionCall)(supplied)).adjustRequiredType((Saxon.Hej.type.JavaExternalObjectType)(reqItemType))) {
                                        itemTypeOK = true;
                                        cardOK = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((itemTypeOK && cardOK)) {
                return exp;
            }
            if ((suppliedCard == -1)) {
                suppliedCard = exp.getCardinality();
                if (!(cardOK)) {
                    cardOK = Saxon.Hej.value.Cardinality.subsumes(reqCard, suppliedCard);
                }
            }
            if ((cardOK && (suppliedCard == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                return exp;
            }
            if (((suppliedCard == Saxon.Hej.expr.StaticProperty.EMPTY) && ((((reqCard&Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) == 0)))) {
                Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode(), supplied.getLocation());
                err.setIsTypeError(role.isTypeError());
                err.setFailingExpression(supplied);
                throw err;
            }
            Saxon.Hej.type.Affinity relation = (itemTypeOK ? /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY : th.relationship(suppliedItemType, reqItemType));
            if ((reqCard == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                relation = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            }
            if ((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                if ((Saxon.Hej.value.Cardinality.allowsZero(suppliedCard) && Saxon.Hej.value.Cardinality.allowsZero(reqCard))) {
                    if ((suppliedCard != Saxon.Hej.expr.StaticProperty.EMPTY)) {
                        string msg = role.composeErrorMessage(reqItemType, supplied, th);
                        msg += ". The expression can succeed only if the supplied value is an empty sequence.";
                        visitor.issueWarning(msg, Saxon.Hej.trans.SaxonErrorCode.SXWN9026, supplied.getLocation());
                    }
                } else {
                    string msg = role.composeErrorMessage(reqItemType, supplied, th);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, role.getErrorCode(), supplied.getLocation());
                    err.setIsTypeError(role.isTypeError());
                    err.setFailingExpression(supplied);
                    throw err;
                }
            }
            if (!((((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                if ((exp is Saxon.Hej.expr.Literal)) {
                    if (req.matches(((Saxon.Hej.expr.Literal)(exp)).getGroundedValue(), th)) {
                        return exp;
                    }
                    Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                    string msg = role.composeErrorMessage(reqItemType, supplied, th);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, role.getErrorCode(), supplied.getLocation());
                    err.setIsTypeError(role.isTypeError());
                    throw err;
                } else {
                    Saxon.Hej.expr.Expression cexp = new Saxon.Hej.expr.ItemChecker(exp, reqItemType, roleSupplier);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                    exp = cexp;
                }
            }
            if (!(cardOK)) {
                if ((exp is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Required cardinality of "+role.getMessage())+" is ")+Saxon.Hej.value.Cardinality.describe(reqCard))+"; supplied value has cardinality ")+Saxon.Hej.value.Cardinality.describe(suppliedCard)), role.getErrorCode(), supplied.getLocation());
                    err.setIsTypeError(role.isTypeError());
                    throw err;
                } else {
                    Saxon.Hej.expr.Expression cexp = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(exp, reqCard, roleSupplier);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, cexp);
                    exp = cexp;
                }
            }
            return exp;
        }
        public virtual Saxon.Hej.expr.Expression makeArithmeticExpression(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.expr.Expression rhs) {
            return new Saxon.Hej.expr.ArithmeticExpression(lhs, @operator, rhs);
        }
        public virtual Saxon.Hej.expr.Expression makeGeneralComparison(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.expr.Expression rhs) {
            return new Saxon.Hej.expr.GeneralComparison20(lhs, @operator, rhs);
        }
        public virtual Saxon.Hej.expr.Expression processValueOf(Saxon.Hej.expr.Expression select, Saxon.Hej.Configuration config) {
            return select;
        }
        private static Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.FunctionItemType reqItemType, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) {
            return reqItemType.makeFunctionSequenceCoercer(exp, role);
        }
        private Saxon.Hej.expr.Expression makeDownCaster(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.AtomicType reqItemType, Saxon.Hej.Configuration config) {
            return Saxon.Hej.expr.AtomicSequenceConverter.makeDownCaster(exp, reqItemType, config);
        }
        public static Saxon.Hej.expr.Expression strictTypeCheck(Saxon.Hej.expr.Expression supplied, Saxon.Hej.value.SequenceType req, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.Expression exp = supplied;
            Saxon.Hej.type.TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType reqItemType = req.getPrimaryType();
            int reqCard = req.getCardinality();
            Saxon.Hej.type.ItemType suppliedItemType = null;
            int suppliedCard = -1;
            bool cardOK = (reqCard == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            if (!(cardOK)) {
                suppliedCard = exp.getCardinality();
                cardOK = Saxon.Hej.value.Cardinality.subsumes(reqCard, suppliedCard);
            }
            bool itemTypeOK = (req.getPrimaryType() is Saxon.Hej.type.AnyItemType);
            if (!(itemTypeOK)) {
                suppliedItemType = exp.getItemType();
                Saxon.Hej.type.Affinity affinity = th.relationship(reqItemType, suppliedItemType);
                itemTypeOK = ((affinity == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (affinity == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES));
            }
            if ((itemTypeOK && cardOK)) {
                return exp;
            }
            if ((suppliedCard == -1)) {
                if ((suppliedItemType is Saxon.Hej.type.ErrorType)) {
                    suppliedCard = Saxon.Hej.expr.StaticProperty.EMPTY;
                } else {
                    suppliedCard = exp.getCardinality();
                }
                if (!(cardOK)) {
                    cardOK = Saxon.Hej.value.Cardinality.subsumes(reqCard, suppliedCard);
                }
            }
            if ((cardOK && (suppliedCard == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                return exp;
            }
            if ((suppliedItemType == null)) {
                suppliedItemType = exp.getItemType();
            }
            if (((suppliedCard == Saxon.Hej.expr.StaticProperty.EMPTY) && ((((reqCard&Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) == 0)))) {
                Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode(), supplied.getLocation());
                err.setIsTypeError(role.isTypeError());
                throw err;
            }
            Saxon.Hej.type.Affinity relation = th.relationship(suppliedItemType, reqItemType);
            if ((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                if ((Saxon.Hej.value.Cardinality.allowsZero(suppliedCard) && Saxon.Hej.value.Cardinality.allowsZero(reqCard))) {
                    if ((suppliedCard != Saxon.Hej.expr.StaticProperty.EMPTY)) {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                        string msg = (((((((("Required item type of "+role.getMessage())+" is ")+reqItemType)+"; supplied value (")+supplied.toShortString())+") has item type ")+suppliedItemType)+". The expression can succeed only if the supplied value is an empty sequence.");
                        env.issueWarning(msg, Saxon.Hej.trans.SaxonErrorCode.SXWN9026, supplied.getLocation());
                    }
                } else {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                    string msg = role.composeErrorMessage(reqItemType, supplied, th);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, role.getErrorCode(), supplied.getLocation());
                    err.setIsTypeError(role.isTypeError());
                    throw err;
                }
            }
            if (!((((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                Saxon.Hej.expr.Expression cexp = new Saxon.Hej.expr.ItemChecker(exp, reqItemType, roleSupplier);
                cexp.adoptChildExpression(exp);
                exp = cexp;
            }
            if (!(cardOK)) {
                if ((exp is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Required cardinality of "+role.getMessage())+" is ")+Saxon.Hej.value.Cardinality.describe(reqCard))+"; supplied value has cardinality ")+Saxon.Hej.value.Cardinality.describe(suppliedCard)), role.getErrorCode(), supplied.getLocation());
                    err.setIsTypeError(role.isTypeError());
                    throw err;
                } else {
                    Saxon.Hej.expr.Expression cexp = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(exp, reqCard, roleSupplier);
                    cexp.adoptChildExpression(exp);
                    exp = cexp;
                }
            }
            return exp;
        }
        public static Saxon.Hej.trans.XPathException testConformance(Saxon.Hej.om.Sequence val, Saxon.Hej.value.SequenceType requiredType, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.ItemType reqItemType = requiredType.getPrimaryType();
            Saxon.Hej.om.SequenceIterator iter = val.iterate();
            int count = 0;
            for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                count++;
                if (!(reqItemType.matches(item, context.getConfiguration().getTypeHierarchy()))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Required type is "+reqItemType)+"; supplied value has type ")+Saxon.Hej.type.UType.getUType(val.materialize())));
                    err.setIsTypeError(true);
                    err.setErrorCode("XPTY0004");
                    return err;
                }
            }
            int reqCardinality = requiredType.getCardinality();
            if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(reqCardinality)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Required type does not allow empty sequence, but supplied value is empty");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                return err;
            }
            if (((count>1) && !(Saxon.Hej.value.Cardinality.allowsMany(reqCardinality)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Required type requires a singleton sequence; supplied value contains "+count)+" items"));
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                return err;
            }
            if (((count>0) && (reqCardinality == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Required type requires an empty sequence, but supplied value is non-empty");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                return err;
            }
            return null;
        }
        public static Saxon.Hej.trans.XPathException ebvError(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.TypeHierarchy th) {
            if (Saxon.Hej.value.Cardinality.allowsZero(exp.getCardinality())) {
                return null;
            }
            Saxon.Hej.type.ItemType t = exp.getItemType();
            if ((((((((th.relationship(t, Saxon.Hej.type.Type.NODE_TYPE) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(t, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(t, Saxon.Hej.type.BuiltInAtomicType.STRING) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(t, Saxon.Hej.type.BuiltInAtomicType.ANY_URI) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(t, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(t, Saxon.Hej.type.NumericType.getInstance()) == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) && !(((t is Saxon.Hej.type.JavaExternalObjectType))))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Effective boolean value is defined only for sequences containing "+"booleans, strings, numbers, URIs, or nodes"));
                err.setErrorCode("FORG0006");
                err.setIsTypeError(true);
                return err;
            }
            return null;
        }
        private static Saxon.Hej.expr.Expression makePromoterToDouble(Saxon.Hej.expr.Expression exp) {
            return makePromoter(exp, new Saxon.Hej.type.Converter.PromoterToDouble(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE);
        }
        private static Saxon.Hej.expr.Expression makePromoterToFloat(Saxon.Hej.expr.Expression exp) {
            return makePromoter(exp, new Saxon.Hej.type.Converter.PromoterToFloat(), Saxon.Hej.type.BuiltInAtomicType.FLOAT);
        }
        private static Saxon.Hej.expr.Expression makePromoterToString(Saxon.Hej.expr.Expression exp) {
            return makePromoter(exp, new Saxon.Hej.type.Converter.ToStringConverter(), Saxon.Hej.type.BuiltInAtomicType.STRING);
        }
        private static Saxon.Hej.expr.Expression makePromoter(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.Converter converter, Saxon.Hej.type.BuiltInAtomicType type) {
            Saxon.Hej.lib.ConversionRules rules = exp.getConfiguration().getConversionRules();
            converter.setConversionRules(rules);
            if (((exp is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(exp)).getGroundedValue() is Saxon.Hej.value.AtomicValue))) {
                Saxon.Hej.type.ConversionResult result = converter.convert((Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(exp)).getGroundedValue()));
                if ((result is Saxon.Hej.value.AtomicValue)) {
                    Saxon.Hej.expr.Literal converted = Saxon.Hej.expr.Literal.makeLiteral((Saxon.Hej.value.AtomicValue)(result), exp);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, converted);
                    return converted;
                }
            }
            Saxon.Hej.expr.AtomicSequenceConverter asc = new Saxon.Hej.expr.AtomicSequenceConverter(exp, type);
            asc.setConverter(converter);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, asc);
            return asc;
        }
    
    }
}