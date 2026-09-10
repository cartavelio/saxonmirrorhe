package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.CompiledItemMappingFunction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.functions.hof.CoercedFunction;
import net.sf.saxon.functions.hof.FunctionSequenceCoercer;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.type.UType;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Opcodes;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.Method;


/**
 * Generate bytecode to evaluate a FunctionSequenceCoercer function
 */
public class FunctionSequenceCoercerCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();

        FunctionSequenceCoercer functionCoercer = (FunctionSequenceCoercer) expression;
        Expression operand = functionCoercer.getBaseExpression();
        Class itemMappingFunction = compiler.getCompiledClass(expression);
        if (itemMappingFunction == null) {
            itemMappingFunction = generateMappingFunction(compiler, functionCoercer);
            compiler.setCompiledClass(expression, itemMappingFunction);
        }

        ga.newInstance(ItemMappingIterator.class);
        ga.dup();
        compiler.compileToIterator(operand);
        ExpressionCompiler.allocateStatic(compiler, itemMappingFunction);
        ga.invokeInstanceMethod(Class.class, "newInstance");
        ga.checkCast(Type.getType(CompiledItemMappingFunction.class));
        ga.push(true);
        ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class, Boolean.TYPE);
    }


    @Override
    public void compileToItem(CompilerService compiler,
                              Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        LabelInfo end = mi.newLabel("endCoercer");

        FunctionSequenceCoercer functionCoercer = (FunctionSequenceCoercer) expression;
        Expression operand = functionCoercer.getBaseExpression();

//        Item item = getBaseExpression().evaluateItem(context);
//        if (item == null) {
//            return null;
//        }
//        if (!(item instanceof Function)) {
//            UType itemType = UType.getUType(item);
//            throw new XPathException(role.composeErrorMessage(requiredItemType, itemType), "XPTY0004");
//        }
//        return new CoercedFunction((Function) item, requiredItemType);

        compiler.compileToItem(operand);
        ga.dup();
        ga.ifNull(end.label());
        ga.dup();
        LabelInfo isFunctionLab = mi.newLabel("isFunctionLab");
        ga.ifInstance(Function.class, isFunctionLab);
        // stack: item
        allocateStatic(compiler, functionCoercer.getRole());
        // stack: item role
        ga.swap();
        // stack: role item
        allocateStatic(compiler, functionCoercer.getItemType());
        // stack: role item requiredItemType
        ga.swap();
        // stack: role requiredItemType item
        ga.invokeStaticMethod(UType.class, "getUType", Item.class);
        // stack: role requiredItemType itemType
        ga.invokeInstanceMethod(RoleDiagnostic.class, "composeErrorMessage", ItemType.class, UType.class);
        compiler.generateParameterizedDynamicError(MessageFromStack.getInstance(), "XPTY0004", expression.getLocation(), true);
        ga.goTo(end);
        mi.placeLabel(isFunctionLab);
        // stack: item
        ga.checkClass(Function.class);
        ga.newInstance(CoercedFunction.class);
        // stack: item CoercedFunction
        ga.dupX1();
        // stack: CoercedFunction item CoercedFunction
        ga.swap();
        // stack: CoercedFunction CoercedFunction item
        allocateStatic(compiler, functionCoercer.getItemType());
        // stack: CoercedFunction CoercedFunction item reqItemType
        ga.invokeConstructor(CoercedFunction.class, Function.class, SpecificFunctionType.class);
        // stack: CoercedFunction



//        ga.checkCast(Type.getType(Function.class));
//        allocateStatic(compiler, functionCoercer.getItemType());
//        allocateStatic(compiler, functionCoercer.getRole());
//        allocateStatic(compiler, functionCoercer.getContainer());
////        public static CoercedFunction coerce(FunctionItem suppliedFunction, SpecificFunctionType requiredType,
////                                         RoleLocator role, TypeCheckerEnvironment visitor, Container container)
//        ga.invokeStaticMethod(CoercedFunction.class, "coerce", Function.class,
//                SpecificFunctionType.class, RoleDiagnostic.class, Container.class);
        mi.placeLabel(end);
    }


    /**
     * Generate code for the mapping function
     *
     * @param compiler   the CompilerService
     * @param expression the expression being compiled
     * @return the compiled class
     * @throws CannotCompileException if compilation fails
     */
    private Class<? extends CompiledItemMappingFunction> generateMappingFunction(CompilerService compiler, Expression expression) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_CompiledItemMappingFunction_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                "com/saxonica/ee/bytecode/map/CompiledItemMappingFunction", new String[]{});
        compiler.pushNewClassInfo(className, CompiledItemMappingFunction.class, cw);

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(CompiledItemMappingFunction.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.om.Item mapItem(net.sf.saxon.om.Item)");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        int contextVar = ga.newLocal(XPathContext.class);
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        FunctionSequenceCoercer expr = (FunctionSequenceCoercer) expression;
        visitLineNumber(compiler, ga, expr);
        ga.loadArg(0);
        ga.checkCast(Type.getType(Function.class));
        allocateStatic(compiler, expr.getItemType());
        allocateStatic(compiler, expr.getRole());
        ga.invokeStaticMethod(CoercedFunction.class, "coerce", Function.class, SpecificFunctionType.class,
                RoleDiagnostic.class);
        ga.returnValue();
        ga.endMethod();
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "FunctionSequenceCoercer " + expr.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return (Class<? extends CompiledItemMappingFunction>) compiler.makeClass(cw, className);
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
