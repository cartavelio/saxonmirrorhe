package com.saxonica.ee.config;

import net.sf.saxon.expr.Assignation;
import net.sf.saxon.expr.LocalBinding;
import net.sf.saxon.expr.StackFrame;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.z.IntHashMap;

public class SlotManagerEE extends SlotManager {

    private IntHashMap<LocalBinding> bindings = new IntHashMap<>();

    /**
     * Allocate a slot number for a variable
     *
     * @param qName   the name of the variable
     * @param binding the binding of the variable (only used in Saxon-EE)
     * @return the allocated slot number (the next one available)
     */

    public int allocateSlotNumber(StructuredQName qName, LocalBinding binding) {
        int slot = super.allocateSlotNumber(qName, binding);
        if (binding != null) {
            bindings.put(slot, binding);
        }
        return slot;
    }


    /**
     * Display the values of the variables and parameters in an XPathContext stack frame
     * to a Logger
     *
     * @param context the XPathContext holding the stack frame
     * @param logger  the destination for the output
     */

    @Override
    public void showStackFrame(XPathContext context, Logger logger) {
        try {
            if (context != null) {
                if (context.getContextItem() == null) {
                    logger.info("     Focus: absent");
                } else {
                    logger.info("     Focus");
                    String item;
                    if (context.getContextItem() instanceof NodeInfo) {
                        item = Navigator.getPath((NodeInfo)context.getContextItem());
                    } else {
                        item = Err.depict(context.getContextItem()).toString();
                    }
                    logger.info("        Context item: " + item);
                    logger.info("        Context position: " + context.getCurrentIterator().position());
                }
                ParameterSet locals = context.getLocalParameters();
                if (locals != null && locals.size() > 0) {
                    logger.info("     Local parameters");
                    showParameterSet(context.getLocalParameters(), logger);
                }
                ParameterSet tunnels = context.getTunnelParameters();
                if (tunnels != null && tunnels.size() > 0) {
                    logger.info("     Tunnel parameters");
                    showParameterSet(context.getTunnelParameters(), logger);
                }
                StackFrame stack = context.getStackFrame();
                SlotManager map = stack.getStackFrameMap();
                Sequence[] values = stack.getStackFrameValues();
                if (map.getNumberOfVariables() != values.length) {
                    logger.info("     Inconsistent stack frame: expecting " + map.getNumberOfVariables() + " variables, but found " + values.length);
                    return;
                }
                if (map.getNumberOfVariables() > 0) {
                    logger.info("     Local variables");
                    boolean foundOne = false;
                    for (int i = 0; i < map.getNumberOfVariables(); i++) {
                        if (values[i] != null) {
                            StructuredQName qName = map.getVariableMap().get(i);
                            String expr = "";
                            LocalBinding binding = bindings.get(i);
                            if (qName.hasURI(NamespaceUri.SAXON_GENERATED_VARIABLE) && binding instanceof Assignation) {
                                expr = " {" + ((Assignation)binding).getSequence().toShortString() + "}";
                            }
                            logger.info("        $" + qName.getDisplayName() + expr + " = " + Err.depictSequence(values[i]));
                            foundOne = true;
                        }
                    }
                    if (!foundOne) {
                        logger.info("        (All variables are null)");
                    }
                }
            }
        } catch (Exception e) {
            //e.printStackTrace();
            assert false;
            // better to display nothing than to crash when producing extra diagnostics
        }
    }

    private void showParameterSet(ParameterSet params, Logger logger) {
        ParameterSet ps;
        try {
            ps = new ParameterSet(params, 0);
            ps.materializeValues();
        } catch (XPathException err) {
            ps = params;
        }
        for (int i = 0; i < ps.size(); i++) {
            logger.info("        $" + ps.getParameterNames()[i].getDisplayName() +
                                " = " + Err.depictSequence(ps.getValue(i)));
        }
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

