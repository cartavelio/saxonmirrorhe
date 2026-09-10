package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.iter.OneToOneMappingIterator;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemChecker;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.NumericType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Opcodes;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.Method;

import javax.xml.transform.SourceLocator;

/**
 * Generate bytecode for an ItemChecker expression, performing run-time type checking
 */
public class ItemCheckerCompiler extends ToIteratorCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        ItemChecker itemChecker = (ItemChecker) expression;
        Expression operand = itemChecker.getBaseExpression();
        compiler.compileToItem(operand);
        visitLineNumber(compiler, ga, expression);
        boolean allowEmpty = Cardinality.allowsZero(operand.getCardinality());
        LabelInfo ok = null;
        if (allowEmpty) {
            // generate "if (null) return null"
            ok = methodInfo.newLabel("ok");
            ga.dup();
            ga.ifNull(ok.label());
        }
        final ItemType requiredType = itemChecker.getRequiredType();
        RoleDiagnostic role = itemChecker.getRoleLocator();
        generateItemCheck(compiler, ga, requiredType, role, expression.getLocation());
        if (ok != null) {
            methodInfo.placeLabel(ok);
        }
    }

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "ItemCheckerCompiler");
        Class checkingIterator = compiler.getCompiledClass(expression);
        if (checkingIterator == null) {
            checkingIterator = generateItemCheckingIterator(compiler, (ItemChecker) expression);
            compiler.setCompiledClass(expression, checkingIterator);
        }
        ExpressionCompiler.allocateStatic(compiler, checkingIterator);
        ga.invokeInstanceMethod(Class.class, "newInstance");
        ga.checkClass(OneToOneMappingIterator.class);
        ga.dup();
        compiler.compileToIterator(((ItemChecker) expression).getBaseExpression());
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(OneToOneMappingIterator.class, "setSequence", SequenceIterator.class, XPathContext.class);
    }

    private Class generateItemCheckingIterator(CompilerService compiler, ItemChecker expression) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        final ItemType requiredType = expression.getRequiredType();
        RoleDiagnostic role = expression.getRoleLocator();

        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "ItemCheckingIterator_" + + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, "gen/" + className, null,
                "com/saxonica/ee/bytecode/iter/OneToOneMappingIterator", new String[]{});
        compiler.pushNewClassInfo("gen." + className, OneToOneMappingIterator.class, cw);
        // public ItemCheckingIteratorXXX() {}

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(OneToOneMappingIterator.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.om.Item map(net.sf.saxon.om.Item)");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        int contextVar = ga.newLocal(XPathContext.class);
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        visitLineNumber(compiler, ga, expression);
        ga.loadThis();
        ga.getField(Type.getType(OneToOneMappingIterator.class), "context", Type.getType(XPathContext.class));
        //ga.storeLocal(contextVar);
        compiler.initNewMethod(ga, false);

        ga.loadArg(0);
        generateItemCheck(compiler, ga, requiredType, role, expression.getLocation());
        ga.returnValue();

        try {
            ga.endMethod();
        } catch (Exception err) {
            System.err.println("**** endMethod failed");
        }
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "Item checker at line " + expression.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return compiler.makeClass(cw, "gen." + className);
    }

    /**
     * Generate code to check the item against the required type
     * On entry: top-of-stack is the item to be checked
     * On exit: stack is unchanged
     *
     * @param compiler     the compiler service
     * @param ga           the GeneratorAdaptor
     * @param requiredType the required type of the item
     * @param role         used to construct an error message if the item has the wrong type
     * @param locator      the location to use in diagnostics
     */

    private void generateItemCheck(final CompilerService compiler, Generator ga,
                                   final ItemType requiredType, final RoleDiagnostic role, final SourceLocator locator) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo ok = methodInfo.newLabel("ok");
        LabelInfo errorLab = null;
        ga.dup();
        if (requiredType == AnyNodeTest.getInstance()) {
            ga.instanceOf(Type.getType(NodeInfo.class));
        } else if (requiredType.equals(BuiltInAtomicType.ANY_ATOMIC)) {
            ga.instanceOf(Type.getType(AtomicValue.class));
//        } else if (requiredType.equals(BuiltInAtomicType.STRING)) {
//            errorLab = methodInfo.newLabel("errorLab");
//            ga.dup();
//            ga.instanceOf(Type.getType(StringValue.class));
//            ga.swap();
//            ga.invokeInstanceMethod(AtomicValue.class, "isUntypedAtomic");
//            ga.not();
//            ga.ifICmp(Generator.EQ, ok.label());
//            ga.goTo(errorLab);
        } else if (requiredType.equals(NumericType.getInstance())) {
            ga.instanceOf(Type.getType(NumericValue.class));
        } else if (requiredType.equals(BuiltInAtomicType.DOUBLE)) {
            ga.instanceOf(Type.getType(DoubleValue.class));
        } else if (requiredType.equals(BuiltInAtomicType.FLOAT)) {
            ga.instanceOf(Type.getType(FloatValue.class));
        } else if (requiredType.equals(BuiltInAtomicType.INTEGER)) {
            ga.instanceOf(Type.getType(IntegerValue.class));
        } else if (requiredType.equals(BuiltInAtomicType.BOOLEAN)) {
            ga.instanceOf(Type.getType(BooleanValue.class));
        } else {
            allocateStatic(compiler, requiredType);
            ga.swap();
            Configuration config = compiler.getConfiguration();
            TypeHierarchy th = config.getTypeHierarchy();
            allocateStatic(compiler, th);
            // TODO: handle URI promotion
            ga.invokeInstanceMethod(ItemType.class, "matches", Item.class, TypeHierarchy.class);
        }

        ga.ifTrue(ok);
        if (errorLab != null) {
            methodInfo.placeLabel(errorLab);
        }
        String msg = role.composeRequiredMessage(requiredType);
        ga.dup();
        ga.invokeStaticMethod(net.sf.saxon.type.Type.class, "displayTypeName", Item.class);
        compiler.generateParameterizedDynamicError(
                    new MessageTemplate(ga, msg + "; actual type is {$1}", -1),
                    role.getErrorCode(), locator, true);
        methodInfo.placeLabel(ok);
    }
}

// Copyright (c) 2011-2020 Saxonica Limited


