using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class ShallowCopyAllRuleSet : Saxon.Hej.trans.rules.ShallowCopyRuleSet {
        private static readonly Saxon.Hej.trans.rules.ShallowCopyAllRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.ShallowCopyAllRuleSet();
        public static new Saxon.Hej.trans.rules.ShallowCopyAllRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private ShallowCopyAllRuleSet()  {
        }
        public override void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                Saxon.Hej.@event.SequenceCollector collector = context.getController().allocateSequenceOutputter();
                Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(collector);
                Saxon.Hej.@event.ProxyOutputter checker = new Saxon.Hej.trans.rules.ShallowCopyAllRuleSet.ShallowCopyProxyOutputterForArrays(cco, locationId);
                collector.setSystemId(@out.getSystemId());
                Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.ma.arrays.ArrayItem)(item)).parcels();
                Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                c2.setOrigin(this);
                c2.trackFocus(iter);
                c2.setCurrentComponent(c2.getCurrentMode());
                pipe.setXPathContext(c2);
                Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, checker, c2, locationId);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
                pipe.setXPathContext(context);
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                foreach (Saxon.Hej.om.Item it in collector.getList()) {
                    members.Add(((Saxon.Hej.ma.map.MapItem)(it)).get(new Saxon.Hej.value.StringValue("value")));
                }
                Saxon.Hej.ma.arrays.SimpleArrayItem newArray = new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
                @out.append(newArray, locationId, 0);
            } else {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    int size = ((Saxon.Hej.ma.map.MapItem)(item)).size();
                    if ((size == 1)) {
                        Saxon.Hej.value.AtomicValue key = null;
                        Saxon.Hej.om.GroundedValue singletonValue = null;
                        foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                            key = pair.key;
                            singletonValue = pair.value;
                            break ;
                        }
                        Saxon.Hej.@event.SequenceCollector collector = context.getController().allocateSequenceOutputter();
                        Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(collector);
                        collector.setSystemId(@out.getSystemId());
                        Saxon.Hej.om.SequenceIterator iter = singletonValue.iterate();
                        Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setOrigin(this);
                        c2.trackFocus(iter);
                        c2.setCurrentComponent(c2.getCurrentMode());
                        pipe.setXPathContext(c2);
                        Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, cco, c2, locationId);
                        while ((tc != null)) {
                            tc = tc.processLeavingTail();
                        }
                        pipe.setXPathContext(context);
                        Saxon.Hej.ma.map.MapItem singletonMap = new Saxon.Hej.ma.map.SingleEntryMap(key, collector.getSequence());
                        @out.append(singletonMap, locationId, 0);
                    } else {
                        if ((size>1)) {
                            Saxon.Hej.@event.SequenceCollector collector = context.getController().allocateSequenceOutputter();
                            Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(collector);
                            Saxon.Hej.@event.ProxyOutputter checker = new Saxon.Hej.trans.rules.ShallowCopyAllRuleSet.ShallowCopyProxyOutputterForMaps(cco, locationId);
                            collector.setSystemId(@out.getSystemId());
                            Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.ma.map.MapItem)(item)).entries();
                            Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                            c2.setOrigin(this);
                            c2.trackFocus(iter);
                            c2.setCurrentComponent(c2.getCurrentMode());
                            pipe.setXPathContext(c2);
                            Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, checker, c2, locationId);
                            while ((tc != null)) {
                                tc = tc.processLeavingTail();
                            }
                            pipe.setXPathContext(context);
                            Saxon.Hej.ma.map.MapItem mergedMap = Saxon.Hej.ma.map.MapFunctionSet.MapMerge.mergeMaps(collector.iterate(), context, "use-last", null, null);
                            @out.append(mergedMap, locationId, 0);
                        }
                    }
                } else {
                    Saxon.Hej.@event.SequenceCollector collector = context.getController().allocateSequenceOutputter();
                    Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(collector);
                    collector.setSystemId(@out.getSystemId());
                    base.process(item, parameters, tunnelParams, cco, context, locationId);
                    Saxon.Hej.om.SequenceIterator resultIter = collector.iterate();
                    Saxon.Hej.om.Item resultItem;
                    while (((resultItem = resultIter.next()) != null)) {
                        @out.append(resultItem);
                    }
                }
            }
        }
        public override string getName() {
            return "shallow-copy-all";
        }
        private class ShallowCopyProxyOutputterForMaps : Saxon.Hej.@event.ProxyOutputter {
            private readonly Saxon.Hej.s9api.Location locationId;
            public ShallowCopyProxyOutputterForMaps(Saxon.Hej.@event.ComplexContentOutputter cco, Saxon.Hej.s9api.Location locationId) : base (cco) {
                this.locationId = locationId;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    base.append(item);
                } else {
                    mustBeParcel(locationId);
                }
            }
            public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    base.append(item, locationId, properties);
                } else {
                    mustBeParcel(locationId);
                }
            }
            private void mustBeParcel(Saxon.Hej.s9api.Location locationId) {
                throw new Saxon.Hej.trans.XPathException(("Template rule invoked when processing a map using the "+"shallow-copy-all rule must return a sequence of maps"), "XPTY0004", locationId);
            }
        
        }
        private class ShallowCopyProxyOutputterForArrays : Saxon.Hej.@event.ProxyOutputter {
            private readonly Saxon.Hej.s9api.Location locationId;
            public ShallowCopyProxyOutputterForArrays(Saxon.Hej.@event.ComplexContentOutputter cco, Saxon.Hej.s9api.Location locationId) : base (cco) {
                this.locationId = locationId;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (Saxon.Hej.ma.map.RecordTest.VALUE_RECORD.matches(item, getConfiguration().getTypeHierarchy())) {
                    base.append(item);
                } else {
                    mustBeValueRecord(locationId);
                }
            }
            public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
                if (Saxon.Hej.ma.map.RecordTest.VALUE_RECORD.matches(item, getConfiguration().getTypeHierarchy())) {
                    base.append(item, locationId, properties);
                } else {
                    mustBeValueRecord(locationId);
                }
            }
            private void mustBeValueRecord(Saxon.Hej.s9api.Location locationId) {
                throw new Saxon.Hej.trans.XPathException(("Template rule invoked when processing an array using the "+"shallow-copy-all rule must return a sequence of value records"), "XPTY0004", locationId);
            }
        
        }
    
    }
}