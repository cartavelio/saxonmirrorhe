package com.saxonica.ee.stream;

import com.saxonica.ee.stream.feed.Feed;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * A SequenceWriter that can be used on a streaming pipeline to direct the output of an operation to the next
 * expression in the pipeline
 */
public class StreamingSequenceWriter extends SequenceWriter {

    private Feed out;

    public StreamingSequenceWriter(PipelineConfiguration pipe, Feed out) {
        super(pipe);
        this.out = out;
    }

    @Override
    public void write(Item item) throws XPathException {
        out.processItem(item);
    }
}

// Copyright (c) 2018 Saxonica Limited.
