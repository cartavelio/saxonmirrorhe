package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.gen.FallbackElaborator;

public class ValueOfElaborator extends FallbackElaborator {

//    @Override
//    public PushEvaluator elaborateForPush() {
//        // TODO: this is highly simplified
//        final ValueOf instr = (ValueOf)getExpression();
//        final boolean createNodeIfEmpty = !instr.isNoNodeIfEmpty();
//        final StringEvaluator content = makeElaborator(instr.getSelect()).elaborateForString(createNodeIfEmpty);
//        final Location location = instr.getLocation();
//        final int properties = 0;
//
//        return (out, context) -> {
//            UnicodeString str = content.eval(context);
//            if (createNodeIfEmpty || str != null) {
//                out.characters(content.eval(context), location, properties);
//            }
//        };
//
//
//
//    }
}

