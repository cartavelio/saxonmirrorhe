package com.saxonica.ee.s9api;

import com.saxonica.ee.validate.StatisticsReporter;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ValidationStatisticsRecipient;
import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaComponent;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Class for collection validation statistics produced via s9api processing
 */
public class ValidationStatistics {

    private final List<Map<SchemaComponent, Integer>> collectedStats = new ArrayList<Map<SchemaComponent, Integer>>();

    protected ValidationStatisticsRecipient recipient = new ValidationStatisticsRecipient() {
        public void notifyValidationStatistics(Map<SchemaComponent, Integer> statistics) throws XPathException {
            collectedStats.add(statistics);
        }
    };

    public void report(Processor processor, Destination destination) throws SaxonApiException {
        try {
            if (collectedStats.size() > 0) {
                Configuration config = processor.getUnderlyingConfiguration();
                Receiver receiver = destination.getReceiver(
                        config.makePipelineConfiguration(),
                        config.obtainDefaultSerializationProperties());
                StatisticsReporter reporter = new StatisticsReporter();
                reporter.setReceiver(receiver);
                reporter.notifyValidationStatistics(collectedStats.get(collectedStats.size() - 1));
            }
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }
}

// Copyright (c) 2018 Saxonica Limited.
