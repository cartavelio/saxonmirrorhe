package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.AttributeSet;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.UseAttributeSet;

/**
 * Generate bytecode for the XSLT use-attribute-sets pseudo-instruction. This is rarely encountered, as attribute
 * sets with statically-known content are turned into global variables. The attribute set is effectively interpreted.
 */

public class UseAttributeSetCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        UseAttributeSet instr = (UseAttributeSet) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        compiler.generateGetContext();
        ga.push(instr.getBindingSlot());
        ga.invokeInstanceMethod(XPathContext.class, "getTargetComponent", Integer.TYPE);
        // Stack: target
        ga.dup();
        // Stack: target target
        ga.invokeInstanceMethod(Component.class, "isHiddenAbstractComponent");
        LabelInfo notAbstract = methodInfo.newLabel("notAbstract");
        ga.ifFalse(notAbstract);

        compiler.generateDynamicError("Cannot call an abstract attribute-set (" +
                                              instr.getTargetAttributeSetName() +
                                              ") with no implementation", "XTDE3052",
            expression.getLocation(), false);
        ga.throwException();

        // Stack: target
        methodInfo.placeLabel(notAbstract);
        //ga.dup();

//        XPathContextMajor c2 = context.newContext();
//        c2.setCurrentComponent(target);
//        c2.setOrigin(this);

        // Stack: target
        compiler.generateGetContext();
        // Stack: target context

        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        // Stack: target contextMajor
        ga.dupX1();
        // Stack: contextMajor target contextMajor
        ga.dupX2();
        // Stack: contextMajor contextMajor target contextMajor
        ga.dup2();
        // Stack: contextMajor contextMajor target contextMajor target contextMajor
        ga.swap();
        // Stack: contextMajor contextMajor target contextMajor contextMajor target
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);
        // Stack: contextMajor contextMajor target contextMajor
        allocateStatic(compiler, expression);
        // Stack: contextMajor contextMajor target contextMajor UseAttributeSet
        ga.checkClass(ContextOriginator.class);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);
        // Stack: contextMajor contextMajor target

//        AttributeSet as = (AttributeSet) target.getCode();
//        SlotManager sm = as.getStackFrameMap();
//        if (sm == null) {
//            sm = SlotManager.EMPTY;
//        }
//        c2.openStackFrame(sm);
//        as.expand(c2);

        ga.invokeInstanceMethod(Component.class, "getActor");
        ga.checkClass(AttributeSet.class);
        // Stack: contextMajor contextMajor AttributeSet
        ga.dup();
        // Stack: contextMajor contextMajor AttributeSet AttributeSet
        ga.invokeInstanceMethod(AttributeSet.class, "getStackFrameMap");
        // Stack: contextMajor contextMajor AttributeSet SlotManager
        ga.dup();
        // Stack: contextMajor contextMajor AttributeSet SlotManager SlotManager
        LabelInfo haveSlotManager = methodInfo.newLabel("haveSlotManager");
        ga.ifNonNull(haveSlotManager.label());
        ga.pop();
        allocateStatic(compiler, SlotManager.EMPTY);
        methodInfo.placeLabel(haveSlotManager);
        // Stack: contextMajor contextMajor AttributeSet SlotManager
        ga.swap();
        // Stack: contextMajor contextMajor SlotManager AttributeSet
        ga.dupX2();
        // Stack: contextMajor AttributeSet contextMajor SlotManager AttributeSet
        ga.pop();
        // Stack: contextMajor AttributeSet contextMajor SlotManager
        ga.invokeInstanceMethod(XPathContextMajor.class, "openStackFrame", SlotManager.class);
        // Stack: contextMajor AttributeSet
        ga.swap();
        // Stack: AttributeSet contextMajor
        compiler.generateGetOutputter();
        // Stack: AttributeSet contextMajor Outputter
        ga.swap();
        // Stack: AttributeSet Outputter contextMajor
        ga.invokeInstanceMethod(AttributeSet.class, "expand", Outputter.class, XPathContext.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited