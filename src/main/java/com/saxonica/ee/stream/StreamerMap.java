package com.saxonica.ee.stream;


import com.saxonica.ee.stream.adjunct.*;

import java.util.Map;
import java.util.function.Supplier;

@SuppressWarnings("Convert2MethodRef")
public class StreamerMap {

    private static final Map<String, Supplier<StreamingAdjunct>> map = StreamingAdjunct.emptyAdjunctMap();

    static {
        map.put("AccumulatorAfter", () -> new AccumulatorAfterStreamer());
        map.put("BooleanFn", () -> new BooleanFnStreamer());
        map.put("CodepointsToString", () -> new CodepointsToStringStreamer());
        map.put("Count", () -> new CountStreamer());
        map.put("CurrentGroup", () -> new CurrentGroupStreamer());
        map.put("CurrentMergeGroup", () -> new CurrentMergeGroupStreamer());
        map.put("DeepEqual", () -> new DeepEqualStreamer());
        map.put("DistinctValues", () -> new DistinctValuesStreamer());
        map.put("Empty", () -> new EmptyStreamer());
        map.put("Exists", () -> new ExistsStreamer());
        map.put("FilterFn", () -> new FilterFnStreamer());
        map.put("Fold", () -> new FoldStreamer());
        map.put("IndexOf", () -> new IndexOfStreamer());
        map.put("InsertBefore", () -> new InsertBeforeStreamer());
        map.put("Last", () -> new LastStreamer());
        map.put("MapEntry", () -> new MapEntryStreamer());
        map.put("Minimax", () -> new MinimaxStreamer());
        map.put("NewMap", () -> new NewMapStreamer());
        map.put("NotFn", () -> new NotFnStreamer());
        map.put("Outermost", () -> new OutermostStreamer());
        map.put("Remove", () -> new RemoveStreamer());
        map.put("Reverse", () -> new ReverseStreamer());
        map.put("Root", () -> new RootStreamer());
        map.put("SnapshotFn", () -> new SnapshotFnStreamer());
        map.put("StringFn", () -> new StringFnStreamer());
        map.put("Subsequence", () -> new SubsequenceStreamer());
        map.put("Trace", () -> new TraceStreamer());
        map.put("TreatFn", () -> new TreatFnStreamer());
        map.put("XmlToJsonFn", () -> new XmlToJsonFnStreamer());

        map.put("saxon:ItemsUntil", () -> new ItemsUntilStreamer());
        map.put("saxon:ItemsBefore", () -> new ItemsBeforeStreamer());
        map.put("saxon:ItemsFrom", () -> new ItemsFromStreamer());
        map.put("saxon:ItemsAfter", () -> new ItemsAfterStreamer());

    }

    public static StreamingAdjunct getStreamingAdjunct(String f) {
        if (f == null) {
            return null;
        }
        try {
            Supplier<StreamingAdjunct> supplier = map.get(f);
            return supplier == null ? null : supplier.get();
        } catch (Exception e) {
            throw new AssertionError(e);
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited


