package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpInnerClass;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions with streamability="filter"
 */

public class FilterFunctionCallAdjunct extends TransmissionAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();


        Operand[] operanda = exp.getOperanda().copy();
        operanda[0].setUsage(OperandUsage.TRANSMISSION);
        for (int i = 1; i < operanda.length; i++) {
            operanda[i].setUsage(Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
        }

        return Streamability.generalStreamabilityRules(
                exp, Arrays.asList(operanda), contextInfo, reasons);

    }

    @Override
    @CSharpInnerClass(outer=true, extra={"Saxon.Hej.om.Sequence[] argValues",
                                         "Saxon.Hej.expr.Component targetComponent",
                                         "Saxon.Hej.expr.instruct.UserFunction target"})
    public ItemFeed makeItemFeed(WatchManager watchManager, final ItemFeed resultFeed, final XPathContext context) throws XPathException {
        UserFunctionCall exp = (UserFunctionCall) getExpression();
        final Component targetComponent = exp.getTargetComponent(context);
        final UserFunction target = exp.getTargetFunction(context);
        final Sequence[] argValues = SequenceTool.makeSequenceArray(target.getArity());
        for (int i=1; i<argValues.length; i++) {
            try {
                argValues[i] = SequenceTool.toGroundedValue(exp.getArg(i).iterate(context));
            } catch (UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        return new ItemFeed(resultFeed, context) {

            @Override
            public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
                argValues[0] = node;
                XPathContextMajor c2 = getContext().newCleanContext();
                c2.setCurrentComponent(targetComponent);
                Sequence result =
                        ((UserFunction)targetComponent.getActor()).call(c2, argValues);
                Item it;
                SequenceIterator iter = result.iterate();
                while ((it = iter.next()) != null) {
                    if (it instanceof FleetingParentNode) {
                        getResultFeed().startSelectedParentNode((FleetingParentNode) it, locationId);
                    } else {
                        getResultFeed().append(it);
                    }
                };
                return null;
            }

            @Override
            public void append(Item item) throws XPathException {
                argValues[0] = item;
                XPathContext c2 = getContext().newCleanContext();
                // TODO: why do we call target rather than targetComponent.getActor() as above?
                Sequence result = target.call(c2, argValues);
                processItems(result.iterate(), getResultFeed());
            }
        };
    }


}

// Copyright (c) 2018-2023 Saxonica Limited