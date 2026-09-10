package com.saxonica.ee.stream;


import com.saxonica.ee.stream.adjunct.*;

import java.util.Map;

public class StreamerMap {

    private static final Map<String, Class<? extends StreamingAdjunct>> map = StreamingAdjunct.emptyAdjunctMap();

    static {
        map.put("AccumulatorAfter", AccumulatorAfterStreamer.class);
        map.put("BooleanFn", BooleanFnStreamer.class);
        map.put("CodepointsToString", CodepointsToStringStreamer.class);
        map.put("Count", CountStreamer.class);
        map.put("CurrentGroup", CurrentGroupStreamer.class);
        map.put("CurrentMergeGroup", CurrentMergeGroupStreamer.class);
        map.put("DeepEqual", DeepEqualStreamer.class);
        map.put("DistinctValues", DistinctValuesStreamer.class);
        map.put("Empty", EmptyStreamer.class);
        map.put("Exists", ExistsStreamer.class);
        map.put("FilterFn", FilterFnStreamer.class);
        map.put("Fold", FoldStreamer.class);
        map.put("IndexOf", IndexOfStreamer.class);
        map.put("InsertBefore", InsertBeforeStreamer.class);
        map.put("Last", LastStreamer.class);
        map.put("MapEntry", MapEntryStreamer.class);
        map.put("Minimax", MinimaxStreamer.class);
        map.put("NewMap", NewMapStreamer.class);
        map.put("NotFn", NotFnStreamer.class);
        map.put("Outermost", OutermostStreamer.class);
        map.put("Remove", RemoveStreamer.class);
        map.put("Reverse", ReverseStreamer.class);
        map.put("Root", RootStreamer.class);
        map.put("SnapshotFn", SnapshotFnStreamer.class);
        map.put("StringFn", StringFnStreamer.class);
        map.put("Subsequence", SubsequenceStreamer.class);
        map.put("Trace", TraceStreamer.class);
        map.put("TreatFn", TreatFnStreamer.class);
        map.put("XmlToJsonFn", XmlToJsonFnStreamer.class);

        map.put("saxon:ItemsUntil", ItemsUntilStreamer.class);
        map.put("saxon:ItemsBefore", ItemsBeforeStreamer.class);
        map.put("saxon:ItemsFrom", ItemsFromStreamer.class);
        map.put("saxon:ItemsAfter", ItemsAfterStreamer.class);

    }

    public static StreamingAdjunct getStreamingAdjunct(String f) {
        if (f == null) {
            return null;
        }
        try {
            Class<? extends StreamingAdjunct> theClass = map.get(f);
            return theClass == null ? null : theClass.newInstance();
        } catch (Exception e) {
            throw new AssertionError(e);
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited


