package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.optim.SearchableValue;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.parser.Evaluator;
import net.sf.saxon.expr.parser.Evaluators;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.*;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.ArrayList;
import java.util.List;

/**
 * Generate bytecode for evaluation of an LetExpression
 */
public class LetExpressionCompiler extends ToIteratorCompiler {

    private Evaluator getEvaluator(LetExpression expr) {
        Evaluator eval = expr.getEvaluator();
        if (eval == null) {
            return Evaluator.EagerSequence.INSTANCE;
        } else {
            return eval;
        }
    }

    /**
     * Generate bytecode to evaluate the LetExpression as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "LetExpressionCompiler - compileToItem");
        LetExpression letExpr = (LetExpression) expression;
        Expression action = letExpr.getAction();
        compiler.generateGetContext();
        ga.dup();
        ga.dup();
        //Stack: XPathContext XPathContext XPathContext
        ga.invokeInstanceMethod(XPathContext.class, "getTemporaryOutputState");
        //Stack: XPathContext XPathContext Integer
        ga.swap();
        //Stack: XPathContext Integer XPathContext
        ga.push(StandardNames.XSL_VARIABLE);
        ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
        compileCommonExpr(compiler, letExpr.getSequence(), getEvaluator(letExpr), letExpr.getNominalReferenceCount());
        int valueRepVar = methodInfo.allocateLocal(Sequence.class);
        ga.storeLocal(valueRepVar);


        //context.setTemporaryOutputState(savedOutputState);
        ga.invokeInstanceMethod(XPathContext.class,"setTemporaryOutputState", Integer.TYPE);
        compiler.generateGetContext();
        ga.push(letExpr.getLocalSlotNumber());
        ga.loadLocal(valueRepVar);
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        //compileCommonLetExpr(compiler, expression);
        compiler.compileToItem(action);
        methodInfo.releaseLocal(valueRepVar);
    }

    /**
     * Generate bytecode to evaluate the LetExpression as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "LetExpressionCompiler - compileToPush");
        LetExpression letExpr = (LetExpression) expression;
        //visitAnnotation(compiler, "LetExpressionCompiler - "+ letExpr.toString());
        Expression action = letExpr.getAction();
        compiler.generateGetContext();
        ga.dup();
        ga.dup();
        //Stack: XPathContext XPathContext XPathContext
        ga.invokeInstanceMethod(XPathContext.class, "getTemporaryOutputState");
        //Stack: XPathContext XPathContext Integer
        ga.swap();
        //Stack: XPathContext Integer XPathContext
        ga.push(StandardNames.XSL_VARIABLE);
        ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
        compileCommonExpr(compiler, letExpr.getSequence(), getEvaluator(letExpr), letExpr.getNominalReferenceCount());
        int valueRepVar = methodInfo.allocateLocal(Sequence.class);
        ga.storeLocal(valueRepVar);
        ga.invokeInstanceMethod(XPathContext.class,"setTemporaryOutputState", Integer.TYPE);
        compiler.generateGetContext();
        ga.push(letExpr.getLocalSlotNumber());
        ga.loadLocal(valueRepVar);
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        //visitAnnotation(compiler, "LetExpressionCompiler - setLocalVariable");
        //visitAnnotation(compiler, "LetExpressionCompiler - action:"+action.getClass().toString()+" is "+action.toString());
        //compileCommonLetExpr(compiler, expression);
        compiler.compileToPush(action);
    }


    /**
     * Generate bytecode to evaluate the LetExpression as an Iterator
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "LetExpressionCompiler - compileToItr");
        LetExpression letExpr = (LetExpression) expression;
        Expression action = letExpr.getAction();
        compiler.generateGetContext();
        ga.dup();
        ga.dup();
        //Stack: XPathContext XPathContext XPathContext
        ga.invokeInstanceMethod(XPathContext.class, "getTemporaryOutputState");
        //Stack: XPathContext XPathContext Integer
        ga.swap();
        //Stack: XPathContext Integer XPathContext
        ga.push(StandardNames.XSL_VARIABLE);
        ga.invokeInstanceMethod(XPathContext.class, "setTemporaryOutputState", Integer.TYPE);
        compileCommonExpr(compiler, letExpr.getSequence(), getEvaluator(letExpr), letExpr.getNominalReferenceCount());
        int valueRepVar = methodInfo.allocateLocal(Sequence.class);
        ga.storeLocal(valueRepVar);
        ga.invokeInstanceMethod(XPathContext.class,"setTemporaryOutputState", Integer.TYPE);
        compiler.generateGetContext();
        ga.push(letExpr.getLocalSlotNumber());
        ga.loadLocal(valueRepVar);
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        //compileCommonLetExpr(compiler, expression);
        compiler.compileToIterator(action);
    }


    /**
     * Helper method to generate bytecode the same for both compileToIterator and compileToItem
     * Precondition: none.
     * Postcondition: the stack contains the variable(s) already evaluated in the form of a Sequence object
     *
     * @param compiler the compiler service
     * @param exp      the expression to be compiled
     * @param evaluationMode the evaluation mode
     * @param refCount
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    public static void compileCommonExpr(CompilerService compiler,
                                         Expression exp, Evaluator evaluationMode, int refCount) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, exp);
        switch (evaluationMode.getCode()) {

            case Evaluators.EVALUATE_LITERAL:
                //return ((Literal)exp).getValue();
                compiler.compileToSequence(exp);
                break;

            case Evaluators.EVALUATE_VARIABLE:
                compiler.compileToSequence(exp);
                //was: VariableReferenceCompiler.genEvaluateVariable(compiler, ga, (VariableReference)exp);
                // but the new code is more robust. QT3 Axes-089 was failing with a ClassCastException here.
                break;

            case Evaluators.MAKE_CLOSURE:
                makeClosure(compiler, exp, refCount, ga, methodInfo);
                break;
            case Evaluators.MAKE_MEMO_CLOSURE:
                makeClosure(compiler, exp, refCount == 1 ? 10 : refCount, ga, methodInfo);
                break;
            case Evaluators.LAZY_TAIL_EXPRESSION:
                TailExpression tail = (TailExpression) exp;
                VariableReference vr = (VariableReference) tail.getBaseExpression();
                compileCommonExpr(compiler, vr, Evaluator.Variable.INSTANCE, refCount);
                ga.dup();
                int baseVar = methodInfo.allocateLocal(Sequence.class);
                ga.storeLocal(baseVar);

                LabelInfo notMemoLab = methodInfo.newLabel("notMemoLabel");
                LabelInfo notIntRangeLab = methodInfo.newLabel("notIntRangeLabel");
                LabelInfo notGroundedLab = methodInfo.newLabel("notGroundedLabel");
                LabelInfo end = methodInfo.newLabel("end_LAZY_TAIL_Label");

                ga.ifNotInstance(MemoClosure.class, notMemoLab);
                ga.loadLocal(baseVar);
                ga.invokeInstanceMethod(Sequence.class, "materialize");
                ga.storeLocal(baseVar);

                methodInfo.placeLabel(notMemoLab);
                ga.loadLocal(baseVar);
                ga.ifNotInstance(IntegerRange.class, notIntRangeLab);
                ga.loadLocal(baseVar);
                ga.checkClass(IntegerRange.class);
                ga.dup();
                ga.invokeInstanceMethod(IntegerRange.class, "getStart");
                ga.push((long)tail.getStart() - 1);
                ga.math(GeneratorAdapter.ADD, Type.LONG_TYPE);
                int startVar = methodInfo.allocateLocal(Long.TYPE);
                ga.storeLocal(startVar);
                ga.invokeInstanceMethod(IntegerRange.class, "getEnd");
                int endVar = methodInfo.allocateLocal(Long.TYPE);
                ga.storeLocal(endVar);
                LabelInfo notNELab = methodInfo.newLabel("notNELabel");
                ga.loadLocal(startVar);
                ga.loadLocal(endVar);
                ga.ifCmp(Type.LONG_TYPE , GeneratorAdapter.NE, notNELab.label());
                ga.loadLocal(endVar);
                ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
                ga.goTo(end);

                methodInfo.placeLabel(notNELab);
                ga.loadLocal(startVar);
                ga.loadLocal(endVar);
                LabelInfo notGTLab = methodInfo.newLabel("notGTLabel");
                ga.ifCmp(Type.LONG_TYPE, GeneratorAdapter.LT, notGTLab.label());
                ga.invokeStaticMethod(EmptySequence.class, "getInstance");
                ga.goTo(end);
                methodInfo.placeLabel(notGTLab);
                ga.newInstance(IntegerRange.class);
                ga.dup();
                ga.loadLocal(startVar);
                ga.push(1L);
                ga.loadLocal(endVar);
                ga.invokeConstructor(IntegerRange.class, Long.TYPE, Long.TYPE, Long.TYPE);
                ga.goTo(end);

                methodInfo.releaseLocal(startVar);
                methodInfo.releaseLocal(endVar);
                ////////
                ga.goTo(end);

                methodInfo.placeLabel(notIntRangeLab);
//                if (base instanceof GroundedValue) {
//                    GroundedValue baseSeq = (GroundedValue) base;
//                    return baseSeq.subsequence(tail.getStart() - 1, baseSeq.getLength() - tail.getStart() + 1);
//                }
                ga.loadLocal(baseVar);
                ga.ifNotInstance(GroundedValue.class, notGroundedLab);
                ga.loadLocal(baseVar);
                ga.checkClass(GroundedValue.class);
                ga.dup();
                //stack: seq seq

                ga.invokeInstanceMethod(GroundedValue.class, "getLength");
                //stack: seq int
                ga.push(tail.getStart());
                //stack: seq int int
                ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                //stack: seq int
                ga.push(1);
                //stack: seq int int
                ga.math(GeneratorAdapter.ADD, Type.INT_TYPE);
                //stack: seq int
                ga.push(tail.getStart() - 1);
                //stack: seq int int
                ga.swap();
                //stack: seq int int
                ga.invokeInstanceMethod(GroundedValue.class, "subsequence", Integer.TYPE, Integer.TYPE);
                //stack: GroundedValue
                ga.invokeInstanceMethod(GroundedValue.class, "reduce");
                ga.goTo(end);
                methodInfo.placeLabel(notGroundedLab);


                //return Closure.make(exp, context, (ref==1 ? 10 : ref));
                visitAnnotation(compiler, "Closure, refCount:" + refCount);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
                int configVar = methodInfo.allocateLocal(Configuration.class);
                ga.storeLocal(configVar);
                CompiledExpression compiledExpr =
                        compiler.compileToByteCode(exp, "", Expression.ITERATE_METHOD);
                if(compiledExpr == null) {
                    throw new CannotCompileException(exp);
                }
                ga.loadLocal(configVar);
                allocateStatic(compiler, compiledExpr);
                refCount = refCount == 1 ? 10 : refCount;
                ga.push(refCount);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(Configuration.class, "makeClosure", Expression.class, Integer.TYPE, XPathContext.class);
                ga.dup();
                ga.instanceOf(Type.getType(Closure.class));
                LabelInfo notClosure = methodInfo.newLabel("notclosure");
                ga.ifFalse(notClosure);
                ga.showMessage(compiler, "created closure");
                ga.checkClass(Closure.class);
                int closureVar = methodInfo.allocateLocal(Closure.class);
                ga.storeLocal(closureVar);

                ga.loadLocal(closureVar);
                allocateStatic(compiler, compiledExpr);

                visitLineNumber(compiler, ga, exp);
                ga.invokeInstanceMethod(Closure.class, "setExpression", Expression.class);

                ga.loadLocal(closureVar);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "newContext");
                ga.invokeInstanceMethod(Closure.class, "setSavedXPathContext", XPathContextMajor.class);

                ga.loadLocal(closureVar);
                allocateStatic(compiler, compiledExpr);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(Closure.class, "saveContext", Expression.class, XPathContext.class);
                ga.loadLocal(closureVar);
                methodInfo.placeLabel(notClosure);

                methodInfo.placeLabel(end);
                ga.checkClass(Sequence.class);
                methodInfo.releaseLocal(configVar);
                methodInfo.releaseLocal(closureVar);
                methodInfo.releaseLocal(baseVar);
                break;

            case Evaluators.MAKE_SINGLETON_CLOSURE:
                //            // return new SingletonClosure(exp, context);
                compiledExpr = compiler.compileToByteCode(exp, "", Expression.ITERATE_METHOD);
                if (compiledExpr == null) {
                    throw new CannotCompileException(exp);
                }
                ga.newInstance(Type.getType(SingletonClosure.class));
                ga.dup();
                allocateStatic(compiler, compiledExpr);
                compiler.generateGetContext();
                ga.invokeConstructor(SingletonClosure.class, Expression.class, XPathContext.class);
                break;
            case Evaluators.RETURN_EMPTY_SEQUENCE:
                ga.invokeStaticMethod(EmptySequence.class, "getInstance");
                ga.checkClass(Sequence.class);
                break;
            //
            case Evaluators.EVALUATE_AND_MATERIALIZE_VARIABLE:
                LabelInfo doneEvalMaterialize = methodInfo.newLabel("doneEvalMaterialize");

                VariableReferenceCompiler.genEvaluateVariable(compiler, ga, (VariableReference)exp);

                ga.dup();
                ga.ifNotInstance(Closure.class, doneEvalMaterialize);
                ga.checkCast(Type.getType(Closure.class));
                ga.invokeInstanceMethod(Closure.class, "iterate");
                ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                methodInfo.placeLabel(doneEvalMaterialize);
                break;
            case Evaluators.CALL_EVALUATE_OPTIONAL_ITEM:
                visitLineNumber(compiler, ga, exp);
                compiler.compileToItem(exp);
                LabelInfo notNullLab = methodInfo.newLabel("notNullLab");
                ga.dup();
                ga.ifNonNull(notNullLab.label());
                ga.pop();
                ga.invokeStaticMethod(EmptySequence.class, "getInstance");
                ga.checkClass(Sequence.class);
                methodInfo.placeLabel(notNullLab);
                break;
            case Evaluators.CALL_EVALUATE_SINGLE_ITEM:
                visitLineNumber(compiler, ga, exp);
                compiler.compileToItem(exp);
                break;

            case Evaluators.UNDECIDED:
            case Evaluators.ITERATE_AND_MATERIALIZE:
                if (refCount == FilterExpression.FILTERED) {
                    // WAS: return new IndexedValue(expression.iterate(context));
                    // 9.9: return SearchableValue.makeSearchable(expression.iterate(context));
                    visitLineNumber(compiler, ga, exp);
                    compiler.compileToIterator(exp);
                    ga.invokeStaticMethod(SearchableValue.class, "makeSearchableValue", SequenceIterator.class);
                } else {
                    // return SequenceExtent.makeSequenceExtent(expression.iterate(context));
                    visitLineNumber(compiler, ga, exp);
                    compiler.compileToIterator(exp);
                    ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                }
                break;

            case Evaluators.PROCESS:
                ToPushCompiler.compilePushToSequenceCollector(compiler, exp);
                ga.invokeInstanceMethod(SequenceCollector.class, "getSequence");
                break;

            case Evaluators.SHARED_APPEND_EXPRESSION:
                if (exp instanceof Block) {
//                    Block block = (Block) exp;
//                    Expression[] children = block.getChildren();
//                    List<Sequence> subsequences = new ArrayList<Sequence>(children.length);
//                    for (Expression child : children) {
//                        if (Cardinality.allowsMany(child.getCardinality())) {
//                            subsequences.add(SequenceTool.toLazySequence(child.iterate(context)));
//                        } else {
//                            Item j = child.evaluateItem(context);
//                            if (j != null) {
//                                subsequences.add(j);
//                            }
//                        }
//                    }
//                    return new Chain(subsequences);
                    Block block = (Block) exp;
                    Operand[] children = block.getOperanda();
                    ga.newInstance(ZenoSequence.class);
                    // Stack: Chain
                    ga.dup();
                    // Stack: Chain Chain
                    ga.newInstance(ArrayList.class);
                    // Stack: Chain Chain ArrayList
                    ga.dup();
                    // Stack: Chain Chain ArrayList ArrayList
                    ga.push(children.length);
                    // Stack: Chain Chain ArrayList ArrayList int
                    ga.invokeConstructor(ArrayList.class, Integer.TYPE);
                    // Stack: Chain Chain ArrayList
                    for (Operand o : children) {
                        Expression child = o.getChildExpression();
                        if (Cardinality.allowsMany(child.getCardinality())) {
                            ga.dup();
                            compiler.compileToIterator(child);
                            // Stack: Chain Chain ArrayList ArrayList SeqIterator
                            ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                            // Stack: Chain Chain ArrayList ArrayList Sequence
                            ga.invokeInstanceMethod(List.class, "add", Object.class);
                            // Stack: Chain Chain ArrayList boolean
                            ga.pop(); // discard the returned boolean
                            // Stack: Chain Chain ArrayList
                        } else {
                            ga.dup();
                            // Stack: Chain Chain ArrayList ArrayList
                            compiler.compileToItem(child);
                            // Stack: Chain Chain ArrayList ArrayList Item
                            if (Cardinality.allowsZero(child.getCardinality())) {
                                LabelInfo itemNull = methodInfo.newLabel("itemNull");
                                LabelInfo itemDone = methodInfo.newLabel("itemDone");
                                LabelInfo groundedLab = methodInfo.newLabel("groundedLab");
                                ga.dup();
                                // Stack: Chain Chain ArrayList ArrayList Item Item
                                ga.ifNull(itemNull.label());
                                // Stack: Chain Chain ArrayList ArrayList Item
                                ga.dup();
                                // Stack: Chain Chain ArrayList ArrayList Item Item
                                ga.ifInstance(GroundedValue.class, groundedLab);
                                // Stack: Chain Chain ArrayList ArrayList Item
                                ga.newInstance(ZeroOrOne.class);
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem
                                ga.dup2();
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem Item SingletonItem
                                ga.swap();
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem SingletonItem Item
                                ga.invokeConstructor(ZeroOrOne.class, Item.class);
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem
                                ga.swap();
                                // Stack: Chain Chain ArrayList ArrayList SingletonItem Item
                                ga.pop();
                                // Stack: Chain Chain ArrayList ArrayList SingletonItem
                                methodInfo.placeLabel(groundedLab);
                                ga.invokeInstanceMethod(List.class, "add", Object.class);
                                // Stack: Chain Chain ArrayList boolean
                                ga.pop(); // discard the returned boolean
                                // Stack: Chain Chain ArrayList
                                ga.goTo(itemDone.label());
                                methodInfo.placeLabel(itemNull);
                                // Stack: Chain Chain ArrayList ArrayList Item
                                ga.pop();
                                ga.pop();
                                // Stack: Chain Chain ArrayList
                                methodInfo.placeLabel(itemDone);
                            } else {
                                // Stack: Chain Chain ArrayList ArrayList Item
                                ga.dup();
                                // Stack: Chain Chain ArrayList ArrayList Item Item
                                LabelInfo groundedLab = methodInfo.newLabel("groundedLab");
                                ga.ifInstance(GroundedValue.class, groundedLab);
                                // Stack: Chain Chain ArrayList ArrayList Item
                                ga.newInstance(ZeroOrOne.class);
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem
                                ga.dup2();
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem Item SingletonItem
                                ga.swap();
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem SingletonItem Item
                                ga.invokeConstructor(ZeroOrOne.class, Item.class);
                                // Stack: Chain Chain ArrayList ArrayList Item SingletonItem
                                ga.swap();
                                // Stack: Chain Chain ArrayList ArrayList SingletonItem Item
                                ga.pop();
                                // Stack: Chain Chain ArrayList ArrayList SingletonItem
                                methodInfo.placeLabel(groundedLab);
                                ga.invokeInstanceMethod(List.class, "add", Object.class);
                                // Stack: Chain Chain ArrayList boolean
                                ga.pop(); // discard the returned boolean
                                // Stack: Chain Chain ArrayList
                            }
                        }
                    }
                    // Stack: Chain Chain ArrayList
                    ga.invokeStaticMethod(ZenoSequence.class, "fromList", List.class);
                    // Stack: Chain

                }
                break;


            case Evaluators.MAKE_INDEXED_VARIABLE:
                compiler.compileToIterator(exp);
                ga.invokeStaticMethod(SearchableValue.class, "makeSearchableValue", SequenceIterator.class);
                break;

            case Evaluators.STREAMING_ARGUMENT:
                PostureAndSweep ps = Streamability.getPostureAndSweepIfKnown(exp);
                if (ps == null || ps.getPosture() == Posture.GROUNDED) {
                    visitLineNumber(compiler, ga, exp);
                    compiler.compileToIterator(exp);
                    ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                } else {
                    allocateStatic(compiler, EmptySequence.getInstance());
                }
                break;

            default:
                throw new CannotCompileException(exp);
        }
    }

    private static void makeClosure(CompilerService compiler, Expression exp, int refCount, Generator ga, GeneratedMethodInfo methodInfo) throws CannotCompileException {
        if (refCount == FilterExpression.FILTERED) {
            allocateStatic(compiler, exp);
            compiler.generateGetContext();
            ga.push(refCount);
            ga.invokeStaticMethod(Closure.class, "make", Expression.class, XPathContext.class, Integer.TYPE);
            /*ga.newInstance(IndexedValue.class);
            ga.dup();
            compiler.compileToIterator(exp);
            ga.invokeConstructor(IndexedValue.class, SequenceIterator.class);*/
        } else {
            int closureVar = -1;
            if (refCount == 1) {
                ga.invokeDefaultConstructor(Closure.class);
                closureVar = methodInfo.allocateLocal(Closure.class);
            } else {
                ga.invokeDefaultConstructor(MemoClosure.class);
                closureVar = methodInfo.allocateLocal(MemoClosure.class);
            }
            ga.storeLocal(closureVar);
            ga.loadLocal(closureVar);
            ga.dup();
            //stack: Closure Closure
            CompiledExpression compiledExpr = compiler.compileToByteCode(exp, "", Expression.ITERATE_METHOD);
            if (compiledExpr == null) {
                throw new CannotCompileException(exp);
            }
            allocateStatic(compiler, compiledExpr);

            //stack: Closure Closure CompiledExpr
            ga.dupX1();     // XY --> YXY
            //stack: Closure CompiledExpr Closure CompiledExpr
            ga.invokeInstanceMethod(Closure.class, "setExpression", Expression.class);
            //stack: Closure CompiledExpr

            ga.loadLocal(closureVar);
            //stack: Closure CompiledExpr Closure
            compiler.generateGetContext();
            //stack: Closure CompiledExpr Closure XPathContext
            ga.invokeInstanceMethod(XPathContext.class, "newContext");
            //stack: Closure CompiledExpr Closure XPathContextMajor
            ga.dupX1();      // XY --> YXY
            //stack: Closure CompiledExpr XPathContextMajor Closure XPathContextMajor
            ga.invokeInstanceMethod(Closure.class, "setSavedXPathContext", XPathContextMajor.class);
            //stack: Closure CompiledExpr XPathContextMajor
            //compiler.generateGetContext();
            //stack: Closure CompiledExpr XPathContext
            ga.invokeInstanceMethod(Closure.class, "saveContext", Expression.class, XPathContext.class);
            //stack:
            ga.loadLocal(closureVar);
            methodInfo.releaseLocal(closureVar);
        }
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
