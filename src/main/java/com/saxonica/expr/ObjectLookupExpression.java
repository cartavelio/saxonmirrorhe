package com.saxonica.expr;

import com.saxonica.functions.extfn.ObjectMap;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.hof.CurriedFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.Map;
import java.util.function.Supplier;

/**
 * Saxon extension: a lookup expression of the form X?Y where X is a Java external object and Y
 * is a method defined on that object
 */

public class ObjectLookupExpression extends LookupExpression {

    private FunctionItem reflexiveFunction = null;

    public ObjectLookupExpression(Expression start, Expression step) {
        super(start, step);
    }

    @Override
    public final ItemType getItemType() {
        ItemType objectType = getLhsExpression().getItemType();
        if (objectType instanceof JavaExternalObjectType && getRhsExpression() instanceof StringLiteral) {
            String fieldName = ((StringLiteral) getRhsExpression()).stringify();
            Map<String, FunctionItem> methodMap =
                    getConfiguration().makeMethodMap(((JavaExternalObjectType) objectType).getJavaClass(), fieldName);
            FunctionItem f = methodMap.get(fieldName);
            if (f != null) {
                return f.getFunctionItemType();
            } else {
                return ErrorType.getInstance();
            }
        }
        return AnyItemType.getInstance();
    }

    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Configuration config = visitor.getConfiguration();

        // Check the first operand
        getLhs().typeCheck(visitor, contextInfo);

        isSingleContainer = getLhsExpression().getCardinality() == StaticProperty.EXACTLY_ONE;


        // Now check the second operand

        getRhs().typeCheck(visitor, contextInfo);
        Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.BINARY_EXPR, "?", 1);
        TypeChecker tc = config.getTypeChecker(false);
        SequenceType req = BuiltInAtomicType.STRING.oneOrMore();
        setRhsExpression(tc.staticTypeCheck(getRhsExpression(), req, role, visitor));
        isSingleEntry = getRhsExpression().getCardinality() == StaticProperty.EXACTLY_ONE;

        if (getRhsExpression() instanceof StringLiteral) {
            ItemType containerType = getLhsExpression().getItemType();
            JavaExternalObjectType target = (JavaExternalObjectType) containerType;
            Class<?> theClass = target.getJavaClass();
            String methodName = ((StringLiteral) getRhsExpression()).stringify();
            if (methodName.equals("this")) {
                // test case QT3extra reflex-055; we should optimize this
                return this;
            }
            Map<String, FunctionItem> methodMap = ObjectMap.makeMethodMap(config, theClass, methodName);
            FunctionItem f = methodMap.get(methodName);
            if (f == null) {
                XPathException err = new XPathException("No unique method " + methodName + " found in class " + theClass, "XPTY0004");
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            } else {
                reflexiveFunction = f;
            }
        }
        return this;
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings a mutable list of (old binding, new binding) pairs
     *                   that is used to update the bindings held in any
     *                   local variable references that are copied.
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public LookupExpression copy(RebindingMap rebindings) {
        ObjectLookupExpression exp = new ObjectLookupExpression(
                getLhsExpression().copy(rebindings), getRhsExpression().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        exp.isArrayLookup = false;
        exp.isMapLookup = false;
        exp.isSingleEntry = isSingleEntry;
        exp.isSingleContainer = isSingleContainer;
        return exp;
    }

    /**
     * Determine the static cardinality of the expression
     */

    @Override
    protected int computeCardinality() {
        if (isSingleContainer && isSingleEntry) {
            return StaticProperty.ALLOWS_ZERO_OR_ONE;
        } else {
            return StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
    }

    /**
     * Iterate the lookup-expression in a given context
     *
     * @param context the evaluation context
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(final XPathContext context) throws XPathException {
        Configuration config = context.getConfiguration();

        SequenceIterator baseIterator = getLhsExpression().iterate(context);
        if (reflexiveFunction != null) {
            return ItemMappingIterator.map(baseIterator, item -> {
                Sequence[] boundArgs = new Sequence[reflexiveFunction.getArity()];
                boundArgs[0] = item;
                return new CurriedFunction(reflexiveFunction, boundArgs);
            });
        } else {
            GroundedValue rhs = SequenceTool.toGroundedValue(getRhsExpression().iterate(context));
            String key = rhs.getUnicodeStringValue().toString();
            return ItemMappingIterator.map(baseIterator,
                    item -> (Item) config.externalObjectAsMap((ObjectValue<?>) item, key).get((StringValue) rhs));
        }

    }


}

// Copyright (c) 2018-2022 Saxonica Limited