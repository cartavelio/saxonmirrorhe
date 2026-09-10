package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.CompiledContextMappingFunction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.type.TypeHierarchy;

/**
 * Bytecode generator for the xsl:for-each-group class
 */
public class ForEachGroupCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        final ForEachGroup instr = (ForEachGroup) expression;

        visitAnnotation(compiler, "ForEachGroupCompiler-Push");
        visitLineNumber(compiler, ga, expression);

        getGroupIterator(compiler, expression);
        int groupIterVar = methodInfo.allocateLocal(GroupIterator.class);
        ga.storeLocal(groupIterVar);

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        int context2Var = methodInfo.allocateLocal(XPathContextMajor.class);
        ga.storeLocal(context2Var);
        methodInfo.pushContextVariableInfo(context2Var, false);

        ga.loadLocal(context2Var);
        ga.loadLocal(groupIterVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentGroupIterator", GroupIterator.class);

        ga.loadLocal(context2Var);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        ga.loadLocal(groupIterVar);
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        ga.loadLocal(context2Var);
        ga.pushNull();
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentTemplateRule", Rule.class);

        LabelInfo loop = methodInfo.placeNewLabel("groupLoop");
        LabelInfo groupBreak = methodInfo.newLabel("groupBreak");

        ga.loadLocal(context2Var);
        ga.invokeInstanceMethod(XPathContext.class, "getCurrentIterator");
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.ifNull(groupBreak.label());

        compiler.compileToPush(instr.getActionExpression());

        ga.goTo(loop);
        methodInfo.placeLabel(groupBreak);

        methodInfo.popContextVariableInfo();

        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(groupIterVar);

    }

    private void getGroupIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        final ForEachGroup instr = (ForEachGroup) expression;

        switch (instr.getAlgorithm()) {
            case ForEachGroup.GROUP_BY: {
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "newMinorContext");
                int context2Var = methodInfo.allocateLocal(XPathContext.class);
                int populationVar = methodInfo.allocateLocal(SequenceIterator.class);

                ga.newInstance(FocusTrackingIterator.class);
                ga.dup();
                compiler.compileToIterator(instr.getSelectExpression());
                ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);

                ga.checkClass(SequenceIterator.class);
                ga.storeLocal(populationVar);

                ga.storeLocal(context2Var);

                ga.loadLocal(context2Var);
                ga.loadLocal(populationVar);
                ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

                ga.newInstance(GroupByIterator.class);
                ga.dup();
                ga.loadLocal(populationVar);
                // TODO: compile the grouping key expression
                allocateStatic(compiler, instr.getGroupingKey());
                ga.loadLocal(context2Var);
                if (instr.getCollation() == null) {
                    getStringCollator(compiler, instr);
                } else {
                    allocateStatic(compiler, instr.getCollation());
                }
                ga.push(instr.isComposite());
                ga.invokeConstructor(GroupByIterator.class, SequenceIterator.class, Expression.class, XPathContext.class, StringCollator.class, Boolean.TYPE);

                methodInfo.releaseLocal(context2Var);
                methodInfo.releaseLocal(populationVar);
                break;
            }
            case ForEachGroup.GROUP_ADJACENT: {

                //groupIterator = new GroupAdjacentIterator(population, key, context, coll);
                ga.newInstance(GroupAdjacentIterator.class);
                ga.dup();
                //ga.loadLocal(populationVar);
                allocateStatic(compiler, instr.getSelectExpression());
                // TODO: compile the grouping key expression
                allocateStatic(compiler, instr.getGroupingKey());
                compiler.generateGetContext();
                if (instr.getCollation() == null) {
                    getStringCollator(compiler, instr);
                } else {
                    allocateStatic(compiler, instr.getCollation());
                }
                ga.push(instr.isComposite());
                ga.invokeConstructor(GroupAdjacentIterator.class, Expression.class, Expression.class, XPathContext.class, StringCollator.class, Boolean.TYPE);
                break;
            }
            case ForEachGroup.GROUP_STARTING:
                //groupIterator = new GroupStartingIterator(population, ((PatternSponsor) key).getPattern(), context);
                ga.newInstance(GroupStartingIterator.class);
                ga.dup();
                //ga.loadLocal(populationVar);
                allocateStatic(compiler, instr.getSelectExpression());
                allocateStatic(compiler, instr.getGroupingKey());
                compiler.generateGetContext();
                ga.invokeConstructor(GroupStartingIterator.class, Expression.class, Pattern.class, XPathContext.class);
                break;

            case ForEachGroup.GROUP_ENDING:
                //groupIterator = new GroupEndingIterator(population, ((PatternSponsor) key).getPattern(), context);
                ga.newInstance(GroupEndingIterator.class);
                ga.dup();
                //ga.loadLocal(populationVar);
                allocateStatic(compiler, instr.getSelectExpression());
                allocateStatic(compiler, instr.getGroupingKey());
                compiler.generateGetContext();
                ga.invokeConstructor(GroupEndingIterator.class, Expression.class, Pattern.class, XPathContext.class);
                break;

            case ForEachGroup.GROUP_BREAKING_WHEN:
//                Function breakWhen = (Function) getGroupingKey().evaluateItem(context);
//                groupIterator = new GroupBreakingIterator(select, breakWhen, context);
                ga.newInstance(GroupBreakingIterator.class);
                ga.dup();
                allocateStatic(compiler, instr.getSelectExpression());
                Class<? extends Item> keyClass = compiler.compileToItem(instr.getGroupingKey());
                ga.downCast(keyClass, Function.class, true);
                compiler.generateGetContext();
                ga.invokeConstructor(GroupBreakingIterator.class, Expression.class, Function.class, XPathContext.class);
                break;

            default:
                throw new AssertionError("Unknown grouping algorithm");
        }

        // now iterate over the leading nodes of the groups

//        if (sortKeys != null) {
//            AtomicComparer[] comps = sortComparators;
//            XPathContext xpc = context.newMinorContext();
//            if (comps == null) {
//                comps = new AtomicComparer[sortKeys.length];
//                for (int s = 0; s < sortKeys.length; s++) {
//                    comps[s] = sortKeys[s].makeComparator(xpc);
//                }
//            }
//            groupIterator = new SortedGroupIterator(xpc, groupIterator, this, comps);
//        }


//        if (groupBinding != null) {
//            groupIterator.setGroupSlot(groupBinding.getLocalSlotNumber());
//        }
//        if (keyBinding != null) {
//            groupIterator.setKeySlot(keyBinding.getLocalSlotNumber());
//        }
//        if (instr.getGroupBinding() != null) {
//            ga.dup();
//            ga.push(instr.getGroupBinding().getLocalSlotNumber());
//            ga.invokeInstanceMethod(GroupIterator.class, "setGroupSlot", Integer.TYPE);
//        }
//        if (instr.getKeyBinding() != null) {
//            ga.dup();
//            ga.push(instr.getKeyBinding().getLocalSlotNumber());
//            ga.invokeInstanceMethod(GroupIterator.class, "setKeySlot", Integer.TYPE);
//        }
        if (instr.getSortKeyDefinitions() != null) {
            int gpIterVar = methodInfo.allocateLocal(GroupByIterator.class);
            ga.storeLocal(gpIterVar);

            ga.newInstance(SortedGroupIterator.class);
            ga.dup();
            compiler.generateGetContext();
            ga.loadLocal(gpIterVar);
            allocateStatic(compiler, instr);
            if (instr.getSortKeyComparators() == null) {
                int skdVar = SortExpressionCompiler.compileSortKeyDefinitions(compiler, instr.getSortKeyDefinitions());
                ga.loadLocal(skdVar);
            } else {
                allocateStatic(compiler, instr.getSortKeyComparators());
            }
            AtomicComparer[] comp1 = instr.getSortKeyComparators();
            ga.invokeConstructor(SortedGroupIterator.class, XPathContext.class,
                    GroupIterator.class, SortKeyEvaluator.class, AtomicComparer[].class);
            // TODO: the sort keys are being evaluated by the interpreter
//            if (instr.getGroupBinding() != null) {
//                ga.dup();
//                ga.push(instr.getGroupBinding().getLocalSlotNumber());
//                ga.invokeInstanceMethod(GroupIterator.class, "setGroupSlot", Integer.TYPE);
//            }
//            if (instr.getKeyBinding() != null) {
//                ga.dup();
//                ga.push(instr.getKeyBinding().getLocalSlotNumber());
//                ga.invokeInstanceMethod(GroupIterator.class, "setKeySlot", Integer.TYPE);
//            }
            methodInfo.releaseLocal(gpIterVar);
        }
    }

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {

        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        final ForEachGroup instr = (ForEachGroup) expression;

        visitAnnotation(compiler, "ForEachGroupCompiler-Itr");
        visitLineNumber(compiler, ga, expression);

        CompiledContextMappingFunction contextMapFunc =
                SlashExpressionCompiler.generateMappingFunction(compiler, instr.getActionExpression());

//        GroupIterator master = getGroupIterator(context);

        getGroupIterator(compiler, expression);
        int groupIterVar = methodInfo.allocateLocal(GroupIterator.class);
        ga.storeLocal(groupIterVar);

//        XPathContextMajor c2 = context.newContext();
//        c2.setOrigin(this);
//        c2.setCurrentIterator(master);
//        c2.setCurrentGroupIterator(master);
//        c2.setCurrentTemplateRule(null);
//        return new ContextMappingIterator(contextMapFunc, c2);

        ga.newInstance(ContextMappingIterator.class);
        ga.dup();
        allocateStatic(compiler, contextMapFunc);

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newContext");
        ga.dup();
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        ga.loadLocal(groupIterVar);
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        ga.dup();
        ga.loadLocal(groupIterVar);
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentGroupIterator", GroupIterator.class);
        ga.dup();
        ga.pushNull();
        ga.invokeInstanceMethod(XPathContextMajor.class, "setCurrentTemplateRule", Rule.class);

        ga.invokeConstructor(ContextMappingIterator.class, ContextMappingFunction.class, XPathContext.class);

        methodInfo.releaseLocal(groupIterVar);

    }

    private void getStringCollator(CompilerService compiler, ForEachGroup instr) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();

        allocateStatic(compiler, compiler.getConfiguration());
        compiler.compileToPrimitive(instr.getCollationNameExpression(), String.class, OnEmpty.RETURN_EMPTY_STRING);
        allocateStatic(compiler, instr.getBaseURI().toString());
        ga.push("FOCH0002");
        ga.invokeInstanceMethod(Configuration.class, "getCollation", String.class, String.class, String.class);

    }

}

//
// Copyright (c) 2011-2022 Saxonica Limited