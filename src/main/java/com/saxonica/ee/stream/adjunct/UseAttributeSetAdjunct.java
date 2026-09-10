package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.instruct.UseAttributeSet;

import java.util.List;

public class UseAttributeSetAdjunct extends StreamingAdjunct {


    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        UseAttributeSet exp = (UseAttributeSet)getExpression();
        if (!exp.isDeclaredStreamable()) {
            if (reasons != null) {
                reasons.add("Attribute set " + exp.getTargetAttributeSetName().getDisplayName() + " is not declared streamable");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        return super.computeStreamability(contextInfo, reasons);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited