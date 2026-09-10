package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.PushToReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.Invalidity;
import net.sf.saxon.lib.InvalidityReportGenerator;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Destination;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.push.Document;
import net.sf.saxon.s9api.push.Element;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.DateTimeValue;

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
    private final int warningCount = 0;
    private int errorCount = 0;
    private String xsdversion = "1.0";
    private String systemId = null;
    private String schemaName = null;
    private Builder builder;
    //private XMLStreamWriter writer;
    private boolean closed = false;

    private Document doc;
    private Element report;

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
//        r = new NamespaceReducer(r);
//        writer = new StreamWriterToReceiver(r);
        doc = new PushToReceiver(r).document(true);
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
        try {
            doc = new PushToReceiver(r).document(true);
        } catch (SaxonApiException e) {
            throw XPathException.makeXPathException(e);
        }
        //writer = new StreamWriterToReceiver(r);
    }

    public void setReceiver(Receiver receiver){
        if (receiver instanceof Builder) {
            builder = (Builder) receiver;
        }
        Receiver r = new NamespaceReducer(receiver);
        try {
            doc = new PushToReceiver(r).document(true);
        } catch (SaxonApiException e) {
            throw new UncheckedXPathException(XPathException.makeXPathException(e));
        }
        //writer = new StreamWriterToReceiver(r);
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
        doc = new PushToReceiver(r).document(true);
//        r = new NamespaceReducer(r);
//        writer = new StreamWriterToReceiver(r);
    }

    /**
     * Set the XML document that is to be validated
     *
     * @param id of the source document
     */
    @Override
    public void setSystemId(String id) {
        systemId = id;
    }

    /**
     * Set the XSD document used to validation process
     *
     * @param name of xsd file
     */
    @Override
    public void setSchemaName(String name) {
        schemaName = name;
    }


    @Override
    public int getErrorCount() {
        return errorCount;
    }

    @Override
    public int getWarningCount() {
        return warningCount;
    }

    @Override
    public void setXsdVersion(String version) {
        xsdversion = version;
    }

//    public XMLStreamWriter getWriter() {
//        return writer;
//    }


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
            Element error = report.element("error")
                    .attribute("line", ifPositive(failure.getLineNumber()))
                    .attribute("column", ifPositive(failure.getColumnNumber()))
                    .attribute("path", failure.getPath().toString())
                    .attribute("system-id",
                               failure.getSystemId() != null && !failure.getSystemId().equals(systemId) ? failure.getSystemId() : null)
                    .attribute("xsd-part", ifPositive(failure.getSchemaPart()))
                    .attribute("constraint", failure.getConstraintReference())
                    .attribute("context-path", failure.getContextPath() == null ? null : failure.getContextPath().toString())
                    .text(failure.getMessage());

            if (failure instanceof ValidationFailure) {
                List<NodeInfo> offenders = ((ValidationFailure) failure).getOffendingNodes();
                for (NodeInfo node : offenders) {
                    error.element("contributing-node")
                            .attribute("type",
                                       Type.displayTypeName(node))
                            .attribute("content",
                                       node.getNodeKind() == Type.TEXT && node.getUnicodeStringValue().estimatedLength() < 200 ? node.getStringValue() : null)
                            .attribute("line",
                                       ifPositive(node.getLineNumber()))
                            .attribute("column",
                                       ifPositive(node.getColumnNumber()))
                            .attribute("path",
                                       Navigator.getPath(node))
                            .attribute("system-id",
                                       node.getSystemId() != null && !node.getSystemId().equals(systemId) ? node.getSystemId() : null)
                            .close();
                }
            }
            error.close();

        } catch (SaxonApiException e) {
            throw new XPathException(e);
        }
    }

    private String ifPositive(int value) {
        return value < 0 ? null : "" + value;
    }

    public Destination getDestination() {
        return destination;
    }

    boolean started = false;

    @Override
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
//                writer.writeStartDocument();
//                writer.setDefaultNamespace(REPORT_NS);
//                writer.writeStartElement(REPORT_NS, "validation-report");
                doc.setDefaultNamespace(REPORT_NS.toString());
                report = doc.element("validation-report");

                if (this.systemId != null) {

                    report.attribute("system-id", this.systemId);
                }
            } catch (SaxonApiException e) {
                throw new XPathException(e);
            }
        }
    }

    @Override
    public Sequence endReporting() throws XPathException {
        if (closed) {
            return null;
        }
        createMetaData();
        try {
            if (!started) {
                throw new IllegalStateException("Reporting not started");
            }
//            writer.writeEndElement();//</validation-report>
//            writer.writeEndDocument();
            report.close();
            doc.close();

            closed = true;
        } catch (SaxonApiException e) {
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
     * @throws XPathException if creating the metadata element fails
     */

    @Override
    public void createMetaData() throws XPathException {
        try {
            Element metadata = report.element("meta-data");
            metadata.element("validator")
                    .attribute("name", Version.getProductName() + "-" + getConfiguration().getEditionCode())
                    .attribute("version", Version.getProductVersion());
            metadata.element("results")
                    .attribute("errors", "" + errorCount)
                    .attribute("warnings", "" + warningCount);
            metadata.element("schema")
                    .attribute("file", schemaName)
                    .attribute("xsd-version", xsdversion);
            metadata.element("run")
                    .attribute("at", DateTimeValue.getCurrentDateTime(null).getStringValue());
            metadata.close();
        } catch (SaxonApiException ex) {
            throw new XPathException(ex);
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited