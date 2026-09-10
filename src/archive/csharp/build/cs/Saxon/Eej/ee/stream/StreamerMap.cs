using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamerMap {

        static StreamerMap() {
            Saxon.Impl.Helpers.MapUtils.Put(map, "AccumulatorAfter", typeof(Saxon.Eej.ee.stream.adjunct.AccumulatorAfterStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "BooleanFn", typeof(Saxon.Eej.ee.stream.adjunct.BooleanFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "CodepointsToString", typeof(Saxon.Eej.ee.stream.adjunct.CodepointsToStringStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Count", typeof(Saxon.Eej.ee.stream.adjunct.CountStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "CurrentGroup", typeof(Saxon.Eej.ee.stream.adjunct.CurrentGroupStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "CurrentMergeGroup", typeof(Saxon.Eej.ee.stream.adjunct.CurrentMergeGroupStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "DeepEqual", typeof(Saxon.Eej.ee.stream.adjunct.DeepEqualStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "DistinctValues", typeof(Saxon.Eej.ee.stream.adjunct.DistinctValuesStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Empty", typeof(Saxon.Eej.ee.stream.adjunct.EmptyStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Exists", typeof(Saxon.Eej.ee.stream.adjunct.ExistsStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "FilterFn", typeof(Saxon.Eej.ee.stream.adjunct.FilterFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Fold", typeof(Saxon.Eej.ee.stream.adjunct.FoldStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "IndexOf", typeof(Saxon.Eej.ee.stream.adjunct.IndexOfStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "InsertBefore", typeof(Saxon.Eej.ee.stream.adjunct.InsertBeforeStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Last", typeof(Saxon.Eej.ee.stream.adjunct.LastStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "MapEntry", typeof(Saxon.Eej.ee.stream.adjunct.MapEntryStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Minimax", typeof(Saxon.Eej.ee.stream.adjunct.MinimaxStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "NewMap", typeof(Saxon.Eej.ee.stream.adjunct.NewMapStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "NotFn", typeof(Saxon.Eej.ee.stream.adjunct.NotFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Outermost", typeof(Saxon.Eej.ee.stream.adjunct.OutermostStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Remove", typeof(Saxon.Eej.ee.stream.adjunct.RemoveStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Reverse", typeof(Saxon.Eej.ee.stream.adjunct.ReverseStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Root", typeof(Saxon.Eej.ee.stream.adjunct.RootStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "SnapshotFn", typeof(Saxon.Eej.ee.stream.adjunct.SnapshotFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "StringFn", typeof(Saxon.Eej.ee.stream.adjunct.StringFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Subsequence", typeof(Saxon.Eej.ee.stream.adjunct.SubsequenceStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "Trace", typeof(Saxon.Eej.ee.stream.adjunct.TraceStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "TreatFn", typeof(Saxon.Eej.ee.stream.adjunct.TreatFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "XmlToJsonFn", typeof(Saxon.Eej.ee.stream.adjunct.XmlToJsonFnStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsUntil", typeof(Saxon.Eej.ee.stream.adjunct.ItemsUntilStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsBefore", typeof(Saxon.Eej.ee.stream.adjunct.ItemsBeforeStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsFrom", typeof(Saxon.Eej.ee.stream.adjunct.ItemsFromStreamer));
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsAfter", typeof(Saxon.Eej.ee.stream.adjunct.ItemsAfterStreamer));
        }
        private static readonly System.Collections.Generic.IDictionary<string,System.Type> map = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.emptyAdjunctMap();
        public static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct getStreamingAdjunct(string f) {
            if ((f == null)) {
                return null;
            }
            try {
                System.Type theClass = Saxon.Impl.Helpers.MapUtils.Get(map, f);
                return ((theClass == null) ? null : (Saxon.Eej.ee.stream.adjunct.StreamingAdjunct)System.Activator.CreateInstance(theClass));
            } catch (System.Exception e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
    
    }
}