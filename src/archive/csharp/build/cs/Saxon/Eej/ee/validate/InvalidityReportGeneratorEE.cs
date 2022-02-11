using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class InvalidityReportGeneratorEE : Saxon.Hej.lib.InvalidityReportGenerator {
        private Saxon.Hej.s9api.Destination destination = null;
        private readonly int warningCount = 0;
        private int errorCount = 0;
        private string xsdversion = "1.0";
        private string systemId = null;
        private string schemaName = null;
        private Saxon.Hej.@event.Builder builder;
        private bool closed = false;
        private Saxon.Hej.s9api.push.Document doc;
        private Saxon.Hej.s9api.push.Element report;
        public InvalidityReportGeneratorEE(Saxon.Hej.Configuration config) : base (config) {
        }
        public InvalidityReportGeneratorEE(Saxon.Hej.Configuration config, Saxon.Hej.s9api.Destination destination) : base (config) {
            this.destination = destination;
            Saxon.Hej.@event.Receiver r = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
            doc = new Saxon.Hej.@event.PushToReceiver(r).document(true);
        }
        public InvalidityReportGeneratorEE(Saxon.Hej.Configuration config, Saxon.Hej.@event.Receiver receiver) : base (config) {
            if ((receiver is Saxon.Hej.@event.Builder)) {
                builder = (Saxon.Hej.@event.Builder)(receiver);
            }
            Saxon.Hej.@event.Receiver r = new Saxon.Hej.@event.NamespaceReducer(receiver);
            try {
                doc = new Saxon.Hej.@event.PushToReceiver(r).document(true);
            } catch (Saxon.Api.SaxonApiException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        public void setReceiver(Saxon.Hej.@event.Receiver receiver) {
            if ((receiver is Saxon.Hej.@event.Builder)) {
                builder = (Saxon.Hej.@event.Builder)(receiver);
            }
            Saxon.Hej.@event.Receiver r = new Saxon.Hej.@event.NamespaceReducer(receiver);
            try {
                doc = new Saxon.Hej.@event.PushToReceiver(r).document(true);
            } catch (Saxon.Api.SaxonApiException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(Saxon.Hej.trans.XPathException.makeXPathException(e));
            }
        }
        public void setDestination(Saxon.Hej.s9api.Destination destination) {
            this.destination = destination;
            Saxon.Hej.@event.Receiver r = destination.getReceiver(getConfiguration().makePipelineConfiguration(), getConfiguration().obtainDefaultSerializationProperties());
            doc = new Saxon.Hej.@event.PushToReceiver(r).document(true);
        }
        public override void setSystemId(string id) {
            systemId = id;
        }
        public override void setSchemaName(string name) {
            schemaName = name;
        }
        public override int getErrorCount() {
            return errorCount;
        }
        public override int getWarningCount() {
            return warningCount;
        }
        public override void setXsdVersion(string version) {
            xsdversion = version;
        }
        public override void reportInvalidity(Saxon.Hej.lib.Invalidity failure) {
            if (closed) {
                return ;
            }
            errorCount++;
            try {
                Saxon.Hej.s9api.push.Element error = ((/*1430*/Saxon.Hej.s9api.push.Element)report.element("error").attribute("line", ifPositive(failure.getLineNumber())).attribute("column", ifPositive(failure.getColumnNumber())).attribute("path", failure.getPath().ToString()).attribute("system-id", (((failure.getSystemId() != null) && !(failure.getSystemId().Equals(systemId))) ? failure.getSystemId() : null)).attribute("xsd-part", ifPositive(failure.getSchemaPart())).attribute("constraint", failure.getConstraintReference()).attribute("context-path", ((failure.getContextPath() == null) ? null : failure.getContextPath().ToString())).text(failure.getMessage()));
                if ((failure is Saxon.Hej.type.ValidationFailure)) {
                    System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> offenders = ((Saxon.Hej.type.ValidationFailure)(failure)).getOffendingNodes();
                    foreach (Saxon.Hej.om.NodeInfo node in offenders) {
                        error.element("contributing-node").attribute("type", Saxon.Hej.type.Type.displayTypeName(node)).attribute("content", (((node.getNodeKind() == Saxon.Hej.type.Type.TEXT) && (node.getUnicodeStringValue().estimatedLength()<200)) ? node.getStringValue() : null)).attribute("line", ifPositive(node.getLineNumber())).attribute("column", ifPositive(node.getColumnNumber())).attribute("path", Saxon.Hej.tree.util.Navigator.getPath(node)).attribute("system-id", (((node.getSystemId() != null) && !(node.getSystemId().Equals(systemId))) ? node.getSystemId() : null)).close();
                    }
                }
                error.close();
            } catch (Saxon.Api.SaxonApiException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        private string ifPositive(int value) {
            return ((value<0) ? null : (""+value));
        }
        public Saxon.Hej.s9api.Destination getDestination() {
            return destination;
        }
        internal bool started = false;
        public override void startReporting(string systemId) {
            if (closed) {
                return ;
            }
            this.systemId = systemId;
            if ((systemId != null)) {
                try {
                    this.systemId = new System.Uri(systemId, System.UriKind.RelativeOrAbsolute).ToString();
                } catch (System.UriFormatException e) {
                }
            }
            if (!(started)) {
                try {
                    started = true;
                    doc.setDefaultNamespace(REPORT_NS);
                    report = doc.element("validation-report");
                    if ((this.systemId != null)) {
                        report.attribute("system-id", this.systemId);
                    }
                } catch (Saxon.Api.SaxonApiException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        }
        public override Saxon.Hej.om.Sequence endReporting() {
            if (closed) {
                return null;
            }
            createMetaData();
            try {
                if (!(started)) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Reporting not started");
                }
                report.close();
                doc.close();
                closed = true;
            } catch (Saxon.Api.SaxonApiException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
            if ((builder != null)) {
                return builder.getCurrentRoot();
            } else {
                return base.endReporting();
            }
        }
        public override void createMetaData() {
            try {
                Saxon.Hej.s9api.push.Element metadata = report.element("meta-data");
                metadata.element("validator").attribute("name", ((Saxon.Hej.Version.getProductName()+"-")+getConfiguration().getEditionCode())).attribute("version", Saxon.Hej.Version.getProductVersion());
                metadata.element("results").attribute("errors", (""+errorCount)).attribute("warnings", (""+warningCount));
                metadata.element("schema").attribute("file", schemaName).attribute("xsd-version", xsdversion);
                metadata.element("run").attribute("at", Saxon.Hej.value.DateTimeValue.getCurrentDateTime(null).getStringValue());
                metadata.close();
            } catch (Saxon.Api.SaxonApiException ex) {
                throw new Saxon.Hej.trans.XPathException(ex);
            }
        }
    
    }
}