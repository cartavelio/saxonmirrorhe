package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a VariableReference
 */
public class VariableReferenceCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate a VariableReference as an Iterator
     * Precondition: none.
     * Postcondition: the stack contains an Iterator representing the evaluated variable
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     * @return details of the iterator left on stack
     */
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "VariableReferenceCompiler-Iterator");
        genEvaluateVariable(compiler, ga, (VariableReference)expression);

        ga.invokeInstanceMethod(Sequence.class, "iterate");

        SequenceType declaredType = ((VariableReference) expression).getBinding().getRequiredType();
        ItemType itemType = declaredType.getPrimaryType();
        Class<? extends Item> returnClass = classForItemType(itemType);
        return new IteratorDetails(SequenceIterator.class, returnClass);
    }

    /**
     * Generate bytecode to evaluate a VariableReference as an Sequence
     * Precondition: none.
     * Postcondition: the stack contains a Sequence representing the evaluated variable
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public static void compileToSequence(CompilerService compiler,
                                  VariableReference expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "VariableReferenceCompiler-Sequence");
        genEvaluateVariable(compiler, ga, expression);
    }



    public static void genEvaluateVariable(CompilerService compiler, Generator ga, VariableReference expression) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        if (expression instanceof GlobalVariableReference) {
            GlobalVariableReference varRef = (GlobalVariableReference) expression;
            if (varRef.getBindingSlot() >= 0) {
                // Code for use with XSLT packaging
                // Stack:
                compiler.generateGetContext();
                // Stack: XPathContext
                ga.dup();
                // Stack: XPathContext XPathContext
                ga.push(varRef.getBindingSlot());
                // Stack: XPathContext XPathContext int
                ga.invokeInstanceMethod(XPathContext.class, "getTargetComponent", Integer.TYPE);
                ga.dup();
                ga.invokeInstanceMethod(Component.class, "isHiddenAbstractComponent");
                LabelInfo notAbstract = methodInfo.newLabel("notAbstract");
                ga.ifFalse(notAbstract);

                compiler.generateDynamicError("Cannot evaluate an abstract variable (" +
                                                      varRef.getVariableName() +
                                                      ") with no implementation", "XTDE3052", expression.getLocation(), false);
                ga.throwException();

                methodInfo.placeLabel(notAbstract);

                // Stack: XPathContext Component
//            GlobalVariable p = (GlobalVariable)target.getProcedure();
//            return p.evaluateVariable(c, target);
                ga.dup();
                // Stack: XPathContext Component Component
                ga.invokeInstanceMethod(Component.class, "getActor");
                // Stack: XPathContext Component Procedure
                ga.checkClass(GlobalVariable.class);
                // Stack: XPathContext Component GlobalVariable
                ga.dupX2();
                // Stack: GlobalVariable XPathContext Component GlobalVariable
                ga.pop();
                // Stack: GlobalVariable XPathContext Component
                ga.invokeInstanceMethod(GlobalVariable.class, "evaluateVariable", XPathContext.class, Component.class);
                // Stack: GroundedValue
            } else {
                // code used with XQuery
//                GlobalVariable b = (GlobalVariable)binding;
//                return b.evaluateVariable(c, b.getDeclaringComponent());
                GlobalVariable var = (GlobalVariable) expression.getBinding();
                allocateStatic(compiler, var);
                compiler.generateGetContext();
                allocateStatic(compiler, var.getDeclaringComponent());
                ga.invokeInstanceMethod(GlobalVariable.class, "evaluateVariable", XPathContext.class, Component.class);
            }
        } else {
            //return c.getStackFrame().slots[slotNumber];
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getStackFrame");
            ga.invokeInstanceMethod(StackFrame.class, "getStackFrameValues");
            ga.push(((LocalVariableReference)expression).getSlotNumber());
            ga.arrayLoad(Type.getType(Sequence.class));
//            allocateStatic(compiler, expression.getBinding());
//            compiler.generateGetContext();
//            ga.invokeInstanceMethod(Binding.class, "evaluateVariable", XPathContext.class);
        }
    }

    /**
     * Generate bytecode to evaluate a VariableReference as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the evaluated variable
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler,
                                               Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "VariableReferenceCompiler-Item");
        genEvaluateVariable(compiler, ga, (VariableReference) expression);
        SequenceType declaredType = ((VariableReference)expression).getBinding().getRequiredType();
        ItemType itemType = declaredType.getPrimaryType();
        Class<? extends Item> returnClass = classForItemType(itemType);
        ga.invokeInstanceMethod(Sequence.class, "head");
        ga.checkClass(returnClass);
        return returnClass;
    }
    
    public static Class<? extends Item> classForItemType(ItemType type) {
        switch (type.getGenre()) {
            case NODE:
                return NodeInfo.class;
            case FUNCTION:
                return FunctionItem.class;
            case ATOMIC:
                switch (type.getPrimitiveType()) {
                    case StandardNames.XS_STRING:
                    case StandardNames.XS_UNTYPED_ATOMIC:
                        return StringValue.class;
                    case StandardNames.XS_BOOLEAN:
                        return BooleanValue.class;
                    case StandardNames.XS_DECIMAL:
                        return DecimalValue.class;
                    case StandardNames.XS_FLOAT:
                        return FloatValue.class;
                    case StandardNames.XS_DOUBLE:
                        return DoubleValue.class;
                    case StandardNames.XS_DURATION:
                        return DurationValue.class;
                    case StandardNames.XS_DATE_TIME:
                        return DateTimeValue.class;
                    case StandardNames.XS_TIME:
                        return TimeValue.class;
                    case StandardNames.XS_DATE:
                        return DateValue.class;
                    case StandardNames.XS_G_YEAR_MONTH:
                        return GYearMonthValue.class;
                    case StandardNames.XS_G_YEAR:
                        return GYearValue.class;
                    case StandardNames.XS_G_MONTH_DAY:
                        return GMonthDayValue.class;
                    case StandardNames.XS_G_DAY:
                        return GDayValue.class;
                    case StandardNames.XS_G_MONTH:
                        return GMonthValue.class;
                    case StandardNames.XS_HEX_BINARY:
                        return HexBinaryValue.class;
                    case StandardNames.XS_BASE64_BINARY:
                        return Base64BinaryValue.class;
                    case StandardNames.XS_ANY_URI:
                        return AnyURIValue.class;
                    case StandardNames.XS_QNAME:
                        return QNameValue.class;
                    case StandardNames.XS_NOTATION:
                        return NotationValue.class;
                    case StandardNames.XS_INTEGER:
                        return IntegerValue.class;
                    default:
                        return AtomicValue.class;
                }
            case EXTERNAL:
                return ObjectValue.class;
            default:
                return Item.class;
        }
    }




    /**
     * Generate bytecode to evaluate a VariableReference in push mode
     * Precondition: none.
     * Postcondition: the stack is unchanged
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    public void compileToPush(CompilerService compiler,
                              Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "VariableReferenceCompiler-Push");
        genEvaluateVariable(compiler, ga, (VariableReference)expression);
        int actualVar = methodInfo.allocateLocal(Sequence.class);
        ga.storeLocal(actualVar);

        LabelInfo done = methodInfo.newLabel("doneVarRef");
        ga.loadLocal(actualVar);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        int itrVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(itrVar);
        LabelInfo loop = methodInfo.placeNewLabel("loopVarRef");
        ga.loadLocal(itrVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(done.label());
        compiler.generateGetOutputter();
        ga.swap();
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class);
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();
        methodInfo.releaseLocal(itrVar);
        methodInfo.releaseLocal(actualVar);
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Binding binding = ((VariableReference) expression).getBinding();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        visitAnnotation(compiler, "VariableReferenceCompiler-toBoolean");
        ItemType type = expression.getItemType();
        Affinity rel = th.relationship(type, AnyNodeTest.getInstance());
        boolean isNode = rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMED_BY;
        boolean isNotNode = rel == Affinity.DISJOINT;
        boolean dontKnow = !(isNode || isNotNode);
        if (Cardinality.allowsMany(expression.getCardinality()) || dontKnow) {
            super.compileToBoolean(compiler, expression);
        } else {
            genEvaluateVariable(compiler, ga, (VariableReference)expression);
            ga.invokeStaticMethod(SequenceTool.class, "asItem", Sequence.class);
            LabelInfo end = methodInfo.newLabel("end");
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNull");
                ga.dup();
                ga.ifNonNull(notNull.label());
                ga.pop();
                ga.push(false);
                ga.goTo(end);
                methodInfo.placeLabel(notNull);
            }
            if (isNode) {
                ga.push(true);
            } else if (th.isSubType(type, BuiltInAtomicType.BOOLEAN)) {
                ga.checkClass(BooleanValue.class);
                ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
            } else {
                ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(AtomicValue.class, "effectiveBooleanValue");
            }
            methodInfo.placeLabel(end);
        }
        {


        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited