using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class AccumulatorManager {
        private readonly Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo,System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData>> accumulatorDataIndex = new Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo,System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData>>();
        private readonly Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo,System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator>> applicableAccumulators = new Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo,System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator>>();
        public AccumulatorManager()  {
        }
        public void setApplicableAccumulators(Saxon.Hej.om.TreeInfo tree, System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators) {
            applicableAccumulators.put(tree, accumulators);
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> getApplicableAccumulators(Saxon.Hej.om.TreeInfo tree) {
            return applicableAccumulators.get(tree);
        }
        public bool isApplicable(Saxon.Hej.om.TreeInfo tree, Saxon.Hej.expr.accum.Accumulator accumulator) {
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accSet = applicableAccumulators.get(tree);
            return ((accSet == null) || accSet.Contains(accumulator));
        }
        private static readonly Saxon.Hej.expr.accum.AccumulatorData MARKER = new Saxon.Hej.expr.accum.AccumulatorData(null);
        public Saxon.Hej.expr.accum.IAccumulatorData getAccumulatorData(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.accum.Accumulator acc, Saxon.Hej.expr.XPathContext context) { lock(this) {
            System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData> map = accumulatorDataIndex.get(doc);
            if ((map != null)) {
                Saxon.Hej.expr.accum.IAccumulatorData data = Saxon.Impl.Helpers.MapUtils.Get(map, acc);
                if ((data != null)) {
                    if ((data == MARKER)) {
                        throw new Saxon.Hej.trans.XPathException((("Accumulator "+acc.getAccumulatorName().getDisplayName())+" requires access to its own value"), "XTDE3400");
                    }
                    return data;
                }
            } else {
                map = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData>();
                accumulatorDataIndex.put(doc, map);
            }
            Saxon.Impl.Helpers.MapUtils.Put(map, acc, MARKER);
            if (((doc is Saxon.Hej.tree.wrapper.VirtualTreeInfo) && ((Saxon.Hej.tree.wrapper.VirtualTreeInfo)(doc)).isCopyAccumulators())) {
                Saxon.Hej.om.NodeInfo original = ((Saxon.Hej.tree.wrapper.VirtualCopy)(doc.getRootNode())).getOriginalNode();
                Saxon.Hej.expr.accum.IAccumulatorData originalData = getAccumulatorData(original.getTreeInfo(), acc, context);
                Saxon.Hej.expr.accum.VirtualAccumulatorData vad = new Saxon.Hej.expr.accum.VirtualAccumulatorData(originalData);
                Saxon.Impl.Helpers.MapUtils.Put(map, acc, vad);
                return vad;
            } else {
                if (((doc is Saxon.Hej.tree.tiny.TinyTree) && (((Saxon.Hej.tree.tiny.TinyTree)(doc)).getCopiedFrom() != null))) {
                    Saxon.Hej.expr.accum.IAccumulatorData original = getAccumulatorData(((Saxon.Hej.tree.tiny.TinyTree)(doc)).getCopiedFrom().getTreeInfo(), acc, context);
                    return new Saxon.Hej.expr.accum.PathMappedAccumulatorData(original, ((Saxon.Hej.tree.tiny.TinyTree)(doc)).getCopiedFrom());
                } else {
                    Saxon.Hej.expr.accum.AccumulatorData d = new Saxon.Hej.expr.accum.AccumulatorData(acc);
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
                    c2.setCurrentComponent(acc.getDeclaringComponent());
                    try {
                        d.buildIndex(doc.getRootNode(), c2);
                        Saxon.Impl.Helpers.MapUtils.Put(map, acc, d);
                        return d;
                    } catch (Saxon.Hej.trans.XPathException err) {
                        Saxon.Hej.expr.accum.IAccumulatorData failed = new Saxon.Hej.expr.accum.FailedAccumulatorData(acc, err);
                        Saxon.Impl.Helpers.MapUtils.Put(map, acc, failed);
                        return failed;
                    }
                }
            }
        }}
        public void addAccumulatorData(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.accum.Accumulator acc, Saxon.Hej.expr.accum.IAccumulatorData accData) { lock(this) {
            System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData> map = accumulatorDataIndex.get(doc);
            if ((map != null)) {
                Saxon.Hej.expr.accum.IAccumulatorData data = Saxon.Impl.Helpers.MapUtils.Get(map, acc);
                if ((data != null)) {
                    return ;
                }
            } else {
                map = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Hej.expr.accum.IAccumulatorData>();
                accumulatorDataIndex.put(doc, map);
            }
            Saxon.Impl.Helpers.MapUtils.Put(map, acc, accData);
        }}
    
    }
}