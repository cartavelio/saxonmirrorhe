package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.SimpleNodePushElaborator;
import net.sf.saxon.expr.instruct.Comment;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;

public class CommentElaborator extends SimpleNodePushElaborator {

    @Override
    public PushEvaluator getPushEvaluator() {
        // TODO: this is highly simplified
        final Comment instr = (Comment)getExpression();
        final StringEvaluator content = Elaborator.make(instr.getSelect()).getStringEvaluator(null);
        final Location location = instr.getLocation();
        final int properties = 0;

        return (out, context) -> {
            UnicodeString value = content.eval(context);
            instr.checkContent(value, context);
            out.comment(content.eval(context), location, properties);
        };



    }
}

