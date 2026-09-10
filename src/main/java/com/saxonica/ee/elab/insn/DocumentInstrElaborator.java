package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.ComplexNodePushElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.instruct.DocumentInstr;

public class DocumentInstrElaborator extends ComplexNodePushElaborator {

    @Override
    public PushEvaluator getPushEvaluator() {
        DocumentInstr instr = (DocumentInstr) getExpression();
        PushEvaluator content = Elaborator.make(instr.getContentExpression()).getPushEvaluator();
        return (out, context) -> {
            out.startDocument(0);
            content.process(out, context);
            out.endDocument();
        };
    }



}

