package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.SimpleNodePushElaborator;
import net.sf.saxon.expr.instruct.ProcessingInstruction;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AtomicValue;

public class ProcessingInstructionElaborator extends SimpleNodePushElaborator {

    @Override
    public PushEvaluator elaborateForPush() {
        final ProcessingInstruction instr = (ProcessingInstruction)getExpression();
        final ItemEvaluator nameEval = makeElaborator(instr.getNameExp()).elaborateForItem();
        final StringEvaluator content = makeElaborator(instr.getSelect()).elaborateForString(true);
        final Location location = instr.getLocation();
        final int properties = 0;

        return (out, context) -> {
            AtomicValue name = (AtomicValue)nameEval.eval(context);
            instr.checkName(name, context);
            UnicodeString value = content.eval(context);
            instr.checkContent(value, context);
            out.processingInstruction(name.toString(), content.eval(context), location, properties);
        };

    }
}

