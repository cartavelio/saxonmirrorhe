package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.SimpleNodePushElaborator;
import net.sf.saxon.expr.instruct.FixedAttribute;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SimpleType;

public class FixedAttributeElaborator extends SimpleNodePushElaborator {

    @Override
    public PushEvaluator elaborateForPush() {
        final FixedAttribute instr = (FixedAttribute) getExpression();
        final StringEvaluator content = makeElaborator(instr.getSelect()).elaborateForString(true);
        final NodeName attName = instr.getAttributeName();
        final SimpleType typeCode = instr.getSchemaType() == null ? BuiltInAtomicType.UNTYPED_ATOMIC : instr.getSchemaType();
        final Location location = instr.getLocation();
        final int properties = 0;

        return (out, context) -> {
            out.attribute(attName, typeCode, content.eval(context).toString(), location, properties);
        };
    }



}

