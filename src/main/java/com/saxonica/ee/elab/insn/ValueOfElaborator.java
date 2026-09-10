package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.SimpleNodePushElaborator;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.s9api.Location;

public class ValueOfElaborator extends SimpleNodePushElaborator {

    @Override
    public PushEvaluator getPushEvaluator() {
        // TODO: this is highly simplified
        final ValueOf instr = (ValueOf)getExpression();
        final StringEvaluator content = Elaborator.make(instr.getSelect()).getStringEvaluator(null);
        final Location location = instr.getLocation();
        final int properties = 0;

        return (out, context) -> {
            out.characters(content.eval(context), location, properties);
        };



    }
}

