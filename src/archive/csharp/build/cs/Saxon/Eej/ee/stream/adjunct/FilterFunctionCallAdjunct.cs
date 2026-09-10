using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class FilterFunctionCallAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction target = exp.getFunction();
            Saxon.Hej.expr.Operand[] operanda = exp.getOperanda().copy();
            operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.TRANSMISSION);
            for (int i = 1; (i<operanda.Length); i++) {
                operanda[i].setUsage(Saxon.Hej.expr.Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
            }
            return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, Saxon.Ejava.util.Arrays.asList(operanda), contextInfo, reasons);
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed resultFeed, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.Component targetComponent = exp.getTargetComponent(context);
            Saxon.Hej.expr.instruct.UserFunction target = exp.getTargetFunction(context);
            Saxon.Hej.om.Sequence[] argValues = Saxon.Hej.om.SequenceTool.makeSequenceArray(target.getArity());
            for (int i = 1; (i<argValues.Length); i++) {
                argValues[i] = Saxon.Hej.om.SequenceTool.toGroundedValue(exp.getArg(i).iterate(context));
            }
            return new Inner_ItemFeed_17(this, resultFeed, context, argValues, targetComponent, target);
        }
    
        private class Inner_ItemFeed_17 : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly FilterFunctionCallAdjunct outerInstance;
            public Inner_ItemFeed_17(FilterFunctionCallAdjunct outerInstance, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg1, /*AA*/Saxon.Hej.expr.XPathContext arg2, Saxon.Hej.om.Sequence[] argValues, Saxon.Hej.expr.Component targetComponent, Saxon.Hej.expr.instruct.UserFunction target) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
                this.argValues = argValues;
                this.targetComponent = targetComponent;
                this.target = target;
            }
            private Saxon.Hej.om.Sequence[] argValues;
            private Saxon.Hej.expr.Component targetComponent;
            private Saxon.Hej.expr.instruct.UserFunction target;
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                /*extraParam*/argValues[0] = /*localparam*/node;
                Saxon.Hej.expr.XPathContextMajor c2 = /*superMethod*/getContext().newCleanContext();
                /*localvar*/c2.setCurrentComponent(/*extraParam*/targetComponent);
                Saxon.Hej.om.Sequence result = ((Saxon.Hej.expr.instruct.UserFunction)(/*extraParam*/targetComponent.getActor())).call(/*localvar*/c2, /*extraParam*/argValues);
                Saxon.Hej.om.Item it;
                Saxon.Hej.om.SequenceIterator iter = /*localvar*/result.iterate();
                while (((/*localvar*/it = /*localvar*/iter.next()) != null)) {
                    if ((/*localvar*/it is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                        /*superMethod*/getResultFeed().startSelectedParentNode((Saxon.Eej.ee.stream.om.FleetingParentNode)(/*localvar*/it), /*localparam*/locationId);
                    } else {
                        /*superMethod*/getResultFeed().append(/*localvar*/it);
                    }
                }
                return null;
            }
            public override void append(Saxon.Hej.om.Item item) {
                /*extraParam*/argValues[0] = /*localparam*/item;
                Saxon.Hej.expr.XPathContext c2 = /*superMethod*/getContext().newCleanContext();
                Saxon.Hej.om.Sequence result = /*extraParam*/target.call(/*localvar*/c2, /*extraParam*/argValues);
                /*superMethod*/processItems(/*localvar*/result.iterate(), /*superMethod*/getResultFeed());
            }
        }

    }
}