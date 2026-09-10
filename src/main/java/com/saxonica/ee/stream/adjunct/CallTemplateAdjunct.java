package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.instruct.CallTemplate;
import net.sf.saxon.expr.instruct.NamedTemplate;
import net.sf.saxon.expr.instruct.WithParam;
import net.sf.saxon.expr.parser.ExpressionTool;

import java.util.ArrayList;
import java.util.List;

public class CallTemplateAdjunct extends StreamingAdjunct {


    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        CallTemplate inst = (CallTemplate)getExpression();
        ArrayList<Operand> list = new ArrayList<Operand>(10);
        WithParam.gatherOperands(inst, inst.getActualParams(), list);
        WithParam.gatherOperands(inst, inst.getTunnelParams(), list);

        NamedTemplate t = (NamedTemplate)inst.getTarget().getActor();
        if (!t.isAbsentFocus() && !t.getRequiredContextItemType().isPlainType()) {
            ContextItemExpression cie = new ContextItemExpression();
            ExpressionTool.copyLocationInfo(inst, cie);
            list.add(new Operand(inst, cie, OperandRole.NAVIGATE));
        }
        return Streamability.generalStreamabilityRules(
                inst, list, contextInfo, reasons);
    }




}

// Copyright (c) 2018-2022 Saxonica Limited