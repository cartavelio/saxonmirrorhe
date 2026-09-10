package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.ComplexNodePushElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.instruct.DocumentInstr;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.value.TextFragmentValue;

public class DocumentInstrElaborator extends ComplexNodePushElaborator {

    @Override
    public PushEvaluator elaborateForPush() {
        DocumentInstr instr = (DocumentInstr) getExpression();
        if (instr.isTextOnly()) {
            if (instr.getConstantText() != null) {
                NodeInfo node = TextFragmentValue.makeTextFragment(
                        getConfiguration(), instr.getConstantText(), instr.getStaticBaseURIString());
                return (out, context) -> {
                    out.append(node);
                };
            } else {
                PullEvaluator contentPuller = Elaborator.makeElaborator(instr.getContentExpression()).elaborateForPull();
                return (out, context) -> {
                    UnicodeBuilder sb = new UnicodeBuilder();
                    SequenceIterator iter = contentPuller.eval(context);
                    Item item;
                    while ((item = iter.next()) != null) {
                        sb.accept(item.getUnicodeStringValue());
                    }
                    NodeInfo node = TextFragmentValue.makeTextFragment(
                            getConfiguration(), sb.toUnicodeString(), instr.getStaticBaseURIString());
                    out.append(node);
                };
            }
        } else if (instr.isPreservingTypes()) {
            PushEvaluator content = makeElaborator(instr.getContentExpression()).elaborateForPush();
            return (out, context) -> {
                out.startDocument(0);
                content.process(out, context);
                out.endDocument();
            };
        } else {
            // fallback
            //noinspection Convert2MethodRef
            return (out, context) -> instr.process(out, context);
        }
    }



}

