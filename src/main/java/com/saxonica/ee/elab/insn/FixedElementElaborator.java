package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.ComplexNodePushElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.type.SchemaType;

public class FixedElementElaborator extends ComplexNodePushElaborator {

    @Override
    public PushEvaluator getPushEvaluator() {
        //TODO: validation; namespace inheritance properties
        final FixedElement instr = (FixedElement) getExpression();
        final PushEvaluator content = Elaborator.make(instr.getContentExpression()).getPushEvaluator();
        final NodeName elementName = instr.getFixedElementName();
        final SchemaType typeCode = instr.getSchemaType();
        final Location location = instr.getLocation();
        final int properties = 0;
        final NamespaceMap namespaceMap = instr.getActiveNamespaces();

        return (out, context) -> {
            out.startElement(elementName, typeCode, location, properties);
            for (NamespaceBinding ns : namespaceMap) {
                out.namespace(ns.getPrefix(), ns.getURI(), ReceiverOption.NONE);
            }
            content.process(out, context);
            out.endElement();
        };
    }



}

