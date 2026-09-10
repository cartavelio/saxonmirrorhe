package com.saxonica.ee.config;

import net.sf.saxon.expr.StackFrame;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;

public class SlotManagerEE extends SlotManager {

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
                if (map.getNumberOfVariables() > 0) {
                    logger.info("     Local variables");
                    boolean foundOne = false;
                    for (int i = 0; i < map.getNumberOfVariables(); i++) {
                        if (values[i] != null) {
                            logger.info("        $" + map.getVariableMap().get(i).getDisplayName() +
                                                " = " + Err.depictSequence(values[i]));
                            foundOne = true;
                        }
                    }
                    if (!foundOne) {
                        logger.info("        (All variables are null)");
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
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

// Copyright (c) 2020-2022 Saxonica Limited

