package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.StreamWriterToReceiver;
import net.sf.saxon.lib.Invalidity;
import net.sf.saxon.lib.InvalidityReportGenerator;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.DateTimeValue;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;


/**
 * This class <code>InvalidityReportGenerator</code> extends the standard error handler for errors found during validation
 * of an instance document against a schema, used if user specifies -report option on validate. Its effect is to output
 * the validation errors found into the filename specified in an XML format.
 */
public class InvalidityReportGeneratorEE  extends InvalidityReportGenerator {

    private Destination destination = null;
    private int warningCount = 0;
    private int errorCount = 0;
    private String xsdversion = "1.0";
    private String systemId = null;
    private String schemaName = null;
    private Builder builder;
    private XMLStreamWriter writer;
    private boolean closed = false;

    public InvalidityReportGeneratorEE(Configuration config){
        super(config);
    }


    /**
     * Create a Report Invalidity Handler writing to a s9api Destination
     *
     * @param config      the Saxon configuration
     * @param destination required to output the validation errors
     */
    public InvalidityReportGeneratorEE(Configuration config, Destination destination) throws SaxonApiException {
        super(config);
        this.destination = destination;
        Receiver r = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
        r = new NamespaceReducer(r);
        writer = new StreamWriterToReceiver(r);
    }

    /**
     * Create a Report Invalidity Handler writing to a Receiver
     *
     * @param config   the Saxon configuration
     * @param receiver required to output the validation errors
     */
    public InvalidityReportGeneratorEE(Configuration config, Receiver receiver) throws XPathException {
        super(config);
        if (receiver instanceof Builder) {
            builder = (Builder) receiver;
        }
        Receiver r = new NamespaceReducer(receiver);
        writer = new StreamWriterToReceiver(r);
    }

    public void setReceiver(Receiver receiver){
        if (receiver instanceof Builder) {
            builder = (Builder) receiver;
        }
        Receiver r = new NamespaceReducer(receiver);
        writer = new StreamWriterToReceiver(r);
    }


    /**
     * Set the writer
     *
     * @param destination required to output the validation errors
     */
    public void setDestination(Destination destination) throws SaxonApiException {
        this.destination = destination;
        Receiver r = destination.getReceiver(
                getConfiguration().makePipelineConfiguration(),
                getConfiguration().obtainDefaultSerializationProperties());
        r = new NamespaceReducer(r);
        writer = new StreamWriterToReceiver(r);
    }

    /**
     * Set the XML document that is to be validated
     *
     * @param id of the source document
     */
    public void setSystemId(String id) {
        systemId = id;
    }

    /**
     * Set the XSD document used to validation process
     *
     * @param name of xsd file
     */
    public void setSchemaName(String name) {
        schemaName = name;
    }


    public int getErrorCount() {
        return errorCount;
    }

    public int getWarningCount() {
        return warningCount;
    }

    public void setXsdVersion(String version) {
        xsdversion = version;
    }

    public XMLStreamWriter getWriter() {
        return writer;
    }


    /**
     * Receive notification of a validity error.
     *
     * @param failure Information about the nature of the invalidity
     */
    @Override
    public void reportInvalidity(Invalidity failure) throws XPathException {
        if (closed) {
            return;
        }
        errorCount++;
        try {
            writer.writeStartElement(REPORT_NS, "error");
            if (failure.getLineNumber() != -1) {
                writer.writeAttribute("line", String.valueOf(failure.getLineNumber()));
            }
            if (failure.getColumnNumber() > -1) {
                writer.writeAttribute("column", String.valueOf(failure.getColumnNumber()));
            }
            if (failure.getPath() != null) {
                writer.writeAttribute("path", failure.getPath().toString());
            }
            if (failure.getSystemId() != null && !failure.getSystemId().equals(systemId)) {
                writer.writeAttribute("system-id", failure.getSystemId());
            }

            if(failure.getSchemaPart() != -1){
                writer.writeAttribute("xsd-part", String.valueOf(failure.getSchemaPart()));
            }
//            if (failure.getConstraintClauseNumber() != null) {
//                writer.writeAttribute("clause", failure.getConstraintClauseNumber());
//            }
            if (failure.getConstraintName() != null) {
                writer.writeAttribute("constraint", failure.getConstraintReference());
            }
            if (failure.getContextPath() != null) {
                writer.writeAttribute("context-path", failure.getContextPath().toString());
            }
            writer.writeCharacters(failure.getMessage());
            writer.writeEndElement();

            if (failure instanceof ValidationFailure) {
                List<NodeInfo> offenders = ((ValidationFailure) failure).getOffendingNodes();
                for (NodeInfo node : offenders) {
                    writer.writeStartElement(REPORT_NS, "contributing-node");
                    writer.writeAttribute("type", Type.displayTypeName(node));
                    if (node.getNodeKind() == Type.TEXT && node.getStringValue().length() < 200) {
                        writer.writeAttribute("content", node.getStringValue());
                    }
                    if (node.getLineNumber() != -1) {
                        writer.writeAttribute("line", String.valueOf(node.getLineNumber()));
                    }
                    if (node.getColumnNumber() > -1) {
                        writer.writeAttribute("column", String.valueOf(node.getColumnNumber()));
                    }
                    writer.writeAttribute("path", Navigator.getPath(node));
                    if (node.getSystemId() != null && !node.getSystemId().equals(systemId)) {
                        writer.writeAttribute("system-id", node.getSystemId());
                    }
                    writer.writeEndElement();
                }
            }

        } catch (XMLStreamException e) {
            throw new XPathException(e);
        }
    }


    public Destination getDestination() {
        return destination;
    }

    boolean started = false;

    public void startReporting(String systemId) throws XPathException {
        if (closed) {
            return;
        }
        this.systemId = systemId;
        if(systemId != null) {
            try {
                this.systemId = new URI(systemId).toString();
            } catch (URISyntaxException e) {

            }
        }
        if (!started) {
            try {
                started = true;
                writer.writeStartDocument();
                writer.setDefaultNamespace(REPORT_NS);
                writer.writeStartElement(REPORT_NS, "validation-report");

                if (this.systemId != null) {

                    writer.writeAttribute("system-id", this.systemId);
                }
            } catch (XMLStreamException e) {
                throw new XPathException(e);
            }
        }
    }

    public Sequence<?> endReporting() throws XPathException {
        if (closed) {
            return null;
        }
        createMetaData();
        try {
            if (!started) {
                throw new IllegalStateException("Reporting not started");
            }
            writer.writeEndElement();//</validation-report>
            writer.writeEndDocument();
            writer.close();
            closed = true;
        } catch (XMLStreamException e) {
            throw new XPathException(e);
        }
        if (builder != null) {
            return builder.getCurrentRoot();
        } else {
            return super.endReporting();
        }

    }


    /**
     * Create metedata element which contains summary information in the output XML document
     *
     * @throws XPathException
     */

    public void createMetaData() throws XPathException {
        try {
            writer.writeStartElement(REPORT_NS, "meta-data");
            writer.writeStartElement(REPORT_NS,"validator");
            writer.writeAttribute("name", Version.getProductName() + "-" + getConfiguration().getEditionCode());
            writer.writeAttribute("version", Version.getProductVersion());
            writer.writeEndElement(); //</validator>
            writer.writeStartElement(REPORT_NS,"results");
            writer.writeAttribute("errors", "" + errorCount);
            writer.writeAttribute("warnings", "" + warningCount);
            writer.writeEndElement(); //</results>
            writer.writeStartElement(REPORT_NS,"schema");
            if (schemaName != null) {
                writer.writeAttribute("file", schemaName);
            }
            writer.writeAttribute("xsd-version", xsdversion);
            writer.writeEndElement(); //</schema>
            writer.writeStartElement(REPORT_NS,"run");
            writer.writeAttribute("at", DateTimeValue.getCurrentDateTime(null).getStringValue());
            writer.writeEndElement(); //</run>
            writer.writeEndElement(); //</meta-data>
        } catch (XMLStreamException ex) {
            throw new XPathException(ex);
        }
    }

}

// Copyright (c) 2018 Saxonica Limited.