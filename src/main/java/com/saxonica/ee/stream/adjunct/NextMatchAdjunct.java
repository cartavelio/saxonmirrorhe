package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.watch.NextMatchAction;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Compile an xsl:next-match expression/instruction to streaming code.
 */

public class NextMatchAdjunct extends StreamingAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // See XSLT 3.0 19.8.4.4
        ArrayList<Operand> list = new ArrayList<Operand>(10);
        ContextItemExpression cie = new ContextItemExpression();
        ExpressionTool.copyLocationInfo(getExpression(), cie);
        cie.setStaticInfo(new ContextItemStaticInfoEE(AnyNodeTest.getInstance(), false, Posture.STRIDING));
        list.add(new Operand(getExpression(), cie, new OperandRole(0, OperandUsage.ABSORPTION, SequenceType.SINGLE_NODE)));
        for (Operand o : getExpression().operands()) {
            list.add(o);
        }
        return Streamability.generalStreamabilityRules(getExpression(), list, contextInfo, reasons);
    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        return (watchManager, out, context) -> {
            Component.M actualMode = context.getCurrentMode();
            NextMatchAction actor = new NextMatchAction(getExpression(), out, context);
            actor.setMode(actualMode);
            actor.setWatchManager(watchManager);
            return new Trigger(AnchorPattern.getInstance(), actor, context);
        };
    }

}

// Copyright (c) 2009-2022 Saxonica Limited