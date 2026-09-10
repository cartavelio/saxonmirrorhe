package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.parser.Evaluator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.value.EmptySequence;
import org.objectweb.asm.Type;


/**
 * Generate bytecode for evaluation of an UserFunctionCall
 */
public class UserFunctionCallCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "UserFunctionCallCompiler-Itr");
        compileFunctionCall(compiler, expression, true);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeInstanceMethod(Sequence.class, "iterate");
    }

    @Override
    public void compileToItem(CompilerService compiler,
                              Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "UserFunctionCallCompiler-Item");
        compileFunctionCall(compiler, expression, true);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeInstanceMethod(Sequence.class, "head");
    }

    @Override          // new for bug 3443
    public void compileToPush(CompilerService compiler,
                              Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "UserFunctionCallCompiler-Push");
        compileFunctionCall(compiler, expression, false);
    }


    public void compileFunctionCall(CompilerService compiler, Expression expression, boolean pull) throws CannotCompileException {

        UserFunctionCall userFunctionCall = (UserFunctionCall) expression;
        UserFunction provisionalFunction = userFunctionCall.getFunction();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo end = methodInfo.newLabel("endUFC");

        int numArgs = userFunctionCall.getArity();
        if (userFunctionCall.getArgumentEvaluators() == null) {
            userFunctionCall.allocateArgumentEvaluators();
        }

        //In the for loop below we expand the evaluateArguments(XPathContext c) method
        Evaluator[] argumentEvaluators = userFunctionCall.getArgumentEvaluators();
        ga.push(numArgs);
        ga.newArray(Type.getType(Sequence.class));
        int arrVar = methodInfo.allocateLocal(Sequence[].class);
        ga.storeLocal(arrVar);
        for (int i = 0; i < numArgs; i++) {
            LabelInfo notNull = methodInfo.newLabel("notNullUFC" + i);
            int refs = 10;
            // was provisionalFunction.getParameterDefinitions()[i].getReferenceCount();
            // but this is unsafe; functions can be overridden
            ga.loadLocal(arrVar);
            ga.push(i);
            // TODO: change this to use the new Evaluator mechanism natively
            LetExpressionCompiler.compileCommonExpr(compiler, userFunctionCall.getArg(i), argumentEvaluators[i], refs);
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.invokeStaticMethod(EmptySequence.class, "getInstance");
            methodInfo.placeLabel(notNull);
            ga.arrayStore(Type.getType(Sequence.class));
        }

        if (userFunctionCall.isRecursiveTailCall()) {
            // for a recursive tail call, jump back to the start of the code

            compiler.generateGetContext();
            ga.checkClass(XPathContextMajor.class);
            ga.loadLocal(arrVar);

            ga.invokeInstanceMethod(XPathContextMajor.class, "resetParameterValues", Sequence[].class);
            visitAnnotation(compiler, "RecursiveTailCall");
            ga.goTo(methodInfo.getTailCallLabel());

        } else if (userFunctionCall.getBindingSlot() >= 0) {
            compiler.generateGetContext();
            ga.push(userFunctionCall.getBindingSlot());
            // Stack: context slot
            ga.invokeInstanceMethod(XPathContext.class, "getTargetComponent", Integer.TYPE);
            // Stack: component
            int componentVar = methodInfo.allocateLocal(Component.class);
            ga.dup();
            ga.storeLocal(componentVar);

            ga.invokeInstanceMethod(Component.class, "isHiddenAbstractComponent");
            LabelInfo notAbstract = methodInfo.newLabel("notAbstract");
            ga.ifFalse(notAbstract);

            compiler.generateDynamicError("Cannot call an abstract function (" +
                                                  userFunctionCall.getDisplayName() + "#" + userFunctionCall.getArity() +
                                                  ") with no implementation", "XTDE3052", expression.getLocation(), false);
            ga.throwException();

            methodInfo.placeLabel(notAbstract);

            // Stack: (empty)
            ga.loadLocal(componentVar);
            // Stack: component
            ga.invokeInstanceMethod(Component.class, "getActor");
            ga.checkClass(UserFunction.class);
            // Stack: UserFunction
            ga.dup();
            // Stack: UserFunction UserFunction
            compiler.generateGetContext();
            // Stack: UserFunction UserFunction context
            ga.pushNull();
            ga.invokeInstanceMethod(UserFunction.class, "makeNewContext", XPathContext.class, ContextOriginator.class);
            // Stack: UserFunction c2
            ga.dup();
            // Stack: UserFunction c2 c2
            ga.loadLocal(componentVar);
            // Stack: UserFunction c2 c2 component
            ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);
            // Stack: UserFunction c2
            ga.loadLocal(arrVar);
            // Stack: UserFunction c2 actualArgs
            if (pull) {
                ga.invokeInstanceMethod(UserFunction.class, "call", XPathContext.class, Sequence[].class);
            } else {
                // Stack: UserFunction actualArgs c2 Outputter
                compiler.generateGetOutputter();
                ga.invokeInstanceMethod(UserFunction.class, "process", XPathContextMajor.class, Sequence[].class, Outputter.class);
            }


        } else {
            // Static binding to the target function: typically true for XQuery
            allocateStatic(compiler, provisionalFunction);
            ga.dup();
            compiler.generateGetContext();
            ga.pushNull();
            ga.invokeInstanceMethod(UserFunction.class, "makeNewContext", XPathContext.class, ContextOriginator.class);
            // stack: function newcontext
            if (pull) {
                ga.loadLocal(arrVar);
                // stack: function newcontext args
                ga.invokeInstanceMethod(UserFunction.class, "call", XPathContext.class, Sequence[].class);
            } else {
                ga.loadLocal(arrVar);
                // stack: function newcontext args
                compiler.generateGetOutputter();
                // stack: function newcontext args outputter
                ga.invokeInstanceMethod(UserFunction.class, "process", XPathContextMajor.class, Sequence[].class, Outputter.class);
            }

        }
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(arrVar);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited
