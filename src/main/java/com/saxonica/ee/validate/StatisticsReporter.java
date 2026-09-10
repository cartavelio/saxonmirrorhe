package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.StreamWriterToReceiver;
import net.sf.saxon.lib.ValidationStatisticsRecipient;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaType;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import java.util.Map;

/**
 * Generates statistics relating to a validation episode
 */
public class StatisticsReporter implements ValidationStatisticsRecipient {

    private XMLStreamWriter writer;

    public StatisticsReporter() {

    }

    /**
     * Create a StatisticsReporter and set a Receiver to receive an XML report of the statistics that have been
     * collected
     *
     * @param receiver the receiver to which the XML report will be sent
     */
    public StatisticsReporter(Receiver receiver) {
        setReceiver(receiver);
    }

    /**
     * Create a StatisticsReporter and set a Receiver to receive an XML report of the statistics that have been
     * collected
     *
     * @param receiver the receiver to which the XML report will be sent
     */

    public void setReceiver(Receiver receiver) {
        this.writer = new StreamWriterToReceiver(receiver);
    }


    /**
     * Notify the validation statistics
     *
     * @param statistics the statistics, in the form of a map from schema components (currently,
     *                   element declarations and schema types) to a count of how often the component
     *                   was used during the validation episode
     */
    public void notifyValidationStatistics(Map<SchemaComponent, Integer> statistics) throws XPathException {
        if (writer == null) {
            throw new IllegalStateException("No writer has been set");
        }
        try {
            //System.err.println("REPORTING STATISTICS");
            writer.writeStartDocument();
            writer.writeStartElement("schemaCoverage");
            for (Map.Entry<SchemaComponent, Integer> entry : statistics.entrySet()) {
                SchemaComponent component = entry.getKey();
                int count = entry.getValue();
                String kind = null;
                String namespace = null;
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
                writer.writeEmptyElement("component");
                writer.writeAttribute("kind", kind);
                writer.writeAttribute("namespace", namespace);
                writer.writeAttribute("name", localname);
                writer.writeAttribute("count", count + "");
            }
            writer.writeEndElement();
            writer.writeEndDocument();
            writer.close();
        } catch (XMLStreamException e) {
            throw new XPathException("Failure to write to statistics report file", e);
        }

    }
}

// Copyright (c) 2011 Saxonica Limited.



