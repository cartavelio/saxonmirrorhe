package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import net.sf.saxon.lib.ValidationStatisticsRecipient;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.sapling.SaplingDocument;
import net.sf.saxon.sapling.SaplingElement;
import net.sf.saxon.sapling.Saplings;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaType;

import java.util.Map;


/**
 * Generates statistics relating to a validation episode
 */
public class StatisticsReporter implements ValidationStatisticsRecipient {

    private Processor processor;
    private Destination destination;

    public StatisticsReporter() {

    }

    /**
     * Create a StatisticsReporter and set a Receiver to receive an XML report of the statistics that have been
     * collected
     *
     * @param processor the processor owning the destination to which the XML report will be sent
     * @param destination the destination to which the XML report will be sent
     */

    public void setReceiver(Processor processor, Destination destination) {
        this.processor = processor;
        this.destination = destination;
    }


    /**
     * Notify the validation statistics
     *
     * @param statistics the statistics, in the form of a map from schema components (currently,
     *                   element declarations and schema types) to a count of how often the component
     *                   was used during the validation episode
     */
    @Override
    public void notifyValidationStatistics(Map<SchemaComponent, Integer> statistics) throws SaxonApiException {
        if (processor == null) {
            throw new IllegalStateException("No destination has been set");
        }
        SaplingElement coverage = Saplings.elem("schemaCoverage");
        for (Map.Entry<SchemaComponent, Integer> entry : statistics.entrySet()) {
            SchemaComponent component = entry.getKey();
            int count = entry.getValue();
            String kind = null;
            NamespaceUri namespace = NamespaceUri.NULL;
            String localname = null;
            if (component instanceof SchemaType) {
                kind = (((SchemaType) component).isComplexType() ? "complexType" : "simpleType");
                namespace = ((SchemaType) component).getTargetNamespace();
                localname = ((SchemaType) component).getName();

            } else if (component instanceof ElementDecl) {
                kind = "element";
                namespace = ((ElementDecl) component).getTargetNamespace();
                localname = ((ElementDecl) component).getName();
            }
            coverage = coverage.withChild(Saplings.elem("component")
                                              .withAttr("kind", kind)
                                              .withAttr("namespace", namespace.toString())
                                              .withAttr("name", localname)
                                              .withAttr("count", count + ""));

        }
        SaplingDocument report = Saplings.doc().withChild(coverage);
        report.send(processor, destination);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited



