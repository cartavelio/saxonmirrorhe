package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.NextMatch;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.rules.BuiltInRuleSet;
import net.sf.saxon.trans.rules.Rule;

/**
 * Compiler for the XSLT next-match instruction
 */
public class NextMatchCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "NextMatch-push");
        visitLineNumber(compiler, ga, expression);
        final NextMatch instr = (NextMatch) expression;

//        ParameterSet params = assembleParams(context, actualParams);
//        ParameterSet tunnels = assembleTunnelParams(context, tunnelParams);

        boolean hasNonTunnelParams = instr.getActualParams() != null;
        int paramsVar = 0;
        if (hasNonTunnelParams) {
            paramsVar = methodInfo.allocateLocal(ParameterSet.class);
            ApplyTemplatesCompiler.compileParameterSet(compiler, this, instr.getActualParams());
            ga.storeLocal(paramsVar);
        }

        int tunnelsVar = methodInfo.allocateLocal(ParameterSet.class);
        ApplyTemplatesCompiler.compileTunnelParameterSet(compiler, this, instr.getTunnelParams());
        ga.storeLocal(tunnelsVar);

        // if (context.getCurrentIterator()==null) {...}

        LabelInfo iteratorNotNull = methodInfo.newLabel("iteratorNotNull");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getCurrentIterator");
        ga.dup();
        ga.ifNonNull(iteratorNotNull.label());
        compiler.generateDynamicError("Cannot call xsl:next-match when there is no context item", "XTDE0565", instr.getLocation(), false);
        methodInfo.placeLabel(iteratorNotNull);

        // Assert Stack: currentIterator

//        Item currentItem = context.getCurrentIterator().current();
//        if (!allowAnyItem && !(currentItem instanceof NodeInfo)) { ... }

        ga.invokeInstanceMethod(FocusIterator.class, "current");

        int currentItemVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(currentItemVar);

//        // Rule rule = controller.getRuleManager().getTemplateRule(currentItem, mode, min, max, context);
//
//        compiler.generateGetContext();
//        ga.invokeInstanceMethod(XPathContext.class, "getController");
//        ga.invokeInstanceMethod(Controller.class, "getRuleManager");

        //        Mode mode = context.getCurrentMode();

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getCurrentMode");
        ga.invokeInstanceMethod(Component.class, "getActor");
        ga.checkClass(Mode.class);

        int modeVar = methodInfo.allocateLocal(Mode.class);
        ga.dup();
        ga.storeLocal(modeVar);

        // Assert stack: mode

        ga.loadLocal(currentItemVar);
        // Assert stack: mode currentItem

//        Rule currentTemplateRule = context.getCurrentTemplateRule();
//        if (currentTemplateRule==null) {...}

        LabelInfo templateNotNull = methodInfo.newLabel("templateNotNull");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getCurrentTemplateRule");
        ga.dup();
        ga.ifNonNull(templateNotNull.label());
        compiler.generateDynamicError("There is no current template rule", "XTDE0560", instr.getLocation(), false);
        methodInfo.placeLabel(templateNotNull);

        // Assert Stack: mode currentItem currentTemplateRule

        compiler.generateGetContext();

        // Assert Stack: mode currentItem currentTemplateRule context

        // Rule rule = mode.getCode().getNextMatchRule(currentItem, currentRule, context);

        ga.invokeInstanceMethod(Mode.class, "getNextMatchRule", Item.class, Rule.class, XPathContext.class);

        // Assert Stack: rule

//        if (rule==null) {             // use the default action for the node
//            mode.getBuiltInRuleSet().process(currentItem, params, tunnels, context, getLocationId());


        LabelInfo ruleNotNull = methodInfo.newLabel("ruleNotNull");
        LabelInfo applyEnd = methodInfo.newLabel("applyEnd");
        ga.dup();
        ga.ifNonNull(ruleNotNull.label());
        ga.pop();
        ga.loadLocal(modeVar);
        ga.invokeInstanceMethod(Mode.class, "getBuiltInRuleSet");
        ga.loadLocal(currentItemVar);
        if (hasNonTunnelParams) {
            ga.loadLocal(paramsVar);
        } else {
            ga.pushNull();
        }
        ga.loadLocal(tunnelsVar);
        compiler.generateGetOutputter();
        compiler.generateGetContext();
        allocateStatic(compiler, instr.getLocation());
        ga.invokeInstanceMethod(BuiltInRuleSet.class, "process", Item.class,
                                ParameterSet.class, ParameterSet.class,
                                Outputter.class, XPathContext.class, Location.class);
        ga.goTo(applyEnd);

////       } else if (useTailRecursion) {
//            //Template nh = (Template)rule.getAction();
//            // clear all the local variables: they are no longer needed
//            Arrays.fill(context.getStackFrame().getStackFrameValues(), null);
//            return new NextMatchPackage(rule, params, tunnels, context);
        // TODO: implement tail recursion

//        } else {
//            XPathContextMajor c2 = context.newContext();
//            c2.setLocalParameters(params);
//            c2.setTunnelParameters(tunnels);
//            c2.setCurrentTemplateRule(rule);
//            Template template = (Template)rule.getAction();
//            c2.openStackFrame(template.getStackFrameMap());
//            template.apply(c2);
//        }

        methodInfo.placeLabel(ruleNotNull);
        // Assert stack: rule
        int ruleVar = methodInfo.allocateLocal(Rule.class);
        ga.storeLocal(ruleVar);
        // Assert stack: (empty)
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        // Assert stack: c2
        ga.dup();
        // Assert stack: c2 c2
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getCurrentMode");
        // Assert stack: c2 c2 component
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);
        ga.dup();
        // Assert stack: c2 c2
        if (hasNonTunnelParams) {
            ga.loadLocal(paramsVar);
        } else {
            ga.pushNull();
        }
        ga.invokeInstanceMethod(XPathContextMajor.class, "setLocalParameters", ParameterSet.class);
        ga.dup();
        // Assert stack: c2 c2
        ga.loadLocal(tunnelsVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setTunnelParameters", ParameterSet.class);
        ga.dup();
        // Assert stack: c2 c2
        ga.loadLocal(ruleVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentTemplateRule", Rule.class);
        ga.dup();
        // Assert stack: c2 c2  -- bug 2261
//        ga.loadLocal(ruleVar);
//        ga.invokeInstanceMethod(Rule.class, "getAction");
//        ga.checkClass(TemplateRule.class);
//        ga.invokeInstanceMethod(ComponentCode.class, "getDeclaringComponent");
//        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentComponent", Component.class);
//        ga.dup();
        // Assert stack: c2 c2
        allocateStatic(compiler, expression);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setOrigin", ContextOriginator.class);
        // Assert stack: c2
        ga.dup();
        // Assert stack: c2 c2
        ga.loadLocal(ruleVar);
        ga.invokeInstanceMethod(Rule.class, "getAction");
        ga.checkClass(TemplateRule.class);
        ga.dup();
        ga.invokeInstanceMethod(TemplateRule.class, "initialize");
        // Assert stack: c2 c2 template
        int templateVar = methodInfo.allocateLocal(TemplateRule.class);
        ga.dup();
        ga.storeLocal(templateVar);
        // Assert stack: c2 c2 template
        ga.invokeInstanceMethod(TemplateRule.class, "getStackFrameMap");
        // Assert stack: c2 c2 stackframe
        ga.invokeInstanceMethod(XPathContextMajor.class, "openStackFrame", SlotManager.class);
        // Assert stack: c2
        ga.loadLocal(templateVar);
        // Assert stack: c2 template
        ga.swap();
        // Assert stack: template c2
        compiler.generateGetOutputter();
        // Assert stack: template c2 Outputter
        ga.swap();
        // Assert stack: template Outputter c2
        ga.invokeInstanceMethod(TemplateRule.class, "apply", Outputter.class, XPathContextMajor.class);

        methodInfo.placeLabel(applyEnd);

        methodInfo.releaseLocal(paramsVar);
        methodInfo.releaseLocal(tunnelsVar);
        methodInfo.releaseLocal(currentItemVar);
        methodInfo.releaseLocal(modeVar);
        methodInfo.releaseLocal(ruleVar);
        methodInfo.releaseLocal(templateVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited



