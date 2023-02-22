using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class ShallowSkipAllRuleSet : Saxon.Hej.trans.rules.ShallowSkipRuleSet {
        private static readonly Saxon.Hej.trans.rules.ShallowSkipAllRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.ShallowSkipAllRuleSet();
        public static new Saxon.Hej.trans.rules.ShallowSkipAllRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private ShallowSkipAllRuleSet()  {
        }
        public override void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.ma.arrays.ArrayItem)(item)).parcels();
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                c2.setOrigin(this);
                c2.trackFocus(iter);
                c2.setCurrentComponent(c2.getCurrentMode());
                Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, @out, c2, locationId);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
            } else {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    int size = ((Saxon.Hej.ma.map.MapItem)(item)).size();
                    if ((size<=1)) {
                        return ;
                    }
                    Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.ma.map.MapItem)(item)).entries();
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setOrigin(this);
                    c2.trackFocus(iter);
                    c2.setCurrentComponent(c2.getCurrentMode());
                    Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, @out, c2, locationId);
                    while ((tc != null)) {
                        tc = tc.processLeavingTail();
                    }
                } else {
                    base.process(item, parameters, tunnelParams, @out, context, locationId);
                }
            }
        }
        public override string getName() {
            return "shallow-skip-all";
        }
    
    }
}