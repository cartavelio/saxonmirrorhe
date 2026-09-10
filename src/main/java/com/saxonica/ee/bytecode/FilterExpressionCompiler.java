package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.iter.CompiledFilterIterator;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.GroundedIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;
import org.objectweb.asm.*;
import org.objectweb.asm.commons.GeneratorAdapter;
import org.objectweb.asm.commons.Method;

/**
 * Generate bytecode to evaluate a filter expression
 */
public class FilterExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        FilterExpression filterExpr = (FilterExpression) expression;
        Expression start = filterExpr.getSelectExpression();
        Expression filter = filterExpr.getActionExpression();
        Generator ga = compiler.getCurrentGenerator();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        visitLineNumber(compiler, ga, expression);
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "FilterExpressionCompiler-Itr");
        LabelInfo endLab = methodInfo.newLabel("endFilterExp");
        LabelInfo notNullLab = methodInfo.newLabel("notNullLab");
        LabelInfo emptyIterLab = methodInfo.newLabel("emptyIterLab");
        int firstVar = ga.newLocal(Item.class);
        int seqItrVar = ga.newLocal(SequenceIterator.class);
        int posVar = ga.newLocal(Integer.TYPE);
        boolean seqItrVarCreated = false;

        GroundedValue literalValue = null;
        LabelInfo notNumericLab = null;
        if (filterExpr.isIndependentFilter()) {

            if (filter instanceof Literal) {
                literalValue = ((Literal) filter).getGroundedValue();
                if (literalValue.getLength() == 1) {
                    int literalIntValue = 0;
                    try {
                        literalIntValue = Integer.parseInt(literalValue.getUnicodeStringValue().toString());
                    } catch (XPathException e) {
                        ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                        return new IteratorDetails(EmptyIterator.class, Item.class);
                    }
                    if (literalIntValue > 0 && literalIntValue < Integer.MAX_VALUE) {

                        ga.push(literalIntValue);
                        // Stack: int
                        ga.storeLocal(posVar);
                        // Stack: <empty>
                        if (start instanceof Literal) {
                            //GroundedValue startAsLiteral = ((Literal) start).getValue();
                            throw new CannotCompileException();

                        } else if (start instanceof VariableReference) {
                            VariableReferenceCompiler.genEvaluateVariable(compiler, ga, (VariableReference) start);
                            // Stack: Sequence
                            ga.dup();
                            // Stack: Sequence Sequence
                            LabelInfo notMemoLab1 = methodInfo.newLabel("notMemoLab1");
                            ga.ifNotInstance(MemoClosure.class, notMemoLab1);
                            ga.checkClass(MemoClosure.class);
                            // Stack: MemoClosure
                            ga.loadLocal(posVar);
                            // Stack: MemoClosure I
                            ga.push(1);
                            // Stack: MemoClosure I I
                            ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                            // Stack: MemoClosure I
                            ga.invokeInstanceMethod(MemoClosure.class, "itemAt", Integer.TYPE);
                            // Stack: Item
                            ga.invokeInstanceMethod(Sequence.class, "iterate");
                            // Stack: SequenceIterator
                            ga.goTo(endLab);

                            methodInfo.placeLabel(notMemoLab1);
                            // Stack: Sequence
                            LabelInfo nullItem2 = methodInfo.newLabel("nullItem2_FilterExpr");
                            ga.invokeInstanceMethod(Sequence.class, "materialize");
                            // Stack: Sequence
                            ga.loadLocal(posVar);
                            // Stack: Sequence I
                            ga.push(1);
                            // Stack: Sequence I I
                            ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                            // Stack: Sequence I
                            ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
                            // Stack: Item
                            ga.dup();
                            // Stack: Item Item
                            ga.ifNull(nullItem2.label());
                            // Stack: Item
                            ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
                            // Stack: SequenceIterator
                            ga.goTo(endLab);
                            // Stack: Item
                            methodInfo.placeLabel(nullItem2);
                            ga.pop();
                            // Stack: <empty>
                            ga.goTo(emptyIterLab);
                        } else {
                            compiler.compileToIterator(start);
                            // Stack: SequenceIterator
                            ga.dup();
                            // Stack: SequenceIterator SequenceIterator
                            LabelInfo notGroundedLab = methodInfo.newLabel("notGroundLab");
                            ga.ifNotInstance(GroundedIterator.class, notGroundedLab);
                            // Stack: GroundedIterator
                            // if itr is grounded then materialize(), itemAt(pos-1)
                            ga.checkClass(GroundedIterator.class);
                            ga.invokeInstanceMethod(GroundedIterator.class, "materialize");
                            // Stack: GroundedValue
                            ga.loadLocal(posVar);
                            ga.push(1);
                            ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                            // Stack: GroundedValue Int
                            ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
                            // Stack: Item
                            ga.dup();
                            // Stack: Item Item
                            LabelInfo gitemNullCheck = methodInfo.newLabel("gitemNullCheck_FilerExpr");
                            ga.ifNull(gitemNullCheck.label());
                            // Stack: Item
                            ga.invokeInstanceMethod(Item.class, "iterate");
                            // Stack: SequenceIterator
                            ga.goTo(endLab);

                            methodInfo.placeLabel(gitemNullCheck);
                            // Stack: Item
                            ga.pop();
                            // Stack: <empty>
                            ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                            // Stack: EmptyIterator
                            ga.goTo(endLab);

                            methodInfo.placeLabel(notGroundedLab);
                            // Stack: SequenceIterator
                            ga.loadLocal(posVar);
                            // Stack: SequenceIterator Int
                            ga.dup();
                            // Stack: SequenceIterator Int Int
                            ga.invokeStaticMethod(SubsequenceIterator.class, "make", SequenceIterator.class, Integer.TYPE, Integer.TYPE);
                            // Stack: SequenceIterator

                            methodInfo.placeLabel(endLab);
                            return new IteratorDetails(SubsequenceIterator.class, Item.class);

                        }


                    } else {
                        ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                        return new IteratorDetails(EmptyIterator.class, Item.class);


                    }


                } else {
                    try {
                        boolean ebv = literalValue.effectiveBooleanValue();
                        if (ebv) {
                            compiler.compileToIterator(start);
                        } else {
                            ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                            return new IteratorDetails(EmptyIterator.class, Item.class);
                        }

                    } catch (XPathException e) {
                        ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                        return new IteratorDetails(EmptyIterator.class, Item.class);
                    }

                }
            } else {
                visitAnnotation(compiler, "FilterExpressionCompiler-Itr2");
                if (!Cardinality.allowsMany(filter.getCardinality())) {
                    // FAST PATH FOR SINGLETON
                    compiler.compileToItem(filter);

                    if (!Cardinality.allowsZero(filter.getCardinality())) {
                        ga.storeLocal(firstVar);
                    } else {
                        ga.dup();
                        ga.storeLocal(firstVar);
                        ga.ifNonNull(notNullLab.label());
                        ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                        ga.goTo(endLab);
                        seqItrVarCreated = false;
                    }
                } else {
                    compiler.compileToIterator(filter);
                    ga.dup();
                    ga.storeLocal(seqItrVar);
                    seqItrVarCreated = true;
                    ga.invokeInstanceMethod(SequenceIterator.class, "next");
                    ga.dup();
                    ga.storeLocal(firstVar);
                    ga.ifNonNull(notNullLab.label());
                    ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
                    ga.goTo(endLab);
                }
                methodInfo.placeLabel(notNullLab);

            }
            Affinity relToNumeric = th.relationship(filter.getItemType(), NumericType.getInstance());


            if (relToNumeric != Affinity.DISJOINT) {
                // possibly numeric
                if (!(relToNumeric == Affinity.SAME_TYPE || relToNumeric == Affinity.SUBSUMED_BY)) {
                    // possibly non-numeric
                    notNumericLab = methodInfo.newLabel("notNumericLab");
                    ga.loadLocal(firstVar);
                    ga.ifNotInstance(NumericValue.class, notNumericLab);
                }
                if (Cardinality.allowsMany(filter.getCardinality())) {
                    // first item is a number
                    // following code needed only if static type allows a sequence
                    LabelInfo singleSeqLab = methodInfo.newLabel("singleSeqLab");

                    ga.loadLocal(seqItrVar);
                    ga.invokeInstanceMethod(SequenceIterator.class, "next");
                    ga.ifNull(singleSeqLab.label());
                    ga.push("sequence of two or more items starting with a numeric value");
                    ga.invokeStaticMethod(ExpressionTool.class, "ebvError", String.class);
                    //throws a exception here
                    ga.pushNull();
                    ga.goTo(endLab);
                    methodInfo.placeLabel(singleSeqLab);
                }
                Affinity relToInteger = th.relationship(filter.getItemType(), BuiltInAtomicType.INTEGER);
                // possibly an integer
                if (!(relToInteger == Affinity.SAME_TYPE || relToNumeric == Affinity.SUBSUMED_BY)) {
                    ga.loadLocal(firstVar);
                    ga.checkClass(NumericValue.class);
                    ga.invokeInstanceMethod(NumericValue.class, "isWholeNumber");
                    ga.ifFalse(emptyIterLab);
                }
                ga.loadLocal(firstVar);
                ga.checkClass(NumericValue.class);

//                    ga.invokeInstanceMethod(NumericValue.class, "longValue");
//                    ga.visitInsn(Opcodes.L2I);
//                    //  handle value > 2^63
//                    ga.storeLocal(posVar);
//                    ga.loadLocal(posVar);
//                    ga.push((int) (Integer.MAX_VALUE));
//                    ga.ifICmp(GeneratorAdapter.GE, emptyIterLab.label());

                ga.invokeInstanceMethod(NumericValue.class, "asSubscript");
                ga.storeLocal(posVar);

                ga.loadLocal(posVar);
                ga.ifZCmp(GeneratorAdapter.LE, emptyIterLab.label());
                if (start instanceof VariableReference) {
                    VariableReferenceCompiler.genEvaluateVariable(compiler, ga, (VariableReference) start);
//                    allocateStatic(compiler, start);
//                    ga.checkClass(VariableReference.class);
//                    compiler.generateGetContext();
//                    ga.invokeInstanceMethod(VariableReference.class, "evaluateVariable", XPathContext.class);
                    ga.dup();
                    LabelInfo notMemoLab = methodInfo.newLabel("notMemoLab");
                    ga.ifNotInstance(MemoClosure.class, notMemoLab);
                    ga.checkClass(MemoClosure.class);
                    ga.loadLocal(posVar);
                    ga.push(1);
                    ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                    ga.invokeStaticMethod(SequenceTool.class, "itemAt", Sequence.class, Integer.TYPE);
                    ga.dup();
                    LabelInfo itemNullCheck = methodInfo.newLabel("itemNullCheck_FilerExpr");

                    ga.ifNull(itemNullCheck.label());
                    //ga.invokeInstanceMethod(MemoClosure.class, "itemAt", Integer.TYPE);
                    ga.invokeInstanceMethod(Item.class, "iterate");
                    ga.goTo(endLab);

                    methodInfo.placeLabel(itemNullCheck);
                    ga.pop();
                    ga.goTo(emptyIterLab);
                    methodInfo.placeLabel(notMemoLab);
                    ga.invokeInstanceMethod(Sequence.class, "materialize");
                    ga.loadLocal(posVar);
                    ga.push(1);
                    ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                    ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
                    ga.dup();
                    LabelInfo itemIsNull = methodInfo.newLabel("itemIsNull");
                    ga.ifNull(itemIsNull.label());
                    ga.invokeInstanceMethod(Item.class, "iterate");
                    ga.goTo(endLab);
                    methodInfo.placeLabel(itemIsNull);
                    ga.pop();
                    ga.goTo(emptyIterLab);
                } else if (start instanceof Literal) {
                    allocateStatic(compiler, ((Literal) start).getGroundedValue());
                    ga.loadLocal(posVar);
                    ga.push(1);
                    ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                    ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
                    ga.dup();
                    LabelInfo itemNullCheck2 = methodInfo.newLabel("itemNullCheck2_FilerExpr");

                    ga.ifNull(itemNullCheck2.label());
                    //ga.invokeInstanceMethod(MemoClosure.class, "itemAt", Integer.TYPE);
                    ga.invokeInstanceMethod(Item.class, "iterate");
                    ga.goTo(endLab);
                    methodInfo.placeLabel(itemNullCheck2);
                    ga.pop();
                    ga.goTo(emptyIterLab);
                } else {
                    compiler.compileToIterator(start);
                    ga.dup();

                    LabelInfo notGrounded = methodInfo.newLabel("notGrounded");
                    ga.ifNotInstance(GroundedIterator.class, notGrounded);
                    ga.dup();
                    ga.checkClass(GroundedIterator.class);
                    ga.invokeInstanceMethod(GroundedIterator.class, "isActuallyGrounded");
                    ga.ifFalse(notGrounded);
                    
                    ga.checkClass(GroundedIterator.class);
                    ga.invokeInstanceMethod(GroundedIterator.class, "materialize");
                    ga.checkClass(GroundedValue.class);
                    ga.loadLocal(posVar);
                    ga.push(1);
                    ga.math(GeneratorAdapter.SUB, Type.INT_TYPE);
                    ga.invokeInstanceMethod(GroundedValue.class, "itemAt", Integer.TYPE);
                    ga.dup();
                    LabelInfo itemNullCheck3 = methodInfo.newLabel("itemNullCheck3_FilerExpr");
                    ga.ifNull(itemNullCheck3.label());
                    ga.invokeInstanceMethod(Item.class, "iterate");
                    ga.goTo(endLab);
                    methodInfo.placeLabel(itemNullCheck3);
                    ga.pop();
                    ga.goTo(emptyIterLab);
                    ga.goTo(endLab);
                    methodInfo.placeLabel(notGrounded);
                    ga.loadLocal(posVar);
                    ga.dup();
                    ga.invokeStaticMethod(SubsequenceIterator.class, "make", SequenceIterator.class, Integer.TYPE, Integer.TYPE);
                    ga.goTo(endLab);
                }
            }

            if (notNumericLab != null) {

                methodInfo.placeLabel(notNumericLab);
            }
            int itemVar = ga.newLocal(Type.BOOLEAN_TYPE);
            ga.push(false);
            ga.storeLocal(itemVar);
            LabelInfo endOfChecks = methodInfo.newLabel("endofChecks_FilterExpr");

            ItemType itemType = filter.getItemType();
            LabelInfo boolLab = methodInfo.newLabel("boolCheck");

            compileNodeCheck2(compiler, ga, th, itemType, endOfChecks, itemVar, firstVar);

            compileBooleanCheck2(compiler, ga, th, itemType, endOfChecks, Cardinality.allowsMany(filter.getCardinality()) && seqItrVarCreated, seqItrVar, itemVar, firstVar);

            compileStringCheck2(compiler, ga, th, itemType, endOfChecks, Cardinality.allowsMany(filter.getCardinality()) && seqItrVarCreated, seqItrVar, itemVar, firstVar);
            ga.push("sequence starting with an atomic value other than a boolean, number, or string");
            ga.invokeStaticMethod(ExpressionTool.class, "ebvError", String.class);
            methodInfo.placeLabel(endOfChecks);
            ga.loadLocal(itemVar);
            ga.ifFalse(emptyIterLab);
            compiler.compileToIterator(start);
            ga.goTo(endLab);
            methodInfo.releaseLocal(seqItrVar);
            methodInfo.releaseLocal(itemVar);


        } else {
            // See if we already have compiled code for this predicate (typically happens when we generate
            // both iterate (pull) and process (push) code for the filter expression) - if so, reuse it
            Class filterIteratorClass = compiler.getCompiledClass(filter);

            if (filterIteratorClass == null) {
                filterIteratorClass = makeFilterIteratorClass(compiler, filter);
                compiler.setCompiledClass(filter, filterIteratorClass);
            }
            ExpressionCompiler.allocateStatic(compiler, filterIteratorClass);
            ga.invokeInstanceMethod(Class.class, "newInstance");
            ga.checkClass(CompiledFilterIterator.class);
            ga.dup();

            compiler.compileToIterator(start);

            ga.checkClass(SequenceIterator.class);
            compiler.generateGetContext();
            ga.invokeInstanceMethod(CompiledFilterIterator.class, "setSequence", SequenceIterator.class, XPathContext.class);
            ga.goTo(endLab);
        }
        methodInfo.placeLabel(emptyIterLab);
        ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");

        methodInfo.placeLabel(endLab);
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

    /**
     * Generate a subclass of CompiledFilterIterator with a customized matches() method
     *
     * @param compiler  the CompilerService
     * @param predicate the predicate expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    private Class<? extends CompiledFilterIterator> makeFilterIteratorClass(
            CompilerService compiler, final Expression predicate) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_CompiledFilterIterator_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(predicate.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                 "com/saxonica/ee/bytecode/iter/CompiledFilterIterator", new String[]{});
        compiler.pushNewClassInfo(className, CompiledFilterIterator.class, cw);

        // public FilterIteratorXXX() {}

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(CompiledFilterIterator.class), m);
        ga.returnValue();
        ga.endMethod();

        // public boolean matches() {
        //    (compile predicate expression to return boolean)
        // }

        m = Method.getMethod("boolean matches()");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        int contextVar = ga.newLocal(XPathContext.class);
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        visitLineNumber(compiler, ga, predicate);
        ga.loadThis();
        ga.getField(Type.getType(CompiledFilterIterator.class), "filterContext", Type.getType(XPathContext.class));
        compiler.initNewMethod(ga, false);

        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType predicateItemType = predicate.getItemType();
        if (predicate.getCardinality() == StaticProperty.EXACTLY_ONE &&
                th.isSubType(predicateItemType, BuiltInAtomicType.BOOLEAN)) {

            // predicate is a simple boolean
            compiler.compileToBoolean(predicate);
        } else if (!Cardinality.allowsMany(predicate.getCardinality())) {

            // predicate is either a singleton, or empty
            compileSingletonPredicate(compiler, predicate);
        } else {

            // predicate is not necessarily a boolean; need special handling for the numeric case.
            // code is similar to ToIteratorCompiler.compileToBoolean
            compileGeneralPredicate(compiler, predicate);
        }
        ga.showMessage(compiler, "FilterExpression-itr-test1");
        ga.returnValue();
        try {
            ga.endMethod();
        } catch (Exception err) {
            System.err.println("**** endMethod failed");
        }
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "Filter expression at line " + predicate.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return (Class<? extends CompiledFilterIterator>) compiler.makeClass(cw, className);

    }

    /**
     * Compile code to evaluate the predicate in the most general case (where it may be multi-valued and may be numeric
     *
     * @param compiler  the bytecode compiler service
     * @param predicate the predicate to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    private void compileGeneralPredicate(CompilerService compiler, final Expression predicate) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType predicateItemType = predicate.getItemType();
        LabelInfo end = methodInfo.newLabel("end");
        LabelInfo checkNoMoreItems = methodInfo.newLabel("checkNoMoreItems");
        LabelInfo error = methodInfo.newLabel("error");

        visitAnnotation(compiler, "FilterExpressionCompiler.compileGeneralPredicate(" + predicate.toString() + ")");

        // iterVar = predicate.iterate()

        compiler.compileToIterator(predicate);
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        //ga.dup();
        ga.storeLocal(iterVar);

        int resultVar = ga.newLocal(Type.BOOLEAN_TYPE);
        ga.push(false);
        ga.storeLocal(resultVar);

        // itemVar = iterVar.next();

        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        int itemVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(itemVar);

        // if (itemVar == null) return false

        if (Cardinality.allowsZero(predicate.getCardinality())) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compileGeneralPredicate(): test if empty");
            LabelInfo notEmptySequence = methodInfo.newLabel("notEmptySequence");
            ga.loadLocal(itemVar);
            ga.ifNonNull(notEmptySequence.label());
            ga.goTo(end);
            methodInfo.placeLabel(notEmptySequence);
        }

        // if (itemVar is boolean) {checkNoMore(); return itemVar.asBoolean()}

        compileBooleanCheck(compiler, ga, th, predicateItemType, checkNoMoreItems, resultVar, itemVar);

        // if (itemVar is numeric) {checkNoMore(); return (itemVar == position()}

        compileNumericCheck(compiler, ga, th, predicateItemType, end, checkNoMoreItems, resultVar, itemVar);

        // if (itemVar is string) {checkNoMore(); return (itemVar.asString().length() != 0}

        compileStringCheck(compiler, ga, th, predicateItemType, checkNoMoreItems, resultVar, itemVar);

        // if (itemVar is node) return true;

        compileNodeCheck(compiler, ga, th, predicateItemType, end, error, resultVar, itemVar);

        // checkNoMore: if (iterVar.next() != null) error;

        methodInfo.placeLabel(checkNoMoreItems);
        visitAnnotation(compiler, "FilterExpressionCompiler.compileGeneralPredicate(): check no more items");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.ifNull(end.label());

        methodInfo.placeLabel(error);
        compiler.generateDynamicError("Effective boolean value is not defined", "FORG0006", predicate.getLocation(), false);
        // return
        methodInfo.placeLabel(end);
        ga.loadLocal(resultVar);
        methodInfo.releaseLocal(itemVar);
        methodInfo.releaseLocal(iterVar);
    }

    /**
     * Compile code to evaluate the predicate in the most general case (where it may be multi-valued and may be numeric
     *
     * @param compiler  the bytecode compiler service
     * @param predicate the predicate to be compiled
     * @throws CannotCompileException in the event of a compile error
     */

    private void compileSingletonPredicate(CompilerService compiler, final Expression predicate) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType predicateItemType = predicate.getItemType();
        LabelInfo end = methodInfo.newLabel("end");
        LabelInfo error = methodInfo.newLabel("error");

        visitAnnotation(compiler, "FilterExpressionCompiler.compileSingletonPredicate(" + predicate + ")");

        // iterVar = predicate.iterate()

        compiler.compileToItem(predicate);

        int resultVar = ga.newLocal(Type.BOOLEAN_TYPE);
        ga.push(false);
        ga.storeLocal(resultVar);

        // itemVar = iterVar.next();


        int itemVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(itemVar);

        // if (itemVar == null) return false

        if (Cardinality.allowsZero(predicate.getCardinality())) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compileSingletonPredicate(): test if empty");
            Label notEmptySequence = new Label();
            ga.loadLocal(itemVar);
            ga.ifNonNull(notEmptySequence);
            ga.goTo(end);
            ga.mark(notEmptySequence);
        }

        // if (itemVar is boolean) {checkNoMore(); return itemVar.asBoolean()}

        compileBooleanCheck(compiler, ga, th, predicateItemType, end, resultVar, itemVar);

        // if (itemVar is numeric) {checkNoMore(); return (itemVar == position()}

        compileNumericCheck(compiler, ga, th, predicateItemType, end, end, resultVar, itemVar);

        // if (itemVar is string) {checkNoMore(); return (itemVar.asString().length() != 0}

        compileStringCheck(compiler, ga, th, predicateItemType, end, resultVar, itemVar);

        // if (itemVar is node) return true;

        compileNodeCheck(compiler, ga, th, predicateItemType, end, error, resultVar, itemVar);

        methodInfo.placeLabel(error);
        compiler.generateDynamicError("Effective boolean value is not defined", "FORG0006", predicate.getLocation(), false);
        visitAnnotation(compiler, "FilterExpressionCompiler.compileSingletonPredicate(): return");
        // return
        methodInfo.placeLabel(end);
        ga.loadLocal(resultVar);
    }


    private void compileBooleanCheck(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo checkNoMoreItems, int resultVar, int itemVar) {
        Affinity relToBoolean = th.relationship(predicateItemType, BuiltInAtomicType.BOOLEAN);
        if (relToBoolean != Affinity.DISJOINT) {
            Label notBoolean = null;
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test boolean value");
            if (!(relToBoolean == Affinity.SAME_TYPE || relToBoolean == Affinity.SUBSUMED_BY)) {
                notBoolean = new Label();
                ga.loadLocal(itemVar);
                ga.instanceOf(Type.getType(BooleanValue.class));
                ga.ifZCmp(Generator.EQ, notBoolean);
            }
            ga.loadLocal(itemVar);
            ga.checkClass(BooleanValue.class);
            ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
            ga.storeLocal(resultVar);
            ga.goTo(checkNoMoreItems);
            if (notBoolean != null) {
                ga.mark(notBoolean);
            }
        }
    }

    private void compileNumericCheck(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo end, LabelInfo checkNoMoreItems, int resultVar, int itemVar) {
        Affinity relToNumeric = th.relationship(predicateItemType, NumericType.getInstance());
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo isZero = methodInfo.newLabel("isZeroLab");
        if (relToNumeric != Affinity.DISJOINT) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test numeric value");
            LabelInfo notNumeric = null;
            if (!(relToNumeric == Affinity.SAME_TYPE || relToNumeric == Affinity.SUBSUMED_BY)) {
                notNumeric = methodInfo.newLabel("notNumeric");
                ga.loadLocal(itemVar);
                ga.instanceOf(Type.getType(NumericValue.class));
                ga.ifFalse(notNumeric);
            }

            if (th.relationship(predicateItemType, BuiltInAtomicType.DOUBLE) != Affinity.DISJOINT ||
                    th.relationship(predicateItemType, BuiltInAtomicType.FLOAT) != Affinity.DISJOINT) {

                // check for NaN
                visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test for NaN");
                ga.loadLocal(itemVar);
                ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
                Label notNaN = new Label();
                ga.ifZCmp(Generator.EQ, notNaN);
                ga.goTo(checkNoMoreItems);
                ga.mark(notNaN);
            }

            ga.loadLocal(itemVar);
            ga.checkClass(NumericValue.class);

            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getCurrentIterator");
            ga.invokeInstanceMethod(FocusIterator.class, "position");
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
            ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
            ga.ifZCmp(Generator.EQ, isZero.label());
            ga.push(false);
            ga.storeLocal(resultVar);
            ga.goTo(checkNoMoreItems);
            methodInfo.placeLabel(isZero);
            ga.push(true);
            ga.storeLocal(resultVar);
            ga.goTo(checkNoMoreItems);
            if (notNumeric != null) {
                methodInfo.placeLabel(notNumeric);
            }
        }
    }

    private void compileStringCheck(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo checkNoMoreItems, int resultVar, int itemVar) {
        Affinity relToString = th.relationship(predicateItemType, BuiltInAtomicType.STRING);
        if (relToString != Affinity.DISJOINT) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate: test string value");
            Label notString = null;
            if (!(relToString == Affinity.SAME_TYPE || relToString == Affinity.SUBSUMED_BY)) {
                notString = new Label();
                ga.loadLocal(itemVar);
                ga.instanceOf(Type.getType(StringValue.class));
                ga.ifZCmp(Generator.EQ, notString);
            }
            ga.loadLocal(itemVar);
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "isEmpty");
            ga.not();
            ga.storeLocal(resultVar);
            ga.goTo(checkNoMoreItems);
            if (notString != null) {
                ga.mark(notString);
            }
        }
    }

    private void compileNodeCheck(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo end, LabelInfo error, int resultVar, int itemVar) {
        Affinity relToNode = th.relationship(predicateItemType, AnyNodeTest.getInstance());
        if (relToNode == Affinity.SAME_TYPE || relToNode == Affinity.SUBSUMED_BY) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): node exists");
            ga.push(true);
            ga.storeLocal(resultVar);
            ga.goTo(end);
        } else if (relToNode != Affinity.DISJOINT) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test node existence");
            ga.loadLocal(itemVar);
            ga.instanceOf(Type.getType(NodeInfo.class));
            ga.ifZCmp(Generator.EQ, error.label());
            ga.push(true);
            ga.storeLocal(resultVar);
            ga.goTo(end);
        }
    }

    private void compileBooleanCheck2(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo end, boolean oneOrMoreCheck, int seqItrVar, int resultVar, int itemVar) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Affinity relToBoolean = th.relationship(predicateItemType, BuiltInAtomicType.BOOLEAN);
        if (relToBoolean != Affinity.DISJOINT) {
            LabelInfo notBoolean = null;
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test boolean value2");
            if (relToBoolean == Affinity.SAME_TYPE || relToBoolean == Affinity.SUBSUMED_BY) {
                ga.loadLocal(itemVar);
                ga.checkClass(BooleanValue.class);
                ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
                ga.storeLocal(resultVar);

            } else {
                notBoolean = methodInfo.newLabel("notBoolean_compileBooleanCheck2");
                ga.loadLocal(itemVar);
                ga.ifNotInstance(BooleanValue.class, notBoolean);
                ga.loadLocal(itemVar);
                ga.checkClass(BooleanValue.class);
                ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
                ga.storeLocal(resultVar);
            }

            if (oneOrMoreCheck) {
                ga.loadLocal(seqItrVar);
                ga.checkClass(SequenceIterator.class);
                ga.invokeInstanceMethod(SequenceIterator.class, "next");
                ga.ifNull(end.label());
                ga.push("sequence of two or more items starting with a boolean value");
                ga.invokeStaticMethod(ExpressionTool.class, "ebvError", String.class);
            }

            ga.goTo(end);
            if (notBoolean != null) {
                methodInfo.placeLabel(notBoolean);
            }
        }
    }

    private void compileStringCheck2(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo end, boolean oneOrMoreCheck, int seqItrVar, int resultVar, int firstVar) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Affinity relToString = th.relationship(predicateItemType, BuiltInAtomicType.STRING);
        if (relToString != Affinity.DISJOINT) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate: test string value");
            LabelInfo notString = null;
            if (!(relToString == Affinity.SAME_TYPE || relToString == Affinity.SUBSUMED_BY)) {
                notString = methodInfo.newLabel("notString_compileStringCheck2");
                ga.loadLocal(firstVar);
                ga.instanceOf(Type.getType(StringValue.class));
                ga.ifZCmp(Generator.EQ, notString.label());
            }
            ga.loadLocal(firstVar);
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "isEmpty");
            ga.not();
            ga.storeLocal(resultVar);
            if (oneOrMoreCheck) {
                ga.loadLocal(seqItrVar);
                ga.invokeInstanceMethod(SequenceIterator.class, "next");
                ga.ifNull(end.label());
                ga.push("sequence of two or more items starting with a string value");
                ga.invokeStaticMethod(ExpressionTool.class, "ebvError", String.class);
            }
            ga.goTo(end);
            if (notString != null) {
                methodInfo.placeLabel(notString);
            }
        }
    }

    private void compileNodeCheck2(CompilerService compiler, Generator ga, TypeHierarchy th, ItemType predicateItemType, LabelInfo end, int resultVar, int itemVar) {
        Affinity relToNode = th.relationship(predicateItemType, AnyNodeTest.getInstance());
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo nextCheck = methodInfo.newLabel("nextcheck");
        if (relToNode == Affinity.SAME_TYPE || relToNode == Affinity.SUBSUMED_BY) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): node exists");
            ga.push(true);
            ga.storeLocal(resultVar);
            ga.goTo(end);
        } else if (relToNode != Affinity.DISJOINT) {
            visitAnnotation(compiler, "FilterExpressionCompiler.compilePredicate(): test node existence");
            ga.loadLocal(itemVar);
            ga.ifNotInstance(NodeInfo.class, nextCheck);
            ga.push(true);
            ga.storeLocal(resultVar);
            ga.goTo(end);
            methodInfo.placeLabel(nextCheck);
        }
    }


}

// Copyright (c) 2011-2022 Saxonica Limited


