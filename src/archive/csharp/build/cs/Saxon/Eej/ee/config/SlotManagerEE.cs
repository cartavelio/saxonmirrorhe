using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class SlotManagerEE : Saxon.Hej.expr.instruct.SlotManager {
        public override void showStackFrame(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.Logger logger) {
            try {
                if ((context != null)) {
                    if ((context.getContextItem() == null)) {
                        logger.info("     Focus: absent");
                    } else {
                        logger.info("     Focus");
                        string item;
                        if ((context.getContextItem() is Saxon.Hej.om.NodeInfo)) {
                            item = Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(context.getContextItem()));
                        } else {
                            item = Saxon.Hej.trans.Err.depict(context.getContextItem()).ToString();
                        }
                        logger.info(("        Context item: "+item));
                        logger.info(("        Context position: "+context.getCurrentIterator().position()));
                    }
                    Saxon.Hej.expr.instruct.ParameterSet locals = context.getLocalParameters();
                    if (((locals != null) && (locals.size()>0))) {
                        logger.info("     Local parameters");
                        showParameterSet(context.getLocalParameters(), logger);
                    }
                    Saxon.Hej.expr.instruct.ParameterSet tunnels = context.getTunnelParameters();
                    if (((tunnels != null) && (tunnels.size()>0))) {
                        logger.info("     Tunnel parameters");
                        showParameterSet(context.getTunnelParameters(), logger);
                    }
                    Saxon.Hej.expr.StackFrame stack = context.getStackFrame();
                    Saxon.Hej.expr.instruct.SlotManager map = stack.getStackFrameMap();
                    Saxon.Hej.om.Sequence[] values = stack.getStackFrameValues();
                    if ((map.getNumberOfVariables()>0)) {
                        logger.info("     Local variables");
                        bool foundOne = false;
                        for (int i = 0; (i<map.getNumberOfVariables()); i++) {
                            if ((values[i] != null)) {
                                logger.info(((("        $"+map.getVariableMap()[i].getDisplayName())+" = ")+Saxon.Hej.trans.Err.depictSequence(values[i])));
                                foundOne = true;
                            }
                        }
                        if (!(foundOne)) {
                            logger.info("        (All variables are null)");
                        }
                    }
                }
            } catch (System.Exception e) {
                System.Console.Error.WriteLine(e);
                System.Diagnostics.Debug.Assert(false);
            }
        }
        private void showParameterSet(Saxon.Hej.expr.instruct.ParameterSet @params, Saxon.Hej.lib.Logger logger) {
            Saxon.Hej.expr.instruct.ParameterSet ps;
            try {
                ps = new Saxon.Hej.expr.instruct.ParameterSet(@params, 0);
                ps.materializeValues();
            } catch (Saxon.Hej.trans.XPathException err) {
                ps = @params;
            }
            for (int i = 0; (i<ps.size()); i++) {
                logger.info(((("        $"+ps.getParameterNames()[i].getDisplayName())+" = ")+Saxon.Hej.trans.Err.depictSequence(ps.getValue(i))));
            }
        }
    
    }
}