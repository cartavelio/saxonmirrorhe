package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.CallTemplate;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.instruct.NamedTemplate;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

public class CallTemplateElaborator extends PullElaborator {

    // TODO: tail recursion

    @Override
    public PullEvaluator elaborateForPull() {
        ForEach forEach = (ForEach) getExpression();
        PullEvaluator select = makeElaborator(forEach.getSelectExpression()).elaborateForPull();
        PullEvaluator action = makeElaborator(forEach.getActionExpression()).elaborateForPull();
        ContextMappingFunction mapper = context -> action.eval(context);
        return context -> {
            XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(select.eval(context));
            return new ContextMappingIterator(mapper, c2);
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        CallTemplate insn = (CallTemplate)getExpression();
        Component fixedTarget = insn.getFixedTarget();
        int bindingSlot = insn.getBindingSlot();
        return (out, context) -> {
            Component target = fixedTarget;
            if (bindingSlot >= 0) {
                target = context.getTargetComponent(bindingSlot);
                if (target.isHiddenAbstractComponent()) {
                    XPathException err = new XPathException("Cannot call an abstract template (" +
                                                                    insn.getCalledTemplateName() +
                                                                    ") with no implementation", "XTDE3052");
                    err.setLocation(insn.getLocation());
                    throw err;
                }
            }
            NamedTemplate t = (NamedTemplate) target.getActor();
            XPathContextMajor c2 = context.newContext();
            c2.setCurrentComponent(target);
            c2.setOrigin(insn);
            c2.openStackFrame(t.getStackFrameMap());
            // TODO: params
            //c2.setLocalParameters(insn.assembleParams(context, actualParams));
            //c2.setTunnelParameters(insn.assembleTunnelParams(context, tunnelParams));
//            if (insn.isWithinDeclaredStreamableConstruct) {
//                c2.setCurrentGroupIterator(null);
//            }
            c2.setCurrentMergeGroupIterator(null);

            try {
                TailCall tc = t.expand(out, c2);
                while (tc != null) {
                    tc = tc.processLeavingTail();
                }
            } catch (StackOverflowError e) {
                XPathException err = new XPathException.StackOverflow(
                        "Too many nested template or function calls. The stylesheet may be looping.",
                        SaxonErrorCode.SXLM0001, insn.getLocation());
                err.setXPathContext(context);
                throw err;
            }

        };

    }
}

