using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamerMap {

        static StreamerMap() {
            Saxon.Impl.Helpers.MapUtils.Put(map, "AccumulatorAfter", () => new Saxon.Eej.ee.stream.adjunct.AccumulatorAfterStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "BooleanFn", () => new Saxon.Eej.ee.stream.adjunct.BooleanFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "CodepointsToString", () => new Saxon.Eej.ee.stream.adjunct.CodepointsToStringStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Count", () => new Saxon.Eej.ee.stream.adjunct.CountStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "CurrentGroup", () => new Saxon.Eej.ee.stream.adjunct.CurrentGroupStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "CurrentMergeGroup", () => new Saxon.Eej.ee.stream.adjunct.CurrentMergeGroupStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "DeepEqual", () => new Saxon.Eej.ee.stream.adjunct.DeepEqualStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "DistinctValues", () => new Saxon.Eej.ee.stream.adjunct.DistinctValuesStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Empty", () => new Saxon.Eej.ee.stream.adjunct.EmptyStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Exists", () => new Saxon.Eej.ee.stream.adjunct.ExistsStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "FilterFn", () => new Saxon.Eej.ee.stream.adjunct.FilterFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Fold", () => new Saxon.Eej.ee.stream.adjunct.FoldStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "IndexOf", () => new Saxon.Eej.ee.stream.adjunct.IndexOfStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "InsertBefore", () => new Saxon.Eej.ee.stream.adjunct.InsertBeforeStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Last", () => new Saxon.Eej.ee.stream.adjunct.LastStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "MapEntry", () => new Saxon.Eej.ee.stream.adjunct.MapEntryStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Minimax", () => new Saxon.Eej.ee.stream.adjunct.MinimaxStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "NewMap", () => new Saxon.Eej.ee.stream.adjunct.NewMapStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "NotFn", () => new Saxon.Eej.ee.stream.adjunct.NotFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Outermost", () => new Saxon.Eej.ee.stream.adjunct.OutermostStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Remove", () => new Saxon.Eej.ee.stream.adjunct.RemoveStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Reverse", () => new Saxon.Eej.ee.stream.adjunct.ReverseStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Root", () => new Saxon.Eej.ee.stream.adjunct.RootStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "SnapshotFn", () => new Saxon.Eej.ee.stream.adjunct.SnapshotFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "StringFn", () => new Saxon.Eej.ee.stream.adjunct.StringFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Subsequence", () => new Saxon.Eej.ee.stream.adjunct.SubsequenceStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "Trace", () => new Saxon.Eej.ee.stream.adjunct.TraceStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "TreatFn", () => new Saxon.Eej.ee.stream.adjunct.TreatFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "XmlToJsonFn", () => new Saxon.Eej.ee.stream.adjunct.XmlToJsonFnStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsUntil", () => new Saxon.Eej.ee.stream.adjunct.ItemsUntilStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsBefore", () => new Saxon.Eej.ee.stream.adjunct.ItemsBeforeStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsFrom", () => new Saxon.Eej.ee.stream.adjunct.ItemsFromStreamer());
            Saxon.Impl.Helpers.MapUtils.Put(map, "saxon:ItemsAfter", () => new Saxon.Eej.ee.stream.adjunct.ItemsAfterStreamer());
        }
        private static readonly System.Collections.Generic.IDictionary<string,Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct>> map = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.emptyAdjunctMap();
        public static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct getStreamingAdjunct(string f) {
            if ((f == null)) {
                return null;
            }
            try {
                Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct> supplier = Saxon.Impl.Helpers.MapUtils.Get(map, f);
                return ((supplier == null) ? null : supplier());
            } catch (System.Exception e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
    
    }
}