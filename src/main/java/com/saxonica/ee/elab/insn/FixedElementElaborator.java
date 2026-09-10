package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.ComplexNodePushElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ElementCreator;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;

/**
 * Elaborator for a FixedElement (literal result element) expression.
 */

public class FixedElementElaborator extends ComplexNodePushElaborator {
    
    @Override
    public PushEvaluator elaborateForPush() {
        final FixedElement instr = (FixedElement) getExpression();
        final PushEvaluator contentPusher =
                Elaborator.makeElaborator(instr.getContentExpression()).elaborateForPush();
        final ElementCreator.ElementCreationDetails details = new ElementCreator.ElementCreationDetails() {
            @Override
            public NodeName getNodeName(XPathContext context) {
                return instr.getFixedElementName();
            }

            @Override
            public String getSystemId(XPathContext context) {
                return instr.getStaticBaseURIString();
            }

            @Override
            public void processContent(Outputter out, XPathContext context) throws XPathException {
                TailCall tc = contentPusher.process(out, context);
                Expression.dispatchTailCall(tc);
            }
        };

        return (out, context) -> {
            instr.specificProcess(out, context, details);
            return null;
        };
    }



}

