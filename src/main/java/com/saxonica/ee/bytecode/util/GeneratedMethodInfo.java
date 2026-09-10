package com.saxonica.ee.bytecode.util;

import org.objectweb.asm.Type;

import java.util.*;

/**
 * Information about the current method whose bytecode is being generated
 */
public class GeneratedMethodInfo {

    public GeneratedMethodInfo() {
    }

    private static class ContextVariableInfo {
        int contextVariablePosition;
        boolean contextIsArgument;
    }

    private static class OutputterInfo {
        int outputterPosition;
    }

    public static class LocalSlotInfo {
        public int slotNumber;
        public Class theClass;
        boolean inUse;
    }

    public Generator currentGenerator;
    public boolean debug;

    /*@Nullable*/ private LabelInfo tailCallLabel = null;

    private final Stack<ContextVariableInfo> contextVariableStack = new Stack<>();
    private final Stack<OutputterInfo> outputterStack = new Stack<>();
    public Set<LabelInfo> labelMap = new HashSet<>();
    public List<LocalSlotInfo> slotList = new ArrayList<>(20);

    private final Stack<LabelInfo> xslIterateLoopLabelStack = new Stack<>();
    private final Stack<LabelInfo> xslIterateBreakLabelStack = new Stack<>();

    public void pushContextVariableInfo(int position, boolean isArgument) {
        ContextVariableInfo info = new ContextVariableInfo();
        info.contextVariablePosition = position;
        info.contextIsArgument = isArgument;
        contextVariableStack.push(info);
//        if (debug) {
//            currentGenerator.visitAnnotation("pushContextVar" + position, true);
//        }
    }

    public void pushOutputterInfo(int position) {
        OutputterInfo info = new OutputterInfo();
        info.outputterPosition = position;
        outputterStack.push(info);
    }

    public void popContextVariableInfo() {
        contextVariableStack.pop();
//        ContextVariableInfo info = contextVariableStack.peek();
//        if (debug) {
//            currentGenerator.visitAnnotation("popContextVar" + info.contextVariablePosition, true);
//        }
    }

    public void popOutputterInfo() {
        outputterStack.pop();
    }

    public int getContextVariablePosition() {
        return contextVariableStack.peek().contextVariablePosition;
    }

    public int getOutputterPosition() {
        return outputterStack.peek().outputterPosition;
    }

    public boolean isOutputterInitialized() {
        return outputterStack.get(0).outputterPosition != -1;
    }

    public boolean isContextVariableAnArgument() {
        return contextVariableStack.peek().contextIsArgument;
    }

    public Generator getCurrentGenerator() {
        return currentGenerator;
    }


    /**
     * Allocate a new label within the method
     *
     * @param name a diagnostic name for the label, used if errors are reported
     * @return the constructed label information
     */

    public LabelInfo newLabel(String name) {
        LabelInfo labelInfo = new LabelInfo();
        labelInfo.name = name;
        labelInfo.theLabel = new NamedLabel();
        labelInfo.theLabel.setName(name);
        labelMap.add(labelInfo);
        return labelInfo;
    }

    /**
     * Place a label at the current position in the bytecode
     *
     * @param labelInfo the label to be placed (marked)
     * @throws AssertionError if the label has already been marked
     */

    public void placeLabel(LabelInfo labelInfo) {
        if (labelInfo.isMarked) {
            throw new AssertionError("The Label: " + labelInfo.name + " has already been marked");
        }
        labelInfo.isMarked = true;
        currentGenerator.mark(labelInfo.theLabel);
        generateTracePoint(labelInfo.name);
    }

    /**
     * Place a new label at the current position in the bytecode
     *
     * @param name the name of the label
     * @return the Label that has been placed
     * @throws AssertionError if the label has already been marked
     */

    public LabelInfo placeNewLabel(String name) {
        LabelInfo label = newLabel(name);
        placeLabel(label);
        return label;
    }

    /**
     * Check (on completion of the method) that all labels that have been used have also been marked
     *
     * @throws AssertionError if this is not the case
     */

    public void checkLabels() {
        for (LabelInfo labelInfo : labelMap) {
            if (labelInfo.isUsed() && !labelInfo.isMarked) {
                throw new AssertionError("The Label: " + labelInfo.name + " has been used but not marked");
            }
        }
    }

    /**
     * Get the stack of labels representing entry points to xsl:iterate instructions within this method
     *
     * @return the stack of labels
     */

    public Stack<LabelInfo> getXslIterateLoopLabelStack() {
        return xslIterateLoopLabelStack;
    }

    /**
     * Get the stack of labels representing entry points to xsl:iterate instructions within this method
     *
     * @return the stack of labels
     */

    public Stack<LabelInfo> getXslIterateBreakLabelStack() {
        return xslIterateBreakLabelStack;
    }

    /**
     * Allocate a local bytecode variable. Reuse an existing slot if available
     *
     * @param theClass the class of value to be held in the local variable
     * @return the slot number on the stack to be used for the variable
     */

    public int allocateLocal(Class<?> theClass) {
        for (LocalSlotInfo slot : slotList) {
            if (slot.theClass == theClass && !slot.inUse) {
                //System.err.println("ALLOCATE " + theClass + " - REUSE " + slot.slotNumber);
                slot.inUse = true;
                return slot.slotNumber;
            }
        }
        int t = currentGenerator.newLocal(Type.getType(theClass));
        LocalSlotInfo slot = new LocalSlotInfo();
        slot.theClass = theClass;
        slot.inUse = true;
        slot.slotNumber = t;
        slotList.add(slot);
        //System.err.println("ALLOCATE " + theClass + " - ALLOCATED " + t);
        return t;
    }

    /**
     * Return a local variable to the pool for reuse
     *
     * @param slotNumber the slot number to be released for reuse
     */

    public void releaseLocal(int slotNumber) {
        for (LocalSlotInfo slot : slotList) {
            if (slot.slotNumber == slotNumber) {
                slot.inUse = false;
                return;
            }
        }
    }

    /**
     * Generate instructions to produce a run-time execution trace, provided debugging is enabled
     *
     * @param labelName not currently used
     */

    @SuppressWarnings({"UnusedParameters"})
    private void generateTracePoint(String labelName) {
        if (debug) {
//            if (labelName != null) {
//                currentGenerator.visitAnnotation(labelName, true);
//            }
            currentGenerator.push(CompilerService.tracePoint++);
            currentGenerator.invokeStaticMethod(ByteCodeTrace.class, "trace", Integer.TYPE);
        }
    }

    /**
     * Supply the label to act as the target of a jump for a self-recursive tail call. Before jumping
     * to this label, the code implementing the function call must adjust the stack frame to make sure
     * that it contains the parameters for the new call.
     *
     * @param label the target label for a tail call loop (or null if there is to be no tail call)
     */

    public void setTailCallLabel(/*@Nullable*/ LabelInfo label) {
        tailCallLabel = label;
    }

    /**
     * Get the label to act as the target of a jump for a self-recursive tail call. Before jumping
     * to this label, the code implementing the function call must adjust the stack frame to make sure
     * that it contains the parameters for the new call.
     *
     * @return the designated label (which will be null if not in the body of a tail-recursive function)
     */

    /*@Nullable*/
    public LabelInfo getTailCallLabel() {
        return tailCallLabel;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


