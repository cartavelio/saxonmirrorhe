package com.saxonica.ee.s9api;

import com.saxonica.ee.validate.StatisticsReporter;
import net.sf.saxon.lib.ValidationStatisticsRecipient;
import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.type.SchemaComponent;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Class for collection validation statistics produced via s9api processing
 */
public class ValidationStatistics {

    private final List<Map<SchemaComponent, Integer>> collectedStats = new ArrayList<>();

    private ValidationStatisticsRecipient recipient;

    public ValidationStatisticsRecipient getRecipient() {
        if (recipient == null) {
            recipient = new ValidationStatisticsRecipient() {
                @Override
                public void notifyValidationStatistics(Map<SchemaComponent, Integer> statistics) {
                    collectedStats.add(statistics);
                }
            };
        }
        return recipient;
    }

    //#if CSHARP==false
    public void report(Processor processor, Destination destination) throws SaxonApiException {
        if (collectedStats.size() > 0) {
            StatisticsReporter reporter = new StatisticsReporter();
            reporter.setReceiver(processor, destination);
            reporter.notifyValidationStatistics(collectedStats.get(collectedStats.size() - 1));
        }
    }
    //#endif
}

// Copyright (c) 2018-2022 Saxonica Limited
