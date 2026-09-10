package com.saxonica.ee.stream;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * A SequenceWriter that can be used on a streaming pipeline to direct the output of an operation to the next
 * expression in the pipeline
 */
public class StreamingSequenceWriter extends SequenceWriter {

    private final Outputter out;

    public StreamingSequenceWriter(PipelineConfiguration pipe, Outputter out) {
        super(pipe);
        this.out = out;
    }

    @Override
    public void write(Item item) throws XPathException {
        out.append(item);
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
