package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.SimpleNodePushElaborator;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;

public class ValueOfElaborator extends SimpleNodePushElaborator {

    @Override
    public PushEvaluator elaborateForPush() {
        // TODO: this is highly simplified
        final ValueOf instr = (ValueOf)getExpression();
        final boolean createNodeIfEmpty = !instr.isNoNodeIfEmpty();
        final StringEvaluator content = makeElaborator(instr.getSelect()).elaborateForString(createNodeIfEmpty);
        final Location location = instr.getLocation();
        final int properties = 0;

        return (out, context) -> {
            UnicodeString str = content.eval(context);
            if (createNodeIfEmpty || str != null) {
                out.characters(content.eval(context), location, properties);
            }
        };



    }
}

